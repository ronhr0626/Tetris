using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Tetris.Models;

public partial class Cell : ObservableObject
{
    [ObservableProperty]
    private IBrush _fill = Brushes.Black;
}