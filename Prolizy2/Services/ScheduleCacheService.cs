using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Prolizy2.API;
using Prolizy2.API.Model.Course;
using Prolizy2.API.Model.Request;
using Prolizy2.API.Utils;
using Prolizy2.Model;

namespace Prolizy2.Services;

public class ScheduleCacheService
{
    // All state below is only touched from the UI thread: every public method is
    // called from it and resumes on it; only fetching and parsing run elsewhere.
    private readonly Dictionary<string, List<ScheduleItem>> _weeklyCache = new();
    private readonly Dictionary<string, Task> _loadingWeeks = new();
    private readonly HashSet<string> _revalidatedWeeks = new();

    private static bool _pruned;

    /// <summary>
    /// Raised (on the UI thread) when a week that was first served from the disk
    /// cache turned out to have changed on the server. The argument is the
    /// Monday of that week.
    /// </summary>
    public event Action<DateOnly>? WeekUpdated;

    public ScheduleCacheService()
    {
        if (_pruned)
            return;
        _pruned = true;
        Task.Run(ScheduleDiskCache.Prune);
    }

    public bool IsWeekCached(DateOnly date)
    {
        var weekKey = GetWeekKey(date);
        return _weeklyCache.ContainsKey(weekKey);
    }

    public bool IsMonthCached(DateOnly date)
    {
        var monthWeeks = GetMonthWeeks(date);
        return monthWeeks.All(week => _weeklyCache.ContainsKey(GetWeekKey(week.startDate)));
    }

    public bool IsMonthLoading(DateOnly date)
    {
        var monthWeeks = GetMonthWeeks(date);
        return monthWeeks.Any(week => _loadingWeeks.ContainsKey(GetWeekKey(week.startDate)));
    }

    public bool IsWeekLoading(DateOnly date)
    {
        var weekKey = GetWeekKey(date);
        return _loadingWeeks.ContainsKey(weekKey);
    }

    public List<ScheduleItem> GetDaySchedule(DateOnly date)
    {
        var weekKey = GetWeekKey(date);
        if (!_weeklyCache.TryGetValue(weekKey, out var weekSchedule))
            return new List<ScheduleItem>();

        return weekSchedule
            .Where(item => DateOnly.FromDateTime(item.StartTime) == date)
            .OrderBy(item => item.StartTime)
            .ToList();
    }

    /// <summary>
    /// Returns the schedule of <paramref name="date"/> as soon as its week is
    /// available. The rest of the month is fetched in the background so the
    /// caller is never kept waiting for data it does not display.
    /// </summary>
    public async Task<List<ScheduleItem>> LoadMonthIfNeededAsync(DateOnly date, string federationId)
    {
        if (!IsWeekCached(date))
            await LoadWeekAsync(date, federationId);

        _ = LoadMonthInBackgroundAsync(date, federationId);

        return GetDaySchedule(date);
    }

    private async Task LoadMonthInBackgroundAsync(DateOnly date, string federationId)
    {
        try
        {
            await LoadMonthAsync(date, federationId);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error preloading month: {ex.Message}");
        }
    }

    public async Task LoadMonthAsync(DateOnly date, string federationId)
    {
        var monthWeeks = GetMonthWeeks(date);
        var loadTasks = new List<Task>();

        foreach (var (startDate, endDate) in monthWeeks)
        {
            var weekKey = GetWeekKey(startDate);
            if (!_weeklyCache.ContainsKey(weekKey))
            {
                loadTasks.Add(LoadWeekAsync(startDate, federationId));
            }
        }

        if (loadTasks.Any())
        {
            await Task.WhenAll(loadTasks);
        }
    }

    /// <summary>
    /// Makes the week containing <paramref name="date"/> available. Concurrent
    /// callers share the same in-flight load instead of polling for it.
    /// </summary>
    public async Task LoadWeekAsync(DateOnly date, string federationId)
    {
        var weekKey = GetWeekKey(date);

        if (_weeklyCache.ContainsKey(weekKey))
            return;

        if (!_loadingWeeks.TryGetValue(weekKey, out var pending))
        {
            pending = LoadWeekCoreAsync(weekKey, date, federationId);
            _loadingWeeks[weekKey] = pending;
        }

        try
        {
            await pending;
        }
        finally
        {
            if (_loadingWeeks.TryGetValue(weekKey, out var current) && ReferenceEquals(current, pending))
                _loadingWeeks.Remove(weekKey);
        }
    }

