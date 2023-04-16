# What Is It?

`PlayPlayMini` is an opinionated framework for making smallish games with `MonoGame`.

It provides a state engine with lifecycle events, a `GraphicsManager` that provides methods for easily drawing sprites & fonts with a variety of effects, and dependency injection using `Autofac`.

If you don't know what all of those things are, don't worry: they're awesome, and this readme will show you how to use them (with code examples!), and explain their benefits.

[![Buy Me a Coffee at ko-fi.com](https://raw.githubusercontent.com/BenMakesGames/AssetsForNuGet/main/buymeacoffee.png)](https://ko-fi.com/A0A12KQ16)

## What Does "Opinionated" Mean?

It means the framework has some hard requirements about how you architect your code. For example, `PlayPlayMini`  doesn't just provide a state engine, it DEMANDS that you use it.

Some examples you may have run into:

| Type        | Opinionated      | Unopinionated |
| ----------- | ---------------- | ------------- |
| HTTP server | ASP.NET, Symfony | Express       |
| web client  | Angular, Vue     | React, jQuery |

If you've never used opinionated frameworks before, it can sometimes feel like they're getting in your way until you understand how they work, and why they're doing what they're doing. When an opinionated framework feels this way, it's often because it has already solved the problem you're trying to solve, but you just haven't yet learned the framework's solution.

There are pros and cons to using opinionated frameworks:

Opinionated frameworks are lame when:
* You already have a strong understanding of the underlying technologies (`MonoGame`, `Autofac`, etc), and don't need someone to tell you how to structure your application.
* You've made games with `MonoGame` before, and can easily copy-paste/modify old projects into new games. (You already have an opinionated framework: one you wrote yourself!)
* You've coded a lot, but never worked with an opinionated framework before. For a while, you'll probably find yourself butting heads with the framework, screaming "just let me do this how I always do it!"

Opinionated frameworks are great when:
* You, or others on your team, aren't aware of best-practices and design patterns, such as dependency injection. The framework is already using these things, and tries its best to make using them easy and automatic!
* You're writing something quick, and don't want to waste time implementing basic features like a state engine, font sheets, zoom levels for chunky pixels, etc.
* You've never written something with the underlying technologies before (in this case, `MonoGame`). A good opinionated framework makes it easy to use the best features of its underlying technology.

Is `PlayPlayMini` a *good* opinionated framework? I can only say that I've tried my best to make it one!

# How to Use

## Create a New `MonoGame` Project

1. Create a new `MonoGame` project using the `dotnet` CLI, or Visual Studio template.
2. Add the `BenMakesGames.PlayPlayMini` NuGet package.
3. Delete the default `Game1.cs`. We don't need or want it.
4. Create your first game state (more on this in a sec)
5. Rewrite the default `Program.cs` (more on this later)

(PlayPlayMini project templates are also available - https://github.com/BenMakesGames/PlayPlayMiniTemplates - but for the purposes of this readme, we're gonna build things a little more from scratch!)

## Game States

A "game state" is something like "the title menu", "exploring a town", "lock-picking mini-game", etc.

In `PlayPlayMini`, you are always in at least one game state. Let's make one:

```c#
// you don't HAVE to put your GameStates in their own folder/namespace, but it helps keep things tidy:
namespace MyGame.GameStates;

// it's common to have a game state for starting up your game. later, when you're loading TONS of
// assets, you can show some loading animation here to keep your users entertained while they wait.
// GameState is required for any game state class. bonus: it's recommended to seal most classes,
// for better performance (search the internet to learn more about this).
sealed class Startup : GameState
{
    private MouseManager Mouse { get; }
    private GameStateManager Game { get; }

    // if you're not familiar with "dependency injection", don't worry about the details too much,
    // but basically, if something in the game "wants" a class, like the MouseManager here, you
    // simply it as an arguments in the constructor, and it magically gets them. YOU'LL never
    // write "new Startup(...)"; it's all handled automatically by PlayPlayMini (via Autofac).
    public Startup(MouseManager mouse, GameStateManager game)
    {
        Mouse = mouse;
        Game = game;
    }

    // Enter() is automatically called when the player first enters this game state
    public override void Enter()
    {
        // the MouseManager requires some setup before it can be used for the first time.
        // this is a great opportunity to do it. this assumes you've actually loaded an image
        // called "Cursor"; more on how this is done, later!
        Mouse.UseCustomCursor("Cursor", (3, 1));

        // we have nothing to wait for, so let's just GET STARTED by going to the TitleMenu
        Game.ChangeState<TitleMenu>();
    }

    // the following methods are lifecycle events provided by GameState. let's go over them:

    // the "Active" methods are only called when this game state is "in the foreground". If
    // the player is in multiple game states at once, only one is "active". this is useful, for
    // example, if you have a "Pause" game state, and want to see the previous game state behind
    // it. there's an example of how to do this, later.
        
    public override void ActiveDraw(GameTime gameTime)
    {
        // in my experience, ActiveDraw doesn't get used much, EXCEPT: this is definitely
        // where you should draw any input cursors you may be using, like the mouse cursor:

        Mouse.Draw(gameTime);
    }

    public override void ActiveUpdate(GameTime gameTime)
    {
        // ActiveUpdate is where you'll put most of your logic for most of your game states.
    }

    public override void ActiveInput(GameTime gameTime)
    {
        // this is where you'll get mouse and/or keyboard input.
    }

    // the "Always" methods are always called for the current/active game state. later on,
    // there's an example of how to call them for "background" game states.
        
    public override void AlwaysDraw(GameTime gameTime)
    {
        // most of your drawing logic will go here.
    }

    public override void AlwaysUpdate(GameTime gameTime)
    {
        // if you've got animated things, like maybe animated water tiles in a tile-based RPG,
        // you might want to keep those animations going even while the state is "inactive",
        // for example while a pause screen is up. AlwaysUpdate is the place to put such
        // logic.
    }

    // there is no "AlwaysInput".
}
```

There's a few things to unpack in that example:

First of all, if you understand what all the methods in `GameState` are for, then you're probably starting to get a sense for how your application will be structured: a series of game states, which the player will move between according to some logic.

Second, you may have noticed:

```c#
Game.ChangeState<NEW_GAME_STATE_CLASS_NAME_HERE>();
```

The `GameStateManager`'s `ChangeState` method is how you'll be moving between game states. There are a couple ways to call it; more on these later.

Third: that whole "dependency injection" thing. What's that about?

## A Brief Aside: Dependency Injection

Feel free to skip/skim this section if you're like "yes, I know all about IoC/DI. I even know all the acronyms."

I mentioned before that you'll never call `new SomeGameState(...)`, which may feel weird to you. This is one of the main features of "dependency injection", provided by the library `Autofac`, which `PlayPlayMini` uses under the hood.

For example, this line:

```
public Startup(MouseManager mouse, GameStateManager game)
```

Here, the `Startup` class is saying "I want a `MouseManager`, and I want a `GameStateManager`"; ordinarily, you'd have to provide those when you call `new Startup(...)`, but again, with dependency injection, you never write "new Startup(...)"!

But if you never write `new Startup(...)`, how does one ever get made?!

Hold that thought. Let's take a moment to look at some of the advantages of never writing `new`.

### Advantages of Dependency Injection Frameworks

First: as your game states grow in number and complexity, you'll want to give them more and more
"services" like the `GraphicsManager`, `FrameCounter`, and others you make yourself. If you were
`new`ing up the game states "manually", then every time you added a new service to a constructor,
you'd have to find all the places you made a `new` one, and give them the new things they need.

```c#
new Startup(new MouseManager(...???), new GameStateManager(?!!?!?));
```

If you've ever found yourself doing that, a DI framework can help.

Second: if you `new` up a game state manually, and it needs a `MouseManager`, you'd also have to create a `new MouseManager()` for it... but what if the `MouseManager`'s constructor also has arguments? Now you'll have to `new` up those, too! Again: with a DI framework, you never write `new`, so you don't have to do any of this!

Third: Many services, like the example `MouseManager`, you really only need one of. You want to use the same instance over and over. You can write global statics (more on why these are bad in a moment), but Dependency Injection frameworks, like `Autofac`, can be configured to find an existing instance of a service class, and use that instead of making a new one, and you don't have to write any extra code to get that functionality.

Fourth: no more global static singletons, or hand-writing lazy instantiation logic! As a project grows, if you make a lot of global statics, it takes great care to prevent your application from becoming less efficient, with longer startup times and higher
memory usage, and it becomes harder to maintain: bugs creep in more easily and more often. Of course, no one library can instantly solve all those problems, but dependency injection frameworks solve a good chunk of them! If you've ever
written an application brimming with globals, and found yourself spending a lot of time fixing bugs with them, DI frameworks are here to make your life easier!

So how do you create a new service without writing `new`? Search the interwebs if you want more details, but to put it simply: the dependency injection framework just does it for you. If `Startup` asks for a `MouseManager`, then the DI framework gives it one. To do so, those classes just have to be registered with the DI framework. (More on how to register classes, later!)

### Learn More
* https://www.google.com/search?q=advantages+of+dependency+injection
* https://autofac.org, the library `PlayPlayMini` uses under the hood
  * It's not currently possible to choose your own IoC library. `PlayPlayMini` uses some features which are not available in all IoC libraries.

## Rewriting `Program.cs`

Alright, so you've made a game state - maybe more; maybe you've even put some logic in - but how do you tell `MonoGame` which game state to start up with?

For that, open up the default `Program.cs`. You need to completely rewrite it... but don't worry: it's pretty easy!

```c#
using BenMakesGames.PlayPlayMini; // don't forget this part!
using MyGame.GameStates; // assuming you put your game state classes here
using System;

// with PlayPlayMini, we use the GameStateManagerBuilder to get things started.
var gsmBuilder = new GameStateManagerBuilder();

gsmBuilder
    .SetInitialGameState<Startup>() // define the starting game state
    .SetWindowSize(480, 270, 2) // 480x270, with a x2 zoom level (window will be 960x540)
    .AddAssets(new IAsset[]
    {
        // immediately loaded
        new PictureMeta("Loading", "Graphics/Loading", true),
        new PictureMeta("Cursor", "Graphics/Cursor", true),

        // deferred
        new PictureMeta("Terrain", "Graphics/Terrain"),
        new PictureMeta("Title", "Graphics/Title"),
        new PictureMeta("TitleBackground", "Graphics/TitleBackground"),
        new SpriteSheetMeta("Treasure", "Graphics/Treasure", 16, 16),
        new SpriteSheetMeta("TerrainTrim", "Graphics/TerrainTrim", 10, 10),
        new FontMeta("Font", "Graphics/Font", 6, 8)
    })
;

// once we're done configuring, we're ready to go!
gsmBuilder.Run();
```

Taking it one step at a time:

```
var gsmBuilder = new GameStateManagerBuilder();
```

We talked about all that fancy dependency injection stuff, but here we are, `new`ing something up manually!

The dependency injection system has to get started somehow. That's actually one of the jobs of the
`GameStateManagerBuilder`.

(Also, if you're new to DI, only objects filled with "business logic" get registered with DI. Data-only objects like `PictureMeta` should still be `new`ed up manually, and should never ask for a service in their constructor.)

```c#
.SetInitialGameState<Startup>() // define the starting game state
.SetWindowSize(480, 270, 2) // 480x270, with a x2 zoom level (window will be 960x540)
```

Hopefully those are pretty self-explanatory. The final `2` in `SetWindowSize` indicates that all pixels should actually be drawn as 2x2 pixels. Under the hood, `PlayPlayMini` upscales your graphics, yielding a chunky pixel look! Set the zoom level to `1` if don't want chunky pixels!

Next up:

```c#
.AddAssets(new IAsset[] {
    ...
})
```

This method tells the `GraphicsManager` (and `SoundManager`) which assets to load, from your `Content/Content.mcgb` file. `Content/Content.mcgb` is part of `MonoGame`'s asset "pipeline". `PlayPlayMini` hides a lot of `MonoGame`'s internals, but the asset pipeline isn't something that can be - or should be - hidden! It's how you tell `MonoGame` what graphics, sounds, and songs, your game will use.

If you've never used the `Content/Content.mgcb` file before, check out `MonoGame`'s documentation on the subject:

* https://docs.monogame.net/articles/content/using_mgcb_editor.html

It's a super-useful tool!

Moving on:

```c#
// immediately loaded
new PictureMeta("Loading", "Graphics/Loading", true),
new PictureMeta("Cursor", "Graphics/Cursor", true),

// deferred
new PictureMeta("Terrain", "Graphics/Terrain"),
```

`PictureMeta` (along with `SpriteSheetMeta` and `FontMeta`) is a struct that contains everything the
`GameManager` service needs to load and store graphics.

The first argument is the name/key/ID/whatever-you-wanna-call-it which you're assigning to the
image. It can be anything, and spaces and other punctuation are allowed, if you want/need them
(it's just a string, after all!) You'll refer to this later, when drawing images.

The second argument is a path to the image, matching your `Content/Content.mgcb` file's definition of
the image.

The third, optional argument tells the `GraphicsManager` whether it needs to load the image up before
doing ANYTHING else. The default is to load the images in the background while the game continues
to run.

From the example above, we're saying that the "Loading" and "Cursor" graphics needs to be available
immediately (presumably to be displayed on a loading screen), while the other images can load in
later.

#### Checking if All Graphics have been Loaded

If your project has any deferred images, you need to make sure to wait for them to be loaded before continuing.

Let's modify the `Startup` game state class from before to check for this, and move on to the title menu only once all the graphics are ready!

```c#
namespace MyGame.GameStates;

sealed class Startup : GameState
{
    private MouseManager Mouse { get; }
    private GameStateManager Game { get; }
    private GraphicsManager Graphics { get; }

    // the GraphicsManager knows whether or not it's loaded everything up. BEHOLD THE POWER OF
    // DEPENDENCY INJECTION: we decided we need the GraphicsManager, so we just ask for it.
    public Startup(MouseManager mouse, GameStateManager game, GraphicsManager graphics)
    {
        Mouse = mouse;
        Game = game;
        Graphics = graphics;
    }

    public override void Enter()
    {
        Mouse.UseCustomCursor("Cursor", (3, 1));

        // we shouldn't do this immediately anymore!
        //Game.ChangeState<TitleMenu>();
    }

    public override void ActiveDraw(GameTime gameTime)
    {
        // since we have the GraphicsManager, let's ask it to draw the "Loading" image.
        // I'm putting it in the very upper-left - 0, 0 - which is kinda' boring. Sorry.
        GraphicsManager.DrawSprite(GraphicsManager.Pictures["Loading"], 0, 0);
            
        Mouse.Draw(gameTime);
    }

    public override void ActiveUpdate(GameTime gameTime)
    {
        // okay! let's check if the graphics are all loaded, and move on only when they are!
        if(Graphics.FullyLoaded)
            Game.ChangeState<TitleMenu>();
    }

    // the rest of the lifecycle methods can be deleted; we're not using them!
}
```

It's important to note: if you try to use a graphic before it's loaded, your application will crash, so don't do it!

If you also have deferred sound effect or music assets, inject the `SoundManager`, and check on _its_ `FullyLoaded` property, as well!

## Services

If you're familiar with DI, you already know this, but you can create your own services. A service is just any class that's been registered with the DI framework (`Autofac`, in our case). Suppose you make a `ParticleEffectService` class... once your register it as a service, you can ask for a `ParticleEffectService` in the constructor of any other class, and you can ask for other services in the constructor of your `ParticleEffectService`.

See "Creating Your Own Services" below for more info, as well as tips on how to avoid "circular dependencies" (instances where two services request one another in their constructors!)

For now, here are the service built into `BenMakesGames.PlayPlayMini`:

### Built-in Services

#### GameStateManager

The `GameStateManager` is needed to change the game's state, for example to transition from your title menu to your load screen, etc.

Most of your game states will probably include the `GameStateManager` as one of their dependencies.

#### GraphicsManager

The `GraphicsManager` has methods for drawing graphics and fonts.

Most of your game states will probably include the `GraphicsManager` as one of their dependencies.

#### SoundManager

The `SoundManager` has methods for playing sounds and looping music.

It uses `MonoGame`'s built-in sound library, which has some limitations, and even some audible bugs on some platforms (such as poor looping of music tracks on Windows).

If/when you get your game to a good state, and you really want to upgrade your game's sound and music, I recommend finding a C# FMOD library, like https://github.com/Martenfur/ChaiFoxes.FMODAudio. You'll have to hook it manually; I recommend creating your own sound manager service to wrap it up!

I'm working on an "official" FMOD package for `PlayPlayMini`, but don't currently have an ETA. If you beat me to the punch, let others (and me) know!

#### KeyboardManager

You can use still `MonoGame`'s `Keyboard` class directly; the `KeyboardManager` provides some additional features, like checking whether or not a particular key was JUST pressed (without having to write checks for that yourself).

Whether or not you use this class really depends on the kind of game you're making, and whether or not you want to/need to write your own keyboard controls.

#### MouseManager

You can use still `MonoGame`'s `Mouse` class directly; the `MouseManager` provides some additional features, including a method for drawing a custom cursor, and disabling the mouse when there's keyboard activity.

Whether or not you use this class really depends on the kind of game you're making, and whether or not you want to/need to write your own mouse controls.

If you're using the `BenMakesGames.PlayPlayMini.UI` extension package, the `MouseManager` becomes a requirement.

#### FrameCounter

The `FrameCounter` counts FPS, and some other stats. Use it if you want to add an FPS indicator on the screen.

### Creating Your Own Service

Once you've created your class, there are two ways you can register it with `Autofac` + `PlayPlayMini`:

#### Registering a Service Manually, in Program.cs

One of the methods you can call on the `GameStateManagerBuilder` is `AddServices`. If there isn't already a call to it, add one; it would look something like this:

```C#
gsmBuilder
	... // loading graphics, etc
    .AddServices(s => {
        s.RegisterType<MyService>();
        s.RegisterType<SomeOtherService>();
    })
    ... // loading graphics, etc
;
```

If `RegisterType` doesn't seem to be available, add `using AutoFac;` to the top of the file. Your IDE should be nice and suggest this for you.

You can also use this method to register classes you didn't write! For example, `PlayPlayMini` already registers `Random` as a service, so any of your services (or game states) can ask for a `Random` in their constructor.

For more info on how to register services, check the `Autofac` documentation: https://autofac.org

#### Registering a Service Automatically, Using the `AutoRegister` Attribute

`PlayPlayMini` provides an attribute called  `AutoRegister` which you can attach to a class to register that class with `Autofac`. When the game starts up, it searches for all classes using this attribute, and registers them for you! This attribute doesn't provide all of the options available when registering manually, but the vast majority of services don't use these options.

If you don't need to do anything fancy with your service registration, just add the `AutoRegister` attribute. It requires a `Lifetime` argument, and optionally allows an `InstanceOf` property.

As an example of the most basic use case:

```c#
[AutoRegister(Lifetime.Singleton)]
sealed class MyService
{
	...
}
```

Possible lifetimes are:

* `Lifetime.Singleton`
  * When a service uses this lifetime, then only one instance of the service class will ever be created. Any and all services requesting a singleton service will receive the same instance.
  * This is probably the behavior you want for most of your services.
* `Lifetime.PerDependency`
  * When a service uses this lifetime, then a new instance of the service class is created each time the service is requested.
  * This is useful for services which contain internal state that you want reset with every use. For example, game states are registered this way: every time you ask for a game state, you get a new copy of that game state.
  * Only use this lifetime if you're sure you need it; otherwise, use `Lifetime.Singleton`.

##### How and When to Use `InstanceOf`

Here's a rough example showing how you would use the `InstanceOf` property of `AutoRegister`:

```C#
// first, an interface
interface IRandomNumberGenerator
{
    int RandomInt(int inclusiveMin, int inclusiveMax);
}

// the class which implements that interface:
[AutoRegister(Lifetime.Singleton, InstanceOf = typeof(IRandomNumberGenerator))]
sealed class MyRNG: IRandomNumberGenerator
{
	public int RandomInt(int inclusiveMin, int inclusiveMax)
    {
    	...
    }
}

// this game state wants some kind of IRandomNumberGenerator; it doesn't know
// (or care) that it will get an instance of MyRNG:
sealed class SomeGameState: GameState
{
    private IRandomNumberGenerator RNG { get; }
    
    public SomeGameState(IRandomNumberGenerator rng)
    {
    	RNG = rng;
    }
    
    public bool LandedHit(int percentChanceToHit)
    {
    	return RNG.RandomInt(1, 100) <= percentChanceToHit;
    		return false;
    }
}
```

You could easily do without the `IRandomNumberGenerator` interface this example, so why/when should you go through the trouble? There are a few reasons, including:

* When your service implements an interface provided by a third-party library.
* When writing unit tests, interfaces can be mocked (for example, with the `NSubstitute` library). If you decide to write unit tests, you will need to add interfaces for the services which you want to mock.

### Dealing with Circular Dependencies

A circular dependency is when two services refer to one another in their constructors. Ex:

```C#
[AutoRegister(Lifetime.Singleton)]
sealed class ServiceA
{
    public ServiceA(ServiceB b)
    {
        ...
    }
}

[AutoRegister(Lifetime.Singleton)]
sealed class ServiceB
{
    public ServiceB(ServiceA a)
    {
        ...
    }
}
```

If you write code like this, `Autofac` will complain (and crash) when it tries to instantiate a service that requests either `ServiceA` or `ServiceB`.

This can be resolved in two ways:

1. Create a new service, `ServiceC`, that services A and B depend on, instead of depending on one another. Move the method or methods needed into `ServiceC`, and update your code accordingly.
2. Register either `ServiceA` or `ServiceB` as a lazy service. There are a few ways to do this; one is to use this NuGet package: https://github.com/servicetitan/lazy-proxy-autofac
   1. When registering services as lazy services, you'll have to manually register them; you can't use `AutoRegister` for quick registration.

### Advanced Services Configuration: Service Lifetime Events

There are several interfaces which services can implement, each allowing the service to hook into different processes within `PlayPlayMini`. A service may implement any combination of this interfaces, or none at all. They are:

* `IServiceDraw`
  * The service class must implement a `Draw` method, which will be called *after* the current state's `AlwaysDraw` and `ActiveDraw` events have been called.
  * An example service which implements this interface is the built-in `FrameCounter` service, which displays the FPS on-screen.
* `IServiceInitialize`
  * The service class must implement an `Initialize` method, which will be called at the beginning of the `MonoGame` Initialize state, before `MonoGame`'s `LoadContent` method is called.
* `IServiceInput`
  * The service class must implement an `Input` method, which will be called *before* the current state's `Input` event.
  * Example services which implements this interface are the built-in `KeyboardManager` and `MouseManager`.
* `IServiceLoadContent`
  * The service class must implement a `LoadContent` method, and `FullyLoaded` getter. The `LoadContent` method is called in `MonoGame`'s `LoadContent` method. It's up to the service class's author to implement `FullyLoaded` accurately (probably as `public bool FullyLoaded { get; private set; }`, setting it to true when `LoadContent` has completed).
* `IServiceUpdate`
  * The service class must implement an `Update` method, which will be called before the current state's `AlwaysUpdate` and `ActiveUpdate` methods.

## More Game State Stuff

### Multiple States at Once

Here's how you can draw two states at once, for example, to show a pause screen on top of another game state:

```c#
public sealed class PauseScreen: GameState
{
	public GameState PreviousState { get; }
    
    ...
    
    public PauseScreen(..., GameState previousState)
    {
    	...
    	
    	PreviousState = previousState;
    }
    
    public override void AlwaysDraw(GameTime gameTime)
    {
    	PreviousState.AlwaysDraw(gameTime);
    	
    	// rest of drawing logic here, for example:
    	GraphicsManager.DrawFilledRectangle(0, 0, GraphicsManager.Width, GraphicsManager.Height, new Color(0, 0, 0, 0.8));
    	
    	GraphicsManager.DrawPicture("Paused", 100, 200);
    }
    
    public override void AlwaysUpdate(GameTime gameTime)
    {
    	PreviousState.AlwaysUpdate(gameTime);
    	
    	// pause screen's own update logic goes here, if any
    }
    
	public override void ActiveInput(GameTime gameTime)
    {
        // press Escape to un-pause
    	if(KeyboardManager.PressedKey(Keys.Escape))
        {
            // we have a reference to a GameState, so we can invoke ChangeState this way:
        	GameStateManager.ChangeState(PreviousState);
        }
    }
    
    ...
}
```

The pause screen would then be opened like this:

```C#
public sealed class CombatEncounter: GameState
{
	...
	
	public override void ActiveInput(GameTime gameTime)
    {
    	...
    	
    	if(KeyboardManager.PressedKey(Keys.Escape))
        {
        	GameStateManager.ChangeState<PauseScreen, GameState>(this);
        }
    }
    
    ...
}
```

By the way, this `ChangeState` method can be used to pass more complex parameters to a game state, too. For example, assuming you have some `PauseScreenConfig` class, you might write:

```c#
GameStateManager.ChangeState<PauseScreen, PauseScreenConfig>(new PauseScreenConfig() {
	PreviousState = this,
	BackgroundOpacity = 0.5f,
	PausePicture = "Pause",
});
```

### Game State Lifecycle Events Summary

A game state's lifecycle event methods are called, in this order.

1. `Enter`
   * Called when the game state is entered into.
   * Do any first-time setup needed for the game state, ex: initializing a star field array.
3. `ActiveInput`
   * Always called for the current game state.
   * Handle user input here. Ex: see that the player is pressing up, and set a property on the player indicating that they'd like to move up.
3. `AlwaysUpdate`
   * Always called for the current game state.
   * Do decorative logic here, such as updating passive particle effects and animations (ex: animated water tiles in a tile-based game).
   * If you have two states running simultaneously, the active state's `AlwaysUpdate` should call the background state's `AlwaysUpdate`. You will have to do this manually.
4. `ActiveUpdate`
   * Always called for the current game state.
   * Do game logic here, such as moving the player (or not, if they're blocked), enemies, etc.
6. `AlwaysDraw`
   * Always called for the current game state.
   * Do main drawing logic here: backgrounds, player, enemies, particles, etc.
   * If you have two states running simultaneously, the active state's `AlwaysDraw` should call the background state's `AlwaysDraw` first thing! You will have to do this manually.
6. `ActiveDraw`
   * Always called for the current game state.
   * Draw things you only want drawn if the game is the foreground state, for example the mouse cursor (`MouseManager`'s `ActiveDraw`).
7. `Leave`
   - Called if a new game state is replacing the current game state.
   - Immediately after `Leave` is called, the current game state is changed to be the next game state.
   - I haven't yet found a use for this method; it's included for completeness/just-in-case.