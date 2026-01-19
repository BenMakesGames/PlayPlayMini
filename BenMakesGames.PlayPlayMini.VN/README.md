# VN

**PlayPlayMini.VN** adds a visual novel engine to the PlayPlayMini framework, which is itself built on top of MonoGame.

You can use **PlayPlayMini.VN** it to make a 100% VN game, or to add visual novel sequences within other games. 

Some basic understanding of PlayPlayMini will be required.

> [🧚 **Hey, listen!** You can support my development of open-source software on Patreon](https://www.patreon.com/BenMakesGames)

## Installing

Add the `BenMakesGames.PlayPlayMini.VN` package to your project.

## Configuring

### `.AddVN()`

Call `.AddVN()` during the `GameStateManagerBuilder` setup:

```c#
var gsmBuilder = new GameStateManagerBuilder();

gsmBuilder
    ...
    .AddVN()
;

```

### Change `VNSettings` properties

There is a `VNSettings` object which contains several properties that can be set to configure the visual novel engine, such as colors and fonts. For example:

```c#
VNSettings.DialogSpeakingBackgroundColor = Color.Ivory;
VNSettings.DialogSpeakingTextColor = Color.Navy;
```

You can set these properties in your `Program.cs`, your game's startup game state... wherever feels appropriate to you.

While you _can_ change these settings in the middle of a visual novel scene, not all changes will be immediately applied (including font changes). This may be improved with future releases of **PlayPlayMini.VN**.

### Load at least one font

`VNSettings.DialogFont` must name a font that has been loaded by PlayPlayMini. By default, it looks for a font called "Font".

## Creating a visual novel sequence

Visual novel sequences are written in C#, using classes and methods from this package.

Here's an example of how you might organize a visual novel sequence in the code:

```c#
public static class Intro
{
    public static List<StoryStep> GetStory()
    {
        var player = new Character()
        {
            Id = "player", // a unique ID - this will never be seen in-game
            Name = "Nina", // the name of the character, shown when they speak
            SpriteSheet = "Characters/Nina", // the name of a PlayPlayMini-loaded sprite sheet
            SpeakingColor = Color.Blue, // the color of the character's name and dialog border
            Expressions = new() // a dictionary of expressions; the number is the sprite index
            {
                ["neutral"] = 0,
                ["surprised"] = 1,
                ["doubtful"] = 2,
                // you can make up any names you want:
                ["holding a sword"] = 3,
                ["wearing a sweater, smiling"] = 4,
                // and each character can have a totally different set of expressions - up to you!
            }
        };

        return
        [
            new StoryStep()
                .ControlSound(s => s.StopMusic())
                .SetCharacterDialog("Hm...", player)
                .AddCharacter(player, 16, 0), // 16, 0 represent the X, Y coordinate to place the character

            new StoryStep()
                .SetBackgroundPicture("Background/OuterSpace") // a PlayPlayMini-loaded picture
                .SetCharacterDialog("Oh!", player)
                .SetExpression(player, "surprised")
                .AddAnimations(player, new Bounce()), // there are several animations available, and you can make your own!

            new StoryStep()
                .SetExpression(player, "neutral")
                .SetThinkingDialog($"Yes. {player.Name} probably wasn't expecting to be suddenly transported into space. It's better than a black void, though, at least."),

            new StoryStep()
                .SetExpression(player, "doubtful")
                .SetCharacterDialog("Is it, though?", player)
        ];
    }
}
```

Creating the same character over and over again is a bit of a pain; you might prefer to do something like this:

```c#
public static class Intro
{
    public static List<StoryStep> GetStory(MyGameWorldClass worldState) // MyGameWorldClass is a class YOU write!
    {
        var character = worldState.GetPlayerCharacter(); // GetPlayerCharacter is a method YOU write!
        
        ...
    }
}
```

## Starting a visual novel sequence

Begin a visual sequence by calling `GameStateManager.PlayScene(...)`, for example:

```c#
var steps = Intro.GetStory(currentGame); // the method from the previous example

GameStateManager.PlayScene(steps, () => {
    // code to run when scene is complete. like, I dunno:
    GameStateManager.ChangeState<ExploringWorldMap>();
});
```

## And that's the basics!

Of course, there are many more things you can do with story steps, including playing sounds, triggering animations, etc. Check the full documentation for more!
