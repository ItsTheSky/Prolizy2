using System;
using System.Globalization;
using Avalonia;
using Avalonia.Animation;
using Avalonia.Animation.Easings;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Media.Transformation;
using Lucide.Avalonia;

namespace Prolizy2.Styles.Controls.Home;

/// <summary>
/// Two-option icon switch whose highlight slides under the selected icon.
/// </summary>
[TemplatePart("PART_Indicator", typeof(Border))]
[TemplatePart("PART_First", typeof(Control))]
[TemplatePart("PART_Second", typeof(Control))]
[PseudoClasses(":second")]
public class HomeSegmentToggle : TemplatedControl
{
    public static readonly StyledProperty<LucideIconKind> FirstIconProperty =
        AvaloniaProperty.Register<HomeSegmentToggle, LucideIconKind>(nameof(FirstIcon));

    public static readonly StyledProperty<LucideIconKind> SecondIconProperty =
        AvaloniaProperty.Register<HomeSegmentToggle, LucideIconKind>(nameof(SecondIcon));

    public static readonly StyledProperty<string?> FirstTipProperty =
        AvaloniaProperty.Register<HomeSegmentToggle, string?>(nameof(FirstTip));

    public static readonly StyledProperty<string?> SecondTipProperty =
        AvaloniaProperty.Register<HomeSegmentToggle, string?>(nameof(SecondTip));

    public static readonly StyledProperty<bool> IsSecondSelectedProperty =
        AvaloniaProperty.Register<HomeSegmentToggle, bool>(
            nameof(IsSecondSelected),
            defaultBindingMode: Avalonia.Data.BindingMode.TwoWay);

    private Border? _indicator;
    private Control? _first;
    private Control? _second;

    public LucideIconKind FirstIcon
    {
        get => GetValue(FirstIconProperty);
        set => SetValue(FirstIconProperty, value);
    }

    public LucideIconKind SecondIcon
    {
        get => GetValue(SecondIconProperty);
        set => SetValue(SecondIconProperty, value);
    }

    public string? FirstTip
    {
        get => GetValue(FirstTipProperty);
        set => SetValue(FirstTipProperty, value);
    }

    public string? SecondTip
    {
        get => GetValue(SecondTipProperty);
        set => SetValue(SecondTipProperty, value);
    }

    public bool IsSecondSelected
    {
        get => GetValue(IsSecondSelectedProperty);
        set => SetValue(IsSecondSelectedProperty, value);
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        if (_first is not null)
            _first.Tapped -= OnFirstTapped;
        if (_second is not null)
            _second.Tapped -= OnSecondTapped;

        _indicator = e.NameScope.Find<Border>("PART_Indicator");
        _first = e.NameScope.Find<Control>("PART_First");
        _second = e.NameScope.Find<Control>("PART_Second");

        if (_first is not null)
            _first.Tapped += OnFirstTapped;
        if (_second is not null)
            _second.Tapped += OnSecondTapped;

        PseudoClasses.Set(":second", IsSecondSelected);
    }

    private void OnFirstTapped(object? sender, TappedEventArgs e) => SetCurrentValue(IsSecondSelectedProperty, false);

    private void OnSecondTapped(object? sender, TappedEventArgs e) => SetCurrentValue(IsSecondSelectedProperty, true);

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == IsSecondSelectedProperty)
        {
            PseudoClasses.Set(":second", IsSecondSelected);
            UpdateIndicator();
        }
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        var size = base.ArrangeOverride(finalSize);
        UpdateIndicator();
        return size;
    }

    private void UpdateIndicator()
    {
        if (_indicator is null || _first is null || _second is null)
            return;

        var offset = IsSecondSelected ? _second.Bounds.X - _first.Bounds.X : 0d;
        _indicator.RenderTransform = TransformOperations.Parse(
            string.Create(CultureInfo.InvariantCulture, $"translateX({offset}px)"));

        // Set after the first placement so the initial state does not slide in.
        _indicator.Transitions ??= new Transitions
        {
            new TransformOperationsTransition
            {
                Property = RenderTransformProperty,
                Duration = TimeSpan.FromMilliseconds(220),
                Easing = new CubicEaseOut()
            }
        };
    }
}
