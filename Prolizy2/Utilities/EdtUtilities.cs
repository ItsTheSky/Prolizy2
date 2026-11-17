using System;
using System.Collections.Generic;
using System.Linq;
using Prolizy2.Model;

namespace Prolizy2.Utilities;

public static class EdtUtilities
{
    /// <summary>
    /// Finds the next or current schedule item (= course) for the given list of items.
    /// This method will check each item by their date to match,
    /// then check for the current time and see if there's an item that is currently ongoing.
    /// If not, it'll try to find the next item that is scheduled after the current time.
    /// </summary>
    /// <param name="items">The list of schedule items to check.</param>
    /// <returns>The next or current schedule item, or null if none found.</returns>
    public static ScheduleItem? FindNextOrCurrentItem(List<ScheduleItem> items) =>
        FindNextOrCurrentItem(items, DateTime.Now);

    /// <summary>
    /// Same as <see cref="FindNextOrCurrentItem(List{ScheduleItem})"/> but with an
    /// injectable "now", so the home cards can compute several upcoming courses.
    /// </summary>
    public static ScheduleItem? FindNextOrCurrentItem(List<ScheduleItem> items, DateTime now)
    {
        if (items == null || items.Count == 0)
            return null;
        
        // first we sort them
        items.Sort((a, b) =>
        {
            var dateComparison = a.Date.CompareTo(b.Date);
            return dateComparison != 0 ? dateComparison : a.StartTimeOnly.CompareTo(b.StartTimeOnly);
        });

        var currentDate = DateOnly.FromDateTime(now);
        var currentTime = TimeOnly.FromDateTime(now);
        
        // Filter items for the current date
        var todayItems = items.FindAll(item => item.Date == currentDate);

        // Check for an ongoing item
        foreach (var item in todayItems)
        {
            if (item.StartTimeOnly <= currentTime && item.EndTimeOnly >= currentTime)
                return item;
        }

        foreach (var item in todayItems)
        {
            if (item.StartTimeOnly > currentTime)
                return item;
        }

        foreach (var item in items)
        {
            if (item.Date > currentDate)
                return item;
        }

        return null;
    }

    /// <summary>
    /// The courses that follow <paramref name="item"/> in <paramref name="items"/>,
    /// sorted by start time. Used to show the rest of the day on the home card.
    /// </summary>
    public static List<ScheduleItem> FindFollowingItems(List<ScheduleItem> items, ScheduleItem? item, int count)
    {
        if (items is null || count <= 0)
            return new List<ScheduleItem>();

        var ordered = items
            .OrderBy(i => DateOnly.FromDateTime(i.StartTime))
            .ThenBy(i => i.StartTime)
            .ToList();

        if (item is null)
            return ordered.Take(count).ToList();

        var index = ordered.IndexOf(item);
        if (index < 0)
            return ordered.Take(count).ToList();

        return ordered.Skip(index + 1).Take(count).ToList();
    }
}
