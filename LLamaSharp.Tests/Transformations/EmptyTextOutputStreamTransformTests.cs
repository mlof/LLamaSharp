using FluentAssertions;
using LLama.Transformations;

namespace LLama.Tests.Transformations
{
    public class EmptyTextOutputStreamTransformTests
    {
        public EmptyTextOutputStreamTransform CreateInstance()
        {
            return new EmptyTextOutputStreamTransform();
        }

        [Fact]
        public void Transform_ReturnsUnchanged()
        {
            // Arrange
            var emptyTextOutputStreamTransform = CreateInstance();

            var tokens = new[] { "Hello world" };
            // Act
            var result = emptyTextOutputStreamTransform.Transform(tokens);

            // Assert

            result.Should().BeEquivalentTo(tokens);
        }

        [Fact]
        public async Task TransformAsync_ReturnsUnchanged()
        {
            // Arrange
            var emptyTextOutputStreamTransform = CreateInstance();

            var tokens = new List<string> { "Hello world" };

            var asyncTokens = tokens.ToAsyncEnumerable();
            // Act

            await emptyTextOutputStreamTransform.TransformAsync(tokens.ToAsyncEnumerable()).ForEachAsync((s, i) =>
                {
                    s.Should().BeEquivalentTo(tokens[i]);
                }
            );
        }
    }
}
