using System;
using System.IO;

namespace MyNamespace;

public static class DirectoryHelpers
{
    private static readonly string AppDataDirectory = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);

//-:cnd:noEmit
    #if DEBUG
        public static readonly string MyNamespaceDirectory = $"{AppDataDirectory}{Path.DirectorySeparatorChar}MyNamespace (Debug)";
    #else
        public static readonly string MyNamespaceDirectory = $"{AppDataDirectory}{Path.DirectorySeparatorChar}MyNamespace";
    #endif
//+:cnd:noEmit

    public static readonly string LogDirectory = $"{MyNamespaceDirectory}{Path.DirectorySeparatorChar}Logs";
    public static readonly string SaveDirectory = $"{MyNamespaceDirectory}{Path.DirectorySeparatorChar}Saves";

    public static void EnsureDirectoryExists()
    {
        Directory.CreateDirectory(MyNamespaceDirectory);
        Directory.CreateDirectory(LogDirectory);
        Directory.CreateDirectory(SaveDirectory);
    }
}
