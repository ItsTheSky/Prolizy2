using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Prolizy2.Utilities;
using Prolizy2.ViewModels.Dialogs;
using SukiUI.Dialogs;

namespace Prolizy2.ViewModels.Tabs.Settings;

public partial class EdtSettingsViewModel : ViewModelBase
{
    public MainViewModel MainView { get; init; }
    public SettingsViewModel SettingsView { get; init; }

    [ObservableProperty] private string? _currentGroup;
    [ObservableProperty] private string? _secondaryGroup;
    [ObservableProperty] private bool _hasCourseDataInCard;

    public EdtSettingsViewModel(MainViewModel mainVm, SettingsViewModel settingsViewModel)
    {
        MainView = mainVm;
        SettingsView = settingsViewModel;

        CurrentGroup = Views.MainView.Preferences.Get<string?>("edt_group");
        SecondaryGroup = Views.MainView.Preferences.Get<string?>("edt_secondary_group");
        HasCourseDataInCard = Views.MainView.Preferences.Get("edt_course_data_in_card", true);
    }

    [RelayCommand]
    public void EditGroup()
    {
        Views.MainView.DialogManager.CreateDialog()
            .WithViewModel(dialog => new GroupSelectionDialogViewModel(dialog, this, false))
            .TryShow();
    }

    [RelayCommand]
    public void EditSecondaryGroup()
    {
        Views.MainView.DialogManager.CreateDialog()
            .WithViewModel(dialog => new GroupSelectionDialogViewModel(dialog, this, true))
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
    public void DeleteSecondaryGroup()
    {
        Views.MainView.Preferences.Remove("edt_secondary_group");

        MainView.EdtViewModel.ClearCache();
        MainView.EdtViewModel.TryRefreshCurrentDay();

        SecondaryGroup = null;
    }

    [RelayCommand]
    public void Back()
    {
        SettingsView.SwitchToMain();
    }
    
    partial void OnHasCourseDataInCardChanged(bool value)
    {
        Views.MainView.Preferences.Set("edt_course_data_in_card", value);
        Paths.SetResourceKey("ProlizyHasItemCardData", value);
    }
}