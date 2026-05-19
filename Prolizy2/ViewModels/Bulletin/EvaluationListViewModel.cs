using System.Collections.ObjectModel;
using Avalonia.Media;
using CommunityToolkit.Mvvm.Input;
using Prolizy2.Model.Bulletin;

namespace Prolizy2.ViewModels.Bulletin;

/// <summary>
/// Drill-down index screen listing every resource or every SAÉ of the semester.
/// </summary>
public partial class EvaluationListViewModel : ViewModelBase
{
    private readonly BulletinCoreViewModel _core;

    public string Title { get; }

    public string Subtitle { get; }

    public ObservableCollection<EvaluationGroupInfo> Items { get; }

    public IBrush AccentBrush { get; }

    public EvaluationListViewModel(BulletinCoreViewModel core, string title, string subtitle,
        ObservableCollection<EvaluationGroupInfo> items, BulletinEntryKind kind)
    {
        _core = core;
        Title = title;
        Subtitle = subtitle;
        Items = items;
        AccentBrush = BulletinStatus.AccentFor(kind);
    }

    [RelayCommand]
    private void Back() => _core.PopDetail();

    [RelayCommand]
    private void OpenItem(EvaluationGroupInfo? group)
    {
        if (group is not null)
            _core.PushDetail(new EvaluationDetailViewModel(_core, group));
    }
}
