using BenMakesGames.PlayPlayMini.VN.GameStates;
using BenMakesGames.PlayPlayMini.VN.Model;

namespace BenMakesGames.PlayPlayMini.VN.Extensions;

public static class GameStateManagerExtensions
{
    public static void PlayScene(
        this GameStateManager gameStateManager,
        List<StoryStep> storySteps,
        Action onComplete
    )
    {
        gameStateManager.ChangeState<PlayScene, PlaySceneConfig>(new(
            storySteps,
            onComplete
        ));
    }

    public static PlayScene CreateScene(
        this GameStateManager gameStateManager,
        List<StoryStep> storySteps,
        Action onComplete
    )
    {
        return gameStateManager.CreateState<PlayScene, PlaySceneConfig>(new(
            storySteps,
            onComplete
        ));
    }
}