    private async Task LoadWeekCoreAsync(string weekKey, DateOnly date, string federationId)
    {
        var (startOfWeek, endOfWeek) = GetWeekBounds(date);

        // 1. Disk: show what we knew last time, then check the server quietly.
        var cached = await Task.Run(() =>
        {
            var json = ScheduleDiskCache.TryRead(federationId, startOfWeek);
            return json is null ? null : TryParse(json);
        });

        if (cached is not null)
        {
            _weeklyCache[weekKey] = BuildItems(cached.Courses);
            _ = RevalidateWeekAsync(weekKey, startOfWeek, endOfWeek, federationId);
            return;
        }

        // 2. Network.
        var fresh = await FetchWeekAsync(startOfWeek, endOfWeek, federationId);
        _weeklyCache[weekKey] = BuildItems(fresh.Courses);
        _revalidatedWeeks.Add(weekKey);
        _ = Task.Run(() => ScheduleDiskCache.Write(federationId, startOfWeek, fresh.Json));
    }

    /// <summary>
    /// Re-fetches a week that was served from disk. Nothing is touched when the
    /// server still returns the same courses, so the UI is not rebuilt for nothing.
    /// </summary>
    private async Task RevalidateWeekAsync(string weekKey, DateOnly startOfWeek, DateOnly endOfWeek,
        string federationId)
    {
        if (!_revalidatedWeeks.Add(weekKey))
            return;

        try
        {
            var fresh = await FetchWeekAsync(startOfWeek, endOfWeek, federationId);
            var freshItems = BuildItems(fresh.Courses);

            var changed = !_weeklyCache.TryGetValue(weekKey, out var current)
                          || !HaveSameContent(current, freshItems);

            _ = Task.Run(() => ScheduleDiskCache.Write(federationId, startOfWeek, fresh.Json));

            if (!changed)
                return;

            _weeklyCache[weekKey] = freshItems;
            WeekUpdated?.Invoke(startOfWeek);
        }
        catch (Exception ex)
        {
            // Offline or server error: keep serving the cached week, retry next time.
            _revalidatedWeeks.Remove(weekKey);
            Console.WriteLine($"Could not refresh week {weekKey}: {ex.Message}");
        }
    }

    private sealed record WeekPayload(string Json, List<(Course Course, CourseDescription Description)> Courses);

    /// <summary>Downloads and parses a week entirely off the UI thread.</summary>
    private static Task<WeekPayload> FetchWeekAsync(DateOnly startOfWeek, DateOnly endOfWeek, string federationId)
    {
        return Task.Run(async () =>
        {
            var json = await EDTClient.GetCoursesJson(new CalendarRequest
            {
                FederationId = federationId,
                StartDate = startOfWeek,
                EndDate = endOfWeek,
                ViewType = CalendarRequest.CalendarViewType.AgendaWeek
            }).ConfigureAwait(false);

            return Parse(json);
        });
    }

    private static WeekPayload Parse(string json)
    {
        var courses = EDTClient.ParseCourses(json);
        var parsed = new List<(Course, CourseDescription)>(courses.Count);
        foreach (var course in courses)
            parsed.Add((course, CourseDescriptionParser.ParseDescription(course.RawDescription)));

        return new WeekPayload(json, parsed);
    }

    private static WeekPayload? TryParse(string json)
    {
        try
        {
            return Parse(json);
        }
        catch
        {
            // Corrupted or outdated cache entry: fall back to the network.
            return null;
        }
    }

    /// <summary>Builds the UI models (they hold brushes, so this stays on the UI thread).</summary>
    private static List<ScheduleItem> BuildItems(List<(Course Course, CourseDescription Description)> courses)
    {
        var items = new List<ScheduleItem>(courses.Count);
        foreach (var (course, description) in courses)
            items.Add(ScheduleItemFactory.Create(course, description));
        return items;
    }

