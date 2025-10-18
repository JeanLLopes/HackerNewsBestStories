using System;

namespace HackerNewsBestStories.Infrastructure.Services
{
    public class HackerNewsApiException : Exception
    {
        public HackerNewsApiException(string message, Exception? innerException = null)
            : base(message, innerException)
        {
        }
    }
}