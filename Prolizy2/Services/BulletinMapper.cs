using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using Prolizy2.API.Model;
using Prolizy2.Model.Bulletin;

namespace Prolizy2.Services;

/// <summary>
/// Converts <see cref="BulletinRoot"/> payloads coming out of <see cref="API.BulletinClient"/>
/// into the UI-friendly <see cref="BulletinData"/> graph rendered by the bulletin views.
/// </summary>
public static class BulletinMapper
{
    /// <summary>Fallback palette used when a UE doesn't expose a colour.</summary>
    private static readonly string[] FallbackPalette =
    {
        "#2563EB", "#7C3AED", "#0D9488", "#D97706", "#DC2626", "#16A34A",
        "#0891B2", "#9333EA", "#CA8A04", "#0F766E"
    };

    /// <summary>
    /// Build a brand-new <see cref="BulletinData"/> from the "première connexion"
    /// payload — fills semesters, current student and the transcript bundle.
    /// </summary>
    public static BulletinData Map(BulletinRoot root)
    {
        var data = new BulletinData();
        FillSemesters(data, root);
        FillTranscript(data, root.Transcript);
        return data;
    }

    /// <summary>
    /// Refresh the transcript-scoped collections (units / resources / SAÉ / student
    /// / competences) on an existing <paramref name="target"/>. The semester list
    /// is preserved so chip selection survives the swap.
    /// </summary>
    public static void MapSemesterInto(BulletinRoot semesterRoot, BulletinData target)
    {
        FillTranscript(target, semesterRoot.Transcript);
    }

    #region Semesters

    private static void FillSemesters(BulletinData data, BulletinRoot root)
    {
        var transcript = root.Transcript;
        var currentId = transcript?.SemesterId ?? -1;

        var ordered = (root.Semesters ?? new List<Semester>())
            .OrderByDescending(s => s.AcademicYear, StringComparer.Ordinal)
            .ThenByDescending(s => s.SemesterNumber);

        var infos = new ObservableCollection<SemesterInfo>();
        foreach (var s in ordered)
        {
            infos.Add(new SemesterInfo
            {
                Id = s.SemesterId.ToString(CultureInfo.InvariantCulture),
                Code = string.IsNullOrWhiteSpace(s.Title) ? $"S{s.SemesterNumber}" : ShortenSemesterTitle(s),
                Year = s.AcademicYear ?? string.Empty,
                Track = null,
                IsFuture = currentId > 0 && s.SemesterId > currentId,
                IsCurrent = s.SemesterId == currentId
            });
        }

        data.Semesters = infos;
    }

    private static string ShortenSemesterTitle(Semester s)
    {
        // The API exposes a friendly title — fall back to "Sn" if it's bloated.
        if (string.IsNullOrWhiteSpace(s.Title))
            return $"S{s.SemesterNumber}";
        // Often "BUT Info S2 2024/2025 FI" — too long for a chip. Use compact form.
        return $"S{s.SemesterNumber}";
    }

    #endregion

    #region Transcript

    private static void FillTranscript(BulletinData data, Transcript? transcript)
    {
        if (transcript is null)
        {
            data.Student = new StudentInfo();
            data.Units = new ObservableCollection<TeachingUnitInfo>();
            data.Resources = new ObservableCollection<EvaluationGroupInfo>();
            data.Saes = new ObservableCollection<EvaluationGroupInfo>();
            data.Competences = new ObservableCollection<CompetenceInfo>();
            return;
        }

        data.Student = MapStudent(transcript.Student);

        var resources = MapGroups(transcript.Resources);
        var saes = MapGroups(transcript.Saes);
        data.Resources = new ObservableCollection<EvaluationGroupInfo>(resources);
        data.Saes = new ObservableCollection<EvaluationGroupInfo>(saes);

        // Lookup tables for label resolution when expanding unit contributions.
        var resourceLabels = (transcript.Resources ?? new Dictionary<string, Resource>())
            .ToDictionary(kv => kv.Key, kv => kv.Value.Title ?? kv.Key, StringComparer.OrdinalIgnoreCase);
        var saeLabels = (transcript.Saes ?? new Dictionary<string, Sae>())
            .ToDictionary(kv => kv.Key, kv => kv.Value.Title ?? kv.Key, StringComparer.OrdinalIgnoreCase);

        var units = MapUnits(transcript.TeachingUnits, resourceLabels, saeLabels);
        data.Units = new ObservableCollection<TeachingUnitInfo>(units);

        data.Competences = new ObservableCollection<CompetenceInfo>(
            units.Select(u => new CompetenceInfo
            {
                Name = string.IsNullOrWhiteSpace(u.Name) ? u.Code : FirstWord(u.Name),
                Status = u.Status,
                UnitCode = u.Code
            }));
    }

    private static StudentInfo MapStudent(Student? s)
    {
        if (s is null)
            return new StudentInfo();

        var civility = string.IsNullOrWhiteSpace(s.Civility) ? string.Empty : s.Civility.Trim() + " ";
        var fullName = (civility + (s.LastName ?? string.Empty) + " " + (s.FirstName ?? string.Empty)).Trim();

        return new StudentInfo
        {
            Name = string.IsNullOrWhiteSpace(fullName) ? "—" : fullName,
            BirthDate = string.IsNullOrWhiteSpace(s.BirthDate) ? "—" : s.BirthDate,
            StudentId = string.IsNullOrWhiteSpace(s.StudentId) ? "—" : s.StudentId,
            Ine = "—",
            Program = s.DepartmentAcronym ?? string.Empty,
            Group = string.Empty
        };
    }

    #endregion

    #region Groups (resources + SAÉ)

