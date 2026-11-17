using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using Prolizy2.Model.Bulletin;
using Prolizy2.ViewModels.Common;

namespace Prolizy2.ViewModels.Home;

public partial class UnitListCardViewModel : ViewModelBase
{
    public UnitListCardViewModel(BulletinData? data, string? unitCode, int maxItems)
    {
        UnitCode = unitCode;
        MaxItems = maxItems <= 0 ? 4 : maxItems;
        Apply(data);
    }

    public string? UnitCode { get; }

    public int MaxItems { get; }

    [ObservableProperty] private ObservableCollection<TeachingUnitInfo> _units = new();

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasNoUnits))]
    private bool _hasUnits;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CountLabel))]
    private int _totalCount;

    public bool HasNoUnits => !HasUnits;

    public string CountLabel => string.Format(
        CultureInfo.CurrentCulture,
        LangViewModel.Translate("HomeCardUnitCount"), TotalCount);

    private void Apply(BulletinData? data)
    {
        if (data is null)
            return;

        IEnumerable<TeachingUnitInfo> source = data.Units;

        if (!string.IsNullOrEmpty(UnitCode))
            source = source.Where(u => string.Equals(u.Code, UnitCode, StringComparison.OrdinalIgnoreCase));

        var all = source.ToList();

        Units = new ObservableCollection<TeachingUnitInfo>(all.Take(MaxItems));
        TotalCount = all.Count;
        HasUnits = Units.Count > 0;
    }
}
