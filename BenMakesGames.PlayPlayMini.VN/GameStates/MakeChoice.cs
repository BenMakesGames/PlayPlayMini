using BenMakesGames.PlayPlayMini.GraphicsExtensions;
using BenMakesGames.PlayPlayMini.Services;
using BenMakesGames.PlayPlayMini.VN.Model;
using BenMakesGames.PlayPlayMini.VN.Model.Buttons;
using Microsoft.Xna.Framework;

namespace BenMakesGames.PlayPlayMini.VN.GameStates;

public sealed class MakeChoice: GameState<MakeChoiceConfig>
{
    private GameStateManager GSM { get; }
    private GraphicsManager Graphics { get; }
    private MouseManager Mouse { get; }
    private KeyboardManager Keyboard { get; }

    private AbstractGameState PreviousState { get; }
    private ISceneController SceneController { get; }
    private ButtonCollection Choices { get; }

    public MakeChoice(
        MakeChoiceConfig config,
        GameStateManager gsm, GraphicsManager graphics, MouseManager mouse,
        KeyboardManager keyboard
    )
    {
        GSM = gsm;
        Graphics = graphics;
        Mouse = mouse;
        Keyboard = keyboard;

        SceneController = config.SceneController ?? throw new ArgumentException("SceneController must be set in config");
        PreviousState = config.PreviousState ?? throw new ArgumentException("PreviousState must be set in config");

        Choices = CreateChoiceButtons(config.Choices);
    }

    private ButtonCollection CreateChoiceButtons(IReadOnlyList<Choice> choices)
    {
        var font = Graphics.Fonts[VNSettings.DialogFont];
        var longestLabel = choices.Max(c => c.Title.Length) * (font.CharacterWidth + font.HorizontalSpacing);
        var x = (Graphics.Width - longestLabel) / 2;
        var y = (Graphics.Height - choices.Count * (font.CharacterHeight + font.VerticalSpacing + 4)) / 2;

        var buttons = choices
            .Select(IButton (choice, i) => new TextButton(
                x,
                y + i * (font.CharacterHeight + font.VerticalSpacing + 6),
                () =>
                {
                    choice.Callback(SceneController);
                    SceneController.ExitMiniGame();
                },
                font,
                choice.Title,
                longestLabel + 8
            ));

        return new ButtonCollection(buttons)
        {
            Mouse = Mouse,
            Keyboard = Keyboard
        };
    }

    /// <inheritdoc/>
    public override void Input(GameTime gameTime)
    {
        Choices.Input(GSM, this);
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

        Choices.Draw(Graphics);

        Mouse.Draw(this);
    }
}

public sealed record MakeChoiceConfig(IReadOnlyList<Choice> Choices) : MiniGameConfig;

public sealed record Choice(string Title, Action<ISceneController> Callback);

public static class MakeChoiceExtensions
{
    public static StoryStep Choose(this StoryStep storyStep, List<Choice> choices)
    {
        storyStep.SetMiniGame<MakeChoice, MakeChoiceConfig>(new(choices));

        return storyStep;
    }
}
