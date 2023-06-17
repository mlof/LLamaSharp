using FluentAssertions;
using LLama.Common;
using LLama.Common.ChatHistory;
using LLama.Transformations;

namespace LLama.Tests.Transformations
{
    public class DefaultHistoryTransformTests
    {
        private const string History = @"User: Hello world
System: Hello world
";

        public DefaultHistoryTransform CreateInstance()
        {
            return new DefaultHistoryTransform();
        }

        [Fact]
        public void Transform_HistoryToText_ReturnsExpectedFormat()
        {
            // Arrange
            var historyTransform = CreateInstance();

            var history = new ChatHistory();
            history.AddMessage(AuthorRole.User, "Hello world");
            history.AddMessage(AuthorRole.System, "Hello world");
            // Act
            var result = historyTransform.HistoryToText(
                history);

            // Assert

            result.Should().BeEquivalentTo(History);
        }
    }
}
