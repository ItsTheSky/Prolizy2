using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Prolizy2.API;
using Prolizy2.API.Model.Request;
using Prolizy2.Model;

namespace Prolizy2.Services;

public class ScheduleCacheService
{
    private readonly Dictionary<string, List<ScheduleItem>> _weeklyCache = new();
    private readonly HashSet<string> _loadingWeeks = new();

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
        return monthWeeks.Any(week => _loadingWeeks.Contains(GetWeekKey(week.startDate)));
    }

    public bool IsWeekLoading(DateOnly date)
    {
        var weekKey = GetWeekKey(date);
        return _loadingWeeks.Contains(weekKey);
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

    public async Task<List<ScheduleItem>> LoadMonthIfNeededAsync(DateOnly date, string federationId)
    {
        if (IsWeekCached(date))
            return GetDaySchedule(date);

        if (IsWeekLoading(date))
        {
            var weekKey = GetWeekKey(date);
            while (_loadingWeeks.Contains(weekKey))
            {
                await Task.Delay(50);
            }
            return GetDaySchedule(date);
        }

        // Load the week containing this date
        await LoadWeekAsync(date, federationId);

        // Load all weeks for the month
        await LoadMonthAsync(date, federationId);

        return GetDaySchedule(date);
    }

    public async Task LoadMonthAsync(DateOnly date, string federationId)
    {
        var monthWeeks = GetMonthWeeks(date);
        var loadTasks = new List<Task>();

        foreach (var (startDate, endDate) in monthWeeks)
        {
            var weekKey = GetWeekKey(startDate);
            if (!_weeklyCache.ContainsKey(weekKey) && !_loadingWeeks.Contains(weekKey))
            {
                loadTasks.Add(LoadWeekAsync(startDate, federationId));
            }
        }

        if (loadTasks.Any())
        {
            await Task.WhenAll(loadTasks);
        }
    }

    public async Task LoadWeekAsync(DateOnly date, string federationId)
    {
        var weekKey = GetWeekKey(date);

        if (_loadingWeeks.Contains(weekKey) || _weeklyCache.ContainsKey(weekKey))
            return;

        _loadingWeeks.Add(weekKey);

        try
        {
            var (startOfWeek, endOfWeek) = GetWeekBounds(date);

            var courses = await EDTClient.GetCourses(new CalendarRequest
            {
                FederationId = federationId,
                StartDate = startOfWeek,
                EndDate = endOfWeek,
                ViewType = CalendarRequest.CalendarViewType.AgendaWeek
            });

            var scheduleItems = new List<ScheduleItem>();

            foreach (var course in courses)
            {
                var description = await course.GetDescription(true);
                var scheduleItem = await ScheduleItemFactory.CreateFromCourse(course, description);
                scheduleItems.Add(scheduleItem);
            }

            _weeklyCache[weekKey] = scheduleItems;
        }
        finally
        {
            _loadingWeeks.Remove(weekKey);
        }
    }

    public async Task PreloadAdjacentMonthsAsync(DateOnly currentDate, string federationId)
    {
        var previousMonth = currentDate.AddMonths(-1);
        var nextMonth = currentDate.AddMonths(1);

        var tasks = new List<Task>();

        if (!IsMonthCached(previousMonth) && !IsMonthLoading(previousMonth))
        {
            tasks.Add(LoadMonthAsync(previousMonth, federationId));
        }

        if (!IsMonthCached(nextMonth) && !IsMonthLoading(nextMonth))
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