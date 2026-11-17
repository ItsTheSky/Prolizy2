using System;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using Prolizy2.API.Model.Course;

namespace Prolizy2.API.Utils;

public static partial class CourseDescriptionParser
{
    // Source-generated regexes: no pattern parsing or IL emission at startup
    // (RegexOptions.Compiled is costly on mobile runtimes).
    [GeneratedRegex(@"^(?:R\d+\.|SA[EÉ]\s*\d+\.\d+)", RegexOptions.IgnoreCase)]
    private static partial Regex SubjectStartRegex();

    [GeneratedRegex(@"\[[A-Z]{2,}\d+[A-Z]\d*[A-Z]?\]")]
    private static partial Regex ModuleCodeRegex();

    [GeneratedRegex(@"^INF\d+(?:-[A-Z]+)*$")]
    private static partial Regex GroupRegex();

    [GeneratedRegex(@"^[A-Z]?\d+\s*-\s*[A-Z]{2,}$")]
    private static partial Regex RoomDashRegex();

    [GeneratedRegex(@"^Amphi\s+[A-Z0-9]", RegexOptions.IgnoreCase)]
    private static partial Regex AmphiPrefixRegex();

    [GeneratedRegex(@"^[A-Z]{2,}(?:[-' ][A-Z]{2,})*\s+[A-Z][a-zà-öø-ÿ]")]
    private static partial Regex ProfessorRegex();

    [GeneratedRegex(@"&#(\d+);")]
    private static partial Regex HtmlNumericEntityRegex();

    [GeneratedRegex(@"^[A-Z]{2,}")]
    private static partial Regex UpperCasePrefixRegex();

    private static readonly Regex SubjectStart = SubjectStartRegex();
    private static readonly Regex ModuleCode = ModuleCodeRegex();
    private static readonly Regex Group = GroupRegex();
    private static readonly Regex RoomDash = RoomDashRegex();
    private static readonly Regex AmphiPrefix = AmphiPrefixRegex();
    private static readonly Regex Professor = ProfessorRegex();
    private static readonly Regex HtmlNumericEntity = HtmlNumericEntityRegex();
    private static readonly Regex UpperCasePrefix = UpperCasePrefixRegex();

    private static string DecodeHtmlEntities(string input)
    {
        if (string.IsNullOrEmpty(input)) return input;

        string decoded = WebUtility.HtmlDecode(input);
        decoded = HtmlNumericEntity.Replace(decoded, match =>
        {
            int number = int.Parse(match.Groups[1].Value);
            return ((char)number).ToString();
        });

        return decoded;
    }

    public static CourseDescription ParseDescription(string description)
    {
        var courseDescription = new CourseDescription();

        var parts = description.Split(["<br />", "<br/>", "\r\n"], StringSplitOptions.RemoveEmptyEntries)
            .Select(p => DecodeHtmlEntities(p.Trim()))
            .Where(p => !string.IsNullOrWhiteSpace(p))
            .ToList();

        foreach (var part in parts)
        {
            if (part.Contains(',') || part.Contains('<'))
            {
                var subParts = part.Split([',', '<'], StringSplitOptions.RemoveEmptyEntries)
                    .Select(p => p.Trim())
                    .Where(p => !string.IsNullOrWhiteSpace(p));

                foreach (var subPart in subParts)
                    ClassifyPart(subPart, courseDescription);
            }
            else
            {
                ClassifyPart(part, courseDescription);
            }
        }

        if (courseDescription.EventType.IsProject() && courseDescription.Subjects.Count == 0)
            courseDescription.Subjects.Add("SAÉ");

        return courseDescription;
    }

    private static void ClassifyPart(string part, CourseDescription courseDescription)
    {
        if (SubjectStart.IsMatch(part) || ModuleCode.IsMatch(part))
        {
            courseDescription.Subjects.Add(part);
            return;
        }

        if (Group.IsMatch(part))
        {
            courseDescription.Groups.Add(part);
            return;
        }

        if (part.EndsWith("VEL") || AmphiPrefix.IsMatch(part) || RoomDash.IsMatch(part))
        {
            courseDescription.Rooms.Add(part);
            return;
        }

        if (Professor.IsMatch(part))
        {
            courseDescription.Professors.Add(part);
            return;
        }

        if (part.Contains("VEL"))
            courseDescription.Rooms.Add(part);
        else if (part.StartsWith("INF"))
            courseDescription.Groups.Add(part);
        else if (UpperCasePrefix.IsMatch(part))
            courseDescription.Professors.Add(part);
        else
            courseDescription.Subjects.Add(part);
    }
}
