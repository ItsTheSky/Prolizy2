using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Prolizy2.Model.Home;
using Prolizy2.ViewModels.Bulletin;
using SukiUI.Dialogs;
using Prolizy2.Utilities;
using Prolizy2.ViewModels.Common;

namespace Prolizy2.ViewModels.Tabs.Settings;

public partial class HomeSettingsViewModel : ViewModelBase
{
    public MainViewModel MainVm { get; }
    public SettingsViewModel SettingsView { get; }

    /// <summary>Working copy of the layout, written back on save only.</summary>
    public ObservableCollection<HomeCardConfig> Cards { get; } = new();

    public ObservableCollection<HomeCardTypeOption> AvailableTypes { get; } = new();

    public ObservableCollection<SemesterChoice> Semesters { get; } = new();

    public ObservableCollection<EvaluationChoice> Groups { get; } = new();

    public ObservableCollection<UnitChoice> Units { get; } = new();

    [ObservableProperty] private bool _isDirty;

    public bool HasNoCards => Cards.Count == 0;

    public HomeSettingsViewModel(MainViewModel mainVm, SettingsViewModel settingsViewModel)
    {
        MainVm = mainVm;
        SettingsView = settingsViewModel;

        foreach (var config in HomeLayout.Cards)
            Track(config.Clone());

        foreach (var value in Enum.GetValues<HomeCardType>())
            AvailableTypes.Add(new HomeCardTypeOption(value));

        RefreshChoices();
        Cards.CollectionChanged += OnCardsChanged;

        _ = LoadBulletinAsync();
    }

    private void Track(HomeCardConfig card)
    {
        card.PropertyChanged += OnCardPropertyChanged;
        Cards.Add(card);
    }

    private void OnCardsChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.NewItems is not null)
        {
            foreach (HomeCardConfig card in e.NewItems)
            {
                card.PropertyChanged -= OnCardPropertyChanged;
                card.PropertyChanged += OnCardPropertyChanged;
            }
        }

        IsDirty = true;
        OnPropertyChanged(nameof(HasNoCards));
    }

    private void OnCardPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (sender is HomeCardConfig card && Cards.Contains(card))
            IsDirty = true;
    }

    private async Task LoadBulletinAsync()
    {
        try
        {
            var bulletin = MainVm.BulletinViewModel;

            await WaitUntilLoadedAsync(bulletin);

            if (bulletin.Data.Units.Count == 0)
                await bulletin.LoadAsync();

            RefreshChoices();
        }
        catch (Exception ex)
        {
            Logs.Error(ex);
        }
    }

    private static Task WaitUntilLoadedAsync(BulletinCoreViewModel bulletin)
    {
        if (!bulletin.IsLoading)
            return Task.CompletedTask;

        var done = new TaskCompletionSource();

        void Handler(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName != nameof(BulletinCoreViewModel.IsLoading) || bulletin.IsLoading)
                return;

            bulletin.PropertyChanged -= Handler;
            done.TrySetResult();
        }

        bulletin.PropertyChanged += Handler;
        return done.Task;
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

        Cards.Move(index, target);
    }

    [RelayCommand]
    private void Reset()
    {
        Cards.Clear();
        foreach (var card in HomeCardConfig.Defaults())
            Cards.Add(card);
    }

    #endregion

    #region Save / back

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

    private void RefreshChoices()
    {
        var data = MainVm.BulletinViewModel.Data;

        Semesters.Clear();
        Semesters.Add(new SemesterChoice(null, LangViewModel.Translate("HomeSettingsSemesterCurrent")));
        foreach (var semester in data.Semesters)
            Semesters.Add(new SemesterChoice(semester.Id, $"{semester.Code} · {semester.Year}"));

        Groups.Clear();
        foreach (var resource in data.Resources)
            Groups.Add(new EvaluationChoice(resource.Code, resource.Label));
        foreach (var sae in data.Saes)
            Groups.Add(new EvaluationChoice(sae.Code, sae.Label));

        Units.Clear();
        foreach (var unit in data.Units)
            Units.Add(new UnitChoice(unit.Code, unit.Name));
    }
}
