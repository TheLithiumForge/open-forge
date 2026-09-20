using System.Collections.ObjectModel;
using OpenForge.Cli.Core.Commands.Index.Models.Operation;
using OpenForge.Cli.Core.Commands.Index.Models.Planning;
using OpenForge.Cli.Core.Commands.Index.Models.Request;
using OpenForge.Cli.Core.Commands.Index.Models.Selection;
using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Operation;

namespace OpenForge.Cli.Core.Commands.Index.Models.Result;

internal sealed record IndexCounts
{
    internal IndexCounts(IReadOnlyList<IndexRegion> regions)
    {
        ArgumentNullException.ThrowIfNull(regions);
        Regions = regions.Count;
        Updates = regions.Count(region => region.Action == IndexRegionAction.Update);
        Unchanged = regions.Count(region => region.Action == IndexRegionAction.Unchanged);
        Applied = regions.Count(region => region.Outcome is IndexRegionOutcome.Applied or IndexRegionOutcome.Verified);
        Verified = regions.Count(region => region.Outcome is IndexRegionOutcome.AlreadyCurrent or IndexRegionOutcome.Verified);
    }

    internal int Regions { get; }

    internal int Updates { get; }

    internal int Unchanged { get; }

    internal int Applied { get; }

    internal int Verified { get; }
}

internal sealed record IndexResult : ICliCommandResult
{
    internal IndexResult(IndexResultFormation formation)
    {
        ArgumentNullException.ThrowIfNull(formation);
        if (!Enum.IsDefined(formation.Mode))
        {
            throw new ArgumentOutOfRangeException(nameof(formation), formation.Mode, "The Index mode is not defined.");
        }

        ArgumentNullException.ThrowIfNull(formation.Selection);
        ArgumentNullException.ThrowIfNull(formation.Regions);
        ArgumentNullException.ThrowIfNull(formation.Recovery);
        ArgumentNullException.ThrowIfNull(formation.Findings);
        var orderedRegions = formation.Regions
            .Select(region => region ?? throw new ArgumentException("Index regions cannot contain null members.", nameof(formation)))
            .OrderBy(region => region.Source.Path, StringComparer.Ordinal)
            .ThenBy(region => region.Source.Id, StringComparer.Ordinal)
            .ToArray();
        if (orderedRegions.Select(region => region.Source.Path).Distinct(StringComparer.Ordinal).Count() != orderedRegions.Length)
        {
            throw new ArgumentException("Index regions require unique canonical source paths.", nameof(formation));
        }

        var findingValues = formation.Findings
            .Select(finding => finding ?? throw new ArgumentException(
                "Index findings cannot contain null members.",
                nameof(formation)))
            .ToArray();
        var orderedFindings = OrderFindings(findingValues).ToArray();
        if (orderedFindings.Any(finding => finding.Status == CliSemanticStatus.Complete))
        {
            throw new ArgumentException("Index findings cannot have complete status.", nameof(formation));
        }

        var status = ReadStatus(orderedFindings);
        ValidateRecovery(formation.Recovery, orderedFindings, status);
        ValidateModeAndRegions(formation, status, orderedRegions);
        Workspace = formation.Workspace;
        Mode = formation.Mode;
        Selection = formation.Selection;
        Regions = new ReadOnlyCollection<IndexRegion>(orderedRegions);
        Recovery = formation.Recovery;
        Findings = new ReadOnlyCollection<IndexFinding>(orderedFindings);
        Counts = new IndexCounts(Regions);
        Status = status;
        Next = IndexDefinitions.ReadNextAction(status, Findings);
    }

    public string Command => IndexDefinitions.CommandIdentity;

    public CliSemanticStatus Status { get; }

    public CliWorkspace? Workspace { get; }

    internal string? WorkspacePath => Workspace?.LexicalRoot;

    internal bool WorkspaceExplicit => Workspace?.SelectedBy == CliWorkspaceSelectionMethod.ExplicitWorkspace;

    public CliNextAction? Next { get; }

    internal IndexMode Mode { get; }

    internal IndexSelection Selection { get; }

    internal IReadOnlyList<IndexRegion> Regions { get; }

    internal IndexRecovery Recovery { get; }

    internal IReadOnlyList<IndexFinding> Findings { get; }

    internal IndexCounts Counts { get; }

    internal static IEnumerable<IndexFinding> OrderFindings(IEnumerable<IndexFinding> findings)
        => findings
            .OrderBy(finding => finding.Code)
            .ThenBy(finding => finding.SourceOccurrence.HasValue ? 1 : 0)
            .ThenBy(finding => finding.SourceOccurrence)
            .ThenBy(finding => finding.Source?.Path, StringComparer.Ordinal)
            .ThenBy(finding => finding.Source?.Id, StringComparer.Ordinal)
            .ThenBy(finding => finding.Cause, StringComparer.Ordinal);

    private static CliSemanticStatus ReadStatus(IReadOnlyList<IndexFinding> findings)
        => CliStatusDefinitions.Collapse(findings.Select(finding => finding.Status));

