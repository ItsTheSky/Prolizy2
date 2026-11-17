using System;
using Avalonia.Animation;
using Avalonia.Animation.Easings;
using Avalonia.Controls;
using Avalonia.Controls.Notifications;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Prolizy2.Utilities;
using Prolizy2.ViewModels.Tabs.Settings;
using Prolizy2.Views;
using Prolizy2.Views.Tabs.Settings;
using SukiUI.Toasts;

namespace Prolizy2.ViewModels.Tabs;

public partial class SettingsViewModel : ViewModelBase
{
    public MainViewModel MainViewModel { get; init; }

    [ObservableProperty] private Control? _currentPage;
    [ObservableProperty] private MainSettingsViewModel _mainSettingsViewModel;

    #region Transitions

    private static readonly IPageTransition SlideLeftTransition = new DirectionalSlideTransition(
        DirectionalSlideTransition.SlideDirection.RightToLeft,
        TimeSpan.FromMilliseconds(200),
        TimeSpan.FromMilliseconds(200),
        new CubicEaseOut()
    );
    
    private static readonly IPageTransition SlideRightTransition = new DirectionalSlideTransition(
        DirectionalSlideTransition.SlideDirection.LeftToRight,
        TimeSpan.FromMilliseconds(200),
        TimeSpan.FromMilliseconds(200),
        new CubicEaseOut()
    );

    [ObservableProperty] private IPageTransition _currentTransitions = SlideRightTransition;
    #endregion

    /// <inheritdoc/>
    public SettingsViewModel(MainViewModel mainVm)
    {
        MainViewModel = mainVm;
        
        MainSettingsViewModel = new MainSettingsViewModel(this);
    }

    /// <summary>
    /// Builds the landing page the first time the settings tab is shown. It used
    /// to be created with the view model, i.e. during application start-up.
    /// </summary>
    public void EnsureInitialized()
    {
        CurrentPage ??= new MainSettingsView { DataContext = MainSettingsViewModel };
    }

    public void SwitchToPage(Control? control)
    {
        CurrentPage = control;
        CurrentTransitions = SlideLeftTransition;
    }

    public void SwitchToMain()
    {
        CurrentPage = new MainSettingsView { DataContext = MainSettingsViewModel };
        CurrentTransitions = SlideRightTransition;
    }
}