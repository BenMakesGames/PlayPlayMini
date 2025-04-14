using System;
using BenMakesGames.PlayPlayMini.Attributes.DI;
using BenMakesGames.PlayPlayMini.GraphicsExtensions;
using BenMakesGames.PlayPlayMini.Model;
using BenMakesGames.PlayPlayMini.Services;
using BenMakesGames.PlayPlayMini.UI.Model;
using BenMakesGames.PlayPlayMini.UI.UIElements;
using Microsoft.Xna.Framework;

namespace BenMakesGames.PlayPlayMini.UI.Services;

[AutoRegister(Lifetime.PerDependency)]
public sealed class UIService
{
    public GraphicsManager Graphics { get; }
    public UIThemeProvider ThemeProvider { get; }

    private MouseManager Cursor { get; }
    private SoundManager Sounds { get; }

    public Font Font => Graphics.Fonts[GetTheme().FontName];

    public Canvas Canvas { get; }

    public bool DebugMode { get; set; } = false;

    private Click? PreviousClick { get; set; }
    private Click? MouseDown { get; set; }

    public IUIElement? Hovered { get; private set; }

    public UIService(
        GraphicsManager gm, MouseManager cursor, UIThemeProvider themeProvider, SoundManager sounds
    )
    {
        Graphics = gm;
        Cursor = cursor;
        ThemeProvider = themeProvider;
        Sounds = sounds;

        Canvas = new Canvas(this)
        {
            X = 0,
            Y = 0,
            Width = gm.Width,
            Height = gm.Height,
            UI = this
        };
    }

    public Theme GetTheme() => ThemeProvider.GetTheme();

    public void AlwaysDraw(GameTime gameTime)
    {
        DrawElement(Canvas, 0, 0, gameTime);
    }

    private void DrawElement(IUIElement e, int xOffset, int yOffset, GameTime gameTime)
    {
        e.Draw(xOffset, yOffset, gameTime);

        for (var i = 0; i < e.Children.Count; i++)
        {
            if(e.Children[i].Visible)
                DrawElement(e.Children[i], xOffset + e.X, yOffset + e.Y, gameTime);
        }

        if (e == Hovered && DebugMode)
            Graphics.DrawRectangle(e.X + xOffset, e.Y + yOffset, e.Width, e.Height, Color.Red);
    }

    /// <summary>
    /// Should only be called by a GameState when that GameState is the GameStateManager's CurrentState.
    /// </summary>
    public void ActiveDraw(GameTime gameTime, bool showMouse = true)
    {
        if (showMouse)
            Cursor.Draw(gameTime);
    }

    /// <summary>
    /// Should only be called by a GameState when that GameState is the GameStateManager's CurrentState.
    /// </summary>
    public void ActiveUpdate(GameTime gameTime)
    {
        DoMouseOverElement(Canvas, Cursor.X, Cursor.Y);

        if (Hovered == null)
            return;

        if (Cursor.LeftDown)
        {
            MouseDown ??= new Click(gameTime.TotalGameTime, Hovered, Cursor.X, Cursor.Y);
        }
        else if (MouseDown is { } click)
        {
            MouseDown = null;

            if (PointsWithinDistance(click.X, click.Y, Cursor.X, Cursor.Y, 2))
            {
                if (PreviousClick is not null && (gameTime.TotalGameTime - PreviousClick.When).TotalMilliseconds <= 500)
                {
                    if (click.What == PreviousClick.What && PointsWithinDistance(click.X, click.Y, PreviousClick.X, PreviousClick.Y, 3))
                        Hovered.DoDoubleClick?.Invoke(new(click.X - Hovered.X, click.Y - Hovered.Y, Cursor.X, Cursor.Y));

                    PreviousClick = null;
                }
                else
                {
                    if(Hovered.CanClick)
                    {
                        if(ThemeProvider.GetTheme().ButtonClickSoundName is {} soundName)
                            Sounds.PlaySound(soundName);

                        Hovered.DoClick?.Invoke(new(click.X - Hovered.X, click.Y - Hovered.Y, Cursor.X, Cursor.Y));
                    }

                    PreviousClick = click;
                }
            }
        }
    }

    private static bool PointsWithinDistance(int x1, int y1, int x2, int y2, int maxDistance)
    {
        return (x2 - x1) * (x2 - x1) + (y2 - y1) * (y2 - y1) <= maxDistance * maxDistance;
    }

    private void DoMouseOverElement(IUIElement e, int x, int y)
    {
        for(var i = e.Children.Count - 1; i >= 0; i--)
        {
            var child = e.Children[i];

            if (child.Contains(x, y))
            {
                DoMouseOverElement(child, x - child.X, y - child.Y);
                return;
            }
        }

        if (e != Hovered)
        {
            Hovered?.DoMouseExit?.Invoke();

            Hovered = e;

            Hovered.DoMouseEnter?.Invoke();

            if(Hovered.CanClick && ThemeProvider.GetTheme().ButtonHoverSoundName is {} soundName)
                Sounds.PlaySound(soundName);
        }
    }

    public Font GetFont() => Graphics.Fonts[GetTheme().FontName];

    private sealed record Click(TimeSpan When, IUIElement What, int X, int Y);
}
