using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Reflection;
using Avalonia;
using Avalonia.Controls;

namespace Prolizy2.Views.Tabs.Settings;

/// <summary>
/// ComboBox that maps the picked choice back to a plain string key (semester id,
/// group code, unit code) so the card configuration stays free of UI types.
/// </summary>
public class HomePickerComboBox : ComboBox
{
    public static readonly StyledProperty<IEnumerable?> ChoicesProperty =
        AvaloniaProperty.Register<HomePickerComboBox, IEnumerable?>(nameof(Choices));

    public static readonly StyledProperty<string?> KeyPathProperty =
        AvaloniaProperty.Register<HomePickerComboBox, string?>(nameof(KeyPath));

    public static readonly StyledProperty<string?> DisplayPathProperty =
        AvaloniaProperty.Register<HomePickerComboBox, string?>(nameof(DisplayPath));

    public static readonly StyledProperty<string?> SelectedKeyProperty =
        AvaloniaProperty.Register<HomePickerComboBox, string?>(
            nameof(SelectedKey),
            defaultBindingMode: Avalonia.Data.BindingMode.TwoWay);

    private List<HomePickerRow> _rows = new();
    private INotifyCollectionChanged? _observedChoices;
    private bool _isSyncing;

    static HomePickerComboBox()
    {
        SelectionChangedEvent.AddClassHandler<HomePickerComboBox>((combo, _) => combo.OnRowSelected());
    }

    protected override Type StyleKeyOverride => typeof(ComboBox);

    public IEnumerable? Choices
    {
        get => GetValue(ChoicesProperty);
        set => SetValue(ChoicesProperty, value);
    }

    public string? KeyPath
    {
        get => GetValue(KeyPathProperty);
        set => SetValue(KeyPathProperty, value);
    }

    public string? DisplayPath
    {
        get => GetValue(DisplayPathProperty);
        set => SetValue(DisplayPathProperty, value);
    }

    public string? SelectedKey
    {
        get => GetValue(SelectedKeyProperty);
        set => SetValue(SelectedKeyProperty, value);
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == ChoicesProperty)
        {
            if (_observedChoices is not null)
                _observedChoices.CollectionChanged -= OnChoicesCollectionChanged;

            _observedChoices = Choices as INotifyCollectionChanged;

            if (_observedChoices is not null)
                _observedChoices.CollectionChanged += OnChoicesCollectionChanged;

            RebuildRows();
        }
        else if (change.Property == KeyPathProperty || change.Property == DisplayPathProperty)
        {
            RebuildRows();
        }
        else if (change.Property == SelectedKeyProperty && !_isSyncing)
        {
            SyncFromKey();
        }
    }

    private void OnChoicesCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e) => RebuildRows();

    private void OnRowSelected()
    {
        if (_isSyncing)
            return;

        _isSyncing = true;
        try
        {
            SetCurrentValue(SelectedKeyProperty, (SelectedItem as HomePickerRow)?.Key);
        }
        finally
        {
            _isSyncing = false;
        }
    }

    private void RebuildRows()
    {
        var keyPath = KeyPath;
        var displayPath = DisplayPath;

        _rows = Choices?
            .Cast<object?>()
            .OfType<object>()
            .Select(item => new HomePickerRow(
                keyPath is null ? item.ToString() : GetMember(item, keyPath),
                (displayPath is null ? item.ToString() : GetMember(item, displayPath)) ?? string.Empty))
            .ToList() ?? new List<HomePickerRow>();

        // Swapping the rows clears the selection: the key must survive it, it is
        // re-applied right after.
        _isSyncing = true;
        try
        {
            ItemsSource = _rows;
        }
        finally
        {
            _isSyncing = false;
        }

        SyncFromKey();
    }

    private void SyncFromKey()
    {
        var key = SelectedKey;

        _isSyncing = true;
        try
        {
            SelectedIndex = _rows.FindIndex(row => string.Equals(row.Key, key, StringComparison.Ordinal));
        }
        finally
        {
            _isSyncing = false;
        }
    }

    private static string? GetMember(object item, string path) =>
        item.GetType()
            .GetProperty(path, BindingFlags.Instance | BindingFlags.Public | BindingFlags.IgnoreCase)
            ?.GetValue(item)
            ?.ToString();
}

public sealed class HomePickerRow
{
    public HomePickerRow(string? key, string label)
    {
        Key = key;
        Label = label;
    }

    public string? Key { get; }

    public string Label { get; }

    public override string ToString() => Label;
}
