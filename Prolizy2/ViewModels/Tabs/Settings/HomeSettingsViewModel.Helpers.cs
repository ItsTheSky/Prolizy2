using System;
using System.Collections.Generic;
using System.Globalization;
using Avalonia.Data;
using Avalonia.Data.Converters;
using Prolizy2.Model.Home;
using Prolizy2.ViewModels.Common;

namespace Prolizy2.ViewModels.Tabs.Settings;

/// <summary>A home-card kind offered by the "add a card" list.</summary>
public sealed class HomeCardTypeOption
{
    public HomeCardTypeOption(HomeCardType type) => Type = type;

    public HomeCardType Type { get; }

    public string Title => Type switch
    {
        HomeCardType.NextCourse => LangViewModel.Translate("HomeCardTypeNextCourse"),
        HomeCardType.BulletinOverview => LangViewModel.Translate("HomeCardTypeOverview"),
        HomeCardType.EvaluationDetail => LangViewModel.Translate("HomeCardTypeEvaluation"),
        _ => LangViewModel.Translate("HomeCardTypeUnits")
    };

    public string Icon => Type switch
    {
        HomeCardType.NextCourse => "CalendarClock",
        HomeCardType.BulletinOverview => "GraduationCap",
        HomeCardType.EvaluationDetail => "NotebookPen",
        _ => "Layers"
    };
}

/// <summary>A semester choice for the overview card.</summary>
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

/// <summary>A resource / SAÉ choice for the evaluation card.</summary>
public sealed class EvaluationChoice
{
    public EvaluationChoice(string code, string label, string kind)
    {
        Code = code;
        Label = string.IsNullOrEmpty(label) ? code : label;
        Kind = kind;
    }

    public string Code { get; }
    public string Label { get; }
    public string Kind { get; }
    public string Display => $"{Code} · {Label}";
}

/// <summary>A teaching-unit choice for the unit-list card.</summary>
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

/// <summary>Translates a <see cref="HomeCardType"/> into its localised label.</summary>
public sealed class HomeCardTypeLabelConverter : IValueConverter
{
    public static readonly HomeCardTypeLabelConverter Instance = new();

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value is HomeCardType type
            ? type switch
            {
                HomeCardType.NextCourse => LangViewModel.Translate("HomeCardTypeNextCourse"),
                HomeCardType.BulletinOverview => LangViewModel.Translate("HomeCardTypeOverview"),
                HomeCardType.EvaluationDetail => LangViewModel.Translate("HomeCardTypeEvaluation"),
                _ => LangViewModel.Translate("HomeCardTypeUnits")
            }
            : value?.ToString() ?? string.Empty;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}

/// <summary>Bidirectional converter between a <see cref="HomeCardSize"/> and "half"/"full".</summary>
public sealed class HomeCardSizeConverter : IValueConverter
{
    public static readonly HomeCardSizeConverter Instance = new();

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not HomeCardSize size)
            return false;

        return size == (parameter is string s && s == "full" ? HomeCardSize.Full : HomeCardSize.Half);
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not bool checkedValue)
            return BindingOperations.DoNothing;

        var isFull = parameter is string s && s == "full";
        return checkedValue ? (isFull ? HomeCardSize.Full : HomeCardSize.Half)
                            : (isFull ? HomeCardSize.Half : HomeCardSize.Full);
    }
}

/// <summary>Bidirectional converter between a <see cref="HomeCardVariant"/> and "classic"/"compact".</summary>
public sealed class HomeCardVariantConverter : IValueConverter
{
    public static readonly HomeCardVariantConverter Instance = new();

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not HomeCardVariant variant)
            return false;

        return variant == (parameter is string s && s == "compact" ? HomeCardVariant.Compact : HomeCardVariant.Classic);
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not bool checkedValue)
            return BindingOperations.DoNothing;

        var isCompact = parameter is string s && s == "compact";
        return checkedValue ? (isCompact ? HomeCardVariant.Compact : HomeCardVariant.Classic)
                            : (isCompact ? HomeCardVariant.Classic : HomeCardVariant.Compact);
    }
}

/// <summary>True when a <see cref="HomeCardType"/> is one of a given set of kinds.</summary>
public sealed class CardTypeCheckConverter : IValueConverter
{
    private readonly HashSet<HomeCardType> _types;

    public CardTypeCheckConverter(params HomeCardType[] types) => _types = new HashSet<HomeCardType>(types);

    /// <summary>True for any card that shows a picker (semester, resource, unit).</summary>
    public static readonly CardTypeCheckConverter Pickers = new(
        HomeCardType.BulletinOverview, HomeCardType.EvaluationDetail, HomeCardType.UnitList);

    public static readonly CardTypeCheckConverter Semester = new(HomeCardType.BulletinOverview);
    public static readonly CardTypeCheckConverter Evaluation = new(HomeCardType.EvaluationDetail);
    public static readonly CardTypeCheckConverter Unit = new(HomeCardType.UnitList);

    /// <summary>True for cards whose row count can be configured.</summary>
    public static readonly CardTypeCheckConverter List = new(HomeCardType.EvaluationDetail, HomeCardType.UnitList);

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        value is HomeCardType type && _types.Contains(type);

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}

#endregion