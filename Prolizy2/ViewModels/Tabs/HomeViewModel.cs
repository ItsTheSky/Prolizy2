using System;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Prolizy2.Model;
using Prolizy2.Model.Home;
using Prolizy2.Utilities;
using Prolizy2.ViewModels.Bulletin;

namespace Prolizy2.ViewModels.Tabs;

/// <summary>
/// View-model of the home tab. It hosts the user's cards (see
/// <see cref="HomeCardViewModel"/>) and keeps the legacy
/// <see cref="NextScheduleItem"/> property alive for the EDT integration.
/// </summary>
public partial class HomeViewModel : ViewModelBase
{
    private readonly MainViewModel _mainVm;

    public HomeViewModel(MainViewModel mainVm)
    {
        _mainVm = mainVm;

        HomeLayout.LayoutChanged += OnLayoutChanged;
        Rebuild();
        _mainVm.BulletinViewModel.PropertyChanged += OnBulletinPropertyChanged;

        // The EDT pushes the next course here whenever its cache changes.
        _mainVm.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName != nameof(MainViewModel.HomeViewModel))
                return;

            foreach (var card in Cards)
            {
                if (card.Config.Type == HomeCardType.NextCourse
                    && card.Content is NextCourseCardViewModel next)
                {
                    next.Refresh();
                }
            }
        };
    }

    /// <summary>The cards shown on the home screen, in display order.</summary>
    public ObservableCollection<HomeCardViewModel> Cards { get; } = new();

    /// <summary>True when no card is configured.</summary>
    public bool HasNoCards => Cards.Count == 0;

    /// <summary>
    /// Next or ongoing course. The EDT view-model pushes it whenever its week
    /// cache changes (the Android widget and the home cards read it).
    /// </summary>
    [ObservableProperty] private ScheduleItem? _nextScheduleItem;

    private HomeCardViewModel CreateCard(HomeCardConfig config)
    {
        var card = new HomeCardViewModel(_mainVm, config);
        card.RebuildContent();
        return card;
    }

    private void OnLayoutChanged() => Rebuild();

    private void OnBulletinPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName is nameof(BulletinCoreViewModel.Data)
            or nameof(BulletinCoreViewModel.SelectedSemester))
        {
            // A refresh keeps the user's layout: only the data is recomputed.
            RefreshCards();
        }
    }

    /// <summary>Rebuilds every card from <see cref="HomeLayout.Cards"/>.</summary>
    public void Rebuild()
    {
        Cards.Clear();
        foreach (var config in HomeLayout.Cards)
            Cards.Add(CreateCard(config));

        OnPropertyChanged(nameof(HasNoCards));
    }

    /// <summary>Refreshes the data of every card without recreating them.</summary>
    private void RefreshCards()
    {
        foreach (var card in Cards)
            card.RebuildContent();
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