using System;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Markup.Xaml;
using Prolizy2.ViewModels.Home;

namespace Prolizy2.Views.Home;

/// <summary>
/// Content template of a home card shell: picks the view matching the card's
/// inner view-model.
/// </summary>
public class HomeCardTemplate : MarkupExtension, IDataTemplate
{
    public override object ProvideValue(IServiceProvider serviceProvider) => this;

    public Control? Build(object? param) => param switch
    {
        NextCourseCardViewModel => new NextCourseCardView(),
        BulletinOverviewCardViewModel => new BulletinOverviewCardView(),
        EvaluationDetailCardViewModel => new EvaluationDetailCardView(),
        UnitListCardViewModel => new UnitListCardView(),
        _ => null
    };

    public bool Match(object? data) => data is NextCourseCardViewModel
        or BulletinOverviewCardViewModel
        or EvaluationDetailCardViewModel
        or UnitListCardViewModel;
}
