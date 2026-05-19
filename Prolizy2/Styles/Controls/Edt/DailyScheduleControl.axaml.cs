using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Shapes;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Threading;
using Lucide.Avalonia;
using Prolizy2.API.Utils;
using Prolizy2.Model;
using Prolizy2.Utilities;
using SukiUI.Controls;

namespace Prolizy2.Styles.Controls.Edt;

public class DailyScheduleControl : TemplatedControl
{
    protected override Type StyleKeyOverride => typeof(DailyScheduleControl);

    public static readonly StyledProperty<ObservableCollection<ScheduleItem>> ItemsProperty =
        AvaloniaProperty.Register<DailyScheduleControl, ObservableCollection<ScheduleItem>>(
            nameof(Items), 
            defaultBindingMode: Avalonia.Data.BindingMode.OneWay);

    public static readonly StyledProperty<bool> ShowHourLinesProperty =
        AvaloniaProperty.Register<DailyScheduleControl, bool>(
            nameof(ShowHourLines), 
            defaultValue: true,
            defaultBindingMode: Avalonia.Data.BindingMode.OneWay);

    public static readonly StyledProperty<bool> ShowHoursProperty =
        AvaloniaProperty.Register<DailyScheduleControl, bool>(
            nameof(ShowHours), 
            defaultValue: true,
            defaultBindingMode: Avalonia.Data.BindingMode.OneWay);

    public static readonly StyledProperty<int> DisplayedHoursProperty =
        AvaloniaProperty.Register<DailyScheduleControl, int>(
            nameof(DisplayedHours), 
            defaultValue: 11,
            defaultBindingMode: Avalonia.Data.BindingMode.OneWay);

    public static readonly StyledProperty<int> StartHourProperty =
        AvaloniaProperty.Register<DailyScheduleControl, int>(
            nameof(StartHour), 
            defaultValue: 8,
            defaultBindingMode: Avalonia.Data.BindingMode.OneWay);

    static DailyScheduleControl()
    {
        // Wire up property change effects
        ItemsProperty.Changed.AddClassHandler<DailyScheduleControl>((x, e) => x.OnItemsChanged(e));
        ShowHourLinesProperty.Changed.AddClassHandler<DailyScheduleControl>((x, e) => x.OnShowHourLinesChanged(e));
        DisplayedHoursProperty.Changed.AddClassHandler<DailyScheduleControl>((x, e) => x.OnDisplayedHoursChanged(e));
        StartHourProperty.Changed.AddClassHandler<DailyScheduleControl>((x, e) => x.OnStartHourChanged(e));
        
        // These properties affect both measure and render
        AffectsRender<DailyScheduleControl>(ItemsProperty, ShowHourLinesProperty);
        AffectsMeasure<DailyScheduleControl>(DisplayedHoursProperty, StartHourProperty);
        AffectsArrange<DailyScheduleControl>(ItemsProperty, DisplayedHoursProperty, StartHourProperty);
    }

    public ObservableCollection<ScheduleItem> Items
    {
        get => GetValue(ItemsProperty);
        set => SetValue(ItemsProperty, value);
    }

    public bool ShowHourLines
    {
        get => GetValue(ShowHourLinesProperty);
        set => SetValue(ShowHourLinesProperty, value);
    }
    
    public bool ShowHours
    {
        get => GetValue(ShowHoursProperty);
        set => SetValue(ShowHoursProperty, value);
    }

    public int DisplayedHours
    {
        get => GetValue(DisplayedHoursProperty);
        set => SetValue(DisplayedHoursProperty, value);
    }

    public int StartHour
    {
        get => GetValue(StartHourProperty);
        set => SetValue(StartHourProperty, value);
    }

    private Grid? _mainGrid;
    private Canvas? _scheduleCanvas;
    private TextBlock? _noDataText;
    private Canvas? _timeLabelsContainer;

    public DailyScheduleControl()
    {
        // Property change handling is now done through static handlers
    }

    private void OnItemsChanged(AvaloniaPropertyChangedEventArgs e)
    {
        // Handle collection change notifications
        if (e.OldValue is ObservableCollection<ScheduleItem> oldCollection)
        {
            oldCollection.CollectionChanged -= OnItemsCollectionChanged;
        }
        
        if (e.NewValue is ObservableCollection<ScheduleItem> newCollection)
        {
            newCollection.CollectionChanged += OnItemsCollectionChanged;
        }
        
        UpdateVisual();
    }

