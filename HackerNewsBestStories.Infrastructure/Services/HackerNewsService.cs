using System.Text.Json;
using HackerNewsBestStories.Application.Dtos;
using HackerNewsBestStories.Application.Interfaces;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;

namespace HackerNewsBestStories.Infrastructure.Services;

public class HackerNewsService : IHackerNewsService
{
    private const int DefaultMaxParallelism = 10;
    private static readonly TimeSpan DefaultCacheExpiration = TimeSpan.FromMinutes(2);
    private const string BestStoriesCacheKey = "BestStories";

    private readonly IDistributedCache _cache;
    private readonly HackerNewsApiClient _apiClient;
    private readonly int _maxParallelism;
    private readonly TimeSpan _cacheExpiration;
    private readonly ILogger<HackerNewsService> _logger;

    public HackerNewsService(
        IHttpClientFactory httpClientFactory,
        IDistributedCache cache,
        ILogger<HackerNewsService> logger,
        ILogger<HackerNewsApiClient> apiLogger,
        int maxParallelism = DefaultMaxParallelism,
        TimeSpan? cacheExpiration = null)
    {
        _cache = cache;
        _logger = logger;
        _apiClient = new HackerNewsApiClient(httpClientFactory, apiLogger);
        _maxParallelism = maxParallelism;
        _cacheExpiration = cacheExpiration ?? DefaultCacheExpiration;
    }

    public async Task<IEnumerable<StoryResponse>> GetBestStoriesAsync(int n)
    {
        var allBestStories = await GetOrRefreshBestStoriesCacheAsync();
        return allBestStories.Take(n).ToList();
    }

    private async Task<List<StoryResponse>> GetOrRefreshBestStoriesCacheAsync()
    {
        var cached = await _cache.GetStringAsync(BestStoriesCacheKey);
        if (!string.IsNullOrEmpty(cached))
        {
            return JsonSerializer.Deserialize<List<StoryResponse>>(cached) ?? new List<StoryResponse>();
        }

        int[]? storyIds;
        try
        {
            storyIds = await _apiClient.GetBestStoryIdsAsync();
        }
        catch (HackerNewsApiException ex)
        {
            _logger.LogError(ex, "Error fetching best story IDs.");
            throw;
        }

        if (storyIds == null || storyIds.Length == 0)
            return new List<StoryResponse>();

        var stories = new ConcurrentBag<StoryResponse>();
        var semaphore = new SemaphoreSlim(_maxParallelism);

        var tasks = storyIds.Select(async id =>
        {
            await semaphore.WaitAsync();
            try
            {
                var story = await _apiClient.GetStoryByIdAsync(id);
                if (StoryValidator.IsValid(story))
                {
                    stories.Add(new StoryResponse
                    {
                        Title = story.Title!.Trim(),
                        Uri = string.IsNullOrWhiteSpace(story.Url) ? string.Empty : story.Url.Trim(),
                        PostedBy = story.By!.Trim(),
                        Time = DateTimeOffset.FromUnixTimeSeconds(story.Time),
                        Score = story.Score,
                        CommentCount = story.Descendants
                    });
                }
            }
            catch (HackerNewsApiException ex)
            {
                _logger.LogWarning(ex, $"Error fetching story details for ID {id}.");
            }
            finally
            {
                semaphore.Release();
            }
        });

        await Task.WhenAll(tasks);

        var result = stories
            .OrderByDescending(s => s.Score)
            .ToList();

        var options = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = _cacheExpiration
        };
        await _cache.SetStringAsync(BestStoriesCacheKey, JsonSerializer.Serialize(result), options);

        return result;
    }
}