using CommunityToolkit.Mvvm.ComponentModel;

namespace Prolizy2.Model.Bulletin;

/// <summary>
/// A single graded evaluation inside a resource or a SAÉ.
/// </summary>
public partial class EvaluationInfo : ObservableObject
{
    [ObservableProperty] private string _name = string.Empty;

    [ObservableProperty] private GradeValue _grade = new();

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CoefficientLabel))]
    private double _coefficient = 1d;

    /// <summary>Coefficient suffix, e.g. <c>×1</c> or <c>×1,5</c>.</summary>
    public string CoefficientLabel => "×" + ContributionInfo.FormatCoefficient(Coefficient);
}
