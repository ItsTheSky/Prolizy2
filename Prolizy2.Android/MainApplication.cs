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
                MaxGpuResourceSizeBytes = 1024 * 1024 * 1024 // 1 GB
            })
            .WithInterFont();
    }
}