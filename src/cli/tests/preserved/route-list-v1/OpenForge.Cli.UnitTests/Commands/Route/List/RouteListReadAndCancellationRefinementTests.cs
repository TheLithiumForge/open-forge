using OpenForge.Cli.Commands.Route.List;
using OpenForge.Cli.Commands.Route.List.Filesystem;
using OpenForge.Cli.Commands.Route.List.Topology;
using OpenForge.Cli.Definitions;
using OpenForge.Cli.Invocation;

namespace OpenForge.Cli.UnitTests.Commands.Route.List;

public sealed class RouteListReadAndCancellationRefinementTests
{
    [Theory(DisplayName = "Route-list typed reads distinguish complete invalid UTF-8 access denied and I/O failure invariants"),
     InlineData((int)RouteListFileReadState.Complete),
     InlineData((int)RouteListFileReadState.InvalidUtf8),
     InlineData((int)RouteListFileReadState.AccessDenied),
     InlineData((int)RouteListFileReadState.IoFailure),
     Trait("Feature", "route-list"), Trait("Refinement", "route-list-acceptance"), Trait("Evidence", "Unit")]
    public void TypedReadsRetainContentOrDirectCause(int stateValue)
    {
        var state = (RouteListFileReadState)stateValue;
        const string directCause = "The operating-system read failed at the selected source.";
        var result = state switch
        {
            RouteListFileReadState.Complete => RouteListFileReadResult.Complete("authored content"),
            RouteListFileReadState.InvalidUtf8 => RouteListFileReadResult.InvalidUtf8(directCause),
            RouteListFileReadState.AccessDenied => RouteListFileReadResult.AccessDenied(directCause),
            RouteListFileReadState.IoFailure => RouteListFileReadResult.IoFailure(directCause),
            _ => throw new ArgumentOutOfRangeException(nameof(stateValue)),
        };

        Assert.Equal(state, result.State);
        if (state == RouteListFileReadState.Complete)
        {
            Assert.Equal("authored content", result.Content);
            Assert.Null(result.DirectCause);
        }
        else
        {
            Assert.Null(result.Content);
            Assert.Equal(directCause, result.DirectCause);
        }
    }

    [Fact(DisplayName = "Route-list topology cancellation retains a resolved selection and already-known relevant findings"), Trait("Feature", "route-list"), Trait("Refinement", "route-list-acceptance"), Trait("Evidence", "Unit")]
    public void TopologyCancellationRetainsAccumulatedFacts()
    {
        var request = Request("root");
        const string rootPath = ".agents/root/_root.md";
        var root = new RouteListFileFact
        {
            Path = rootPath,
            Id = "root",
            Kind = RouteListRowKind.Entrypoint,
            IsCategoryEntrypoint = true,
            IsCompatibilityEntrypoint = false,
            IsSkill = false,
            IsLoader = false,
            Metadata = new RouteListMetadataFacts("Root", ["Root"], true, []),
        };
        var loader = new RouteListFileFact
        {
            Path = RouteListDefinitions.LoaderPath,
            Id = "loader",
            Kind = RouteListRowKind.Entrypoint,
            IsCategoryEntrypoint = false,
            IsCompatibilityEntrypoint = false,
            IsSkill = false,
            IsLoader = true,
            Metadata = new RouteListMetadataFacts(null, null, true, []),
        };
        var filesByPath = new Dictionary<string, RouteListFileFact>(StringComparer.Ordinal)
        {
            [root.Path] = root,
            [loader.Path] = loader,
        };
        var filesById = new Dictionary<string, IReadOnlyList<RouteListFileFact>>(StringComparer.Ordinal)
        {
            [root.Id] = [root],
            [loader.Id] = [loader],
        };
        var folders = new Dictionary<string, RouteListFolderFact>(StringComparer.Ordinal)
        {
            [".agents"] = new RouteListFolderFact
            {
                Path = ".agents",
                RecognizedEntrypointPaths = [],
            },
            [".agents/root"] = new RouteListFolderFact
            {
                Path = ".agents/root",
                RecognizedEntrypointPaths = [root.Path],
            },
        };
        var loaderContents = string.Join(
            '\n',
            "# Loader",
            "",
            "## Entries",
            "",
            RouteListDefinitions.LoaderStartMarker,
            RouteListDefinitions.LoaderEmptyState,
            RouteListDefinitions.LoaderEndMarker);
        var inventoryFinding = new RouteListFinding(
            RouteListFindingCodes.FilesystemReadFailed,
            "A selected source could not be read.",
            ".agents/root/broken.md");
        var inventory = new RouteListSourceInventory(
            request.Workspace.Path,
            filesByPath,
            filesById,
            folders,
            new Dictionary<string, string>(StringComparer.Ordinal)
            {
                [loader.Path] = loaderContents,
            },
            loader,
            new Dictionary<string, RouteListFileFact>(StringComparer.Ordinal),
            [inventoryFinding],
            true);
        var graph = RouteListSourceGraph.Create(inventory);
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();

        var interrupted = RouteListTopologyResolver.Resolve(
            new RouteListTopologyInput(request, inventory, graph),
            cancellation.Token);

        Assert.Equal(CliSemanticStatus.Interrupted, interrupted.Status);
        Assert.Equal(RouteListSelectionKind.ExplicitSource, interrupted.Payload.Selection.Kind);
        Assert.Equal("root", interrupted.Payload.Selection.SourceId);
        Assert.Equal(rootPath, interrupted.Payload.Selection.SourcePath);
        Assert.Equal(RouteListCoverageState.Incomplete, interrupted.Payload.Coverage.State);
        Assert.Equal(RouteListCoverageBoundaries.CallerCancellation, interrupted.Payload.Coverage.Boundary);
        Assert.Equal(
            [RouteListFindingCodes.Cancelled, RouteListFindingCodes.FilesystemReadFailed],
            interrupted.Payload.Findings.Select(finding => finding.Code));
        Assert.Equal(
            [null, ".agents/root/broken.md"],
            interrupted.Payload.Findings.Select(finding => finding.Path));
        Assert.Empty(interrupted.Payload.Rows);
        Assert.NotNull(interrupted.Next);
        Assert.Equal(RouteListDefinitions.ListCommandPath, interrupted.Next.Command);
        Assert.NotEmpty(interrupted.Next.Reason);
    }

