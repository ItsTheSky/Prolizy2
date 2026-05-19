using System;
using System.Collections.Generic;
using System.IO;
using Avalonia.Controls;
using Avalonia.Controls.Notifications;
using Avalonia.Input.Platform;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Threading;
using AvaloniaEdit.Document;
using AvaloniaEdit.Rendering;
using Prolizy2.Utilities;
using Prolizy2.Utilities.Bridges;

namespace Prolizy2.Views.Tabs;

public partial class DebugTab : UserControl
{
    public static readonly List<string> WaitingLines = new();
    
    public static DebugTab Instance { get; set; }

    public DebugTab()
    {
        InitializeComponent();
        
        Instance = this;
        TextBlock.TextArea.TextView.LineTransformers.Add(new ColorizeAvalonEdit());
        
        foreach (var line in WaitingLines)
            AddDebugText(line);
        WaitingLines.Clear();

        // var waitingDebugLines = AndroidBridgeProvider.Bridge?.GetWaitingDebugLines() ?? new List<string>();
        // if (waitingDebugLines.Count > 0)
        // {
        //     foreach (var line in waitingDebugLines)
        //         AddDebugText("[PRE] " + line);
        //     
        //     waitingDebugLines.Clear();
        // }
    }

    public static bool AddDebugText(string line)
    {
        if (Instance == null!)
        {
            WaitingLines.Add(line);
            return false;
        }
        
        // Ensure we're on the UI thread
        Dispatcher.UIThread.Post(() =>
        {
            Instance.TextBlock.Text += line + "\n";
            Instance.TextBlock.TextArea.TextView.Redraw();
            Instance.TextBlock.ScrollToEnd();
        });
        return true;
    }

    private string _storedLogs = "";
    private void LoadErrorFile_OnClick(object? sender, RoutedEventArgs e)
    {
        var btn = (Button) sender!;
        var errorFile = Paths.Build("error.txt");
        if (File.Exists(errorFile))
        {
            _storedLogs = Instance.TextBlock.Text ?? "";
            var errorLogs = File.ReadAllText(errorFile);
            
            btn.Content = "Restore Logs";
            btn.Click -= LoadErrorFile_OnClick;
            btn.Click += RestoreLogs_OnClick;
        }
        else
        {
            AddDebugText("No error file found!");
        }
    }
    
    private void RestoreLogs_OnClick(object? sender, RoutedEventArgs e)
    {
        var btn = (Button) sender!;
        Instance.TextBlock.Text = _storedLogs;
        
        btn.Content = "Load Err. Rep.";
        btn.Click -= RestoreLogs_OnClick;
        btn.Click += LoadErrorFile_OnClick;
    }

    private void Clear_OnClick(object? sender, RoutedEventArgs e)
    {
        Instance.TextBlock.Text = "";
    }
    
    private void Crash_OnClick(object? sender, RoutedEventArgs e)
    {
        throw new Exception("Crash test");
    }
    
    private async void Copy_OnClick(object? sender, RoutedEventArgs e)
    {
        await MainView.TopLevel.Clipboard?.SetTextAsync(Instance.TextBlock.Text ?? "")!;
    }
}

public class ColorizeAvalonEdit : DocumentColorizingTransformer
{
    protected override void ColorizeLine(DocumentLine line)
    {
        var lineStartOffset = line.Offset;
        var start = 0;
        
        var clr = LogType.MatchPrefix(DebugTab.Instance.TextBlock.Document.GetText(line));
        if (clr != null)
        {
            base.ChangeLinePart(
                lineStartOffset,              // startOffset
                lineStartOffset + line.Length,         // endOffset
                (VisualLineElement element) =>
                {
                    // Modifier le style du texte
                    element.TextRunProperties.SetForegroundBrush(clr);
                });
        }
    }
}