using Avalonia.Controls;
using DigitalTwinSimulation.UI.ViewModels;

namespace DigitalTwinSimulation.UI.Views;

public partial class StationGroupDetailWindow : Window
{
    private CompletedVehiclesWindow? _completedVehiclesWindow;

    public StationGroupDetailWindow()
    {
        InitializeComponent();
    }

    private void ShowCompletedVehicles_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (DataContext is not StationGroupDetailWindowViewModel detailViewModel)
            return;

        if (!detailViewModel.IsFinalGroup)
            return;

        if (Owner is not MainWindow mainWindow)
            return;

        if (mainWindow.DataContext is not MainWindowViewModel mainViewModel)
            return;

        var completedVehiclesViewModel = mainViewModel.CreateCompletedVehiclesWindowViewModel();

        if (_completedVehiclesWindow is not null)
        {
            _completedVehiclesWindow.Activate();
            return;
        }

        _completedVehiclesWindow = new CompletedVehiclesWindow
        {
            DataContext = completedVehiclesViewModel
        };

        _completedVehiclesWindow.Closed += (_, _) =>
        {
            _completedVehiclesWindow = null;
        };

        _completedVehiclesWindow.Show(this);
    }
}