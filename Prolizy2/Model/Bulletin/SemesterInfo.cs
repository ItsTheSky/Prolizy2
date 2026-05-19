using CommunityToolkit.Mvvm.ComponentModel;

namespace Prolizy2.Model.Bulletin;

/// <summary>
/// One selectable semester chip (e.g. <c>S2 · 2024/2025 · FI</c>).
/// </summary>
public partial class SemesterInfo : ObservableObject
{
    /// <summary>Stable identifier (e.g. <c>s2</c>).</summary>
    [ObservableProperty] private string _id = string.Empty;

    /// <summary>Short code shown on the chip (e.g. <c>S2</c>).</summary>
    [ObservableProperty] private string _code = string.Empty;

    /// <summary>Academic year (e.g. <c>2024/2025</c>).</summary>
    [ObservableProperty] private string _year = string.Empty;

    /// <summary>Optional track tag (e.g. <c>FI</c>, <c>FA</c>).</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasTrack))]
    private string? _track;

    /// <summary>A future semester cannot be opened — the chip is disabled.</summary>
    [ObservableProperty] private bool _isFuture;

    /// <summary>Marks the semester currently being studied.</summary>
    [ObservableProperty] private bool _isCurrent;

    /// <summary>Year decision code (e.g. <c>ADM</c>).</summary>
    [ObservableProperty] private string? _decision;

    /// <summary>Human-readable decision label (e.g. <c>Admis</c>).</summary>
    [ObservableProperty] private string? _decisionLabel;

    /// <summary>Enrollment date, displayed in the fine print.</summary>
    [ObservableProperty] private string? _enrollmentDate;

    public bool HasTrack => !string.IsNullOrWhiteSpace(Track);
}
