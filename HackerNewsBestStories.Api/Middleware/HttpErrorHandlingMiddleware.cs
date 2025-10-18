using System.Net;
using System.Text.Json;
using HackerNewsBestStories.Infrastructure.Services;

namespace HackerNewsBestStories.Api.Middleware
{
    public class HttpErrorHandlingMiddleware
    {
        private readonly RequestDelegate _next;

        public HttpErrorHandlingMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (HackerNewsApiException ex)
            {
                await HandleCustomExceptionAsync(context, ex);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex);
            }
        }

        private static async Task HandleCustomExceptionAsync(HttpContext context, HackerNewsApiException exception)
        {
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)HttpStatusCode.BadGateway;

            var errorResponse = new
            {
                error = "HackerNews API error.",
                details = exception.Message
            };

            var json = JsonSerializer.Serialize(errorResponse);
            await context.Response.WriteAsync(json);
        }

        private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

            var errorResponse = new
            {
                error = "An unexpected error occurred.",
                details = exception.Message
            };

            var json = JsonSerializer.Serialize(errorResponse);
            await context.Response.WriteAsync(json);
        }
    }
}