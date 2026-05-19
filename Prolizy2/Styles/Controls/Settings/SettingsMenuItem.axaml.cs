using System.Windows.Input;
using Avalonia;
using Avalonia.Controls.Primitives;
using CommunityToolkit.Mvvm.Input;
using Lucide.Avalonia;

namespace Prolizy2.Styles.Controls.Settings;

public class SettingsMenuItem : TemplatedControl
{
    public static readonly StyledProperty<string> HeaderProperty =
        AvaloniaProperty.Register<SettingsMenuItem, string>(nameof(Header));

    public static readonly StyledProperty<LucideIconKind?> IconProperty =
        AvaloniaProperty.Register<SettingsMenuItem, LucideIconKind?>(nameof(Icon));

    public static readonly StyledProperty<IRelayCommand?> CommandProperty =
        AvaloniaProperty.Register<SettingsMenuItem, IRelayCommand?>(nameof(Command));
    
    public static readonly StyledProperty<bool> HasSoonBadgeProperty =
        AvaloniaProperty.Register<SettingsMenuItem, bool>(nameof(HasSoonBadge), false);


    public string Header
    {
        get => GetValue(HeaderProperty);
        set => SetValue(HeaderProperty, value);
    }
    
    public LucideIconKind? Icon
    {
        get => GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }
    
    public IRelayCommand? Command
    {
        get => GetValue(CommandProperty);
        set => SetValue(CommandProperty, value);
    }
    
    public bool HasSoonBadge
    {
        get => GetValue(HasSoonBadgeProperty);
        set => SetValue(HasSoonBadgeProperty, value);
    }
}