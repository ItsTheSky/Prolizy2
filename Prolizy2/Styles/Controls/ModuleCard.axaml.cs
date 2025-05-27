using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using IconPacks.Avalonia.Lucide;

namespace Prolizy2.Styles.Controls;

public class ModuleCard : ContentControl
{
    
    public static readonly StyledProperty<string> TitleProperty =
        AvaloniaProperty.Register<ModuleCard, string>(nameof(Title));
    
    public static readonly StyledProperty<PackIconLucideKind> IconProperty =
        AvaloniaProperty.Register<ModuleCard, PackIconLucideKind>(nameof(Icon));
    
    public string Title
    {
        get => GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }
    
    public PackIconLucideKind Icon
    {
        get => GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }
    
}