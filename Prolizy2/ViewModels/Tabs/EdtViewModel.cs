using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Prolizy2.API;
using Prolizy2.API.Model.Request;
using Prolizy2.Model;
using Prolizy2.Utilities;
using Prolizy2.ViewModels.Common;
using Prolizy2.Views;
using SukiUI.Dialogs;

namespace Prolizy2.ViewModels.Tabs;

public partial class EdtViewModel : ViewModelBase
{
    public MainViewModel MainVm { get; init; }
    
    [ObservableProperty] private bool _isLoading = false;
    
    [ObservableProperty] private DateOnly _currentDay = DateOnly.FromDateTime(DateTime.Now);
    public string CurrentDayString => CurrentDay.ToString("dddd dd MMMM", MainVm.LangViewModel.CurrentCulture);
    
    [ObservableProperty] private ObservableCollection<ScheduleItem> _displayedItems = new();

    public EdtViewModel(MainViewModel mainVm)
    {
        MainVm = mainVm;
        if (Design.IsDesignMode)
            return;
        
        AttachExternalUpdaters();
        
        CurrentDay = DateOnly.FromDateTime(DateTime.Now.AddDays(2));
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
    }

    #endregion
    
    public void TryRefreshCurrentDay()
    {
        if (IsLoading)
            return; // Prevent multiple refreshes at the same time
        
        IsLoading = true;
        RefreshCurrentDay().ContinueWith(t =>
        {
            if (t.IsFaulted)
            {
                // Handle error
                Console.WriteLine(t.Exception);
            }
            else
            {
                IsLoading = false;
            }
        });
    }

    public async Task RefreshCurrentDay()
    {
        DisplayedItems.Clear();

        var courses = await EDTClient.GetCourses(new CalendarRequest
        {
            FederationId = "INF1-B",
            StartDate = CurrentDay,
            EndDate = CurrentDay,
            ViewType = CalendarRequest.CalendarViewType.AgendaWeek
        });

        foreach (var course in courses)
        {
            var description = await course.GetDescription(true);

            var baseColor = ColorMatcher.FindClosestColor(course.BackgroundColor);
            var lighterColor = ColorMatcher.MakeLighter(baseColor, 0.2);
            var backgroundBrush = new LinearGradientBrush()
            {
                GradientStops =
                [
                    new GradientStop(baseColor, 0),
                    new GradientStop(lighterColor, 1)
                ]
            };
            
            DisplayedItems.Add(new ScheduleItem
            {
                StartTime = course.Start,
                EndTime = course.End,
                Group = string.Join(", ", description.Groups),
                Subject = string.Join(", ", description.Subjects),
                Room = string.Join(", ", description.Rooms),
                Professor = string.Join(", ", description.Professors),
                BackgroundColor = backgroundBrush,
                
                ForegroundColor = new SolidColorBrush(Colors.White),
                BorderColor = new SolidColorBrush(Colors.Gray),
                Course = course,
                Type = course.CourseType
            });
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
    public void NextDay()
    {
        CurrentDay = CurrentDay.AddDays(1);
        TryRefreshCurrentDay();
    }
    
    [RelayCommand]
    public void PreviousDay()
    {
        CurrentDay = CurrentDay.AddDays(-1);
        TryRefreshCurrentDay();
    }
    
    [RelayCommand]
    public void GoToToday()
    {
        CurrentDay = DateOnly.FromDateTime(DateTime.Now);
        TryRefreshCurrentDay();
    }
    
    #endregion
}