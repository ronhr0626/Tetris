using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace Tetris.Models;

// Saves and loads the top scores as JSON in the user's AppData folder
public static class HighScoreStore
{
    public const int MaxEntries = 20;

    private static readonly string FilePath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "Tetris",
        "highscores.json");

    public static List<int> Load()
    {
        try
        {
            if (!File.Exists(FilePath)) return new();
            string json = File.ReadAllText(FilePath);
            return JsonSerializer.Deserialize<List<int>>(json) ?? new();
        }
        catch
        {
            return new(); // Corrupt or unreadable file: start fresh
        }
    }

    // Adds a score, keeps only the best ones, saves and returns the new list
    public static List<int> Add(int score)
    {
        var top = Load()
            .Append(score)
            .OrderByDescending(s => s)
            .Take(MaxEntries)
            .ToList();

        Save(top);
        return top;
    }

    private static void Save(List<int> scores)
    {
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(FilePath)!);
            File.WriteAllText(FilePath, JsonSerializer.Serialize(scores));
        }
        catch
        {
            // Saving failed (e.g. no write access): the game keeps running anyway
        }
    }
}