using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Prolizy2.Model;

namespace Prolizy2.Styles.Controls.Edt;

public class ItemDialogOverlay : TemplatedControl
{
    
    public static readonly StyledProperty<ScheduleItem> ItemProperty =
        AvaloniaProperty.Register<ItemDialogOverlay, ScheduleItem>(nameof(Item));
    
    public ScheduleItem Item
    {
        get => GetValue(ItemProperty);
        set => SetValue(ItemProperty, value);
    }
    
}