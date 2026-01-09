namespace BenMakesGames.PlayPlayMini.VN.Model.SceneInstructions;

public sealed record RemoveAllCharacters: ISceneInstructions
{
    public void Execute(ISceneController sceneController)
    {
        sceneController.Characters.Clear();
    }
}

public static class RemoveAllCharactersExtensions
{
    public static StoryStep RemoveAllCharacters(this StoryStep storyStep)
    {
        storyStep.Instructions.Add(new RemoveAllCharacters());
        return storyStep;
    }
}
