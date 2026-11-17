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
        Icon = HomeCardVisuals.Icon(Config.Type);
        AccentHex = HomeCardVisuals.AccentHex(Config.Type);
        Hint = string.Empty;

        switch (Config.Type)
        {
            case HomeCardType.NextCourse:
                Title = LangViewModel.Translate("HomeCardTitleNextCourse");
                break;

            case HomeCardType.BulletinOverview:
                Title = LangViewModel.Translate("HomeCardTitleOverview");
                break;

            case HomeCardType.EvaluationDetail:
                var group = FindGroup();
                if (group is null)
                {
                    Title = LangViewModel.Translate("HomeCardTitleEvaluation");
                    break;
                }

                Title = string.IsNullOrEmpty(group.Label) ? group.Code : group.Label;
                Hint = EvaluationDetailCardViewModel.TranslateKind(group.Kind);
                if (group.Kind == BulletinEntryKind.Sae)
                    Icon = LucideIconKind.WandSparkles;
                break;

            default:
                var unit = FindUnit();
                Title = unit?.Code ?? LangViewModel.Translate("HomeCardTitleUnits");
                Hint = unit?.Name ?? string.Empty;
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

    private TeachingUnitInfo? FindUnit() =>
        string.IsNullOrEmpty(Config.UnitCode)
            ? null
            : Bulletin?.Data.Units.FirstOrDefault(u =>
                string.Equals(u.Code, Config.UnitCode, StringComparison.OrdinalIgnoreCase));
}
