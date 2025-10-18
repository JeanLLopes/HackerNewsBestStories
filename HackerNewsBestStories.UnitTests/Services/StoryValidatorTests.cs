using HackerNewsBestStories.Infrastructure.Models;
using HackerNewsBestStories.Infrastructure.Services;
using Xunit;

namespace HackerNewsBestStories.UnitTests.Services
{
    public class StoryValidatorTests
    {
        [Fact]
        public void IsValid_ReturnsFalse_WhenStoryIsNull()
        {
            Assert.False(StoryValidator.IsValid(null));
        }

        [Fact]
        public void IsValid_ReturnsFalse_WhenTitleIsNullOrWhiteSpace()
        {
            var story = new HackerNewsStory { Title = " ", By = "user", Time = 123, Score = 1, Descendants = 0 };
            Assert.False(StoryValidator.IsValid(story));
        }

        [Fact]
        public void IsValid_ReturnsFalse_WhenByIsNullOrWhiteSpace()
        {
            var story = new HackerNewsStory { Title = "Title", By = "", Time = 123, Score = 1, Descendants = 0 };
            Assert.False(StoryValidator.IsValid(story));
        }

        [Fact]
        public void IsValid_ReturnsFalse_WhenTimeIsZeroOrNegative()
        {
            var story = new HackerNewsStory { Title = "Title", By = "user", Time = 0, Score = 1, Descendants = 0 };
            Assert.False(StoryValidator.IsValid(story));
        }

        [Fact]
        public void IsValid_ReturnsFalse_WhenScoreIsNegative()
        {
            var story = new HackerNewsStory { Title = "Title", By = "user", Time = 123, Score = -1, Descendants = 0 };
            Assert.False(StoryValidator.IsValid(story));
        }

        [Fact]
        public void IsValid_ReturnsFalse_WhenDescendantsIsNegative()
        {
            var story = new HackerNewsStory { Title = "Title", By = "user", Time = 123, Score = 1, Descendants = -1 };
            Assert.False(StoryValidator.IsValid(story));
        }

        [Fact]
        public void IsValid_ReturnsTrue_WhenAllFieldsAreValid()
        {
            var story = new HackerNewsStory { Title = "Title", By = "user", Time = 123, Score = 1, Descendants = 0 };
            Assert.True(StoryValidator.IsValid(story));
        }
    }
}