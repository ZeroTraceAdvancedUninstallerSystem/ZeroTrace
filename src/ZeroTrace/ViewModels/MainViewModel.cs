using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using ZeroTrace.Core.Discovery;
using ZeroTrace.Core.Models;

namespace ZeroTrace.ViewModels;

public class MainViewModel : INotifyPropertyChanged
{
    private readonly DiscoveryService _discoveryService;
    private bool _isBusy;
    private string _statusMessage = "Bereit für Scan...";

    // Die Liste, die die UI automatisch aktualisiert
    public ObservableCollection<InstalledProgram> InstalledPrograms { get; } = new();

    public bool IsBusy
    {
        get => _isBusy;
        set { _isBusy = value; OnPropertyChanged(); }
    }

    public string StatusMessage
    {
        get => _statusMessage;
        set { _statusMessage = value; OnPropertyChanged(); }
    }

    // Der Befehl, der beim Klick auf den Button ausgeführt wird
    public ICommand ScanCommand { get; }

    public MainViewModel(DiscoveryService discoveryService)
    {
        _discoveryService = discoveryService;
        
        // Initialisierung des Scan-Commands
        ScanCommand = new RelayCommand(async () => await PerformScanAsync());
    }

    private async Task PerformScanAsync()
    {
        if (IsBusy) return;

        IsBusy = true;
        StatusMessage = "Scanne System nach Programmen...";
        
        try
        {
            var programs = await _discoveryService.GetAllInstalledProgramsAsync();
            
            // UI-Thread sicher aktualisieren
            App.Current.Dispatcher.Invoke(() => {
                InstalledPrograms.Clear();
                foreach (var prog in programs)
                {
                    InstalledPrograms.Add(prog);
                }
            });

            StatusMessage = $"Scan abgeschlossen. {InstalledPrograms.Count} Programme gefunden.";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Fehler beim Scan: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string? name = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}

// Hilfsklasse für die Buttons
public class RelayCommand : ICommand
{
    private readonly Func<Task> _execute;
    public RelayCommand(Func<Task> execute) => _execute = execute;
    public event EventHandler? CanExecuteChanged;
    public bool CanExecute(object? parameter) => true;
    public async void Execute(object? parameter) => await _execute();
}