using Avalonia;
using Avalonia.Controls.Primitives;
using Avalonia.Media;
using Prolizy2.Model.Bulletin;

namespace Prolizy2.Styles.Controls.Bulletin;

/// <summary>
/// Renders a <see cref="GradeValue"/> with the design's "big grade" treatment:
/// a large integer part, a smaller dimmed decimal part and an optional
/// <c>/20</c> suffix. Textual grades (EXC, ATT, …) fall back to a single run.
/// </summary>
public class GradeText : TemplatedControl
{
    public static readonly StyledProperty<GradeValue?> GradeProperty =
        AvaloniaProperty.Register<GradeText, GradeValue?>(nameof(Grade));

    public static readonly StyledProperty<double> GradeSizeProperty =
        AvaloniaProperty.Register<GradeText, double>(nameof(GradeSize), 28d);

    public static readonly StyledProperty<bool> ShowMaximumProperty =
        AvaloniaProperty.Register<GradeText, bool>(nameof(ShowMaximum), true);

    public static readonly StyledProperty<IBrush?> DimBrushProperty =
        AvaloniaProperty.Register<GradeText, IBrush?>(nameof(DimBrush));

    // ---- Derived properties consumed by the template -----------------------

    public static readonly StyledProperty<double> DecimalFontSizeProperty =
        AvaloniaProperty.Register<GradeText, double>(nameof(DecimalFontSize), 28d * 0.55d);

    public static readonly StyledProperty<double> MaximumFontSizeProperty =
        AvaloniaProperty.Register<GradeText, double>(nameof(MaximumFontSize), 28d * 0.42d);

    public static readonly StyledProperty<bool> ShowsMaximumProperty =
        AvaloniaProperty.Register<GradeText, bool>(nameof(ShowsMaximum), true);

    static GradeText()
    {
        GradeSizeProperty.Changed.AddClassHandler<GradeText>((c, _) => c.Recompute());
        GradeProperty.Changed.AddClassHandler<GradeText>((c, _) => c.Recompute());
        ShowMaximumProperty.Changed.AddClassHandler<GradeText>((c, _) => c.Recompute());
    }

    public GradeText() => Recompute();

    public GradeValue? Grade
    {
        get => GetValue(GradeProperty);
        set => SetValue(GradeProperty, value);
    }

    /// <summary>Font size of the large integer part.</summary>
    public double GradeSize
    {
        get => GetValue(GradeSizeProperty);
        set => SetValue(GradeSizeProperty, value);
    }

    /// <summary>Whether the <c>/20</c> suffix is rendered for numeric grades.</summary>
    public bool ShowMaximum
    {
        get => GetValue(ShowMaximumProperty);
        set => SetValue(ShowMaximumProperty, value);
    }

    /// <summary>Brush used for the decimal part and the maximum suffix.</summary>
    public IBrush? DimBrush
    {
        get => GetValue(DimBrushProperty);
        set => SetValue(DimBrushProperty, value);
    }

    public double DecimalFontSize
    {
        get => GetValue(DecimalFontSizeProperty);
        private set => SetValue(DecimalFontSizeProperty, value);
    }

    public double MaximumFontSize
    {
        get => GetValue(MaximumFontSizeProperty);
        private set => SetValue(MaximumFontSizeProperty, value);
    }

    public bool ShowsMaximum
    {
        get => GetValue(ShowsMaximumProperty);
        private set => SetValue(ShowsMaximumProperty, value);
    }

    private void Recompute()
    {
        DecimalFontSize = GradeSize * 0.55d;
        MaximumFontSize = GradeSize * 0.42d;
        ShowsMaximum = ShowMaximum && (Grade?.IsNumeric ?? false);
    }
}
