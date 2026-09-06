using System.Collections.ObjectModel;
using OpenForge.Cli.Core.Commands.Update.Models.Comparison;
using OpenForge.Cli.Core.Commands.Update.Models.Effects;
using OpenForge.Cli.Core.Commands.Update.Models.Request;
using OpenForge.Cli.Core.Framework.Lifecycle;
using OpenForge.Cli.Core.Framework.Workspace;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Pipeline;

namespace OpenForge.Cli.Core.Commands.Update.Models.Result;

internal enum UpdateLifecycleTrust
{
    NotRequested,
    Trusted,
    Unavailable,
    Blocked,
}

internal enum UpdateLifecycleCoverage
{
    NotRequested,
    Complete,
    Incomplete,
    Blocked,
}

internal enum UpdateLifecycleAction
{
    None,
    Preserve,
    Publish,
}

internal enum UpdateLifecycleOutcome
{
    NotRequested,
    Planned,
    AlreadyCurrent,
    NotStarted,
    Verified,
    VerificationFailed,
    CompletionUnknown,
}

internal enum UpdateGeneratedNavigationCoverage
{
    Complete,
    Incomplete,
    Blocked,
}

internal enum UpdateGeneratedNavigationRegionState
{
    Unchanged,
    Changed,
    New,
    Retired,
    Unavailable,
    Blocked,
}

internal enum UpdateRecoveryState
{
    NotRequired,
    NotCreated,
    Removed,
    Retained,
    Unknown,
}

internal enum UpdateVerificationState
{
    NotRequested,
    Verified,
    Failed,
    Unknown,
}

internal sealed record UpdateSource
{
    public required string Id { get; init; }

    public required string? Version { get; init; }

    public required string InventoryFingerprint { get; init; }

    public required int AssetCount { get; init; }

    internal void Validate()
    {
        if (!string.Equals(Id, "framework", StringComparison.Ordinal))
        {
            throw new ArgumentException("Update source identity must be framework.", nameof(Id));
        }
        if (!IsSha256(InventoryFingerprint))
        {
            throw new ArgumentException("Update source inventory identity must be lowercase SHA-256.", nameof(InventoryFingerprint));
        }
        if (AssetCount < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(AssetCount), AssetCount, "Update source asset count cannot be negative.");
        }
    }

    private static bool IsSha256(string value)
        => value.Length == 64
            && value.All(character => character is >= '0' and <= '9' or >= 'a' and <= 'f');
}

internal sealed record UpdateGeneratedNavigationRegion
{
    public required string Path { get; init; }

    public required UpdateGeneratedNavigationRegionState State { get; init; }
}

internal sealed record UpdateGeneratedNavigation
{
    public required UpdateGeneratedNavigationCoverage Coverage { get; init; }

    public required IReadOnlyList<UpdateGeneratedNavigationRegion> Regions { get; init; }
}

internal sealed record UpdateLifecycle
{
    public required UpdateLifecycleTrust Trust { get; init; }

    public required UpdateLifecycleCoverage Coverage { get; init; }

    public required UpdateLifecycleAction Action { get; init; }

    public required UpdateLifecycleOutcome Outcome { get; init; }
}

internal sealed record UpdateRecovery
{
    public required UpdateRecoveryState State { get; init; }

    public required IReadOnlyList<string> ProtectedPaths { get; init; }

    public required string? ResidualPath { get; init; }
}

internal sealed class UpdateFinding
{
    internal UpdateFinding(
        UpdateFindingCode code,
        string? target,
        string cause)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(cause);
        if (target is not null && target.Length == 0)
        {
            throw new ArgumentException("An Update finding target cannot be empty.", nameof(target));
        }

        Code = code;
        Status = UpdateDefinitions.ReadStatus(code);
        Target = target;
        Cause = cause;
    }

    internal UpdateFindingCode Code { get; }

    internal CliSemanticStatus Status { get; }

    internal string? Target { get; }

    internal string Cause { get; }
}

internal sealed record UpdateResultFormation
{
    public required CliWorkspace? Workspace { get; init; }

    public required UpdateMode Mode { get; init; }

    public required bool Force { get; init; }

