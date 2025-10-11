using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Prolizy2.Model;

namespace Prolizy2.ViewModels.Tabs;

public partial class HomeViewModel(MainViewModel mainVm) : ViewModelBase
{

    [ObservableProperty] private ScheduleItem? _nextScheduleItem;

    [RelayCommand]
    public void NavigateToAndOpenNextItem()
    {
        if (NextScheduleItem == null) return;
        
        mainVm.NavigateTo(typeof(EdtViewModel));
        mainVm.EdtViewModel.CurrentDay = NextScheduleItem.Date;
        mainVm.EdtViewModel.LoadCurrentDayFromCache();
        NextScheduleItem.ItemClicked();
    }

}