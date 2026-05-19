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
    private ScheduleCacheService _cacheService = new();
    private ScheduleCacheService _secondaryCacheService = new();
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

    public bool HasSecondaryGroup => MainView.Preferences.ContainsKey("edt_secondary_group");
    public string? PrimaryGroupName => MainView.Preferences.Get<string?>("edt_group");
    public string? SecondaryGroupName => MainView.Preferences.Get<string?>("edt_secondary_group");
    
    #region Transitions

    private static readonly IPageTransition SlideLeftTransition = new DirectionalSlideTransition(
        DirectionalSlideTransition.SlideDirection.RightToLeft,
        TimeSpan.FromMilliseconds(400),
        TimeSpan.FromMilliseconds(500),
        new CubicEaseOut()
    );
    
    private static readonly IPageTransition SlideRightTransition = new DirectionalSlideTransition(
        DirectionalSlideTransition.SlideDirection.LeftToRight,
        TimeSpan.FromMilliseconds(400),
        TimeSpan.FromMilliseconds(500),
        new CubicEaseOut()
    );
    
    [ObservableProperty] private bool _isGoingRight;
    public IPageTransition DailyItemsTransition => IsGoingRight ? SlideLeftTransition : SlideRightTransition;

    partial void OnIsGoingRightChanged(bool _) => OnPropertyChanged(nameof(DailyItemsTransition));

    #endregion
    
    public EdtViewModel(MainViewModel mainVm)
    {
        MainVm = mainVm;
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
                Logs.Error(t.Exception);
            }

            IsLoading = false;
            _lastLoadedDay = CurrentDay;

            var items = CurrentDailyControl.Items.ToList();
            AndroidBridgeProvider.Bridge?.UpdateScheduleWidget(EdtUtilities.FindNextOrCurrentItem(items));
        });
    }

    public async Task RefreshCurrentDay()
    {
        var targetDay = CurrentDay; // Capture the current day

        CurrentDailyControl.Items.Clear();
        SecondaryDailyControl.Items.Clear();

        var daySchedule = await _cacheService.LoadMonthIfNeededAsync(targetDay, MainView.Preferences.Get<string>("edt_group"));

        // Verify we're still on the same day before updating UI
        if (targetDay != CurrentDay)
            return;

        CurrentDailyControl = new DailyScheduleControl
        {
            Items = new ObservableCollection<ScheduleItem>(daySchedule)
        };

        // Always create a new secondary control to avoid visual parent conflicts
        if (HasSecondaryGroup)
        {
            var secondaryGroup = MainView.Preferences.Get<string?>("edt_secondary_group");
            if (!string.IsNullOrEmpty(secondaryGroup))
            {
                var secondarySchedule = await _secondaryCacheService.LoadMonthIfNeededAsync(targetDay, secondaryGroup);
                SecondaryDailyControl = new DailyScheduleControl
                {
                    Items = new ObservableCollection<ScheduleItem>(secondarySchedule),
                    ShowHours = false
                };
            }
            else
            {
                SecondaryDailyControl = new DailyScheduleControl { Items = [] };
            }
        }
        else
        {
            // Create new empty control to detach previous one
            SecondaryDailyControl = new DailyScheduleControl { Items = [] };
        }

        OnPropertyChanged(nameof(HasSecondaryGroup));
        OnPropertyChanged(nameof(PrimaryGroupName));
        OnPropertyChanged(nameof(SecondaryGroupName));

        _lastLoadedDay = targetDay;
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
        if (_lastLoadedDay == CurrentDay && CurrentDailyControl.Items.Any())
            return;

        var currentLoadId = ++_loadingCounter;

        if (_cacheService.IsWeekCached(CurrentDay))
        {
            Logs.Debug("EDT: Week is cached, loading from cache. (Is latest request? " + (currentLoadId == _loadingCounter) + ")");
            // Only update if this is still the latest request
            if (currentLoadId != _loadingCounter)
                return;

            var daySchedule = _cacheService.GetDaySchedule(CurrentDay);
            CurrentDailyControl = new DailyScheduleControl()
            {
                Items = new ObservableCollection<ScheduleItem>(daySchedule)
            };

            // Always create a new secondary control to avoid visual parent conflicts
            if (HasSecondaryGroup)
            {
                var secondaryGroup = MainView.Preferences.Get<string?>("edt_secondary_group");
                if (!string.IsNullOrEmpty(secondaryGroup) && _secondaryCacheService.IsWeekCached(CurrentDay))
                {
                    var secondarySchedule = _secondaryCacheService.GetDaySchedule(CurrentDay);
                    SecondaryDailyControl = new DailyScheduleControl()
                    {
                        Items = new ObservableCollection<ScheduleItem>(secondarySchedule),
                        ShowHours = false
                    };
                }
                else
                {
                    SecondaryDailyControl = new DailyScheduleControl() { Items = [] };
                }
            }
            else
            {
                SecondaryDailyControl = new DailyScheduleControl() { Items = [] };
            }

            OnPropertyChanged(nameof(HasSecondaryGroup));
            OnPropertyChanged(nameof(PrimaryGroupName));
            OnPropertyChanged(nameof(SecondaryGroupName));

            _lastLoadedDay = CurrentDay;

            var items = CurrentDailyControl.Items.ToList();
            AndroidBridgeProvider.Bridge?.UpdateScheduleWidget(EdtUtilities.FindNextOrCurrentItem(items));
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
        _cacheService = new ScheduleCacheService();
        _secondaryCacheService = new ScheduleCacheService();
        _lastLoadedDay = DateOnly.MinValue;
        _loadingCounter = 0;
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