namespace ZeroTrace.Core.Models;

public sealed class ResidualItem
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");

    public string Path { get; set; } = string.Empty;
    public string ItemType { get; set; } = string.Empty;
    public string Source { get; set; } = string.Empty;

    public string? ProgramId { get; set; }
    public double ConfidenceScore { get; set; }

    public long? SizeBytes { get; set; }
    public DateTimeOffset? LastModifiedUtc { get; set; }

    public string? Blake3Hash { get; set; }

    public bool IsLocked { get; set; }
    public bool RequiresReboot { get; set; }

    public Dictionary<string, string> Metadata { get; set; } = new(StringComparer.OrdinalIgnoreCase);
}
