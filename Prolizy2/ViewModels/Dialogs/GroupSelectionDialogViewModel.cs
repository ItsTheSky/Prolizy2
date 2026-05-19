using System.Collections.ObjectModel;
using System.Threading.Tasks;
using AsyncAwaitBestPractices;
using Avalonia.Controls.Metadata;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Prolizy2.API;
using Prolizy2.API.Model.Request;
using Prolizy2.ViewModels.Tabs;
using Prolizy2.ViewModels.Tabs.Settings;
using Prolizy2.Views;
using Prolizy2.Views.Tabs;
using SukiUI.Dialogs;

namespace Prolizy2.ViewModels.Dialogs;

public partial class GroupSelectionDialogViewModel(ISukiDialog dialog, EdtSettingsViewModel edtSettingsViewModel, bool isSecondaryGroup)
    : ObservableObject
{
    [ObservableProperty] private bool _isSelectedGroupValid;

    [ObservableProperty] private string? _selectedGroup;
    [ObservableProperty] private ObservableCollection<string> _loadedGroups = [];

    [ObservableProperty] private string query;
    [ObservableProperty] private bool _isQueryValid = false;
    [ObservableProperty] private bool _areResultsEmpty = false;

    [RelayCommand]
    public void CloseDialog()
    {
        dialog.Dismiss();
    }

    [RelayCommand]
    public Task ValidateSelection()
    {
        if (isSecondaryGroup)
        {
            MainView.Preferences.Set("edt_secondary_group", SelectedGroup);
            edtSettingsViewModel.SecondaryGroup = SelectedGroup;
        }
        else
        {
            MainView.Preferences.Set("edt_group", SelectedGroup);
            edtSettingsViewModel.CurrentGroup = SelectedGroup;

            edtSettingsViewModel.MainView.EdtViewModel.IsConfigured = true;
            edtSettingsViewModel.MainView.EdtViewModel.IsLoading = false;
        }

        edtSettingsViewModel.MainView.EdtViewModel.ClearCache();
        edtSettingsViewModel.MainView.EdtViewModel.TryRefreshCurrentDay();

        if (!isSecondaryGroup)
        {
            edtSettingsViewModel.SettingsView.SwitchToMain();
            edtSettingsViewModel.MainView.NavigateTo(typeof(EdtViewModel));
        }

        dialog.Dismiss();
        return Task.CompletedTask;
    }

    partial void OnSelectedGroupChanged(string? value)
    {
        IsSelectedGroupValid = value != null;
    }

    partial void OnQueryChanged(string value)
    {
        if (value.Length < 2)
        {
            IsQueryValid = false;
            AreResultsEmpty = false;
            LoadedGroups.Clear();
            return;
        }

        Dispatcher.UIThread.InvokeAsync(async () =>
        {
            var groups = await EDTClient.GetGroups(new FederationRequest()
            {
                Page = 1, PageSize = 50,
                SearchTerm = value
            });

            IsQueryValid = true;
            AreResultsEmpty = groups.Count == 0;
            LoadedGroups = new ObservableCollection<string>(groups);
            if (SelectedGroup != null && !LoadedGroups.Contains(SelectedGroup))
                SelectedGroup = null;
        }).SafeFireAndForget();
    }
}