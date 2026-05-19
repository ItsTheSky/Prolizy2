using CommunityToolkit.Mvvm.Input;
using Prolizy2.Model.Bulletin;

namespace Prolizy2.ViewModels.Bulletin;

/// <summary>
/// Drill-down detail screen for a single teaching unit: the unit hero plus its
/// contributive resources, SAÉ and portfolio rows.
/// </summary>
public partial class UnitDetailViewModel : ViewModelBase
{
    private readonly BulletinCoreViewModel _core;

    public TeachingUnitInfo Unit { get; }

    public BulletinDisplayOptions Options => BulletinDisplayOptions.Instance;

    public UnitDetailViewModel(BulletinCoreViewModel core, TeachingUnitInfo unit)
    {
        _core = core;
        Unit = unit;
    }

    [RelayCommand]
    private void Back() => _core.PopDetail();

    [RelayCommand]
    private void OpenContribution(ContributionInfo? contribution)
    {
        if (contribution is null)
            return;

        var group = _core.FindGroup(contribution.Code);
        if (group is not null)
            _core.PushDetail(new EvaluationDetailViewModel(_core, group));
    }
}
