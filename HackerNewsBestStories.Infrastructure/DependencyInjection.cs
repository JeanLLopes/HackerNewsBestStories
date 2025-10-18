using HackerNewsBestStories.Application.Interfaces;
using HackerNewsBestStories.Infrastructure.Services;
using Microsoft.Extensions.DependencyInjection;

namespace HackerNewsBestStories.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        services.AddHttpClient("HackerNews", client =>
        {
            client.BaseAddress = new Uri("https://hacker-news.firebaseio.com/v0/");
        });

        services.AddScoped<IHackerNewsService, HackerNewsService>();
        services.AddScoped<HackerNewsApiClient>();

        return services;
    }
}