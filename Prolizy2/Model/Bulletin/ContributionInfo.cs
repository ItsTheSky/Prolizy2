using System.Globalization;
using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Prolizy2.Model.Bulletin;

/// <summary>
/// A single resource / SAÉ / portfolio line contributing to a teaching unit.
/// </summary>
public partial class ContributionInfo : ObservableObject
{
    /// <summary>Code of the contributing element (e.g. <c>R2.01</c>, <c>SAE2.01</c>).</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Kind))]
    [NotifyPropertyChangedFor(nameof(AccentBrush))]
    [NotifyPropertyChangedFor(nameof(AccentSoftBrush))]
    private string _code = string.Empty;

    [ObservableProperty] private string _label = string.Empty;

    [ObservableProperty] private GradeValue _grade = new();

    /// <summary>Weight of this contribution inside the unit average.</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CoefficientLabel))]
    private double _coefficient;

    /// <summary>
    /// Pixel height of the mini weight bar — assigned by
    /// <see cref="TeachingUnitInfo.NormalizeWeights"/>.
    /// </summary>
    [ObservableProperty] private double _weightBarHeight = 6d;

    /// <summary>Derived from the <see cref="Code"/> prefix.</summary>
    public BulletinEntryKind Kind => BulletinStatus.KindFromCode(Code);

    /// <summary>Accent colour matching the contribution kind.</summary>
    public IBrush AccentBrush => BulletinStatus.AccentFor(Kind);

    /// <summary>Translucent accent — used behind the code chip.</summary>
    public IBrush AccentSoftBrush => BulletinStatus.AccentSoftFor(Kind);

    /// <summary>Coefficient suffix, e.g. <c>×21</c> or <c>×1,5</c>.</summary>
    public string CoefficientLabel => "×" + FormatCoefficient(Coefficient);

    internal static string FormatCoefficient(double value) =>
        value == (int)value
            ? ((int)value).ToString(CultureInfo.InvariantCulture)
            : value.ToString("0.##", CultureInfo.InvariantCulture).Replace('.', ',');
}
