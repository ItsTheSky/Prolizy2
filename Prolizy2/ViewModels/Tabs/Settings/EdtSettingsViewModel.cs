using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Prolizy2.ViewModels.Dialogs;
using SukiUI.Dialogs;

namespace Prolizy2.ViewModels.Tabs.Settings;

public partial class EdtSettingsViewModel : ViewModelBase
{
    public MainViewModel MainView { get; init; }
    public SettingsViewModel SettingsView { get; init; }

    [ObservableProperty] private string? _currentGroup;

    public EdtSettingsViewModel(MainViewModel mainVm, SettingsViewModel settingsViewModel)
    {
        MainView = mainVm;
        SettingsView = settingsViewModel;
        
        CurrentGroup = Views.MainView.Preferences.Get<string?>("edt_group");
    }

    [RelayCommand]
    public void EditGroup()
    {
        Views.MainView.DialogManager.CreateDialog()
            .WithViewModel(dialog => new GroupSelectionDialogViewModel(dialog, this))
            .TryShow();
    }

    [RelayCommand]
    public void DeleteGroup()
    {
        Views.MainView.Preferences.Remove("edt_group");

        MainView.EdtViewModel.IsConfigured = false;
        MainView.EdtViewModel.IsLoading = false;
        
        MainView.EdtViewModel.ClearCache();
        MainView.EdtViewModel.TryRefreshCurrentDay();

        CurrentGroup = null;
    }

    [RelayCommand]
    public void Back()
    {
        SettingsView.SwitchToMain();
    }
}