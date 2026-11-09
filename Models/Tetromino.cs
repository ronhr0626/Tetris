using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia.Media;

namespace Tetris.Models;

public class Tetromino
{
    private static readonly Random Rng = new();

    public char Type { get; }

    // Block positions relative to the piece's pivot
    public (int X, int Y)[] Blocks { get; private set; }
    public IBrush Color { get; }

    // Piece position on the board
    public int X { get; set; }
    public int Y { get; set; }

    private Tetromino(char type, (int X, int Y)[] blocks, IBrush color)
    {
        Type = type;
        Blocks = blocks;
        Color = color;
    }

    // Returns the actual board cells the piece occupies
    public IEnumerable<(int X, int Y)> GetCells()
    {
        foreach (var b in Blocks)
            yield return (X + b.X, Y + b.Y);
    }

    // Rotate 90 degrees clockwise around the pivot: (x, y) -> (-y, x)
    public void Rotate()
    {
        if (Type == 'O') return; // O looks the same in every rotation
        Blocks = Blocks.Select(b => (-b.Y, b.X)).ToArray();
    }

    // Rotate 90 degrees counter-clockwise (used to undo an invalid rotation)
    public void RotateBack()
    {
        if (Type == 'O') return;
        Blocks = Blocks.Select(b => (b.Y, -b.X)).ToArray();
    }

    public static Tetromino Create(char type) => type switch
    {
        'I' => new Tetromino(type, [(-1, 0), (0, 0), (1, 0), (2, 0)], Brushes.Cyan),
        'O' => new Tetromino(type, [(0, 0), (1, 0), (0, 1), (1, 1)], Brushes.Yellow),
        'T' => new Tetromino(type, [(-1, 0), (0, 0), (1, 0), (0, 1)], Brushes.Purple),
        'S' => new Tetromino(type, [(0, 0), (1, 0), (-1, 1), (0, 1)], Brushes.LimeGreen),
        'Z' => new Tetromino(type, [(-1, 0), (0, 0), (0, 1), (1, 1)], Brushes.Red),
        'J' => new Tetromino(type, [(-1, 0), (0, 0), (1, 0), (1, 1)], Brushes.Blue),
        'L' => new Tetromino(type, [(-1, 0), (0, 0), (1, 0), (-1, 1)], Brushes.Orange),
        _ => throw new ArgumentException($"Unknown piece type: {type}")
    };

    public static Tetromino CreateRandom() => Create("IOTSZJL"[Rng.Next(7)]);
}