using System.ComponentModel;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Prolizy2.Model.Bulletin;
using Prolizy2.ViewModels.Bulletin;

namespace Prolizy2.Styles.Controls.Bulletin;

/// <summary>Interaction style of a <see cref="UnitCard"/>.</summary>
public enum UnitCardMode
{
    /// <summary>Tapping the card expands an inline detail panel (V1 / V2).</summary>
    Accordion,

    /// <summary>Tapping the card raises <see cref="UnitCard.Command"/> (V3 master list).</summary>
    Compact
}

/// <summary>
/// Card representing one teaching unit. It serves both the accordion variants
/// (inline expansion of the contribution list) and the drill-down master list
/// (navigation). Density and colour mode react to <see cref="BulletinDisplayOptions"/>.
/// </summary>
[PseudoClasses(":dense", ":grade-tone")]
public class UnitCard : TemplatedControl
{
    public static readonly StyledProperty<TeachingUnitInfo?> UnitProperty =
        AvaloniaProperty.Register<UnitCard, TeachingUnitInfo?>(nameof(Unit));

    public static readonly StyledProperty<UnitCardMode> DisplayModeProperty =
        AvaloniaProperty.Register<UnitCard, UnitCardMode>(nameof(DisplayMode), UnitCardMode.Accordion);

    public static readonly StyledProperty<bool> IsExpandedProperty =
        AvaloniaProperty.Register<UnitCard, bool>(nameof(IsExpanded));

    public static readonly StyledProperty<ICommand?> CommandProperty =
        AvaloniaProperty.Register<UnitCard, ICommand?>(nameof(Command));

    public static readonly StyledProperty<object?> CommandParameterProperty =
        AvaloniaProperty.Register<UnitCard, object?>(nameof(CommandParameter));

    // ---- Derived properties consumed by the template ----------------------

    public static readonly StyledProperty<bool> ShowChevronDownProperty =
        AvaloniaProperty.Register<UnitCard, bool>(nameof(ShowChevronDown), true);

    public static readonly StyledProperty<bool> ShowChevronUpProperty =
        AvaloniaProperty.Register<UnitCard, bool>(nameof(ShowChevronUp));

    public static readonly StyledProperty<bool> ShowChevronRightProperty =
        AvaloniaProperty.Register<UnitCard, bool>(nameof(ShowChevronRight));

    private Button? _header;

    static UnitCard()
    {
        DisplayModeProperty.Changed.AddClassHandler<UnitCard>((c, _) => c.RefreshChevrons());
        IsExpandedProperty.Changed.AddClassHandler<UnitCard>((c, _) => c.RefreshChevrons());
    }

    public UnitCard() => RefreshChevrons();

    public TeachingUnitInfo? Unit
    {
        get => GetValue(UnitProperty);
        set => SetValue(UnitProperty, value);
    }

    public UnitCardMode DisplayMode
    {
        get => GetValue(DisplayModeProperty);
        set => SetValue(DisplayModeProperty, value);
    }

    public bool IsExpanded
    {
        get => GetValue(IsExpandedProperty);
        set => SetValue(IsExpandedProperty, value);
    }

    /// <summary>Command raised when tapped in <see cref="UnitCardMode.Compact"/> mode.</summary>
    public ICommand? Command
    {
        get => GetValue(CommandProperty);
        set => SetValue(CommandProperty, value);
    }

    public object? CommandParameter
    {
        get => GetValue(CommandParameterProperty);
        set => SetValue(CommandParameterProperty, value);
    }

    public bool ShowChevronDown
    {
        get => GetValue(ShowChevronDownProperty);
        private set => SetValue(ShowChevronDownProperty, value);
    }

    public bool ShowChevronUp
    {
        get => GetValue(ShowChevronUpProperty);
        private set => SetValue(ShowChevronUpProperty, value);
    }

    public bool ShowChevronRight
    {
        get => GetValue(ShowChevronRightProperty);
        private set => SetValue(ShowChevronRightProperty, value);
    }

    private void RefreshChevrons()
    {
        var accordion = DisplayMode == UnitCardMode.Accordion;
        ShowChevronDown = accordion && !IsExpanded;
        ShowChevronUp = accordion && IsExpanded;
        ShowChevronRight = !accordion;
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        if (_header is not null)
            _header.Click -= OnHeaderClick;

        _header = e.NameScope.Find<Button>("PART_Header");

        if (_header is not null)
            _header.Click += OnHeaderClick;
    }

    private void OnHeaderClick(object? sender, RoutedEventArgs e)
    {
        if (DisplayMode == UnitCardMode.Accordion)
        {
            IsExpanded = !IsExpanded;
        }
        else if (Command?.CanExecute(CommandParameter) == true)
        {
            Command.Execute(CommandParameter);
        }
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);

        var options = BulletinDisplayOptions.Instance;
        options.PropertyChanged += OnOptionsChanged;
        ApplyOptionPseudoClasses(options);
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        BulletinDisplayOptions.Instance.PropertyChanged -= OnOptionsChanged;
        base.OnDetachedFromVisualTree(e);
    }

    private void OnOptionsChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName is nameof(BulletinDisplayOptions.Density)
            or nameof(BulletinDisplayOptions.ColorMode))
        {
            ApplyOptionPseudoClasses(BulletinDisplayOptions.Instance);
        }
    }

    private void ApplyOptionPseudoClasses(BulletinDisplayOptions options)
    {
        PseudoClasses.Set(":dense", options.IsCompact);
        PseudoClasses.Set(":grade-tone", options.IsColorByGrade);
    }
}
