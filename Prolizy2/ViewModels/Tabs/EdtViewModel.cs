using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Animation;
using Avalonia.Animation.Easings;
using Avalonia.Controls;
using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Prolizy2.API;
using Prolizy2.API.Model.Request;
using Prolizy2.Model;
using Prolizy2.Services;
using Prolizy2.Styles.Controls.Edt;
using Prolizy2.Utilities;
using Prolizy2.Utilities.Bridges;
using Prolizy2.ViewModels.Common;
using Prolizy2.ViewModels.Tabs.Settings;
using Prolizy2.Views;
using Prolizy2.Views.Tabs.Settings;
using SukiUI.Dialogs;

namespace Prolizy2.ViewModels.Tabs;

public partial class EdtViewModel : ViewModelBase
{
    public MainViewModel MainVm { get; init; }
    private ScheduleCacheService _cacheService;
    private ScheduleCacheService _secondaryCacheService;
    private DateOnly _lastLoadedDay = DateOnly.MinValue;
    private int _loadingCounter = 0;

    [ObservableProperty] private bool _isLoading = false;
    [ObservableProperty] private bool _isPreloading = false;
    [ObservableProperty] private bool _isConfigured = false;

    [ObservableProperty] private DateOnly _currentDay = DateOnly.FromDateTime(DateTime.Now);
    public string CurrentDayString
    {
        get
        {
            var dayString = CurrentDay.ToString("dddd dd MMMM", MainVm.LangViewModel.CurrentCulture);
            return char.ToUpper(dayString[0]) + dayString[1..];
        }
    }

    [ObservableProperty] private DailyScheduleControl _currentDailyControl = new() { Items = [] };
    [ObservableProperty] private DailyScheduleControl _secondaryDailyControl = new() { Items = [] };
    [ObservableProperty] private DailyScheduleControl _singleViewDailyControl = new() { Items = [] };

    public bool HasSecondaryGroup => MainView.Preferences.ContainsKey("edt_secondary_group");
    public string? PrimaryGroupName => MainView.Preferences.Get<string?>("edt_group");
    public string? SecondaryGroupName => MainView.Preferences.Get<string?>("edt_secondary_group");
    
    #region Transitions

    private static readonly IPageTransition SlideLeftTransition = new DirectionalSlideTransition(
        DirectionalSlideTransition.SlideDirection.RightToLeft,
        TimeSpan.FromMilliseconds(200),
        TimeSpan.FromMilliseconds(200),
        new CubicEaseOut()
    );
    
    private static readonly IPageTransition SlideRightTransition = new DirectionalSlideTransition(
        DirectionalSlideTransition.SlideDirection.LeftToRight,
        TimeSpan.FromMilliseconds(200),
        TimeSpan.FromMilliseconds(200),
        new CubicEaseOut()
    );
    
    [ObservableProperty] private bool _isGoingRight;
    public IPageTransition DailyItemsTransition => IsGoingRight ? SlideLeftTransition : SlideRightTransition;

    partial void OnIsGoingRightChanged(bool _) => OnPropertyChanged(nameof(DailyItemsTransition));

    #endregion
    
