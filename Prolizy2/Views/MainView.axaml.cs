using System;
using Avalonia.Controls;
using Avalonia.Controls.Notifications;
using Avalonia.Input;
using Avalonia.Styling;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using IconPacks.Avalonia.Lucide;
using Prolizy2.ViewModels;
using SukiUI;
using SukiUI.Controls;
using SukiUI.Dialogs;
using SukiUI.Toasts;
using Vector = Avalonia.Vector;

namespace Prolizy2.Views;

public partial class MainView : UserControl
{
    public static ISukiDialogManager DialogManager { get; private set; }
    public static ISukiToastManager ToastManager { get; private set; }
    public static SukiMainHost SukiHost { get; set; }
    public static TopLevel TopLevel { get; set; }
    
    private Vector? lastScrollVector;

    public MainView()
    {
        InitializeComponent();
        DialogManager = DialogHost.Manager = new SukiDialogManager();
        ToastManager = ToastHost.Manager = new SukiToastManager();

        Loaded += (sender, args) =>
        {
            SukiTheme.GetInstance().ChangeBaseTheme(ThemeVariant.Dark);
            
            MainBorder.AddHandler(Gestures.ScrollGestureEvent, (o, args) => lastScrollVector = args.Delta);
            MainBorder.AddHandler(Gestures.ScrollGestureEndedEvent, (o, args) =>
            {
                if (args.Handled || lastScrollVector == null)
                    return;

                var x = lastScrollVector.Value.X;

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
        
    }

    private void PrevTab()
    {
        
    }
}