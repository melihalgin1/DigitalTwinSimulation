using Avalonia.Controls;
using Avalonia.Threading;
using DigitalTwinSimulation.UI.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DigitalTwinSimulation.UI.Views;

public partial class MainWindow : Window
{
    private readonly DispatcherTimer _simulationTimer;
    private readonly Dictionary<string, StationGroupDetailWindow> _openGroupDetailWindows = [];

    private SupplementaryDetailWindow? _supplementaryDetailWindow;
    private AsrsDetailWindow? _asrsDetailWindow;

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
        RefreshOpenGroupDetailWindows(viewModel);
        RefreshSupplementaryDetailWindow(viewModel);
        RefreshAsrsDetailWindow(viewModel);
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
            RefreshOpenGroupDetailWindows(viewModel);
            RefreshSupplementaryDetailWindow(viewModel);
            RefreshAsrsDetailWindow(viewModel);
        }
    }

    private void StepTakt_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (DataContext is not MainWindowViewModel viewModel)
            return;

        viewModel.Pause();
        _simulationTimer.Stop();

        viewModel.AdvanceTakt();
        RefreshOpenGroupDetailWindows(viewModel);
        RefreshSupplementaryDetailWindow(viewModel);
        RefreshAsrsDetailWindow(viewModel);
    }

    private void ResetSimulation_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (DataContext is not MainWindowViewModel viewModel)
            return;

        _simulationTimer.Stop();
        viewModel.ResetSimulation();

        foreach (var window in _openGroupDetailWindows.Values.ToList())
        {
            window.Close();
        }

        _openGroupDetailWindows.Clear();

        _supplementaryDetailWindow?.Close();
        _supplementaryDetailWindow = null;

        _asrsDetailWindow?.Close();
        _asrsDetailWindow = null;
    }

    private void GroupMonitor_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (sender is not Button button)
            return;

        if (button.DataContext is not GroupMonitorItemViewModel groupMonitor)
            return;

        if (DataContext is not MainWindowViewModel viewModel)
            return;

        string groupName = groupMonitor.GroupName;

        if (_openGroupDetailWindows.TryGetValue(groupName, out var existingWindow))
        {
            existingWindow.DataContext = viewModel.CreateStationGroupDetailViewModel(groupName);
            existingWindow.Activate();
            return;
        }

        var detailViewModel = viewModel.CreateStationGroupDetailViewModel(groupName);

        var detailWindow = new StationGroupDetailWindow
        {
            DataContext = detailViewModel
        };

        detailWindow.Closed += (_, _) =>
        {
            _openGroupDetailWindows.Remove(groupName);
        };

        _openGroupDetailWindows[groupName] = detailWindow;

        detailWindow.Show(this);
    }

    private void SupplementaryMonitor_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (DataContext is not MainWindowViewModel viewModel)
            return;

        var detailViewModel = viewModel.CreateSupplementaryDetailWindowViewModel();

        if (_supplementaryDetailWindow is not null)
        {
            _supplementaryDetailWindow.DataContext = detailViewModel;
            _supplementaryDetailWindow.Activate();
            return;
        }

        _supplementaryDetailWindow = new SupplementaryDetailWindow
        {
            DataContext = detailViewModel
        };

        _supplementaryDetailWindow.Closed += (_, _) =>
        {
            _supplementaryDetailWindow = null;
        };

        _supplementaryDetailWindow.Show(this);
    }

    private void AsrsMonitor_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (DataContext is not MainWindowViewModel viewModel)
            return;

        var detailViewModel = viewModel.CreateAsrsDetailWindowViewModel();

        if (_asrsDetailWindow is not null)
        {
            _asrsDetailWindow.DataContext = detailViewModel;
            _asrsDetailWindow.Activate();
            return;
        }

        _asrsDetailWindow = new AsrsDetailWindow
        {
            DataContext = detailViewModel
        };

        _asrsDetailWindow.Closed += (_, _) =>
        {
            _asrsDetailWindow = null;
        };

        _asrsDetailWindow.Show(this);
    }

    private void RefreshOpenGroupDetailWindows(MainWindowViewModel viewModel)
    {
        foreach (var groupName in _openGroupDetailWindows.Keys.ToList())
        {
            if (!_openGroupDetailWindows.TryGetValue(groupName, out var window))
                continue;

            window.DataContext = viewModel.CreateStationGroupDetailViewModel(groupName);
        }
    }

    private void RefreshSupplementaryDetailWindow(MainWindowViewModel viewModel)
    {
        if (_supplementaryDetailWindow is null)
            return;

        _supplementaryDetailWindow.DataContext =
            viewModel.CreateSupplementaryDetailWindowViewModel();
    }

    private void RefreshAsrsDetailWindow(MainWindowViewModel viewModel)
    {
        if (_asrsDetailWindow is null)
            return;

        _asrsDetailWindow.DataContext =
            viewModel.CreateAsrsDetailWindowViewModel();
    }
}