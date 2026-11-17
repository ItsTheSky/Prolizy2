using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using Prolizy2.Model.Bulletin;
using Prolizy2.ViewModels.Common;

namespace Prolizy2.ViewModels.Tabs;

/// <summary>
/// Card listing teaching units (or a single chosen one) with their average and
/// status — a compact view of the semester transcript.
/// </summary>
public partial class UnitListCardViewModel : ViewModelBase
{
    public UnitListCardViewModel(BulletinData? data, string? unitCode, int maxItems)
    {
        UnitCode = unitCode;
        MaxItems = maxItems <= 0 ? 4 : maxItems;
        Apply(data);
    }

    /// <summary>Code of a single unit to show, or null to list them all.</summary>
    public string? UnitCode { get; }

    /// <summary>Maximum number of rows displayed.</summary>
    public int MaxItems { get; }

    [ObservableProperty] private ObservableCollection<TeachingUnitInfo> _units = new();

    [ObservableProperty]
    private bool _hasUnits;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CountLabel))]
    private int _totalCount;

    public bool HasNoUnits => !HasUnits;

    public string CountLabel => string.Format(
        System.Globalization.CultureInfo.CurrentCulture,
        LangViewModel.Translate("HomeCardUnitCount"), TotalCount);

    /// <summary>Rebuilds the list from the bulletin data.</summary>
    public void Apply(BulletinData? data)
    {
        if (data is null)
        {
            Units = new ObservableCollection<TeachingUnitInfo>();
            TotalCount = 0;
            HasUnits = false;
            return;
        }

        System.Collections.Generic.IEnumerable<TeachingUnitInfo> source = data.Units;

        if (!string.IsNullOrEmpty(UnitCode))
            source = source.Where(u => string.Equals(u.Code, UnitCode, System.StringComparison.OrdinalIgnoreCase));

        var all = new System.Collections.Generic.List<TeachingUnitInfo>(source);
        var shown = all.Count <= MaxItems ? all : all.GetRange(0, MaxItems);

        Units = new ObservableCollection<TeachingUnitInfo>(shown);
        TotalCount = all.Count;
        HasUnits = shown.Count > 0;
    }
}