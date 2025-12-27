using System;
using System.IO;

namespace DebugAgentPrototype.Services;

public class SourceCodeService
{
    public static string GetSourceCode(string filePath)
    {
        return File.ReadAllText(filePath);
    }

    public static string GetInspectedFilePath()
    {
        var gamePath = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "test_program", "game");
        if (!Path.IsPathRooted(gamePath))
        {
            gamePath = Path.GetFullPath(gamePath);
        }

        if (!File.Exists(gamePath))
        {
            throw new FileNotFoundException($"Game executable not found at: {gamePath}");
        }

        return gamePath;
    }

    public static string GetInspectedFileContent()
    {
        var inspectedFilePath = GetInspectedFilePath();
        return File.ReadAllText(inspectedFilePath);
    }
}