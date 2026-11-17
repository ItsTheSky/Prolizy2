using System;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Lucide.Avalonia;
using Prolizy2.Model.Bulletin;
using Prolizy2.Model.Home;
using Prolizy2.ViewModels.Bulletin;
using Prolizy2.ViewModels.Common;
using Prolizy2.ViewModels.Tabs;

namespace Prolizy2.ViewModels.Home;

public partial class HomeCardViewModel : ViewModelBase
{
    public HomeCardViewModel(MainViewModel mainVm, HomeCardConfig config)
    {
        MainVm = mainVm;
        Config = config;

        RebuildContent();
    }

    public MainViewModel MainVm { get; }

    public HomeCardConfig Config { get; }

    [ObservableProperty] private ViewModelBase? _content;

    [ObservableProperty] private string _title = string.Empty;

    [ObservableProperty] private string _hint = string.Empty;

    [ObservableProperty] private LucideIconKind _icon = LucideIconKind.Circle;

    [ObservableProperty] private string _accentHex = "#8C8C8C";

    public bool IsCompact => Config.IsCompact;

    public bool IsFullWidth => Config.IsFullWidth;

    private BulletinCoreViewModel? Bulletin => MainVm.BulletinViewModel;

    public void RebuildContent()
    {
        RefreshMetadata();
        Content = CreateContent();
    }

    private void RefreshMetadata()
    {
        switch (Config.Type)
        {
            case HomeCardType.NextCourse:
                Title = LangViewModel.Translate("HomeCardTitleNextCourse");
                Icon = LucideIconKind.CalendarClock;
                AccentHex = "#FF8C42";
                Hint = LangViewModel.Translate("HomeCardTypeNextCourse");
                break;

            case HomeCardType.BulletinOverview:
                Title = LangViewModel.Translate("HomeCardTitleOverview");
                Icon = LucideIconKind.GraduationCap;
                AccentHex = "#22C55E";
                Hint = string.Empty;
                break;

            case HomeCardType.EvaluationDetail:
                var group = FindGroup();
                Title = group is null
                    ? LangViewModel.Translate("HomeCardTitleEvaluation")
                    : string.IsNullOrEmpty(group.Label) ? group.Code : group.Label;
                Icon = group?.Kind == BulletinEntryKind.Sae ? LucideIconKind.WandSparkles : LucideIconKind.NotebookPen;
                AccentHex = "#10B981";
                Hint = EvaluationDetailCardViewModel.TranslateKind(group?.Kind);
                break;

            default:
                Title = ResolveUnitTitle();
                Icon = LucideIconKind.Layers;
                AccentHex = "#2563EB";
                Hint = LangViewModel.Translate("HomeCardTitleUnits");
                break;
        }
    }

    private ViewModelBase CreateContent() => Config.Type switch
    {
        HomeCardType.NextCourse => new NextCourseCardViewModel(MainVm, IsCompact),
        HomeCardType.BulletinOverview => new BulletinOverviewCardViewModel(
            Bulletin?.Data, Config.SemesterId, MainVm.HomeViewModel.GetSemesterData),
        HomeCardType.EvaluationDetail => new EvaluationDetailCardViewModel(FindGroup(), Config.MaxItems),
        _ => new UnitListCardViewModel(Bulletin?.Data, Config.UnitCode, Config.MaxItems)
    };

    [RelayCommand]
    private void Open()
    {
        var home = MainVm.HomeViewModel;

        switch (Content)
        {
            case NextCourseCardViewModel { NextItem: { } item }:
                home.OpenScheduleItem(item);
                break;

            case NextCourseCardViewModel:
                MainVm.NavigateTo(typeof(EdtViewModel));
                break;

            case BulletinOverviewCardViewModel overview:
                home.OpenBulletin(overview.ResolvedSemesterId);
                break;

            default:
                home.OpenBulletin(null);
                break;
        }
    }

    private EvaluationGroupInfo? FindGroup() =>
        Bulletin?.FindGroup(Config.GroupCode ?? string.Empty);

    private string ResolveUnitTitle()
    {
        if (string.IsNullOrEmpty(Config.UnitCode))
            return LangViewModel.Translate("HomeCardTitleUnits");

        var unit = Bulletin?.Data.Units
            .FirstOrDefault(u => string.Equals(u.Code, Config.UnitCode, StringComparison.OrdinalIgnoreCase));

        return unit?.Code ?? Config.UnitCode;
    }
}
