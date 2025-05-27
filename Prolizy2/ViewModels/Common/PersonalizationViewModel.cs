using System;
using System.Collections.ObjectModel;
using Avalonia.Styling;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SukiUI;
using SukiUI.Enums;
using SukiUI.Models;

namespace Prolizy2.ViewModels.Common;

public partial class PersonalizationViewModel : ViewModelBase
{
    public MainViewModel MainView { get; init; }

    public ObservableCollection<SukiColorTheme> AvailableColors { get; } = new();
    public ObservableCollection<SukiBackgroundStyle> AvailableBackgroundStyles { get; } = new();
    
    [ObservableProperty] private bool _isLightTheme;
    [ObservableProperty] private SukiBackgroundStyle _backgroundStyle ;
    [ObservableProperty] private bool _backgroundAnimations;
    [ObservableProperty] private bool _backgroundTransitions;
    
    private readonly SukiTheme _theme = SukiTheme.GetInstance();

    public PersonalizationViewModel(MainViewModel mainVm)
    {
        MainView = mainVm;
        
        AvailableBackgroundStyles = new ObservableCollection<SukiBackgroundStyle>(Enum.GetValues<SukiBackgroundStyle>());
        AvailableColors = new ObservableCollection<SukiColorTheme>(_theme.ColorThemes);
        IsLightTheme = _theme.ActiveBaseTheme == ThemeVariant.Light;
        _theme.OnBaseThemeChanged += variant =>
            IsLightTheme = variant == ThemeVariant.Light;
    }
    
    partial void OnIsLightThemeChanged(bool value) =>
        _theme.ChangeBaseTheme(value ? ThemeVariant.Light : ThemeVariant.Dark);
    
    [RelayCommand]
    private void SwitchToColorTheme(SukiColorTheme colorTheme) =>
        _theme.ChangeColorTheme(colorTheme);

    partial void OnBackgroundStyleChanged(SukiBackgroundStyle value) =>
        Views.MainView.SukiHost.BackgroundStyle = value;
    
    partial void OnBackgroundAnimationsChanged(bool value) => 
        Views.MainView.SukiHost.BackgroundAnimationEnabled = value;
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