    private static void ValidateRecovery(
        IndexRecovery recovery,
        IReadOnlyList<IndexFinding> findings,
        CliSemanticStatus status)
    {
        var retainedFinding = findings.Any(finding => finding.Code == IndexFindingCode.RecoveryArtifactRetained);
        var skippedFinding = findings.Any(finding => finding.Code == IndexFindingCode.MetadataSkipped);
        var optionalOnly = OpenForge.Cli.Core.Commands.Index.IndexDefinitions.HasOnlyOptionalFindings(findings);
        if (retainedFinding && recovery.State != IndexRecoveryState.Retained)
        {
            throw new ArgumentException("The retained recovery finding requires one positively retained artifact.", nameof(findings));
        }

        if (status == CliSemanticStatus.Attention
            && !optionalOnly
            && !retainedFinding)
        {
            throw new ArgumentException("Index attention requires its retained recovery artifact and finding.", nameof(recovery));
        }

        if (recovery.State == IndexRecoveryState.Retained
            && status is not (CliSemanticStatus.Attention or CliSemanticStatus.Incomplete or CliSemanticStatus.Failed or CliSemanticStatus.Interrupted))
        {
            throw new ArgumentException("A retained recovery artifact requires attention, failed, or interrupted status.", nameof(recovery));
        }

        if (recovery.State == IndexRecoveryState.Retained
            && status == CliSemanticStatus.Incomplete
            && !retainedFinding)
        {
            throw new ArgumentException("An incomplete Index result with retained recovery requires its retained-artifact finding.", nameof(recovery));
        }

        if (retainedFinding
            && status != CliSemanticStatus.Attention
            && !(status == CliSemanticStatus.Incomplete && skippedFinding))
        {
            throw new ArgumentException("The retained recovery attention finding requires an otherwise successful application.", nameof(findings));
        }
    }

    private static void ValidateModeAndRegions(
        IndexResultFormation formation,
        CliSemanticStatus status,
        IReadOnlyList<IndexRegion> regions)
    {
        var updates = regions.Where(region => region.Action == IndexRegionAction.Update).ToArray();
        if (formation.Mode == IndexMode.DryRun
            && (formation.Recovery.State != IndexRecoveryState.NotRequired
                || updates.Any(region => region.Outcome != IndexRegionOutcome.NotRequested)))
        {
            throw new ArgumentException("A dry-run Index result cannot carry application effects or recovery state.", nameof(formation));
        }

        var optionalOnly = OpenForge.Cli.Core.Commands.Index.IndexDefinitions.HasOnlyOptionalFindings(formation.Findings.ToArray());
        var skipped = formation.Findings
            .Where(finding => finding.Code == IndexFindingCode.MetadataSkipped)
            .ToArray();

        if ((status == CliSemanticStatus.Complete || optionalOnly)
            && regions.Any(region => region.Action == IndexRegionAction.NotEstablished))
        {
            throw new ArgumentException("A complete Index result cannot carry unavailable region facts.", nameof(formation));
        }

        if ((status == CliSemanticStatus.Complete || optionalOnly)
            && formation.Mode == IndexMode.Apply)
        {
            var expectedRecovery = updates.Length == 0
                ? IndexRecoveryState.NotRequired
                : IndexRecoveryState.Removed;
            if (updates.Any(region => region.Outcome != IndexRegionOutcome.Verified)
                || formation.Recovery.State != expectedRecovery)
            {
                throw new ArgumentException("A complete Index application requires verified updates and exact recovery disposition.", nameof(formation));
            }
        }

        if (status == CliSemanticStatus.Attention
            && !optionalOnly
            && (formation.Mode != IndexMode.Apply
                || updates.Length == 0
                || updates.Any(region => region.Outcome != IndexRegionOutcome.Verified)
                || regions.Any(region => region.Action == IndexRegionAction.NotEstablished)))
        {
            throw new ArgumentException("Index attention requires a fully verified application with a retained artifact.", nameof(formation));
        }

        if (status == CliSemanticStatus.Incomplete && skipped.Length > 0)
        {
            var skippedPaths = skipped
                .Select(finding => finding.Details?.ParentPath)
                .Where(path => path is not null)
                .Cast<string>()
                .ToHashSet(StringComparer.Ordinal);
            var unavailable = regions
                .Where(region => region.Action == IndexRegionAction.NotEstablished)
                .ToArray();
            if (formation.Selection.Scope == IndexSelectionScope.NotEstablished
                || formation.Selection.Sources.Count == 0
                || regions.Count == 0
                || updates.Length == 0
                || unavailable.Length == 0
                || unavailable.Length == regions.Count
                || unavailable.Any(region => !skippedPaths.Contains(region.Source.Path)))
            {
                throw new ArgumentException("An incomplete Index result with skipped metadata requires an independently established updated subset.", nameof(formation));
            }

            if (formation.Mode == IndexMode.Apply
                && (updates.Any(region => region.Outcome != IndexRegionOutcome.Verified)
                    || formation.Recovery.State is not (IndexRecoveryState.Removed or IndexRecoveryState.Retained)))
            {
                throw new ArgumentException("An applied incomplete Index result requires verified independent updates and exact recovery disposition.", nameof(formation));
            }
        }

        if (status is CliSemanticStatus.Complete or CliSemanticStatus.Attention
            && (formation.Selection.Scope == IndexSelectionScope.NotEstablished
                || formation.Selection.Sources.Count == 0
                || regions.Count == 0
                || regions.Any(region => region.Action == IndexRegionAction.NotEstablished)))
        {
            throw new ArgumentException("A successful Index result requires an established selection and complete region facts.", nameof(formation));
        }
    }
}

internal sealed record IndexResultFormation
{
    public CliWorkspace? Workspace { get; init; }

    public required IndexMode Mode { get; init; }

    public required IndexSelection Selection { get; init; }

    public required IEnumerable<IndexRegion> Regions { get; init; }

    public required IndexRecovery Recovery { get; init; }

    public required IEnumerable<IndexFinding> Findings { get; init; }
}
