using HackerNewsBestStories.Application.Dtos;

namespace HackerNewsBestStories.Application.Interfaces;

public interface IHackerNewsService
{
    Task<IEnumerable<StoryResponse>> GetBestStoriesAsync(int n);
}