using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Prolizy2.Model;
using Prolizy2.Model.Bulletin;
using Prolizy2.Model.Home;
using Prolizy2.Services;
using Prolizy2.Utilities;
using Prolizy2.ViewModels.Bulletin;
using Prolizy2.ViewModels.Home;

namespace Prolizy2.ViewModels.Tabs;

/// <remarks>
/// The cards read the EDT and bulletin view-models, which are created after this
/// one: they are built once those exist (see <see cref="OnMainViewModelPropertyChanged"/>).
/// </remarks>
public partial class HomeViewModel : ViewModelBase
{
    private readonly MainViewModel _mainVm;
    private readonly ObservableCollection<HomeCardViewModel> _cards = new();
    private readonly Dictionary<string, BulletinData> _semesterSnapshots = new();
    private readonly HashSet<string> _pendingSnapshots = new();
    private BulletinCoreViewModel? _bulletin;
    private bool _built;

    public HomeViewModel(MainViewModel mainVm)
    {
        _mainVm = mainVm;

        HomeLayout.LayoutChanged += OnLayoutChanged;
        _mainVm.PropertyChanged += OnMainViewModelPropertyChanged;

        if (Design.IsDesignMode)
            return;

        // "Dans 20 min" and the ongoing course would otherwise only move when
        // the EDT cache changes.
        var clock = new DispatcherTimer { Interval = TimeSpan.FromMinutes(1) };
        clock.Tick += (_, _) => RefreshNextCourseCards();
        clock.Start();
    }

    public ObservableCollection<HomeCardViewModel> Cards
    {
        get
        {
            EnsureBuilt();
            return _cards;
        }
    }

    public bool HasNoCards => Cards.Count == 0;

    [ObservableProperty] private ScheduleItem? _nextScheduleItem;

    private void EnsureBuilt()
    {
        if (_built)
            return;

        _built = true;
        Rebuild();
    }

    #region View-model wiring

    private void OnMainViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        switch (e.PropertyName)
        {
            case nameof(MainViewModel.EdtViewModel) when _mainVm.EdtViewModel is not null:
                RefreshNextCourseCards();
                break;

            case nameof(MainViewModel.BulletinViewModel):
                if (_bulletin is not null)
                    _bulletin.PropertyChanged -= OnBulletinPropertyChanged;

                _bulletin = _mainVm.BulletinViewModel;
                _semesterSnapshots.Clear();

                if (_bulletin is not null)
                {
                    _bulletin.PropertyChanged += OnBulletinPropertyChanged;
                    Rebuild();
                }
                break;
        }
    }

    private void OnBulletinPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (_bulletin is null)
            return;

        switch (e.PropertyName)
        {
            case nameof(BulletinCoreViewModel.Data):
                _semesterSnapshots.Clear();
                if (!_bulletin.IsLoading)
                    RefreshCards();
                break;

            // A semester switch mutates Data in place once its fetch completes,
            // without raising Data: the end of the load is the reliable signal.
            case nameof(BulletinCoreViewModel.IsLoading) when !_bulletin.IsLoading:
                RefreshCards();
                break;
        }
    }

    private void OnLayoutChanged() => Rebuild();

    #endregion

    public void Rebuild()
    {
        _cards.Clear();
        foreach (var config in HomeLayout.Cards)
            _cards.Add(new HomeCardViewModel(_mainVm, config));

        OnPropertyChanged(nameof(HasNoCards));
    }

    public void RefreshCards()
    {
        foreach (var card in _cards)
            card.RebuildContent();
    }

    public void RefreshNextCourseCards()
    {
        // The first build picks the cache up by itself.
        if (!_built)
            return;

        foreach (var card in _cards)
        {
            if (card.Content is NextCourseCardViewModel next)
                next.Refresh();
        }
    }

    #region Semester snapshots

    /// <summary>
    /// The transcript of <paramref name="semester"/>: the bulletin tab data when it
    /// currently holds that semester, otherwise a separately fetched copy. Returns
    /// null while the copy is loading; the cards are refreshed once it arrives.
    /// </summary>
    public BulletinData? GetSemesterData(SemesterInfo semester)
    {
        var bulletin = _mainVm.BulletinViewModel;
        if (bulletin is null || semester.IsFuture)
            return null;

        if (!bulletin.IsLoading
            && !bulletin.HasError
            && string.Equals(bulletin.SelectedSemester?.Id, semester.Id, StringComparison.Ordinal))
        {
            return bulletin.Data;
        }

        if (_semesterSnapshots.TryGetValue(semester.Id, out var snapshot))
            return snapshot;

        if (!bulletin.IsLoading && bulletin.IsConfigured)
            _ = LoadSemesterSnapshotAsync(semester.Id);

        return null;
    }

    private async Task LoadSemesterSnapshotAsync(string semesterId)
    {
        if (!int.TryParse(semesterId, NumberStyles.Integer, CultureInfo.InvariantCulture, out var formsemestreId))
            return;

        if (!_pendingSnapshots.Add(semesterId))
            return;

        try
        {
            var root = await _mainVm.BulletinService.LoadSemesterAsync(formsemestreId);
            if (root is null)
                return;

            var data = new BulletinData();
            BulletinMapper.MapSemesterInto(root, data);

            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                _semesterSnapshots[semesterId] = data;
                RefreshCards();
            });
        }
        catch (Exception ex)
        {
            Logs.Error(ex);
        }
        finally
        {
            _pendingSnapshots.Remove(semesterId);
        }
    }

    #endregion

    #region Commands

    [RelayCommand]
    public void EditLayout()
    {
        _mainVm.NavigateTo(typeof(SettingsViewModel));
        _mainVm.SettingsViewModel.MainSettingsViewModel.GoToHome();
    }

    [RelayCommand]
    public void NavigateToAndOpenNextItem()
    {
        if (NextScheduleItem is not null)
            OpenScheduleItem(NextScheduleItem);
    }

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