    private static IEnumerable<EvaluationGroupInfo> MapGroups(IDictionary<string, Resource>? resources)
    {
        if (resources is null)
            yield break;

        foreach (var (key, value) in resources)
            yield return MapResourceGroup(key, value);
    }

    private static IEnumerable<EvaluationGroupInfo> MapGroups(IDictionary<string, Sae>? saes)
    {
        if (saes is null)
            yield break;

        foreach (var (key, value) in saes)
            yield return MapSaeGroup(key, value);
    }

    private static EvaluationGroupInfo MapResourceGroup(string key, Resource resource)
    {
        // Always use the dict key — that's what unit contributions reference,
        // so FindGroup(contribution.Code) keeps matching even when ApogeeCode diverges.
        return new EvaluationGroupInfo
        {
            Code = key,
            Label = resource.Title ?? key,
            Average = ParseGrade(resource.Average),
            Evaluations = new ObservableCollection<EvaluationInfo>(
                (resource.Evaluations ?? new List<Evaluation>()).Select(MapEvaluation))
        };
    }

    private static EvaluationGroupInfo MapSaeGroup(string key, Sae sae)
    {
        return new EvaluationGroupInfo
        {
            Code = key,
            Label = sae.Title ?? key,
            Average = ParseGrade(sae.Average),
            Evaluations = new ObservableCollection<EvaluationInfo>(
                (sae.Evaluations ?? new List<Evaluation>()).Select(MapEvaluation))
        };
    }

    private static EvaluationInfo MapEvaluation(Evaluation e) => new()
    {
        Name = string.IsNullOrWhiteSpace(e.Description) ? "Évaluation" : e.Description,
        Grade = ParseGrade(e.Grade),
        Coefficient = ParseDouble(e.Coefficient, 1d)
    };

    #endregion

    #region Units

    private static List<TeachingUnitInfo> MapUnits(
        IDictionary<string, TeachingUnit>? units,
        IReadOnlyDictionary<string, string> resourceLabels,
        IReadOnlyDictionary<string, string> saeLabels)
    {
        var result = new List<TeachingUnitInfo>();
        if (units is null)
            return result;

        var colorIndex = 0;
        foreach (var (key, unit) in units)
        {
            var color = !string.IsNullOrWhiteSpace(unit.Color)
                ? NormalizeHex(unit.Color)
                : FallbackPalette[colorIndex++ % FallbackPalette.Length];

            var average = ParseGrade(unit.Average);

            var info = new TeachingUnitInfo
            {
                Code = key,
                ColorHex = color,
                Name = unit.Title ?? key,
                ShortName = FirstWord(unit.Title ?? string.Empty),
                Average = average,
                Rank = ParseInt(unit.Average?.Rank, 0),
                Total = unit.Average?.TotalRank ?? 0,
                Bonus = ParseDouble(unit.Bonus, 0d),
                Malus = ParseDouble(unit.Malus, 0d),
                Status = ResolveUnitStatus(average),
                Contributions = new ObservableCollection<ContributionInfo>(
                    BuildContributions(unit, resourceLabels, saeLabels))
            };
            info.NormalizeWeights();
            result.Add(info);
        }

        return result;
    }

    private static IEnumerable<ContributionInfo> BuildContributions(
        TeachingUnit unit,
        IReadOnlyDictionary<string, string> resourceLabels,
        IReadOnlyDictionary<string, string> saeLabels)
    {
        if (unit.Resources is not null)
        {
            foreach (var (code, entry) in unit.Resources)
            {
                resourceLabels.TryGetValue(code, out var label);
                yield return new ContributionInfo
                {
                    Code = code,
                    Label = label ?? code,
                    Grade = ParseGradeString(entry.Average),
                    Coefficient = entry.Coefficient
                };
            }
        }

        if (unit.Saes is not null)
        {
            foreach (var (code, entry) in unit.Saes)
            {
                saeLabels.TryGetValue(code, out var label);
                yield return new ContributionInfo
                {
                    Code = code,
                    Label = label ?? code,
                    Grade = ParseGradeString(entry.Average),
                    Coefficient = entry.Coefficient
                };
            }
        }
    }

    private static string ResolveUnitStatus(GradeValue avg)
    {
        if (!avg.IsNumeric)
            return "ATT";
        return avg.NumericValue!.Value >= 10d ? "ADM" : "AJ";
    }

    #endregion

    #region Helpers

    private static GradeValue ParseGrade(Grade? grade)
    {
        if (grade is null)
            return GradeValue.Text("—");
        return ParseGradeString(grade.Value);
    }

    private static GradeValue ParseGradeString(string? value)
    {
        if (string.IsNullOrWhiteSpace(value) || value == "~" || value == "-" || value == "—")
            return GradeValue.Text("—");

        if (TryParseDouble(value, out var d))
            return GradeValue.Numeric(d);

        return GradeValue.Text(value.Trim());
    }

    private static double ParseDouble(string? value, double fallback)
    {
        if (string.IsNullOrWhiteSpace(value))
            return fallback;
        return TryParseDouble(value, out var d) ? d : fallback;
    }

    private static int ParseInt(string? value, int fallback)
    {
        if (string.IsNullOrWhiteSpace(value))
            return fallback;
        return int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var v)
            ? v : fallback;
    }

    private static bool TryParseDouble(string value, out double result)
    {
        var normalised = value.Trim().Replace(',', '.');
        return double.TryParse(normalised, NumberStyles.Float, CultureInfo.InvariantCulture, out result);
    }

    private static string NormalizeHex(string hex)
    {
        hex = hex.Trim();
        return hex.StartsWith('#') ? hex : "#" + hex;
    }

    private static string FirstWord(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return string.Empty;
        var space = text.IndexOf(' ');
        return space < 0 ? text : text[..space];
    }

    #endregion
}
