// ZeroTrace - Advanced Uninstaller System
// Copyright (c) 2026 Mario B. | MIT License

using System.Collections.Concurrent;
using System.Text;

namespace ZeroTrace.Core.Logging;

public sealed class FileLogger : IZeroTraceLogger, IAsyncDisposable
{
    private readonly string                     _logFilePath;
    private readonly LogLevel                   _minimumLevel;
    private readonly ConcurrentQueue<string>    _queue = new();
    private readonly SemaphoreSlim              _writeLock = new(1, 1);
    private readonly CancellationTokenSource    _cts = new();
    private readonly Task                       _flushTask;

    public FileLogger(string? logDirectory = null, LogLevel minimumLevel = LogLevel.Info)
    {
        _minimumLevel = minimumLevel;
        var directory = logDirectory
            ?? Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "ZeroTrace", "Logs");
        Directory.CreateDirectory(directory);
        _logFilePath = Path.Combine(directory, $"ZeroTrace_{DateTime.Now:yyyy-MM-dd}.log");
        _flushTask = Task.Run(FlushLoopAsync);
    }

    public void Debug(string message)                        => Enqueue(LogLevel.Debug, message);
    public void Info(string message)                         => Enqueue(LogLevel.Info, message);
    public void Warning(string message)                      => Enqueue(LogLevel.Warning, message);
    public void Error(string message, Exception? ex = null)  =>
        Enqueue(LogLevel.Error, ex is null
            ? message
            : $"{message} | {ex.GetType().Name}: {ex.Message}\n  StackTrace: {ex.StackTrace}");

    private void Enqueue(LogLevel level, string message)
    {
        if (level < _minimumLevel) return;
        var line = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] [{level.ToString().ToUpperInvariant(),-7}] [T{Environment.CurrentManagedThreadId:D3}] {message}";
        _queue.Enqueue(line);
    }

    private async Task FlushLoopAsync()
    {
        while (!_cts.Token.IsCancellationRequested)
        {
            try  { await Task.Delay(500, _cts.Token); await FlushAsync(); }
            catch (OperationCanceledException) { break; }
            catch { }
        }
        await FlushAsync();
    }

    private async Task FlushAsync()
    {
        if (_queue.IsEmpty) return;
        await _writeLock.WaitAsync();
        try
        {
            var sb = new StringBuilder(4096);
            while (_queue.TryDequeue(out var msg)) sb.AppendLine(msg);
            if (sb.Length > 0)
                await File.AppendAllTextAsync(_logFilePath, sb.ToString());
        }
        finally { _writeLock.Release(); }
    }

    public string GetCurrentLogFilePath() => _logFilePath;

    public async ValueTask DisposeAsync()
    {
        await _cts.CancelAsync();
        try { await _flushTask; } catch (OperationCanceledException) { }
        _cts.Dispose();
        _writeLock.Dispose();
    }
}