    private void OnShowHourLinesChanged(AvaloniaPropertyChangedEventArgs e)
    {
        UpdateVisual();
    }

    private void OnDisplayedHoursChanged(AvaloniaPropertyChangedEventArgs e)
    {
        GenerateTimeLabels();
        UpdateVisual();
    }

    private void OnStartHourChanged(AvaloniaPropertyChangedEventArgs e)
    {
        GenerateTimeLabels();
        UpdateVisual();
    }

    private void OnItemsCollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    {
        UpdateVisual();
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        _mainGrid = e.NameScope.Find<Grid>("PART_MainGrid");
        _scheduleCanvas = e.NameScope.Find<Canvas>("PART_ScheduleCanvas");
        _noDataText = e.NameScope.Find<TextBlock>("PART_NoDataText");
        _timeLabelsContainer = e.NameScope.Find<Canvas>("PART_TimeLabelsContainer");

        // Delay the initial update to ensure proper bounds
        Dispatcher.UIThread.Post(() =>
        {
            GenerateTimeLabels();
            UpdateVisual();
        });
    }

    protected override void OnSizeChanged(SizeChangedEventArgs e)
    {
        base.OnSizeChanged(e);
        UpdateVisual();
    }

    private void UpdateVisual()
    {
        if (_scheduleCanvas == null || _mainGrid == null || _noDataText == null)
            return;

        _scheduleCanvas.Children.Clear();

        if (Items == null || !Items.Any())
        {
            _noDataText.IsVisible = true;
            return;
        }

        _noDataText.IsVisible = false;

        var canvasHeight = _scheduleCanvas.Bounds.Height;
        if (canvasHeight <= 0) return;

        var hourHeight = canvasHeight / DisplayedHours;

        GenerateTimeLabels();

        // Draw hour lines if enabled
        if (ShowHourLines && !Items.Any(x => x.Type.IsHoliday()) && Items.Any())
        {
            DrawHourLines(hourHeight);
        }

        // Draw schedule items
        foreach (var item in Items)
        {
            DrawScheduleItem(item, hourHeight, canvasHeight);
        }
    }

    private void DrawHourLines(double hourHeight)
    {
        if (_scheduleCanvas == null) return;

        for (var i = 0; i <= DisplayedHours; i++)
        {
            var y = i * hourHeight;
            var line = new Line
            {
                StartPoint = new Point(0, y),
                EndPoint = new Point(_scheduleCanvas.Bounds.Width, y),
                Stroke = Brushes.Gray,
                StrokeThickness = 1,
                StrokeDashArray = [2, 2]
            };
            _scheduleCanvas.Children.Add(line);
        }
    }

    private void DrawScheduleItem(ScheduleItem item, double hourHeight, double canvasHeight)
    {
        if (_scheduleCanvas == null) return;

        var startY = (item.StartTime.TimeOfDay.TotalHours - StartHour) * hourHeight;
        var endY = (item.EndTime.TimeOfDay.TotalHours - StartHour) * hourHeight;
        var itemHeight = endY - startY;

        // Clamp to canvas bounds
        if (startY + itemHeight > canvasHeight)
        {
            itemHeight = canvasHeight - startY;
        }
        if (startY < 0)
        {
            itemHeight += startY;
            startY = 0;
        }

        if (itemHeight <= 0) return;

        Control cardControl = CreateCardControl(item, itemHeight);
        
        // Handle overlay if present
        if (item.Overlay != null)
        {
            var overlayGrid = CreateOverlayGrid(cardControl, item, itemHeight);
            Canvas.SetTop(overlayGrid, startY);
            _scheduleCanvas.Children.Add(overlayGrid);
        }
        else
        {
            Canvas.SetTop(cardControl, startY);
            _scheduleCanvas.Children.Add(cardControl);
        }
    }

