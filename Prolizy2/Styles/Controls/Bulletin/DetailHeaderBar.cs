using System.Windows.Input;
using Avalonia;
using Avalonia.Controls.Primitives;
using Avalonia.Media;

namespace Prolizy2.Styles.Controls.Bulletin;

/// <summary>
/// Navigation bar for the drill-down detail screens: a back button plus a
/// title / subtitle pair.
/// </summary>
public class DetailHeaderBar : TemplatedControl
{
    public static readonly StyledProperty<string> TitleProperty =
        AvaloniaProperty.Register<DetailHeaderBar, string>(nameof(Title), string.Empty);

    public static readonly StyledProperty<string?> SubtitleProperty =
        AvaloniaProperty.Register<DetailHeaderBar, string?>(nameof(Subtitle));

    public static readonly StyledProperty<ICommand?> BackCommandProperty =
        AvaloniaProperty.Register<DetailHeaderBar, ICommand?>(nameof(BackCommand));

    public static readonly StyledProperty<IBrush?> AccentProperty =
        AvaloniaProperty.Register<DetailHeaderBar, IBrush?>(nameof(Accent));

    public string Title
    {
        get => GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public string? Subtitle
    {
        get => GetValue(SubtitleProperty);
        set => SetValue(SubtitleProperty, value);
    }

    public ICommand? BackCommand
    {
        get => GetValue(BackCommandProperty);
        set => SetValue(BackCommandProperty, value);
    }

    /// <summary>Tint applied to the back chevron.</summary>
    public IBrush? Accent
    {
        get => GetValue(AccentProperty);
        set => SetValue(AccentProperty, value);
    }
}
