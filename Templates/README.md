# PlayPlayMini Templates

This is a collection of templates for [PlayPlayMini](https://github.com/BenMakesGames/PlayPlayMini).

The available templates are:
1. `playplaymini.skeleton`
   * A basic PlayPlayMini project, with no other dependencies.
   * Creates a template `Startup` and `Playing` game state to help you get started.
2. `playplaymini.serilog`
   * Based on `playplaymini.skeleton`, and adds Serilog to log exceptions, and other information.
3. `playplaymini.steam`
    * Based on `playplaymini.skeleton`, and adds Steamworks.NET, Serilog, and a Powershell build script.
    * Does NOT come bundled with `steam_api64.dll` - you must download this file from Valve.

> [🧚 **Hey, listen!** You can support my development of open-source software on Patreon](https://www.patreon.com/BenMakesGames)

## Install Instructions

`dotnet new install BenMakesGames.PlayPlayMini.Templates`

## Usage Instructions

To create a new PlayPlayMini project, run the following command from the directory where you keep all your projects:

`dotnet new TEMPLATENAME -n GAMENAME`

1. Replace `TEMPLATENAME` with one of the templates listed above (ex: `playplaymini.skeleton`).
2. Replace `GAMENAME` with the name of your game.
3. A new directory will be created in the current directory called `GAMENAME`. 

For example:

`dotnet new playplaymini.skeleton -n TetrisClone`

A new directory called `TetrisClone` will have been created in the current directory.
