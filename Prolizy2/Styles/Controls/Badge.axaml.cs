using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Reactive;
using Lucide.Avalonia;

namespace Prolizy2.Styles.Controls;

public class Badge : TemplatedControl
{
    
    public static readonly StyledProperty<string> TextProperty =
        AvaloniaProperty.Register<Badge, string>(nameof(Text));
    
    public static readonly StyledProperty<LucideIconKind?> IconProperty =
        AvaloniaProperty.Register<Badge, LucideIconKind?>(nameof(Icon));
    
    public static readonly StyledProperty<BadgeType> BadgeTypeProperty =
        AvaloniaProperty.Register<Badge, BadgeType>(nameof(BadgeType), defaultValue: BadgeType.Default);

    #region Class Region

    public static readonly StyledProperty<bool> IsWarningProperty =
        AvaloniaProperty.Register<Badge, bool>(nameof(IsWarning), defaultValue: false);
    
    public static readonly StyledProperty<bool> IsErrorProperty =
        AvaloniaProperty.Register<Badge, bool>(nameof(IsError), defaultValue: false);
    
    public static readonly StyledProperty<bool> IsSuccessProperty =
        AvaloniaProperty.Register<Badge, bool>(nameof(IsSuccess), defaultValue: false);
    
    public static readonly StyledProperty<bool> IsInfoProperty =
        AvaloniaProperty.Register<Badge, bool>(nameof(IsInfo), defaultValue: false);
    
    public static readonly StyledProperty<bool> IsNeutralProperty =
        AvaloniaProperty.Register<Badge, bool>(nameof(IsNeutral), defaultValue: false);
    
    public bool IsWarning
    {
        get => GetValue(IsWarningProperty);
        set => SetValue(IsWarningProperty, value);
    }
    
    public bool IsError
    {
        get => GetValue(IsErrorProperty);
        set => SetValue(IsErrorProperty, value);
    }
    
    public bool IsSuccess
    {
        get => GetValue(IsSuccessProperty);
        set => SetValue(IsSuccessProperty, value);
    }
    
    public bool IsInfo
    {
        get => GetValue(IsInfoProperty);
        set => SetValue(IsInfoProperty, value);
    }
    
    public bool IsNeutral
    {
        get => GetValue(IsNeutralProperty);
        set => SetValue(IsNeutralProperty, value);
    }

    #endregion

    public string Text
    {
        get => GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }
    
    public LucideIconKind? Icon
    {
        get => GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }
    
    public BadgeType BadgeType
    {
        get => GetValue(BadgeTypeProperty);
        set => SetValue(BadgeTypeProperty, value);
    }

    static Badge()
    {
        AffectsRender<Badge>(TextProperty);
        AffectsRender<Badge>(IconProperty);
        AffectsRender<Badge>(BadgeTypeProperty);

        AffectsMeasure<Badge>(IsWarningProperty);
        AffectsMeasure<Badge>(IsErrorProperty);
        AffectsMeasure<Badge>(IsSuccessProperty);
        AffectsMeasure<Badge>(IsInfoProperty);
        AffectsMeasure<Badge>(IsNeutralProperty);
        
        
        AffectsMeasure<Badge>(TextProperty);
        AffectsMeasure<Badge>(IconProperty);
        AffectsMeasure<Badge>(BadgeTypeProperty);
        
        AffectsRender<Badge>(IsWarningProperty);
        AffectsRender<Badge>(IsErrorProperty);
        AffectsRender<Badge>(IsSuccessProperty);
        AffectsRender<Badge>(IsInfoProperty);
        AffectsRender<Badge>(IsNeutralProperty);

        BadgeTypeProperty.Changed.Subscribe(new AnonymousObserver<AvaloniaPropertyChangedEventArgs<BadgeType>>(next =>
        {
            var badge = (Badge) next.Sender;
            var newValue = next.NewValue.GetValueOrDefault();
            badge.IsWarning = newValue == BadgeType.Warning;
            badge.IsError = newValue == BadgeType.Error;
            badge.IsSuccess = newValue == BadgeType.Success;
            badge.IsInfo = newValue == BadgeType.Info;
            badge.IsNeutral = newValue == BadgeType.Neutral;
        }));
    }
}

public enum BadgeType
{
    Default,
    Warning,
    Error,
    Success,
    Info,
    Neutral
}