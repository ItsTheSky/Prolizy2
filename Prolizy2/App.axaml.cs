using System.Globalization;
using System.Threading.Tasks;
using AsyncAwaitBestPractices;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using Prolizy2.Utilities;
using Prolizy2.ViewModels;
using Prolizy2.ViewModels.Common;
using Prolizy2.Views;
using SukiUI.Controls;
using SukiUI.Enums;

namespace Prolizy2;

public partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
#if DEBUG
        this.AttachDeveloperTools();
#endif
        Logs.Init();
    }

    public override void OnFrameworkInitializationCompleted()
    {
        // Apply the saved theme before any view exists: doing it afterwards
        // re-evaluates every dynamic resource of the tree that was just built
        // (and used to flash the default theme first).
        PersonalizationViewModel.ApplyThemeFromPreferences();
        var backgroundAnimations = PersonalizationViewModel.GetSavedBackgroundAnimations();

        var view = new MainView { DataContext = new MainViewModel() };
        var sukiMiddleman = new SukiMainHost
        {
            BackgroundStyle = PersonalizationViewModel.GetSavedBackgroundStyle(),
            BackgroundTransitionsEnabled = true,
            Content = view
        };
        MainView.SukiHost = sukiMiddleman;

        // The background only starts animating once it is on screen (desktop and mobile alike).
        sukiMiddleman.Loaded += (_, _) => sukiMiddleman.BackgroundAnimationEnabled = backgroundAnimations;

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow { Content = sukiMiddleman };
            MainView.TopLevel = desktop.MainWindow;
        }
        else if (ApplicationLifetime is ISingleViewApplicationLifetime singleViewPlatform)
        {
            singleViewPlatform.MainView = sukiMiddleman;
            MainView.TopLevel = TopLevel.GetTopLevel(singleViewPlatform.MainView)!;
        }

        base.OnFrameworkInitializationCompleted();
    }
}