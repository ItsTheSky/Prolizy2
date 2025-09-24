using Avalonia.Interactivity;

namespace Prolizy2.Utilities.Gestures;
public static class CustomGestures
{
    public static readonly RoutedEvent<SwipeGestureEventArgs> SwipeGestureEvent =
        RoutedEvent.Register<SwipeGestureEventArgs>(
            "Swipe", RoutingStrategies.Bubble, typeof(CustomGestures));

    public static readonly RoutedEvent<SwipeGestureEndedEventArgs> SwipeGestureEndedEvent =
        RoutedEvent.Register<SwipeGestureEndedEventArgs>(
            "SwipeEnded", RoutingStrategies.Bubble, typeof(CustomGestures));
}
