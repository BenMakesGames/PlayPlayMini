using BenMakesGames.PlayPlayMini.GraphicsExtensions;
using BenMakesGames.PlayPlayMini.Model;
using BenMakesGames.PlayPlayMini.Services;
using BenMakesGames.PlayPlayMini.VN.Model;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace BenMakesGames.PlayPlayMini.VN.Extensions;

public static class GraphicsManagerExtensions
{
    public static void DrawCharacterDialog(this GraphicsManager graphics, Character speaker, ReadOnlySpan<char> wrappedText, int xOffset = 0, int dialogLines = 2)
    {
        var textX = xOffset + 5;
        var font = graphics.Fonts[VNSettings.DialogFont];
        var dialogHeight = dialogLines * (font.CharacterHeight + font.VerticalSpacing) + 7;
        var speakerLength = speaker.Name.Length * (font.CharacterWidth + font.HorizontalSpacing) - font.HorizontalSpacing;

        graphics.DrawFilledRectangle(xOffset, graphics.Height - dialogHeight, graphics.Width, dialogHeight, VNSettings.DialogSpeakingBackgroundColor);

        graphics.DrawFilledRectangle(textX - 1, graphics.Height - dialogHeight - font.CharacterHeight - 1, speakerLength + 3, 1, VNSettings.DialogSpeakingBackgroundColor);
        graphics.DrawFilledRectangle(textX - 2, graphics.Height - dialogHeight - font.CharacterHeight, speakerLength + 5, 1, VNSettings.DialogSpeakingBackgroundColor);
        graphics.DrawFilledRectangle(textX - 3, graphics.Height - dialogHeight - font.CharacterHeight + 1, speakerLength + 7, font.CharacterHeight + 1, VNSettings.DialogSpeakingBackgroundColor);
        graphics.DrawFilledRectangle(textX - 1, graphics.Height - dialogHeight - font.CharacterHeight, speakerLength + 3, 1, speaker.SpeakingColor);
        graphics.DrawFilledRectangle(textX - 2, graphics.Height - dialogHeight - font.CharacterHeight + 1, speakerLength + 5, font.CharacterHeight, speaker.SpeakingColor);
        graphics.DrawFilledRectangle(xOffset, graphics.Height - dialogHeight + 1, graphics.Width, 1, speaker.SpeakingColor);
        graphics.DrawText(font, textX, graphics.Height - dialogHeight - font.CharacterHeight + 2, speaker.Name, VNSettings.DialogSpeakingBackgroundColor);

        graphics.DrawText(font, textX, graphics.Height - dialogHeight + 5, wrappedText, VNSettings.DialogSpeakingTextColor);
    }

    public static void DrawSpriteFlippedWithOutline(this GraphicsManager graphics, SpriteSheet spriteSheet, int x, int y, int spriteIndex, SpriteEffects effects, Color outlineColor)
    {
        // TODO: this outline color doesn't actually work. we should use a pixel shader here
        graphics.DrawSpriteFlipped(spriteSheet, x - 1, y, spriteIndex, effects, outlineColor);
        graphics.DrawSpriteFlipped(spriteSheet, x + 1, y, spriteIndex, effects, outlineColor);
        graphics.DrawSpriteFlipped(spriteSheet, x, y - 1, spriteIndex, effects, outlineColor);
        graphics.DrawSpriteFlipped(spriteSheet, x, y + 1, spriteIndex, effects, outlineColor);
        graphics.DrawSpriteFlipped(spriteSheet, x, y, spriteIndex, effects);
    }

    public static void DrawSpriteFlippedWithOutline(this GraphicsManager graphics, string name, int x, int y, int spriteIndex, SpriteEffects effects, Color outlineColor)
        => graphics.DrawSpriteFlippedWithOutline(graphics.SpriteSheets[name], x, y, spriteIndex, effects, outlineColor);
}
