using System.Text;
using System.Text.Json;
using OpenForge.Cli.Core.Commands.Route.Remove;
using OpenForge.Cli.Core.Commands.Route.Remove.Models.Planning;
using OpenForge.Cli.Core.Commands.Route.Remove.Models.Request;
using OpenForge.Cli.Core.Commands.Route.Remove.Models.Result;
using OpenForge.Cli.Core.Commands.Route.Remove.Shared.Planning;
using OpenForge.Cli.Core.Commands.Route.Remove.Shared.References;
using OpenForge.Cli.Core.Commands.Route.Shared.Models.References;
using OpenForge.Cli.Core.Commands.Route.Shared.References;
using OpenForge.Cli.Core.Framework.Documents.Markdown;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Filesystem.TypedReads;
using OpenForge.Cli.Core.Framework.Mutation.Validation;
using OpenForge.Cli.Core.Framework.Sources.Inventory;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;
using OpenForge.Cli.Core.Framework.Sources.References;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.IntegrationTests.Commands.Route.Remove;

public sealed class RouteRemoveReferenceIntegrationTests
{
    [Trait("Boundary", "Host")]
    [Fact(DisplayName = "Route Remove records every external detachment with an independent visible-label oracle"),
     Trait("Feature", "route-remove"), Trait("Evidence", "Integration")]
    public async Task ExternalDetachmentRetainsLocationAndSurroundingProse()
    {
        using var workspace = RouteRemoveIntegrationWorkspace.Create("route-remove-reference-detachment");
        workspace.WriteText(
            "outside.md",
            "Before [Readable guide](.agents/guidance/old%20guide.md#part), after.\n");
        var before = workspace.ReadText("outside.md");
        var output = new StringWriter();
        var error = new StringWriter();

        var completion = await workspace.RunAsync(
            ["route", "remove", RouteRemoveIntegrationWorkspace.LeafId, "--dry-run", "--format", "json", "--detail", "standard"],
            output,
            error);

        Assert.Equal(0, completion.ExitCode);
        Assert.Equal(CliSemanticStatus.Complete, completion.Status);
        using var document = JsonDocument.Parse(output.ToString());
        var detachments = document.RootElement
            .GetProperty("data")
            .GetProperty("detachedLinks")
            .EnumerateArray()
            .ToArray();
        var detachment = Assert.Single(
            detachments,
            candidate => candidate.GetProperty("path").GetString() == "outside.md");
        Assert.Equal(
            "Before [Readable guide](.agents/guidance/old%20guide.md#part), after.",
            detachment.GetProperty("before").GetString());
        Assert.Equal("Before Readable guide, after.", detachment.GetProperty("after").GetString());
        Assert.Contains(":", detachment.GetProperty("location").GetString(), StringComparison.Ordinal);
        Assert.Equal(before, workspace.ReadText("outside.md"));
    }

    [Trait("Boundary", "Host")]
    [Fact(DisplayName = "Route Remove leaves external URLs and unrelated targets untouched"),
     Trait("Feature", "route-remove"), Trait("Evidence", "Integration")]
    public async Task UnrelatedReferencesRemainByteIdentical()
    {
        using var workspace = RouteRemoveIntegrationWorkspace.Create("route-remove-reference-preservation");
        const string outside = "Before https://example.invalid/a?b=c#part and [Topics](.agents/guidance/topics/_topics.md) after.\n";
        workspace.WriteText("outside.md", outside);
        var before = workspace.SnapshotHashes();
        var output = new StringWriter();
        var error = new StringWriter();

        var completion = await workspace.RunAsync(
            ["route", "remove", RouteRemoveIntegrationWorkspace.LeafId, "--automatic"],
            output,
            error);

        Assert.Equal(0, completion.ExitCode);
        Assert.Equal(CliSemanticStatus.Complete, completion.Status);
        Assert.Equal(outside, workspace.ReadText("outside.md"));
        Assert.Equal(before[".agents/open-forge.lock.json"],
            workspace.SnapshotHashes()[".agents/open-forge.lock.json"]);
    }

