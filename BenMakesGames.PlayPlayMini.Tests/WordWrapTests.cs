using BenMakesGames.PlayPlayMini.Extensions;
using BenMakesGames.PlayPlayMini.Model;
using BenMakesGames.PlayPlayMini.Services;
using Shouldly;
using Xunit;

namespace BenMakesGames.PlayPlayMini.Tests;

public sealed class WordWrapTests
{
    [Theory]
    [InlineData(1, 1, 0, 0, "Hello, world", 11, 6, 2)]
    [InlineData(1, 1, 0, 0, "Hello, world", 12, 12, 1)]
    [InlineData(1, 1, 0, 0, "Hello, world", 13, 12, 1)]

    [InlineData(5, 8, 0, 0, "Hello, world", 59, 30, 16)]
    [InlineData(5, 8, 0, 0, "Hello, world", 60, 60, 8)]
    [InlineData(5, 8, 0, 0, "Hello, world", 61, 60, 8)]

    [InlineData(1, 1, 1, 0, "Hello, world", 22, 11, 2)]
    [InlineData(1, 1, 1, 0, "Hello, world", 23, 23, 1)]
    [InlineData(1, 1, 1, 0, "Hello, world", 24, 23, 1)]

    [InlineData(5, 8, 1, 1, "Hello, world", 70, 35, 17)]
    [InlineData(5, 8, 1, 1, "Hello, world", 71, 71, 8)]
    [InlineData(5, 8, 1, 1, "Hello, world", 72, 71, 8)]
    public void GraphicsManagerComputeDimensionsWithWordWrap_ReturnsExpected(int charWidth, int charHeight, int horizontalSpacing, int verticalSpacing, string text, int maxWidth, int expectedWidth, int expectedHeight)
    {
        // Arrange
        var graphicsManager = new GraphicsManager(null!);
        var font = new Font(null!, charWidth, charHeight, horizontalSpacing, verticalSpacing, ' ');

        // Act
        var (actualWidth, actualHeight) = graphicsManager.ComputeDimensionsWithWordWrap(font, maxWidth, text);

        actualWidth.ShouldBe(expectedWidth);
        actualHeight.ShouldBe(expectedHeight);
    }

    private static readonly Font Font = new(null!, 1, 1, 0, 0, ' ');

    [Theory]
    [InlineData("Hello, world", 6, "Hello,\nworld")]
    [InlineData("Hello, world", 7, "Hello,\nworld")]
    [InlineData("Hello, world", 11, "Hello,\nworld")]
    [InlineData("Hello, world", 12, "Hello, world")]
    public void StringExtensionsWordWrap_ReturnsExpected(string originalText, int maxWidth, string expectedText)
    {
        originalText.WrapText(Font, maxWidth).ShouldBe(expectedText);
    }
}
