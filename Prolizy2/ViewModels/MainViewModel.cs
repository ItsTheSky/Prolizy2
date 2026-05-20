using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Lucide.Avalonia;
using Prolizy2.Services;
using Prolizy2.Utilities;
using Prolizy2.ViewModels.Bulletin;
using Prolizy2.ViewModels.Common;
using Prolizy2.ViewModels.Tabs;
using Prolizy2.Views;
using Prolizy2.Views.Tabs;

namespace Prolizy2.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    #region TabControl-like System

    [ObservableProperty] private ObservableCollection<TabItemEntry> _tabEntries;
    [ObservableProperty] private TabItemEntry _selectedTabEntry;
    public Control DisplayedControl => SelectedTabEntry?.Content ?? new TextBlock() { Text = "None" };

    public void InitTabEntries()
    {
        TabEntries =
        [
            new TabItemEntry(this)
            {
                Icon = LucideIconKind.House,
                Title = "TabHome",
                ViewModelType = typeof(HomeViewModel),
                ContentFactory = () => new HomeTab { DataContext = HomeViewModel }
            },

            new TabItemEntry(this)
            {
                Icon = LucideIconKind.Calendar,
                Title = "TabEdt",
                ViewModelType = typeof(EdtViewModel),
                ContentFactory = () => new EdtTab { DataContext = EdtViewModel }
            },

            new TabItemEntry(this)
            {
                Icon = LucideIconKind.NotebookPen,
                Title = "TabBulletin",
                ViewModelType = typeof(BulletinCoreViewModel),
                ContentFactory = () => new BulletinTab { DataContext = BulletinViewModel }
            },

            new TabItemEntry(this)
            {
                Icon = LucideIconKind.Settings,
                Title = "TabSettings",
                ViewModelType = typeof(SettingsViewModel),
                ContentFactory = () => new SettingsTab { DataContext = SettingsViewModel }
            }
        ];

        NavigateTo(TabEntries[0]);
    }

    public void NavigateTo(TabItemEntry entry)
    {
        if (SelectedTabEntry != null!)
            SelectedTabEntry.IsSelected = false;

        SelectedTabEntry = entry;
        SelectedTabEntry.IsSelected = true;
        OnPropertyChanged(nameof(DisplayedControl));
    }

    public void NavigateTo(Type vmType)
    {
        foreach (var entry in TabEntries)
        {
            if (entry.ViewModelType == vmType)
            {
                NavigateTo(entry);
                return;
            }
        }
    }

    #endregion

    #region View Models

    [ObservableProperty] private HomeViewModel _homeViewModel;
    [ObservableProperty] private EdtViewModel _edtViewModel;
    [ObservableProperty] private BulletinCoreViewModel _bulletinViewModel;
    [ObservableProperty] private SettingsViewModel _settingsViewModel;

    [ObservableProperty] private LangViewModel _langViewModel;

    /// <summary>Shared bulletin client wrapper — credentials live in SecureStorage.</summary>
    public BulletinService BulletinService { get; } = new();

    #endregion

    [ObservableProperty] private bool _isStartButtonBusy = false;
    [ObservableProperty] private bool _isWelcomeScreenVisible = false;

    public MainViewModel()
    {
        IsWelcomeScreenVisible = !MainView.Preferences.Get("welcome_screen_shown", false);
        
        LangViewModel = new(this);

        // Initialize the ViewModels
        HomeViewModel = new HomeViewModel(this);
        EdtViewModel = new EdtViewModel(this);
        SettingsViewModel = new SettingsViewModel(this);
        BulletinViewModel = new BulletinCoreViewModel(this);

        InitTabEntries();

        LangViewModel.CurrentCultureChanged += info =>
        {
            foreach (var entry in TabEntries)
                entry.RefreshTitle(); // Force update the title
        };
    }

    [RelayCommand]
    public async Task Start()
    {
        IsStartButtonBusy = true;
        await Task.Delay(500);
        
        IsStartButtonBusy = false;
        IsWelcomeScreenVisible = false;

        MainView.Preferences.Set("welcome_screen_shown", true);
        /*MainView.DialogManager.CreateDialog()
            .WithTitle("A Standard Dialog")
            .WithContent("This is a standard dialog. Click the button below to dismiss.")
            .WithActionButton("Dismiss", _ => { }, true)
            .TryShow();*/
    }
}

public partial class TabItemEntry(MainViewModel mainVm) : ObservableObject
{
    [ObservableProperty] private LucideIconKind _icon;
    [ObservableProperty] private string _title;
    [ObservableProperty] private bool _isSelected;

    /// <summary>View-model type hosted by this tab; used by <see cref="MainViewModel.NavigateTo(Type)"/>.</summary>
    public Type? ViewModelType { get; init; }

    /// <summary>Factory building the tab view on first display. Mutually exclusive with a direct <see cref="Content"/>.</summary>
    public Func<Control>? ContentFactory { get; init; }

    private Control? _content;

    /// <summary>
    /// The control shown for this tab. When <see cref="ContentFactory"/> is set the
    /// view is wrapped in a <see cref="LazyLoader"/> and built only on first display.
    /// </summary>
    public Control? Content
    {
        get => _content ??= ContentFactory is { } factory ? new LazyLoader(factory) : null;
        set => _content = value;
    }

    [RelayCommand]
    public void Clicked()
    {
        if (IsSelected)
            return;

        mainVm.NavigateTo(this);
    }

    public void RefreshTitle()
    {
        OnPropertyChanged(nameof(Title));
    }
}