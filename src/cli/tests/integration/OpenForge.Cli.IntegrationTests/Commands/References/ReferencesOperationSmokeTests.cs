using OpenForge.Cli.Core.Commands.References;
using OpenForge.Cli.Core.Commands.References.Models.Occurrence;
using OpenForge.Cli.Core.Commands.References.Models.Request;
using OpenForge.Cli.Core.Commands.References.Models.Result;
using OpenForge.Cli.Core.Commands.References.Models.Selection;
using OpenForge.Cli.Core.Framework.Sources.Models.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;
using OpenForge.Cli.Core.Framework.Sources.Models.Selection;
using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.TestSupport;

namespace OpenForge.Cli.IntegrationTests.Commands.References;

public sealed class ReferencesOperationSmokeTests
{
    [Fact(DisplayName = "References operation reports direct outgoing and incoming Markdown links"), Trait("Feature", "references"), Trait("Evidence", "Integration")]
    public async Task OperationReportsDirectOutgoingAndIncomingLinks()
    {
        using var workspace = TemporaryWorkspace.Create("references-operation");
        workspace.CreateDirectory(".agents");
        workspace.WriteText(".agents/docs.md", "# Docs\n\n[other](other.md)\n");
        workspace.WriteText(".agents/other.md", "# Other\n\n[docs](docs.md)\n");
        var cliWorkspace = new CliWorkspace(
            workspace.Path,
            workspace.Path,
            CliWorkspaceSelectionMethod.CurrentDirectory);
        var request = new ReferencesRequest(
            cliWorkspace,
            "docs",
            ReferencesDirection.Both,
            Array.Empty<SourceUniverseSelectorOccurrence>());

        var result = await ReferencesOperationFactory.Create().ExecuteAsync(request, CancellationToken.None);

        Assert.Equal(OpenForge.Cli.Core.Shell.Definitions.CliSemanticStatus.Complete, result.Status);
        Assert.NotNull(result.Incoming);
        Assert.NotNull(result.Outgoing);
        Assert.Single(result.Incoming!.Occurrences);
        Assert.Single(result.Outgoing!.Occurrences);
        Assert.Equal("docs.md", result.Incoming.Occurrences[0].Target.Path is { } path ? System.IO.Path.GetFileName(path) : null);
        Assert.Equal("other.md", result.Outgoing.Occurrences[0].Target.Path is { } outgoingPath ? System.IO.Path.GetFileName(outgoingPath) : null);
    }

    [Fact(DisplayName = "References operation preserves base then overwrite duplicates without writing"), Trait("Feature", "references"), Trait("Evidence", "Integration")]
    public async Task OperationPreservesLayerOrderDuplicatesAndWorkspaceBytes()
    {
        using var workspace = TemporaryWorkspace.Create("references-layers");
        workspace.WriteText(".agents/docs.md", "# Docs\n\n[first](target.md)\n[duplicate](target.md)\n");
        workspace.WriteText(".agents/docs.overwrite.md", "# Override\n\n[overwrite](target.md)\n");
        workspace.WriteText(".agents/target.md", "# Target\n");
        var before = workspace.SnapshotHashes();

        var result = await ExecuteAsync(workspace, "docs", ReferencesDirection.Out);

        Assert.Equal(CliSemanticStatus.Complete, result.Status);
        Assert.Null(result.Incoming);
        Assert.Equal(
            ["target.md", "target.md", "target.md"],
            result.Outgoing!.Occurrences.Select(occurrence => occurrence.RawDestination));
        Assert.Equal(
            [".agents/docs.md", ".agents/docs.md", ".agents/docs.overwrite.md"],
            result.Outgoing.Occurrences.Select(occurrence => occurrence.Source.Path));
        Assert.Equal(
            [SourceLayerKind.Base, SourceLayerKind.Base, SourceLayerKind.Overwrite],
            result.Outgoing.Occurrences.Select(occurrence => occurrence.Source.Layer));
        Assert.Equal(before, workspace.SnapshotHashes());
    }

