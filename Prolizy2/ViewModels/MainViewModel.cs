using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Prolizy2.Views;
using SukiUI.Dialogs;

namespace Prolizy2.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    
    [ObservableProperty] private bool _isLoggingIn;
    [ObservableProperty] private string _vector = "none";

    [RelayCommand]
    public async Task Login()
    {
        IsLoggingIn = true;
        // Simulate a login process
        await Task.Delay(2000);
        // After login, you can navigate to another page or perform other actions
        IsLoggingIn = false;

        MainView.DialogManager.CreateDialog()
            .WithTitle("A Standard Dialog")
            .WithContent("This is a standard dialog. Click the button below to dismiss.")
            .WithActionButton("Dismiss", _ => { }, true)
            .TryShow();
    }
    
}