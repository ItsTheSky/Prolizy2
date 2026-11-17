using System;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using Prolizy2.Model.Home;
using Prolizy2.ViewModels.Bulletin;
using Prolizy2.ViewModels.Common;
using Prolizy2.ViewModels.Tabs;

namespace Prolizy2.ViewModels.Tabs;

/// <summary>
/// One home card: wraps the configuration with the presentation data every kind
/// of card needs (title, icon, accents) and the view-model of its own kind.
/// </summary>
public partial class HomeCardViewModel : ViewModelBase
{
    public HomeCardViewModel(MainViewModel mainVm, HomeCardConfig config)
    {
        MainVm = mainVm;
        Config = config;

        RefreshMetadata();
        RebuildContent();
    }

    public MainViewModel MainVm { get; }

    /// <summary>The home view-model, used for navigation (open a course, open the bulletin).</summary>
    public HomeViewModel Home => MainVm.HomeViewModel;

    /// <summary>The card configuration (type, size, options).</summary>
    public HomeCardConfig Config { get; }

    /// <summary>View-model of the specific kind, used by the card views.</summary>
    [ObservableProperty] private ViewModelBase? _content;

    /// <summary>Header title, already translated.</summary>
    [ObservableProperty] private string _title = string.Empty;

    /// <summary>Second header line (kind of resource, type of card…).</summary>
    public string Hint { get; private set; } = string.Empty;

    /// <summary>Lucide icon key shown in the header.</summary>
    [ObservableProperty] private string _icon = "Circle";

    /// <summary>Accent colour of the card, as a hex string.</summary>
    [ObservableProperty] private string _accentHex = "#8C8C8C";

    /// <summary>Header layout variant (classic / compact).</summary>
    public bool IsCompact => Config.IsCompact;

    /// <summary>True when the card spans the whole row.</summary>
    public bool IsFullWidth => Config.IsFullWidth;

    /// <summary>True when the card has no data yet (bulletin not loaded, no course…).</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasNoData))]
    private bool _isEmpty;

    public bool HasNoData => !IsEmpty;

    /// <summary>
    /// Refreshes the title / icon / accent from the configuration.
    /// </summary>
    private void RefreshMetadata()
    {
        switch (Config.Type)
        {
            case HomeCardType.NextCourse:
                Title = LangViewModel.Translate("HomeCardTitleNextCourse");
                Icon = "CalendarClock";
                AccentHex = "#FF8C42";
                Hint = LangViewModel.Translate("HomeCardTypeNextCourse");
                break;

            case HomeCardType.BulletinOverview:
                Title = LangViewModel.Translate("HomeCardTitleOverview");
                Icon = "GraduationCap";
                AccentHex = "#22C55E";
                Hint = string.Empty;
                break;

            case HomeCardType.EvaluationDetail:
                Title = ResolveEvaluationTitle();
                Icon = KindIcon();
                AccentHex = "#10B981";
                Hint = ResolveKindLabel();
                break;

            default:
                Title = ResolveUnitTitle();
                Icon = "Layers";
                AccentHex = "#2563EB";
                Hint = LangViewModel.Translate("HomeCardTitleUnits");
                break;
        }
    }

    /// <summary>Creates the inner view-model of the card from its configuration.</summary>
    public ViewModelBase CreateContent() => Config.Type switch
    {
        HomeCardType.NextCourse => new NextCourseCardViewModel(MainVm, IsCompact),
        HomeCardType.BulletinOverview =>
            new BulletinOverviewCardViewModel(Bulletin?.Data, Config.SemesterId),
        HomeCardType.EvaluationDetail => new EvaluationDetailCardViewModel(FindGroup(), Config.MaxItems),
        _ => new UnitListCardViewModel(Bulletin?.Data, Config.UnitCode, Config.MaxItems)
    };

    /// <summary>Rebuilds the inner view-model — called when the bulletin data is replaced.</summary>
    public void RebuildContent()
    {
        RefreshMetadata();
        Content = CreateContent();

        IsEmpty = Content switch
        {
            NextCourseCardViewModel next => !next.HasCourse,
            BulletinOverviewCardViewModel overview => !overview.HasData,
            EvaluationDetailCardViewModel evaluation => !evaluation.HasGroup,
            UnitListCardViewModel units => !units.HasUnits,
            _ => false
        };

    }

    /// <summary>
    /// The bulletin view-model, or null while it is still being created: the
    /// bulletin cards then show their empty state until the next refresh.
    /// </summary>
    private BulletinCoreViewModel? Bulletin => MainVm.BulletinViewModel;

    private Prolizy2.Model.Bulletin.EvaluationGroupInfo? FindGroup() =>
        Bulletin?.FindGroup(Config.GroupCode ?? string.Empty);

    private string ResolveEvaluationTitle()
    {
        var group = FindGroup();
        if (group is null)
            return LangViewModel.Translate("HomeCardTitleEvaluation");

        return string.IsNullOrEmpty(group.Label) ? group.Code : group.Label;
    }

    private string ResolveUnitTitle()
    {
        if (string.IsNullOrEmpty(Config.UnitCode))
            return LangViewModel.Translate("HomeCardTitleUnits");

        var unit = Bulletin?.Data.Units
            .FirstOrDefault(u => string.Equals(u.Code, Config.UnitCode, StringComparison.OrdinalIgnoreCase));

        return unit?.Code ?? Config.UnitCode;
    }

    private string ResolveKindLabel() => FindGroup()?.Kind switch
    {
        Prolizy2.Model.Bulletin.BulletinEntryKind.Sae => LangViewModel.Translate("HomeCardKindSae"),
        Prolizy2.Model.Bulletin.BulletinEntryKind.Portfolio => LangViewModel.Translate("HomeCardKindPortfolio"),
        _ => LangViewModel.Translate("HomeCardKindResource")
    };

    private string KindIcon() =>
        FindGroup()?.Kind == Prolizy2.Model.Bulletin.BulletinEntryKind.Sae ? "WandSparkles" : "NotebookPen";
}