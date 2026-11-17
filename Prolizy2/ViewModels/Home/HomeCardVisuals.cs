using Avalonia.Media;
using Lucide.Avalonia;
using Prolizy2.Model.Home;

namespace Prolizy2.ViewModels.Home;

public static class HomeCardVisuals
{
    public static LucideIconKind Icon(HomeCardType type) => type switch
    {
        HomeCardType.NextCourse => LucideIconKind.CalendarClock,
        HomeCardType.BulletinOverview => LucideIconKind.GraduationCap,
        HomeCardType.EvaluationDetail => LucideIconKind.NotebookPen,
        _ => LucideIconKind.Layers
    };

    public static string AccentHex(HomeCardType type) => type switch
    {
        HomeCardType.NextCourse => "#FF8C42",
        HomeCardType.BulletinOverview => "#22C55E",
        HomeCardType.EvaluationDetail => "#10B981",
        _ => "#2563EB"
    };

    public static IBrush AccentBrush(HomeCardType type) => new SolidColorBrush(Color.Parse(AccentHex(type)));

    public static IBrush AccentTintBrush(HomeCardType type) =>
        new SolidColorBrush(Color.Parse(AccentHex(type)), 0.18);
}