    private Control CreateCardControl(ScheduleItem item, double itemHeight)
    {
        if (_scheduleCanvas == null) 
            throw new InvalidOperationException("Schedule canvas is null");

        if (!item.Type.IsHoliday())
        {
            var scheduleCard = new ScheduleItemCard
            {
                Item = item,
                Width = _scheduleCanvas.Bounds.Width,
                Height = itemHeight,
                Cursor = new Cursor(StandardCursorType.Hand)
            };
            
            scheduleCard.Tapped += (_, _) => item.ItemClicked();
            
            return scheduleCard;
        }
        else
        {
            return CreateHolidayCard(item, itemHeight);
        }
    }

    private Control CreateHolidayCard(ScheduleItem item, double itemHeight)
    {
        if (_scheduleCanvas == null) 
            throw new InvalidOperationException("Schedule canvas is null");

        var holidayContent = new StackPanel
        {
            Orientation = Orientation.Vertical,
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Center,
            Spacing = 10,
            Children =
            {
                new LucideIcon()
                {
                    Kind = LucideIconKind.MoonStar,
                    Foreground = item.ForegroundColor,
                    Size = 28,
                    HorizontalAlignment = HorizontalAlignment.Center
                },
                new TextBlock
                {
                    Text = "Jour férié/Vacance", 
                    Foreground = item.ForegroundColor,
                    FontWeight = FontWeight.SemiBold, 
                    TextAlignment = TextAlignment.Center, 
                    FontSize = 18
                }
            }
        };
        
        return new GlassCard
        {
            Padding = new Thickness(5),
            CornerRadius = new CornerRadius(10),
            Width = _scheduleCanvas.Bounds.Width,
            Height = itemHeight,
            Content = holidayContent,
            Background = item.BackgroundColor,
            Foreground = item.ForegroundColor,
            BorderThickness = new Thickness(2),
            BorderBrush = item.BorderColor ?? new SolidColorBrush(Color.Parse("#FFCCCCCC")),
        };
    }

    private Grid CreateOverlayGrid(Control cardControl, ScheduleItem item, double itemHeight)
    {
        if (_scheduleCanvas == null || item.Overlay == null) 
            throw new InvalidOperationException("Required components are null");

        var overlayGrid = new Grid();
        overlayGrid.Children.Add(cardControl);
        
        var overlayText = new TextBlock
        {
            Text = item.Overlay.DisplayText,
            Foreground = item.Overlay.TextColor,
            FontWeight = FontWeight.Bold,
            TextAlignment = TextAlignment.Center,
            FontSize = 18,
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Center
        };
        
        var overlayBorder = new Border
        {
            Background = new SolidColorBrush(Color.Parse("#7F000000")),
            Child = overlayText,
            Width = _scheduleCanvas.Bounds.Width,
            Height = itemHeight,
            Padding = new Thickness(5),
            CornerRadius = new CornerRadius(5),
            ClipToBounds = true,
            BorderBrush = item.Overlay.Overlay,
            BorderThickness = new Thickness(5)
        };
        
        overlayGrid.Children.Add(overlayBorder);
        return overlayGrid;
    }

    private void GenerateTimeLabels()
    {
        if (_timeLabelsContainer == null || _scheduleCanvas == null) return;

        _timeLabelsContainer.Children.Clear();
        
        if (!ShowHours) return;

        var canvasHeight = _scheduleCanvas.Bounds.Height;
        if (canvasHeight <= 0) return;

        var hourHeight = canvasHeight / DisplayedHours;

        for (int i = 0; i <= DisplayedHours; i++)
        {
            var hour = StartHour + i;
            var timeLabel = new TextBlock
            {
                Text = $"{hour}h",
                VerticalAlignment = VerticalAlignment.Top,
                HorizontalAlignment = HorizontalAlignment.Right,
                Margin = new Thickness(0, -4, 5, 0),
                FontSize = 16
            };

            // Position the label at the correct vertical position
            var yPosition = (i * hourHeight) - 8; // Offset by 8 to center with hour lines
            if (yPosition < 0) yPosition = 0;

            Canvas.SetTop(timeLabel, yPosition);
            Canvas.SetRight(timeLabel, 0);
            
            _timeLabelsContainer.Children.Add(timeLabel);
        }
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        // Clean up collection change subscriptions
        if (Items is { } collection)
        {
            collection.CollectionChanged -= OnItemsCollectionChanged;
        }
        
        base.OnDetachedFromVisualTree(e);
    }
}