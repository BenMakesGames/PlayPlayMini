using BenMakesGames.PlayPlayMini.VN.Model.CharacterAnimations;

namespace BenMakesGames.PlayPlayMini.VN.Model.SceneInstructions;

public sealed record RemoveAnimations(Character Character, Func<ICharacterAnimation, bool> Predicate): ISceneInstructions
{
    public void Execute(ISceneController sceneController)
    {
        sceneController.Characters[Character.Id].Animations.RemoveAll(a => Predicate(a));
    }
}

public static class RemoveAnimationsExtensions
{
    public static StoryStep RemoveAnimations(this StoryStep storyStep, Character character, Func<ICharacterAnimation, bool> predicate)
    {
        storyStep.Instructions.Add(new RemoveAnimations(character, predicate));
        return storyStep;
    }

    public static StoryStep RemoveAnimations<TAnimation>(this StoryStep storyStep, Character character)
        => storyStep.RemoveAnimations(character, a => a is TAnimation);
}
