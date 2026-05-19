using System.Windows.Input;
using Avalonia;
using Avalonia.Controls.Primitives;

namespace Prolizy2.Styles.Controls.Bulletin;

/// <summary>
/// Page header shared by every bulletin variant: a title plus the download and
/// student-information actions.
/// </summary>
public class BulletinHeaderBar : TemplatedControl
{
    public static readonly StyledProperty<string> TitleProperty =
        AvaloniaProperty.Register<BulletinHeaderBar, string>(nameof(Title), "Bulletin");

    public static readonly StyledProperty<ICommand?> DownloadCommandProperty =
        AvaloniaProperty.Register<BulletinHeaderBar, ICommand?>(nameof(DownloadCommand));

    public static readonly StyledProperty<ICommand?> StudentCommandProperty =
        AvaloniaProperty.Register<BulletinHeaderBar, ICommand?>(nameof(StudentCommand));

    public string Title
    {
        get => GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public ICommand? DownloadCommand
    {
        get => GetValue(DownloadCommandProperty);
        set => SetValue(DownloadCommandProperty, value);
    }

    public ICommand? StudentCommand
    {
        get => GetValue(StudentCommandProperty);
        set => SetValue(StudentCommandProperty, value);
    }
}
