using System.Net;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Prolizy2.Styles.Controls;
using Prolizy2.ViewModels.Bulletin;
using Prolizy2.ViewModels.Common;
using Prolizy2.Views;

namespace Prolizy2.ViewModels.Tabs.Settings;

/// <summary>
/// Settings sub-page that edits the shared <see cref="BulletinDisplayOptions"/>
/// and stores the ScoDoc credentials used by the bulletin tab.
/// </summary>
public partial class BulletinSettingsViewModel : ViewModelBase
{
    public MainViewModel MainVm { get; }
    public SettingsViewModel SettingsView { get; }

    /// <summary>The shared display options — bound directly by the view.</summary>
    public BulletinDisplayOptions Options => BulletinDisplayOptions.Instance;

    [ObservableProperty] private string _username = string.Empty;
    [ObservableProperty] private string _password = string.Empty;

    [ObservableProperty] private bool _isBusy;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasStatus))]
    private string? _statusMessage;

    [ObservableProperty] private BadgeType _statusBadge = BadgeType.Neutral;

    [ObservableProperty] private bool _isConfigured;

    public bool HasStatus => !string.IsNullOrEmpty(StatusMessage);

    public BulletinSettingsViewModel(MainViewModel mainVm, SettingsViewModel settingsViewModel)
    {
        MainVm = mainVm;
        SettingsView = settingsViewModel;

        _ = LoadCredentialsAsync();
    }

    private async Task LoadCredentialsAsync()
    {
        IsConfigured = await MainVm.BulletinService.IsConfiguredAsync();
        Username = await MainVm.BulletinService.GetUsernameAsync() ?? string.Empty;
        Password = await MainVm.BulletinService.GetPasswordAsync() ?? string.Empty;

        if (IsConfigured)
        {
            StatusMessage = LangViewModel.Translate("BulletinSettingsCredentialsSaved");
            StatusBadge = BadgeType.Success;
        }
    }

    [RelayCommand]
    public async Task SaveCredentials()
    {
        if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Password))
        {
            StatusMessage = LangViewModel.Translate("BulletinSettingsCredentialsRequired");
            StatusBadge = BadgeType.Warning;
            return;
        }

        IsBusy = true;
        StatusMessage = LangViewModel.Translate("BulletinSettingsConnecting");
        StatusBadge = BadgeType.Info;
        try
        {
            var status = await Services.BulletinService.TestLoginAsync(Username, Password);
            if (status != HttpStatusCode.OK)
            {
                StatusMessage = $"{LangViewModel.Translate("BulletinSettingsConnectionFailed")} ({(int)status})";
                StatusBadge = BadgeType.Error;
                return;
            }

            await MainVm.BulletinService.SaveCredentialsAsync(Username, Password);
            IsConfigured = true;
            StatusMessage = LangViewModel.Translate("BulletinSettingsCredentialsSaved");
            StatusBadge = BadgeType.Success;

            await MainVm.BulletinViewModel.RefreshConfigurationAsync();
        }
        catch (System.Exception ex)
        {
            StatusMessage = LangViewModel.Translate("BulletinSettingsError") + " : " + ex.Message;
            StatusBadge = BadgeType.Error;
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    public async Task DeleteCredentials()
    {
        IsBusy = true;
        try
        {
            await MainVm.BulletinService.ClearCredentialsAsync();
            Username = string.Empty;
            Password = string.Empty;
            IsConfigured = false;
            StatusMessage = LangViewModel.Translate("BulletinSettingsCredentialsDeleted");
            StatusBadge = BadgeType.Neutral;
            await MainVm.BulletinViewModel.RefreshConfigurationAsync();
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    public void Back() => SettingsView.SwitchToMain();
}
