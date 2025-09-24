using Android.App;
using Android.Appwidget;
using Android.Content;
using Android.Graphics;
using Android.Widget;
using System;
using System.Globalization;
using Prolizy2.Model;
using Prolizy2.API.Model.Course;
using Prolizy2.API.Utils;

namespace Prolizy2.Android.Widget;

[BroadcastReceiver(Label = "Prolizy Schedule Widget")]
[IntentFilter(new string[] { "android.appwidget.action.APPWIDGET_UPDATE" })]
[MetaData("android.appwidget.provider", Resource = "@xml/prolizy_schedule_widget_info")]
public class ProlizyScheduleWidgetProvider : AppWidgetProvider
{
    private const string ACTION_WIDGET_REFRESH = "com.companyname.prolizy2.SCHEDULE_WIDGET_REFRESH";
    private const string ACTION_WIDGET_OPEN_APP = "com.companyname.prolizy2.SCHEDULE_WIDGET_OPEN_APP";
    
    private static ScheduleItem? _currentScheduleItem;

    public override void OnUpdate(Context context, AppWidgetManager appWidgetManager, int[] appWidgetIds)
    {
        foreach (int appWidgetId in appWidgetIds)
        {
            UpdateAppWidget(context, appWidgetManager, appWidgetId);
        }
    }

    public override void OnReceive(Context context, Intent intent)
    {
        base.OnReceive(context, intent);

        if (intent?.Action == ACTION_WIDGET_REFRESH)
        {
            var appWidgetManager = AppWidgetManager.GetInstance(context);
            var appWidgetIds = appWidgetManager.GetAppWidgetIds(
                new ComponentName(context, Java.Lang.Class.FromType(typeof(ProlizyScheduleWidgetProvider))));
            
            foreach (int appWidgetId in appWidgetIds)
            {
                UpdateAppWidget(context, appWidgetManager, appWidgetId);
            }
        }
        else if (intent?.Action == ACTION_WIDGET_OPEN_APP)
        {
            var launchIntent = context.PackageManager?.GetLaunchIntentForPackage(context.PackageName);
            if (launchIntent != null)
            {
                launchIntent.AddFlags(ActivityFlags.NewTask);
                context.StartActivity(launchIntent);
            }
        }
    }

    /// <summary>
    /// Updates the widget with the provided ScheduleItem data
    /// </summary>
    /// <param name="scheduleItem">The schedule item to display in the widget</param>
    public static void UpdateWidget(ScheduleItem? scheduleItem)
    {
        _currentScheduleItem = scheduleItem;
        
        // Get the context from the current application
        var context = Application.Context;
        if (context == null) 
            return;

        var appWidgetManager = AppWidgetManager.GetInstance(context);
        var appWidgetIds = appWidgetManager.GetAppWidgetIds(
            new ComponentName(context, Java.Lang.Class.FromType(typeof(ProlizyScheduleWidgetProvider))));
        
        foreach (int appWidgetId in appWidgetIds)
        {
            UpdateAppWidget(context, appWidgetManager, appWidgetId);
        }
    }

    private static void UpdateAppWidget(Context context, AppWidgetManager appWidgetManager, int appWidgetId)
    {
        try
        {
            var views = new RemoteViews(context.PackageName, Resource.Layout.prolizy_schedule_widget);

            // Set up click intent to open app
            SetupClickIntent(context, views);

            // Update widget content with schedule item data
            UpdateWidgetContent(views);

            // Update the widget
            appWidgetManager.UpdateAppWidget(appWidgetId, views);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error updating schedule widget: {ex.Message}");
        }
    }

    private static void SetupClickIntent(Context context, RemoteViews views)
    {
        // Open app intent when widget is clicked
        var openAppIntent = new Intent(context, typeof(ProlizyScheduleWidgetProvider));
        openAppIntent.SetAction(ACTION_WIDGET_OPEN_APP);
        var openAppPendingIntent = PendingIntent.GetBroadcast(context, 1, openAppIntent, 
            PendingIntentFlags.UpdateCurrent | PendingIntentFlags.Immutable);
        
        // Set click listener on the entire widget
        views.SetOnClickPendingIntent(Resource.Id.widget_title, openAppPendingIntent);
        views.SetOnClickPendingIntent(Resource.Id.widget_course_title, openAppPendingIntent);
    }

