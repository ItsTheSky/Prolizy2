using System.ComponentModel;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Prolizy2.Model.Bulletin;
using Prolizy2.ViewModels.Bulletin;

namespace Prolizy2.Styles.Controls.Bulletin;

/// <summary>
/// Expandable card for a resource or a SAÉ — header shows the code, label and
/// average; tapping it reveals the evaluation list.
/// </summary>
[PseudoClasses(":dense")]
public class EvaluationGroupCard : TemplatedControl
{
    public static readonly StyledProperty<EvaluationGroupInfo?> GroupProperty =
        AvaloniaProperty.Register<EvaluationGroupCard, EvaluationGroupInfo?>(nameof(Group));

    public static readonly StyledProperty<bool> IsExpandedProperty =
        AvaloniaProperty.Register<EvaluationGroupCard, bool>(nameof(IsExpanded));

    private Border? _header;

    public EvaluationGroupInfo? Group
    {
        get => GetValue(GroupProperty);
        set => SetValue(GroupProperty, value);
    }

    public bool IsExpanded
    {
        get => GetValue(IsExpandedProperty);
        set => SetValue(IsExpandedProperty, value);
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        if (_header is not null)
            _header.Tapped -= OnHeaderTapped;

        _header = e.NameScope.Find<Border>("PART_Header");

        if (_header is not null)
            _header.Tapped += OnHeaderTapped;
    }

    private void OnHeaderTapped(object? sender, TappedEventArgs e) => IsExpanded = !IsExpanded;

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);

        var options = BulletinDisplayOptions.Instance;
        options.PropertyChanged += OnOptionsChanged;
        PseudoClasses.Set(":dense", options.IsCompact);
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        BulletinDisplayOptions.Instance.PropertyChanged -= OnOptionsChanged;
        base.OnDetachedFromVisualTree(e);
    }

    private void OnOptionsChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(BulletinDisplayOptions.Density))
            PseudoClasses.Set(":dense", BulletinDisplayOptions.Instance.IsCompact);
    }
}
