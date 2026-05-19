using Avalonia.Media;

namespace Prolizy2.Model.Bulletin;

/// <summary>
/// Centralised brushes for the bulletin feature. Mirrors the design-system
/// semantic palette (light tones for text/badges, vivid tones for grade stripes).
/// </summary>
public static class BulletinPalette
{
    // Semantic "light" tones — used for grade text and badges.
    public static readonly IBrush Success = Make("#A5D6A7");
    public static readonly IBrush Info = Make("#BBDEFB");
    public static readonly IBrush Warning = Make("#FFB74D");
    public static readonly IBrush Error = Make("#EF9A9A");
    public static readonly IBrush Neutral = Make("#E0E0E0");

    // Vivid grade-tone colours — used for the "colour by grade" UE stripe.
    public static readonly IBrush GradeHigh = Make("#16A34A");
    public static readonly IBrush GradeMid = Make("#0284C7");
    public static readonly IBrush GradeLow = Make("#D97706");
    public static readonly IBrush GradeBad = Make("#DC2626");

    // Accents for the contribution kinds.
    public static readonly IBrush ResourceAccent = Make("#10B981");
    public static readonly IBrush SaeAccent = Make("#B968C7");
    public static readonly IBrush PortfolioAccent = Make("#FFB74D");

    private static SolidColorBrush Make(string hex) => new(Color.Parse(hex));
}