    private static void UpdateWidgetContent(RemoteViews views)
    {
        try
        {
            if (_currentScheduleItem != null)
            {
                SetScheduleItemContent(views, _currentScheduleItem);
            }
            else
            {
                SetDefaultContent(views);
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error updating widget content: {ex.Message}");
            SetDefaultContent(views);
        }
    }

    private static void SetScheduleItemContent(RemoteViews views, ScheduleItem item)
    {
        // Title
        views.SetTextViewText(Resource.Id.widget_title, "Prochain Cours");
        
        // Domain/Subject - Using the formatted type (TD, TP, CM, etc.) with icon
        views.SetTextViewText(Resource.Id.widget_domain, $"📚 {item.FormattedType}");
        
        // Time with icon
        views.SetTextViewText(Resource.Id.widget_time, $"⏰ {item.FormattedTimeRange}");
        
        // Course title
        views.SetTextViewText(Resource.Id.widget_course_title, item.Subject);
        
        // Room with icon
        views.SetTextViewText(Resource.Id.widget_room, $"🏫 {item.Room}");
        
        // Professor with icon
        views.SetTextViewText(Resource.Id.widget_professor, $"👨‍🏫 {item.Professor}");
        
        // Additional info
        var additionalInfo = !string.IsNullOrEmpty(item.AdditionalText) 
            ? item.AdditionalText 
            : "Cliquez pour plus d'informations";
        views.SetTextViewText(Resource.Id.widget_additional_info, additionalInfo);

        // Set background colors based on course type
        SetCourseTypeBackground(views, item.Type);
    }

    private static void SetCourseTypeBackground(RemoteViews views, string courseType)
    {
        int domainBg, timeBg, roomBg, professorBg;

        // Customize colors based on course type
        switch (courseType)
        {
            case CourseTypes.Examen:
                // Red theme for exams
                domainBg = Resource.Drawable.exam_background;
                timeBg = Resource.Drawable.exam_background;
                roomBg = Resource.Drawable.exam_background;
                professorBg = Resource.Drawable.exam_background;
                break;
            case CourseTypes.TravauxDiriges:
                // Green theme for TD
                domainBg = Resource.Drawable.td_background;
                timeBg = Resource.Drawable.time_background;
                roomBg = Resource.Drawable.room_background;
                professorBg = Resource.Drawable.professor_background;
                break;
            case CourseTypes.TravauxPratiques:
                // Purple theme for TP
                domainBg = Resource.Drawable.tp_background;
                timeBg = Resource.Drawable.time_background;
                roomBg = Resource.Drawable.room_background;
                professorBg = Resource.Drawable.professor_background;
                break;
            case CourseTypes.CoursMagistral:
                // Blue theme for CM
                domainBg = Resource.Drawable.cm_background;
                timeBg = Resource.Drawable.time_background;
                roomBg = Resource.Drawable.room_background;
                professorBg = Resource.Drawable.professor_background;
                break;
            case CourseTypes.Projet:
                // Orange theme for SAÉ/Projects
                domainBg = Resource.Drawable.sae_background;
                timeBg = Resource.Drawable.time_background;
                roomBg = Resource.Drawable.room_background;
                professorBg = Resource.Drawable.professor_background;
                break;
            default:
                // Default colors
                domainBg = Resource.Drawable.domain_background;
                timeBg = Resource.Drawable.time_background;
                roomBg = Resource.Drawable.room_background;
                professorBg = Resource.Drawable.professor_background;
                break;
        }

        // Apply backgrounds
        views.SetInt(Resource.Id.widget_domain, "setBackgroundResource", domainBg);
        views.SetInt(Resource.Id.widget_time, "setBackgroundResource", timeBg);
        views.SetInt(Resource.Id.widget_room, "setBackgroundResource", roomBg);
        views.SetInt(Resource.Id.widget_professor, "setBackgroundResource", professorBg);
    }

    private static void SetDefaultContent(RemoteViews views)
    {
        views.SetTextViewText(Resource.Id.widget_title, "Prochain Cours");
        views.SetTextViewText(Resource.Id.widget_domain, "📚 Domaine");
        views.SetTextViewText(Resource.Id.widget_time, "⏰ -- : --");
        views.SetTextViewText(Resource.Id.widget_course_title, "Aucun cours programmé");
        views.SetTextViewText(Resource.Id.widget_room, "🏫 Salle");
        views.SetTextViewText(Resource.Id.widget_professor, "👨‍🏫 Professeur");
        views.SetTextViewText(Resource.Id.widget_additional_info, "Cliquez pour ouvrir l'application");
    }
}