    public required bool Prune { get; init; }

    public required bool Automatic { get; init; }

    public required UpdateSource? Source { get; init; }

    public required IReadOnlyList<UpdateComparison> Comparisons { get; init; }

    public required UpdateGeneratedNavigation? GeneratedNavigation { get; init; }

    public required IReadOnlyList<UpdatePhysicalEffect> Effects { get; init; }

    public required UpdateLifecycle Lifecycle { get; init; }

    public required UpdateRecovery Recovery { get; init; }

    public required UpdateVerificationState Verification { get; init; }

    public required IReadOnlyList<UpdateFinding> Findings { get; init; }
}

internal sealed record UpdateResult : ICliCommandResult
{
    internal UpdateResult(UpdateResultFormation formation)
    {
        ArgumentNullException.ThrowIfNull(formation);
        if (!Enum.IsDefined(formation.Mode))
        {
            throw new ArgumentOutOfRangeException(nameof(formation.Mode), formation.Mode, "The Update mode is not defined.");
        }

        ArgumentNullException.ThrowIfNull(formation.Comparisons);
        ArgumentNullException.ThrowIfNull(formation.Effects);
        ArgumentNullException.ThrowIfNull(formation.Findings);
        ArgumentNullException.ThrowIfNull(formation.Lifecycle);
        ArgumentNullException.ThrowIfNull(formation.Recovery);
        var comparisons = NormalizeComparisons(formation.Comparisons);
        var effects = NormalizeEffects(formation.Effects);
        var findings = formation.Findings
            .Select(finding => finding ?? throw new ArgumentException("Update findings cannot contain null members."))
            .OrderBy(finding => (int)finding.Code)
            .ThenBy(finding => finding.Target is null ? 0 : 1)
            .ThenBy(finding => finding.Target, StringComparer.Ordinal)
            .ThenBy(finding => finding.Cause, StringComparer.Ordinal)
            .ToArray();

        Source = formation.Source;
        Source?.Validate();
        ValidateLifecycle(formation.Lifecycle);
        var recovery = NormalizeRecovery(formation.Recovery, effects);
        var generatedNavigation = NormalizeGeneratedNavigation(formation.GeneratedNavigation);
        ValidateVerification(formation.Verification);

        Workspace = formation.Workspace;
        Mode = formation.Mode;
        Force = formation.Force;
        Prune = formation.Prune;
        Automatic = formation.Automatic;
        Comparisons = new ReadOnlyCollection<UpdateComparison>(comparisons);
        GeneratedNavigation = generatedNavigation;
        Effects = new ReadOnlyCollection<UpdatePhysicalEffect>(effects);
        Lifecycle = formation.Lifecycle;
        Recovery = recovery;
        Verification = formation.Verification;
        Findings = new ReadOnlyCollection<UpdateFinding>(findings);
        Status = ReadStatus(Findings);
        Next = UpdateDefinitions.ReadNextAction(Status, Findings, Force, Prune, Automatic, Mode);
    }

    public string Command => UpdateDefinitions.CommandIdentity;

    public CliSemanticStatus Status { get; }

    public CliWorkspace? Workspace { get; }

    public CliNextAction? Next { get; }

    internal UpdateMode Mode { get; }

    internal bool Force { get; }

    internal bool Prune { get; }

    internal bool Automatic { get; }

    internal UpdateSource? Source { get; }

    internal IReadOnlyList<UpdateComparison> Comparisons { get; }

    internal UpdateGeneratedNavigation? GeneratedNavigation { get; }

    internal IReadOnlyList<UpdatePhysicalEffect> Effects { get; }

    internal UpdateLifecycle Lifecycle { get; }

    internal UpdateRecovery Recovery { get; }

    internal UpdateVerificationState Verification { get; }

    internal IReadOnlyList<UpdateFinding> Findings { get; }

    private static CliSemanticStatus ReadStatus(IReadOnlyList<UpdateFinding> findings)
    {
        var precedence = new[]
        {
            CliSemanticStatus.Failed,
            CliSemanticStatus.Interrupted,
            CliSemanticStatus.Invalid,
            CliSemanticStatus.Blocked,
            CliSemanticStatus.Incomplete,
            CliSemanticStatus.Attention,
        };
        return precedence.FirstOrDefault(
            status => findings.Any(finding => finding.Status == status),
            CliSemanticStatus.Complete);
    }

