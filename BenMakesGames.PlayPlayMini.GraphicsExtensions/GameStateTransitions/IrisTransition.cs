using System;
using BenMakesGames.PlayPlayMini.Services;
using Microsoft.Xna.Framework;

namespace BenMakesGames.PlayPlayMini.GraphicsExtensions.GameStateTransitions;

public sealed class IrisTransition: GameState<IrisTransitionConfig>
{
    private GraphicsManager Graphics { get; }
    private IrisTransitionConfig Config { get; }
    private GameStateManager GSM { get; }

    private int Step { get; set; }
    private double Age { get; set; }
    private double MaxRadius { get; }

    public IrisTransition(
        GraphicsManager graphics, IrisTransitionConfig config, GameStateManager gsm
    )
    {
        Graphics = graphics;
        Config = config;
        GSM = gsm;

        var fx = config.FocalPoint.X;
        var fy = config.FocalPoint.Y;
        var w = graphics.Width;
        var h = graphics.Height;
        var dxMax = Math.Max(fx, w - fx);
        var dyMax = Math.Max(fy, h - fy);
        MaxRadius = Math.Sqrt((double)dxMax * dxMax + (double)dyMax * dyMax);
    }

    /// <inheritdoc />
    public override void Update(GameTime gameTime)
    {
        if (GSM.CurrentState != this)
            return;

        Age += gameTime.ElapsedGameTime.TotalSeconds;

        if (Step == 0) // close
        {
            if (Age >= Config.TransitionTime)
            {
                Step++;
                Age -= Config.TransitionTime;
            }
        }

        if (Step == 1) // hold
        {
            if (Age >= Config.MinimumHoldTime && (Config.HoldUntil?.Invoke() ?? true))
            {
                Step++;
                Age -= Config.MinimumHoldTime;
            }
        }

        if (Step == 2) // open
        {
            if (Age >= Config.TransitionTime)
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
            DrawIris(1.0 - Age / Config.TransitionTime);
        }
        else if (Step == 1)
        {
            Graphics.Clear(Config.Color);

            if (Config.Message != null && Config.MessageColor.HasValue)
            {
                var x = (Graphics.Width - Graphics.Fonts["Font"].ComputeWidth(Config.Message)) / 2;
                Graphics.DrawText("Font", x, Graphics.Height / 2 - 5, Config.Message, Config.MessageColor.Value);
            }
        }
        else
        {
            Config.NextState.Draw(gameTime);
            DrawIris(Age / Config.TransitionTime);
        }
    }

    // openFraction: 0.0 = fully closed (entire screen is Config.Color), 1.0 = fully open (no color drawn).
    private void DrawIris(double openFraction)
    {
        var clamped = Math.Clamp(openFraction, 0.0, 1.0);
        var radius = (int)(MaxRadius * clamped);
        var cx = Config.FocalPoint.X;
        var cy = Config.FocalPoint.Y;
        var w = Graphics.Width;
        var h = Graphics.Height;
        var color = Config.Color;

        if (radius <= 0)
        {
            Graphics.DrawFilledRectangle(0, 0, w, h, color);
            return;
        }

        var topH = cy - radius;
        if (topH > 0)
            Graphics.DrawFilledRectangle(0, 0, w, topH, color);

        var bottomY = cy + radius + 1;
        if (bottomY < h)
            Graphics.DrawFilledRectangle(0, bottomY, w, h - bottomY, color);

        var yStart = Math.Max(0, cy - radius);
        var yEnd = Math.Min(h - 1, cy + radius);
        var rSq = radius * radius;

        for (var y = yStart; y <= yEnd; y++)
        {
            var dy = y - cy;
            var halfWidth = (int)Math.Sqrt(rSq - dy * dy);
            var left = cx - halfWidth;
            var right = cx + halfWidth;

            if (left > 0)
                Graphics.DrawFilledRectangle(0, y, left, 1, color);
            if (right + 1 < w)
                Graphics.DrawFilledRectangle(right + 1, y, w - right - 1, 1, color);
        }
    }
}

public readonly struct IrisTransitionConfig
{
    public required AbstractGameState PreviousState { get; init; }
    public required AbstractGameState NextState { get; init; }
    public required Point FocalPoint { get; init; }
    public string? Message { get; init; }
    public Color? MessageColor { get; init; }
    public Func<bool>? HoldUntil { get; init; }
    public Color Color { get; init; } = Color.Black;
    public double TransitionTime { get; init; } = 0.3;
    public double MinimumHoldTime { get; init; }

    public IrisTransitionConfig()
    {
    }
}
