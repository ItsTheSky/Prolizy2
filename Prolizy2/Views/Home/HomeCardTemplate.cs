using System;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Markup.Xaml;
using Lucide.Avalonia;
using Prolizy2.ViewModels;
using Prolizy2.ViewModels.Tabs;
using Prolizy2.Views.Tabs;

namespace Prolizy2.Views.Home;

/// <summary>
/// Item template of the home card list: it builds the view of the card's inner
/// view-model and wraps it in a <see cref="Styles.Controls.Home.HomeCardShell"/>
/// driven by the card itself (title, icon, accent, compact variant).
/// </summary>
public class HomeCardTemplate : MarkupExtension, IDataTemplate
{
    public override object ProvideValue(IServiceProvider serviceProvider) => this;

    private static Control? BuildView(HomeCardViewModel? card) => card?.Content switch
    {
        NextCourseCardViewModel => new NextCourseCardView(),
        BulletinOverviewCardViewModel => new BulletinOverviewCardView(),
        EvaluationDetailCardViewModel => new EvaluationDetailCardView(),
        UnitListCardViewModel => new UnitListCardView(),
        _ => null
    };

    private static LucideIconKind ToIconKind(string key) =>
        Enum.TryParse<LucideIconKind>(key, out var kind) ? kind : LucideIconKind.Circle;

    public Control? Build(object? param)
    {
        if (param is not HomeCardViewModel card)
            return null;

        var view = BuildView(card);
        if (view is null)
            return null;

        // The card views are declared with x:DataType on their own view-model,
        // which reaches the navigation commands through its parent card.
        view.DataContext = card.Content;

        return new Styles.Controls.Home.HomeCardShell
        {
            Title = card.Title,
            Hint = card.Hint,
            Icon = ToIconKind(card.Icon),
            AccentHex = card.AccentHex,
            IsCompact = card.IsCompact,
            Content = view
        };
    }

    public bool Match(object? data) => data is HomeCardViewModel card && BuildView(card) is not null;
}