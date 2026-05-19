using System;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Threading;

namespace Prolizy2.Utilities;

/// <summary>
/// Hosts a view but defers building it until the control is first shown.
/// A lightweight placeholder is displayed while the real view — and its first
/// layout pass — is constructed, which keeps tab switches responsive instead of
/// paying the whole UI cost up-front.
/// </summary>
public class LazyLoader : ContentControl
{
    private readonly Func<Control> _viewFactory;
    private bool _isLoaded;

    public LazyLoader(Func<Control> viewFactory)
    {
        _viewFactory = viewFactory;
        HorizontalContentAlignment = HorizontalAlignment.Stretch;
        VerticalContentAlignment = VerticalAlignment.Stretch;
        Content = BuildPlaceholder();

        AttachedToVisualTree += (_, _) => Load();
    }

    /// <summary>Builds the real view once; subsequent calls are no-ops.</summary>
    private void Load()
    {
        if (_isLoaded)
            return;
        _isLoaded = true;

        // Build on the next background tick so the placeholder paints first and
        // the tab transition is not stalled by the view + layout construction.
        Dispatcher.UIThread.Post(() => Content = _viewFactory(), DispatcherPriority.Background);
    }

    private static Control BuildPlaceholder() => new ProgressBar
    {
        IsIndeterminate = true,
        Width = 140,
        HorizontalAlignment = HorizontalAlignment.Center,
        VerticalAlignment = VerticalAlignment.Center
    };
}
