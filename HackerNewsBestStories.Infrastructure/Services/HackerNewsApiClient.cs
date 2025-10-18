using HackerNewsBestStories.Infrastructure.Models;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.CircuitBreaker;
using Polly.Retry;
using System.Net.Http.Json;

namespace HackerNewsBestStories.Infrastructure.Services;

public class HackerNewsApiClient
{
    private const int DefaultRetryCount = 3;
    private const int DefaultCircuitBreakerFailures = 5;
    private static readonly TimeSpan DefaultCircuitBreakerDuration = TimeSpan.FromSeconds(30);

    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<HackerNewsApiClient> _logger;
    private readonly AsyncRetryPolicy _retryPolicy;
    private readonly AsyncCircuitBreakerPolicy _circuitBreakerPolicy;

    public HackerNewsApiClient(IHttpClientFactory httpClientFactory, ILogger<HackerNewsApiClient> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;

        _retryPolicy = Policy
            .Handle<Exception>()
            .WaitAndRetryAsync(DefaultRetryCount, attempt => TimeSpan.FromSeconds(Math.Pow(2, attempt)),
                (exception, timeSpan, retryCount, context) =>
                {
                    _logger.LogWarning(exception, "Retry {RetryCount} after {Delay}s due to error: {Message}", retryCount, timeSpan.TotalSeconds, exception.Message);
                });

        _circuitBreakerPolicy = Policy
            .Handle<Exception>()
            .CircuitBreakerAsync(DefaultCircuitBreakerFailures, DefaultCircuitBreakerDuration,
                onBreak: (ex, breakDelay) =>
                {
                    _logger.LogError(ex, "Circuit breaker opened for {Delay}s due to error: {Message}", breakDelay.TotalSeconds, ex.Message);
                },
                onReset: () =>
                {
                    _logger.LogInformation("Circuit breaker reset.");
                },
                onHalfOpen: () =>
                {
                    _logger.LogInformation("Circuit breaker is half-open, next call is a trial.");
                });
    }

    public async Task<int[]?> GetBestStoryIdsAsync()
    {
        try
        {
            return await _retryPolicy.ExecuteAsync(() =>
                _circuitBreakerPolicy.ExecuteAsync(async () =>
                {
                    var client = _httpClientFactory.CreateClient("HackerNews");
                    return await client.GetFromJsonAsync<int[]>("beststories.json");
                }));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to fetch best story IDs from Hacker News.");
            throw new HackerNewsApiException("Failed to fetch best story IDs from Hacker News.", ex);
        }
    }

    public async Task<HackerNewsStory?> GetStoryByIdAsync(int id)
    {
        try
        {
            return await _retryPolicy.ExecuteAsync(() =>
                _circuitBreakerPolicy.ExecuteAsync(async () =>
                {
                    var client = _httpClientFactory.CreateClient("HackerNews");
                    return await client.GetFromJsonAsync<HackerNewsStory>($"item/{id}.json");
                }));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Failed to fetch story details for ID {id} from Hacker News.");
            throw new HackerNewsApiException($"Failed to fetch story details for ID {id} from Hacker News.", ex);
        }
    }
}
