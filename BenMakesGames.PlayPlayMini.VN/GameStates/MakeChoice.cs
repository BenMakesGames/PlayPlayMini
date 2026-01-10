using BenMakesGames.PlayPlayMini.GraphicsExtensions;
using BenMakesGames.PlayPlayMini.Services;
using BenMakesGames.PlayPlayMini.VN.Model;
using Microsoft.Xna.Framework;

namespace BenMakesGames.PlayPlayMini.VN.GameStates;

public sealed class MakeChoice: GameState<MakeChoiceConfig>
{
    private GameStateManager GSM { get; }
    private GraphicsManager Graphics { get; }
    private MouseManager Mouse { get; }

    private AbstractGameState PreviousState { get; }
    private ISceneController SceneController { get; }
    private List<ChoiceButton> Choices { get; }
    private ChoiceButton? HoveredChoice { get; set; }

    public MakeChoice(
        MakeChoiceConfig config,
        GameStateManager gsm, GraphicsManager graphics, MouseManager mouse
    )
    {
        GSM = gsm;
        Graphics = graphics;
        Mouse = mouse;

        SceneController = config.SceneController ?? throw new ArgumentException();
        PreviousState = config.PreviousState ?? throw new ArgumentException();

        var font = Graphics.Fonts[VNSettings.DialogFont];
        var longestLabel = config.Choices.Max(c => c.Title.Length) * (font.CharacterWidth + font.HorizontalSpacing);
        var x = (Graphics.Width - longestLabel) / 2;
        var y = (Graphics.Height - config.Choices.Count * (font.CharacterHeight + font.VerticalSpacing + 4)) / 2;

        Choices = config.Choices
            .Select((choice, i) => new ChoiceButton()
            {
                X = x,
                Y = y + i * (font.CharacterHeight + font.VerticalSpacing + 6),
                Width = longestLabel + 6,
                Height = font.CharacterHeight + 4,
                Label = choice.Title,
                Action = () =>
                {
                    choice.Callback(SceneController);
                    SceneController.ExitMiniGame();
                }
            })
            .ToList()
        ;

        HoveredChoice = Choices.FirstOrDefault(c => c.Contains(Mouse));
    }

    /// <inheritdoc/>
    public override void Input(GameTime gameTime)
    {
        if(Mouse.Moved)
            HoveredChoice = Choices.FirstOrDefault(c => c.Contains(Mouse));

        if (Mouse.LeftClicked && HoveredChoice is { } choice)
            choice.Action();
    }

    /// <inheritdoc/>
    public override void Update(GameTime gameTime)
    {
        PreviousState.Update(gameTime);
    }

    /// <inheritdoc/>
    public override void Draw(GameTime gameTime)
    {
        PreviousState.Draw(gameTime);

        foreach (var choice in Choices)
        {
            var (backgroundColor, textColor) = choice == HoveredChoice
                ? (VNSettings.ChoiceHoveredBackgroundColor, VNSettings.ChoiceHoveredTextColor)
                : (VNSettings.ChoiceBackgroundColor, VNSettings.ChoiceTextColor);

            Graphics.DrawFilledRectangle(choice.X, choice.Y, choice.Width, choice.Height, backgroundColor);
            Graphics.DrawText("Font", choice.X + 3, choice.Y + 2, choice.Label, textColor);
        }

        Mouse.Draw(this);
    }
}

public sealed record MakeChoiceConfig(IReadOnlyList<Choice> Choices): MiniGameConfig
{
}

public sealed class ChoiceButton: IRectangle<int>
{
    public required int X { get; init; }
    public required int Y { get; init; }
    public required int Width { get; init; }
    public required int Height { get; init; }

    public required string Label { get; init; }
    public required Action Action { get; init; }
}

public sealed record Choice(string Title, Action<ISceneController> Callback);

public static class MakeChoiceExtensions
{
    public static StoryStep Choose(this StoryStep storyStep, List<Choice> choices)
    {
        storyStep.SetMiniGame<MakeChoice, MakeChoiceConfig>(new(choices));

        return storyStep;
    }
}
