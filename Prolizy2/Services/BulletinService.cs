using System;
using System.Net;
using System.Threading.Tasks;
using Prolizy2.API;
using Prolizy2.API.Model;
using Prolizy2.Views;

namespace Prolizy2.Services;

/// <summary>
/// Manages the lifecycle of a <see cref="BulletinClient"/> for the application —
/// pulls credentials from <see cref="MainView.SecureStorage"/>, lazily logs in,
/// and exposes initial / per-semester fetches.
/// </summary>
public class BulletinService
{
    private const string UsernameKey = "bulletin_username";
    private const string PasswordKey = "bulletin_password";

    private BulletinClient? _client;
    private string? _cachedUsername;
    private string? _cachedPassword;

    /// <summary>The most recent payload returned by <see cref="LoadInitialAsync"/>.</summary>
    public BulletinRoot? LatestRoot { get; private set; }

    public bool IsConfigured => MainView.SecureStorage.ContainsKey(UsernameKey)
                                && MainView.SecureStorage.ContainsKey(PasswordKey);

    public async Task<string?> GetUsernameAsync() =>
        await MainView.SecureStorage.GetAsync(UsernameKey);

    public async Task<string?> GetPasswordAsync() =>
        await MainView.SecureStorage.GetAsync(PasswordKey);

    /// <summary>
    /// Persists the given credentials and forces the next fetch to log in again.
    /// </summary>
    public async Task SaveCredentialsAsync(string username, string password)
    {
        await MainView.SecureStorage.SetAsync(UsernameKey, username);
        await MainView.SecureStorage.SetAsync(PasswordKey, password);
        Invalidate();
    }

    public async Task ClearCredentialsAsync()
    {
        await MainView.SecureStorage.RemoveAsync(UsernameKey);
        await MainView.SecureStorage.RemoveAsync(PasswordKey);
        Invalidate();
    }

    /// <summary>
    /// Drops cached client/state so the next fetch builds a fresh client.
    /// </summary>
    public void Invalidate()
    {
        _client = null;
        _cachedUsername = null;
        _cachedPassword = null;
        LatestRoot = null;
    }

    /// <summary>
    /// Builds and (if needed) logs in a client using the stored credentials.
    /// Returns null when credentials are missing or the login fails.
    /// </summary>
    private async Task<BulletinClient?> EnsureClientAsync()
    {
        var username = await GetUsernameAsync();
        var password = await GetPasswordAsync();
        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            return null;

        if (_client is not null && _client.IsLoggedIn
            && _cachedUsername == username && _cachedPassword == password)
            return _client;

        _client = new BulletinClient { Username = username, Password = password };
        _cachedUsername = username;
        _cachedPassword = password;

        var status = await _client.Login();
        if (status != HttpStatusCode.OK)
        {
            _client = null;
            return null;
        }

        return _client;
    }

    /// <summary>
    /// Fetches the "premiere connexion" payload — semesters + initial transcript.
    /// Caches the result in <see cref="LatestRoot"/>.
    /// </summary>
    public async Task<BulletinRoot?> LoadInitialAsync()
    {
        var client = await EnsureClientAsync();
        if (client is null)
            return null;

        var root = await client.FetchDatas();
        if (root is not null)
            LatestRoot = root;

        return root;
    }

    /// <summary>
    /// Fetches the transcript for a specific <c>formsemestre_id</c>.
    /// </summary>
    public async Task<BulletinRoot?> LoadSemesterAsync(int formsemestreId)
    {
        var client = await EnsureClientAsync();
        if (client is null)
            return null;

        return await client.FetchDatas(formsemestreId);
    }

    /// <summary>
    /// Runs a one-shot login attempt with explicit credentials. Used by the
    /// settings page to validate before saving.
    /// </summary>
    public static async Task<HttpStatusCode> TestLoginAsync(string username, string password)
    {
        var probe = new BulletinClient { Username = username, Password = password };
        try
        {
            return await probe.Login();
        }
        catch (Exception)
        {
            return HttpStatusCode.ServiceUnavailable;
        }
    }
}
