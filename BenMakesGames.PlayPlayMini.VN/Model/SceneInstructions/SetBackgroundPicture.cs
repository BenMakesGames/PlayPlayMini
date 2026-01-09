namespace BenMakesGames.PlayPlayMini.VN.Model.SceneInstructions;

public sealed record SetBackgroundPicture(string? Picture): ISceneInstructions
{
    public void Execute(ISceneController sceneController)
    {
        sceneController.SetBackgroundPicture(Picture);
    }
}

public static class SetBackgroundPictureExtensions
{
    public static StoryStep SetBackgroundPicture(this StoryStep storyStep, string? picture)
    {
        storyStep.Instructions.Add(new SetBackgroundPicture(picture));
        return storyStep;
    }

    public static StoryStep RemoveBackgroundPicture(this StoryStep storyStep)
    {
        storyStep.Instructions.Add(new SetBackgroundPicture(null));
        return storyStep;
    }
}
