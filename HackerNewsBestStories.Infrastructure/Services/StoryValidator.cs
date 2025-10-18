using HackerNewsBestStories.Infrastructure.Models;

namespace HackerNewsBestStories.Infrastructure.Services;

public static class StoryValidator
{
    public static bool IsValid(HackerNewsStory? story)
    {
        if (story == null)
            return false;
        if (string.IsNullOrWhiteSpace(story.Title))
            return false;
        if (string.IsNullOrWhiteSpace(story.By))
            return false;
        if (story.Time <= 0)
            return false;
        if (story.Score < 0)
            return false;
        if (story.Descendants < 0)
            return false;
        return true;
    }
}