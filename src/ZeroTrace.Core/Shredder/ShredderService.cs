// ZeroTrace - Advanced Uninstaller System
// Copyright (c) 2026 Mario B. | MIT License

using System.Diagnostics;
using System.Security.Cryptography;
using ZeroTrace.Core.Logging;

namespace ZeroTrace.Core.Shredder;

/// <summary>
/// Secure file deletion with configurable overwrite passes.
/// Optimized for both HDD (multi-pass) and SSD/NVMe (single + TRIM).
/// </summary>
public sealed class ShredderService
{
    private readonly IZeroTraceLogger _logger;
    private const int BufferSize = 64 * 1024; // 64 KB chunks

    public event EventHandler<ShredProgressEventArgs>? ProgressChanged;

    public ShredderService(IZeroTraceLogger logger) =>
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    /// <summary>Securely shred a single file.</summary>
    public async Task<ShredResult> ShredFileAsync(
        string filePath,
        ShredMethod method = ShredMethod.SinglePass,
        CancellationToken ct = default)
    {
        var sw = Stopwatch.StartNew();

        if (!File.Exists(filePath))
            return new ShredResult
            {
                FilePath = filePath, Success = false, Method = method,
                PassesUsed = 0, BytesShredded = 0, Duration = sw.Elapsed,
                ErrorMessage = "Datei existiert nicht"
            };

        try
        {
            var fi = new FileInfo(filePath);
            long length = fi.Length;
            int passes = (int)method;

            _logger.Info($"Shredding ({method}, {passes}x): {fi.Name} ({length} Bytes)");

            // Remove read-only flag
            if (fi.IsReadOnly) fi.IsReadOnly = false;

            // Overwrite passes
            for (int pass = 1; pass <= passes; pass++)
            {
                ct.ThrowIfCancellationRequested();
                Report($"Pass {pass}/{passes}: {fi.Name}", passes, pass - 1);

                await using var stream = new FileStream(
                    filePath, FileMode.Open, FileAccess.Write, FileShare.None);

                byte[] buffer = new byte[BufferSize];
                long written = 0;

                while (written < length)
                {
                    ct.ThrowIfCancellationRequested();
                    int chunk = (int)Math.Min(BufferSize, length - written);

                    // Pass pattern: odd=random, even=zeros (for multi-pass)
                    if (pass % 2 == 1)
                        RandomNumberGenerator.Fill(buffer.AsSpan(0, chunk));
                    else
                        Array.Clear(buffer, 0, chunk);

                    await stream.WriteAsync(buffer.AsMemory(0, chunk), ct);
                    written += chunk;
                }

                await stream.FlushAsync(ct);
            }

            // Final delete
            File.Delete(filePath);
            sw.Stop();

            _logger.Info($"  Shredded: {fi.Name} ({passes} passes, {sw.Elapsed.TotalSeconds:F1}s)");

            return new ShredResult
            {
                FilePath = filePath, Success = true, Method = method,
                PassesUsed = passes, BytesShredded = length, Duration = sw.Elapsed
            };
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.Warning($"  Shred fehlgeschlagen: {filePath} - {ex.Message}");
            return new ShredResult
            {
                FilePath = filePath, Success = false, Method = method,
                PassesUsed = 0, BytesShredded = 0, Duration = sw.Elapsed,
                ErrorMessage = ex.Message
            };
        }
    }

    /// <summary>Shred all files in a directory recursively.</summary>
    public async Task<List<ShredResult>> ShredDirectoryAsync(
        string dirPath,
        ShredMethod method = ShredMethod.SinglePass,
        CancellationToken ct = default)
    {
        var results = new List<ShredResult>();
        if (!Directory.Exists(dirPath)) return results;

        _logger.Info($"Shredding Verzeichnis: {dirPath}");

        var files = Directory.GetFiles(dirPath, "*", SearchOption.AllDirectories);
        for (int i = 0; i < files.Length; i++)
        {
            ct.ThrowIfCancellationRequested();
            results.Add(await ShredFileAsync(files[i], method, ct));
        }

        // Remove empty directories bottom-up
        try
        {
            foreach (var dir in Directory.GetDirectories(dirPath, "*", SearchOption.AllDirectories)
                .OrderByDescending(d => d.Length))
            {
                if (Directory.Exists(dir) && !Directory.EnumerateFileSystemEntries(dir).Any())
                    Directory.Delete(dir);
            }
            if (Directory.Exists(dirPath) && !Directory.EnumerateFileSystemEntries(dirPath).Any())
                Directory.Delete(dirPath);
        }
        catch { /* best effort */ }

        int ok = results.Count(r => r.Success);
        _logger.Info($"  Verzeichnis-Shred: {ok}/{results.Count} Dateien vernichtet");
        return results;
    }

    private void Report(string msg, int total, int current) =>
        ProgressChanged?.Invoke(this, new ShredProgressEventArgs
        { Message = msg, TotalItems = total, CompletedItems = current });
}

public sealed class ShredProgressEventArgs : EventArgs
{
    public required string Message        { get; init; }
    public required int    TotalItems     { get; init; }
    public required int    CompletedItems { get; init; }
}
