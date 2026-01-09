using System;
using BenMakesGames.PlayPlayMini.Services;
using Microsoft.Xna.Framework;

namespace BenMakesGames.PlayPlayMini.GraphicsExtensions;

/// <summary>
/// Extension methods for drawing text with a wavy animation effect using the GraphicsManager.
/// </summary>
public static class WavyText
{
    /// <param name="graphics">The graphics manager instance.</param>
    extension(GraphicsManager graphics)
    {
        /// <summary>
        /// Draws text with a wavy animation effect, centered horizontally and vertically on the screen.
        /// </summary>
        /// <param name="fontName">Name of the font to use.</param>
        /// <param name="gameTime">Current game time for animation.</param>
        /// <param name="text">Text to draw.</param>
        /// <param name="color">Color of the text.</param>
        public void DrawWavyText(string fontName, GameTime gameTime, string text, Color color)
        {
            var font = graphics.Fonts[fontName];

            var x = (graphics.Width - text.Length * font.CharacterWidth - (text.Length - 1) * font.HorizontalSpacing) / 2;
            var y = (graphics.Height - font.CharacterHeight) / 2;

            graphics.DrawWavyText(fontName, x, y, gameTime, text, color);
        }

        /// <summary>
        /// Draws text with a wavy animation effect, centered horizontally and vertically on the screen, using white color.
        /// </summary>
        /// <param name="fontName">Name of the font to use.</param>
        /// <param name="gameTime">Current game time for animation.</param>
        /// <param name="text">Text to draw.</param>
        public void DrawWavyText(string fontName, GameTime gameTime, string text)
            => graphics.DrawWavyText(fontName, gameTime, text, Color.White);

        /// <summary>
        /// Draws text with a wavy animation effect at the specified vertical position, centered horizontally.
        /// </summary>
        /// <param name="fontName">Name of the font to use.</param>
        /// <param name="y">Vertical position of the text.</param>
        /// <param name="gameTime">Current game time for animation.</param>
        /// <param name="text">Text to draw.</param>
        /// <param name="color">Color of the text.</param>
        public void DrawWavyText(string fontName, int y, GameTime gameTime, string text, Color color)
        {
            var x = (graphics.Width - text.Length * graphics.Fonts[fontName].CharacterWidth) / 2;

            graphics.DrawWavyText(fontName, x, y, gameTime, text, color);
        }

        /// <summary>
        /// Draws text with a wavy animation effect at the specified vertical position, centered horizontally, using white color.
        /// </summary>
        /// <param name="fontName">Name of the font to use.</param>
        /// <param name="y">Vertical position of the text.</param>
        /// <param name="gameTime">Current game time for animation.</param>
        /// <param name="text">Text to draw.</param>
        public void DrawWavyText(string fontName, int y, GameTime gameTime, string text)
            => graphics.DrawWavyText(fontName, y, gameTime, text, Color.White);

        /// <summary>
        /// Draws text with a wavy animation effect at the specified position.
        /// </summary>
        /// <param name="fontName">Name of the font to use.</param>
        /// <param name="x">Horizontal position of the text.</param>
        /// <param name="y">Vertical position of the text.</param>
        /// <param name="gameTime">Current game time for animation.</param>
        /// <param name="text">Text to draw.</param>
        /// <param name="color">Color of the text.</param>
        public void DrawWavyText(string fontName, int x, int y, GameTime gameTime, string text, Color color)
        {
            var font = graphics.Fonts[fontName];

            for (var i = 0; i < text.Length; i++)
            {
                var yOffset = (int) (Math.Cos(gameTime.TotalGameTime.TotalSeconds * 8 + i / 3.0) * 1.95);

                graphics.DrawText(fontName, x + i * (font.CharacterWidth + font.HorizontalSpacing), y + yOffset, text[i], color);
            }
        }

        /// <summary>
        /// Draws text with a wavy animation effect at the specified position, using white color.
        /// </summary>
        /// <param name="fontName">Name of the font to use.</param>
        /// <param name="x">Horizontal position of the text.</param>
        /// <param name="y">Vertical position of the text.</param>
        /// <param name="gameTime">Current game time for animation.</param>
        /// <param name="text">Text to draw.</param>
        public void DrawWavyText(string fontName, int x, int y, GameTime gameTime, string text)
            => graphics.DrawWavyText(fontName, x, y, gameTime, text, Color.White);
    }
}
