namespace BenMakesGames.PlayPlayMini.VN.Model.SceneInstructions;

public sealed record AddCharacter(Character Character, int X, int Y, bool FlippedHorizontally): ISceneInstructions
{
    public void Execute(ISceneController sceneController)
    {
        sceneController.Characters[Character.Id] = new SceneCharacter(Character, X, Y, FlippedHorizontally);
    }
}

public static class AddCharacterExtensions
{
    public static StoryStep AddCharacter(this StoryStep storyStep, Character character, int x, int y, bool flippedHorizontally = false)
    {
        storyStep.Instructions.Add(new AddCharacter(character, x, y, flippedHorizontally));
        return storyStep;
    }
}
