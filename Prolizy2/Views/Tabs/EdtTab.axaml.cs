using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Prolizy2.Utilities.Gestures;
using Prolizy2.ViewModels.Tabs;

namespace Prolizy2.Views.Tabs;

public partial class EdtTab : UserControl
{
    public EdtTab()
    {
        InitializeComponent();

        DataContextChanged += OnDataContextChanged;
    }

    private void OnDataContextChanged(object? sender, EventArgs e)
    {
        if (DataContext is EdtViewModel viewModel)
        {
            var scheduleControl = this.Find<Control>("ScheduleControl");
            if (scheduleControl != null)
            {
                // Add swipe gesture recognizer
                var swipeRecognizer = new SwipeGestureRecognizer
                {
                    CanSwipeLeft = true,
                    CanSwipeRight = true,
                    SwipeThreshold = 50
                };

                scheduleControl.GestureRecognizers.Add(swipeRecognizer);

                // Handle swipe ended event
                scheduleControl.AddHandler(CustomGestures.SwipeGestureEndedEvent, (object? s, SwipeGestureEndedEventArgs args) =>
                {
                    if (args.Direction == SwipeDirection.Left)
                    {
                        // Swipe left = next day
                        viewModel.NextDayCommand.Execute(null);
                    }
                    else if (args.Direction == SwipeDirection.Right)
                    {
                        // Swipe right = previous day
                        viewModel.PreviousDayCommand.Execute(null);
                    }
                });
            }
        }
    }
}