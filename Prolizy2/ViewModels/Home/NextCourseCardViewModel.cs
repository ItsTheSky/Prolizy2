using System;
using System.Collections.ObjectModel;
using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Prolizy2.Model;
using Prolizy2.Utilities;
using Prolizy2.ViewModels.Common;

namespace Prolizy2.ViewModels.Home;

public partial class NextCourseCardViewModel : ViewModelBase
{
    private readonly MainViewModel _mainVm;

    public NextCourseCardViewModel(MainViewModel mainVm, bool compact = false)
    {
        _mainVm = mainVm;
        Compact = compact;

        if (mainVm is null)
            return; // design-time instance

        Refresh();
    }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasCourse))]
    [NotifyPropertyChangedFor(nameof(HasNothingPlanned))]
    [NotifyPropertyChangedFor(nameof(ShowDetails))]
    private ScheduleItem? _nextItem;

    [ObservableProperty] private ObservableCollection<ScheduleItem> _followingItems = new();

    [ObservableProperty] private string _statusText = string.Empty;

    public bool Compact { get; }

    public bool ShowDetails => !Compact && HasCourse;

    public bool ShowFollowing => !Compact;

    public bool HasCourse => NextItem is not null;

    public bool HasNothingPlanned => NextItem is null;

    [RelayCommand]
    private void OpenScheduleItem(ScheduleItem? item)
    {
        if (item is null)
            return;

        _mainVm.HomeViewModel.OpenScheduleItem(item);
    }

    public void Refresh()
    {
        // The EDT view-model is created after the home one.
        if (_mainVm?.EdtViewModel is not { } edt)
            return;

        var cache = edt.GetCachedItems();

        var next = EdtUtilities.FindNextOrCurrentItem(cache);
        var following = EdtUtilities.FindFollowingItems(cache, next, 3);

        // Same instance kept so the ItemsControl is not rebuilt for nothing.
        FollowingItems.Clear();
        foreach (var item in following)
            FollowingItems.Add(item);

        NextItem = next;
        StatusText = DescribeStatus(next);
    }

    private static bool IsOngoing(ScheduleItem item)
    {
        var now = DateTime.Now;
        return item.StartTime <= now && item.EndTime >= now;
    }

    private static string DescribeStatus(ScheduleItem? item)
    {
        if (item is null)
            return string.Empty;

        if (IsOngoing(item))
            return LangViewModel.Translate("HomeCardStatusOngoing");

        var now = DateTime.Now;
        var today = DateOnly.FromDateTime(now);

        if (item.Date == today)
        {
            var minutes = (int)Math.Round((item.StartTime - now).TotalMinutes);
            if (minutes <= 0)
                minutes = 1;

            return minutes switch
            {
                < 60 => string.Format(CultureInfo.CurrentCulture,
                    LangViewModel.Translate("HomeCardStatusInMinutes"), minutes),
                _ => string.Format(CultureInfo.CurrentCulture,
                    LangViewModel.Translate("HomeCardStatusInHours"),
                    minutes / 60, minutes % 60)
            };
        }

        if (item.Date == today.AddDays(1))
            return LangViewModel.Translate("HomeCardStatusTomorrow");

        var day = item.FormattedDate;
        return string.IsNullOrEmpty(day) ? string.Empty : char.ToUpper(day[0]) + day[1..];
    }
}
