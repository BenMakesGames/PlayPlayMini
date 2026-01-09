using Autofac;
using BenMakesGames.PlayPlayMini.Services;

namespace BenMakesGames.PlayPlayMini.VN.Model.SceneInstructions;

public sealed record GenericAction(Action Action) : ISceneInstructions
{
    public void Execute(ISceneController sceneController) => Action();
}

public sealed record GenericAction<T>(Action<T> Action) : ISceneInstructions where T: notnull
{
    public void Execute(ISceneController sceneController) => Action(sceneController.IoCContainer.Resolve<T>());
}

public static class GenericActionExtensions
{
    public static StoryStep Action(this StoryStep storyStep, Action action)
    {
        storyStep.Instructions.Add(new GenericAction(action));
        return storyStep;
    }

    public static StoryStep Action<T>(this StoryStep storyStep, Action<T> action) where T: notnull
    {
        storyStep.Instructions.Add(new GenericAction<T>(action));
        return storyStep;
    }

    public static StoryStep ControlSound(this StoryStep storyStep, Action<SoundManager> soundAction)
    {
        storyStep.Instructions.Add(new GenericAction<SoundManager>(soundAction));
        return storyStep;
    }
}
