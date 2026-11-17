using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Avalonia;
using Avalonia.Controls;

namespace Prolizy2.Views.Tabs.Settings;

/// <summary>
/// ComboBox bound to one of the picker lists of the home settings page
/// (semesters, resources/SAÉs, units). It maps the selected item back to the
/// plain string stored on the card (a semester id, a group code, a unit code),
/// so the card configuration stays free of any UI type.
///
/// The choices come from <see cref="ItemsControl.ItemsSource"/> and are turned
/// into display rows (<see cref="HomePickerRow"/>) so the ComboBox can show a
/// readable label without a data template.
/// </summary>
public class HomePickerComboBox : ComboBox
{
    /// <summary>Property of the choices holding the key stored on the card.</summary>
    public static readonly StyledProperty<string?> KeyPathProperty =
        AvaloniaProperty.Register<HomePickerComboBox, string?>(nameof(KeyPath));

    /// <summary>Property of the choices holding the text to display.</summary>
    public static readonly StyledProperty<string?> DisplayPathProperty =
        AvaloniaProperty.Register<HomePickerComboBox, string?>(nameof(DisplayPath));

    /// <summary>The key currently selected — two-way bound to a card option.</summary>
    public static readonly StyledProperty<string?> SelectedKeyProperty =
        AvaloniaProperty.Register<HomePickerComboBox, string?>(
            nameof(SelectedKey),
            defaultBindingMode: Avalonia.Data.BindingMode.TwoWay);

    private bool _isSyncing;

    static HomePickerComboBox()
    {
        KeyPathProperty.Changed.AddClassHandler<HomePickerComboBox>(OnChanged);
        DisplayPathProperty.Changed.AddClassHandler<HomePickerComboBox>(OnChanged);
        SelectedKeyProperty.Changed.AddClassHandler<HomePickerComboBox>(OnSelectedKeyChanged);
        SelectionChangedEvent.AddClassHandler<HomePickerComboBox>(OnSelectionChanged);

        // Re-read the choices when the source collection is swapped.
        ItemsControl.ItemsSourceProperty.Changed.AddClassHandler<HomePickerComboBox>(
            (combo, _) => combo.RebuildRows());
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

    /// <summary>The rows actually shown, one per choice.</summary>
    private List<HomePickerRow>? _rows;

    private static void OnChanged(HomePickerComboBox combo, AvaloniaPropertyChangedEventArgs _) =>
        combo.RebuildRows();

    private static void OnSelectedKeyChanged(HomePickerComboBox combo, AvaloniaPropertyChangedEventArgs _) =>
        combo.SyncFromKey();

    private static void OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (sender is not HomePickerComboBox combo || combo._isSyncing)
            return;

        var key = (combo.SelectedItem as HomePickerRow)?.Key;

        combo._isSyncing = true;
        try
        {
            combo.SetCurrentValue(SelectedKeyProperty, key);
        }
        finally
        {
            combo._isSyncing = false;
        }
    }

    private void RebuildRows()
    {
        var keyPath = KeyPath;
        var displayPath = DisplayPath;

        _rows = ItemsSource?
            .Cast<object>()
            .Where(item => item is not null)
            .Select(item => new HomePickerRow(
                keyPath is null ? item.ToString() : GetMember(item, keyPath),
                displayPath is null ? item.ToString() ?? string.Empty : GetMember(item, displayPath) ?? string.Empty))
            .ToList() ?? new List<HomePickerRow>();

        _isSyncing = true;
        try
        {
            base.ItemsSource = _rows;
        }
        finally
        {
            _isSyncing = false;
        }

        SyncFromKey();
    }

    private void SyncFromKey()
    {
        if (_rows is null)
            return;

        var key = SelectedKey;

        _isSyncing = true;
        try
        {
            var index = key is null
                ? -1
                : _rows.FindIndex(row => string.Equals(row.Key, key, StringComparison.Ordinal));

            if (index >= 0 || SelectedIndex != -1)
                SelectedIndex = index;
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

/// <summary>A row of a <see cref="HomePickerComboBox"/>: the stored key and its label.</summary>
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