    private static bool HaveSameContent(List<ScheduleItem> left, List<ScheduleItem> right)
    {
        if (left.Count != right.Count)
            return false;

        for (var i = 0; i < left.Count; i++)
        {
            var a = left[i];
            var b = right[i];
            if (a.StartTime != b.StartTime || a.EndTime != b.EndTime
                || a.Subject != b.Subject || a.Room != b.Room
                || a.Professor != b.Professor || a.Group != b.Group
                || a.Type != b.Type
                || a.Course?.Id != b.Course?.Id
                || a.Course?.BackgroundColor != b.Course?.BackgroundColor)
                return false;
        }

        return true;
    }

    public async Task PreloadAdjacentMonthsAsync(DateOnly currentDate, string federationId)
    {
        var previousMonth = currentDate.AddMonths(-1);
        var nextMonth = currentDate.AddMonths(1);

        var tasks = new List<Task>();

        // Weeks already being loaded (e.g. the one shared with the current
        // month) are simply awaited by LoadWeekAsync, so a month must not be
        // skipped just because one of its weeks is in flight.
        if (!IsMonthCached(previousMonth))
        {
            tasks.Add(LoadMonthAsync(previousMonth, federationId));
        }

        if (!IsMonthCached(nextMonth))
        {
            tasks.Add(LoadMonthAsync(nextMonth, federationId));
        }

        if (tasks.Any())
        {
            await Task.WhenAll(tasks);
        }
    }

    public List<ScheduleItem> GetCachedItems()
    {
        return _weeklyCache.Values.SelectMany(list => list).ToList();
    }

    public void ClearCache()
    {
        _weeklyCache.Clear();
        _loadingWeeks.Clear();
    }

    public void ClearOldCache(DateOnly currentDate, int monthsToKeep = 3)
    {
        var cutoffDate = currentDate.AddMonths(-monthsToKeep);
        var keysToRemove = _weeklyCache.Keys
            .Where(key => ParseWeekKey(key) < cutoffDate)
            .ToList();

        foreach (var key in keysToRemove)
        {
            _weeklyCache.Remove(key);
        }
    }

    private static string GetWeekKey(DateOnly date)
    {
        var (startOfWeek, _) = GetWeekBounds(date);
        return $"{startOfWeek.Year}-W{GetWeekOfYear(startOfWeek):D2}";
    }

    private static DateOnly ParseWeekKey(string weekKey)
    {
        var parts = weekKey.Split('-');
        var year = int.Parse(parts[0]);
        var weekNumber = int.Parse(parts[1][1..]);

        var jan1 = new DateOnly(year, 1, 1);
        var daysOffset = DayOfWeek.Monday - jan1.DayOfWeek;
        var firstMonday = jan1.AddDays(daysOffset);

        return firstMonday.AddDays((weekNumber - 1) * 7);
    }

    private static (DateOnly startOfWeek, DateOnly endOfWeek) GetWeekBounds(DateOnly date)
    {
        var daysFromMonday = ((int)date.DayOfWeek + 6) % 7;
        var startOfWeek = date.AddDays(-daysFromMonday);
        var endOfWeek = startOfWeek.AddDays(6);

        return (startOfWeek, endOfWeek);
    }

    private static List<(DateOnly startDate, DateOnly endDate)> GetMonthWeeks(DateOnly date)
    {
        var weeks = new List<(DateOnly startDate, DateOnly endDate)>();
        var firstDayOfMonth = new DateOnly(date.Year, date.Month, 1);
        var lastDayOfMonth = firstDayOfMonth.AddMonths(1).AddDays(-1);

        var (firstWeekStart, _) = GetWeekBounds(firstDayOfMonth);
        var (_, lastWeekEnd) = GetWeekBounds(lastDayOfMonth);

        var currentWeekStart = firstWeekStart;
        while (currentWeekStart <= lastWeekEnd)
        {
            var weekEnd = currentWeekStart.AddDays(6);
            weeks.Add((currentWeekStart, weekEnd));
            currentWeekStart = currentWeekStart.AddDays(7);
        }

        return weeks;
    }

    private static int GetWeekOfYear(DateOnly date)
    {
        var culture = CultureInfo.CurrentCulture;
        var calendar = culture.Calendar;
        return calendar.GetWeekOfYear(date.ToDateTime(TimeOnly.MinValue),
            culture.DateTimeFormat.CalendarWeekRule,
            culture.DateTimeFormat.FirstDayOfWeek);
    }
}