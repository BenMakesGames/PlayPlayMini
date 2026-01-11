using BenMakesGames.PlayPlayMini.GraphicsExtensions;
using BenMakesGames.PlayPlayMini.Services;
using Microsoft.Xna.Framework;

namespace BenMakesGames.PlayPlayMini.VN.Model.Buttons;

public interface IButton: IRectangle<int>
{
    Action? Click { get; }

    void Draw(GraphicsManager graphics, bool isHovered);
}

public static class IButtonExtensions
{
    extension(IButton button)
    {
        public bool IsEnabled => button.Click != null;
    }

    extension(IRectangle<int> rectangle)
    {
        public Vector2 GetCenter() => new(rectangle.X + rectangle.Width / 2f, rectangle.Y + rectangle.Height / 2f);
    }
}
