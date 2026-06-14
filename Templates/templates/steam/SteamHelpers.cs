using Steamworks;
using System;

namespace MyNamespace;

public static class SteamHelpers
{
    // TODO: replace with YOUR Steam app id (and edit the steam_appid.txt file as well!)
    public const int SteamAppId = 480;

    public static bool Initialized { get; private set; }

    public static bool Startup()
    {
        if (Initialized)
            throw new InvalidOperationException();

        if(SteamAPI.RestartAppIfNecessary(new AppId_t(SteamAppId)))
            return false;

        if (!SteamAPI.Init())
            return false;

        Initialized = true;

        return true;
    }

    public static void Shutdown()
    {
        if (!Initialized)
            throw new InvalidOperationException();

        Initialized = false;

        SteamAPI.Shutdown();
    }
}
