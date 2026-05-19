namespace Prolizy2.ViewModels.Bulletin;

/// <summary>The layout directions offered for the bulletin.</summary>
public enum BulletinVariant
{
    /// <summary>Segmented tabs (Synthèse / Ressources / SAÉ).</summary>
    Tabs,

    /// <summary>Master / detail drill-down navigation.</summary>
    DrillDown
}

/// <summary>Vertical density of the bulletin rows.</summary>
public enum BulletinDensity
{
    Comfortable,
    Compact
}

/// <summary>Ordering applied to the teaching-unit list.</summary>
public enum UnitSortMode
{
    ByCode,
    ByAverage
}

/// <summary>Source of the colour used on the UE stripe.</summary>
public enum UnitColorMode
{
    /// <summary>Use each unit's canonical colour.</summary>
    ByUnit,

    /// <summary>Use a tone derived from the unit average.</summary>
    ByGrade
}
