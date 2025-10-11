using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using IconPacks.Avalonia.Lucide;
using Prolizy2.Utilities;
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
                Icon = PackIconLucideKind.House,
                Title = "TabHome",
                Content = new HomeTab { DataContext = HomeViewModel }
            },

            new TabItemEntry(this)
            {
                Icon = PackIconLucideKind.Calendar,
                Title = "TabEdt",
                Content = new EdtTab { DataContext = EdtViewModel }
            },

            new TabItemEntry(this)
            {
                Icon = PackIconLucideKind.NotebookPen,
                Title = "TabBulletin",
            },

            new TabItemEntry(this)
            {
                Icon = PackIconLucideKind.Settings,
                Title = "TabSettings",
                Content = new SettingsTab { DataContext = SettingsViewModel }
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
            if (entry.Content?.DataContext?.GetType() == vmType)
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
    [ObservableProperty] private SettingsViewModel _settingsViewModel;

    [ObservableProperty] private LangViewModel _langViewModel;

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
    [ObservableProperty] private PackIconLucideKind _icon;
    [ObservableProperty] private string _title;
    [ObservableProperty] private bool _isSelected;

    [ObservableProperty] private Control? _content;

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