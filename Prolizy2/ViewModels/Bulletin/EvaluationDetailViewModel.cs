using Avalonia.Media;
using CommunityToolkit.Mvvm.Input;
using Prolizy2.Model.Bulletin;

namespace Prolizy2.ViewModels.Bulletin;

/// <summary>
/// Drill-down detail screen for a single resource or SAÉ — shows its average
/// and the full list of evaluations.
/// </summary>
public partial class EvaluationDetailViewModel : ViewModelBase
{
    private readonly BulletinCoreViewModel _core;

    public EvaluationGroupInfo Group { get; }

    /// <summary>Localised kind label — "Ressource" or "SAÉ".</summary>
    public string KindLabel { get; }

    public IBrush AccentBrush => Group.AccentBrush;

    public EvaluationDetailViewModel(BulletinCoreViewModel core, EvaluationGroupInfo group)
    {
        _core = core;
        Group = group;
        KindLabel = group.Kind == BulletinEntryKind.Sae ? "SAÉ" : "Ressource";
    }

    [RelayCommand]
    private void Back() => _core.PopDetail();
}
