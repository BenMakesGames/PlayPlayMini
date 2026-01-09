namespace BenMakesGames.PlayPlayMini.VN.Model.SceneInstructions;

public sealed record SetExpression(Character Character, int SpriteIndex): ISceneInstructions
{
    public void Execute(ISceneController sceneController)
    {
        sceneController.Characters[Character.Id].BasePosition.SpriteIndex = SpriteIndex;
    }
}

public static class SetExpressionExtensions
{
    public static StoryStep SetExpression(this StoryStep storyStep, Character character, int spriteIndex)
    {
        storyStep.Instructions.Add(new SetExpression(character, spriteIndex));
        return storyStep;
    }

    public static StoryStep SetExpression(this StoryStep storyStep, Character character, string expression)
    {
        if(character.Expressions.TryGetValue(expression, out var characterExpression))
            storyStep.Instructions.Add(new SetExpression(character, characterExpression));

        return storyStep;
    }
}
