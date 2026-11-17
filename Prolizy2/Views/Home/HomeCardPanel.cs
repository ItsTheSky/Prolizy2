using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Layout;

namespace Prolizy2.Views.Home;

/// <summary>
/// Lays its items out like a two-column grid: an item is wide (spans both
/// columns) when its <see cref="ViewModels.Tabs.HomeCardViewModel.IsFullWidth"/>
/// is true. Rows are implicit — a row is as tall as its tallest card.
///
/// Every item is wrapped in its own <see cref="ContentPresenter"/> kept in a
/// plain <see cref="Panel"/> owned by this control, so no <c>ItemsPresenter</c>
/// template is involved and the two-column flow stays under our control.
/// </summary>
public class HomeCardPanel : ItemsControl
{
    private const double ColumnGap = 10d;
    private const double RowGap = 10d;

    private readonly Panel _host = new();
    private ContentPresenter[] _presenters = Array.Empty<ContentPresenter>();

    protected override void OnApplyTemplate(Avalonia.Controls.Primitives.TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        if (!LogicalChildren.Contains(_host))
            LogicalChildren.Add(_host);
    }

    protected override Control CreateContainerForItemOverride(object? item, int index, object? recycleKey)
    {
        return new ContentPresenter
        {
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Top,
            HorizontalContentAlignment = HorizontalAlignment.Stretch,
            VerticalContentAlignment = VerticalAlignment.Top
        };
    }

    /// <summary>
    /// True when the presented card spans both columns. The shell (not the
    /// presenter) carries the card, so it is read on the shell's DataContext.
    /// </summary>
    private static bool IsWide(Control child) =>
        child is ContentPresenter { Content: Control shell }
        && shell.DataContext is ViewModels.Tabs.HomeCardViewModel card
        && card.IsFullWidth;

    /// <summary>
    /// Creates the presenters for the current items (or reuses the existing
    /// ones) and refreshes their data context / content.
    /// </summary>
    private void SyncPresenters()
    {
        var count = Items.Count;

        if (_presenters.Length != count)
        {
            _host.Children.Clear();
            _presenters = new ContentPresenter[count];

            for (var i = 0; i < count; i++)
            {
                var presenter = new ContentPresenter
                {
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    VerticalAlignment = VerticalAlignment.Top,
                    HorizontalContentAlignment = HorizontalAlignment.Stretch,
                    VerticalContentAlignment = VerticalAlignment.Top
                };

                _presenters[i] = presenter;
                _host.Children.Add(presenter);
            }
        }

        for (var i = 0; i < count; i++)
        {
            var item = Items[i];
            var presenter = _presenters[i];

            // The presenter shows the card itself (a shell, built by the item
            // template). Re-assigning it rebuilds the card when its view-model
            // is replaced, so the header and the body stay in sync.
            if (!ReferenceEquals(presenter.Content, item))
                presenter.Content = item;

            if (!ReferenceEquals(presenter.DataContext, item))
                presenter.DataContext = item;
        }
    }

    protected override Size MeasureOverride(Size availableSize)
    {
        SyncPresenters();

        var hasBoundedWidth = !double.IsInfinity(availableSize.Width);
        var halfWidth = hasBoundedWidth
            ? Math.Max(0d, (availableSize.Width - ColumnGap) / 2d)
            : double.PositiveInfinity;

        var y = 0d;
        var rowHeight = 0d;
        var column = 0;

        foreach (var presenter in _presenters)
        {
            var wide = IsWide(presenter);

            // A wide card always starts a new row; two half cards share one.
            if (wide && column == 1)
            {
                y += rowHeight + RowGap;
                rowHeight = 0d;
                column = 0;
            }

            var childWidth = wide || !hasBoundedWidth ? availableSize.Width : halfWidth;
            // Measure with an infinite height so a card never gets clipped by
            // the viewport: the scroll viewer clips the panel, not the cards.
            presenter.Measure(new Size(childWidth, double.PositiveInfinity));

            if (wide)
            {
                y += presenter.DesiredSize.Height + RowGap;
                rowHeight = 0d;
                column = 0;
                continue;
            }

            rowHeight = Math.Max(rowHeight, presenter.DesiredSize.Height);
            column++;

            if (column == 2)
            {
                y += rowHeight + RowGap;
                rowHeight = 0d;
                column = 0;
            }
        }

        if (column == 1)
            y += rowHeight;

        return new Size(availableSize.Width, y);
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        var halfWidth = Math.Max(0d, (finalSize.Width - ColumnGap) / 2d);

        var heights = ComputeRowHeights();

        var y = 0d;
        var rowHeight = 0d;
        var column = 0;
        var rowIndex = 0;

        foreach (var presenter in _presenters)
        {
            var wide = IsWide(presenter);

            if (wide && column == 1)
            {
                y += rowHeight + RowGap;
                rowHeight = 0d;
                column = 0;
            }

            var width = wide ? finalSize.Width : halfWidth;
            var x = wide ? 0d : column * (halfWidth + ColumnGap);

            // Both cards of a row are stretched to the tallest one so the two
            // columns stay visually aligned.
            var height = wide || rowIndex >= heights.Count
                ? presenter.DesiredSize.Height
                : heights[rowIndex];
            presenter.Arrange(new Rect(x, y, width, height));
            rowHeight = Math.Max(rowHeight, presenter.DesiredSize.Height);

            if (wide)
            {
                y += rowHeight + RowGap;
                rowHeight = 0d;
                column = 0;
                continue;
            }

            column++;
            if (column == 2)
            {
                y += rowHeight + RowGap;
                rowHeight = 0d;
                column = 0;
                rowIndex++;
            }
        }

        return finalSize;
    }

    /// <summary>
    /// The height of each implicit row: the tallest of its cards. Computed from
    /// the last measure pass so a row's two cards can be stretched equally.
    /// </summary>
    private IReadOnlyList<double> ComputeRowHeights()
    {
        var heights = new List<double>();
        double current = 0d;
        var column = 0;

        foreach (var presenter in _presenters)
        {
            var wide = IsWide(presenter);

            if (wide && column == 1)
            {
                heights.Add(current);
                current = 0d;
                column = 0;
            }

            if (wide)
            {
                heights.Add(presenter.DesiredSize.Height);
                current = 0d;
                column = 0;
                continue;
            }

            current = Math.Max(current, presenter.DesiredSize.Height);
            column++;

            if (column == 2)
            {
                heights.Add(current);
                current = 0d;
                column = 0;
            }
        }

        if (column == 1)
            heights.Add(current);

        return heights;
    }
}