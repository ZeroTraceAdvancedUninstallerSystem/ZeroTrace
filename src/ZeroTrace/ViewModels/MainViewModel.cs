// ZeroTrace - Advanced Uninstaller System
// Copyright (c) 2026 Mario B. | MIT License

using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using ZeroTrace.Core.Discovery;
using ZeroTrace.Core.Logging;
using ZeroTrace.Core.Models;
using ZeroTrace.Core.Security;

namespace ZeroTrace.ViewModels;

public sealed class MainViewModel : INotifyPropertyChanged
{
    private readonly DiscoveryService _discoveryService;
    private readonly IZeroTraceLogger _logger;
    private readonly ObservableCollection<InstalledProgram> _allPrograms = [];

    private bool _isBusy;
    private string _statusMessage = "Bereit.";
    private string _searchText = "";
    private string _currentPageTitle = "Dashboard";
    private string _currentPageSubtitle = "Willkommen bei ZeroTrace";
    private InstalledProgram? _selectedProgram;
    private int _residualCount;
    private string _freedSpace = "0 MB";
    private int _backupCount;

    // ── Collections ──────────────────────────────────────────────
    public ObservableCollection<InstalledProgram> FilteredPrograms { get; } = [];

    // ── Properties ───────────────────────────────────────────────
    public bool IsBusy
    {
        get => _isBusy;
        set { _isBusy = value; Notify(); }
    }

    public string StatusMessage
    {
        get => _statusMessage;
        set { _statusMessage = value; Notify(); }
    }

    public string SearchText
    {
        get => _searchText;
        set { _searchText = value; Notify(); ApplyFilter(); }
    }

    public string CurrentPageTitle
    {
        get => _currentPageTitle;
        set { _currentPageTitle = value; Notify(); }
    }

    public string CurrentPageSubtitle
    {
        get => _currentPageSubtitle;
        set { _currentPageSubtitle = value; Notify(); }
    }

    public InstalledProgram? SelectedProgram
    {
        get => _selectedProgram;
        set { _selectedProgram = value; Notify(); }
    }

    public int ProgramCount => FilteredPrograms.Count;
    public int ResidualCount
    {
        get => _residualCount;
        set { _residualCount = value; Notify(); }
    }

    public string FreedSpace
    {
        get => _freedSpace;
        set { _freedSpace = value; Notify(); }
    }

    public int BackupCount
    {
        get => _backupCount;
        set { _backupCount = value; Notify(); }
    }

    public string PrivilegeDisplay => AdminRightsHelper.IsRunningAsAdmin()
        ? "Administrator" : "Standardbenutzer";

    // ── Commands ─────────────────────────────────────────────────
    public ICommand ScanCommand { get; }
    public ICommand ShowProgramsCommand { get; }
    public ICommand ShowCleanupCommand { get; }
    public ICommand ShowVaultCommand { get; }
    public ICommand ShowSettingsCommand { get; }
    public ICommand ShowAdminCommand { get; }

    // ── Constructor ──────────────────────────────────────────────
    public MainViewModel(DiscoveryService discoveryService, IZeroTraceLogger logger)
    {
        _discoveryService = discoveryService;
        _logger = logger;

        ScanCommand          = new RelayCommand(async () => await PerformScanAsync());
        ShowProgramsCommand  = new RelayCommand(() => { SetPage("Programmliste", "Alle installierten Programme"); return Task.CompletedTask; });
        ShowCleanupCommand   = new RelayCommand(() => { SetPage("System bereinigen", "Temp-Dateien, Browser-Cache, Registry"); return Task.CompletedTask; });
        ShowVaultCommand     = new RelayCommand(() => { SetPage("Vault / Backups", "Gesicherte Daten vor Loeschungen"); return Task.CompletedTask; });
        ShowSettingsCommand  = new RelayCommand(() => { SetPage("Einstellungen", "Sprache, Pfade, Auto-Wartung"); return Task.CompletedTask; });
        ShowAdminCommand     = new RelayCommand(() => { SetPage("Admin-Bereich", "System-Health, Logs, Wartung"); return Task.CompletedTask; });
    }

    // ── Actions ──────────────────────────────────────────────────
    private async Task PerformScanAsync()
    {
        if (IsBusy) return;

        IsBusy = true;
        StatusMessage = "Scanne System...";
        SetPage("Scan-Ergebnisse", "Programme werden gesucht...");

        try
        {
            var programs = await _discoveryService.GetAllInstalledProgramsAsync();

            Application.Current.Dispatcher.Invoke(() =>
            {
                _allPrograms.Clear();
                foreach (var p in programs) _allPrograms.Add(p);
                ApplyFilter();
            });

            StatusMessage = $"Scan abgeschlossen. {programs.Count} Programme gefunden.";
            CurrentPageSubtitle = $"{programs.Count} Programme auf diesem System";
            _logger.Info($"UI-Scan fertig: {programs.Count} Programme");
        }
        catch (Exception ex)
        {
            StatusMessage = $"Fehler: {ex.Message}";
            _logger.Error("Scan fehlgeschlagen", ex);
        }
        finally
        {
            IsBusy = false;
        }
    }

    private void ApplyFilter()
    {
        FilteredPrograms.Clear();
        var query = _searchText.Trim();

        foreach (var p in _allPrograms)
        {
            if (string.IsNullOrEmpty(query)
                || (p.DisplayName?.Contains(query, StringComparison.OrdinalIgnoreCase) ?? false)
                || (p.Publisher?.Contains(query, StringComparison.OrdinalIgnoreCase) ?? false))
            {
                FilteredPrograms.Add(p);
            }
        }

        Notify(nameof(ProgramCount));
    }

    private void SetPage(string title, string subtitle)
    {
        CurrentPageTitle = title;
        CurrentPageSubtitle = subtitle;
    }

    // ── INotifyPropertyChanged ───────────────────────────────────
    public event PropertyChangedEventHandler? PropertyChanged;
    private void Notify([CallerMemberName] string? name = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}

// ── RelayCommand ─────────────────────────────────────────────────
public sealed class RelayCommand : ICommand
{
    private readonly Func<Task> _execute;
    public RelayCommand(Func<Task> execute) => _execute = execute;
    public event EventHandler? CanExecuteChanged;
    public bool CanExecute(object? parameter) => true;
    public async void Execute(object? parameter) => await _execute();
}
