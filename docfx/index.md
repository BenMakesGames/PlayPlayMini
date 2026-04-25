# Getting Started

### Install MonoGame's MGCB Editor

`dotnet tool install -g dotnet-mgcb`

If you're not familiar with this tool, [refer to MonoGame's official documentation](https://monogame.net/articles/tools/mgcb_editor.html). PlayPlayMini (currently) requires you to use this tool to add new assets, such as graphics, music, and sounds.

### Install PlayPlayMini Templates

`dotnet new install BenMakesGames.PlayPlayMini.Templates`

### Create a New Game

1. Create an empty solution (optional, but recommended)
2. From within the solution's root directory, run `dotnet new playplaymini.skeleton -n YourGameNameHere`
   1. replacing `YourGameNameHere` with your game name, of course!
3. Add the new project to your solution
4. Run it!

