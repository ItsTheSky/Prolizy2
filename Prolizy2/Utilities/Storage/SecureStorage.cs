namespace Prolizy2.Utilities.Storage;

using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

/// <summary>
/// Stockage sécurisé (chiffré) cross-platform pour données sensibles
/// Utilise AES-256 avec dérivation de clé PBKDF2
/// </summary>
public class SecureStorage
{
    private readonly string _filePath;
    private readonly SemaphoreSlim _lock = new(1, 1);
    private Dictionary<string, string> _cache;
    private readonly byte[] _entropy;

    private const int KeySize = 256;
    private const int SaltSize = 32;
    private const int Iterations = 100000;

    /// <summary>
    /// Initialise le stockage sécurisé
    /// </summary>
    public SecureStorage(string appName = "AvaloniaApp")
    {
        _filePath = GetSecureStoragePath(appName);
        _entropy = GetOrCreateEntropy(appName);
        _cache = new Dictionary<string, string>();
        LoadFromDisk();
    }

    /// <summary>
    /// Obtient le chemin du fichier sécurisé selon la plateforme
    /// </summary>
    private static string GetSecureStoragePath(string appName)
    {
        string baseDir;

        if (OperatingSystem.IsAndroid())
        {
            baseDir = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
        }
        else if (OperatingSystem.IsIOS() || OperatingSystem.IsMacCatalyst())
        {
            var libraryPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            baseDir = Path.Combine(Directory.GetParent(libraryPath)!.FullName, "Library", "Application Support");
        }
        else
        {
            baseDir = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        }

        var appDir = Path.Combine(baseDir, appName, ".secure");
        Directory.CreateDirectory(appDir);

        return Path.Combine(appDir, "secure.dat");
    }

    /// <summary>
    /// Génère ou récupère l'entropy pour la dérivation de clé
    /// </summary>
    private byte[] GetOrCreateEntropy(string appName)
    {
        var entropyPath = Path.Combine(
            Path.GetDirectoryName(_filePath)!,
            ".entropy"
        );

        if (File.Exists(entropyPath))
        {
            return File.ReadAllBytes(entropyPath);
        }

        // Génère une entropy unique basée sur l'appareil
        var deviceId = GetDeviceIdentifier();
        var entropy = SHA256.HashData(Encoding.UTF8.GetBytes($"{appName}:{deviceId}"));

        File.WriteAllBytes(entropyPath, entropy);

        // Masque le fichier sur Windows
        if (OperatingSystem.IsWindows())
        {
            File.SetAttributes(entropyPath, FileAttributes.Hidden | FileAttributes.System);
        }

        return entropy;
    }

    /// <summary>
    /// Obtient un identifiant unique de l'appareil
    /// </summary>
    private string GetDeviceIdentifier()
    {
        try
        {
            if (OperatingSystem.IsWindows())
            {
                return Environment.MachineName + Environment.UserName;
            }
            else if (OperatingSystem.IsLinux() || OperatingSystem.IsMacOS())
            {
                var machineId = File.Exists("/etc/machine-id")
                    ? File.ReadAllText("/etc/machine-id").Trim()
                    : Environment.MachineName;
                return machineId + Environment.UserName;
            }
            else if (OperatingSystem.IsAndroid())
            {
                // Sur Android, utiliser Android.Provider.Settings.Secure.ANDROID_ID
                // Pour l'instant, on utilise un fallback
                return "android_" + Environment.UserName;
            }
            else
            {
                return Environment.MachineName + Environment.UserName;
            }
        }
        catch
        {
            return Guid.NewGuid().ToString();
        }
    }

    /// <summary>
    /// Chiffre les données avec AES-256
    /// </summary>
    private byte[] Encrypt(string plainText)
    {
        using var aes = Aes.Create();
        aes.KeySize = KeySize;

        // Génère un salt aléatoire
        var salt = RandomNumberGenerator.GetBytes(SaltSize);

        // Dérive la clé à partir de l'entropy
        using var keyDerivation = new Rfc2898DeriveBytes(
            _entropy,
            salt,
            Iterations,
            HashAlgorithmName.SHA256
        );

        aes.Key = keyDerivation.GetBytes(KeySize / 8);
        aes.GenerateIV();

        using var encryptor = aes.CreateEncryptor();
        var plainBytes = Encoding.UTF8.GetBytes(plainText);
        var cipherBytes = encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length);

        // Format: [Salt(32)] + [IV(16)] + [CipherText]
        var result = new byte[salt.Length + aes.IV.Length + cipherBytes.Length];
        Buffer.BlockCopy(salt, 0, result, 0, salt.Length);
        Buffer.BlockCopy(aes.IV, 0, result, salt.Length, aes.IV.Length);
        Buffer.BlockCopy(cipherBytes, 0, result, salt.Length + aes.IV.Length, cipherBytes.Length);

