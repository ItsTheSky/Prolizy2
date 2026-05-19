using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using Prolizy2.Styles.Controls;

namespace Prolizy2.Model.Bulletin;

/// <summary>
/// A teaching unit (UE) with its average, ranking and contributing elements.
/// </summary>
public partial class TeachingUnitInfo : ObservableObject
{
    /// <summary>Code of the unit (e.g. <c>UE2.1</c>).</summary>
    [ObservableProperty] private string _code = string.Empty;

    /// <summary>Canonical hex colour of the unit.</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(UnitBrush))]
    private string _colorHex = "#475569";

    /// <summary>Full descriptive name.</summary>
    [ObservableProperty] private string _name = string.Empty;

    /// <summary>Short competence name (e.g. <c>Réaliser</c>).</summary>
    [ObservableProperty] private string _shortName = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(GradeBrush))]
    private GradeValue _average = new();

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(RankLabel))]
    private int _rank;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(RankLabel))]
    private int _total;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasBonus))]
    [NotifyPropertyChangedFor(nameof(HasBonusOrMalus))]
    [NotifyPropertyChangedFor(nameof(BonusLabel))]
    private double _bonus;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasMalus))]
    [NotifyPropertyChangedFor(nameof(HasBonusOrMalus))]
    [NotifyPropertyChangedFor(nameof(MalusLabel))]
    private double _malus;

    /// <summary>Status code — <c>ADM</c>, <c>AJ</c>, <c>DEF</c>, …</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(StatusBadgeType))]
    [NotifyPropertyChangedFor(nameof(IsAcquired))]
    [NotifyPropertyChangedFor(nameof(AcquisitionDotBrush))]
    private string _status = "ADM";

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(TotalWeight))]
    private ObservableCollection<ContributionInfo> _contributions = new();

    private Color ParsedColor => Color.Parse(string.IsNullOrWhiteSpace(ColorHex) ? "#475569" : ColorHex);

    /// <summary>Brush built from <see cref="ColorHex"/>.</summary>
    public IBrush UnitBrush => new SolidColorBrush(ParsedColor);

    /// <summary>Translucent fill for the unit detail hero card.</summary>
    public IBrush HeroBackgroundBrush => new SolidColorBrush(ParsedColor, 0.22d);

    /// <summary>Translucent border for the unit detail hero card.</summary>
    public IBrush HeroBorderBrush => new SolidColorBrush(ParsedColor, 0.5d);

    /// <summary>Vivid brush picked from the unit average tone (colour-by-grade mode).</summary>
    public IBrush GradeBrush
    {
        get
        {
            if (!Average.IsNumeric)
                return UnitBrush;

            var value = Average.NumericValue!.Value;
            if (value >= 16d) return BulletinPalette.GradeHigh;
            if (value >= 12d) return BulletinPalette.GradeMid;
            if (value >= 10d) return BulletinPalette.GradeLow;
            return BulletinPalette.GradeBad;
        }
    }

    public BadgeType StatusBadgeType => BulletinStatus.ToBadgeType(Status);

    public bool IsAcquired => Status == "ADM";

    /// <summary>Brush for the small acquisition dot shown in the semester hero.</summary>
    public IBrush AcquisitionDotBrush => IsAcquired
        ? new SolidColorBrush(Color.Parse("#FF8C42"))
        : new SolidColorBrush(Color.Parse("#33FFFFFF"));

    public bool HasBonus => Bonus > 0d;

    public bool HasMalus => Malus > 0d;

    public bool HasBonusOrMalus => HasBonus || HasMalus;

    public string BonusLabel => "Bonus +" + FormatGrade(Bonus);

    public string MalusLabel => "Malus −" + FormatGrade(Malus);

    /// <summary>Ranking suffix, e.g. <c>3/71</c>.</summary>
    public string RankLabel => $"{Rank}/{Total}";

    /// <summary>Sum of all contribution coefficients.</summary>
    public double TotalWeight => Contributions.Sum(c => c.Coefficient);

    public string TotalWeightLabel =>
        "∑ coef " + TotalWeight.ToString("0.##", CultureInfo.InvariantCulture).Replace('.', ',');

    /// <summary>Resource contributions only (used by the drill-down detail screen).</summary>
    public System.Collections.Generic.IEnumerable<ContributionInfo> ResourceContributions =>
        Contributions.Where(c => c.Kind == BulletinEntryKind.Resource);

    /// <summary>SAÉ contributions only.</summary>
    public System.Collections.Generic.IEnumerable<ContributionInfo> SaeContributions =>
        Contributions.Where(c => c.Kind == BulletinEntryKind.Sae);

    /// <summary>Portfolio contributions only.</summary>
    public System.Collections.Generic.IEnumerable<ContributionInfo> PortfolioContributions =>
        Contributions.Where(c => c.Kind == BulletinEntryKind.Portfolio);

    public bool HasResourceContributions => Contributions.Any(c => c.Kind == BulletinEntryKind.Resource);

    public bool HasSaeContributions => Contributions.Any(c => c.Kind == BulletinEntryKind.Sae);

    public bool HasPortfolioContributions => Contributions.Any(c => c.Kind == BulletinEntryKind.Portfolio);

    /// <summary>
    /// Assigns each contribution a mini weight-bar height proportional to its
    /// coefficient. Call once after the contribution list is populated.
    /// </summary>
    public void NormalizeWeights()
    {
        var total = TotalWeight;
        foreach (var contribution in Contributions)
        {
            contribution.WeightBarHeight = total > 0d
                ? System.Math.Max(6d, contribution.Coefficient / total * 44d)
                : 6d;
        }
    }

    private static string FormatGrade(double value) =>
        value.ToString("0.00", CultureInfo.InvariantCulture).Replace('.', ',');
}
