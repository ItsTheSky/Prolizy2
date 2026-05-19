using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using Avalonia.Animation;
using Avalonia.Animation.Easings;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Prolizy2.Model.Bulletin;
using Prolizy2.Utilities;
using Prolizy2.Views;
using Prolizy2.Views.Bulletin;
using SukiUI.Dialogs;

namespace Prolizy2.ViewModels.Bulletin;

/// <summary>
/// Root view model for the bulletin tab. Owns the bulletin data, the shared
/// display options and — for the drill-down variant — the navigation stack.
/// </summary>
public partial class BulletinCoreViewModel : ViewModelBase
{
    public MainViewModel MainVm { get; }

    /// <summary>Shared, persisted display options driving every variant.</summary>
    public BulletinDisplayOptions Options => BulletinDisplayOptions.Instance;

    [ObservableProperty] private BulletinData _data;

    /// <summary>Teaching units in the order requested by <see cref="Options"/>.</summary>
    [ObservableProperty] private ObservableCollection<TeachingUnitInfo> _sortedUnits = new();

    [ObservableProperty] private SemesterInfo? _selectedSemester;

    #region Drill-down navigation

    private readonly System.Collections.Generic.Stack<ViewModelBase> _navigationStack = new();

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasDetail))]
    private ViewModelBase? _currentDetail;

    public bool HasDetail => CurrentDetail is not null;

    private static readonly IPageTransition ForwardTransition = new DirectionalSlideTransition(
        DirectionalSlideTransition.SlideDirection.RightToLeft,
        TimeSpan.FromMilliseconds(380),
        TimeSpan.FromMilliseconds(480),
        new CubicEaseOut());

    private static readonly IPageTransition BackwardTransition = new DirectionalSlideTransition(
        DirectionalSlideTransition.SlideDirection.LeftToRight,
        TimeSpan.FromMilliseconds(380),
        TimeSpan.FromMilliseconds(480),
        new CubicEaseOut());

    [ObservableProperty] private IPageTransition _detailTransition = ForwardTransition;

    #endregion

    public BulletinCoreViewModel(MainViewModel mainVm)
    {
        MainVm = mainVm;
        _data = BulletinSampleData.Create();

        SelectedSemester = Data.Semesters.FirstOrDefault(s => s.IsCurrent)
                           ?? Data.Semesters.FirstOrDefault(s => !s.IsFuture);

        RefreshSortedUnits();
        Options.PropertyChanged += OnOptionsChanged;
    }

    private void OnOptionsChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(BulletinDisplayOptions.SortMode))
            RefreshSortedUnits();
    }

    private void RefreshSortedUnits()
    {
        var ordered = Options.SortMode == UnitSortMode.ByAverage
            ? Data.Units.OrderByDescending(u => u.Average.NumericValue ?? double.MinValue)
            : Data.Units.OrderBy(u => u.Code, StringComparer.OrdinalIgnoreCase);

        SortedUnits = new ObservableCollection<TeachingUnitInfo>(ordered);
    }

    #region Shared commands

    [RelayCommand]
    private void OpenStudentInfo()
    {
        MainView.DialogManager.CreateDialog()
            .WithTitle("Informations étudiant")
            .WithContent(new StudentInfoView { DataContext = Data.Student })
            .WithActionButton("Fermer", _ => { }, true)
            .TryShow();
    }

    #endregion

    #region Drill-down commands

    [RelayCommand]
    private void OpenUnit(TeachingUnitInfo? unit)
    {
        if (unit is not null)
            PushDetail(new UnitDetailViewModel(this, unit));
    }

    [RelayCommand]
    private void OpenAllResources()
    {
        PushDetail(new EvaluationListViewModel(
            this, "Ressources",
            $"{Data.Resources.Count} ressources",
            Data.Resources, BulletinEntryKind.Resource));
    }

    [RelayCommand]
    private void OpenAllSaes()
    {
        PushDetail(new EvaluationListViewModel(
            this, "SAÉ",
            $"{Data.Saes.Count} situations d'apprentissage",
            Data.Saes, BulletinEntryKind.Sae));
    }

    [RelayCommand]
    private void Back() => PopDetail();

    public void PushDetail(ViewModelBase detail)
    {
        _navigationStack.Push(detail);
        DetailTransition = ForwardTransition;
        CurrentDetail = detail;
    }

    public void PopDetail()
    {
        if (_navigationStack.Count > 0)
            _navigationStack.Pop();

        DetailTransition = BackwardTransition;
        CurrentDetail = _navigationStack.Count > 0 ? _navigationStack.Peek() : null;
    }

    /// <summary>Looks up a resource or SAÉ group by code (used when drilling from a UE).</summary>
    public EvaluationGroupInfo? FindGroup(string code) =>
        Data.Resources.FirstOrDefault(r => r.Code == code)
        ?? Data.Saes.FirstOrDefault(s => s.Code == code);

    #endregion
}
