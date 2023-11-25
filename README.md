# What Is It?

`PlayPlayMini` is an opinionated framework for making smallish 2D games with `MonoGame` in .NET Core.

It provides a state engine with lifecycle events, dependency injection using `Autofac`, and several built-in services to help with common tasks such as drawing sprites & fonts, and getting user input.

[![Buy Me a Coffee at ko-fi.com](https://raw.githubusercontent.com/BenMakesGames/AssetsForNuGet/main/buymeacoffee.png)](https://ko-fi.com/A0A12KQ16)

This repository contains the following libraries:

| Name | Description | Links |
| --- | --- | --- |
| `PlayPlayMini`                    | The PlayPlayMini library itself.                                                      | [NuGet](https://www.nuget.org/packages/BenMakesGames.PlayPlayMini)<br />[Documentation](BenMakesGames.PlayPlayMini/README.md) |
| `PlayPlayMini.GraphicsExtensions` | Graphics extensions for PlayPlayMini, including shape primitives and font extensions. | [NuGet](https://www.nuget.org/packages/BenMakesGames.PlayPlayMini.GraphicsExtensions)<br />[Documentation](BenMakesGames.PlayPlayMini.GraphicsExtensions/README.md) |
| `PlayPlayMini.UI`                 | Skinnable, object-oriented UI Framework for PlayPlayMini.                             | [NuGet](https://www.nuget.org/packages/BenMakesGames.PlayPlayMini.UI)<br />[Documentation](BenMakesGames.PlayPlayMini.UI/README.md) |
| `PlayPlayMini.BeepBoop`           | Methods for generating wave forms on the fly. Preview release; very few features.     | [NuGet](https://www.nuget.org/packages/BenMakesGames.PlayPlayMini.BeepBoop) |

See also:
* [PlayPlayMiniTemplates](https://github.com/BenMakesGames/PlayPlayMiniTemplates) for project templates.
* [Block-break](https://github.com/BenMakesGames/BlockBreak), a demo game made with PlayPlayMini, EntityFramework (for saving settings & high scores), and Serliog, and which demonstrates multiple game states, player input, font-rendering, sprite sheets, pictures, and sounds.
