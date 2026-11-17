using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonia.Media;
using Prolizy2.API;
using Prolizy2.API.Model.Course;
using Prolizy2.API.Utils;
using Prolizy2.Model;
using Prolizy2.Utilities;

namespace Prolizy2.Services;

public static class ScheduleItemFactory
{
    // Courses reuse a handful of colours: share one brush per colour instead of
    // allocating a gradient (and two stops) for every single course.
    private static readonly Dictionary<string, IBrush> BackgroundBrushes = new();

    public static async Task<ScheduleItem> CreateFromCourse(Course course, CourseDescription? description = null)
    {
        description ??= await course.GetDescription(true);
        return Create(course, description);
    }

    /// <summary>Builds the UI model of a course. Must be called on the UI thread (it creates brushes).</summary>
    public static ScheduleItem Create(Course course, CourseDescription description)
    {
        return new ScheduleItem
        {
            StartTime = course.Start,
            EndTime = course.End ?? course.Start,
            Group = string.Join(", ", description.Groups),
            Subject = string.Join(", ", description.Subjects),
            Room = string.Join(", ", description.Rooms),
            Professor = string.Join(", ", description.Professors),
            BackgroundColor = GetBackgroundBrush(course.BackgroundColor),
            ForegroundColor = Brushes.White,
            BorderColor = Brushes.Gray,
            Course = course,
            Type = course.CourseType
        };
    }

    private static IBrush GetBackgroundBrush(string rawColor)
    {
        var key = rawColor ?? string.Empty;
        if (BackgroundBrushes.TryGetValue(key, out var brush))
            return brush;

        var baseColor = ColorMatcher.FindClosestColor(rawColor);
        var lighterColor = ColorMatcher.MakeLighter(baseColor, 0.2);
        brush = new LinearGradientBrush
        {
            GradientStops =
            [
                new GradientStop(baseColor, 0),
                new GradientStop(lighterColor, 1)
            ]
        }.ToImmutable();

        BackgroundBrushes[key] = brush;
        return brush;
    }
}