    [Trait("Boundary", "Host")]
    [Fact(DisplayName = "Route Remove blocks an unsupported incoming transformation before any write"),
     Trait("Feature", "route-remove"), Trait("Evidence", "Integration")]
    public async Task UnsupportedIncomingTransformationIsWriteFree()
    {
        using var workspace = RouteRemoveIntegrationWorkspace.Create("route-remove-reference-unsafe");
        workspace.WriteText(
            "outside.md",
            "Before [Readable guide][target], after.\n\n[target]: .agents/guidance/old%20guide.md\n");
        var before = workspace.SnapshotHashes();
        var output = new StringWriter();
        var error = new StringWriter();

        var completion = await workspace.RunAsync(
            ["route", "remove", RouteRemoveIntegrationWorkspace.LeafId, "--detail", "full"],
            output,
            error);

        Assert.Equal(5, completion.ExitCode);
        Assert.Equal(CliSemanticStatus.Blocked, completion.Status);
        Assert.Contains("route-remove.reference-unsafe", error.ToString(), StringComparison.Ordinal);
        Assert.Equal(before, workspace.SnapshotHashes());
        workspace.AssertNoLockInfrastructure();
    }

    [Trait("Boundary", "OS")]
    [Fact(DisplayName = "Route Remove live absence scanning retains cancellation and fresh read failures"), Trait("Feature", "route-remove"), Trait("Evidence", "Integration")]
    public async Task LiveAbsenceScanningRetainsInterruptionAndFreshReadFailure()
    {
        using var workspace = RouteRemoveIntegrationWorkspace.Create("route-remove-live-absence-scan");
        workspace.WriteText("absence-source.md", "An ordinary readable source.\n");
        var request = new RouteRemoveRequest(workspace.Workspace, "guidance/absent", RouteRemoveMode.DryRun);
        var scope = Assert.IsType<RouteRemoveAbsenceScope>(RouteRemoveAbsenceScope.TryCreate(request));
        var sourceCatalogue = await new SourceCatalogueReader().ReadAsync(
            new SourceCatalogueRequest(workspace.Workspace, [".agents"]),
            TestContext.Current.CancellationToken);
        var physical = new PhysicalPathResolver();
        var markdown = new MarkdownDocumentParser();
        var catalogue = await new RouteMarkdownCatalogueReader(physical).ReadAsync(
            new RouteMarkdownCatalogueRequest(
                workspace.Workspace, ["absence-source.md"], [], new RouteMarkdownCatalogueFilters([".md"])),
            TestContext.Current.CancellationToken);
        Assert.Equal(RouteMarkdownCatalogueCoverage.Complete, catalogue.Coverage);
        Assert.Equal(["absence-source.md"], catalogue.SelectedPaths);
        var scanner = new RouteRemoveReferenceScanner(
            markdown,
            new SourceLinkDestinationResolver(
                (selectedWorkspace, lexicalPath) => physical.ResolveCandidate(
                    selectedWorkspace.LexicalRoot, selectedWorkspace.PhysicalRoot, lexicalPath),
                StrictUtf8FileReader.ReadAsync,
                markdown.Parse),
            new FileExpectationValidator(physical));
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();

        var interrupted = await scanner.ProveAbsenceAsync(scope, sourceCatalogue, catalogue, cancellation.Token);

        var interruption = Assert.IsType<RouteRemoveFinding>(interrupted.Finding);
        Assert.Equal(RouteRemoveFindingCode.Interrupted, interruption.Code);
        Assert.Equal(CliSemanticStatus.Interrupted, interruption.Status);
        Assert.Equal("absence-source.md", interruption.Target);
        Assert.Equal("Route Remove absence reference inspection was interrupted.", interruption.Cause);
        Assert.Equal(RouteRemoveCoverage.Interrupted, interrupted.References.Coverage);
        Assert.Equal(0, interrupted.References.ScannedSourceCount);
        Assert.Equal(0, interrupted.References.InspectedSourceCount);
        Assert.Equal(0, interrupted.References.OccurrenceCount);
        Assert.Empty(interrupted.References.Detachments);

        var complete = await scanner.ProveAbsenceAsync(scope, sourceCatalogue, catalogue, TestContext.Current.CancellationToken);

        Assert.Null(complete.Finding);
        Assert.Equal(RouteRemoveCoverage.Complete, complete.References.Coverage);
        Assert.Equal(1, complete.References.ScannedSourceCount);
        Assert.Equal(1, complete.References.InspectedSourceCount);
        Assert.Equal(0, complete.References.OccurrenceCount);
        workspace.DeleteFile("absence-source.md");

        var incomplete = await scanner.ProveAbsenceAsync(scope, sourceCatalogue, catalogue, TestContext.Current.CancellationToken);

        var failure = Assert.IsType<RouteRemoveFinding>(incomplete.Finding);
        Assert.Equal(RouteRemoveFindingCode.ReferenceCoverageIncomplete, failure.Code);
        Assert.Equal(CliSemanticStatus.Incomplete, failure.Status);
        Assert.Equal("absence-source.md", failure.Target);
        Assert.Equal("One Markdown source could not retain an exact readable absence-proof snapshot.", failure.Cause);
        Assert.Equal(RouteRemoveCoverage.Incomplete, incomplete.References.Coverage);
        Assert.Equal(0, incomplete.References.ScannedSourceCount);
        Assert.Equal(0, incomplete.References.InspectedSourceCount);
        Assert.Equal(0, incomplete.References.OccurrenceCount);
    }

