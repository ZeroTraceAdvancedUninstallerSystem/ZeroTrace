namespace ZeroTrace.Core.Logging;

public interface IZeroTraceLogger
{
    ValueTask TraceAsync(string message, CancellationToken cancellationToken = default);
    ValueTask DebugAsync(string message, CancellationToken cancellationToken = default);
    ValueTask InfoAsync(string message, CancellationToken cancellationToken = default);
    ValueTask WarningAsync(string message, CancellationToken cancellationToken = default);
    ValueTask ErrorAsync(string message, CancellationToken cancellationToken = default);
    ValueTask ErrorAsync(Exception exception, string message, CancellationToken cancellationToken = default);
    ValueTask CriticalAsync(string message, CancellationToken cancellationToken = default);
}