    public EdtViewModel(MainViewModel mainVm)
    {
        MainVm = mainVm;
        _cacheService = CreateCacheService();
        _secondaryCacheService = CreateCacheService();
        if (Design.IsDesignMode)
            return;

        IsConfigured = MainView.Preferences.ContainsKey("edt_group");
        Paths.SetResourceKey("ProlizyHasItemCardData", MainView.Preferences.Get("edt_course_data_in_card", true));
        
        AttachExternalUpdaters();
        
        CurrentDay = DateOnly.FromDateTime(DateTime.Now);
        if (IsConfigured) TryRefreshCurrentDay();
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
    
    private ScheduleCacheService CreateCacheService()
    {
        var service = new ScheduleCacheService();
        service.WeekUpdated += weekStart => OnWeekUpdated(service, weekStart);
        return service;
    }

    /// <summary>
    /// A week first shown from the disk cache changed on the server: refresh
    /// what is on screen in place (no page transition) if it is affected.
    /// </summary>
    private void OnWeekUpdated(ScheduleCacheService source, DateOnly weekStart)
    {
        if (!ReferenceEquals(source, _cacheService) && !ReferenceEquals(source, _secondaryCacheService))
            return; // stale service, replaced by ClearCache

        UpdateHomeNextCourse();

        if (CurrentDay < weekStart || CurrentDay > weekStart.AddDays(6))
            return;

        if (HasSecondaryGroup)
        {
            if (ReferenceEquals(source, _cacheService))
                CurrentDailyControl.Items = new ObservableCollection<ScheduleItem>(_cacheService.GetDaySchedule(CurrentDay));
            else
                SecondaryDailyControl.Items = new ObservableCollection<ScheduleItem>(_secondaryCacheService.GetDaySchedule(CurrentDay));
        }
        else if (ReferenceEquals(source, _cacheService))
        {
            SingleViewDailyControl.Items = new ObservableCollection<ScheduleItem>(_cacheService.GetDaySchedule(CurrentDay));
        }

        UpdateWidgetFromDisplayedDay();
    }

    /// <summary>Pushes the next course of the displayed day to the home-screen widget, off the UI thread.</summary>
    private void UpdateWidgetFromDisplayedDay()
    {
        var items = HasSecondaryGroup ? CurrentDailyControl.Items.ToList() : SingleViewDailyControl.Items.ToList();
        UpdateWidget(EdtUtilities.FindNextOrCurrentItem(items));
    }

    private static void UpdateWidget(ScheduleItem? item)
    {
        var bridge = AndroidBridgeProvider.Bridge;
        if (bridge is null)
            return;

        // RemoteViews updates are binder calls: no reason to make a tap wait for them.
        Task.Run(() => bridge.UpdateScheduleWidget(item));
    }

    public async void TryRefreshCurrentDay()
    {
        // Every request gets an id; only the latest one may update the loading
        // state. A newer tap is never dropped because an older load is running.
        var currentLoadId = ++_loadingCounter;
        IsLoading = true;

        try
        {
            await RefreshCurrentDay();
        }
        catch (Exception ex)
        {
            Logs.Error(ex);
        }

        if (currentLoadId != _loadingCounter)
            return;

        IsLoading = false;
        UpdateWidgetFromDisplayedDay();
    }

    public async Task RefreshCurrentDay()
    {
        var targetDay = CurrentDay; // Capture the current day

        var daySchedule = await _cacheService.LoadMonthIfNeededAsync(targetDay, MainView.Preferences.Get<string>("edt_group"));

        // Verify we're still on the same day before updating UI
        if (targetDay != CurrentDay)
            return;

        // Load secondary group if configured
        if (HasSecondaryGroup)
        {
            var secondaryGroup = MainView.Preferences.Get<string?>("edt_secondary_group");
            if (!string.IsNullOrEmpty(secondaryGroup))
            {
                var secondarySchedule = await _secondaryCacheService.LoadMonthIfNeededAsync(targetDay, secondaryGroup);
                if (targetDay != CurrentDay)
                    return;

                // Use separate controls for dual-column view
                CurrentDailyControl = new DailyScheduleControl
                {
                    Items = new ObservableCollection<ScheduleItem>(daySchedule)
                };
                SecondaryDailyControl = new DailyScheduleControl
                {
                    Items = new ObservableCollection<ScheduleItem>(secondarySchedule)
                };
            }
        }
        else
        {
            // Use separate control for single-column view
            SingleViewDailyControl = new DailyScheduleControl
            {
                Items = new ObservableCollection<ScheduleItem>(daySchedule)
            };
        }

        OnPropertyChanged(nameof(HasSecondaryGroup));
        OnPropertyChanged(nameof(PrimaryGroupName));
        OnPropertyChanged(nameof(SecondaryGroupName));

        _lastLoadedDay = targetDay;

        // The home card only needs the current week: do not make it wait for
        // the adjacent months to finish downloading.
        UpdateHomeNextCourse();
        PreloadAdjacentMonthsInBackground();
    }

    private async void PreloadAdjacentMonthsInBackground()
    {
        if (IsPreloading) return;

        IsPreloading = true;
        try
        {
            await _cacheService.PreloadAdjacentMonthsAsync(CurrentDay, MainView.Preferences.Get<string>("edt_group"));
            _cacheService.ClearOldCache(CurrentDay);

            // Preload secondary group if configured
            if (HasSecondaryGroup)
            {
                var secondaryGroup = MainView.Preferences.Get<string?>("edt_secondary_group");
                if (!string.IsNullOrEmpty(secondaryGroup))
                {
                    await _secondaryCacheService.PreloadAdjacentMonthsAsync(CurrentDay, secondaryGroup);
                    _secondaryCacheService.ClearOldCache(CurrentDay);
                }
            }

            UpdateHomeNextCourse();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error preloading adjacent months: {ex}");
        }
        finally
        {
            IsPreloading = false;
        }
    }

    #region Cache access

    /// <summary>
    /// The cached weeks, flattened and sorted by start time. Used by the home
    /// cards, which must not own a second copy of the week cache.
    /// </summary>
    public List<ScheduleItem> GetCachedItems() => _cacheService.GetCachedItems();

    #endregion

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
                IsGoingRight = CurrentDay >= DateOnly.FromDateTime(DateTime.Now);
                TryRefreshCurrentDay();
            }, true)
            .TryShow();
    }
    
    [RelayCommand]
    public async Task NextDay()
    {
        CurrentDay = CurrentDay.AddDays(1);
        IsGoingRight = true;
        LoadCurrentDayFromCache();
    }

    [RelayCommand]
    public async Task PreviousDay()
    {
        CurrentDay = CurrentDay.AddDays(-1);
        IsGoingRight = false;
        LoadCurrentDayFromCache();
    }

    public void LoadCurrentDayFromCache()
    {
        // Prevent loading if the day hasn't changed
        if (_lastLoadedDay == CurrentDay && (HasSecondaryGroup ? CurrentDailyControl.Items.Any() : SingleViewDailyControl.Items.Any()))
            return;

        var currentLoadId = ++_loadingCounter;

        if (_cacheService.IsWeekCached(CurrentDay))
        {
            Logs.Debug("EDT: Week is cached, loading from cache. (Is latest request? " + (currentLoadId == _loadingCounter) + ")");
            // Only update if this is still the latest request
            if (currentLoadId != _loadingCounter)
                return;

            var daySchedule = _cacheService.GetDaySchedule(CurrentDay);

            // Load secondary group from cache if configured
            if (HasSecondaryGroup)
            {
                var secondaryGroup = MainView.Preferences.Get<string?>("edt_secondary_group");
                if (!string.IsNullOrEmpty(secondaryGroup) && _secondaryCacheService.IsWeekCached(CurrentDay))
                {
                    var secondarySchedule = _secondaryCacheService.GetDaySchedule(CurrentDay);

                    // Use separate controls for dual-column view
                    CurrentDailyControl = new DailyScheduleControl()
                    {
                        Items = new ObservableCollection<ScheduleItem>(daySchedule)
                    };
                    SecondaryDailyControl = new DailyScheduleControl()
                    {
                        Items = new ObservableCollection<ScheduleItem>(secondarySchedule)
                    };
                }
            }
            else
            {
                // Use separate control for single-column view
                SingleViewDailyControl = new DailyScheduleControl()
                {
                    Items = new ObservableCollection<ScheduleItem>(daySchedule)
                };
            }

            OnPropertyChanged(nameof(HasSecondaryGroup));
            OnPropertyChanged(nameof(PrimaryGroupName));
            OnPropertyChanged(nameof(SecondaryGroupName));

            _lastLoadedDay = CurrentDay;

            // This request is now the latest one and it is already satisfied:
            // an older network load must not leave the spinner on.
            IsLoading = false;

            var items = daySchedule.ToList();
            UpdateWidget(EdtUtilities.FindNextOrCurrentItem(items));
            Logs.Debug("EDT: from week cache, loaded " + items.Count + " items.");

            PreloadAdjacentMonthsInBackground();
        }
        else
        {
            Logs.Debug("EDT: Week is NOT cached, fetching from network. (Is latest request? " + (currentLoadId == _loadingCounter) + ")");
            TryRefreshCurrentDay();
        }

        UpdateHomeNextCourse();
    }
    
    [RelayCommand]
    public async Task GoToToday()
    {
        IsGoingRight = CurrentDay <= DateOnly.FromDateTime(DateTime.Now);
        CurrentDay = DateOnly.FromDateTime(DateTime.Now);
        LoadCurrentDayFromCache();
    }

    [RelayCommand]
    public void ClearCache()
    {
        _cacheService = CreateCacheService();
        _secondaryCacheService = CreateCacheService();
        _lastLoadedDay = DateOnly.MinValue;
        _loadingCounter++; // invalidate in-flight loads without reusing their ids
    }


    [RelayCommand]
    public void OpenSettings()
    {
        MainVm.NavigateTo(typeof(SettingsViewModel));
        MainVm.SettingsViewModel.SwitchToPage(new EdtSettingsView
        {
            DataContext = new EdtSettingsViewModel(MainVm, MainVm.SettingsViewModel)
        });
    }

    public void UpdateHomeNextCourse()
    {
        var cache = _cacheService.GetCachedItems();
        MainVm.HomeViewModel.NextScheduleItem = EdtUtilities.FindNextOrCurrentItem(cache);
        Logs.Debug("EDT: Updated home next course: " + (MainVm.HomeViewModel.NextScheduleItem?.Subject ?? "null") + $"Got {cache.Count} items in cache.");
    }
    
    #endregion
}