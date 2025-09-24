using System;
using Android.Content;
using Prolizy2.Model;

namespace Prolizy2.Android.Widget;

public static class ProlizyWidgetService {
    
    private static bool _isInitialized = false;
    
    /// <summary>
    /// Update widget to show a specific course
    /// Useful when user selects a specific course in the app
    /// </summary>
    /// <param name="scheduleItem">The specific course to show</param>
    public static void ShowSpecificCourse(ScheduleItem scheduleItem)
    {
        try
        {
            ScheduleWidgetManager.UpdateWithSpecificCourse(scheduleItem);
            System.Diagnostics.Debug.WriteLine($"Widget updated with specific course: {scheduleItem.Subject}");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error showing specific course: {ex.Message}");
        }
    }

    /// <summary>
    /// Clear the widget content
    /// Call this when user logs out or when no schedule is available
    /// </summary>
    public static void ClearWidget()
    {
        try
        {
            ScheduleWidgetManager.ClearWidget();
            System.Diagnostics.Debug.WriteLine("Widget cleared");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error clearing widget: {ex.Message}");
        }
    }

    /// <summary>
    /// Handle widget click from the home screen
    /// This method can be called when the widget is tapped
    /// </summary>
    public static void HandleWidgetClick(Context context)
    {
        try
        {
            // Launch the main application
            var launchIntent = context.PackageManager?.GetLaunchIntentForPackage(context.PackageName);
            if (launchIntent != null)
            {
                launchIntent.AddFlags(ActivityFlags.NewTask | ActivityFlags.ClearTop);
                context.StartActivity(launchIntent);
                System.Diagnostics.Debug.WriteLine("Main app launched from widget click");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error handling widget click: {ex.Message}");
        }
    }

    /// <summary>
    /// Get widget status for debugging
    /// </summary>
    /// <returns>True if widget service is initialized</returns>
    public static bool IsInitialized => _isInitialized;
}