    [Trait("Boundary", "OS")]
    [Fact(DisplayName = "Route Remove live absence planning retains a residual incoming reference cause"), Trait("Feature", "route-remove"), Trait("Evidence", "Integration")]
    public async Task LiveAbsencePlanningRetainsResidualIncomingReferenceCause()
    {
        using var workspace = RouteRemoveIntegrationWorkspace.Create("route-remove-live-absence-reference");
        var request = new RouteRemoveRequest(workspace.Workspace, "guidance/absent", RouteRemoveMode.DryRun);
        var planner = RouteRemoveOperationFactory.CreatePlanBuilder();

        var missing = await planner.BuildAsync(request, TestContext.Current.CancellationToken);

        Assert.Null(missing.Plan);
        Assert.Empty(missing.Formation.Findings);
        Assert.Equal(RouteRemovePlanCompleteness.Complete, missing.Formation.Plan.Completeness);
        Assert.Equal(RouteRemovePlanSafety.Safe, missing.Formation.Plan.Safety);
        Assert.Equal(RouteRemoveVerificationState.Verified, missing.Formation.Verification);
        Assert.Empty(missing.Formation.Effects);
        workspace.WriteText("outside.md", "Before [Gone](.agents/guidance/absent/_absent.md), after.\n");
        var before = workspace.SnapshotHashes();

        var residual = await planner.BuildAsync(request, TestContext.Current.CancellationToken);

        Assert.Null(residual.Plan);
        var finding = Assert.Single(residual.Formation.Findings);
        Assert.Equal(RouteRemoveFindingCode.SourceNotFound, finding.Code);
        Assert.Equal(CliSemanticStatus.Invalid, finding.Status);
        Assert.Equal("outside.md", finding.Target);
        Assert.Equal("A residual incoming Markdown reference prevents verified Route Remove absence.", finding.Cause);
        Assert.Equal(RouteRemoveCoverage.Complete, residual.Formation.References.Coverage);
        Assert.Equal(0, residual.Formation.References.ScannedSourceCount);
        Assert.Equal(0, residual.Formation.References.InspectedSourceCount);
        Assert.Equal(0, residual.Formation.References.OccurrenceCount);
        Assert.Equal(before, workspace.SnapshotHashes());
        workspace.AssertNoLockInfrastructure();
    }

