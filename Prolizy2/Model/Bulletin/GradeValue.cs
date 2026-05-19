using System;
using System.Globalization;
using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using Prolizy2.Styles.Controls;

namespace Prolizy2.Model.Bulletin;

/// <summary>
/// Semantic tone of a grade, derived from its numeric value.
/// </summary>
public enum BulletinGradeTone
{
    Neutral,
    Error,
    Warning,
    Info,
    Success
}

/// <summary>
/// A grade that is either numeric (e.g. <c>17.90</c>) or a textual token
/// (e.g. <c>EXC</c>, <c>ATT</c>, <c>—</c>). All display-oriented properties are
/// computed so the view layer only ever has to bind.
/// </summary>
public partial class GradeValue : ObservableObject
{
    /// <summary>Numeric value of the grade, or <c>null</c> when textual.</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsNumeric))]
    [NotifyPropertyChangedFor(nameof(Display))]
    [NotifyPropertyChangedFor(nameof(IntegerPart))]
    [NotifyPropertyChangedFor(nameof(DecimalPart))]
    [NotifyPropertyChangedFor(nameof(Tone))]
    [NotifyPropertyChangedFor(nameof(ToneBrush))]
    [NotifyPropertyChangedFor(nameof(BadgeType))]
    private double? _numericValue;

    /// <summary>Textual token displayed when the grade is not numeric.</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Display))]
    [NotifyPropertyChangedFor(nameof(IntegerPart))]
    private string? _label;

    /// <summary>The grade ceiling — usually 20.</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(MaximumLabel))]
    private double _maximum = 20d;

    public GradeValue()
    {
    }

    public GradeValue(double value, double maximum = 20d)
    {
        _numericValue = value;
        _maximum = maximum;
    }

    public GradeValue(string label)
    {
        _label = label;
    }

    /// <summary>Convenience factory for a numeric grade.</summary>
    public static GradeValue Numeric(double value, double maximum = 20d) => new(value, maximum);

    /// <summary>Convenience factory for a textual grade (EXC, ATT, …).</summary>
    public static GradeValue Text(string label) => new(label);

    public bool IsNumeric => NumericValue.HasValue;

    /// <summary>French-formatted full value, e.g. <c>17,90</c> or <c>EXC</c>.</summary>
    public string Display => IsNumeric
        ? NumericValue!.Value.ToString("0.00", CultureInfo.InvariantCulture).Replace('.', ',')
        : Label ?? "—";

    /// <summary>Integer portion, e.g. <c>17</c> (or the token when textual).</summary>
    public string IntegerPart => IsNumeric
        ? ((int)Math.Floor(NumericValue!.Value)).ToString(CultureInfo.InvariantCulture)
        : Label ?? "—";

    /// <summary>Decimal portion with a French comma, e.g. <c>,90</c> (empty when textual).</summary>
    public string DecimalPart
    {
        get
        {
            if (!IsNumeric)
                return string.Empty;

            var formatted = NumericValue!.Value.ToString("0.00", CultureInfo.InvariantCulture);
            var dot = formatted.IndexOf('.');
            return dot >= 0 ? "," + formatted[(dot + 1)..] : string.Empty;
        }
    }

    /// <summary>Maximum suffix, e.g. <c>/20</c>.</summary>
    public string MaximumLabel => "/" + ((int)Maximum).ToString(CultureInfo.InvariantCulture);

    public BulletinGradeTone Tone
    {
        get
        {
            if (!IsNumeric)
                return BulletinGradeTone.Neutral;

            var value = NumericValue!.Value;
            if (value >= 16d) return BulletinGradeTone.Success;
            if (value >= 12d) return BulletinGradeTone.Info;
            if (value >= 10d) return BulletinGradeTone.Warning;
            return BulletinGradeTone.Error;
        }
    }

    /// <summary>Foreground brush matching the grade tone.</summary>
    public IBrush ToneBrush => Tone switch
    {
        BulletinGradeTone.Success => BulletinPalette.Success,
        BulletinGradeTone.Info => BulletinPalette.Info,
        BulletinGradeTone.Warning => BulletinPalette.Warning,
        BulletinGradeTone.Error => BulletinPalette.Error,
        _ => BulletinPalette.Neutral
    };

    /// <summary>Maps the tone onto the shared <see cref="BadgeType"/> enum.</summary>
    public BadgeType BadgeType => Tone switch
    {
        BulletinGradeTone.Success => BadgeType.Success,
        BulletinGradeTone.Info => BadgeType.Info,
        BulletinGradeTone.Warning => BadgeType.Warning,
        BulletinGradeTone.Error => BadgeType.Error,
        _ => BadgeType.Neutral
    };
}
