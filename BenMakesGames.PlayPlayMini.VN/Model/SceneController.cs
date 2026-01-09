using Autofac;
using BenMakesGames.PlayPlayMini.Extensions;
using BenMakesGames.PlayPlayMini.GraphicsExtensions;
using BenMakesGames.PlayPlayMini.Services;
using BenMakesGames.PlayPlayMini.VN.Extensions;
using BenMakesGames.PlayPlayMini.VN.GameStates;
using BenMakesGames.PlayPlayMini.VN.Model.Dialog;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace BenMakesGames.PlayPlayMini.VN.Model;

public interface ISceneController
{
    ILifetimeScope IoCContainer { get; }
    Color BackgroundColor { get; set; }
    Texture2D? BackgroundPicture { get; set; }
    Dictionary<string, SceneCharacter> Characters { get; }

    List<StoryStep> StorySteps { get; }

    void SetBackgroundPicture(string? pictureName);

    void ExitMiniGame();
}

internal sealed class SceneController: ISceneController
{
    public bool IsDone => StorySteps.Count == 0;

    public Color BackgroundColor { get; set; } = VNSettings.SceneDefaultBackgroundColor;

    public Texture2D? BackgroundPicture { get; set; }

    private float BackgroundImageX { get; set; }
    private float BackgroundImageY { get; set; }
    private int TargetBackgroundImageX { get; set; }
    private int TargetBackgroundImageY { get; set; }

    public Dictionary<string, SceneCharacter> Characters { get; } = [ ];

    private PlayScene PlayScene { get; }
    public ILifetimeScope IoCContainer { get; }
    public List<StoryStep> StorySteps { get; }
    private AnimatedDialog? Dialog { get; set; }

    public SceneController(
        PlayScene playScene,
        ILifetimeScope iocContainer,
        List<StoryStep> storySteps
    )
    {
        PlayScene = playScene;
        StorySteps = storySteps;
        IoCContainer = iocContainer;

        ApplyCurrentStep();
    }

    private void ApplyCurrentStep()
    {
        if (StorySteps.Count == 0)
            return;

        Dialog = StorySteps[0].Dialog is { } dialog ? new AnimatedDialog(IoCContainer.Resolve<GraphicsManager>(), dialog) : null;

        foreach (var i in StorySteps[0].Instructions)
        {
            i.Execute(this);
        }
    }

    public void Input(GameStateManager gsm, GraphicsManager graphics, MouseManager mouse)
    {
        if (mouse.Moved && BackgroundPicture != null)
        {
            var xOverflow = graphics.Width - BackgroundPicture.Width;
            var yOverflow = graphics.Height - BackgroundPicture.Height;

            TargetBackgroundImageX = Math.Clamp(mouse.X, 0, graphics.Width) * xOverflow / graphics.Width;
            TargetBackgroundImageY = Math.Clamp(mouse.Y, 0, graphics.Height) * yOverflow / graphics.Height;
        }

        if (mouse.LeftClicked && graphics.Contains(mouse) && gsm.IsActive)
        {
            if (Dialog == null)
            {
                AdvanceStep();
            }
            else
            {
                Dialog.Click();

                if (!Dialog.HasText)
                    AdvanceStep();
            }
        }
    }

    private void AdvanceStep()
    {
        if (StorySteps[0].MiniGame is { } miniGame)
        {
            var gsm = IoCContainer.Resolve<GameStateManager>();
            gsm.ChangeState(miniGame(gsm, this));
        }
        else
        {
            StorySteps.RemoveAt(0);
            ApplyCurrentStep();
        }
    }

    public void SetBackgroundPicture(string? pictureName)
    {
        BackgroundPicture = pictureName is null ? null : IoCContainer.Resolve<GraphicsManager>().Pictures[pictureName];
    }

    public void ExitMiniGame()
    {
        IoCContainer.Resolve<GameStateManager>().ChangeState(PlayScene);
        StorySteps.RemoveAt(0);
        ApplyCurrentStep();
    }

