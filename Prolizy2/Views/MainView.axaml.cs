using System;
using Avalonia.Controls;
using Avalonia.Input;
using Prolizy2.ViewModels;
using SukiUI.Dialogs;
using Vector = Avalonia.Vector;

namespace Prolizy2.Views;

public partial class MainView : UserControl
{
    public static ISukiDialogManager DialogManager { get; private set; }
    private Vector? lastScrollVector;

    public MainView()
    {
        InitializeComponent();

        DialogManager = new SukiDialogManager();
        DialogHost.Manager = DialogManager;

        Loaded += (sender, args) =>
        {
            MainBorder.AddHandler(Gestures.ScrollGestureEvent, (o, args) => lastScrollVector = args.Delta);
            MainBorder.AddHandler(Gestures.ScrollGestureEndedEvent, (o, args) =>
            {
                if (args.Handled || lastScrollVector == null)
                    return;

                var x = lastScrollVector.Value.X;
                (DataContext as MainViewModel)!.Vector = x.ToString();

                // let a threshold of 4 to avoid accidental scroll
                if (x == 0 || Math.Abs(x) < 3)
                    return;

                if (x > 0) // scroll on right
                    NextTab();
                else
                    PrevTab();
            });
        };
    }

    private void NextTab()
    {
        if (MainTabControl.SelectedIndex < MainTabControl.Items.Count - 1)
            MainTabControl.SelectedIndex++;
    }

    private void PrevTab()
    {
        if (MainTabControl.SelectedIndex > 0)
            MainTabControl.SelectedIndex--;
    }
}