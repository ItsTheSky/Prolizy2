using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Prolizy2.Model.Bulletin;

/// <summary>
/// The complete data set for one semester bulletin. This is the model the
/// hosting application feeds with real ScoDoc data — every collection and
/// scalar is observable so the views update automatically.
/// </summary>
public partial class BulletinData : ObservableObject
{
    [ObservableProperty] private StudentInfo _student = new();

    [ObservableProperty] private ObservableCollection<SemesterInfo> _semesters = new();

    [ObservableProperty] private ObservableCollection<CompetenceInfo> _competences = new();

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(SemesterAverage))]
    [NotifyPropertyChangedFor(nameof(AcquiredUnitCount))]
    [NotifyPropertyChangedFor(nameof(TotalUnitCount))]
    [NotifyPropertyChangedFor(nameof(AcquiredUnitsLabel))]
    private ObservableCollection<TeachingUnitInfo> _units = new();

    [ObservableProperty] private ObservableCollection<EvaluationGroupInfo> _resources = new();

    [ObservableProperty] private ObservableCollection<EvaluationGroupInfo> _saes = new();

    /// <summary>Overall rank of the student within the promotion.</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(RankLabel))]
    private int _rank;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(RankLabel))]
    private int _total;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(DecisionBadgeText))]
    private string _decision = "ADM";

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(DecisionBadgeText))]
    private string _decisionLabel = "Admis";

    /// <summary>Simple mean of the numeric unit averages — the "Moyenne semestre".</summary>
    public GradeValue SemesterAverage
    {
        get
        {
            var numeric = Units
                .Select(u => u.Average)
                .Where(g => g.IsNumeric)
                .Select(g => g.NumericValue!.Value)
                .ToList();

            return numeric.Count == 0
                ? GradeValue.Text("—")
                : GradeValue.Numeric(numeric.Average());
        }
    }

    public int AcquiredUnitCount => Units.Count(u => u.IsAcquired);

    public int TotalUnitCount => Units.Count;

    public string AcquiredUnitsLabel => $"{AcquiredUnitCount}/{TotalUnitCount}";

    public string RankLabel => $"Rang {Rank}/{Total}";

    public string DecisionBadgeText => $"{Decision} · {DecisionLabel}";
}