    [Fact(DisplayName = "References incoming filters retain order and inspect only the effective universe"), Trait("Feature", "references"), Trait("Evidence", "Integration")]
    public async Task IncomingFiltersControlOnlyTheEffectiveScan()
    {
        using var workspace = TemporaryWorkspace.Create("references-filtered-incoming");
        workspace.WriteText(".agents/docs.md", "# Docs\n");
        workspace.WriteText(".agents/alpha.md", "# Alpha\n\n[docs](docs.md)\n");
        workspace.WriteText(".agents/beta.md", "# Beta\n\n[docs](docs.md)\n");
        var selectors = new[]
        {
            new SourceUniverseSelectorOccurrence(SourceUniverseSelectorRole.Include, ".agents/alpha.md", 1),
            new SourceUniverseSelectorOccurrence(SourceUniverseSelectorRole.Exclude, ".agents/beta.md", 2),
        };
        var before = workspace.SnapshotHashes();

        var result = await ExecuteAsync(workspace, "docs", ReferencesDirection.In, selectors);

        Assert.Equal(CliSemanticStatus.Complete, result.Status);
        Assert.Equal(ReferencesSelectionMode.Filtered, result.IncomingSelection!.Mode);
        Assert.Equal([".agents/alpha.md", ".agents/beta.md"], result.IncomingSelection.Supplied.Select(value => value.Value));
        Assert.Equal(["alpha"], result.IncomingSelection.EffectiveSources.Select(value => value.Id));
        var inspected = Assert.Single(result.IncomingSelection.InspectedSources);
        Assert.Equal("alpha", inspected.Source.Id);
        Assert.Equal(".agents/alpha.md", inspected.Path);
        var occurrence = Assert.Single(result.Incoming!.Occurrences);
        Assert.Equal("alpha", occurrence.Source.Id);
        Assert.Equal(ReferencesProvenance.FilteredIncomingScan, occurrence.Provenance);
        Assert.Equal(before, workspace.SnapshotHashes());
    }

    [Fact(DisplayName = "References operation classifies local fragments encodings external schemes and missing targets"), Trait("Feature", "references"), Trait("Evidence", "Integration")]
    public async Task DestinationResolutionUsesAcceptedOneHopFacts()
    {
        using var workspace = TemporaryWorkspace.Create("references-resolution");
        workspace.WriteText(
            ".agents/docs.md",
            "# Docs\n\n"
            + "[self](#docs)\n"
            + "[missing fragment](#absent)\n"
            + "[unicode](unicod%C3%A9.md#caf%C3%A9)\n"
            + "[outside](../README.md)\n"
            + "[missing](missing.md)\n"
            + "[external](https://example.invalid/path#fragment)\n"
            + "[unsupported](mailto:docs@example.invalid)\n");
        workspace.WriteText(".agents/unicodé.md", "# Café\n");
        workspace.WriteText("README.md", "# Readme\n");

        var result = await ExecuteAsync(workspace, "docs", ReferencesDirection.Out);

        Assert.Equal(CliSemanticStatus.Attention, result.Status);
        Assert.Equal(7, result.Outgoing!.OccurrenceCount);
        Assert.Collection(
            result.Outgoing.Occurrences,
            occurrence => AssertTarget(occurrence.Target, ReferencesTargetKind.Local, ReferencesTargetResolution.Complete, ".agents/docs.md", "docs"),
            occurrence => AssertTarget(occurrence.Target, ReferencesTargetKind.Local, ReferencesTargetResolution.FragmentMissing, ".agents/docs.md", "docs"),
            occurrence => AssertTarget(occurrence.Target, ReferencesTargetKind.Local, ReferencesTargetResolution.Complete, ".agents/unicodé.md", "unicodé"),
            occurrence => AssertTarget(occurrence.Target, ReferencesTargetKind.Local, ReferencesTargetResolution.Complete, "README.md", null),
            occurrence => AssertTarget(occurrence.Target, ReferencesTargetKind.Local, ReferencesTargetResolution.Missing, ".agents/missing.md", null),
            occurrence =>
            {
                AssertTarget(occurrence.Target, ReferencesTargetKind.External, ReferencesTargetResolution.ExternalUnchecked, null, null);
                Assert.Equal(ReferencesNetworkState.NetworkNotAttempted, occurrence.Target.Network);
            },
            occurrence => AssertTarget(occurrence.Target, ReferencesTargetKind.Unsupported, ReferencesTargetResolution.Unsupported, null, null));
        Assert.Equal(
            [ReferencesFindingCode.DestinationUnsupported, ReferencesFindingCode.TargetMissing, ReferencesFindingCode.FragmentMissing],
            result.Findings.Select(finding => finding.Code));
    }

