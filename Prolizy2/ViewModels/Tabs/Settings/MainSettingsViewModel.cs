using System.Reflection;
using Avalonia;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.Input;
using Prolizy2.ViewModels.Common;
using Prolizy2.Views.Common;
using Prolizy2.Views.Tabs.Settings;

namespace Prolizy2.ViewModels.Tabs.Settings;

public partial class MainSettingsViewModel(SettingsViewModel settingsViewModel) : ViewModelBase
{
    
    [RelayCommand]
    public void GoToHome()
    {
        settingsViewModel.SwitchToPage(new Views.Tabs.Settings.HomeSettingsView()
        {
            DataContext = new HomeSettingsViewModel(settingsViewModel.MainViewModel, settingsViewModel)
        });
    }

    [RelayCommand]
    public void GoToEdt()
    {
        settingsViewModel.SwitchToPage(new EdtSettingsView()
        {
            DataContext = new EdtSettingsViewModel(settingsViewModel.MainViewModel, settingsViewModel)
        });
    }

    [RelayCommand]
    public void GoToBulletin()
    {
        settingsViewModel.SwitchToPage(new BulletinSettingsView()
        {
            DataContext = new BulletinSettingsViewModel(settingsViewModel.MainViewModel, settingsViewModel)
        });
    }

    [RelayCommand]
    public void GoToPersonalisation()
    {
        settingsViewModel.SwitchToPage(new PersonalizationView()
        {
            DataContext = new PersonalizationViewModel(settingsViewModel.MainViewModel, settingsViewModel)
        });
    }
    
    public static string Version =>
        (Assembly.GetExecutingAssembly().GetName().Version?.ToString(3) ?? "Unknown") + " (" + Assembly.GetAssembly(typeof(Control))?.GetName().Version + ")";
    
}