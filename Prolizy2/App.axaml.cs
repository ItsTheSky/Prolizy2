using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Prolizy2.Utilities;
using Prolizy2.ViewModels;
using Prolizy2.Views;
using SukiUI.Controls;
using SukiUI.Enums;

namespace Prolizy2;

public partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
        this.AttachDeveloperTools();
        Logs.Init();
    }

    public override void OnFrameworkInitializationCompleted()
    {

        var view = new MainView { DataContext = new MainViewModel() };
        var sukiMiddleman = new SukiMainHost
        {
            BackgroundStyle = SukiBackgroundStyle.Bubble,
            BackgroundTransitionsEnabled = true,
            Content = view
        };
        MainView.SukiHost = sukiMiddleman;

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow { Content = sukiMiddleman };
            MainView.TopLevel = desktop.MainWindow;
        }
        else if (ApplicationLifetime is ISingleViewApplicationLifetime singleViewPlatform)
        {
            singleViewPlatform.MainView = sukiMiddleman;
            singleViewPlatform.MainView.Loaded += (sender, args) =>
            {
                (singleViewPlatform.MainView as SukiMainHost)!.BackgroundAnimationEnabled = true;
            };
            MainView.TopLevel = TopLevel.GetTopLevel(singleViewPlatform.MainView)!;
        }

        base.OnFrameworkInitializationCompleted();
    }
}