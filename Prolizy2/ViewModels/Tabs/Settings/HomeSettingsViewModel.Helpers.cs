using System;
using System.Collections.Generic;
using System.Globalization;
using Avalonia.Data;
using Avalonia.Data.Converters;
using Prolizy2.Model.Home;
using Prolizy2.ViewModels.Common;

namespace Prolizy2.ViewModels.Tabs.Settings;

public sealed class HomeCardTypeOption
{
    public HomeCardTypeOption(HomeCardType type) => Type = type;

    public HomeCardType Type { get; }

    public string Title => HomeCardTypeLabelConverter.Translate(Type);
}

public sealed class SemesterChoice
{
    public SemesterChoice(string? id, string label)
    {
        Id = id;
        Label = label;
    }

    /// <summary><c>formsemestre_id</c>, or null for the current semester.</summary>
    public string? Id { get; }

    public string Label { get; }
}

public sealed class EvaluationChoice
{
    public EvaluationChoice(string code, string label)
    {
        Code = code;
        Label = string.IsNullOrEmpty(label) ? code : label;
    }

    public string Code { get; }
    public string Label { get; }
    public string Display => $"{Code} · {Label}";
}

public sealed class UnitChoice
{
    public UnitChoice(string code, string name)
    {
        Code = code;
        Label = string.IsNullOrEmpty(name) ? code : name;
    }

    public string Code { get; }
    public string Label { get; }
    public string Display => $"{Code} · {Label}";
}

#region Converters

public sealed class HomeCardTypeLabelConverter : IValueConverter
{
    public static readonly HomeCardTypeLabelConverter Instance = new();

    public static string Translate(HomeCardType type) => type switch
    {
        HomeCardType.NextCourse => LangViewModel.Translate("HomeCardTypeNextCourse"),
        HomeCardType.BulletinOverview => LangViewModel.Translate("HomeCardTypeOverview"),
        HomeCardType.EvaluationDetail => LangViewModel.Translate("HomeCardTypeEvaluation"),
        _ => LangViewModel.Translate("HomeCardTypeUnits")
    };

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        value is HomeCardType type ? Translate(type) : value?.ToString() ?? string.Empty;

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}

/// <summary>Maps a radio button (parameter "half" / "full") to a <see cref="HomeCardSize"/>.</summary>
public sealed class HomeCardSizeConverter : IValueConverter
{
    public static readonly HomeCardSizeConverter Instance = new();

    private static HomeCardSize FromParameter(object? parameter) =>
        parameter is "full" ? HomeCardSize.Full : HomeCardSize.Half;

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        value is HomeCardSize size && size == FromParameter(parameter);

    // Only the radio being checked writes back; the one being unchecked must not
    // overwrite the value its sibling just set.
    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        value is true ? FromParameter(parameter) : BindingOperations.DoNothing;
}

/// <summary>Maps a radio button (parameter "classic" / "compact") to a <see cref="HomeCardVariant"/>.</summary>
public sealed class HomeCardVariantConverter : IValueConverter
{
    public static readonly HomeCardVariantConverter Instance = new();

    private static HomeCardVariant FromParameter(object? parameter) =>
        parameter is "compact" ? HomeCardVariant.Compact : HomeCardVariant.Classic;

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        value is HomeCardVariant variant && variant == FromParameter(parameter);

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        value is true ? FromParameter(parameter) : BindingOperations.DoNothing;
}

public sealed class CardTypeCheckConverter : IValueConverter
{
    private readonly HashSet<HomeCardType> _types;

    public CardTypeCheckConverter(params HomeCardType[] types) => _types = new HashSet<HomeCardType>(types);

    public static readonly CardTypeCheckConverter Pickers = new(
        HomeCardType.BulletinOverview, HomeCardType.EvaluationDetail, HomeCardType.UnitList);

    public static readonly CardTypeCheckConverter Semester = new(HomeCardType.BulletinOverview);
    public static readonly CardTypeCheckConverter Evaluation = new(HomeCardType.EvaluationDetail);
    public static readonly CardTypeCheckConverter Unit = new(HomeCardType.UnitList);
    public static readonly CardTypeCheckConverter List = new(HomeCardType.EvaluationDetail, HomeCardType.UnitList);

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        value is HomeCardType type && _types.Contains(type);

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}

#endregion
