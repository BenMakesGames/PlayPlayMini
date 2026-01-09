using BenMakesGames.PlayPlayMini.VN.Model.CharacterAnimations;

namespace BenMakesGames.PlayPlayMini.VN.Model.SceneInstructions;

public sealed record AddAnimations(Character Character, params IReadOnlyList<ICharacterAnimation> Animations): ISceneInstructions
{
    public void Execute(ISceneController sceneController)
    {
        sceneController.Characters[Character.Id].Animations.AddRange(Animations);
    }
}

public static class AddAnimationsExtensions
{
    public static StoryStep AddAnimations(this StoryStep storyStep, Character character, params IReadOnlyList<ICharacterAnimation> animations)
    {
        storyStep.Instructions.Add(new AddAnimations(character, animations));
        return storyStep;
    }
}
