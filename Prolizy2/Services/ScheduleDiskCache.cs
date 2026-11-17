using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace Prolizy2.Services;

/// <summary>
/// Persists the raw calendar payload of each week so the schedule can be shown
/// instantly on the next launch (and offline), before the network answers.
/// One file per group and per week, named after the week's Monday.
/// All members are safe to call from a background thread.
/// </summary>
public static class ScheduleDiskCache
{
    private const int MaxAgeDays = 120;

    private static readonly string RootDirectory = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "Prolizy2", "edt-cache");

    public static string? TryRead(string? group, DateOnly weekStart)
    {
        try
        {
            var path = GetPath(group, weekStart);
            return path is not null && File.Exists(path) ? File.ReadAllText(path) : null;
        }
        catch
        {
            return null;
        }
    }

    public static void Write(string? group, DateOnly weekStart, string json)
    {
        try
        {
            var path = GetPath(group, weekStart);
            if (path is null)
                return;

            Directory.CreateDirectory(Path.GetDirectoryName(path)!);

            // Write to a temp file first so a killed process never leaves a truncated payload.
            var tempPath = path + ".tmp";
            File.WriteAllText(tempPath, json);
            File.Move(tempPath, path, overwrite: true);
        }
        catch
        {
            // The cache is an optimisation only; a failed write must never surface.
        }
    }

    /// <summary>Removes the weeks that are too old to be browsed back to.</summary>
    public static void Prune()
    {
        try
        {
            if (!Directory.Exists(RootDirectory))
                return;

            var cutoff = DateOnly.FromDateTime(DateTime.Now).AddDays(-MaxAgeDays);
            foreach (var file in Directory.EnumerateFiles(RootDirectory, "*.json", SearchOption.AllDirectories))
            {
                var name = Path.GetFileNameWithoutExtension(file);
                if (DateOnly.TryParseExact(name, "yyyy-MM-dd", CultureInfo.InvariantCulture,
                        DateTimeStyles.None, out var weekStart) && weekStart < cutoff)
                    File.Delete(file);
            }
        }
        catch
        {
            // ignored
        }
    }

    private static string? GetPath(string? group, DateOnly weekStart)
    {
        if (string.IsNullOrWhiteSpace(group))
            return null;

        return Path.Combine(RootDirectory, Sanitize(group),
            weekStart.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) + ".json");
    }

    private static string Sanitize(string group)
    {
        var builder = new StringBuilder(group.Length);
        foreach (var c in group)
            builder.Append(char.IsLetterOrDigit(c) || c is '-' or '_' ? c : '_');
        return builder.ToString();
    }
}
