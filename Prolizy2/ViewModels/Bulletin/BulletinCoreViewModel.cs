using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Animation;
using Avalonia.Animation.Easings;
using Avalonia.Controls;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Prolizy2.Model.Bulletin;
using Prolizy2.Services;
using Prolizy2.Utilities;
using Prolizy2.ViewModels.Tabs;
using Prolizy2.ViewModels.Common;
using Prolizy2.ViewModels.Tabs.Settings;
using Prolizy2.Views;
using Prolizy2.Views.Bulletin;
using Prolizy2.Views.Tabs.Settings;
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

    /// <summary>True when credentials are present and the bulletin can be queried.</summary>
    [ObservableProperty] private bool _isConfigured;

    /// <summary>True while an HTTP fetch is in flight.</summary>
    [ObservableProperty] private bool _isLoading;

    /// <summary>Last fatal error encountered while fetching; null on success.</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasError))]
    private string? _loadError;

    public bool HasError => !string.IsNullOrEmpty(LoadError);

    private bool _suppressSemesterReload;

    #region Drill-down navigation

    private readonly System.Collections.Generic.Stack<ViewModelBase> _navigationStack = new();

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasDetail))]
    [NotifyPropertyChangedFor(nameof(CurrentPage))]
    private ViewModelBase? _currentDetail;

    public bool HasDetail => CurrentDetail is not null;

    /// <summary>
    /// The view-model currently shown by the drill-down's TransitioningContentControl —
    /// either the active detail or the core VM itself (templated to the master view).
    /// </summary>
    public object CurrentPage => CurrentDetail ?? (object)this;

    private static readonly IPageTransition ForwardTransition = new DirectionalSlideTransition(
        DirectionalSlideTransition.SlideDirection.RightToLeft,
        TimeSpan.FromMilliseconds(200),
        TimeSpan.FromMilliseconds(200),
        new CubicEaseOut());

    private static readonly IPageTransition BackwardTransition = new DirectionalSlideTransition(
        DirectionalSlideTransition.SlideDirection.LeftToRight,
        TimeSpan.FromMilliseconds(200),
        TimeSpan.FromMilliseconds(200),
        new CubicEaseOut());

    [ObservableProperty] private IPageTransition _detailTransition = ForwardTransition;

    #endregion

    public BulletinCoreViewModel(MainViewModel mainVm)
    {
        MainVm = mainVm;

        if (Design.IsDesignMode)
        {
            _data = BulletinSampleData.Create();
            SelectedSemester = Data.Semesters.FirstOrDefault(s => s.IsCurrent)
                               ?? Data.Semesters.FirstOrDefault(s => !s.IsFuture);
            RefreshSortedUnits();
            Options.PropertyChanged += OnOptionsChanged;
            IsConfigured = true;
            return;
        }

        _data = new BulletinData();
        // The credential store is decrypted in the background; start from a
        // cheap guess and let LoadAsync settle the real state.
        IsConfigured = MainVm.BulletinService.IsProbablyConfigured;

        RefreshSortedUnits();
        Options.PropertyChanged += OnOptionsChanged;

        if (IsConfigured)
            _ = LoadAsync();
    }

    /// <summary>
    /// Reloads everything — called on first display, after credentials change,
    /// and from the retry button when an error is shown.
    /// </summary>
    public async Task LoadAsync()
    {
        IsConfigured = await MainVm.BulletinService.IsConfiguredAsync();
        if (!IsConfigured)
            return;

        if (IsLoading)
            return;

        IsLoading = true;
        LoadError = null;
        try
        {
            var root = await MainVm.BulletinService.LoadInitialAsync();
            if (root is null)
            {
                LoadError = LangViewModel.Translate("BulletinErrorLoginRefused");
                return;
            }

            var data = BulletinMapper.Map(root);
            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                _suppressSemesterReload = true;
                Data = data;
                SelectedSemester = Data.Semesters.FirstOrDefault(s => s.IsCurrent)
                                   ?? Data.Semesters.FirstOrDefault(s => !s.IsFuture);
                _suppressSemesterReload = false;
                RefreshSortedUnits();
            });
        }
        catch (Exception ex)
        {
            Logs.Error(ex);
            LoadError = LangViewModel.Translate("BulletinErrorFetchFailed") + ex.Message;
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>
    /// Notifies the view model that credentials were saved or removed so it can
    /// rebuild its state.
    /// </summary>
    public async Task RefreshConfigurationAsync()
    {
        IsConfigured = await MainVm.BulletinService.IsConfiguredAsync();
        if (IsConfigured)
        {
            await LoadAsync();
        }
        else
        {
            Data = new BulletinData();
            RefreshSortedUnits();
            LoadError = null;
        }
    }

    [RelayCommand]
    private async Task Retry() => await LoadAsync();

    private async Task ReloadSemesterAsync(SemesterInfo semester)
    {
        if (semester.IsFuture)
            return;

        if (!int.TryParse(semester.Id, NumberStyles.Integer, CultureInfo.InvariantCulture, out var formsemestreId))
            return;

        if (IsLoading)
            return;

        IsLoading = true;
        LoadError = null;
        try
        {
            var root = await MainVm.BulletinService.LoadSemesterAsync(formsemestreId);
            if (root is null)
            {
                LoadError = LangViewModel.Translate("BulletinErrorSemesterLoadFailed");
                return;
            }

            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                BulletinMapper.MapSemesterInto(root, Data);
                RefreshSortedUnits();
            });
        }
        catch (Exception ex)
        {
            Logs.Error(ex);
            LoadError = LangViewModel.Translate("BulletinErrorSemesterLoad") + ex.Message;
        }
        finally
        {
            IsLoading = false;
        }
    }

    partial void OnSelectedSemesterChanged(SemesterInfo? value)
    {
        if (_suppressSemesterReload || value is null || Design.IsDesignMode)
            return;
        _ = ReloadSemesterAsync(value);
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
            .WithTitle(LangViewModel.Translate("BulletinStudentInfoTitle"))
            .WithContent(new StudentInfoView { DataContext = Data.Student })
            .WithActionButton("Fermer", _ => { }, true)
            .TryShow();
    }

    [RelayCommand]
    private async Task RefreshDatas() => await LoadAsync();

    [RelayCommand]
    private void OpenSettings()
    {
        MainVm.NavigateTo(typeof(SettingsViewModel));
        MainVm.SettingsViewModel.SwitchToPage(new BulletinSettingsView
        {
            DataContext = new BulletinSettingsViewModel(MainVm, MainVm.SettingsViewModel)
        });
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
