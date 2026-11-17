using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Prolizy2.Model.Home;
using SukiUI.Dialogs;
using Prolizy2.Utilities;
using Prolizy2.ViewModels.Common;

namespace Prolizy2.ViewModels.Tabs.Settings;

/// <summary>
/// Settings sub-page that edits the home layout: add / remove / reorder cards and
/// tune the options of each of them (width, style, resource, semester, rows).
/// </summary>
public partial class HomeSettingsViewModel : ViewModelBase
{
    public MainViewModel MainVm { get; }
    public SettingsViewModel SettingsView { get; }

    /// <summary>The cards being edited — a working copy until the user saves.</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasNoCards))]
    private ObservableCollection<HomeCardConfig> _cards = new();

    /// <summary>Cards offered by the "add a card" list.</summary>
    public ObservableCollection<HomeCardTypeOption> AvailableTypes { get; } = new();

    /// <summary>Semesters offered by the bulletin overview card.</summary>
    public ObservableCollection<SemesterChoice> Semesters { get; } = new();

    /// <summary>Resources and SAÉs offered by the evaluation card.</summary>
    public ObservableCollection<EvaluationChoice> Groups { get; } = new();

    /// <summary>Teaching units offered by the unit-list card.</summary>
    public ObservableCollection<UnitChoice> Units { get; } = new();

    [ObservableProperty] private bool _isDirty;

    public bool HasNoCards => Cards.Count == 0;

    public HomeSettingsViewModel(MainViewModel mainVm, SettingsViewModel settingsViewModel)
    {
        MainVm = mainVm;
        SettingsView = settingsViewModel;

        foreach (var config in HomeLayout.Cards)
            Cards.Add(config.Clone());

        RefreshChoices();
        Cards.CollectionChanged += (_, _) =>
        {
            IsDirty = true;
            OnPropertyChanged(nameof(HasNoCards));
        };

        // The bulletin may still be loading when the page opens (first launch,
        // or credentials just saved): pull it and rebuild the pickers.
        _ = LoadBulletinAsync();
    }

    private async Task LoadBulletinAsync()
    {
        try
        {
            if (!MainVm.BulletinViewModel.IsConfigured)
                await MainVm.BulletinViewModel.LoadAsync();

            if (MainVm.BulletinViewModel.Data.Units.Count == 0)
                return;

            RefreshChoices();
        }
        catch (Exception ex)
        {
            Logs.Error(ex);
        }
    }

    #region Card list commands

    [RelayCommand]
    private void AddCard(HomeCardType? type)
    {
        Cards.Add(new HomeCardConfig { Type = type ?? HomeCardType.NextCourse });
    }

    [RelayCommand]
    private void RemoveCard(HomeCardConfig? card)
    {
        if (card is not null)
            Cards.Remove(card);
    }

    [RelayCommand]
    private void MoveUp(HomeCardConfig? card) => Move(card, -1);

    [RelayCommand]
    private void MoveDown(HomeCardConfig? card) => Move(card, 1);

    private void Move(HomeCardConfig? card, int delta)
    {
        if (card is null)
            return;

        var index = Cards.IndexOf(card);
        var target = index + delta;
        if (index < 0 || target < 0 || target >= Cards.Count)
            return;

        Cards.RemoveAt(index);
        Cards.Insert(target, card);
    }

    /// <summary>Restores the default layout in the editor.</summary>
    [RelayCommand]
    private void Reset()
    {
        Cards.Clear();
        foreach (var card in HomeCardConfig.Defaults())
            Cards.Add(card);
    }

    #endregion

    #region Save / back

    /// <summary>Writes the edited layout back to the shared home layout.</summary>
    [RelayCommand]
    private void Save()
    {
        HomeLayout.ReplaceAll(Cards);
        IsDirty = false;
        SettingsView.SwitchToMain();
    }

    [RelayCommand]
    private void Back()
    {
        if (!IsDirty)
        {
            SettingsView.SwitchToMain();
            return;
        }

        Views.MainView.DialogManager.CreateDialog()
            .WithTitle(LangViewModel.Translate("HomeSettingsDiscardTitle"))
            .WithContent(LangViewModel.Translate("HomeSettingsDiscardMessage"))
            .WithActionButton(LangViewModel.Translate("HomeSettingsDiscardKeepEditing"), _ => { }, true)
            .WithActionButton(LangViewModel.Translate("HomeSettingsDiscardLeave"), _ => SettingsView.SwitchToMain(), true)
            .TryShow();
    }

    #endregion

    /// <summary>Re-reads the pickers from the (possibly refreshed) bulletin data.</summary>
    public void ReloadChoices() => RefreshChoices();

    private void RefreshChoices()
    {
        AvailableTypes.Clear();
        foreach (var value in Enum.GetValues<HomeCardType>())
            AvailableTypes.Add(new HomeCardTypeOption(value));

        Semesters.Clear();
        Semesters.Add(new SemesterChoice(null, LangViewModel.Translate("HomeSettingsSemesterCurrent")));
        foreach (var semester in MainVm.BulletinViewModel.Data.Semesters)
            Semesters.Add(new SemesterChoice(semester.Id, $"{semester.Code} · {semester.Year}"));

        Groups.Clear();
        foreach (var resource in MainVm.BulletinViewModel.Data.Resources)
            Groups.Add(new EvaluationChoice(resource.Code, resource.Label, resource.Kind.ToString()));
        foreach (var sae in MainVm.BulletinViewModel.Data.Saes)
            Groups.Add(new EvaluationChoice(sae.Code, sae.Label, sae.Kind.ToString()));

        Units.Clear();
        foreach (var unit in MainVm.BulletinViewModel.Data.Units)
            Units.Add(new UnitChoice(unit.Code, unit.Name));
    }
}