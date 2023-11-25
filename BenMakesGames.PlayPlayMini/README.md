# What Is It?

**PlayPlayMini** is an opinionated framework for making smallish 2D games with **MonoGame**.

It provides a state engine with lifecycle events, a `GraphicsManager` that provides methods for easily drawing sprites & fonts with a variety of effects, and dependency injection using **Autofac**.

If you don't know what all of those things are, don't worry: they're awesome, and this readme will show you how to use them (with code examples!), and explain their benefits.

If you prefer learning purely by example, check out [Block-break](https://github.com/BenMakesGames/BlockBreak), a demo game made with PlayPlayMini, EntityFramework, and Serliog that uses fonts, sprite sheets, pictures, and sounds.

[![Buy Me a Coffee at ko-fi.com](https://raw.githubusercontent.com/BenMakesGames/AssetsForNuGet/main/buymeacoffee.png)](https://ko-fi.com/A0A12KQ16)

# Upgrading from 3.x to 4.0.0

## Breaking Changes

1. Upgraded to .NET 8.0.
2. `FontMeta` has three new properties: `FirstCharacter`, `HorizontalSpacing`, and `VerticalSpacing`. These default to `' '`, `1`, and `1`, respectively. Previous behavior was equivalent to a horizontal and vertical spacing of `0`.
   * **Quick fix:** update your fonts' `FontMeta` to set the `HorizontalSpacing` and `VerticalSpacing` properties to `0`; OR:
   * **Better fix:** update your fonts' images to remove spacing between characters; update the `Width` and `Height` properties accordingly.
3. The `[Obsolete]` `Generators` methods and class for drawing primitive shapes (lines, circles) have been removed.
   * Use the `BenMakesGames.PlayPlayMini.GraphicsExtensions` package for drawing primitives. It's capable of drawing more shapes, and has _much_ better performance!

# Upgrading from 2.x to 3.0.0

## Breaking Changes

1. The lifecycle methods `ActiveInput`, `ActiveUpdate`, `AlwaysUpdate`, `ActiveDraw`, and `AlwaysDraw` have been removed from `GameState`, replaced with `Input`, `Update`, and `Draw`.
    * Use `GameStateManager.CurrentState` to check the current state, as needed.
2. The `GraphicsManager`'s many `[Obsolete]` methods have been removed.
3. `Random` is no longer registered as a singleton service automatically.
    * Use the C#-built-in `Random.Shared`, instead, OR:
    * Register `Random` manually in `AddServices` (if you actually use different implementations of an RNG, for example in unit tests).
4. Updated **Autofac** from 6.5.0 to 7.1.0
    * **PlayPlayMini** users will probably not experience any of the very few breaking changes introduced by this upgrade. See https://docs.autofac.org/en/latest/whats-new/upgradingfrom6to7.html for more information.

## Additions & Other Changes

1. `GameState` now has a `FixedUpdate` method, which is called ~60 times per second regardless of the game's frame rate.
    * If you're using `GameStateManagerBuilder`'s `UseFixedTimeStep(true)`, there is little practical difference between `FixedUpdate` and `Update`.
2. Specifying a lifetime value for the `[AutoRegister]` is now optional; the default is `Lifetime.Singleton`.
3. Added in 2.7.0, but: `GameStateManagerBuilder` now has a `.SetLostFocusGameState<T>()` method, which allows you to set a game state to be used when the game loses focus (for example, when the player alt-tabs out of the game).
    * This game state will receive a `LostFocusConfig` object in its constructor, containing a reference to the previous game state.
    * See the **PlayPlayMini** skeleton projects for examples.

# How to Use

## Create a New Project

1. Install the **PlayPlayMini** project templates:
   * `dotnet new install BenMakesGames.PlayPlayMini.Templates`
2. Optional: create an empty solution.
3. Create a new project:
   * `dotnet new playplaymini.skeleton -n MyGame`
   * Make sure to run this within the solution directory, if you created a solution in step 2.
4. If you created a solution, add the newly-created project to it!

## Understanding Game States

A "game state" is something like "the title menu", "exploring a town", "lock-picking mini-game", etc.

In **PlayPlayMini**, your game is always in at least one game state.

If you used one of the **PlayPlayMini** template projects, check out the `Startup` class in the `GameStates` folder. It should look like this:

```c#
namespace MyGame.GameStates;

// inheriting game states is a path that leads to madness, so always seal your game states!
public sealed class Startup: GameState
{
    private GraphicsManager Graphics { get; }
    private GameStateManager GSM { get; }
    private MouseManager Mouse { get; }

    public Startup(GraphicsManager graphics, GameStateManager gsm, MouseManager mouse)
    {
        Graphics = graphics;
        GSM = gsm;
        Mouse = mouse;

        Mouse.UseCustomCursor("Cursor", (3, 1));
    }

    // note: you do NOT need to call the `base.` for lifecycle methods. so save some CPU cycles,
    // and don't call them :P

    public override void Update(GameTime gameTime)
    {
        if (Graphics.FullyLoaded)
        {
            // TODO: go to title menu, once that exists; for now, just jump straight into the game:
            GSM.ChangeState<Playing>();
        }
    }

    public override void Draw(GameTime gameTime)
    {
        // TODO: draw loading screen

        // only draw the mouse once
        if(GSM.CurrentState == this)
            Mouse.ActiveDraw(gameTime);
    }
}
```

There's a few things to unpack in that example:

First: all `PlayPlayMini` game states must inherit the `GameState` class.

There are several lifecycle methods which a game state may optionally override:
* `Enter()`, called once, when the game state is entered into.
* `Input(...)`, called every frame. This is a good place to capture user input for later processing.
* `FixedUpdate(...)`, called 60 times per second, regardless of the game's framerate; useful for doing physics-based updates.
* `Update(...)` called every frame. Most game logic will probably go here.
* `Draw(...)`, called every frame. Place drawing logic here.
* `Leave()`, called once, when the game state is left.

The current game state can be changed using one of the `GameStateManager`'s `ChangeState` methods. The `Startup` game state uses this to start the game once all of the graphic assets have been loaded:

```c#
GSM.ChangeState<Playing>();
```

Finally, you may have noticed that the `Startup` game state has a constructor. You'll never call `new Startup(...)` anywhere, however! This is because **PlayPlayMini** uses a dependency injection framework called **Autofac** under the hood.

If you're not familiar with dependency injection, you may find yourself wanting to `new` up game states and `MouseManager`s and things. Stop right there! Don't do it! That path leads to madness. Read on to learn an easier way to manage a class's dependencies:

## A Brief Intro to Dependency Injection

Feel free to skip/skim this section if you're like "yes, I know all about IoC/DI. I even know all the acronyms."

Consider this line, from the above code:

```
public Startup(GraphicsManager graphics, GameStateManager gsm, MouseManager mouse)
```

Here, rather than the `Startup` class `new`ing up its own graphics manager, game state manager, or mouse, it asks those things to be given via its constructor. That's the main principle of dependency injection frameworks: "inversion of control". Rather than a class creating the things it needs, it gives up control of that task to something else.

You may wonder: "Why bother? I'll just have to provide them all when I call `new Startup(...)` anyway!"

However, with a dependency injection framework, you will never write `new Startup(...)`!

So how do the game states get made?

Hold that thought. Let's take a moment to look at some of the advantages of never writing `new`.

### Advantages of Dependency Injection Frameworks

First: as your game states grow in number and complexity, you'll want to give them more
"services" like the `GraphicsManager`, `FrameCounter`, and others you make yourself (perhaps a SQLite database connection to save and load te game).
If you were `new`ing up the game states "manually", then every time you added a new service to a constructor,
you'd have to find all the places you made a `new` one, and give them the new things they need.

```c#
new Startup(new MouseManager(...???), new GameStateManager(?!!?!?));
```

Second: if you `new` up a game state manually, and it needs a `MouseManager`, you'd also have to create a `new MouseManager()` for it... but what if the `MouseManager`'s constructor also has arguments? Now you'll have to `new` up those, too!

But again: with a DI framework, you don't have to `new` any of this stuff up. You're free to add more dependencies, as constructor arguments, without making any other changes across your codebase. Nice!

Third: Many services, like the `MouseManager`, you really only need one of, so you can use the same instance over and over. You _could_ write global statics, but if you've done that in a big project before, you know the trouble and performance problems that can lead to. Dependency injection frameworks, like **Autofac**, can be configured to find existing instances of certain service classes, and use those instead of making a new ones, with very little effort. (No need to write lazy-initalization logic, etc.)

So how do you create a new service without writing `new`? Search the interwebs if you want more details, but to put it simply: the dependency injection framework just does it for you. When you ask **PlayPlayMini** to change into the `Playing` class, **Autofac** creates it for you. And if `Playing` asks for a `MouseManager`, autofac creates that, too (or finds one that already exists).

In order for a dependency injection framework to do this, any classes you want it to create need to be registered with it. In **PlayPlayMini**, your game states, and many of the services you'll use, are registered automatically. You can also register your own services, if you want - more on this, later!

### Final Tips

1. Only objects containing "business logic" get registered with DI. Data-only objects like `PictureMeta` should still be `new`ed up manually, and should never ask for a service in their constructor.
2. If a method, or collection of methods, has no dependencies, and no internal state, don't make a service - a collection of `static` methods will be more-performant, easier to re-use (copy-paste into other projects), and easier to test.

### Learn More
* https://www.google.com/search?q=advantages+of+dependency+injection
* https://autofac.org, the library **PlayPlayMini** uses under the hood.
  * It's not currently possible to choose your own IoC library. **PlayPlayMini** uses some features which are not available in all IoC libraries.

## Configuring Your Game's Configuration: `Program.cs`

If you used one of the **PlayPlayMini** template projects, check out the `Program` class in the project root. Skipping the `using` statements, it should look like this:

```c#
var gsmBuilder = new GameStateManagerBuilder();

gsmBuilder
    .SetWindowSize(1920 / 4, 1080 / 4, 2)
    .SetInitialGameState<Startup>()
    .SetLostFocusGameState<LostFocus>()

    // TODO: set a better window title
    .SetWindowTitle("MyGame")

    // TODO: add any resources needed (refer to PlayPlayMini documentation for more info)
    .AddAssets(new IAsset[]
    {
        new FontMeta("Font", "Graphics/Font", 6, 8),
        new PictureMeta("Cursor", "Graphics/Cursor", true),

        // new FontMeta(...)
        // new PictureMeta(...)
        // new SpriteSheetMeta(...)
        // new SongMeta(...)
        // new SoundEffectMeta(...)
    })

    // TODO: any additional service registration (refer to PlayPlayMini and/or Autofac documentation for more info)
    .AddServices((s, c) => {

    })
;

gsmBuilder.Run();
```

Taking it one step at a time:

```
var gsmBuilder = new GameStateManagerBuilder();
```

If you've used ASP.NET Core before, this kind of startup logic should look pretty familiar.

The `GameStateManagerBuilder` is responsible for getting your game configured and started, including creating the dependency injection service container.

```c#
    .SetWindowSize(1920 / 4, 1080 / 4, 2)
    .SetInitialGameState<Startup>()
```

Hopefully those are pretty self-explanatory. The final `2` in `SetWindowSize` indicates that all pixels should actually be drawn as 2x2 pixels. Under the hood, **PlayPlayMini** upscales your graphics, yielding a chunky pixel look! Set the zoom level to `1` if don't want chunky pixels!

Next:

```c#
    .SetLostFocusGameState<LostFocus>()
```

This is optional; it configured a game state to be used when your game loses focus. If you don't want this feature, you can delete this line, and the `LostFocus` class that comes with the template.

Next up:

```c#
    .AddAssets(new IAsset[] {
        ...
    })
```

This method tells the `GraphicsManager` (and `SoundManager`) which assets to load, from your `Content/Content.mcgb` file. `Content/Content.mcgb` is part of **MonoGame**'s asset "pipeline". **PlayPlayMini** hides a lot of **MonoGame**'s internals, but the asset pipeline isn't something that can be - or should be - hidden! It's how you tell **MonoGame** what graphics, sounds, and songs, your game will use.

If you've never used the `Content/Content.mgcb` file before, check out **MonoGame**'s documentation on the subject:

* https://docs.monogame.net/articles/content/using_mgcb_editor.html

It's a super-useful tool!

Moving on:

```c#
        new FontMeta("Font", "Graphics/Font", 6, 8),
        new PictureMeta("Cursor", "Graphics/Cursor", true),
```

`FontMeta` (along with `PictureMeta` and `SpriteSheetMeta`) is a struct that contains everything the
`GraphicsManager` needs to load and store graphics.

The first argument is the name/key/ID/whatever-you-wanna-call-it which you're assigning to the
image. It can be anything, and spaces and other punctuation are allowed, if you want/need them
(it's just a string, after all!) You'll refer to this later, when drawing images.

The second argument is a path to the image, matching your `Content/Content.mgcb` file's definition of
the image.

For fonts and sprite sheets, the dimensions of each individual sprite are specified in the third and fourth parameters.

Finally, an optional boolean specifies whether or not the asset needs to be loaded before the game's
startup state is entered. In the example above, this is set for the "Cursor" graphic, so that 
the mouse cursor can be shown while the rest of the assets are loading.

Unless every single one of your assets are set to load before the game's startup state (which is not recommended!), you'll need to wait for them to load before starting the rest of the game.

The template-provided `Startup` game state does this by checking the `GraphicsManager`'s `FullyLoaded` attribute, seen in an early code example.

If you also have deferred sound effect or music assets, inject the `SoundManager`, and check on _its_ `FullyLoaded` property, as well.

Remember: if you try to use an asset before it's loaded, your application will crash!

## Services

If you're familiar with DI, you already know this, but you can create your own services. A service is just any class that's been registered with the DI framework (**Autofac**, in our case). Suppose you make a `ParticleEffectService` class... once your register it as a service, you can ask for a `ParticleEffectService` in the constructor of any other class, and you can ask for other services in the constructor of your `ParticleEffectService`.

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

It uses **MonoGame**'s built-in sound library, which has some limitations, and even some audible bugs on some platforms (such as poor looping of music tracks on Windows).

If/when you get your game to a good state, and you really want to upgrade your game's sound and music, I recommend finding a C# FMOD library, like https://github.com/Martenfur/ChaiFoxes.FMODAudio. You'll have to hook it manually; I recommend creating your own sound manager service to wrap it up!

I'm working on an "official" FMOD package for **PlayPlayMini**, but don't currently have an ETA. If you beat me to the punch, let others (and me) know!

#### KeyboardManager

You can use still **MonoGame**'s `Keyboard` class directly; the `KeyboardManager` provides some additional features, like checking whether or not a particular key was JUST pressed (without having to write checks for that yourself).

Whether or not you use this class really depends on the kind of game you're making, and whether or not you want to/need to write your own keyboard controls.

#### MouseManager

You can use still **MonoGame**'s `Mouse` class directly; the `MouseManager` provides some additional features, including a method for drawing a custom cursor, and disabling the mouse when there's keyboard activity.

Whether or not you use this class really depends on the kind of game you're making, and whether or not you want to/need to write your own mouse controls.

If you're using the `BenMakesGames.PlayPlayMini.UI` extension package, the `MouseManager` becomes a requirement.

#### FrameCounter

The `FrameCounter` counts FPS, and some other stats. Use it if you want to add an FPS indicator on the screen.

### Creating Your Own Service

Once you've created your class, there are two ways you can register it with **Autofac** + **PlayPlayMini**:

#### Registering a Service Automatically, Using the `AutoRegister` Attribute

**PlayPlayMini** provides an attribute called `AutoRegister` which you can attach to a class to register that class with **Autofac**. When the game starts up, it searches for all classes using this attribute, and registers them for you! This attribute doesn't provide all of the options available when registering manually, but should suffice for the vast majority of use-cases.

A simple example, registering a singleton service without any interface (the most common case for most **PlayPlayMini** applications):

```c#
[AutoRegister]
sealed class MyService
{
	...
}
```

If needed, you can also register services with a per-dependency lifetime, and/or an interface:

```c#
[AutoRegister(Lifetime.PerDependency, InstanceOf = typeof(IMyService))]
sealed class MyService: IMyService
{
    ...
}
```

Possible lifetimes are:

* `Lifetime.Singleton` (default)
  * When a service uses this lifetime, then only one instance of the service class will ever be created. Any and all services requesting a singleton service will receive the same instance.
  * This is probably the behavior you want for most of your services.
* `Lifetime.PerDependency`
  * When a service uses this lifetime, then a new instance of the service class is created each time the service is requested.
  * This is useful for services which contain internal state that you want reset with every use. For example, game states are registered this way: every time you ask for a game state, you get a new copy of that game state.
  * Only use this lifetime if you're sure you need it; otherwise, use `Lifetime.Singleton`.
  * Remember that a singleton service cannot depend on a per-dependency service.

#### Registering a Service Manually, in Program.cs

One of the methods you can call on the `GameStateManagerBuilder` is `AddServices`. If there isn't already a call to it, add one; it would look something like this:

```c#
    .AddServices((s, c) => {
        s.RegisterType<MyService>();
        s.RegisterType<SomeOtherService>();
    })
```

If `RegisterType` doesn't seem to be available, add `using AutoFac;` to the top of the file. (Your IDE should be nice and suggest this for you.)

For more info on how to register services, check the **Autofac** documentation: https://autofac.org

#### Dealing with Circular Dependencies

A circular dependency is when two services refer to one another in their constructors. Ex:

```C#
[AutoRegister]
sealed class ServiceA
{
    public ServiceA(ServiceB b)
    {
        ...
    }
}

[AutoRegister]
sealed class ServiceB
{
    public ServiceB(ServiceA a)
    {
        ...
    }
}
```

If you write code like this, **Autofac** will complain (and crash) when it tries to instantiate a service that requests either `ServiceA` or `ServiceB`.

This can be resolved in two ways:

1. Create a new service, `ServiceC`, that services A and B depend on, instead of depending on one another. Move the method or methods needed into `ServiceC`, and update your code accordingly.
2. Register either `ServiceA` or `ServiceB` as a lazy service. There are a few ways to do this; one is to use this NuGet package: https://github.com/servicetitan/lazy-proxy-autofac
   * When registering services as lazy services, you'll have to manually register them; you can't use `[AutoRegister]` for quick registration.

#### Advanced Services Configuration: Service Lifetime Events

There are several interfaces which services can implement, each allowing the service to hook into different processes within **PlayPlayMini**. A service may implement any combination of these interfaces, or none at all. They are:

* `IServiceDraw`
  * The service class must implement a `Draw` method, which will be called *after* the current state's `Draw` methods have been called.
  * An example service which implements this interface is the built-in `FrameCounter` service, which displays the FPS on-screen.
* `IServiceInitialize`
  * The service class must implement an `Initialize` method, which will be called at the beginning of the **MonoGame** Initialize state, before **MonoGame**'s `LoadContent` method is called.
* `IServiceInput`
  * The service class must implement an `Input` method, which will be called *before* the current state's `Input` event.
  * Example services which implements this interface are the built-in `KeyboardManager` and `MouseManager`.
* `IServiceLoadContent`
  * The service class must implement a `LoadContent` method, and `FullyLoaded` getter. The `LoadContent` method is called in **MonoGame**'s `LoadContent` method. It's up to the service class's author to implement `FullyLoaded` accurately (probably as `public bool FullyLoaded { get; private set; }`, setting it to true when `LoadContent` has completed).
* `IServiceUpdate`
  * The service class must implement an `Update` method, which will be called before the current state's `Update` method.

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
    
    public override void Draw(GameTime gameTime)
    {
    	PreviousState.Draw(gameTime);
    	
    	// rest of drawing logic here, for example:
    	Graphics.DrawFilledRectangle(0, 0, GraphicsManager.Width, GraphicsManager.Height, new Color(0, 0, 0, 0.8));
    	
    	Graphics.DrawPicture("Paused", 100, 200);
    }
    
    public override void Update(GameTime gameTime)
    {
    	PreviousState.Update(gameTime);
    	
    	// pause screen's own update logic goes here, if any
    }
    
	public override void Input(GameTime gameTime)
    {
        // press Escape to un-pause
    	if(Keyboard.PressedKey(Keys.Escape))
        {
            // we have a reference to a GameState, so we can invoke ChangeState this way:
        	GSM.ChangeState(PreviousState);
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
	
	public override void Input(GameTime gameTime)
    {
    	...
    	
    	if(KeyboardManager.PressedKey(Keys.Escape))
        {
        	GSM.ChangeState<PauseScreen, GameState>(this);
        }
    }
    
    ...
}
```

By the way, this `ChangeState` method can be used to pass more complex parameters to a game state, too. For example, assuming you have some `PauseScreenConfig` class, you might write:

```c#
GSM.ChangeState<PauseScreen, PauseScreenConfig>(new PauseScreenConfig() {
	PreviousState = this,
	BackgroundOpacity = 0.5f,
	PausePicture = "Pause",
});
```

The `PauseScreen` game state can then ask for the `PauseScreenConfig` in its constructor. 

## Reference

### Game State Lifecycle

A game state's lifecycle event methods are called, in this order.

1. `Enter`
   * Called when the game state is entered into.
   * Do any first-time setup needed for the game state, ex: initializing a star field array.
2. `Input`
   * Called once per frame.
   * Handle user input here. For example, check if player is pressing up, and set a property on the player indicating that they'd like to move up.
3. `FixedUpdate`
   * Called ~60 times per second, regardless of the game's frame-rate.
   * This is a good place to handle physics updates, if your game has any.
   * If you've configured your application to `UseFixedTimeStep`, there's no difference between using `FixedUpdate` and `Update`.
4. `Update`
   * Called once per frame.
   * This is where many games will run most of their game logic, such as moving the player, updating particle effects, etc.
   * Use the passed-in `gameTime` to ensure that you update things at a consistent rate, regardless of the current frame rate.
5. `Draw`
   * Called once per frame.
   * Do main drawing logic here: backgrounds, player, enemies, particles, etc.
6. `Leave`
   * Called if a new game state is replacing the current game state.
   * Immediately after `Leave` is called, the current game state is changed to be the next game state.
   * I haven't yet found a use for this method; it's included for completeness/just-in-case.
