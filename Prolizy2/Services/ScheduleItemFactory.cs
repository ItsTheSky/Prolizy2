using System.Threading.Tasks;
using Avalonia.Media;
using Prolizy2.API;
using Prolizy2.API.Model.Course;
using Prolizy2.Model;
using Prolizy2.Utilities;

namespace Prolizy2.Services;

public static class ScheduleItemFactory
{
    public static async Task<ScheduleItem> CreateFromCourse(Course course, CourseDescription? description = null)
    {
        description ??= await course.GetDescription(true);

        var baseColor = ColorMatcher.FindClosestColor(course.BackgroundColor);
        var lighterColor = ColorMatcher.MakeLighter(baseColor, 0.2);
        var backgroundBrush = new LinearGradientBrush()
        {
            GradientStops =
            [
                new GradientStop(baseColor, 0),
                new GradientStop(lighterColor, 1)
            ]
        };

        return new ScheduleItem
        {
            StartTime = course.Start,
            EndTime = course.End ?? course.Start,
            Group = string.Join(", ", description.Groups),
            Subject = string.Join(", ", description.Subjects),
            Room = string.Join(", ", description.Rooms),
            Professor = string.Join(", ", description.Professors),
            BackgroundColor = backgroundBrush,
            ForegroundColor = new SolidColorBrush(Colors.White),
            BorderColor = new SolidColorBrush(Colors.Gray),
            Course = course,
            Type = course.CourseType
        };
    }
}