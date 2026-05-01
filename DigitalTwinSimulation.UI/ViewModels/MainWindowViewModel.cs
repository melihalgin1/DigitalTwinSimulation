using DigitalTwinSimulation.Core.Layout;
using DigitalTwinSimulation.Engine.Simulation;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace DigitalTwinSimulation.UI.ViewModels;

public class MainWindowViewModel : ViewModelBase, INotifyPropertyChanged
{
    private readonly PlantSimulationEngine _engine;

    private int _currentTakt;
    private int _finishedVehicleCount;
    private int _readyPowertrainCount;
    private string _nextReadyPowertrainVin = "NONE";
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

        RefreshSnapshots();
    }

    public void AdvanceTakt()
    {
        _engine.AdvanceOneTakt();

        CurrentTakt = _engine.CurrentTakt;
        FinishedVehicleCount = _engine.FinishedVehicleCount;
        ReadyPowertrainCount = _engine.ReadyPowertrainCount;
        NextReadyPowertrainVin = _engine.NextReadyPowertrainVin;

        RefreshSnapshots();
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