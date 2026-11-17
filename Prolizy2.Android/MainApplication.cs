using System;
using Android.App;
using Android.Runtime;
using Avalonia;
using Avalonia.Android;

namespace Prolizy2.Android;

[Application]
public class MainApplication : AvaloniaAndroidApplication<App>
{
    protected MainApplication(IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer) { }
    
    
    protected override AppBuilder CustomizeAppBuilder(AppBuilder builder)
    {
        return base.CustomizeAppBuilder(builder)
            .UseSkia()
            .With(new SkiaOptions
            {
                // 1 GB was far more than a phone can give a single app's GPU
                // cache; an oversized budget only invites memory pressure.
                MaxGpuResourceSizeBytes = 128 * 1024 * 1024 // 128 MB
            })
            .WithInterFont();
    }
}