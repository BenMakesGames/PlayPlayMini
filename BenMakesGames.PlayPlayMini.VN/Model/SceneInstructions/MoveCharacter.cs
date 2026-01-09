namespace BenMakesGames.PlayPlayMini.VN.Model.SceneInstructions;

public sealed record MoveCharacter: ISceneInstructions
{
    private string CharacterId { get; }
    private int? X { get; }
    private int? Y { get; }
    private bool? FlippedHorizontally { get; }

    public MoveCharacter(Character character, int? x = null, int? y = null, bool? flippedHorizontally = null)
    {
        CharacterId = character.Id;
        X = x;
        Y = y;
        FlippedHorizontally = flippedHorizontally;
    }

    public MoveCharacter(string characterId, int? x = null, int? y = null, bool? flippedHorizontally = null)
    {
        CharacterId = characterId;
        X = x;
        Y = y;
        FlippedHorizontally = flippedHorizontally;
    }

    public void Execute(ISceneController sceneController)
    {
        if (X.HasValue) sceneController.Characters[CharacterId].BasePosition.X = X.Value;
        if (Y.HasValue) sceneController.Characters[CharacterId].BasePosition.Y = Y.Value;
        if (FlippedHorizontally.HasValue) sceneController.Characters[CharacterId].BasePosition.FlippedHorizontally = FlippedHorizontally.Value;
    }
}

public static class MoveCharacterExtensions
{
    /// <summary>
    /// Move a character to a new position. They will move there instantly.
    /// </summary>
    /// <param name="storyStep"></param>
    /// <param name="character"></param>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="flippedHorizontally"></param>
    /// <returns></returns>
    public static StoryStep MoveCharacter(this StoryStep storyStep, Character character, int? x = null, int? y = null, bool? flippedHorizontally = null)
    {
        storyStep.Instructions.Add(new MoveCharacter(character, x, y, flippedHorizontally));
        return storyStep;
    }

    /// <summary>
    /// Move a character to a new position. They will move there instantly.
    /// </summary>
    /// <param name="storyStep"></param>
    /// <param name="characterId"></param>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="flippedHorizontally"></param>
    /// <returns></returns>
    public static StoryStep MoveCharacter(this StoryStep storyStep, string characterId, int? x = null, int? y = null, bool? flippedHorizontally = null)
    {
        storyStep.Instructions.Add(new MoveCharacter(characterId, x, y, flippedHorizontally));
        return storyStep;
    }
}