    [Fact(DisplayName = "References operation preserves malformed local forms and strict encoding outcomes"), Trait("Feature", "references"), Trait("Evidence", "Integration")]
    public async Task MalformedLocalFormsRemainTypedOutgoingOccurrences()
    {
        using var workspace = TemporaryWorkspace.Create("references-local-forms");
        workspace.WriteText(
            ".agents/docs.md",
            "# Docs\n\n"
            + "[space](<space file.md>)\n"
            + "[absolute](/root.md)\n"
            + "[drive](C:/root.md)\n"
            + "[query](target.md?mode=1)\n"
            + "[encoding](bad%ZZ.md)\n"
            + "[outside](../../outside.md)\n"
            + "[invalid target](invalid.md#heading)\n");
        workspace.WriteText(".agents/space file.md", "# Space\n");
        workspace.WriteBytes(".agents/invalid.md", [0xFF, 0xFE]);

        var result = await ExecuteAsync(workspace, "docs", ReferencesDirection.Out);

        Assert.Equal(CliSemanticStatus.Blocked, result.Status);
        Assert.Equal(
            [
                ReferencesTargetResolution.Complete,
                ReferencesTargetResolution.Absolute,
                ReferencesTargetResolution.Absolute,
                ReferencesTargetResolution.Query,
                ReferencesTargetResolution.EncodingUnsupported,
                ReferencesTargetResolution.OutsideWorkspace,
                ReferencesTargetResolution.EncodingUnsupported,
            ],
            result.Outgoing!.Occurrences.Select(occurrence => occurrence.Target.Resolution));
        Assert.Equal(".agents/space file.md", result.Outgoing.Occurrences[0].Target.Path);
        Assert.Null(result.Outgoing.Occurrences[4].Target.Path);
        Assert.Equal(".agents/invalid.md", result.Outgoing.Occurrences[6].Target.Path);
        Assert.Equal(
            [
                ReferencesFindingCode.InvalidEncoding,
                ReferencesFindingCode.InvalidEncoding,
                ReferencesFindingCode.DestinationMalformed,
                ReferencesFindingCode.DestinationMalformed,
                ReferencesFindingCode.DestinationMalformed,
                ReferencesFindingCode.TargetUnsafe,
            ],
            result.Findings.Select(finding => finding.Code));
    }

    [Fact(DisplayName = "References incoming complete-empty result records every established physical layer"), Trait("Feature", "references"), Trait("Evidence", "Integration")]
    public async Task IncomingCompleteEmptyRequiresCompleteLayerEvidence()
    {
        using var workspace = TemporaryWorkspace.Create("references-empty-incoming");
        workspace.WriteText(".agents/docs.md", "# Docs\n");
        workspace.WriteText(".agents/alpha.md", "# Alpha\n");
        workspace.WriteText(".agents/beta.md", "# Beta\n");
        workspace.WriteText(".agents/beta.overwrite.md", "# Beta override\n");

        var result = await ExecuteAsync(workspace, "docs", ReferencesDirection.In);

        Assert.Equal(CliSemanticStatus.Complete, result.Status);
        Assert.Equal(ReferencesCoverage.Complete, result.Incoming!.Coverage);
        Assert.Empty(result.Incoming.Occurrences);
        Assert.Equal(
            [
                ".agents/alpha.md",
                ".agents/beta.md",
                ".agents/beta.overwrite.md",
                ".agents/docs.md",
            ],
            result.IncomingSelection!.InspectedSources.Select(source => source.Path));
    }

