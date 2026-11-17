using System;
using System.Collections.Generic;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Prolizy2.Model.Home;

/// <summary>
/// A single user-placed card on the home screen: what it shows, how wide it is,
/// and the per-kind options (chosen resource, bulletin semester, …).
/// </summary>
public partial class HomeCardConfig : ObservableObject
{
    /// <summary>Stable identifier used to match the saved configuration.</summary>
    public string Id { get; init; } = Guid.NewGuid().ToString("N");

    [ObservableProperty] private HomeCardType _type;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsFullWidth))]
    private HomeCardSize _size = HomeCardSize.Half;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsCompact))]
    private HomeCardVariant _variant = HomeCardVariant.Classic;

    /// <summary>Code of the chosen resource / SAÉ — <see cref="HomeCardType.EvaluationDetail"/>.</summary>
    [ObservableProperty] private string? _groupCode;

    /// <summary><c>formsemestre_id</c> of the bulletin semester to display (null = current).</summary>
    [ObservableProperty] private string? _semesterId;

    /// <summary>Code of the chosen teaching unit (null = show the full list) — <see cref="HomeCardType.UnitList"/>.</summary>
    [ObservableProperty] private string? _unitCode;

    /// <summary>Number of rows shown by the list-like cards.</summary>
    [ObservableProperty] private int _maxItems = 4;

    public bool IsFullWidth => Size == HomeCardSize.Full;

    public bool IsCompact => Variant == HomeCardVariant.Compact;

    /// <summary>Clones this card, used when the settings page adds a new one.</summary>
    public HomeCardConfig Clone() => new()
    {
        Id = Id,
        Type = Type,
        Size = Size,
        Variant = Variant,
        GroupCode = GroupCode,
        SemesterId = SemesterId,
        UnitCode = UnitCode,
        MaxItems = MaxItems
    };

    // ---- Persistence DTO -------------------------------------------------

    /// <summary>Flat, serialisable form of the configuration list.</summary>
    public sealed class Dto
    {
        public string Id { get; set; } = string.Empty;
        public int Type { get; set; }
        public int Size { get; set; }
        public int Variant { get; set; }
        public string? GroupCode { get; set; }
        public string? SemesterId { get; set; }
        public string? UnitCode { get; set; }
        public int MaxItems { get; set; } = 4;
    }

    public static HomeCardConfig FromDto(Dto dto) => new()
    {
        Id = string.IsNullOrEmpty(dto.Id) ? Guid.NewGuid().ToString("N") : dto.Id,
        Type = Enum.IsDefined(typeof(HomeCardType), dto.Type) ? (HomeCardType)dto.Type : HomeCardType.NextCourse,
        Size = Enum.IsDefined(typeof(HomeCardSize), dto.Size) ? (HomeCardSize)dto.Size : HomeCardSize.Half,
        Variant = Enum.IsDefined(typeof(HomeCardVariant), dto.Variant) ? (HomeCardVariant)dto.Variant : HomeCardVariant.Classic,
        GroupCode = dto.GroupCode,
        SemesterId = dto.SemesterId,
        UnitCode = dto.UnitCode,
        MaxItems = dto.MaxItems is > 0 and <= 12 ? dto.MaxItems : 4
    };

    public Dto ToDto() => new()
    {
        Id = Id,
        Type = (int)Type,
        Size = (int)Size,
        Variant = (int)Variant,
        GroupCode = GroupCode,
        SemesterId = SemesterId,
        UnitCode = UnitCode,
        MaxItems = MaxItems
    };

    /// <summary>The cards shown on first launch, before the user customises them.</summary>
    public static IReadOnlyList<HomeCardConfig> Defaults() => new List<HomeCardConfig>
    {
        new() { Type = HomeCardType.NextCourse, Size = HomeCardSize.Full },
        new() { Type = HomeCardType.BulletinOverview, Size = HomeCardSize.Half },
        new() { Type = HomeCardType.UnitList, Size = HomeCardSize.Half }
    };
}