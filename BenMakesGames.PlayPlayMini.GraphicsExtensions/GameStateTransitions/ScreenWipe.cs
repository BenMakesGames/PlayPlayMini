using System;
using BenMakesGames.PlayPlayMini.Services;
using Microsoft.Xna.Framework;

namespace BenMakesGames.PlayPlayMini.GraphicsExtensions.GameStateTransitions;

public sealed class ScreenWipe: GameState<ScreenWipeConfig>
{
    private GraphicsManager Graphics { get; }
    private ScreenWipeConfig Config { get; }
    private GameStateManager GSM { get; }

    private int Step { get; set; }
    private double Age { get; set; }

    public ScreenWipe(
        GraphicsManager graphics, ScreenWipeConfig config, GameStateManager gsm
    )
    {
        Graphics = graphics;
        Config = config;
        GSM = gsm;
    }

    /// <inheritdoc />
    public override void Update(GameTime gameTime)
    {
        if (GSM.CurrentState != this)
            return;

        Age += gameTime.ElapsedGameTime.TotalSeconds;

        if(Step == 0) // wipe out
        {
            if(Age >= Config.WipeTime)
            {
                Step++;
                Age -= Config.WipeTime;
            }
        }

        if(Step == 1) // hold
        {
            if (Age >= Config.MinimumHoldTime && (Config.HoldUntil?.Invoke() ?? true))
            {
                Step++;
                Age -= Config.MinimumHoldTime;
            }
        }

        if (Step == 2) // wipe in
        {
            if (Age >= Config.WipeTime)
            {
                GSM.ChangeState(Config.NextState);
            }
        }
    }

    /// <inheritdoc />
    public override void Draw(GameTime gameTime)
    {
        if (Step == 0)
        {
            Config.PreviousState.Draw(gameTime);
            DrawWipe(Age / Config.WipeTime, false);
        }
        else if (Step == 1)
        {
            Graphics.Clear(Config.Color);

            if(Config.Message != null && Config.MessageColor.HasValue)
            {
                var x = (Graphics.Width - Config.Message.Length * Graphics.Fonts["Font"].CharacterWidth) / 2;
                Graphics.DrawText("Font", x, Graphics.Height / 2 - 5, Config.Message, Config.MessageColor.Value);
            }
        }
        else
        {
            Config.NextState.Draw(gameTime);
            DrawWipe(Age / Config.WipeTime, true);
        }
    }

    private void DrawWipe(double progress, bool reentering)
    {
        switch(Config.Direction)
        {
            case ScreenWipeDirection.LeftToRight: DrawWipeLeftToRight(progress, reentering); break;
            case ScreenWipeDirection.RightToLeft: DrawWipeRightToLeft(progress, reentering); break;
            case ScreenWipeDirection.TopToBottom: DrawWipeTopToBottom(progress, reentering); break;
            case ScreenWipeDirection.BottomToTop: DrawWipeBottomToTop(progress, reentering); break;
        }
    }

    private void DrawWipeRightToLeft(double progress, bool reentering)
    {
        var width = (int)(Graphics.Width * progress);

        if (reentering)
            width = Graphics.Width - width;

        var x = reentering ? 0 : Graphics.Width - width;

        Graphics.DrawFilledRectangle(x, 0, width, Graphics.Height, Config.Color);
    }

    private void DrawWipeLeftToRight(double progress, bool reentering)
    {
        var width = (int)(Graphics.Width * progress);

        if (reentering)
            width = Graphics.Width - width;

        var x = reentering ? Graphics.Width - width : 0;

        Graphics.DrawFilledRectangle(x, 0, width, Graphics.Height, Config.Color);
    }

    private void DrawWipeTopToBottom(double progress, bool reentering)
    {
        var height = (int)(Graphics.Height * progress);

        if (reentering)
            height = Graphics.Height - height;

        var y = reentering ? Graphics.Height - height : 0;

        Graphics.DrawFilledRectangle(0, y, Graphics.Width, height, Config.Color);
    }

    private void DrawWipeBottomToTop(double progress, bool reentering)
    {
        var height = (int)(Graphics.Height * progress);

        if (reentering)
            height = Graphics.Height - height;

        var y = reentering ? 0 : Graphics.Height - height;

        Graphics.DrawFilledRectangle(0, y, Graphics.Width, height, Config.Color);
    }
}

public readonly struct ScreenWipeConfig
{
    public required AbstractGameState PreviousState { get; init; }
    public required AbstractGameState NextState { get; init; }
    public string? Message { get; init; }
    public Color? MessageColor { get; init; }
    public Func<bool>? HoldUntil { get; init; }
    public required ScreenWipeDirection Direction { get; init; }
    public Color Color { get; init; } = Color.Black;
    public double WipeTime { get; init; } = 0.2f;
    public double MinimumHoldTime { get; init; }

    public ScreenWipeConfig()
    {
    }
}

/// <summary>
/// Which direction the screen wipe goes.
/// </summary>
public enum ScreenWipeDirection
{
    LeftToRight,
    RightToLeft,
    TopToBottom,
    BottomToTop
}