    [Fact(DisplayName = "References incoming incomplete result omits unavailable layers from inspected evidence"), Trait("Feature", "references"), Trait("Evidence", "Integration")]
    public async Task IncomingUnavailableLayerCannotBecomeCompleteEmptyEvidence()
    {
        using var workspace = TemporaryWorkspace.Create("references-incomplete-incoming");
        workspace.WriteText(".agents/docs.md", "# Docs\n");
        workspace.WriteBytes(".agents/unreadable.md", [0xFF, 0xFE]);

        var result = await ExecuteAsync(workspace, "docs", ReferencesDirection.In);

        Assert.Equal(CliSemanticStatus.Incomplete, result.Status);
        Assert.Equal(ReferencesCoverage.Incomplete, result.Incoming!.Coverage);
        Assert.Empty(result.Incoming.Occurrences);
        Assert.Equal([".agents/docs.md"], result.IncomingSelection!.InspectedSources.Select(source => source.Path));
        Assert.Contains(result.Findings, finding => finding.Code == ReferencesFindingCode.InvalidEncoding);
    }

    [Fact(DisplayName = "References incoming exact paths survive automatic ID collisions"), Trait("Feature", "references"), Trait("Evidence", "Integration")]
    public async Task IncomingMatchUsesExactLogicalPathWhenIdsCollide()
    {
        using var workspace = TemporaryWorkspace.Create("references-id-collision");
        workspace.WriteText(".agents/collision.md", "# Collision leaf\n");
        workspace.WriteText(".agents/collision/_collision.md", "# Collision entrypoint\n");
        workspace.WriteText(".agents/ref.md", "# Ref\n\n[target](collision.md)\n");

        var result = await ExecuteAsync(workspace, ".agents/collision.md", ReferencesDirection.In);

        Assert.Equal(CliSemanticStatus.Attention, result.Status);
        Assert.Equal(ReferencesCoverage.Complete, result.Incoming!.Coverage);
        var occurrence = Assert.Single(result.Incoming.Occurrences);
        Assert.Equal(".agents/collision.md", occurrence.Target.Path);
        Assert.Null(occurrence.Target.Id);
        Assert.Equal(SourceLayerKind.Base, occurrence.Target.Layer);
        Assert.Contains(result.Findings, finding => finding.Code == ReferencesFindingCode.IdentityCollision);
    }

    [Fact(DisplayName = "References operation blocks a physical target escape and preserves the authored occurrence"), Trait("Feature", "references"), Trait("Evidence", "Integration")]
    public async Task PhysicalTargetEscapeIsBlockedWithoutFollowingTheLink()
    {
        using var workspace = TemporaryWorkspace.Create("references-target-escape");
        using var outside = TemporaryWorkspace.Create("references-target-outside");
        workspace.WriteText(".agents/docs.md", "# Docs\n\n[escape](escape.md)\n");
        outside.WriteText("outside.md", "# Outside\n");
        if (!workspace.TryCreateFileSymbolicLink(".agents/escape.md", outside.Combine("outside.md"), out _))
        {
            return;
        }

        var outsideBefore = outside.SnapshotHashes();
        var result = await ExecuteAsync(workspace, "docs", ReferencesDirection.Out);

        Assert.Equal(CliSemanticStatus.Blocked, result.Status);
        var occurrence = Assert.Single(result.Outgoing!.Occurrences);
        Assert.Equal(ReferencesTargetResolution.PhysicalEscape, occurrence.Target.Resolution);
        Assert.Contains(result.Findings, finding => finding.Code == ReferencesFindingCode.TargetUnsafe);
        Assert.Equal(outsideBefore, outside.SnapshotHashes());
    }

