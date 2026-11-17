using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;

namespace Prolizy2.Utilities;

/// <summary>
/// Attached behaviour that raises an <see cref="ICommand"/> when an element is
/// tapped.
///
/// It exists as a layout-neutral replacement for wrapping card content in a
/// <see cref="Button"/>: a Button measures its content with an unconstrained
/// width, which collapses star-sized <see cref="Grid"/> columns — wrapping text
/// then never wraps and trailing content (grades, chevrons) overflows the card.
/// A <see cref="Border"/> carrying this behaviour keeps the measure constraint
/// intact while still being tappable.
/// </summary>
public static class TappedCommand
{
    public static readonly AttachedProperty<ICommand?> CommandProperty =
        AvaloniaProperty.RegisterAttached<Control, ICommand?>("Command", typeof(TappedCommand));

    public static readonly AttachedProperty<object?> CommandParameterProperty =
        AvaloniaProperty.RegisterAttached<Control, object?>("CommandParameter", typeof(TappedCommand));

    static TappedCommand()
    {
        CommandProperty.Changed.AddClassHandler<Control>(OnCommandChanged);
    }

    public static void SetCommand(Control element, ICommand? value) =>
        element.SetValue(CommandProperty, value);

    public static ICommand? GetCommand(Control element) =>
        element.GetValue(CommandProperty);

    public static void SetCommandParameter(Control element, object? value) =>
        element.SetValue(CommandParameterProperty, value);

    public static object? GetCommandParameter(Control element) =>
        element.GetValue(CommandParameterProperty);

    private static void OnCommandChanged(Control element, AvaloniaPropertyChangedEventArgs e)
    {
        element.Tapped -= OnTapped;

        if (e.NewValue is ICommand)
            element.Tapped += OnTapped;
    }

    private static void OnTapped(object? sender, TappedEventArgs e)
    {
        if (sender is not Control element)
            return;

        var command = GetCommand(element);
        var parameter = GetCommandParameter(element);

        if (command?.CanExecute(parameter) != true)
            return;

        // Consumed: a TappedCommand nested in another one must not trigger both.
        e.Handled = true;
        command.Execute(parameter);
    }
}
