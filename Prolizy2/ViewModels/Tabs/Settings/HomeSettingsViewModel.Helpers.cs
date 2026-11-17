using System;
using System.Collections.Generic;
using System.Globalization;
using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Media;
using Lucide.Avalonia;
using Prolizy2.Model.Home;
using Prolizy2.ViewModels.Common;
using Prolizy2.ViewModels.Home;

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
    /// <param name="code">Unit code, or null for "all units".</param>
    public UnitChoice(string? code, string name)
    {
        Code = code;
        Label = string.IsNullOrEmpty(name) ? code ?? string.Empty : name;
    }

    public string? Code { get; }
    public string Label { get; }
    public string Display => Code is null ? Label : $"{Code} · {Label}";
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

/// <summary>True for <see cref="HomeCardSize.Full"/>, the second option of the width switch.</summary>
public sealed class HomeCardSizeConverter : IValueConverter
{
    public static readonly HomeCardSizeConverter Instance = new();

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        value is HomeCardSize.Full;

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        value is bool full ? (full ? HomeCardSize.Full : HomeCardSize.Half) : BindingOperations.DoNothing;
}

/// <summary>True for <see cref="HomeCardVariant.Compact"/>, the second option of the style switch.</summary>
public sealed class HomeCardVariantConverter : IValueConverter
{
    public static readonly HomeCardVariantConverter Instance = new();

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        value is HomeCardVariant.Compact;

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        value is bool compact ? (compact ? HomeCardVariant.Compact : HomeCardVariant.Classic) : BindingOperations.DoNothing;
}

public static class HomeCardTypeConverters
{
    public static readonly IValueConverter Icon =
        new FuncValueConverter<HomeCardType, LucideIconKind>(HomeCardVisuals.Icon);

    public static readonly IValueConverter Accent =
        new FuncValueConverter<HomeCardType, IBrush>(HomeCardVisuals.AccentBrush);

    public static readonly IValueConverter AccentTint =
        new FuncValueConverter<HomeCardType, IBrush>(HomeCardVisuals.AccentTintBrush);
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
