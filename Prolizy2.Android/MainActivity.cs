using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Views;
using Avalonia;
using Avalonia.Android;
using Prolizy2.Utilities.Bridges;

namespace Prolizy2.Android;

[Activity(
    Label = "Prolizy2.Android",
    Theme = "@style/MyTheme.NoActionBar",
    Icon = "@drawable/icon",
    MainLauncher = true,
    ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.ScreenSize | ConfigChanges.UiMode)]
public class MainActivity : AvaloniaMainActivity<App>
{
    protected override AppBuilder CustomizeAppBuilder(AppBuilder builder)
    {
        return base.CustomizeAppBuilder(builder)
            .UseSkia()
            .With(new SkiaOptions
            {
                MaxGpuResourceSizeBytes = 1024 * 1024 * 1024 // 1 GB
            })
            .WithInterFont();
    }

    protected override void OnCreate(Bundle? savedInstanceState)
    {
        base.OnCreate(savedInstanceState);
        AndroidBridgeProvider.SetBridge(new AndroidBridge());
        
        Window.SetFlags(WindowManagerFlags.HardwareAccelerated, 
            WindowManagerFlags.HardwareAccelerated);
    }
}