# What Is It?

`PlayPlayMini` is an opinionated framework for making smallish 2D games with `MonoGame` in .NET Core.

It provides a state engine with lifecycle events, dependency injection using `Autofac`, and several built-in services to help with common tasks such as drawing sprites & fonts, and getting user input.

> [🧚 **Hey, listen!** You can support my development of open-source software on Patreon](https://www.patreon.com/BenMakesGames)

This repository contains the following libraries:

| Name                              | Description                                                                                                                                               | Links |
|-----------------------------------|-----------------------------------------------------------------------------------------------------------------------------------------------------------| --- |
| `PlayPlayMini`                    | The PlayPlayMini library itself.                                                                                                                          | [NuGet](https://www.nuget.org/packages/BenMakesGames.PlayPlayMini)<br />[Documentation](BenMakesGames.PlayPlayMini/README.md) |
| `PlayPlayMini.GraphicsExtensions` | Graphics extensions for PlayPlayMini, including shape primitives and font extensions.                                                                     | [NuGet](https://www.nuget.org/packages/BenMakesGames.PlayPlayMini.GraphicsExtensions)<br />[Documentation](BenMakesGames.PlayPlayMini.GraphicsExtensions/README.md) |
| `PlayPlayMini.NAudio`             | Use NAudio to play music, instead of MonoGame's built-in audio engine. This makes music loop seamlessly, and adds support for easy cross-fading of songs. | [NuGet](https://www.nuget.org/packages/BenMakesGames.PlayPlayMini.NAudio)<br />[Documentation](BenMakesGames.PlayPlayMini.NAudio/README.md) |
| `PlayPlayMini.VN`                 | Visual Novel game engine. Preview release, but contains many features. Has been used in games published to Steam.                                         | [NuGet](https://www.nuget.org/packages/BenMakesGames.PlayPlayMini.BeepBoop) |
| `PlayPlayMini.BeepBoop`           | Methods for generating wave forms on the fly. Preview release; very few features.                                                                         | [NuGet](https://www.nuget.org/packages/BenMakesGames.PlayPlayMini.BeepBoop) |
| `PlayPlayMini.UI`                 | ⚠ABANDONED⚠ Skinnable, object-oriented UI Framework for PlayPlayMini.                                                                                     | [NuGet](https://www.nuget.org/packages/BenMakesGames.PlayPlayMini.UI)<br />[Documentation](BenMakesGames.PlayPlayMini.UI/README.md) |

See also:
* [PlayPlayMiniTemplates](https://github.com/BenMakesGames/PlayPlayMiniTemplates) for project templates.
* [Block-break](https://github.com/BenMakesGames/BlockBreak), a demo game made with PlayPlayMini, EntityFramework (for saving settings & high scores), and Serliog, and which demonstrates multiple game states, player input, font-rendering, sprite sheets, pictures, and sounds.
* [API documentation](https://benmakesgames.github.io/PlayPlayMini/) (work-in-progress; currently documents many classes and methods in `PlayPlayMini`, `PlayPlayMini.GraphicsExtension`, and `PlayPlayMini.NAudio`)
