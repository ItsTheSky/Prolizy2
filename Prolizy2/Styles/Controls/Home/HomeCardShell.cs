using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Lucide.Avalonia;

namespace Prolizy2.Styles.Controls.Home;

/// <summary>
/// Card shell shared by every home card: a tappable glass card with a coloured
/// icon, a title, an optional hint and the card content below.
/// </summary>
[PseudoClasses(":compact")]
public class HomeCardShell : ContentControl
{
    public static readonly StyledProperty<string> TitleProperty =
        AvaloniaProperty.Register<HomeCardShell, string>(nameof(Title));

    public static readonly StyledProperty<LucideIconKind?> IconProperty =
        AvaloniaProperty.Register<HomeCardShell, LucideIconKind?>(nameof(Icon));

    /// <summary>Accent colour used by the icon tile.</summary>
    public static readonly StyledProperty<string> AccentHexProperty =
        AvaloniaProperty.Register<HomeCardShell, string>(nameof(AccentHex), "#8C8C8C");

    /// <summary>Optional hint shown under the title.</summary>
    public static readonly StyledProperty<string> HintProperty =
        AvaloniaProperty.Register<HomeCardShell, string>(nameof(Hint));

    /// <summary>Command raised when the card is tapped.</summary>
    public static readonly StyledProperty<System.Windows.Input.ICommand?> CommandProperty =
        AvaloniaProperty.Register<HomeCardShell, System.Windows.Input.ICommand?>(nameof(Command));

    public static readonly StyledProperty<object?> CommandParameterProperty =
        AvaloniaProperty.Register<HomeCardShell, object?>(nameof(CommandParameter));

    /// <summary>Compact header: smaller icon, tighter paddings.</summary>
    public static readonly StyledProperty<bool> IsCompactProperty =
        AvaloniaProperty.Register<HomeCardShell, bool>(nameof(IsCompact));

    public static readonly StyledProperty<bool> IsInteractiveProperty =
        AvaloniaProperty.Register<HomeCardShell, bool>(nameof(IsInteractive), true);

    /// <summary>Side of the icon tile (compact cards use a smaller one).</summary>
    public static readonly StyledProperty<double> IconSizeProperty =
        AvaloniaProperty.Register<HomeCardShell, double>(nameof(IconSize), 36d);

    /// <summary>Glyph size of the header icon.</summary>
    public static readonly StyledProperty<double> IconGlyphSizeProperty =
        AvaloniaProperty.Register<HomeCardShell, double>(nameof(IconGlyphSize), 18d);

    /// <summary>Font size of the card title.</summary>
    public static readonly StyledProperty<double> TitleFontSizeProperty =
        AvaloniaProperty.Register<HomeCardShell, double>(nameof(TitleFontSize), 14d);

    public double IconSize
    {
        get => GetValue(IconSizeProperty);
        set => SetValue(IconSizeProperty, value);
    }

    public double IconGlyphSize
    {
        get => GetValue(IconGlyphSizeProperty);
        set => SetValue(IconGlyphSizeProperty, value);
    }

    public double TitleFontSize
    {
        get => GetValue(TitleFontSizeProperty);
        set => SetValue(TitleFontSizeProperty, value);
    }

    public string Title
    {
        get => GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public LucideIconKind? Icon
    {
        get => GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    public string AccentHex
    {
        get => GetValue(AccentHexProperty);
        set => SetValue(AccentHexProperty, value);
    }

    public string Hint
    {
        get => GetValue(HintProperty);
        set => SetValue(HintProperty, value);
    }

    public System.Windows.Input.ICommand? Command
    {
        get => GetValue(CommandProperty);
        set => SetValue(CommandProperty, value);
    }

    public object? CommandParameter
    {
        get => GetValue(CommandParameterProperty);
        set => SetValue(CommandParameterProperty, value);
    }

    public bool IsCompact
    {
        get => GetValue(IsCompactProperty);
        set => SetValue(IsCompactProperty, value);
    }

    public bool IsInteractive
    {
        get => GetValue(IsInteractiveProperty);
        set => SetValue(IsInteractiveProperty, value);
    }

    static HomeCardShell()
    {
        IsCompactProperty.Changed.AddClassHandler<HomeCardShell>(
            (shell, _) => shell.PseudoClasses.Set(":compact", shell.IsCompact));
    }
}