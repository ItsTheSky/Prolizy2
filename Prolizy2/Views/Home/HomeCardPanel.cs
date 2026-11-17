using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Prolizy2.ViewModels.Home;

namespace Prolizy2.Views.Home;

/// <summary>
/// Two-column flow used as the items panel of the home cards: a full-width card
/// takes a whole row, half-width cards are paired, and both cards of a row get
/// the height of the tallest one.
/// </summary>
public class HomeCardPanel : Panel
{
    private const double ColumnGap = 10d;
    private const double RowGap = 10d;

    private static bool IsWide(Control child) => child.DataContext is HomeCardViewModel { IsFullWidth: true };

    private List<Control[]> BuildRows()
    {
        var rows = new List<Control[]>();
        Control? pending = null;

        foreach (var child in Children)
        {
            if (!child.IsVisible)
                continue;

            if (IsWide(child))
            {
                if (pending is not null)
                {
                    rows.Add(new[] { pending });
                    pending = null;
                }

                rows.Add(new[] { child });
            }
            else if (pending is null)
            {
                pending = child;
            }
            else
            {
                rows.Add(new[] { pending, child });
                pending = null;
            }
        }

        if (pending is not null)
            rows.Add(new[] { pending });

        return rows;
    }

    private static double HalfWidth(double width) =>
        double.IsInfinity(width) ? double.PositiveInfinity : Math.Max(0d, (width - ColumnGap) / 2d);

    private static double RowHeight(Control[] row)
    {
        var height = 0d;
        foreach (var child in row)
            height = Math.Max(height, child.DesiredSize.Height);
        return height;
    }

    protected override Size MeasureOverride(Size availableSize)
    {
        var halfWidth = HalfWidth(availableSize.Width);
        var desiredWidth = 0d;

        foreach (var child in Children)
        {
            var width = IsWide(child) ? availableSize.Width : halfWidth;
            child.Measure(new Size(width, double.PositiveInfinity));
            desiredWidth = Math.Max(desiredWidth, child.DesiredSize.Width);
        }

        var rows = BuildRows();
        var height = 0d;
        foreach (var row in rows)
            height += RowHeight(row);
        height += RowGap * Math.Max(0, rows.Count - 1);

        return new Size(double.IsInfinity(availableSize.Width) ? desiredWidth : availableSize.Width, height);
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        var halfWidth = HalfWidth(finalSize.Width);
        var y = 0d;

        foreach (var row in BuildRows())
        {
            var height = RowHeight(row);

            if (IsWide(row[0]))
            {
                row[0].Arrange(new Rect(0d, y, finalSize.Width, height));
            }
            else
            {
                for (var column = 0; column < row.Length; column++)
                    row[column].Arrange(new Rect(column * (halfWidth + ColumnGap), y, halfWidth, height));
            }

            y += height + RowGap;
        }

        return finalSize;
    }
}
