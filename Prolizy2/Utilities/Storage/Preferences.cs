namespace Prolizy2.Utilities.Storage;

using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

/// <summary>
/// Stockage de préférences cross-platform optimisé pour Avalonia (version synchrone)
/// Supporte: Windows, Linux, macOS, Android, iOS
/// </summary>
public class Preferences
{
    private readonly string _filePath;
    private readonly object _lock = new();
    private Dictionary<string, object> _cache;

    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true
    };

    /// <summary>
    /// Initialise le système de préférences avec un nom d'application
    /// </summary>
    public Preferences(string appName = "AvaloniaApp")
    {
        _filePath = GetPreferencesPath(appName);
        _cache = new Dictionary<string, object>();
        LoadFromDisk();
    }

    /// <summary>
    /// Obtient le chemin du fichier de préférences selon la plateforme
    /// </summary>
    private static string GetPreferencesPath(string appName)
    {
        string baseDir;

        if (OperatingSystem.IsAndroid())
        {
            // Android: Utilise le répertoire interne de l'application
            baseDir = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
        }
        else if (OperatingSystem.IsIOS() || OperatingSystem.IsMacCatalyst())
        {
            // iOS: Library/Preferences
            var libraryPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            baseDir = Path.Combine(Directory.GetParent(libraryPath)!.FullName, "Library", "Preferences");
        }
        else
        {
            // Windows, Linux, macOS Desktop
            baseDir = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        }

        var appDir = Path.Combine(baseDir, appName);
        Directory.CreateDirectory(appDir);

        return Path.Combine(appDir, "preferences.json");
    }

    /// <summary>
    /// Charge les préférences depuis le disque
    /// </summary>
    private void LoadFromDisk()
    {
        try
        {
            if (File.Exists(_filePath))
            {
                var json = File.ReadAllText(_filePath);
                _cache = JsonSerializer.Deserialize<Dictionary<string, object>>(json, _jsonOptions)
                         ?? new Dictionary<string, object>();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Preferences] Erreur de chargement: {ex.Message}");
            _cache = new Dictionary<string, object>();
        }
    }

    /// <summary>
    /// Sauvegarde les préférences sur le disque
    /// </summary>
    private void SaveToDisk()
    {
        try
        {
            var json = JsonSerializer.Serialize(_cache, _jsonOptions);
            File.WriteAllText(_filePath, json);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Preferences] Erreur de sauvegarde: {ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// Définit une valeur (type simple ou complexe)
    /// </summary>
    public bool Set<T>(string key, T value)
    {
        if (string.IsNullOrWhiteSpace(key))
            throw new ArgumentException("La clé ne peut pas être vide", nameof(key));

        lock (_lock)
        {
            try
            {
                // Convertir en JsonElement pour gérer tous les types
                var jsonValue = JsonSerializer.SerializeToElement(value, _jsonOptions);
                _cache[key] = jsonValue;

                SaveToDisk();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Preferences] Erreur Set: {ex.Message}");
                return false;
            }
        }
    }

    /// <summary>
    /// Obtient une valeur avec fallback
    /// </summary>
    public T Get<T>(string key, T defaultValue = default)
    {
        lock (_lock)
        {
            try
            {
                if (!_cache.TryGetValue(key, out var value))
                    return defaultValue;

                if (value is JsonElement jsonElement)
                {
                    return JsonSerializer.Deserialize<T>(jsonElement.GetRawText(), _jsonOptions);
                }

                if (value is T typedValue)
                {
                    return typedValue;
                }

                // Tentative de conversion via JSON
                var json = JsonSerializer.Serialize(value, _jsonOptions);
                return JsonSerializer.Deserialize<T>(json, _jsonOptions);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Preferences] Erreur Get: {ex.Message}");
                return defaultValue;
            }
        }
    }

    /// <summary>
    /// Vérifie si une clé existe
    /// </summary>
    public bool ContainsKey(string key)
    {
        lock (_lock)
        {
            return _cache.ContainsKey(key);
        }
    }

    /// <summary>
    /// Supprime une clé
    /// </summary>
    public bool Remove(string key)
    {
        lock (_lock)
        {
            try
            {
                if (!_cache.Remove(key))
                    return false;

                SaveToDisk();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Preferences] Erreur Remove: {ex.Message}");
                return false;
            }
        }
    }

    /// <summary>
    /// Efface toutes les préférences
    /// </summary>
    public int Clear()
    {
        lock (_lock)
        {
            try
            {
                var count = _cache.Count;
                _cache.Clear();
                SaveToDisk();
                return count;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Preferences] Erreur Clear: {ex.Message}");
                return -1;
            }
        }
    }

    /// <summary>
    /// Récupère toutes les clés
    /// </summary>
    public IEnumerable<string> GetAllKeys()
    {
        lock (_lock)
        {
            return new List<string>(_cache.Keys);
        }
    }

    /// <summary>
    /// Recharge les préférences depuis le disque
    /// </summary>
    public void Reload()
    {
        lock (_lock)
        {
            LoadFromDisk();
        }
    }

    /// <summary>
    /// Obtient le chemin complet du fichier de préférences (utile pour debug)
    /// </summary>
    public string GetFilePath() => _filePath;
}