using System;
using Avalonia.Controls;
using Avalonia.Controls.Notifications;
using Avalonia.Input;
using Avalonia.Styling;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Lucide.Avalonia;
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
    public static SecureStorage SecureStorage = new SecureStorage("Prolizy2");
    public static SukiMainHost SukiHost { get; set; }
    public static TopLevel TopLevel { get; set; }
    
    private Vector? lastScrollVector;

    public MainView()
    {
        InitializeComponent();
        DialogManager = DialogHost.Manager = new SukiDialogManager();
        ToastManager = ToastHost.Manager = new SukiToastManager();
        
        DataContextChanged += (_, _) => AttachWelcomeScreen();

        // Once the first screen is up, build the other tabs in idle time.
        Loaded += (_, _) => (DataContext as MainViewModel)?.PreloadTabsWhenIdle();
    }

    private MainViewModel? _welcomeSource;

    /// <summary>
    /// Creates the welcome screen only when it has to be shown (first launch)
    /// and releases it once dismissed, instead of always building it hidden.
    /// </summary>
    private void AttachWelcomeScreen()
    {
        if (_welcomeSource is not null)
            _welcomeSource.PropertyChanged -= OnMainViewModelPropertyChanged;

        _welcomeSource = DataContext as MainViewModel;
        if (_welcomeSource is null)
            return;

        _welcomeSource.PropertyChanged += OnMainViewModelPropertyChanged;
        UpdateWelcomeScreen();
    }

    private void OnMainViewModelPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(MainViewModel.IsWelcomeScreenVisible))
            UpdateWelcomeScreen();
    }

    private void UpdateWelcomeScreen()
    {
        var visible = _welcomeSource?.IsWelcomeScreenVisible == true;
        if (visible && WelcomeHost.Children.Count == 0)
        {
            WelcomeHost.Children.Add(new Welcome.WelcomeView
            {
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch,
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Stretch,
                HorizontalContentAlignment = Avalonia.Layout.HorizontalAlignment.Stretch,
                VerticalContentAlignment = Avalonia.Layout.VerticalAlignment.Stretch
            });
        }
        else if (!visible)
        {
            WelcomeHost.Children.Clear();
        }
    }

    private void NextTab()
    {
        
    }

    private void PrevTab()
    {
        
    }
}