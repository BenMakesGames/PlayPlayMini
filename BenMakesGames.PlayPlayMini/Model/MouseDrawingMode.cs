namespace BenMakesGames.PlayPlayMini.Model;

public enum MouseDrawingMode
{
    /// <summary>
    /// Do not render the mouse cursor while it is over the game window.
    /// </summary>
    None,

    /// <summary>
    /// Render the system default mouse cursor while it is over the game window.
    /// </summary>
    System,

    /// <summary>
    /// A custom cursor will be drawn. It is expected that the active game state will
    /// call MouseManager.Draw() to draw this cursor.
    /// </summary>
    Custom
}