using DigitalTwinSimulation.Core.Layout;
using DigitalTwinSimulation.Engine.Simulation;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;

namespace DigitalTwinSimulation.UI.ViewModels;

public class MainWindowViewModel : ViewModelBase, INotifyPropertyChanged
{
    private const double ShiftDurationMinutes = 480.0;

    private readonly PlantSimulationEngine _engine;

    private int _currentTakt;
    private int _finishedVehicleCount;
    private int _readyPowertrainCount;
    private string _nextReadyPowertrainVin = "NONE";

    private double _simulatedElapsedMinutes;
    private double _taktDurationMinutes = 1.0;
    private bool _isRunning;

    private SupplementaryMonitorItemViewModel? _supplementaryMonitor;
    private AsrsMonitorItemViewModel? _asrsMonitor;

    public event PropertyChangedEventHandler? PropertyChanged;

    public int CurrentTakt
    {
        get => _currentTakt;
        private set
        {
            if (_currentTakt != value)
            {
                _currentTakt = value;
                OnPropertyChanged();
            }
        }
    }

    public int FinishedVehicleCount
    {
        get => _finishedVehicleCount;
        private set
        {
            if (_finishedVehicleCount != value)
            {
                _finishedVehicleCount = value;
                OnPropertyChanged();
            }
        }
    }

    public int ReadyPowertrainCount
    {
        get => _readyPowertrainCount;
        private set
        {
            if (_readyPowertrainCount != value)
            {
                _readyPowertrainCount = value;
                OnPropertyChanged();
            }
        }
    }

    public string NextReadyPowertrainVin
    {
        get => _nextReadyPowertrainVin;
        private set
        {
            if (_nextReadyPowertrainVin != value)
            {
                _nextReadyPowertrainVin = value;
                OnPropertyChanged();
            }
        }
    }

