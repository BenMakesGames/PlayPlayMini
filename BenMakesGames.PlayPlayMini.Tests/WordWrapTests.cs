using BenMakesGames.PlayPlayMini.Model;
using BenMakesGames.PlayPlayMini.Services;
using FluentAssertions;
using Xunit;

namespace BenMakesGames.PlayPlayMini.Tests;

public sealed class WordWrapTests
{
    [Theory]
    [InlineData(1, 1, 0, 0, "Hello, world!", 10, 6, 1)]
    public void GraphicsManagerComputeDimensionsWithWordWrap_ReturnsExpected(int charWidth, int charHeight, int horizontalSpacing, int verticalSpacing, string text, int maxWidth, int expectedWidth, int expectedHeight)
    {
        // Arrange
        var graphicsManager = new GraphicsManager(null!);
        var font = new Font(null!, charWidth, charHeight, horizontalSpacing, verticalSpacing, ' ');

        // Act
        var (actualWidth, actualHeight) = graphicsManager.ComputeDimensionsWithWordWrap(font, maxWidth, text);

        actualWidth.Should().Be(expectedWidth);
        actualHeight.Should().Be(expectedHeight);
    }
}