        return result;
    }

    /// <summary>
    /// Déchiffre les données
    /// </summary>
    private string Decrypt(byte[] encryptedData)
    {
        using var aes = Aes.Create();
        aes.KeySize = KeySize;

        // Extrait le salt
        var salt = new byte[SaltSize];
        Buffer.BlockCopy(encryptedData, 0, salt, 0, SaltSize);

        // Extrait l'IV
        var iv = new byte[16];
        Buffer.BlockCopy(encryptedData, SaltSize, iv, 0, 16);

        // Extrait le texte chiffré
        var cipherBytes = new byte[encryptedData.Length - SaltSize - 16];
        Buffer.BlockCopy(encryptedData, SaltSize + 16, cipherBytes, 0, cipherBytes.Length);

        // Dérive la clé
        using var keyDerivation = new Rfc2898DeriveBytes(
            _entropy,
            salt,
            Iterations,
            HashAlgorithmName.SHA256
        );

        aes.Key = keyDerivation.GetBytes(KeySize / 8);
        aes.IV = iv;

        using var decryptor = aes.CreateDecryptor();
        var plainBytes = decryptor.TransformFinalBlock(cipherBytes, 0, cipherBytes.Length);

        return Encoding.UTF8.GetString(plainBytes);
    }

    /// <summary>
    /// Charge les données chiffrées depuis le disque
    /// </summary>
    private void LoadFromDisk()
    {
        try
        {
            if (!File.Exists(_filePath))
            {
                _cache = new Dictionary<string, string>();
                return;
            }

            var encryptedData = File.ReadAllBytes(_filePath);
            var json = Decrypt(encryptedData);
            _cache = JsonSerializer.Deserialize<Dictionary<string, string>>(json)
                     ?? new Dictionary<string, string>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[SecureStorage] Erreur de chargement: {ex.Message}");
            _cache = new Dictionary<string, string>();
        }
    }

    /// <summary>
    /// Sauvegarde les données chiffrées sur le disque
    /// </summary>
    private async Task SaveToDiskAsync()
    {
        try
        {
            var json = JsonSerializer.Serialize(_cache);
            var encryptedData = Encrypt(json);
            await File.WriteAllBytesAsync(_filePath, encryptedData);

            // Définit les permissions sur Unix
            if (OperatingSystem.IsLinux() || OperatingSystem.IsMacOS())
            {
                File.SetUnixFileMode(_filePath, UnixFileMode.UserRead | UnixFileMode.UserWrite);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[SecureStorage] Erreur de sauvegarde: {ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// Stocke une valeur de manière sécurisée
    /// </summary>
    public async Task<bool> SetAsync(string key, string value)
    {
        if (string.IsNullOrWhiteSpace(key))
            throw new ArgumentException("La clé ne peut pas être vide", nameof(key));

        await _lock.WaitAsync();
        try
        {
            _cache[key] = value;
            await SaveToDiskAsync();
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[SecureStorage] Erreur SetAsync: {ex.Message}");
            return false;
        }
        finally
        {
            _lock.Release();
        }
    }

    /// <summary>
    /// Récupère une valeur sécurisée
    /// </summary>
    public async Task<string> GetAsync(string key, string defaultValue = null!)
    {
        await _lock.WaitAsync();
        try
        {
            return _cache.TryGetValue(key, out var value) ? value : defaultValue;
        }
        finally
        {
            _lock.Release();
        }
    }

    /// <summary>
    /// Vérifie si une clé existe
    /// </summary>
    public bool ContainsKey(string key)
    {
        _lock.Wait();
        try
        {
            return _cache.ContainsKey(key);
        }
        finally
        {
            _lock.Release();
        }
    }

    /// <summary>
    /// Supprime une clé
    /// </summary>
    public async Task<bool> RemoveAsync(string key)
    {
        await _lock.WaitAsync();
        try
        {
            if (!_cache.Remove(key))
                return false;

            await SaveToDiskAsync();
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[SecureStorage] Erreur RemoveAsync: {ex.Message}");
            return false;
        }
        finally
        {
            _lock.Release();
        }
    }

    /// <summary>
    /// Efface toutes les données sécurisées
    /// </summary>
    public async Task<bool> ClearAsync()
    {
        await _lock.WaitAsync();
        try
        {
            _cache.Clear();
            await SaveToDiskAsync();
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[SecureStorage] Erreur ClearAsync: {ex.Message}");
            return false;
        }
        finally
        {
            _lock.Release();
        }
    }

    /// <summary>
    /// Récupère toutes les clés
    /// </summary>
    public IEnumerable<string> GetAllKeys()
    {
        _lock.Wait();
        try
        {
            return new List<string>(_cache.Keys);
        }
        finally
        {
            _lock.Release();
        }
    }
}