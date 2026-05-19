using CommunityToolkit.Mvvm.Input;
using Prolizy2.ViewModels.Bulletin;

namespace Prolizy2.ViewModels.Tabs.Settings;

/// <summary>
/// Settings sub-page that edits the shared <see cref="BulletinDisplayOptions"/>.
/// </summary>
public partial class BulletinSettingsViewModel : ViewModelBase
{
    public SettingsViewModel SettingsView { get; }

    /// <summary>The shared display options — bound directly by the view.</summary>
    public BulletinDisplayOptions Options => BulletinDisplayOptions.Instance;

    public BulletinSettingsViewModel(MainViewModel mainVm, SettingsViewModel settingsViewModel)
    {
        SettingsView = settingsViewModel;
    }

    [RelayCommand]
    public void Back() => SettingsView.SwitchToMain();
}
