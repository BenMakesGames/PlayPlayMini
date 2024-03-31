# What Is It?

Seamlessy-looping music is important for many games, but MonoGame's built-in music player isn't able to consistently loop music - more often than not, it lags, adding a short, but noticeable delay before looping.

`PlayPlayMini.NAudio` allows you to use NAudio to play music, resolving this issue, and adding support for cross-fading songs!

[![Buy Me a Coffee at ko-fi.com](https://raw.githubusercontent.com/BenMakesGames/AssetsForNuGet/main/buymeacoffee.png)](https://ko-fi.com/A0A12KQ16)

## How To Use

### Required Setup

1. Install this package.
   * `dotnet add package BenMakesGames.PlayPlayMini.NAudio`
2. Do NOT add your songs to the MCGB content pipeline tool (remove them if they're already there); instead, ensure your songs are set to "copy if newer" in the project's properties.
   * You can add something like the following to your `.csproj` file to automatically include all songs; change the path as needed, of course:
     ```xml
     <ItemGroup>
        <None Update="Content\Music\**\*.*">
          <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
        </None>
     </ItemGroup>
     ``` 
3. When adding assets to your game's `GameStateManagerBuilder`, use `new NAudioSongMeta(...)` instead of `new SongMeta(...)`. When using `new NAudioSongMeta(...)`, you must specify the extension of the song.
   * For example, `new NAudioSongMeta("TitleTheme", "Content/Music/TitleTheme.mp3")`.
4. In your startup state, where you wait for content loaders to finish loading, wait for `NAudioMusicPlayer.FullyLoaded`, too. 

**Note:** All songs you load must have the same sample rate (typically 44.1khz) and channel count (typically 2). When songs are loading, an error will be logged if they do not all match, and not all songs will be loaded.

### Optional Setup

`.mp3`, `.aiff`, and `.wav` files are supported out of the box. For other formats, you will need to install additional NAudio packages and do a little extra configuration:

#### Add Ogg Vorbis (`.ogg`) Support

Add the `NAudio.Vorbis` package to your project.

In your game's `.AddServices(...)` configuration, add the following line:

```c#
s.RegisterInstance(new NAudioFileLoader("ogg", f => new VorbisWaveReader(f)));
```

#### Add FLAC (`.flac`) Support

Add the `NAudio.Flac` package to your project.

In your game's `.AddServices(...)` configuration, add the following line:

```c#
s.RegisterInstance(new NAudioFileLoader("flac", f => new FlacReader(f)));
```
 
### Use

In your game state or services, get an `NAudioMusicPlayer` via the constructor (just as you would any other service), and use it to play and stop songs.

Example:

```c#
NAudioMusicPlayer
    .StopAllSongs(1000) // stop all songs, fading them out over 1 second
    .PlaySong("TitleTheme", 0); // start the TitleTheme with no fade-in time
```

Refer to the reference, below, for a list of available methods.

## `NAudioMusicPlayer` Method Reference

Note: negative fade-in and fade-out times are treated as 0.

### `NAudioMusicPlayer PlaySong(string name, int fadeInMilliseconds = 0)`

Starts playing the specific song, fading it in over the specified number of milliseconds.

Songs which are already playing will not be stopped! You must explicitly stop them using `StopAllSongs` or `StopSong` (below).

If the song is already playing, it will not be played again (you cannot play two copies of the song playing at the same time). If the song is fading in, its fade-in time will not be changed.  

### `NAudioMusicPlayer StopAllSongs(int fadeOutMilliseconds = 0)`

Stops all songs, fading them out over the specified number of milliseconds.

Songs which are already fading out will not have their fade-out time changed. A fade-out time of 0 will always immediately stops all songs, however.

To cross-fade between songs, you can chain `StopSongs` and `PlaySong` calls. For example:

```c#
NAudioMusicPlayer
    .StopAllSongs(1000)
    .PlaySong("TitleTheme");
```

### `NAudioMusicPlayer StopAllSongsExcept(string[] songsToContinue, int fadeOutMilliseconds = 0)`

Works like `StopAllSongs` (above), but does NOT stop the songs named in `songsToContinue`.

### `NAudioMusicPlayer StopAllSongsExcept(string name, int fadeOutMilliseconds = 0)`

Works like `StopAllSongs` (above), but does NOT stop the named song.

### `NAudioMusicPlayer StopSong(string name, int fadeOutMilliseconds = 0)`

Like `StopAllSongs` (above), but stops only the named song.

### `NAudioMusicPlayer SetVolume(float volume)`

Changes the volume for all songs.

### `bool IsPlaying(string name)`

Returns `true` if the specific song is currently playing.

### `string[] GetPlayingSongs()`

Returns an array of the names of all songs currently playing.
