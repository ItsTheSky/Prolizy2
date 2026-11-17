using System;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using Prolizy2.Model.Bulletin;
using Prolizy2.Utilities;
using Prolizy2.ViewModels.Common;

namespace Prolizy2.ViewModels.Tabs;

/// <summary>
/// Card showing the grades of one resource or one SAÉ: its average plus the
/// list of its evaluations.
/// </summary>
public partial class EvaluationDetailCardViewModel : ViewModelBase
{
    public EvaluationDetailCardViewModel(EvaluationGroupInfo? group, int maxItems)
    {
        MaxItems = maxItems <= 0 ? 4 : maxItems;
        Apply(group);
    }

    /// <summary>Maximum number of evaluations listed before the card truncates.</summary>
    public int MaxItems { get; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasGroup))]
    [NotifyPropertyChangedFor(nameof(HasNoGroup))]
    private EvaluationGroupInfo? _group;

    [ObservableProperty] private GradeValue? _average;

    [ObservableProperty] private ObservableCollection<EvaluationInfo> _evaluations = new();

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(MoreCountLabel))]
    [NotifyPropertyChangedFor(nameof(HasMore))]
    private int _hiddenCount;

    /// <summary>Code of the group (e.g. <c>R2.01</c>, <c>SAE2.01</c>).</summary>
    public string Code => Group?.Code ?? string.Empty;

    public string Label => Group?.Label ?? string.Empty;

    public bool HasGroup => Group is not null;

    public bool HasNoGroup => Group is null;

    public bool HasMore => HiddenCount > 0;

    public string MoreCountLabel => string.Format(
        System.Globalization.CultureInfo.CurrentCulture,
        LangViewModel.Translate("HomeCardMoreEvaluations"), HiddenCount);

    /// <summary>Kind label — "Ressource" / "SAÉ".</summary>
    public string KindLabel => Group?.Kind switch
    {
        BulletinEntryKind.Sae => LangViewModel.Translate("HomeCardKindSae"),
        BulletinEntryKind.Portfolio => LangViewModel.Translate("HomeCardKindPortfolio"),
        _ => LangViewModel.Translate("HomeCardKindResource")
    };

    public void Apply(EvaluationGroupInfo? group)
    {
        if (group is null)
        {
            Group = null;
            Average = null;
            Evaluations = new ObservableCollection<EvaluationInfo>();
            HiddenCount = 0;
            return;
        }

        Group = group;
        Average = group.Average;

        var all = group.Evaluations;
        var shown = Math.Min(all.Count, MaxItems);
        var visible = new EvaluationInfo[shown];
        for (var i = 0; i < shown; i++)
            visible[i] = all[i];

        Evaluations = new ObservableCollection<EvaluationInfo>(visible);
        HiddenCount = all.Count - shown;

        OnPropertyChanged(nameof(Code));
        OnPropertyChanged(nameof(Label));
        OnPropertyChanged(nameof(KindLabel));
    }
}