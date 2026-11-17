using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Prolizy2.API.Model.Course;
using Prolizy2.API.Model.Request;
using Prolizy2.API.Utils;

namespace Prolizy2.API;

public static class EDTClient
{
    
    public static async Task<List<Course>> GetCourses(CalendarRequest request, CancellationToken token = default)
    {
        return ParseCourses(await GetCoursesJson(request, token).ConfigureAwait(false));
    }

    /// <summary>Raw calendar payload, as returned by the server (kept so it can be cached on disk).</summary>
    public static async Task<string> GetCoursesJson(CalendarRequest request, CancellationToken token = default)
    {
        var response = await RequestUtils.PostAsync(RequestUtils.TargetServer.Edt,
            "GetCalendarData", request, token: token).ConfigureAwait(false);
        return await response.Content.ReadAsStringAsync(token).ConfigureAwait(false);
    }

    public static List<Course> ParseCourses(string json) => JsonUtils.Deserialize<List<Course>>(json);

    public static async Task<CourseDescription> GetDescription(this Course course, bool offline = false, CancellationToken token = default)
    {
        if (offline)
            return CourseDescriptionParser.ParseDescription(course.RawDescription);
        
        var obj = new { eventId = course.Id };
        var response = await RequestUtils.PostAsync(RequestUtils.TargetServer.Edt, "GetSideBarEvent", obj, token: token).ConfigureAwait(false);
        var content = await response.Content.ReadAsStringAsync(token).ConfigureAwait(false);
        var elementsJson = JsonDocument.Parse(content).RootElement.GetProperty("elements");
        var description = new CourseDescription();

        string? lastElementType = null;
        foreach (var element in elementsJson.EnumerateArray())
        {
            var key = element.GetProperty("label").GetString();
            if (key == null && lastElementType != null)
                key = lastElementType;
            else
                lastElementType = key;
            
            var value = (element.GetProperty("content").ValueKind == JsonValueKind.Array 
                ? string.Join(", ", element.GetProperty("content").EnumerateArray().Select(e => e.GetString())) 
                : element.GetProperty("content").GetString())
                ?.Replace("<br />", "\n");
            if (value == null)
                continue;
            
            switch (key)
            {
                case "Personnel":
                    description.Professors.Add(value);
                    break;
                case "Salle":
                    description.Rooms.Add(value);
                    break;
                case "Matière":
                case "Matières":
                    description.Subjects.Add(value);
                    break;
                case "Groupe":
                    description.Groups.Add(value);
                    break;
                case "Catégorie d’événement":
                    description.EventTypes.Add(value);
                    break;
                case "Remarques":
                    description.Notes.Add(value);
                    break;
                case "Heure":
                    description.Hours.Add(value);
                    break;
            }
        }
        
        return description;
    }
    
    public static async Task<List<string>> GetGroups(FederationRequest request, CancellationToken token = default)
    {
        var response = await RequestUtils.PostAsync(RequestUtils.TargetServer.Edt, "ReadResourceListItems", request, token: token).ConfigureAwait(false);
        var content = await response.Content.ReadAsStringAsync(token).ConfigureAwait(false);
        //Console.WriteLine(content);
        var json = JsonDocument.Parse(content);
        var results = json.RootElement.GetProperty("results");
        var groups = new List<string>();
        foreach (var group in results.EnumerateArray())
            groups.Add(group.GetProperty("text").GetString()!);
        return groups;
    }
    
}