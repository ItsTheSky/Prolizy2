using System.Collections.ObjectModel;
using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Prolizy2.Model.Bulletin;

/// <summary>
/// A resource or a SAÉ together with its list of evaluations.
/// </summary>
public partial class EvaluationGroupInfo : ObservableObject
{
    /// <summary>Code of the group (e.g. <c>R2.01</c> or <c>SAE2.01</c>).</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Kind))]
    [NotifyPropertyChangedFor(nameof(AccentBrush))]
    [NotifyPropertyChangedFor(nameof(AccentSoftBrush))]
    private string _code = string.Empty;

    [ObservableProperty] private string _label = string.Empty;

    [ObservableProperty] private GradeValue _average = new();

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasEvaluations))]
    [NotifyPropertyChangedFor(nameof(EvaluationCountLabel))]
    private ObservableCollection<EvaluationInfo> _evaluations = new();

    public BulletinEntryKind Kind => BulletinStatus.KindFromCode(Code);

    public IBrush AccentBrush => BulletinStatus.AccentFor(Kind);

    /// <summary>Translucent accent — used behind the code chip.</summary>
    public IBrush AccentSoftBrush => BulletinStatus.AccentSoftFor(Kind);

    public bool HasEvaluations => Evaluations.Count > 0;

    /// <summary>Pluralised count, e.g. <c>3 évaluations</c> / <c>Aucune évaluation</c>.</summary>
    public string EvaluationCountLabel => Evaluations.Count switch
    {
        0 => "Aucune évaluation",
        1 => "1 évaluation",
        var n => $"{n} évaluations"
    };
}
