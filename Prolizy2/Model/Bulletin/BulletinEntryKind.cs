using Avalonia.Media;
using Prolizy2.Styles.Controls;

namespace Prolizy2.Model.Bulletin;

/// <summary>
/// Classifies a bulletin entry — a resource, a SAÉ or a portfolio item.
/// </summary>
public enum BulletinEntryKind
{
    Resource,
    Sae,
    Portfolio
}

/// <summary>
/// Helpers shared by the bulletin models for status codes and entry kinds.
/// </summary>
public static class BulletinStatus
{
    /// <summary>Maps a ScoDoc status code onto a <see cref="BadgeType"/>.</summary>
    public static BadgeType ToBadgeType(string? status) => status switch
    {
        "ADM" => BadgeType.Success,
        "AJ" => BadgeType.Warning,
        "DEF" => BadgeType.Error,
        "ATT" => BadgeType.Info,
        _ => BadgeType.Neutral
    };

    /// <summary>Infers the entry kind from a code prefix (R…, SAE…, P…).</summary>
    public static BulletinEntryKind KindFromCode(string? code)
    {
        if (string.IsNullOrEmpty(code))
            return BulletinEntryKind.Resource;

        if (code.StartsWith("SAE", System.StringComparison.OrdinalIgnoreCase)
            || code.StartsWith("SAÉ", System.StringComparison.OrdinalIgnoreCase))
            return BulletinEntryKind.Sae;

        if (code.StartsWith("P", System.StringComparison.OrdinalIgnoreCase))
            return BulletinEntryKind.Portfolio;

        return BulletinEntryKind.Resource;
    }

    /// <summary>Accent brush for an entry kind.</summary>
    public static IBrush AccentFor(BulletinEntryKind kind) => kind switch
    {
        BulletinEntryKind.Sae => BulletinPalette.SaeAccent,
        BulletinEntryKind.Portfolio => BulletinPalette.PortfolioAccent,
        _ => BulletinPalette.ResourceAccent
    };

    /// <summary>Translucent accent brush — used behind code chips.</summary>
    public static IBrush AccentSoftFor(BulletinEntryKind kind)
    {
        var hex = kind switch
        {
            BulletinEntryKind.Sae => "#B968C7",
            BulletinEntryKind.Portfolio => "#FFB74D",
            _ => "#10B981"
        };
        return new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.Parse(hex), 0.18d);
    }
}
