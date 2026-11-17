using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using CommunityToolkit.Mvvm.Input;

namespace Prolizy2.Styles.Controls.Settings;

public class SettingSubPage : ContentControl
{
    
    public static readonly StyledProperty<string> TitleProperty =
        AvaloniaProperty.Register<SettingSubPage, string>(nameof(Title), string.Empty);
    
    public static readonly StyledProperty<IRelayCommand?> BackCommandProperty =
        AvaloniaProperty.Register<SettingSubPage, IRelayCommand?>(nameof(BackCommand));

    /// <summary>Extra buttons shown next to "Retour", outside the scrolled content.</summary>
    public static readonly StyledProperty<object?> ActionsProperty =
        AvaloniaProperty.Register<SettingSubPage, object?>(nameof(Actions));


    public string Title
    {
        get => GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }
    
    public IRelayCommand? BackCommand
    {
        get => GetValue(BackCommandProperty);
        set => SetValue(BackCommandProperty, value);
    }

    public object? Actions
    {
        get => GetValue(ActionsProperty);
        set => SetValue(ActionsProperty, value);
    }
    
}