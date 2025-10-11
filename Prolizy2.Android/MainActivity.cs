using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Views;
using Avalonia;
using Avalonia.Android;
using Prolizy2.API.Utils;
using Prolizy2.Utilities.Bridges;
using Xamarin.Android.Net;

namespace Prolizy2.Android;

[Activity(
    Label = "Prolizy2.Android",
    Theme = "@style/MyTheme.NoActionBar",
    Icon = "@drawable/icon",
    MainLauncher = true,
    ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.ScreenSize | ConfigChanges.UiMode)]
public class MainActivity : AvaloniaMainActivity
{

    protected override void OnCreate(Bundle? savedInstanceState)
    {
        base.OnCreate(savedInstanceState);
        AndroidBridgeProvider.SetBridge(new AndroidBridge());

        // Configure HttpClient avec AndroidMessageHandler pour compatibilité Android 10
        var handler = new AndroidMessageHandler
        {
            ServerCertificateCustomValidationCallback = (message, cert, chain, errors) =>
            {
                // Pour Android 10, on autorise les certificats système
                return true; // En production, implémenter une vraie validation
            }
        };

        var httpClient = new System.Net.Http.HttpClient(handler);
        RequestUtils.SetHttpClient(httpClient);

        Window.SetFlags(WindowManagerFlags.HardwareAccelerated,
            WindowManagerFlags.HardwareAccelerated);
    }
}