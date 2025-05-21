using Prolizy2.Views;
using SukiUI.Dialogs;

namespace Prolizy2.Utilities;

public static class Dialogs
{

    public static ISukiDialogManager GetManager() => 
        MainView.DialogManager;

}