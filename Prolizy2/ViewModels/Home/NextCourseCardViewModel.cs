using System;
using System.Collections.ObjectModel;
using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Prolizy2.Model;
using Prolizy2.Utilities;
using Prolizy2.ViewModels.Common;

namespace Prolizy2.ViewModels.Tabs;

/// <summary>
/// Card showing the next (or ongoing) course, with the rest of the day below.
/// It replaces the single "Prochain cours" block of the first home screen.
/// </summary>
public partial class NextCourseCardViewModel : ViewModelBase
{
    private readonly MainViewModel _mainVm;

    public NextCourseCardViewModel(MainViewModel mainVm, bool compact = false)
    {
        _mainVm = mainVm;
        Compact = compact;

        if (mainVm is null)
            return; // design-time instance

        // The EDT pushes the next course whenever its week cache changes.
        mainVm.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(MainViewModel.HomeViewModel))
                Refresh();
        };

        Refresh();
    }

    /// <summary>The course that is ongoing or coming next; null when nothing is scheduled.</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasCourse))]
    [NotifyPropertyChangedFor(nameof(HasNothingPlanned))]
    [NotifyPropertyChangedFor(nameof(ShowDetails))]
    private ScheduleItem? _nextItem;

    /// <summary>The courses following <see cref="NextItem"/> (rest of the day, then later days).</summary>
    [ObservableProperty] private ObservableCollection<ScheduleItem> _followingItems = new();

    /// <summary>True when the card is rendered in its compact variant (no detail rows).</summary>
    public bool Compact { get; }

    /// <summary>Detail chips are hidden in the compact variant.</summary>
    public bool ShowDetails => !Compact && HasCourse;

    /// <summary>The "rest of the day" list is hidden in the compact variant.</summary>
    public bool ShowFollowing => !Compact;

    public bool HasCourse => NextItem is not null;

    public bool HasNothingPlanned => NextItem is null;

    /// <summary>Status line: "En cours" / "Dans 1 h 20" / "Demain".</summary>
    public string StatusText => DescribeStatus(NextItem);

    /// <summary>Navigates to the EDT tab and opens the given course.</summary>
    [RelayCommand]
    private void OpenScheduleItem(ScheduleItem? item)
    {
        if (item is null)
            return;

        _mainVm.HomeViewModel.OpenScheduleItem(item);
    }

    /// <summary>Recomputes the card from the EDT cache.</summary>
    public void Refresh()
    {
        // The EDT view-model owns the only up-to-date week cache. It is created
        // after the home view-model, so it may not exist yet.
        if (_mainVm?.EdtViewModel is not { } edt)
            return;

        var cache = edt.GetCachedItems();

        var next = EdtUtilities.FindNextOrCurrentItem(cache);
        var following = EdtUtilities.FindFollowingItems(cache, next, 3);

        // Keep the instance stable so the ItemsControl is not rebuilt for nothing.
        FollowingItems.Clear();
        foreach (var item in following)
            FollowingItems.Add(item);

        NextItem = next;
        OnPropertyChanged(nameof(StatusText));
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
        return char.ToUpper(day[0]) + day[1..];
    }
}