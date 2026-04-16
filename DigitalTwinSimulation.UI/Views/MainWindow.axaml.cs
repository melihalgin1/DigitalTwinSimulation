using Avalonia.Controls;
using Avalonia.Interactivity;
using DigitalTwinSimulation.UI.ViewModels;

namespace DigitalTwinSimulation.UI.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        DataContext = new MainWindowViewModel();
    }

    private void AdvanceTakt_Click(object? sender, RoutedEventArgs e)
    {
        if (DataContext is MainWindowViewModel vm)
        {
            vm.AdvanceTakt();
        }
    }
}