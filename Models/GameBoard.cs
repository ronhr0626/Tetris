using System;
using System.Linq;
using Avalonia.Media;

namespace Tetris.Models;

public class GameBoard
{
    public int Columns { get; }
    public int Rows { get; }

    // null = empty cell, otherwise the color of the locked block
    private readonly IBrush?[,] _grid;

    public GameBoard(int columns, int rows)
    {
        Columns = columns;
        Rows = rows;
        _grid = new IBrush?[columns, rows];
    }

    public IBrush? GetColor(int x, int y) => _grid[x, y];

    // A cell is free if it's inside the walls/floor and not occupied.
    // y < 0 (above the board) counts as free so pieces can spawn partly hidden.
    public bool IsFree(int x, int y)
    {
        if (x < 0 || x >= Columns || y >= Rows) return false;
        if (y < 0) return true;
        return _grid[x, y] == null;
    }

    // Checks if the piece fits when moved by dx, dy
    public bool CanPlace(Tetromino piece, int dx, int dy) =>
        piece.GetCells().All(c => IsFree(c.X + dx, c.Y + dy));

    // Writes the piece into the grid permanently
    public void Lock(Tetromino piece)
    {
        foreach (var (x, y) in piece.GetCells())
        {
            if (y >= 0)
                _grid[x, y] = piece.Color;
        }
    }

    // Removes all full rows and returns how many were cleared
    public int ClearFullRows()
    {
        int cleared = 0;

        for (int y = Rows - 1; y >= 0; y--)
        {
            if (!IsRowFull(y)) continue;

            RemoveRow(y);
            cleared++;
            y++; // Everything moved down one step, so check this row again
        }

        return cleared;
    }

    private bool IsRowFull(int y)
    {
        for (int x = 0; x < Columns; x++)
        {
            if (_grid[x, y] == null) return false;
        }
        return true;
    }

    // Moves every row above 'row' down by one, and empties the top row
    private void RemoveRow(int row)
    {
        for (int y = row; y > 0; y--)
        for (int x = 0; x < Columns; x++)
            _grid[x, y] = _grid[x, y - 1];

        for (int x = 0; x < Columns; x++)
            _grid[x, 0] = null;
    }

    public void Clear() => Array.Clear(_grid);
}