    private static UpdateComparison[] NormalizeComparisons(IReadOnlyList<UpdateComparison> comparisons)
    {
        var materialized = comparisons
            .Select(comparison => comparison ?? throw new ArgumentException("Update comparisons cannot contain null members."))
            .ToArray();
        var identities = new HashSet<(
            string Path,
            UpdateComparisonTargetKind Kind,
            string? Region)>();
        foreach (var comparison in materialized)
        {
            comparison.Validate();
            if (!identities.Add((comparison.RelativePath, comparison.Kind, comparison.RegionIdentity)))
            {
                throw new ArgumentException("Update comparisons cannot contain duplicate target identities.", nameof(comparisons));
            }
        }

        return materialized
            .OrderBy(comparison => comparison.RelativePath, StringComparer.Ordinal)
            .ThenBy(comparison => comparison.Kind)
            .ThenBy(comparison => comparison.RegionIdentity ?? string.Empty, StringComparer.Ordinal)
            .ToArray();
    }

    private static UpdatePhysicalEffect[] NormalizeEffects(IReadOnlyList<UpdatePhysicalEffect> effects)
    {
        var materialized = effects
            .Select(effect => effect ?? throw new ArgumentException("Update effects cannot contain null members."))
            .ToArray();
        var paths = new HashSet<string>(StringComparer.Ordinal);
        foreach (var effect in materialized)
        {
            ValidateEffect(effect);
            if (!paths.Add(effect.Path))
            {
                throw new ArgumentException("Update effects cannot contain duplicate physical paths.", nameof(effects));
            }
        }

        return materialized
            .OrderBy(effect => effect.Path, StringComparer.Ordinal)
            .ToArray();
    }

    private static void ValidateEffect(UpdatePhysicalEffect effect)
    {
        if (!IsCanonicalRelative(effect.Path))
        {
            throw new ArgumentException("Update effect paths must be canonical workspace-relative paths.");
        }
        if (effect.Changes.Count == 0)
        {
            throw new ArgumentException("Update effects require logical changes.");
        }

        var identities = new HashSet<(
            UpdateComparisonTargetKind Kind,
            string? Region,
            string? SourceAssetPath)>();
        UpdateLogicalChange? prior = null;
        foreach (var change in effect.Changes)
        {
            change.Validate();
            if (!identities.Add((change.Kind, change.Region, change.SourceAssetPath)))
            {
                throw new ArgumentException("Update effects cannot contain duplicate logical identities.");
            }
            if (prior is not null && CompareLogicalChanges(prior, change) > 0)
            {
                throw new ArgumentException("Update logical changes must use deterministic order.");
            }

            prior = change;
        }

        UpdateDefinitions.ReadMachineName(effect.Action);
        UpdateDefinitions.ReadMachineName(effect.Outcome);
        UpdateDefinitions.ReadMachineName(effect.Residual);
    }

    private static void ValidateLifecycle(UpdateLifecycle lifecycle)
    {
        UpdateDefinitions.ReadMachineName(lifecycle.Trust);
        UpdateDefinitions.ReadMachineName(lifecycle.Coverage);
        UpdateDefinitions.ReadMachineName(lifecycle.Action);
        UpdateDefinitions.ReadMachineName(lifecycle.Outcome);
    }

