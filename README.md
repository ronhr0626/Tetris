# Tetris in Avalonia

A Tetris game built in C# with [Avalonia UI](https://avaloniaui.net/) and MVVM, made as a learning project for Avalonia.

## Features

- 10×20 board with all 7 classic pieces
- Rotation, collision and piece locking
- Full rows are cleared, with scoring based on rows cleared at once (1 / 2 / 3 / 4)
- Levels: speed increases every 10 lines
- Ghost piece showing where the piece will land
- Next piece preview, pause and game over
- High scores (top 20) saved to file

## Controls

| Key | Action |
|---|---|
| ← → / A D | Move |
| ↑ / W | Rotate |
| ↓ / S | Soft drop |
| Space | Hard drop |
| P / Esc | Pause |
| H | High scores |
| R | Restart |

## Running the project

Requires the [.NET 10 SDK](https://dotnet.microsoft.com/download).

```bash
git clone https://github.com/ronhr0626/Tetris.git
cd Tetris
dotnet run
```

## Structure

```
Models/       Game logic, no UI
  Cell.cs           A single cell on the board
  Tetromino.cs      Pieces, position and rotation
  GameBoard.cs      Board state, collision and row clearing
  HighScoreStore.cs High score persistence as JSON
ViewModels/   Connects logic to the UI
  MainViewModel.cs  Game loop, input, scoring and game state
Views/        What is displayed
  MainWindow.axaml      Layout and styling
  MainWindow.axaml.cs   Keyboard input
```

## What I learned

- **MVVM**: View (XAML) and ViewModel (C#) are separated and connected through bindings
- **CommunityToolkit.Mvvm**: `[ObservableProperty]` and `[RelayCommand]`
- **Layout**: `Grid`, `StackPanel`, `UniformGrid`, `Viewbox` and overlays
- **Styles**: reusable styles with classes and states (`:pointerover`, `:pressed`)
- **Game loop** with `DispatcherTimer`
- **Keyboard input** with `KeyDown`, tunneling and `PhysicalKey`
- **Game logic** separated into its own classes in `Models/`
- **File handling** with JSON for high scores

## Tech

- C# / .NET 10
- Avalonia 12
- CommunityToolkit.Mvvm