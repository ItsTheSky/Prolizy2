using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Lucide.Avalonia;

namespace Prolizy2.Styles.Controls;

public class ModuleCard : ContentControl
{
    
    public static readonly StyledProperty<string> TitleProperty =
        AvaloniaProperty.Register<ModuleCard, string>(nameof(Title));
    
    public static readonly StyledProperty<LucideIconKind> IconProperty =
        AvaloniaProperty.Register<ModuleCard, LucideIconKind>(nameof(Icon));
    
    public string Title
    {
        get => GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }
    
    public LucideIconKind Icon
    {
        get => GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }
    
}