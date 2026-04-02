namespace ZeroTrace.Core.Logging;

public sealed class FileLogger : IZeroTraceLogger
{
    public ValueTask TraceAsync(string message, CancellationToken cancellationToken = default)
    {
        Console.WriteLine($"[TRACE] {message}");
        return ValueTask.CompletedTask;
    }

    public ValueTask DebugAsync(string message, CancellationToken cancellationToken = default)
    {
        Console.WriteLine($"[DEBUG] {message}");
        return ValueTask.CompletedTask;
    }

    public ValueTask InfoAsync(string message, CancellationToken cancellationToken = default)
    {
        Console.WriteLine($"[INFO] {message}");
        return ValueTask.CompletedTask;
    }

    public ValueTask WarningAsync(string message, CancellationToken cancellationToken = default)
    {
        Console.WriteLine($"[WARN] {message}");
        return ValueTask.CompletedTask;
    }

    public ValueTask ErrorAsync(string message, CancellationToken cancellationToken = default)
    {
        Console.WriteLine($"[ERROR] {message}");
        return ValueTask.CompletedTask;
    }

    public ValueTask ErrorAsync(Exception exception, string message, CancellationToken cancellationToken = default)
    {
        Console.WriteLine($"[ERROR] {message} | {exception}");
        return ValueTask.CompletedTask;
    }

    public ValueTask CriticalAsync(string message, CancellationToken cancellationToken = default)
    {
        Console.WriteLine($"[CRITICAL] {message}");
        return ValueTask.CompletedTask;
    }
}
