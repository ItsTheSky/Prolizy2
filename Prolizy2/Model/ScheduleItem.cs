using System;
using System.Globalization;
using System.Threading.Tasks;
using Avalonia.Media;
using Avalonia.Threading;
using Prolizy2.API;
using Prolizy2.API.Model.Course;
using Prolizy2.API.Utils;
using Prolizy2.Styles.Controls.Edt;
using Prolizy2.ViewModels.Common;
using Prolizy2.Views;
using SukiUI.Controls;
using SukiUI.Dialogs;

namespace Prolizy2.Model;

public class ScheduleItem
{
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    
    public DateOnly Date => DateOnly.FromDateTime(StartTime);
    public TimeOnly StartTimeOnly => TimeOnly.FromDateTime(StartTime);
    public TimeOnly EndTimeOnly => TimeOnly.FromDateTime(EndTime);

    public string Professor { get; set; }
    public string Subject { get; set; }
    public string Room { get; set; }
    public string Group { get; set; }

    public string Type { get; set; }
    
    public string? AdditionalText { get; set; }

    public ScheduleItemOverlay? Overlay { get; set; }

    public IBrush BackgroundColor { get; set; }
    public IBrush ForegroundColor { get; set; }
    public IBrush BorderColor { get; set; }

    public string FormattedTimeRange => $"{StartTime:HH\\hmm} - {EndTime:HH\\hmm}";

    public string FormattedDuration
    {
        get
        {
            var duration = EndTime - StartTime;
            if (duration.Minutes == 0)
                return $"{(int)duration.TotalHours}h";
            
            return $"{(int)duration.TotalHours}h{duration.Minutes}m";
        }
    }

    public string FormattedType => Type switch
    {
        CourseTypes.Examen => "DS",
        CourseTypes.TravauxDiriges => "TD",
        CourseTypes.TravauxPratiques => "TP",
        CourseTypes.CoursMagistral => "CM",
        CourseTypes.Projet => "SAÉ",
        _ => Type
    };
    
    // format: Mercredi 1 Février
    public string FormattedDate => StartTime.ToString("dddd d MMMM", CultureInfo.CurrentCulture);

    public Course Course { get; set; }

    public void ItemClicked()
    {
        Course.GetDescription().ContinueWith(t =>
        {
            if (t.IsFaulted || t.Result == null!)
            {
                Console.WriteLine("Failed to fetch course description:");
                Console.WriteLine(t.Exception);
                return;
            }

            var description = t.Result;
            AdditionalText = string.Join("\n", description.Notes);
            
            Dispatcher.UIThread.Invoke(() =>
            {
                MainView.DialogManager.CreateDialog()
                    .WithTitle(Subject)
                    .WithContent(new ItemDialogOverlay { Item = this })
                    .WithActionButton(LangViewModel.Translate("Ok"), _ => { }, true)
                    .TryShow();
            });
        });
    }
}

public record ScheduleItemOverlay(IBrush Overlay, IBrush TextColor, string DisplayText);