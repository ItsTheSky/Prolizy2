using System;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using Prolizy2.Views;

namespace Prolizy2.ViewModels.Bulletin;

/// <summary>
/// User-tunable display parameters for the bulletin feature. A single shared
/// instance drives every variant and is edited from the settings sub-page;
/// it is persisted to <see cref="MainView.Preferences"/> automatically.
/// </summary>
public partial class BulletinDisplayOptions : ObservableObject
{
    private const string PreferenceKey = "bulletin_display_options";

    private static BulletinDisplayOptions? _instance;

    /// <summary>The application-wide, shared options instance.</summary>
    public static BulletinDisplayOptions Instance => _instance ??= new BulletinDisplayOptions();

    private bool _suppressPersist;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsTabsVariant))]
    [NotifyPropertyChangedFor(nameof(IsDrillDownVariant))]
    private BulletinVariant _variant = BulletinVariant.Tabs;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsComfortable))]
    [NotifyPropertyChangedFor(nameof(IsCompact))]
    private BulletinDensity _density = BulletinDensity.Comfortable;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsSortByCode))]
    [NotifyPropertyChangedFor(nameof(IsSortByAverage))]
    private UnitSortMode _sortMode = UnitSortMode.ByCode;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsColorByUnit))]
    [NotifyPropertyChangedFor(nameof(IsColorByGrade))]
    private UnitColorMode _colorMode = UnitColorMode.ByUnit;

    [ObservableProperty] private bool _showRank = true;
    [ObservableProperty] private bool _showCoefficients = true;
    [ObservableProperty] private bool _showWeightBars = true;

    private BulletinDisplayOptions() => Load();

    #region Two-way bool helpers (radio / toggle binding)

    public bool IsTabsVariant
    {
        get => Variant == BulletinVariant.Tabs;
        set { if (value) Variant = BulletinVariant.Tabs; }
    }

    public bool IsDrillDownVariant
    {
        get => Variant == BulletinVariant.DrillDown;
        set { if (value) Variant = BulletinVariant.DrillDown; }
    }

    public bool IsComfortable
    {
        get => Density == BulletinDensity.Comfortable;
        set { if (value) Density = BulletinDensity.Comfortable; }
    }

    public bool IsCompact
    {
        get => Density == BulletinDensity.Compact;
        set { if (value) Density = BulletinDensity.Compact; }
    }

    public bool IsSortByCode
    {
        get => SortMode == UnitSortMode.ByCode;
        set { if (value) SortMode = UnitSortMode.ByCode; }
    }

    public bool IsSortByAverage
    {
        get => SortMode == UnitSortMode.ByAverage;
        set { if (value) SortMode = UnitSortMode.ByAverage; }
    }

    public bool IsColorByUnit
    {
        get => ColorMode == UnitColorMode.ByUnit;
        set { if (value) ColorMode = UnitColorMode.ByUnit; }
    }

    public bool IsColorByGrade
    {
        get => ColorMode == UnitColorMode.ByGrade;
        set { if (value) ColorMode = UnitColorMode.ByGrade; }
    }

    #endregion

    #region Persistence

    partial void OnVariantChanged(BulletinVariant value) => Persist();
    partial void OnDensityChanged(BulletinDensity value) => Persist();
    partial void OnSortModeChanged(UnitSortMode value) => Persist();
    partial void OnColorModeChanged(UnitColorMode value) => Persist();
    partial void OnShowRankChanged(bool value) => Persist();
    partial void OnShowCoefficientsChanged(bool value) => Persist();
    partial void OnShowWeightBarsChanged(bool value) => Persist();

    private void Load()
    {
        if (Design.IsDesignMode)
            return;

        var dto = MainView.Preferences.Get<OptionsDto?>(PreferenceKey, null);
        if (dto is null)
            return;

        _suppressPersist = true;
        var variant = (BulletinVariant)dto.Variant;
        Variant = Enum.IsDefined(variant) ? variant : BulletinVariant.Tabs;
        Density = (BulletinDensity)dto.Density;
        SortMode = (UnitSortMode)dto.SortMode;
        ColorMode = (UnitColorMode)dto.ColorMode;
        ShowRank = dto.ShowRank;
        ShowCoefficients = dto.ShowCoefficients;
        ShowWeightBars = dto.ShowWeightBars;
        _suppressPersist = false;
    }

    private void Persist()
    {
        if (_suppressPersist || Design.IsDesignMode)
            return;

        MainView.Preferences.Set(PreferenceKey, new OptionsDto
        {
            Variant = (int)Variant,
            Density = (int)Density,
            SortMode = (int)SortMode,
            ColorMode = (int)ColorMode,
            ShowRank = ShowRank,
            ShowCoefficients = ShowCoefficients,
            ShowWeightBars = ShowWeightBars
        });
    }

    private sealed class OptionsDto
    {
        public int Variant { get; set; }
        public int Density { get; set; }
        public int SortMode { get; set; }
        public int ColorMode { get; set; }
        public bool ShowRank { get; set; } = true;
        public bool ShowCoefficients { get; set; } = true;
        public bool ShowWeightBars { get; set; } = true;
    }

    #endregion
}
