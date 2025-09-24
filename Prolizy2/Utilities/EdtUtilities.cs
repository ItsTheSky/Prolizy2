using System;
using System.Collections.Generic;
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
    public static ScheduleItem? FindNextOrCurrentItem(List<ScheduleItem> items)
    {
        if (items == null || items.Count == 0)
            return null;

        var now = DateTime.Now;
        var currentDate = DateOnly.FromDateTime(now);

        // First, try to find an item that is currently ongoing
        foreach (var item in items)
        {
            if (item.StartTime <= now && item.EndTime >= now &&
                DateOnly.FromDateTime(item.StartTime.Date) == currentDate)
            {
                return item;
            }
        }

        // If no ongoing item found, find the next item after the current time
        ScheduleItem? nextItem = null;
        foreach (var item in items)
        {
            if (DateOnly.FromDateTime(item.StartTime.Date) == currentDate && item.StartTime > now)
            {
                if (nextItem == null || item.StartTime < nextItem.StartTime)
                {
                    nextItem = item;
                }
            }
        }

        return nextItem;
    }
    
}