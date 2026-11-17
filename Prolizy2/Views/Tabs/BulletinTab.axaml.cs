using System.ComponentModel;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Prolizy2.ViewModels.Bulletin;
using Prolizy2.Views.Bulletin;

namespace Prolizy2.Views.Tabs;

public partial class BulletinTab : UserControl
{
    private BulletinVariant? _shownVariant;

    public BulletinTab()
    {
        InitializeComponent();

        // The tab lives as long as the application, like the shared options.
        BulletinDisplayOptions.Instance.PropertyChanged += OnOptionsChanged;
        ShowSelectedVariant();
    }

    private void OnOptionsChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(BulletinDisplayOptions.Variant))
            ShowSelectedVariant();
    }

    /// <summary>Hosts the view of the selected layout variant, replacing the previous one.</summary>
    private void ShowSelectedVariant()
    {
        var variant = BulletinDisplayOptions.Instance.Variant;
        if (_shownVariant == variant)
            return;
        _shownVariant = variant;

        VariantHost.Children.Clear();
        VariantHost.Children.Add(variant == BulletinVariant.DrillDown
            ? new BulletinDrillDownView()
            : new BulletinTabsView());
    }
}
