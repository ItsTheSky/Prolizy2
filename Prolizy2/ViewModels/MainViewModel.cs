using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Markup.Xaml.MarkupExtensions;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using IconPacks.Avalonia.Lucide;
using Prolizy2.ViewModels.Common;
using Prolizy2.ViewModels.Tabs;
using Prolizy2.Views.Common;
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
                Content = new EdtTab { DataContext = EdtViewMode }
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
                Content = new PersonalizationView { DataContext = PersonalizationViewModel }
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

    #endregion

    #region View Models
    
    [ObservableProperty] private HomeViewModel _homeViewModel;
    [ObservableProperty] private EdtViewModel _edtViewMode;

    [ObservableProperty] private PersonalizationViewModel _personalizationViewModel;
    [ObservableProperty] private LangViewModel _langViewModel;

    #endregion
    
    [ObservableProperty] private bool _isStartButtonBusy = false;
    [ObservableProperty] private bool _isWelcomeScreenVisible = true;

    public MainViewModel()
    {
        PersonalizationViewModel = new(this);
        LangViewModel = new(this);
        
        // Initialize the ViewModels
        HomeViewModel = new(this);
        EdtViewMode = new(this);
        
        InitTabEntries();
        
        IsWelcomeScreenVisible = true;

        LangViewModel.CurrentCultureChanged += info =>
        {
            Console.WriteLine($"CurrentCulture v2: {info}");
            foreach (var entry in TabEntries)
                entry.RefreshTitle(); // Force update the title
        };
    }

    [RelayCommand]
    public async Task Start()
    {
        IsStartButtonBusy = true;
        // Simulate a login process
        await Task.Delay(500);
        // After login, you can navigate to another page or perform other actions
        IsStartButtonBusy = false;
        IsWelcomeScreenVisible = false;

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