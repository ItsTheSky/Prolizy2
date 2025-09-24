using System;
using Prolizy2.Model;

namespace Prolizy2.Utilities.Bridges;

public interface IAndroidBridge
{

    void UpdateScheduleWidget(ScheduleItem? scheduleItem);

}

public static class AndroidBridgeProvider
{
    public static IAndroidBridge? Bridge { get; private set; }

    public static void SetBridge(IAndroidBridge bridge)
    {
        Bridge = bridge ?? throw new ArgumentNullException(nameof(bridge), "Bridge cannot be null.");
    }
}