    private static UpdateRecovery NormalizeRecovery(
        UpdateRecovery recovery,
        IReadOnlyList<UpdatePhysicalEffect> effects)
    {
        ArgumentNullException.ThrowIfNull(recovery.ProtectedPaths);
        UpdateDefinitions.ReadMachineName(recovery.State);
        if (recovery.State is UpdateRecoveryState.Retained && string.IsNullOrWhiteSpace(recovery.ResidualPath))
        {
            throw new ArgumentException("A retained Update recovery artifact requires a residual path.");
        }
        if (recovery.State is UpdateRecoveryState.NotRequired or UpdateRecoveryState.NotCreated or UpdateRecoveryState.Removed)
        {
            if (recovery.ResidualPath is not null)
            {
                throw new ArgumentException("A recovery state without a residual cannot expose a residual path.");
            }
        }

        var protectedPaths = recovery.ProtectedPaths
            .Select(path => path ?? throw new ArgumentException(
                "Update recovery protected paths cannot contain null members.",
                nameof(recovery.ProtectedPaths)))
            .ToArray();
        var paths = new HashSet<string>(StringComparer.Ordinal);
        foreach (var path in protectedPaths)
        {
            if (!IsCanonicalRelative(path))
            {
                throw new ArgumentException(
                    "Update recovery protected paths must be canonical workspace-relative paths.",
                    nameof(recovery.ProtectedPaths));
            }
            if (!paths.Add(path))
            {
                throw new ArgumentException(
                    "Update recovery protected paths cannot contain duplicates.",
                    nameof(recovery.ProtectedPaths));
            }
        }

        var existingEffectOrder = effects
            .Where(effect => effect.Action is UpdatePhysicalEffectAction.Replace or UpdatePhysicalEffectAction.Delete)
            .Select((effect, index) => (effect.Path, index))
            .ToDictionary(item => item.Path, item => item.index, StringComparer.Ordinal);
        var ordered = protectedPaths
            .OrderBy(path => path == LifecycleSchema.RelativePath ? 1 : 0)
            .ThenBy(
                path => existingEffectOrder.TryGetValue(path, out var index)
                    ? index
                    : int.MaxValue)
            .ThenBy(path => path, StringComparer.Ordinal)
            .ToArray();
        return recovery with
        {
            ProtectedPaths = new ReadOnlyCollection<string>(ordered),
        };
    }

    private static UpdateGeneratedNavigation? NormalizeGeneratedNavigation(UpdateGeneratedNavigation? navigation)
    {
        if (navigation is null)
        {
            return null;
        }
        UpdateDefinitions.ReadMachineName(navigation.Coverage);
        ArgumentNullException.ThrowIfNull(navigation.Regions);
        var regions = navigation.Regions
            .Select(region => region ?? throw new ArgumentException(
                "Generated navigation regions cannot contain null members.",
                nameof(navigation.Regions)))
            .ToArray();
        var paths = new HashSet<string>(StringComparer.Ordinal);
        foreach (var region in regions)
        {
            if (!IsCanonicalRelative(region.Path))
            {
                throw new ArgumentException("Generated navigation paths must be canonical workspace-relative paths.");
            }
            if (!paths.Add(region.Path))
            {
                throw new ArgumentException("Generated navigation paths cannot contain duplicates.");
            }
            UpdateDefinitions.ReadMachineName(region.State);
        }

        return navigation with
        {
            Regions = new ReadOnlyCollection<UpdateGeneratedNavigationRegion>(
                regions.OrderBy(region => region.Path, StringComparer.Ordinal).ToArray()),
        };
    }

    private static void ValidateVerification(UpdateVerificationState verification)
        => UpdateDefinitions.ReadMachineName(verification);

    private static int CompareLogicalChanges(UpdateLogicalChange left, UpdateLogicalChange right)
    {
        var kind = left.Kind.CompareTo(right.Kind);
        if (kind != 0)
        {
            return kind;
        }

        var region = string.Compare(
            left.Region ?? string.Empty,
            right.Region ?? string.Empty,
            StringComparison.Ordinal);
        if (region != 0)
        {
            return region;
        }

        return string.Compare(
            left.SourceAssetPath ?? string.Empty,
            right.SourceAssetPath ?? string.Empty,
            StringComparison.Ordinal);
    }

    private static bool IsCanonicalRelative(string? value)
        => !string.IsNullOrWhiteSpace(value)
            && !value.StartsWith('/')
            && !IsDriveQualified(value)
            && !value.Contains('\\')
            && value.Split('/', StringSplitOptions.None).All(segment => segment.Length != 0
                && segment != "."
                && segment != ".."
                && segment.All(character => !char.IsControl(character)));

    private static bool IsDriveQualified(string value)
        => value.Length >= 2
            && char.IsAsciiLetter(value[0])
            && value[1] == ':';
}
