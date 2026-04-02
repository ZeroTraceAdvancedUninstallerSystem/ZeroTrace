// ZeroTrace - Advanced Uninstaller System
// Copyright (c) 2026 Mario B. | MIT License

using ZeroTrace.Core.Logging;
using ZeroTrace.Core.Models;
using ZeroTrace.Core.Security;

namespace ZeroTrace.Core.AI;

/// <summary>
/// Performs a final risk assessment before any deletion operation.
/// Acts as a safety layer between the user's selection and the actual cleanup.
/// Checks for protected paths, shared dependencies, and critical files.
/// </summary>
public sealed class RiskAssessment
{
    private readonly IZeroTraceLogger _logger;
    private readonly SystemGuard _guard;

    // File extensions that indicate shared runtime components
    private static readonly HashSet<string> SharedComponentExtensions =
        new(StringComparer.OrdinalIgnoreCase)
    {
        ".dll", ".ocx", ".tlb", ".olb"
    };

    // Known shared runtime folders
    private static readonly string[] SharedRuntimePaths =
    [
        @"Microsoft Shared",
        @"Common Files",
        @"Windows Kits",
        @"dotnet\shared",
        @"Microsoft.NET",
        @"Microsoft\VisualStudio",
    ];

    public RiskAssessment(IZeroTraceLogger logger, SystemGuard guard)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _guard = guard ?? throw new ArgumentNullException(nameof(guard));
    }

    /// <summary>
    /// Assess risk for a list of items before deletion.
    /// Returns items categorized by risk level with recommendations.
    /// </summary>
    public RiskReport AssessItems(IReadOnlyList<ResidualItem> items)
    {
        _logger.Info($"Risiko-Bewertung: {items.Count} Elemente");

        var assessments = new List<ItemRiskAssessment>();
        int blocked = 0, warned = 0, safe = 0;

        foreach (var item in items)
        {
            var assessment = AssessSingleItem(item);
            assessments.Add(assessment);

            switch (assessment.Decision)
            {
                case RiskDecision.Block: blocked++; break;
                case RiskDecision.Warn: warned++; break;
                case RiskDecision.Safe: safe++; break;
            }
        }

        _logger.Info($"  Risiko-Ergebnis: {safe} sicher, {warned} Warnung, {blocked} blockiert");

        return new RiskReport
        {
            TotalItems = items.Count,
            SafeCount = safe,
            WarnCount = warned,
            BlockedCount = blocked,
            Assessments = assessments.AsReadOnly(),
            OverallRisk = blocked > 0 ? OverallRisk.Dangerous
                        : warned > items.Count / 2 ? OverallRisk.Risky
                        : OverallRisk.Safe
        };
    }

    private ItemRiskAssessment AssessSingleItem(ResidualItem item)
    {
        var reasons = new List<string>();

        // Check 1: SystemGuard (absolute block)
        if (item.ItemType is ResidualItemType.File or ResidualItemType.Directory)
        {
            if (!_guard.IsPathSafe(item.FullPath))
            {
                reasons.Add("Systemkritischer Pfad (SystemGuard)");
                return MakeAssessment(item, RiskDecision.Block, reasons);
            }
        }
        else if (item.ItemType is ResidualItemType.RegistryKey or ResidualItemType.RegistryValue)
        {
            if (!_guard.IsRegistryPathSafe(item.FullPath))
            {
                reasons.Add("Systemkritischer Registry-Schluessel");
                return MakeAssessment(item, RiskDecision.Block, reasons);
            }
        }

        // Check 2: Shared components
        if (IsSharedComponent(item.FullPath))
        {
            reasons.Add("Moeglicherweise von anderen Programmen genutzt");
            return MakeAssessment(item, RiskDecision.Warn, reasons);
        }

        // Check 3: Low confidence
        if (item.ConfidenceScore < 0.4)
        {
            reasons.Add($"Niedrige Konfidenz ({item.ConfidenceScore:P0})");
            return MakeAssessment(item, RiskDecision.Warn, reasons);
        }

        // Check 4: Very large items (> 500 MB)
        if (item.SizeInBytes > 500L * 1024 * 1024)
        {
            reasons.Add("Sehr grosse Datei/Ordner (>500 MB) - bitte manuell pruefen");
            return MakeAssessment(item, RiskDecision.Warn, reasons);
        }

        // All checks passed
        reasons.Add("Alle Pruefungen bestanden");
        return MakeAssessment(item, RiskDecision.Safe, reasons);
    }

    private static bool IsSharedComponent(string path)
    {
        var lower = path.ToLowerInvariant();

        // Check shared runtime paths
        if (SharedRuntimePaths.Any(p => lower.Contains(p.ToLowerInvariant())))
            return true;

        // Check shared DLLs in common folders
        var ext = Path.GetExtension(path);
        if (SharedComponentExtensions.Contains(ext))
        {
            if (lower.Contains(@"\common files\") || lower.Contains(@"\shared\"))
                return true;
        }

        return false;
    }

    private static ItemRiskAssessment MakeAssessment(
        ResidualItem item, RiskDecision decision, List<string> reasons) => new()
    {
        ItemPath = item.FullPath,
        ItemType = item.ItemType,
        OriginalConfidence = item.ConfidenceScore,
        Decision = decision,
        Reasons = reasons.AsReadOnly()
    };
}

// ── Report Types ─────────────────────────────────────────────────

public sealed class RiskReport
{
    public required int    TotalItems   { get; init; }
    public required int    SafeCount    { get; init; }
    public required int    WarnCount    { get; init; }
    public required int    BlockedCount { get; init; }
    public required OverallRisk OverallRisk { get; init; }
    public required IReadOnlyList<ItemRiskAssessment> Assessments { get; init; }

    public string OverallDisplay => OverallRisk switch
    {
        OverallRisk.Safe      => "Sicher - Alle Elemente koennen geloescht werden",
        OverallRisk.Risky     => "Vorsicht - Einige Elemente erfordern Pruefung",
        OverallRisk.Dangerous => "Gefahr - Systemkritische Elemente erkannt",
        _ => "Unbekannt"
    };
}

public sealed class ItemRiskAssessment
{
    public required string           ItemPath           { get; init; }
    public required ResidualItemType ItemType           { get; init; }
    public required double           OriginalConfidence { get; init; }
    public required RiskDecision     Decision           { get; init; }
    public required IReadOnlyList<string> Reasons       { get; init; }
}

public enum RiskDecision { Safe, Warn, Block }
public enum OverallRisk { Safe, Risky, Dangerous }
