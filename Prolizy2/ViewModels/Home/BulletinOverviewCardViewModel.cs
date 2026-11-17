using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using Prolizy2.Model.Bulletin;
using Prolizy2.ViewModels.Common;

namespace Prolizy2.ViewModels.Tabs;

/// <summary>
/// Card summarising a semester bulletin: the semester average, the decision and
/// the units acquired / total.
/// </summary>
public partial class BulletinOverviewCardViewModel : ViewModelBase
{
    public BulletinOverviewCardViewModel(BulletinData? data, string? semesterId)
    {
        SemesterId = semesterId;
        Apply(data);
    }

    /// <summary>Requested <c>formsemestre_id</c>, or null for the current semester.</summary>
    public string? SemesterId { get; }

    /// <summary>Label of the semester shown (e.g. <c>S2 · 2024/2025</c>).</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasData))]
    [NotifyPropertyChangedFor(nameof(HasNoData))]
    private string _semesterLabel = string.Empty;

    [ObservableProperty] private GradeValue? _average;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(AcquiredSummary))]
    private int _acquiredUnits;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(AcquiredSummary))]
    private int _totalUnits;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(DecisionText))]
    [NotifyPropertyChangedFor(nameof(DecisionBadgeType))]
    private string _decision = "—";

    [ObservableProperty] private ObservableCollection<TeachingUnitInfo> _acquiredList = new();

    [ObservableProperty]
    private bool _hasData;

    public bool HasNoData => !HasData;

    public string AcquiredSummary => $"{AcquiredUnits}/{TotalUnits}";

    public string DecisionText => Decision switch
    {
        "ADM" => LangViewModel.Translate("HomeCardDecisionAdmitted"),
        "AJ" => LangViewModel.Translate("HomeCardDecisionDeferred"),
        "DEF" => LangViewModel.Translate("HomeCardDecisionFailed"),
        _ => Decision
    };

    public global::Prolizy2.Styles.Controls.BadgeType DecisionBadgeType =>
        BulletinStatus.ToBadgeType(Decision);

    /// <summary>Fills the card from the bulletin; safe to call every time the data changes.</summary>
    public void Apply(BulletinData? data)
    {
        if (data is null)
        {
            HasData = false;
            SemesterLabel = string.Empty;
            Average = null;
            AcquiredUnits = 0;
            TotalUnits = 0;
            Decision = "—";
            AcquiredList = new ObservableCollection<TeachingUnitInfo>();
            return;
        }

        var semester = FindSemester(data, SemesterId);

        SemesterLabel = semester is null
            ? data.Student?.Program ?? string.Empty
            : $"{semester.Code} · {semester.Year}";

        Average = data.SemesterAverage;
        AcquiredUnits = data.AcquiredUnitCount;
        TotalUnits = data.TotalUnitCount;
        Decision = data.Decision;

        var acquired = data.Units.Where(u => u.IsAcquired).ToList();
        AcquiredList = new ObservableCollection<TeachingUnitInfo>(acquired);

        HasData = true;
    }

    private static SemesterInfo? FindSemester(BulletinData data, string? semesterId)
    {
        if (string.IsNullOrEmpty(semesterId))
            return data.Semesters.FirstOrDefault(s => s.IsCurrent)
                   ?? data.Semesters.FirstOrDefault(s => !s.IsFuture);

        foreach (var semester in data.Semesters)
        {
            if (string.Equals(semester.Id, semesterId, System.StringComparison.Ordinal))
                return semester;
        }

        return null;
    }
}