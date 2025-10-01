using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Prolizy2.API;
using Prolizy2.API.Model.Request;
using Prolizy2.Model;
using Prolizy2.Services;
using Prolizy2.Utilities;
using Prolizy2.Utilities.Bridges;
using Prolizy2.ViewModels.Common;
using Prolizy2.Views;
using SukiUI.Dialogs;

namespace Prolizy2.ViewModels.Tabs;

public partial class EdtViewModel : ViewModelBase
{
    public MainViewModel MainVm { get; init; }
    private readonly ScheduleCacheService _cacheService = new();
    private DateOnly _lastLoadedDay = DateOnly.MinValue;
    private int _loadingCounter = 0;

    [ObservableProperty] private bool _isLoading = false;
    [ObservableProperty] private bool _isPreloading = false;

    [ObservableProperty] private DateOnly _currentDay = DateOnly.FromDateTime(DateTime.Now);
    public string CurrentDayString => CurrentDay.ToString("dddd dd MMMM", MainVm.LangViewModel.CurrentCulture);

    [ObservableProperty] private ObservableCollection<ScheduleItem> _displayedItems = new();

    public EdtViewModel(MainViewModel mainVm)
    {
        MainVm = mainVm;
        if (Design.IsDesignMode)
            return;
        
        AttachExternalUpdaters();
        
        CurrentDay = DateOnly.FromDateTime(DateTime.Now);
        TryRefreshCurrentDay();
    }

    #region Updater

    public void AttachExternalUpdaters()
    {
        MainVm.LangViewModel.CurrentCultureChanged += _ => OnPropertyChanged(nameof(CurrentDayString));
    }

    partial void OnCurrentDayChanged(DateOnly value)
    {
        OnPropertyChanged(nameof(CurrentDayString));

        // Reset last loaded day to force refresh on day change
        if (_lastLoadedDay != value)
        {
            _lastLoadedDay = DateOnly.MinValue;
        }
    }

    #endregion
    
    public void TryRefreshCurrentDay()
    {
        if (IsLoading)
            return; // Prevent multiple refreshes at the same time

        var currentLoadId = ++_loadingCounter;
        IsLoading = true;

        RefreshCurrentDay().ContinueWith(t =>
        {
            // Only update UI if this is still the latest request
            if (currentLoadId != _loadingCounter)
                return;

            if (t.IsFaulted)
            {
                // Handle error
                Console.WriteLine(t.Exception);
            }

            IsLoading = false;
            _lastLoadedDay = CurrentDay;

            var items = DisplayedItems.ToList();
            Console.WriteLine($"Refreshing EDT for {items.Count} items.");
            AndroidBridgeProvider.Bridge?.UpdateScheduleWidget(EdtUtilities.FindNextOrCurrentItem(items));
        });
    }

    public async Task RefreshCurrentDay()
    {
        var targetDay = CurrentDay; // Capture the current day

        DisplayedItems.Clear();

        var daySchedule = await _cacheService.LoadMonthIfNeededAsync(targetDay);

        // Verify we're still on the same day before updating UI
        if (targetDay != CurrentDay)
            return;

        foreach (var item in daySchedule)
        {
            DisplayedItems.Add(item);
        }

        _lastLoadedDay = targetDay;
        PreloadAdjacentMonthsInBackground();
    }

    private async void PreloadAdjacentMonthsInBackground()
    {
        if (_isPreloading) return;

        _isPreloading = true;
        try
        {
            await _cacheService.PreloadAdjacentMonthsAsync(CurrentDay);
            _cacheService.ClearOldCache(CurrentDay);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error preloading adjacent months: {ex}");
        }
        finally
        {
            _isPreloading = false;
        }
    }

    #region Commands

    [RelayCommand]
    public void OpenCalendar()
    {
        var calendar = new Calendar
        {
            SelectedDate = CurrentDay.ToDateTime(new TimeOnly(0, 0)),
        };
        
        MainView.DialogManager.CreateDialog()
            .WithTitle(LangViewModel.Translate("EdtSelectDateTitle"))
            .WithContent(calendar)
            .WithActionButton(LangViewModel.Translate("Cancel"), _ => { }, true)
            .WithActionButton(LangViewModel.Translate("Confirm"), _ =>
            {
                var selectedDate = calendar.SelectedDate;
                if (!selectedDate.HasValue)
                    return;
                
                CurrentDay = DateOnly.FromDateTime(selectedDate.Value);
                TryRefreshCurrentDay();
            }, true)
            .TryShow();
    }
    
    [RelayCommand]
    public async void NextDay()
    {
        CurrentDay = CurrentDay.AddDays(1);
        await LoadCurrentDayFromCache();
    }

    [RelayCommand]
    public async void PreviousDay()
    {
        CurrentDay = CurrentDay.AddDays(-1);
        await LoadCurrentDayFromCache();
    }

    private async Task LoadCurrentDayFromCache()
    {
        // Prevent loading if the day hasn't changed
        if (_lastLoadedDay == CurrentDay && DisplayedItems.Any())
            return;

        var currentLoadId = ++_loadingCounter;

        if (_cacheService.IsWeekCached(CurrentDay))
        {
            // Only update if this is still the latest request
            if (currentLoadId != _loadingCounter)
                return;

            var daySchedule = _cacheService.GetDaySchedule(CurrentDay);
            DisplayedItems.Clear();
            foreach (var item in daySchedule)
            {
                DisplayedItems.Add(item);
            }

            _lastLoadedDay = CurrentDay;

            var items = DisplayedItems.ToList();
            AndroidBridgeProvider.Bridge?.UpdateScheduleWidget(EdtUtilities.FindNextOrCurrentItem(items));

            PreloadAdjacentMonthsInBackground();
        }
        else
        {
            TryRefreshCurrentDay();
        }
    }
    
    [RelayCommand]
    public async void GoToToday()
    {
        CurrentDay = DateOnly.FromDateTime(DateTime.Now);
        await LoadCurrentDayFromCache();
    }

    [RelayCommand]
    public void ClearCache()
    {
        _cacheService.ClearCache();
        _lastLoadedDay = DateOnly.MinValue;
        _loadingCounter = 0;
        TryRefreshCurrentDay();
    }
    
    #endregion
}