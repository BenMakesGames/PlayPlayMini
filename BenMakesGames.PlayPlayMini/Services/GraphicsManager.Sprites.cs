using BenMakesGames.PlayPlayMini.Model;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Runtime.CompilerServices;

namespace BenMakesGames.PlayPlayMini.Services;

public sealed partial class GraphicsManager
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Rectangle SpriteRectangle(SpriteSheet spriteSheet, int spriteIndex) => new(
        (spriteIndex % spriteSheet.Columns) * spriteSheet.SpriteWidth,
        (spriteIndex / spriteSheet.Columns) * spriteSheet.SpriteHeight,
        spriteSheet.SpriteWidth,
        spriteSheet.SpriteHeight
    );

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void DrawSpriteWithTransformations(string spriteSheetName, int x, int y, int spriteIndex, SpriteEffects flip, float angle, float scale, Color tint)
        => DrawTextureWithTransformations(
            SpriteSheets[spriteSheetName].Texture,
            x, y,
            SpriteRectangle(SpriteSheets[spriteSheetName], spriteIndex),
            flip,
            angle,
            scale,
            scale,
            tint
        );

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void DrawSpriteWithTransformations(SpriteSheet spriteSheet, int x, int y, int spriteIndex, SpriteEffects flip, float angle, float scale, Color tint)
        => DrawTextureWithTransformations(
            spriteSheet.Texture,
            x, y,
            SpriteRectangle(spriteSheet, spriteIndex),
            flip,
            angle,
            scale,
            scale,
            tint
        );

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void DrawSpriteRotatedAndScaled(string spriteSheet, int x, int y, int spriteIndex, float angle, float scale, Color c) =>
        DrawTextureWithTransformations(
            SpriteSheets[spriteSheet].Texture,
            x,
            y,
            SpriteRectangle(SpriteSheets[spriteSheet], spriteIndex),
            SpriteEffects.None,
            angle,
            scale,
            scale,
            c
        )
    ;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void DrawSpriteRotatedAndScaled(SpriteSheet spriteSheet, int x, int y, int spriteIndex, float angle, float scale, Color c) =>
        DrawTextureWithTransformations(
            spriteSheet.Texture,
            x,
            y,
            SpriteRectangle(spriteSheet, spriteIndex),
            SpriteEffects.None,
            angle,
            scale,
            scale,
            c
        )
    ;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void DrawSprite(string spriteSheetName, int x, int y, int spriteIndex)
        => DrawSprite(SpriteSheets[spriteSheetName], x, y, spriteIndex, Color.White);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void DrawSprite(string spriteSheetName, (int x, int y) position, int spriteIndex)
        => DrawSprite(SpriteSheets[spriteSheetName], position.x, position.y, spriteIndex, Color.White);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void DrawSprite(SpriteSheet spriteSheet, int x, int y, int spriteIndex)
        => DrawSprite(spriteSheet, x, y, spriteIndex, Color.White);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void DrawSprite(SpriteSheet spriteSheet, (int x, int y) position, int spriteIndex)
        => DrawSprite(spriteSheet, position.x, position.y, spriteIndex, Color.White);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void DrawSprite(string spriteSheetName, int x, int y, int spriteIndex, Color tint)
        => DrawSprite(SpriteSheets[spriteSheetName], x, y, spriteIndex, tint);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void DrawSprite(string spriteSheetName, (int x, int y) position, int spriteIndex, Color tint)
        => DrawSprite(SpriteSheets[spriteSheetName], position.x, position.y, spriteIndex, tint);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void DrawSprite(SpriteSheet spriteSheet, (int x, int y) position, int spriteIndex, Color tint)
        => DrawSprite(spriteSheet, position.x, position.y, spriteIndex, tint);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void DrawSprite(SpriteSheet spriteSheet, int x, int y, int spriteIndex, Color tint) =>
        DrawTexture(
            spriteSheet.Texture,
            x, y,
            SpriteRectangle(spriteSheet, spriteIndex),
            tint
        );

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void DrawSpriteStretched(string spriteSheetName, int x, int y, int width, int height, int spriteIndex) =>
        DrawTextureStretched(
            SpriteSheets[spriteSheetName].Texture,
            x, y,
            width, height,
            SpriteRectangle(SpriteSheets[spriteSheetName], spriteIndex),
            Color.White
        )
    ;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void DrawSpriteStretched(SpriteSheet spriteSheet, int x, int y, int width, int height, int spriteIndex) =>
        DrawTextureStretched(
            spriteSheet.Texture,
            x, y,
            width, height,
            SpriteRectangle(spriteSheet, spriteIndex),
            Color.White
        )
    ;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void DrawSpriteFlipped(string spriteSheetName, int x, int y, int spriteIndex, SpriteEffects flip) =>
        DrawTextureFlipped(
            SpriteSheets[spriteSheetName].Texture,
            x,
            y,
            SpriteRectangle(SpriteSheets[spriteSheetName], spriteIndex),
            flip,
            Color.White
        )
    ;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void DrawSpriteFlipped(SpriteSheet spriteSheet, int x, int y, int spriteIndex, SpriteEffects flip) =>
        DrawTextureFlipped(
            spriteSheet.Texture,
            x,
            y,
            SpriteRectangle(spriteSheet, spriteIndex),
            flip,
            Color.White
        )
    ;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void DrawSpriteFlipped(SpriteSheet spriteSheet, int x, int y, int spriteIndex, SpriteEffects flip, Color tint) =>
        DrawTextureFlipped(
            spriteSheet.Texture,
            x,
            y,
            SpriteRectangle(spriteSheet, spriteIndex),
            flip,
            tint
        )
    ;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void DrawSpriteFlipped(string spriteName, int x, int y, int spriteIndex, SpriteEffects flip, Color tint) =>
        DrawTextureFlipped(
            SpriteSheets[spriteName].Texture,
            x,
            y,
            SpriteRectangle(SpriteSheets[spriteName], spriteIndex),
            flip,
            tint
        );
}
