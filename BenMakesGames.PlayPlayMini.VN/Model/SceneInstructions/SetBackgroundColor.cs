using Microsoft.Xna.Framework;

namespace BenMakesGames.PlayPlayMini.VN.Model.SceneInstructions;

public sealed record SetBackgroundColor(Color Color): ISceneInstructions
{
    public void Execute(ISceneController sceneController)
    {
        sceneController.BackgroundColor = Color;
    }
}

public static class SetBackgroundColorExtensions
{
    public static StoryStep SetBackgroundColor(this StoryStep storyStep, Color color)
    {
        storyStep.Instructions.Add(new SetBackgroundColor(color));
        return storyStep;
    }
}
