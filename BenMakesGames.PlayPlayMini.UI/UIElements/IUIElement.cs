using System;
using BenMakesGames.PlayPlayMini.UI.Services;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using BenMakesGames.PlayPlayMini.UI.Model;

namespace BenMakesGames.PlayPlayMini.UI.UIElements;

public interface IUIElement
{
    UIService UI { get; }

    int X { get; }
    int Y { get; }
    int Width { get; }
    int Height { get; }
    bool Visible { get; }
    IReadOnlyList<IUIElement> Children { get; }

    Action<ClickEvent>? DoClick { get; }
    Action<DoubleClickEvent>? DoDoubleClick { get; }
    Action? DoMouseEnter { get; }
    Action? DoMouseExit { get; }

    void Draw(int xOffset, int yOffset, GameTime gameTime);
}