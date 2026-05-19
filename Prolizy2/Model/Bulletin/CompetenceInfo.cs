using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using Prolizy2.Styles.Controls;

namespace Prolizy2.Model.Bulletin;

/// <summary>
/// A RCUE competence line (e.g. <c>Réaliser — ADM</c>).
/// </summary>
public partial class CompetenceInfo : ObservableObject
{
    [ObservableProperty] private string _name = string.Empty;

    /// <summary>Status code — <c>ADM</c>, <c>AJ</c>, <c>DEF</c>, …</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(StatusBadgeType))]
    [NotifyPropertyChangedFor(nameof(DotBrush))]
    private string _status = "ADM";

    /// <summary>Optional code of the UE this competence maps to (e.g. <c>UE2.1</c>).</summary>
    [ObservableProperty] private string? _unitCode;

    public BadgeType StatusBadgeType => BulletinStatus.ToBadgeType(Status);

    /// <summary>Status dot brush shown in the drill-down RCUE block.</summary>
    public IBrush DotBrush => Status == "ADM"
        ? new SolidColorBrush(Color.Parse("#16A34A"))
        : new SolidColorBrush(Color.Parse("#71717A"));
}
