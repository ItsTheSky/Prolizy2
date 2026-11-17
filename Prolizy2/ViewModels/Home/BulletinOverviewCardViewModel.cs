using System;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using Prolizy2.Model.Bulletin;
using Prolizy2.ViewModels.Common;

namespace Prolizy2.ViewModels.Home;

public partial class BulletinOverviewCardViewModel : ViewModelBase
{
    // The bulletin tab data only holds the transcript of the semester selected
    // there, so the card's own semester is resolved separately.
    public BulletinOverviewCardViewModel(
        BulletinData? bulletinData,
        string? semesterId,
        Func<SemesterInfo, BulletinData?> resolveSemesterData)
    {
        SemesterId = semesterId;

        var semester = bulletinData is null ? null : FindSemester(bulletinData, semesterId);
        ResolvedSemesterId = semester?.Id;

        Apply(semester is null ? null : resolveSemesterData(semester), semester);
    }

    public string? SemesterId { get; }

    public string? ResolvedSemesterId { get; }

    [ObservableProperty] private string _semesterLabel = string.Empty;

    [ObservableProperty] private GradeValue? _average;

    [ObservableProperty] private int _acquiredUnits;

    [ObservableProperty] private int _totalUnits;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(DecisionText))]
    [NotifyPropertyChangedFor(nameof(DecisionBadgeType))]
    private string _decision = "—";

    [ObservableProperty] private ObservableCollection<TeachingUnitInfo> _acquiredList = new();

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasNoData))]
    private bool _hasData;

    public bool HasNoData => !HasData;

    public string DecisionText => Decision switch
    {
        "ADM" => LangViewModel.Translate("HomeCardDecisionAdmitted"),
        "AJ" => LangViewModel.Translate("HomeCardDecisionDeferred"),
        "DEF" => LangViewModel.Translate("HomeCardDecisionFailed"),
        _ => Decision
    };

    public global::Prolizy2.Styles.Controls.BadgeType DecisionBadgeType =>
        BulletinStatus.ToBadgeType(Decision);

    private void Apply(BulletinData? data, SemesterInfo? semester)
    {
        if (data is null || semester is null || data.Units.Count == 0)
        {
            HasData = false;
            return;
        }

        SemesterLabel = $"{semester.Code} · {semester.Year}";
        Average = data.SemesterAverage;
        AcquiredUnits = data.AcquiredUnitCount;
        TotalUnits = data.TotalUnitCount;
        Decision = data.Decision;
        AcquiredList = new ObservableCollection<TeachingUnitInfo>(data.Units.Where(u => u.IsAcquired));
        HasData = true;
    }

    private static SemesterInfo? FindSemester(BulletinData data, string? semesterId)
    {
        if (string.IsNullOrEmpty(semesterId))
            return data.Semesters.FirstOrDefault(s => s.IsCurrent)
                   ?? data.Semesters.FirstOrDefault(s => !s.IsFuture);

        return data.Semesters.FirstOrDefault(s => string.Equals(s.Id, semesterId, StringComparison.Ordinal));
    }
}