    [Trait("Boundary", "Host")]
    [Fact(DisplayName = "Route Remove repeats a verified absence without effects on the owned workspace"),
     Trait("Feature", "route-remove"), Trait("Evidence", "Integration")]
    public async Task VerifiedAbsenceRepeatsWithoutEffects()
    {
        using var workspace = RouteRemoveIntegrationWorkspace.Create("route-remove-verified-absence-repeat");
        var request = new RouteRemoveRequest(workspace.Workspace, "guidance/absent", RouteRemoveMode.Apply);
        var before = workspace.SnapshotHashes();
        var planner = RouteRemoveOperationFactory.CreatePlanBuilder();

        var first = await planner.BuildAsync(request, TestContext.Current.CancellationToken);
        var second = await planner.BuildAsync(request, TestContext.Current.CancellationToken);

        foreach (var build in new[] { first, second })
        {
            Assert.Null(build.Plan);
            Assert.Equal(RouteRemovePlanCompleteness.Complete, build.Formation.Plan.Completeness);
            Assert.Equal(RouteRemovePlanSafety.Safe, build.Formation.Plan.Safety);
            Assert.Equal(RouteRemoveVerificationState.Verified, build.Formation.Verification);
            Assert.Equal(RouteRemoveCoverage.Complete, build.Formation.References.Coverage);
            Assert.Equal(RouteRemoveCoverage.Complete, build.Formation.GeneratedNavigation.Coverage);
            Assert.Equal(RouteRemoveOwnershipState.Unmanaged, build.Formation.Ownership.State);
            Assert.Equal(RouteRemoveOwnershipTrust.Trusted, build.Formation.Ownership.Framework);
            Assert.Equal(RouteRemoveOwnershipTrust.Trusted, build.Formation.Ownership.Extensions);
            Assert.Empty(build.Formation.Ownership.Claims);
            Assert.Empty(build.Formation.Findings);
            Assert.Empty(build.Formation.Effects);
        }

        Assert.Equal(before, workspace.SnapshotHashes());
        workspace.AssertNoLockInfrastructure();
    }

