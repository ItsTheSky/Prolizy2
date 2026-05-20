using System;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using Prolizy2.API.Model.Course;

namespace Prolizy2.API.Utils;

public static class CourseDescriptionParser
{
    private static readonly Regex SubjectStart =
        new(@"^(?:R\d+\.|SA[EÉ]\s*\d+\.\d+)", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    private static readonly Regex ModuleCode =
        new(@"\[[A-Z]{2,}\d+[A-Z]\d*[A-Z]?\]", RegexOptions.Compiled);

    private static readonly Regex Group =
        new(@"^INF\d+(?:-[A-Z]+)*$", RegexOptions.Compiled);

    private static readonly Regex RoomDash =
        new(@"^[A-Z]?\d+\s*-\s*[A-Z]{2,}$", RegexOptions.Compiled);

    private static readonly Regex AmphiPrefix =
        new(@"^Amphi\s+[A-Z0-9]", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    private static readonly Regex Professor =
        new(@"^[A-Z]{2,}(?:[-' ][A-Z]{2,})*\s+[A-Z][a-zà-öø-ÿ]", RegexOptions.Compiled);

    private static readonly Regex HtmlNumericEntity =
        new(@"&#(\d+);", RegexOptions.Compiled);

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
        else if (Regex.IsMatch(part, @"^[A-Z]{2,}"))
            courseDescription.Professors.Add(part);
        else
            courseDescription.Subjects.Add(part);
    }
}
