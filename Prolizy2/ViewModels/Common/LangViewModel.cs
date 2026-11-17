using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using Avalonia;
using Avalonia.Data.Converters;
using Avalonia.Markup.Xaml;
using Avalonia.Markup.Xaml.Styling;
using Avalonia.Styling;
using Avalonia.Svg;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Prolizy2.Utilities;

namespace Prolizy2.ViewModels.Common;

public partial class LangViewModel : ObservableObject
{
    public MainViewModel MainView { get; init; }
    
    [ObservableProperty] private CultureInfo _currentCulture;
    [ObservableProperty] private ObservableCollection<LangEntry> _langs;
    [ObservableProperty] private LangEntry _selectedValue;
    
    public event Action<CultureInfo> CurrentCultureChanged;

    public LangViewModel(MainViewModel mainVm)
    {
        MainView = mainVm;

        // No intermediate culture here: selecting the first entry below loads the
        // language dictionary exactly once (it used to be loaded three times).
        List<CultureInfo> availableCultures =
        [
            new("fr-FR"),
            new("en-US"),
        ];
        
        Langs = new ObservableCollection<LangEntry>(availableCultures.Select(x => new LangEntry(x, this)));
        SelectedValue = Langs.First();
    }
    
    public void ChangeLanguage(CultureInfo cultureInfo)
    {
        if (cultureInfo != null! && !Equals(CurrentCulture, cultureInfo))
        {
            // The generated setter already runs OnCurrentCultureChanged.
            CurrentCulture = cultureInfo;
        }
    }
    
    partial void OnSelectedValueChanged(LangEntry value)
    {
        if (value != null!)
        {
            ChangeLanguage(value.Culture);
        }
    }

    partial void OnCurrentCultureChanged(CultureInfo value)
    {
        if (value != null!)
        {
            CultureInfo.DefaultThreadCurrentCulture = value;
            CultureInfo.DefaultThreadCurrentUICulture = value;

            var translations = Application.Current!.Resources.MergedDictionaries
                .OfType<ResourceInclude>()
                .FirstOrDefault(x => x.Source?.OriginalString.Contains("/Lang/") ?? false);
            if (translations != null)
                Application.Current.Resources.MergedDictionaries.Remove(translations);

            var dict = new ResourceInclude(new Uri($"avares://Prolizy2/Assets/Lang/{value}.axaml"))
            {
                Source = new Uri($"avares://Prolizy2/Assets/Lang/{value}.axaml")
            };
            Application.Current.Resources.MergedDictionaries.Add(dict);

            CurrentCultureChanged?.Invoke(value);
            Logs.Debug($"LANG: CurrentCulture: {value}");
        }
    }

    public static string Translate(string key)
    {
        if (Application.Current!.TryGetResource(key, ThemeVariant.Default, out var value))
            return value?.ToString() ?? "Not Found [" + key + "]";

        return "Not Found [" + key + "]";
    }
}

public class LangMarkupExtension(string key) : MarkupExtension
{
    public string Key { get; set; } = key;

    public override object ProvideValue(IServiceProvider serviceProvider)
    {
        object? value = null;
        var resource = Application.Current?.Resources
            .TryGetResource(Key, ThemeVariant.Default, out value) ?? false;

        return value ?? Key;
    }
}

public class LangKeyToValueConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is string key)
        {
            object val = null!;
            var resource = Application.Current?.Resources
                .TryGetResource(key, ThemeVariant.Default, out val) ?? false;

            return val ?? key;
        }

        return value;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

public partial class LangEntry : ObservableObject
{
    
    [ObservableProperty] private string _displayName;
    [ObservableProperty] private CultureInfo _culture;

    private SvgImage? _icon;

    /// <summary>
    /// Flag of the language. Parsed on first use only: it is shown on a single
    /// settings page, so there is no reason to load the SVG stack at start-up.
    /// </summary>
    public SvgImage Icon => _icon ??= new SvgImage
    {
        Source = SvgSource.Load($"avares://Prolizy2/Assets/Lang/Icons/{Culture}.svg", null)
    };

    public LangEntry(CultureInfo cultureInfo, LangViewModel langVm)
    {
        Culture = cultureInfo;
        
        DisplayName = cultureInfo.DisplayName;
    }
    
}