using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Prolizy2.Views;

namespace Prolizy2.Model.Home;

/// <summary>
/// The user's home layout: an ordered list of <see cref="HomeCardConfig"/> kept
/// in <see cref="MainView.Preferences"/>. Every card owns its own settings and
/// the list order is the display order.
/// </summary>
public static class HomeLayout
{
    private const string PreferenceKey = "home_layout";

    /// <summary>Raised whenever the layout is loaded, changed or reset.</summary>
    public static event Action? LayoutChanged;

    private static ObservableCollection<HomeCardConfig>? _cards;

    /// <summary>The cards currently configured, in display order.</summary>
    public static ObservableCollection<HomeCardConfig> Cards => _cards ??= Load();

    /// <summary>Replaces the current cards (used by the settings editor).</summary>
    public static void ReplaceAll(IEnumerable<HomeCardConfig> cards)
    {
        Cards.Clear();
        foreach (var card in cards)
            Cards.Add(card);
        Save();
    }

    /// <summary>Persists the current layout.</summary>
    public static void Save()
    {
        if (Avalonia.Controls.Design.IsDesignMode)
            return;

        MainView.Preferences.Set(PreferenceKey, Cards.Select(c => c.ToDto()).ToList());
        LayoutChanged?.Invoke();
    }

    private static ObservableCollection<HomeCardConfig> Load()
    {
        var cards = new ObservableCollection<HomeCardConfig>();

        List<HomeCardConfig.Dto>? saved = null;
        if (!Avalonia.Controls.Design.IsDesignMode)
            saved = MainView.Preferences.Get<List<HomeCardConfig.Dto>?>(PreferenceKey, null);

        if (saved is { Count: > 0 })
        {
            foreach (var dto in saved)
                cards.Add(HomeCardConfig.FromDto(dto));
            return cards;
        }

        foreach (var card in HomeCardConfig.Defaults())
            cards.Add(card);
        return cards;
    }
}