    public double SimulatedElapsedMinutes
    {
        get => _simulatedElapsedMinutes;
        private set
        {
            if (Math.Abs(_simulatedElapsedMinutes - value) > 0.0001)
            {
                _simulatedElapsedMinutes = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(SimulatedElapsedTimeText));
            }
        }
    }

    public string SimulatedElapsedTimeText
    {
        get
        {
            int totalMinutes = (int)Math.Round(SimulatedElapsedMinutes);
            int hours = totalMinutes / 60;
            int minutes = totalMinutes % 60;

            return $"{hours:D2}:{minutes:D2}";
        }
    }

    public double TaktDurationMinutes
    {
        get => _taktDurationMinutes;
        set
        {
            double clampedValue = Math.Clamp(value, 0.5, 2.0);

            if (Math.Abs(_taktDurationMinutes - clampedValue) > 0.0001)
            {
                _taktDurationMinutes = clampedValue;
                OnPropertyChanged();
                OnPropertyChanged(nameof(TaktDurationDisplayText));
            }
        }
    }

    public string TaktDurationDisplayText =>
        $"{TaktDurationMinutes:0.0} min/takt";

    public bool IsRunning
    {
        get => _isRunning;
        private set
        {
            if (_isRunning != value)
            {
                _isRunning = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(RunStateText));
                OnPropertyChanged(nameof(StartPauseButtonText));
            }
        }
    }

    public string RunStateText => IsRunning ? "Running" : "Paused";

    public string StartPauseButtonText => IsRunning ? "Pause" : "Start";

    public SupplementaryMonitorItemViewModel? SupplementaryMonitor
    {
        get => _supplementaryMonitor;
        private set
        {
            if (_supplementaryMonitor != value)
            {
                _supplementaryMonitor = value;
                OnPropertyChanged();
            }
        }
    }

    public AsrsMonitorItemViewModel? AsrsMonitor
    {
        get => _asrsMonitor;
        private set
        {
            if (_asrsMonitor != value)
            {
                _asrsMonitor = value;
                OnPropertyChanged();
            }
        }
    }

    public ObservableCollection<GroupMonitorItemViewModel> GroupMonitors { get; } = new();
    public ObservableCollection<StationGroupSnapshot> StationGroups { get; } = new();
    public ObservableCollection<string> EngineDressingSnapshot { get; } = new();

    public MainWindowViewModel()
    {
        _engine = new PlantSimulationEngine();

        CurrentTakt = _engine.CurrentTakt;
        FinishedVehicleCount = _engine.FinishedVehicleCount;
        ReadyPowertrainCount = _engine.ReadyPowertrainCount;
        NextReadyPowertrainVin = _engine.NextReadyPowertrainVin;
        SimulatedElapsedMinutes = _engine.SimulatedElapsedMinutes;
        IsRunning = false;

        RefreshSnapshots();
    }

    public void ToggleRunPause()
    {
        IsRunning = !IsRunning;
    }

    public void Pause()
    {
        IsRunning = false;
    }

    public void AdvanceTakt()
    {
        _engine.AdvanceOneTakt(TaktDurationMinutes);

        CurrentTakt = _engine.CurrentTakt;
        FinishedVehicleCount = _engine.FinishedVehicleCount;
        ReadyPowertrainCount = _engine.ReadyPowertrainCount;
        NextReadyPowertrainVin = _engine.NextReadyPowertrainVin;
        SimulatedElapsedMinutes = _engine.SimulatedElapsedMinutes;

        RefreshSnapshots();
    }

    public void ResetSimulation()
    {
        Pause();

        _engine.Reset();

        CurrentTakt = _engine.CurrentTakt;
        FinishedVehicleCount = _engine.FinishedVehicleCount;
        ReadyPowertrainCount = _engine.ReadyPowertrainCount;
        NextReadyPowertrainVin = _engine.NextReadyPowertrainVin;
        SimulatedElapsedMinutes = _engine.SimulatedElapsedMinutes;

        RefreshSnapshots();
    }

    public StationGroupDetailWindowViewModel CreateStationGroupDetailViewModel(string groupName)
    {
        var monitorSnapshot = _engine
            .GetStandardGroupMonitors()
            .FirstOrDefault(monitor => monitor.GroupName == groupName);

        if (monitorSnapshot is null)
        {
            throw new InvalidOperationException($"Monitor snapshot not found for group '{groupName}'.");
        }

        var groupSnapshot = _engine
            .GetGroupedSnapshot()
            .FirstOrDefault(group => group.GroupName == groupName);

        if (groupSnapshot is null)
        {
            throw new InvalidOperationException($"Station group snapshot not found for group '{groupName}'.");
        }

        return new StationGroupDetailWindowViewModel(
            monitorSnapshot,
            groupSnapshot
        );
    }

    public SupplementaryDetailWindowViewModel CreateSupplementaryDetailWindowViewModel()
    {
        if (SupplementaryMonitor is null)
        {
            throw new InvalidOperationException("Supplementary monitor snapshot is not available.");
        }

        return new SupplementaryDetailWindowViewModel(
            SupplementaryMonitor,
            _engine.GetEngineDressingGroupSnapshot()
        );
    }

    public AsrsDetailWindowViewModel CreateAsrsDetailWindowViewModel()
    {
        if (AsrsMonitor is null)
        {
            throw new InvalidOperationException("ASRS monitor snapshot is not available.");
        }

        return new AsrsDetailWindowViewModel(AsrsMonitor);
    }

    public CompletedVehiclesWindowViewModel CreateCompletedVehiclesWindowViewModel()
    {
        var completedVehicles = _engine.GetCompletedVehiclesInLastShift();

        return new CompletedVehiclesWindowViewModel(
            completedVehicles,
            ShiftDurationMinutes,
            _engine.SimulatedElapsedMinutes
        );
    }

    private void RefreshSnapshots()
    {
        GroupMonitors.Clear();
        foreach (var monitor in _engine.GetStandardGroupMonitors())
        {
            GroupMonitors.Add(new GroupMonitorItemViewModel(monitor));
        }

        SupplementaryMonitor = new SupplementaryMonitorItemViewModel(
            _engine.GetSupplementaryMonitor()
        );

        AsrsMonitor = new AsrsMonitorItemViewModel(
            _engine.GetAsrsMonitor()
        );

        StationGroups.Clear();
        foreach (var group in _engine.GetGroupedSnapshot())
        {
            StationGroups.Add(group);
        }

        EngineDressingSnapshot.Clear();
        foreach (var line in _engine.GetEngineDressingSnapshot())
        {
            EngineDressingSnapshot.Add(line);
        }
    }

    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}