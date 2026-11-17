using System;

namespace Prolizy2.Model.Home;

/// <summary>
/// Identifies a kind of home card. Each kind maps to one card view
/// (see <c>HomeCardViewFactory</c>) and one card view-model.
/// </summary>
public enum HomeCardType
{
    /// <summary>The next (or ongoing) course.</summary>
    NextCourse,

    /// <summary>The semester average plus the units acquired.</summary>
    BulletinOverview,

    /// <summary>The grades of a chosen resource or SAÉ.</summary>
    EvaluationDetail,

    /// <summary>The list of teaching units with their average.</summary>
    UnitList
}

/// <summary>Width of a card: one half of the row or the whole row.</summary>
public enum HomeCardSize
{
    /// <summary>Half width — two cards sit side by side.</summary>
    Half,

    /// <summary>Full width — the card spans both columns.</summary>
    Full
}

/// <summary>How much detail a card shows.</summary>
public enum HomeCardVariant
{
    /// <summary>Standard card: header, body and extra detail rows.</summary>
    Classic,

    /// <summary>Compact card: smaller header, body only (no extra rows).</summary>
    Compact
}