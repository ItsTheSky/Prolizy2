using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Input;
using Prolizy2.ViewModels.Dialogs;
using SukiUI.Dialogs;

namespace Prolizy2.ViewModels.Tabs.Settings;

public partial class EdtSettingsViewModel : ViewModelBase
{
    public MainViewModel MainView { get; init; }
    public SettingsViewModel SettingsView { get; init; }

    public EdtSettingsViewModel(MainViewModel mainVm, SettingsViewModel settingsViewModel)
    {
        MainView = mainVm;
        SettingsView = settingsViewModel;
    }

    [RelayCommand]
    public void EditGroup()
    {
        Views.MainView.DialogManager.CreateDialog()
            .WithViewModel(dialog => new GroupSelectionDialogViewModel(dialog, this))
            .TryShow();
    }

    [RelayCommand]
    public void Back()
    {
        SettingsView.SwitchToMain();
    }
}