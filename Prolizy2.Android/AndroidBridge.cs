using Prolizy2.Android.Widget;
using Prolizy2.Model;
using Prolizy2.Utilities.Bridges;

namespace Prolizy2.Android;

public class AndroidBridge : IAndroidBridge
{
    public void UpdateScheduleWidget(ScheduleItem? scheduleItem)
    {
        ProlizyScheduleWidgetProvider.UpdateWidget(scheduleItem);
    }
}