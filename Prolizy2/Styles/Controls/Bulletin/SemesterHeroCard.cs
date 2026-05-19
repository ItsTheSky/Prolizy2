using Avalonia;
using Avalonia.Controls.Primitives;
using Prolizy2.Model.Bulletin;

namespace Prolizy2.Styles.Controls.Bulletin;

/// <summary>
/// Hero card summarising the semester: average, decision, rank and the count
/// of acquired teaching units. Shared by every bulletin variant.
/// </summary>
public class SemesterHeroCard : TemplatedControl
{
    public static readonly StyledProperty<BulletinData?> BulletinProperty =
        AvaloniaProperty.Register<SemesterHeroCard, BulletinData?>(nameof(Bulletin));

    public BulletinData? Bulletin
    {
        get => GetValue(BulletinProperty);
        set => SetValue(BulletinProperty, value);
    }
}