    [Trait("Boundary", "Host")]
    [Fact(DisplayName = "Route Remove ordinary absence proof retains every legacy boundary fact"),
     Trait("Feature", "route-remove"), Trait("Evidence", "IntegrationSafety")]
    public async Task OrdinaryAbsenceProofRetainsEveryLegacyBoundaryFact()
    {
        (string Name, Action<RouteRemoveIntegrationWorkspace> Seed, RouteRemoveFindingCode Code, CliSemanticStatus Status)[] cases =
        [
            (
                "leaf",
                workspace => workspace.WriteText(
                    ".agents/guidance/absent.md",
                    "# A still-present leaf\n"),
                RouteRemoveFindingCode.TargetChanged,
                CliSemanticStatus.Blocked),
            (
                "overwrite",
                workspace => workspace.WriteText(
                    ".agents/guidance/absent.overwrite.md",
                    "An overwrite companion remains.\n"),
                RouteRemoveFindingCode.InvalidSubject,
                CliSemanticStatus.Invalid),
            (
                "reference",
                workspace => workspace.WriteText(
                    "outside.md",
                    "[Absent](.agents/guidance/absent/_absent.md)\n"),
                RouteRemoveFindingCode.SourceNotFound,
                CliSemanticStatus.Invalid),
            (
                "ownership",
                workspace => workspace.SeedFrameworkClaim(".agents/guidance/absent.md"),
                RouteRemoveFindingCode.OwnershipClaimed,
                CliSemanticStatus.Blocked),
        ];

        foreach (var (name, seed, code, status) in cases)
        {
            using var workspace = RouteRemoveIntegrationWorkspace.Create($"route-remove-ordinary-absence-{name}");
            seed(workspace);
            var before = workspace.SnapshotHashes();
            var sourceReference = name is "leaf" or "overwrite"
                ? ".agents/guidance/absent/_absent.md"
                : "guidance/absent";

            var build = await RouteRemoveOperationFactory.CreatePlanBuilder().BuildAsync(
                new RouteRemoveRequest(workspace.Workspace, sourceReference, RouteRemoveMode.DryRun),
                TestContext.Current.CancellationToken);

            Assert.Null(build.Plan);
            var finding = Assert.Single(build.Formation.Findings);
            Assert.Equal(code, finding.Code);
            Assert.Equal(status, finding.Status);
            if (name == "overwrite")
            {
                var output = new StringWriter();
                var error = new StringWriter();
                var completion = await workspace.RunAsync(
                    ["route", "remove", sourceReference, "--format", "json"],
                    output,
                    error);
                Assert.Equal(4, completion.ExitCode);
                using var document = JsonDocument.Parse(output.ToString());
                var jsonFinding = Assert.Single(document.RootElement.GetProperty("findings").EnumerateArray());
                Assert.Equal("route-remove.invalid-subject", jsonFinding.GetProperty("code").GetString());
                Assert.Equal(
                    $"{sourceReference} is an overwrite file; remove its base file.",
                    jsonFinding.GetProperty("message").GetString());
            }
            Assert.Equal(before, workspace.SnapshotHashes());
            workspace.AssertNoLockInfrastructure();
        }
    }

    [Trait("Boundary", "OS")]
    [Fact(DisplayName = "Route Remove coalesces Unicode references on one CRLF line into exact authored bytes"), Trait("Feature", "route-remove"), Trait("Evidence", "Integration")]
    public async Task SameLineDetachmentsRetainUnicodeAndCrLfBytes()
    {
        using var workspace = RouteRemoveIntegrationWorkspace.Create("route-remove-same-line-detachment");
        const string source = "Pré [One](.agents/guidance/old%20guide.md#part), [Deux](.agents/guidance/old%20guide.md) fin.\r\n";
        const string expected = "Pré One, Deux fin.\r\n";
        workspace.WriteText("outside.md", source);
        var before = workspace.SnapshotHashes();

        var build = await RouteRemoveOperationFactory.CreatePlanBuilder().BuildAsync(
            new RouteRemoveRequest(workspace.Workspace, RouteRemoveIntegrationWorkspace.LeafId, RouteRemoveMode.DryRun),
            TestContext.Current.CancellationToken);

        var plan = Assert.IsType<RouteRemovePlan>(build.Plan);
        var document = Assert.Single(plan.Projection.References.Documents, document => document.SourcePath == "outside.md");
        Assert.Equal(expected, document.IntendedText);
        Assert.Equal(Encoding.UTF8.GetBytes(source), document.Snapshot.Bytes.ToArray());
        var edit = Assert.Single(document.Edits);
        Assert.Equal("Pré [One](.agents/guidance/old%20guide.md#part), [Deux](.agents/guidance/old%20guide.md) fin.\r", edit.Before);
        Assert.Equal("Pré One, Deux fin.\r", edit.Expected);
        var detachments = plan.Projection.References.References.Detachments.Where(detachment => detachment.SourcePath == "outside.md").ToArray();
        Assert.Equal(["One", "Deux"], detachments.Select(detachment => detachment.VisibleLabel));
        var change = Assert.Single(plan.Projection.FileChanges, change => change.LogicalPath == workspace.Combine("outside.md"));
        Assert.Equal(Encoding.UTF8.GetBytes(expected), change.IntendedBytes.ToArray());
        Assert.Equal(before, workspace.SnapshotHashes());
        workspace.AssertNoLockInfrastructure();
    }
}
