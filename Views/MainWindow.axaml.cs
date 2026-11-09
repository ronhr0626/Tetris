using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Tetris.ViewModels;

namespace Tetris.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();

        // Tunnel = we get the key before buttons and other controls do
        AddHandler(KeyDownEvent, OnGameKeyDown, RoutingStrategies.Tunnel);
    }

    private void OnGameKeyDown(object? sender, KeyEventArgs e)
    {
        if (DataContext is not MainViewModel vm) return;

        // Shortcuts: use Key (the letter), not PhysicalKey (the position)
        switch (e.Key)
        {
            case Key.R:
                vm.RestartCommand.Execute(null);
                e.Handled = true;
                return;

            case Key.P:
            case Key.Escape:
                vm.TogglePauseCommand.Execute(null);
                e.Handled = true;
                return;

            case Key.H:
                vm.ToggleHighScoresCommand.Execute(null);
                e.Handled = true;
                return;
        }

        // Movement: PhysicalKey = key position, works on any keyboard layout
        switch (e.PhysicalKey)
        {
            case PhysicalKey.ArrowLeft:
            case PhysicalKey.A:
                vm.MoveLeft();
                break;

            case PhysicalKey.ArrowRight:
            case PhysicalKey.D:
                vm.MoveRight();
                break;

            case PhysicalKey.ArrowDown:
            case PhysicalKey.S:
                vm.SoftDrop();
                break;

            case PhysicalKey.ArrowUp:
            case PhysicalKey.W:
                vm.Rotate();
                break;

            case PhysicalKey.Space:
                vm.HardDrop();
                break;

            default:
                return; // Not a game key: let it pass through
        }

        e.Handled = true; // Stop the key from also moving focus or clicking buttons
    }
}