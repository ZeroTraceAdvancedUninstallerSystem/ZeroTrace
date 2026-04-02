// ZeroTrace - Advanced Uninstaller System
// Copyright (c) 2026 Mario B. | MIT License

namespace ZeroTrace.Core.Shredder;

/// <summary>
/// Defines how aggressively files are overwritten before deletion.
/// Higher methods are slower but more secure.
/// </summary>
public enum ShredMethod
{
    /// <summary>Single pass with random data. Fast, sufficient for SSDs.</summary>
    SinglePass = 1,

    /// <summary>3 passes: zeros, ones, random. Good balance of speed and security.</summary>
    ThreePass = 3,

    /// <summary>7-pass DoD 5220.22-M standard. High security for HDDs.</summary>
    DoD7Pass = 7
}

/// <summary>Result of a shred operation.</summary>
public sealed class ShredResult
{
    public required string FilePath     { get; init; }
    public required bool   Success      { get; init; }
    public required ShredMethod Method  { get; init; }
    public required int    PassesUsed   { get; init; }
    public required long   BytesShredded { get; init; }
    public          string? ErrorMessage { get; init; }
    public required TimeSpan Duration   { get; init; }
}
