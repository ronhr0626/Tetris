using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Threading;
using Tetris.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Tetris.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    public const int Columns = 10;
    public const int Rows = 20;
    public const int PreviewSize = 4;
    public const int SidebarHighScores = 3;

    public ObservableCollection<Cell> Cells { get; } = new();
    public ObservableCollection<Cell> NextCells { get; } = new();

    // Top 3 for the side panel, top 20 for the high score screen
    public ObservableCollection<string> HighScores { get; } = new();
    public ObservableCollection<string> AllHighScores { get; } = new();

    [ObservableProperty]
    private int _score;

    [ObservableProperty]
    private int _lines;

    [ObservableProperty]
    private int _level = 1;

    [ObservableProperty]
    private bool _isGameOver;

    [ObservableProperty]
    private bool _isPaused;

    [ObservableProperty]
    private bool _isHighScoresOpen;

    private readonly GameBoard _board = new(Columns, Rows);
    private readonly DispatcherTimer _timer;
    private Tetromino _current = Tetromino.CreateRandom();
    private Tetromino _next = Tetromino.CreateRandom();

    // Input is only allowed while the game is actually running
    private bool CanPlay => !IsGameOver && !IsPaused && !IsHighScoresOpen;

    public MainViewModel()
    {
        for (int i = 0; i < Columns * Rows; i++)
            Cells.Add(new Cell());

        for (int i = 0; i < PreviewSize * PreviewSize; i++)
            NextCells.Add(new Cell());

        ShowHighScores(HighScoreStore.Load());

        _timer = new DispatcherTimer();
        _timer.Tick += (_, _) => Tick();
        UpdateSpeed();

        SpawnPiece();

        // Don't start the timer in the VS Code previewer
        if (!Design.IsDesignMode)
            _timer.Start();
    }

    public Cell GetCell(int x, int y) => Cells[y * Columns + x];

    // Runs on every tick
    private void Tick()
    {
        if (CanMove(0, 1))
        {
            _current.Y++;
            Draw();
        }
        else
        {
            LandPiece();
        }
    }

    // Lock the piece, clear rows, update score and spawn the next piece
    private void LandPiece()
    {
        _board.Lock(_current);
        int cleared = _board.ClearFullRows();
        AddScore(cleared);
        SpawnPiece();
    }

    // Classic scoring, multiplied by level
    private void AddScore(int clearedRows)
    {
        if (clearedRows == 0) return;

        Lines += clearedRows;
        Score += clearedRows switch
        {
            1 => 100,
            2 => 300,
            3 => 500,
            4 => 800,
            _ => 0
        } * Level;

        // Level up every 10 lines
        int newLevel = Lines / 10 + 1;
        if (newLevel != Level)
        {
            Level = newLevel;
            UpdateSpeed();
        }
    }

    // Starts at 800 ms per step, 60 ms faster each level, never below 100 ms
    private void UpdateSpeed()
    {
        const int startMs = 800;
        const int stepMs = 60;
        const int minMs = 100;

        int ms = Math.Max(minMs, startMs - (Level - 1) * stepMs);
        _timer.Interval = TimeSpan.FromMilliseconds(ms);
    }

    // Fills both lists, with "-" for empty slots
    private void ShowHighScores(List<int> scores)
    {
        HighScores.Clear();
        for (int i = 0; i < SidebarHighScores; i++)
            HighScores.Add(FormatEntry(i, scores));

        AllHighScores.Clear();
        for (int i = 0; i < HighScoreStore.MaxEntries; i++)
            AllHighScores.Add(FormatEntry(i, scores));
    }

    // Rank padded to 2 characters so the list lines up: " 1.  1200"
    private static string FormatEntry(int index, List<int> scores)
    {
        string value = index < scores.Count ? scores[index].ToString() : "-";
        return $"{index + 1,2}.  {value}";
    }

    // Player actions, called from the View
    public void MoveLeft() => TryMove(-1, 0);
    public void MoveRight() => TryMove(1, 0);

    public void SoftDrop()
    {
        if (TryMove(0, 1))
            Score += 1; // Small reward for dropping faster
    }

    public void HardDrop()
    {
        if (!CanPlay) return;

        int distance = GetDropDistance();
        _current.Y += distance;
        Score += distance * 2; // Bigger reward for committing
        LandPiece();
    }

    public void Rotate()
    {
        if (!CanPlay) return;

        _current.Rotate();
        if (!CanMove(0, 0))
            _current.RotateBack(); // Rotation would collide: undo it
        Draw();
    }

    [RelayCommand]
    private void TogglePause()
    {
        if (IsGameOver || IsHighScoresOpen) return;

        IsPaused = !IsPaused;
        if (IsPaused)
            _timer.Stop();
        else if (!Design.IsDesignMode)
            _timer.Start();
    }

    [RelayCommand]
    private void ToggleHighScores()
    {
        IsHighScoresOpen = !IsHighScoresOpen;

        if (IsHighScoresOpen)
            _timer.Stop();
        else if (CanPlay && !Design.IsDesignMode)
            _timer.Start(); // Only resume if not paused or game over
    }

    // Returns true if the move happened
    private bool TryMove(int dx, int dy)
    {
        if (!CanPlay || !CanMove(dx, dy)) return false;
        _current.X += dx;
        _current.Y += dy;
        Draw();
        return true;
    }

    // The board handles walls, floor and locked blocks
    private bool CanMove(int dx, int dy) => _board.CanPlace(_current, dx, dy);

    // How many rows the piece can fall before it hits something
    private int GetDropDistance()
    {
        int distance = 0;
        while (CanMove(0, distance + 1))
            distance++;
        return distance;
    }

    private void SpawnPiece()
    {
        // The "next" piece becomes the current one, and we roll a new "next"
        _current = _next;
        _next = Tetromino.CreateRandom();

        _current.X = Columns / 2 - 1;
        _current.Y = 0;

        // No room for the new piece: game over
        if (!CanMove(0, 0))
        {
            _timer.Stop();
            IsGameOver = true;

            // Save the score (only kept if it makes the top 20)
            if (Score > 0 && !Design.IsDesignMode)
                ShowHighScores(HighScoreStore.Add(Score));
        }

        Draw();
        DrawNext();
    }

    [RelayCommand]
    private void Restart()
    {
        _board.Clear();
        Score = 0;
        Lines = 0;
        Level = 1;
        IsGameOver = false;
        IsPaused = false;
        IsHighScoresOpen = false;
        UpdateSpeed();

        _next = Tetromino.CreateRandom();
        SpawnPiece();

        if (!Design.IsDesignMode)
            _timer.Start();
    }

    private void Draw()
    {
        // 1. Locked blocks from the board
        for (int y = 0; y < Rows; y++)
        for (int x = 0; x < Columns; x++)
            GetCell(x, y).Fill = _board.GetColor(x, y) ?? Brushes.Black;

        // 2. Ghost piece: same shape, faded, where the piece would land
        int drop = GetDropDistance();
        if (drop > 0 && _current.Color is ISolidColorBrush solid)
        {
            var ghost = new SolidColorBrush(solid.Color, 0.25);
            foreach (var (x, y) in _current.GetCells())
            {
                int gy = y + drop;
                if (x >= 0 && x < Columns && gy >= 0 && gy < Rows)
                    GetCell(x, gy).Fill = ghost;
            }
        }

        // 3. Active piece on top
        foreach (var (x, y) in _current.GetCells())
        {
            if (x >= 0 && x < Columns && y >= 0 && y < Rows)
                GetCell(x, y).Fill = _current.Color;
        }
    }

    private void DrawNext()
    {
        foreach (var cell in NextCells)
            cell.Fill = Brushes.Black;

        // Blocks range from x = -1..2 and y = 0..1, so shift them into the 4x4 grid
        foreach (var (bx, by) in _next.Blocks)
        {
            int x = bx + 1;
            int y = by + 1;
            NextCells[y * PreviewSize + x].Fill = _next.Color;
        }
    }
}