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
                // Read the document directly instead of going through the
                // reflection-based serializer: this runs during start-up.
                var cache = new Dictionary<string, object>();
                ReadOnlyMemory<byte> bytes = File.ReadAllBytes(_filePath);
                if (bytes.Length >= 3 && bytes.Span[0] == 0xEF && bytes.Span[1] == 0xBB && bytes.Span[2] == 0xBF)
                    bytes = bytes[3..]; // tolerate a UTF-8 byte-order mark
                using var document = JsonDocument.Parse(bytes);
                if (document.RootElement.ValueKind == JsonValueKind.Object)
                {
                    foreach (var property in document.RootElement.EnumerateObject())
                        cache[property.Name] = property.Value.Clone();
                }

                _cache = cache;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Preferences] Erreur de chargement: {ex.Message}");
            _cache = new Dictionary<string, object>();
        }
    }

    // Writes are queued on a background task so changing a setting never makes
    // the UI thread wait for the disk. Each write carries a full snapshot, and
    // they are chained so the last one always wins.
    private Task _pendingWrite = Task.CompletedTask;

    /// <summary>
    /// Sauvegarde les préférences sur le disque (en arrière-plan)
    /// </summary>
    private void SaveToDisk()
    {
        // Called under _lock: take the snapshot now, write it later.
        byte[] snapshot;
        using (var buffer = new MemoryStream())
        {
            using (var writer = new Utf8JsonWriter(buffer, new JsonWriterOptions { Indented = true }))
            {
                writer.WriteStartObject();
                foreach (var (key, value) in _cache)
                {
                    writer.WritePropertyName(key);
                    if (value is JsonElement element)
                        element.WriteTo(writer);
                    else
                        JsonSerializer.Serialize(writer, value, _jsonOptions);
                }
                writer.WriteEndObject();
            }

            snapshot = buffer.ToArray();
        }

        _pendingWrite = _pendingWrite.ContinueWith(_ =>
        {
            try
            {
                // Temp file + rename: a killed process never leaves a truncated file.
                var tempPath = _filePath + ".tmp";
                File.WriteAllBytes(tempPath, snapshot);
                File.Move(tempPath, _filePath, overwrite: true);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Preferences] Erreur de sauvegarde: {ex.Message}");
            }
        }, TaskScheduler.Default);
    }

    /// <summary>Waits until every queued write has reached the disk.</summary>
    public Task FlushAsync()
    {
        lock (_lock)
        {
            return _pendingWrite;
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
                    // Fast paths for the types read on hot paths (several times
                    // per tap): no serializer round-trip for a string or a bool.
                    if (typeof(T) == typeof(string))
                    {
                        if (jsonElement.ValueKind == JsonValueKind.String)
                            return (T)(object)jsonElement.GetString()!;
                        if (jsonElement.ValueKind == JsonValueKind.Null)
                            return (T)(object)null!;
                    }
                    else if (typeof(T) == typeof(bool))
                    {
                        if (jsonElement.ValueKind == JsonValueKind.True)
                            return (T)(object)true;
                        if (jsonElement.ValueKind == JsonValueKind.False)
                            return (T)(object)false;
                    }

                    return jsonElement.Deserialize<T>(_jsonOptions);
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