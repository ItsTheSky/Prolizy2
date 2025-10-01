using CommunityToolkit.Mvvm.Input;
using Prolizy2.ViewModels.Common;
using Prolizy2.Views.Common;

namespace Prolizy2.ViewModels.Tabs.Settings;

public partial class MainSettingsViewModel(SettingsViewModel settingsViewModel) : ViewModelBase
{
    
    [RelayCommand]
    public void GoToHome()
    {
        
    }

    [RelayCommand]
    public void GoToEdt()
    {
        
    }

    [RelayCommand]
    public void GoToBulletin()
    {
        
    }

    [RelayCommand]
    public void GoToPersonalisation()
    {
        settingsViewModel.SwitchToPage(new PersonalizationView()
        {
            DataContext = new PersonalizationViewModel(settingsViewModel.MainViewModel, settingsViewModel)
        });
    }
    
}