    [Fact(DisplayName = "Route-list completed result facts survive cancellation after formation"), Trait("Feature", "route-list"), Trait("Refinement", "route-list-acceptance"), Trait("Evidence", "Unit")]
    public void CompletedResultSurvivesLateCancellation()
    {
        var request = Request("root");
        var payload = new RouteListPayload(
            RouteListSelectionFactory.ResolvedSource("root", ".agents/root/_root.md"),
            request.RequestedDepth,
            RouteListDepth.Bounded(0),
            new RouteListCoverage(RouteListCoverageState.Complete, null),
            [],
            [Row("root", ".agents/root/_root.md", null, null, 0)]);
        var formed = new RouteListResult(
            RouteListDefinitions.SchemaVersion,
            RouteListDefinitions.ResultCommand,
            CliSemanticStatus.Complete,
            request.Workspace,
            payload,
            null);

        var retained = RouteListCancellation.RetainOrInterrupt(
            formed,
            [],
            resultFormationCompleted: true);

        Assert.Equal(formed.SchemaVersion, retained.SchemaVersion);
        Assert.Equal(formed.Command, retained.Command);
        Assert.Equal(formed.Status, retained.Status);
        Assert.Equal(formed.Workspace, retained.Workspace);
        Assert.Equal(formed.Result.Selection, retained.Result.Selection);
        Assert.Equal(formed.Result.RequestedDepth, retained.Result.RequestedDepth);
        Assert.Equal(formed.Result.EffectiveDepth, retained.Result.EffectiveDepth);
        Assert.Equal(formed.Result.Coverage, retained.Result.Coverage);
        Assert.Equal(formed.Result.Findings, retained.Result.Findings);
        Assert.Equal(formed.Result.Rows, retained.Result.Rows);
        Assert.Equal(formed.Next, retained.Next);
    }

    private static RouteListRequest Request(string sourceId)
    {
        return new RouteListRequest(
            new CliWorkspace("C:/workspace", CliWorkspaceSelection.ExplicitWorkspace),
            RouteListSourceReference.Parse(sourceId),
            RouteListDepth.All);
    }

    private static RouteListRow Row(
        string id,
        string path,
        string? parentId,
        string? parentPath,
        int relativeDepth)
    {
        return new RouteListRow(
            id,
            path,
            parentId,
            parentPath,
            relativeDepth,
            relativeDepth,
            RouteListRowKind.RoutedLeaf,
            "Description",
            ["Route"],
            null,
            [RouteListProvenance.AuthoredTopology]);
    }
}
