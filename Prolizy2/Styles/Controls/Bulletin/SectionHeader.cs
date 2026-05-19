using Avalonia;
using Avalonia.Controls.Primitives;
using Lucide.Avalonia;

namespace Prolizy2.Styles.Controls.Bulletin;

/// <summary>
/// Small "icon + label + optional hint" header used to introduce a section
/// inside any bulletin variant.
/// </summary>
public class SectionHeader : TemplatedControl
{
    public static readonly StyledProperty<LucideIconKind> IconProperty =
        AvaloniaProperty.Register<SectionHeader, LucideIconKind>(nameof(Icon), LucideIconKind.Layers);

    public static readonly StyledProperty<string> TitleProperty =
        AvaloniaProperty.Register<SectionHeader, string>(nameof(Title), string.Empty);

    public static readonly StyledProperty<string?> HintProperty =
        AvaloniaProperty.Register<SectionHeader, string?>(nameof(Hint));

    public LucideIconKind Icon
    {
        get => GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    public string Title
    {
        get => GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    /// <summary>Optional trailing hint, e.g. an item count.</summary>
    public string? Hint
    {
        get => GetValue(HintProperty);
        set => SetValue(HintProperty, value);
    }
}
