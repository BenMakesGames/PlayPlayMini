namespace BenMakesGames.PlayPlayMini.VN.Model.SceneInstructions;

public sealed record RemoveCharacter: ISceneInstructions
{
    private string CharacterId { get; }

    public RemoveCharacter(Character character)
    {
        CharacterId = character.Id;
    }

    public RemoveCharacter(string characterId)
    {
        CharacterId = characterId;
    }

    public void Execute(ISceneController sceneController)
    {
        sceneController.Characters.Remove(CharacterId);
    }
}

public static class RemoveCharacterExtensions
{
    public static StoryStep RemoveCharacter(this StoryStep storyStep, Character character)
    {
        storyStep.Instructions.Add(new RemoveCharacter(character));
        return storyStep;
    }

    public static StoryStep RemoveCharacter(this StoryStep storyStep, string characterId)
    {
        storyStep.Instructions.Add(new RemoveCharacter(characterId));
        return storyStep;
    }
}
