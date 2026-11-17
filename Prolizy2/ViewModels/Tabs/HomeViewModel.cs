using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Prolizy2.Model;
using Prolizy2.Model.Home;
using Prolizy2.ViewModels.Bulletin;

namespace Prolizy2.ViewModels.Tabs;

/// <summary>
/// View-model of the home tab. It hosts the user's cards (see
/// <see cref="HomeCardViewModel"/>) and keeps the legacy
/// <see cref="NextScheduleItem"/> property alive for the EDT integration.
///
/// The cards read the EDT and bulletin view-models, which are created *after*
/// this one. They are therefore built once those are available (see
/// <see cref="OnMainViewModelPropertyChanged"/>), never from the constructor.
/// </summary>
public partial class HomeViewModel : ViewModelBase
{
    private readonly MainViewModel _mainVm;
    private readonly ObservableCollection<HomeCardViewModel> _cards = new();
    private bool _built;

    public HomeViewModel(MainViewModel mainVm)
    {
        _mainVm = mainVm;

        HomeLayout.LayoutChanged += OnLayoutChanged;
        _mainVm.PropertyChanged += OnMainViewModelPropertyChanged;
    }

    /// <summary>The cards shown on the home screen, in display order.</summary>
    public ObservableCollection<HomeCardViewModel> Cards
    {
        get
        {
            EnsureBuilt();
            return _cards;
        }
    }

    /// <summary>True when no card is configured.</summary>
    public bool HasNoCards => Cards.Count == 0;

    /// <summary>
    /// Next or ongoing course. The EDT view-model pushes it whenever its week
    /// cache changes (the Android widget and the home cards read it).
    /// </summary>
    [ObservableProperty] private ScheduleItem? _nextScheduleItem;

    /// <summary>
    /// Builds the cards if it has not been done yet. Called by the
    /// <see cref="Cards"/> getter so an early binding never sees an empty list,
    /// and at the end of the view-model setup.
    /// </summary>
    private void EnsureBuilt()
    {
        if (_built)
            return;

        _built = true;
        Rebuild();
    }

    private HomeCardViewModel CreateCard(HomeCardConfig config)
    {
        var card = new HomeCardViewModel(_mainVm, config);
        card.RebuildContent();
        return card;
    }

    #region View-model wiring

    /// <summary>
    /// Hooks the view-models the cards depend on as soon as they exist, and
    /// refreshes the cards when their data changes.
    /// </summary>
    private void OnMainViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        switch (e.PropertyName)
        {
            case nameof(MainViewModel.EdtViewModel) when _mainVm.EdtViewModel is { } edt:
                // The next-course cards read the EDT cache: pull them up now
                // that it exists (the first load is still running, they refresh
                // again when it completes).
                RefreshNextCourseCards();
                break;

            case nameof(MainViewModel.BulletinViewModel) when _mainVm.BulletinViewModel is { } bulletin:
                bulletin.PropertyChanged += OnBulletinPropertyChanged;
                // The bulletin cards need the view-model to exist: build them.
                Rebuild();
                break;
        }
    }

    private void OnBulletinPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName is nameof(BulletinCoreViewModel.Data)
            or nameof(BulletinCoreViewModel.SelectedSemester))
        {
            // A refresh keeps the user's layout: only the data is recomputed.
            RefreshCards();
        }
    }

    private void OnLayoutChanged() => Rebuild();

    #endregion

    /// <summary>Rebuilds every card from <see cref="HomeLayout.Cards"/>.</summary>
    public void Rebuild()
    {
        _cards.Clear();
        foreach (var config in HomeLayout.Cards)
            _cards.Add(CreateCard(config));

        OnPropertyChanged(nameof(HasNoCards));
    }

    /// <summary>Refreshes the data of every card without recreating them.</summary>
    public void RefreshCards()
    {
        foreach (var card in _cards)
            card.RebuildContent();
    }

    /// <summary>
    /// Recomputes the "next course" cards only. Called by the EDT view-model
    /// whenever its week cache changes.
    /// </summary>
    public void RefreshNextCourseCards()
    {
        // Nothing to refresh before the first build (it picks the cache up).
        if (!_built)
            return;

        foreach (var card in _cards)
        {
            if (card.Config.Type == HomeCardType.NextCourse
                && card.Content is NextCourseCardViewModel next)
            {
                next.Refresh();
            }
        }
    }

    #region Commands

    /// <summary>Opens the page that edits the home layout.</summary>
    [RelayCommand]
    public void EditLayout()
    {
        _mainVm.NavigateTo(typeof(SettingsViewModel));
        _mainVm.SettingsViewModel.SwitchToPage(
            new Views.Tabs.Settings.HomeSettingsView
            {
                DataContext = new Settings.HomeSettingsViewModel(_mainVm, _mainVm.SettingsViewModel)
            });
    }

    [RelayCommand]
    public void NavigateToAndOpenNextItem()
    {
        if (NextScheduleItem is not null)
            OpenScheduleItem(NextScheduleItem);
    }

    /// <summary>
    /// Same navigation as <see cref="NavigateToAndOpenNextItem"/> but for any
    /// course shown by a home card (the "rest of the day" rows).
    /// </summary>
    [RelayCommand]
    public void OpenScheduleItem(ScheduleItem? item)
    {
        if (item is null)
            return;

        _mainVm.NavigateTo(typeof(EdtViewModel));
        _mainVm.EdtViewModel.CurrentDay = item.Date;
        _mainVm.EdtViewModel.LoadCurrentDayFromCache();
        item.ItemClicked();
    }

    /// <summary>Navigates to the bulletin tab, optionally selecting a given semester.</summary>
    [RelayCommand]
    public void OpenBulletin(string? semesterId)
    {
        _mainVm.NavigateTo(typeof(BulletinCoreViewModel));

        if (string.IsNullOrEmpty(semesterId))
            return;

        foreach (var semester in _mainVm.BulletinViewModel.Data.Semesters)
        {
            if (string.Equals(semester.Id, semesterId, StringComparison.Ordinal))
            {
                _mainVm.BulletinViewModel.SelectedSemester = semester;
                break;
            }
        }
    }

    #endregion
}