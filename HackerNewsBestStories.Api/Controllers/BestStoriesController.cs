using HackerNewsBestStories.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace HackerNewsBestStories.Api.Controllers
{
    public static class BestStoriesConstants
    {
        public const int DefaultStoriesCount = 10;
        public const int MinStoriesCount = 1;
        public const int MaxStoriesCount = 500;
    }

    [ApiController]
    [Route("api/v1/stories")]
    public class BestStoriesController : ControllerBase
    {
        private readonly IHackerNewsService _hackerNewsService;

        public BestStoriesController(IHackerNewsService hackerNewsService)
        {
            _hackerNewsService = hackerNewsService;
        }

        [HttpGet("best")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetBestStories(
            [FromQuery, Range(BestStoriesConstants.MinStoriesCount, BestStoriesConstants.MaxStoriesCount)]
            int numbersOfStories = BestStoriesConstants.DefaultStoriesCount)
        {
            var stories = await _hackerNewsService.GetBestStoriesAsync(numbersOfStories);
            return Ok(stories);
        }
    }
}