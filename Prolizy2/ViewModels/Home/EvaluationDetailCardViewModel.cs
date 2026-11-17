using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using Prolizy2.Model.Bulletin;
using Prolizy2.ViewModels.Common;

namespace Prolizy2.ViewModels.Home;

public partial class EvaluationDetailCardViewModel : ViewModelBase
{
    public EvaluationDetailCardViewModel(EvaluationGroupInfo? group, int maxItems)
    {
        MaxItems = maxItems <= 0 ? 4 : maxItems;
        Apply(group);
    }

    public int MaxItems { get; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasGroup))]
    [NotifyPropertyChangedFor(nameof(HasNoGroup))]
    [NotifyPropertyChangedFor(nameof(Code))]
    [NotifyPropertyChangedFor(nameof(Label))]
    [NotifyPropertyChangedFor(nameof(KindLabel))]
    private EvaluationGroupInfo? _group;

    [ObservableProperty] private GradeValue? _average;

    [ObservableProperty] private ObservableCollection<EvaluationInfo> _evaluations = new();

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(MoreCountLabel))]
    [NotifyPropertyChangedFor(nameof(HasMore))]
    private int _hiddenCount;

    public string Code => Group?.Code ?? string.Empty;

    public string Label => Group?.Label ?? string.Empty;

    public bool HasGroup => Group is not null;

    public bool HasNoGroup => Group is null;

    public bool HasMore => HiddenCount > 0;

    public string MoreCountLabel => string.Format(
        CultureInfo.CurrentCulture,
        LangViewModel.Translate("HomeCardMoreEvaluations"), HiddenCount);

    public string KindLabel => TranslateKind(Group?.Kind);

    public static string TranslateKind(BulletinEntryKind? kind) => kind switch
    {
        BulletinEntryKind.Sae => LangViewModel.Translate("HomeCardKindSae"),
        BulletinEntryKind.Portfolio => LangViewModel.Translate("HomeCardKindPortfolio"),
        _ => LangViewModel.Translate("HomeCardKindResource")
    };

    private void Apply(EvaluationGroupInfo? group)
    {
        Group = group;

        if (group is null)
            return;

        Average = group.Average;

        var shown = Math.Min(group.Evaluations.Count, MaxItems);
        Evaluations = new ObservableCollection<EvaluationInfo>(group.Evaluations.Take(shown));
        HiddenCount = group.Evaluations.Count - shown;
    }
}
