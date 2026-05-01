using Avalonia.Controls;
using Avalonia.Threading;
using DigitalTwinSimulation.UI.ViewModels;
using System;

namespace DigitalTwinSimulation.UI.Views;

public partial class MainWindow : Window
{
    private readonly DispatcherTimer _simulationTimer;

    public MainWindow()
    {
        InitializeComponent();

        _simulationTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(500)
        };

        _simulationTimer.Tick += SimulationTimer_Tick;
    }

    private void SimulationTimer_Tick(object? sender, EventArgs e)
    {
        if (DataContext is not MainWindowViewModel viewModel)
            return;

        if (!viewModel.IsRunning)
        {
            _simulationTimer.Stop();
            return;
        }

        viewModel.AdvanceTakt();
    }

    private void StartPause_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (DataContext is not MainWindowViewModel viewModel)
            return;

        viewModel.ToggleRunPause();

        if (viewModel.IsRunning)
        {
            _simulationTimer.Start();
        }
        else
        {
            _simulationTimer.Stop();
        }
    }

    private void StepTakt_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (DataContext is not MainWindowViewModel viewModel)
            return;

        viewModel.Pause();
        _simulationTimer.Stop();

        viewModel.AdvanceTakt();
    }

    private void ResetSimulation_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (DataContext is not MainWindowViewModel viewModel)
            return;

        _simulationTimer.Stop();
        viewModel.ResetSimulation();
    }
}