    public void Update(GameTime gameTime)
    {
        foreach(var c in Characters)
            c.Value.Update(gameTime);

        BackgroundImageX = MathHelper.Lerp(BackgroundImageX, TargetBackgroundImageX, (float)gameTime.ElapsedGameTime.TotalSeconds * 5);
        BackgroundImageY = MathHelper.Lerp(BackgroundImageY, TargetBackgroundImageY, (float)gameTime.ElapsedGameTime.TotalSeconds * 5);

        Dialog?.Update(gameTime);
    }

    public void Draw(GraphicsManager graphics)
    {
        graphics.Clear(BackgroundColor);

        if (BackgroundPicture != null)
        {
            graphics.DrawTexture(
                BackgroundPicture,
                /*(screenShake?.XOffset ?? 0) + */(int)BackgroundImageX,
                /*(screenShake?.YOffset ?? 0) + */(int)BackgroundImageY
            );
        }

        foreach (var c in Characters)
            c.Value.Draw(graphics);

        Dialog?.Draw(graphics, 0);
    }
}

public sealed class AnimatedDialog
{
    public bool HasText => WrappedText.Count > 0;

    private DialogStyle DialogStyle { get; }
    private Character? Speaker { get; }
    private int Lines { get; }

    private List<string> WrappedText { get; } = [ ];
    private double TimeAlive { get; set; }

    public AnimatedDialog(GraphicsManager graphics, IDialog dialog)
    {
        DialogStyle = dialog.Style;
        Speaker = dialog.Speaker;
        Lines = dialog.Lines;

        var allWrappedText = dialog.Text.WrapText(graphics.Fonts[VNSettings.DialogFont], graphics.Width - 10);
        var textLines = allWrappedText.Split('\n');

        for (int i = 0; i < textLines.Length; i += dialog.Lines)
        {
            var group = string.Join("\n", textLines.Skip(i).Take(dialog.Lines));
            WrappedText.Add(group);
        }
    }

    public void Click()
    {
        if (WrappedText.Count == 0)
            return;

        if(TimeAlive * 40 < WrappedText[0].Length)
            TimeAlive = WrappedText[0].Length / 40.0;
        else
        {
            WrappedText.RemoveAt(0);
            TimeAlive = 0;
        }
    }

    public void Update(GameTime gameTime)
    {
        TimeAlive += gameTime.ElapsedGameTime.TotalSeconds;
    }

    public void Draw(GraphicsManager graphics, int xOffset)
    {
        if (!HasText)
            return;

        var text = WrappedText[0].AsSpan();

        if (TimeAlive * 40 < WrappedText[0].Length)
            text = text[..(int)(TimeAlive * 40)];

        if (DialogStyle == DialogStyle.Speaking)
        {
            if(Speaker is not null)
                graphics.DrawCharacterDialog(Speaker, text, xOffset, Lines);

            return;
        }

        var font = graphics.Fonts[VNSettings.DialogFont];
        var dialogHeight = Lines * (font.CharacterHeight + font.VerticalSpacing) + 7;

        switch (DialogStyle)
        {
            case DialogStyle.Thinking:
                graphics.DrawFilledRectangle(xOffset, graphics.Height - dialogHeight, graphics.Width, dialogHeight, VNSettings.DialogThinkingBackgroundColor);
                graphics.DrawFilledRectangle(xOffset, graphics.Height - dialogHeight + 1, graphics.Width, 1, VNSettings.DialogThinkingTextColor);
                graphics.DrawText(font, xOffset + 5, graphics.Height - dialogHeight + 5, text, VNSettings.DialogThinkingTextColor);
                break;

            case DialogStyle.None:
                graphics.DrawTextWithOutline(font, xOffset + 5, graphics.Height - dialogHeight + 5, text, VNSettings.DialogTransparentTextColor, VNSettings.DialogTransparentOutlineColor);
                break;

            case DialogStyle.NoneInverted:
                graphics.DrawTextWithOutline(font, xOffset + 5, graphics.Height - dialogHeight + 5, text, VNSettings.DialogTransparentOutlineColor, VNSettings.DialogTransparentTextColor);
                break;
        }
    }
}
