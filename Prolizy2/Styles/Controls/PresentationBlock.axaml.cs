using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Lucide.Avalonia;

namespace Prolizy2.Styles.Controls;

public class PresentationBlock : ContentControl
{
    public static readonly StyledProperty<string> TitleProperty =
        AvaloniaProperty.Register<PresentationBlock, string>(nameof(Title));

    public static readonly StyledProperty<string?> DescriptionProperty =
        AvaloniaProperty.Register<PresentationBlock, string?>(nameof(Description));

    public static readonly StyledProperty<LucideIconKind> IconProperty =
        AvaloniaProperty.Register<PresentationBlock, LucideIconKind>(nameof(Icon), LucideIconKind.OctagonAlert);

    public static readonly StyledProperty<ICommand?> ButtonCommandProperty =
        AvaloniaProperty.Register<PresentationBlock, ICommand?>(nameof(ButtonCommand));

    public static readonly StyledProperty<string?> ButtonTextProperty =
        AvaloniaProperty.Register<PresentationBlock, string?>(nameof(ButtonText));

    public static readonly StyledProperty<LucideIconKind?> ButtonIconProperty =
        AvaloniaProperty.Register<PresentationBlock, LucideIconKind?>(nameof(ButtonIcon));

    public string Title
    {
        get => GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public string? Description
    {
        get => GetValue(DescriptionProperty);
        set => SetValue(DescriptionProperty, value);
    }

    public LucideIconKind Icon
    {
        get => GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    public ICommand? ButtonCommand
    {
        get => GetValue(ButtonCommandProperty);
        set => SetValue(ButtonCommandProperty, value);
    }

    public string? ButtonText
    {
        get => GetValue(ButtonTextProperty);
        set => SetValue(ButtonTextProperty, value);
    }

    public LucideIconKind? ButtonIcon
    {
        get => GetValue(ButtonIconProperty);
        set => SetValue(ButtonIconProperty, value);
    }
}