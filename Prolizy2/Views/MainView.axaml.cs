using System;
using Avalonia.Controls;
using Avalonia.Controls.Notifications;
using Avalonia.Input;
using Avalonia.Styling;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using IconPacks.Avalonia.Lucide;
using Prolizy2.Utilities.Storage;
using Prolizy2.ViewModels;
using Prolizy2.ViewModels.Common;
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
    public static Preferences Preferences = new Preferences("Prolizy2");
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
        };
    }

    private void NextTab()
    {
        
    }

    private void PrevTab()
    {
        
    }
}