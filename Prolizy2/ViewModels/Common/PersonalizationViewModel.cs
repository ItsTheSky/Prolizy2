using System;
using System.Collections.ObjectModel;
using System.Linq;
using Avalonia.Styling;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Lucide.Avalonia;
using Prolizy2.Utilities;
using Prolizy2.ViewModels.Tabs;
using Prolizy2.Views.Tabs;
using SukiUI;
using SukiUI.Enums;
using SukiUI.Models;

namespace Prolizy2.ViewModels.Common;

public partial class PersonalizationViewModel : ViewModelBase
{
    public MainViewModel MainView { get; init; }
    public SettingsViewModel SettingsView { get; init; }

    public ObservableCollection<SukiColorTheme> AvailableColors { get; } = new();
    public ObservableCollection<SukiBackgroundStyle> AvailableBackgroundStyles { get; } = new();
    
    [ObservableProperty] private bool _isLightTheme;
    [ObservableProperty] private SukiBackgroundStyle _backgroundStyle ;
    [ObservableProperty] private bool _backgroundAnimations;
    [ObservableProperty] private bool _backgroundTransitions;
    
    private readonly SukiTheme _theme = SukiTheme.GetInstance();

    public PersonalizationViewModel(MainViewModel mainVm, SettingsViewModel settingsViewModel)
    {
        MainView = mainVm;
        SettingsView = settingsViewModel;
        
        AvailableBackgroundStyles = new ObservableCollection<SukiBackgroundStyle>(Enum.GetValues<SukiBackgroundStyle>());
        AvailableColors = new ObservableCollection<SukiColorTheme>(_theme.ColorThemes);
        IsLightTheme = _theme.ActiveBaseTheme == ThemeVariant.Light;
        _theme.OnBaseThemeChanged += variant =>
            IsLightTheme = variant == ThemeVariant.Light;
        
        LoadFromPreferences();
    }
    
    partial void OnIsLightThemeChanged(bool value)
    {
        _theme.ChangeBaseTheme(value ? ThemeVariant.Light : ThemeVariant.Dark);
        Views.MainView.Preferences.Set("per_base_theme", value ? "Light" : "Dark");
    }

    [RelayCommand]
    private void SwitchToColorTheme(SukiColorTheme colorTheme)
    {
        _theme.ChangeColorTheme(colorTheme);
        Views.MainView.Preferences.Set("per_color_theme", colorTheme.DisplayName);
    }

    [RelayCommand]
    private void SwitchToDebugPane()
    {
        MainView.NavigateTo(new TabItemEntry(MainView)
        {
            Icon = LucideIconKind.MonitorCog,
            Title = "TabDebug",
            Content = new DebugTab()
        });
    }

    [RelayCommand]
    public void Back()
    {
        SettingsView.SwitchToMain();
    }

    partial void OnBackgroundStyleChanged(SukiBackgroundStyle value)
    {
        Views.MainView.SukiHost.BackgroundStyle = value;
        Views.MainView.Preferences.Set("per_background_style", value.ToString());
    }

    partial void OnBackgroundAnimationsChanged(bool value)
    {
        Views.MainView.SukiHost.BackgroundAnimationEnabled = value;
        Views.MainView.Preferences.Set("per_background_animations", value);
    }


    public void LoadFromPreferences()
    {
        var backgroundStyleStr = Views.MainView.Preferences.Get("per_background_style", "Mica");
        BackgroundStyle = Enum.TryParse<SukiBackgroundStyle>(backgroundStyleStr, out var bgStyle) ? bgStyle : SukiBackgroundStyle.Gradient;
        BackgroundAnimations = Views.MainView.Preferences.Get("per_background_animations", true);
    }

    public static void LoadFromPreferencesToApp()
    {
        var theme = SukiTheme.GetInstance();
        Logs.Debug("Given theme is " + theme);
        var baseTheme = Views.MainView.Preferences.Get("per_base_theme", "Light");
        theme.ChangeBaseTheme(baseTheme == "Light" ? ThemeVariant.Light : ThemeVariant.Dark);
        var colorThemeName = Views.MainView.Preferences.Get("per_color_theme", "Blue");
        Logs.Debug($"PER: colorThemeName: {colorThemeName}");
        var colorTheme = theme.ColorThemes.ToList().Find(t => t.DisplayName == colorThemeName);
        Logs.Debug($"PER: colorTheme: {colorTheme}");
        if (colorTheme != null)
            theme.ChangeColorTheme(colorTheme);
        var backgroundStyleStr = Views.MainView.Preferences.Get("per_background_style", "Gradient");
        Logs.Debug($"PER: backgroundStyleStr: {backgroundStyleStr}");
        var backgroundStyle = Enum.TryParse<SukiBackgroundStyle>(backgroundStyleStr, out var bgStyle) ? bgStyle : SukiBackgroundStyle.Gradient;
        Logs.Debug($"PER: backgroundStyle: {backgroundStyle}");
        Views.MainView.SukiHost.BackgroundStyle = backgroundStyle;
        var backgroundAnimations = Views.MainView.Preferences.Get("per_background_animations", true);
        Views.MainView.SukiHost.BackgroundAnimationEnabled = backgroundAnimations;
    }
}

public partial class PersonalizationLangViewModel : ObservableObject
{

    /*[ObservableProperty] private string _baseThemeHeader;
    [ObservableProperty] private string _colorThemeHeader;
    [ObservableProperty] private string _backgroundHeader;*/
    public string LangHeader { get; set; }
    public string BaseThemeHeader { get; set; }
    public string ColorThemeHeader { get; set; }
    public string BackgroundHeader { get; set; }

    public PersonalizationLangViewModel(MainViewModel mainVm)
    {
        LangHeader = LangViewModel.Translate("");
        BaseThemeHeader = LangViewModel.Translate("");
        ColorThemeHeader = LangViewModel.Translate("");
        BackgroundHeader = LangViewModel.Translate("");
    }
    

}