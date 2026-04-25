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

            var x = (graphics.Width - font.ComputeWidth(text)) / 2;
            var y = (graphics.Height - font.MaxCharacterHeight) / 2;

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
            var font = graphics.Fonts[fontName];
            var x = (graphics.Width - font.ComputeWidth(text)) / 2;

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

            if (font.Sheets.Count == 1)
            {
                var sheet = font.Sheets[0];

                for (var i = 0; i < text.Length; i++)
                {
                    var yOffset = (int) (Math.Cos(gameTime.TotalGameTime.TotalSeconds * 8 + i / 3.0) * 1.95);

                    graphics.DrawText(sheet, x + i * (sheet.CharacterWidth + sheet.HorizontalSpacing), y + yOffset, text[i], color);
                }

                return;
            }

            var currentX = x;

            for (var i = 0; i < text.Length; i++)
            {
                var yOffset = (int) (Math.Cos(gameTime.TotalGameTime.TotalSeconds * 8 + i / 3.0) * 1.95);

                if (font.TryGetSheet(text[i], out var sheet))
                {
                    graphics.DrawText(sheet!, currentX, y + yOffset, text[i], color);
                    currentX += sheet!.CharacterWidth + sheet.HorizontalSpacing;
                }
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