    [Fact(DisplayName = "References operation retains interrupted status and incomplete requested coverage"), Trait("Feature", "references"), Trait("Evidence", "Integration")]
    public async Task PreCancelledOperationCannotClaimCompleteCoverage()
    {
        using var workspace = TemporaryWorkspace.Create("references-cancelled");
        workspace.WriteText(".agents/docs.md", "# Docs\n");
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();
        var request = CreateRequest(workspace, "docs", ReferencesDirection.Both, []);

        var result = await ReferencesOperationFactory.Create().ExecuteAsync(request, cancellation.Token);

        Assert.Equal(CliSemanticStatus.Interrupted, result.Status);
        Assert.Equal(ReferencesCoverage.Incomplete, result.Incoming!.Coverage);
        Assert.Equal(ReferencesCoverage.Incomplete, result.Outgoing!.Coverage);
        Assert.Empty(result.Incoming.Occurrences);
        Assert.Empty(result.Outgoing.Occurrences);
        Assert.Single(result.Findings, finding => finding.Code == ReferencesFindingCode.Interrupted);
    }

    [Fact(DisplayName = "References operation blocks an unresolved incoming selector without scanning"), Trait("Feature", "references"), Trait("Evidence", "Integration")]
    public async Task InvalidIncomingSelectorCannotEstablishScanCoverage()
    {
        using var workspace = TemporaryWorkspace.Create("references-invalid-filter");
        workspace.WriteText(".agents/docs.md", "# Docs\n");
        var selectors = new[]
        {
            new SourceUniverseSelectorOccurrence(SourceUniverseSelectorRole.Include, "missing", 1),
        };

        var result = await ExecuteAsync(workspace, "docs", ReferencesDirection.In, selectors);

        Assert.Equal(CliSemanticStatus.Invalid, result.Status);
        Assert.Equal(ReferencesCoverage.Blocked, result.Incoming!.Coverage);
        Assert.Empty(result.Incoming.Occurrences);
        Assert.Empty(result.IncomingSelection!.InspectedSources);
        Assert.Single(result.Findings, finding => finding.Code == ReferencesFindingCode.InvalidFilter);
        Assert.Equal("open-forge references --help", result.Next!.Command);
    }

    [Fact(DisplayName = "References operation retains invalid-source facts with blocked requested sections"), Trait("Feature", "references"), Trait("Evidence", "Integration")]
    public async Task InvalidSourceCannotClaimRequestedCoverage()
    {
        using var workspace = TemporaryWorkspace.Create("references-invalid-source");
        workspace.WriteText(".agents/docs.md", "# Docs\n");

        var result = await ExecuteAsync(workspace, "missing", ReferencesDirection.Both);

        Assert.Equal(CliSemanticStatus.Invalid, result.Status);
        Assert.Null(result.Source);
        Assert.Equal(ReferencesCoverage.Blocked, result.Incoming!.Coverage);
        Assert.Equal(ReferencesCoverage.Blocked, result.Outgoing!.Coverage);
        Assert.Empty(result.Incoming.Occurrences);
        Assert.Empty(result.Outgoing.Occurrences);
        Assert.Contains(result.Findings, finding => finding.Code == ReferencesFindingCode.InvalidSource);
    }

