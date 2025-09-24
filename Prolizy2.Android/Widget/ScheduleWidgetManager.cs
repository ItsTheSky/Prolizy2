using System;
using System.Linq;
using Prolizy2.Model;

namespace Prolizy2.Android.Widget;

/// <summary>
/// Utility class to manage and update the Prolizy Schedule Widget
/// </summary>
public static class ScheduleWidgetManager
{
    /// <summary>
    /// Updates the widget with the next upcoming schedule item
    /// </summary>
    /// <param name="scheduleItems">List of schedule items</param>
    public static void UpdateWithNextCourse(ScheduleItem[]? scheduleItems)
    {
        if (scheduleItems == null || !scheduleItems.Any())
        {
            ProlizyScheduleWidgetProvider.UpdateWidget(null);
            return;
        }

        var now = DateTime.Now;
        
        // Find the next upcoming course
        var nextCourse = scheduleItems
            .Where(item => item.StartTime > now)
            .OrderBy(item => item.StartTime)
            .FirstOrDefault();

        // If no upcoming course today, get the first course of tomorrow
        if (nextCourse == null)
        {
            var tomorrow = now.Date.AddDays(1);
            nextCourse = scheduleItems
                .Where(item => item.StartTime.Date >= tomorrow)
                .OrderBy(item => item.StartTime)
                .FirstOrDefault();
        }

        ProlizyScheduleWidgetProvider.UpdateWidget(nextCourse);
    }

    /// <summary>
    /// Updates the widget with a specific schedule item
    /// </summary>
    /// <param name="scheduleItem">The schedule item to display</param>
    public static void UpdateWithSpecificCourse(ScheduleItem? scheduleItem)
    {
        ProlizyScheduleWidgetProvider.UpdateWidget(scheduleItem);
    }

    /// <summary>
    /// Updates the widget with the current ongoing course
    /// </summary>
    /// <param name="scheduleItems">List of schedule items</param>
    public static void UpdateWithCurrentCourse(ScheduleItem[]? scheduleItems)
    {
        if (scheduleItems == null || !scheduleItems.Any())
        {
            ProlizyScheduleWidgetProvider.UpdateWidget(null);
            return;
        }

        var now = DateTime.Now;
        
        // Find the current ongoing course
        var currentCourse = scheduleItems
            .Where(item => item.StartTime <= now && item.EndTime > now)
            .FirstOrDefault();

        if (currentCourse != null)
        {
            ProlizyScheduleWidgetProvider.UpdateWidget(currentCourse);
        }
        else
        {
            // If no current course, show next course
            UpdateWithNextCourse(scheduleItems);
        }
    }

    /// <summary>
    /// Clears the widget content
    /// </summary>
    public static void ClearWidget()
    {
        ProlizyScheduleWidgetProvider.UpdateWidget(null);
    }
}