    [Theory(DisplayName = "References cancellation retains unresolved incoming selector rows"), Trait("Feature", "references"), Trait("Evidence", "Integration")]
    [InlineData(false), InlineData(true)]
    public async Task PreCancelledOperationRetainsIncomingSelectors(bool both)
    {
        using var workspace = TemporaryWorkspace.Create("references-filtered-cancelled");
        workspace.WriteText(".agents/docs.md", "# Docs\n");
        workspace.WriteText(".agents/skip.md", "# Skip\n");
        var selectors = new[]
        {
            new SourceUniverseSelectorOccurrence(SourceUniverseSelectorRole.Include, "docs", 1),
            new SourceUniverseSelectorOccurrence(SourceUniverseSelectorRole.Exclude, ".agents/skip.md", 2),
            new SourceUniverseSelectorOccurrence(SourceUniverseSelectorRole.Include, "docs", 3),
            new SourceUniverseSelectorOccurrence(SourceUniverseSelectorRole.Exclude, "./.agents/skip.md", 4),
        };
        var request = CreateRequest(workspace, "docs", both ? ReferencesDirection.Both : ReferencesDirection.In, selectors);
        var before = workspace.SnapshotHashes();
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();

        var result = await ReferencesOperationFactory.Create().ExecuteAsync(request, cancellation.Token);

        Assert.Equal(CliSemanticStatus.Interrupted, result.Status);
        var finding = Assert.Single(result.Findings);
        Assert.Equal(ReferencesFindingCode.Interrupted, finding.Code);
        Assert.Null(finding.Direction);
        Assert.Null(finding.Subject);
        Assert.Null(result.Source);
        var selection = Assert.IsType<ReferencesIncomingSelection>(result.IncomingSelection);
        Assert.Equal(ReferencesSelectionMode.Filtered, selection.Mode);
        Assert.Equal(
            [
                (SourceUniverseSelectorRole.Include, "docs"),
                (SourceUniverseSelectorRole.Exclude, ".agents/skip.md"),
                (SourceUniverseSelectorRole.Include, "docs"),
                (SourceUniverseSelectorRole.Exclude, "./.agents/skip.md"),
            ],
            selection.Supplied.Select(row => (row.Role, row.Value)));
        Assert.Equal(
            [
                (SourceUniverseSelectorRole.Include, 1, "docs", SourceReferenceKind.SourceId),
                (SourceUniverseSelectorRole.Exclude, 1, ".agents/skip.md", SourceReferenceKind.SourcePath),
                (SourceUniverseSelectorRole.Include, 2, "docs", SourceReferenceKind.SourceId),
                (SourceUniverseSelectorRole.Exclude, 2, "./.agents/skip.md", SourceReferenceKind.SourcePath),
            ],
            selection.Resolved.Select(row => (row.Role, row.Occurrence, row.Supplied, row.Form)));
        Assert.All(selection.Resolved, row =>
        {
            Assert.Equal(SourceReferenceResolutionState.Unknown, row.Resolution);
            Assert.Null(row.Source);
            Assert.Null(row.Expansion);
            Assert.Empty(row.Candidates);
        });
        Assert.Empty(selection.EffectiveSources);
        Assert.Empty(selection.InspectedSources);
        var incoming = Assert.IsType<ReferencesSection>(result.Incoming);
        Assert.Equal(ReferencesCoverage.Incomplete, incoming.Coverage);
        Assert.Equal(0, incoming.OccurrenceCount);
        Assert.Empty(incoming.Occurrences);
        if (both)
        {
            var outgoing = Assert.IsType<ReferencesSection>(result.Outgoing);
            Assert.Equal(ReferencesCoverage.Incomplete, outgoing.Coverage);
            Assert.Equal(0, outgoing.OccurrenceCount);
            Assert.Empty(outgoing.Occurrences);
        }
        else
        {
            Assert.Null(result.Outgoing);
        }
        Assert.Equal(before, workspace.SnapshotHashes());
    }

    private static async Task<ReferencesResult> ExecuteAsync(
        TemporaryWorkspace workspace,
        string sourceReference,
        ReferencesDirection direction,
        IReadOnlyList<SourceUniverseSelectorOccurrence>? selectors = null)
        => await ReferencesOperationFactory.Create().ExecuteAsync(
            CreateRequest(workspace, sourceReference, direction, selectors ?? []),
            CancellationToken.None);

    private static ReferencesRequest CreateRequest(
        TemporaryWorkspace workspace,
        string sourceReference,
        ReferencesDirection direction,
        IReadOnlyList<SourceUniverseSelectorOccurrence> selectors)
        => new(
            new CliWorkspace(workspace.Path, workspace.Path, CliWorkspaceSelectionMethod.CurrentDirectory),
            sourceReference,
            direction,
            selectors);

    private static void AssertTarget(
        ReferencesTarget target,
        ReferencesTargetKind kind,
        ReferencesTargetResolution resolution,
        string? path,
        string? id)
    {
        Assert.Equal(kind, target.Kind);
        Assert.Equal(resolution, target.Resolution);
        Assert.Equal(path, target.Path);
        Assert.Equal(id, target.Id);
    }
}
