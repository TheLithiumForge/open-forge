using OpenForge.Cli.Core.Commands.Index;
using OpenForge.Cli.Core.Commands.Index.Models.Operation;
using OpenForge.Cli.Core.Commands.Index.Models.Planning;
using OpenForge.Cli.Core.Commands.Index.Models.Request;
using OpenForge.Cli.Core.Commands.Index.Models.Result;
using OpenForge.Cli.Core.Commands.Index.Models.Selection;
using OpenForge.Cli.Core.Commands.Index.Shared.Result;
using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Pipeline;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Presentation;
using OpenForge.Cli.Core.Shell.Presentation.Models;

namespace OpenForge.Cli.Core.UnitTests.Commands.Index.Shared;

internal static class IndexTestData
{
    internal static IndexLogicalSource Source(
        string id = "memory",
        string path = ".agents/memory/_memory.md",
        IndexLogicalSourceScope scope = IndexLogicalSourceScope.Rooted)
        => new(id, path, scope);

    internal static IndexSelection Selection(params IndexLogicalSource[] sources)
        => new(
            IndexSelectionOrigin.ExplicitSources,
            ReadScope(sources),
            sources);

    internal static IndexRegion Update(
        IndexRegionOutcome outcome = IndexRegionOutcome.NotRequested,
        string beforeBody = "old\n",
        string expectedBody = "new\n",
        int? beforeEntryCount = 1)
        => IndexRegion.Update(
            Source(),
            new IndexRegionUpdate
            {
                BeforeEntryCount = beforeEntryCount,
                ExpectedEntryCount = 1,
                Change = new IndexChange(beforeBody, expectedBody),
                Outcome = outcome,
            });

    internal static IndexResult Result(
        IEnumerable<IndexRegion>? regions = null,
        IEnumerable<IndexFinding>? findings = null,
        IndexRecovery? recovery = null,
        IndexMode mode = IndexMode.Apply)
    {
        var regionValues = regions?.ToArray()
            ?? [IndexRegion.Unchanged(Source("loader", SourceLogicalPath.LoaderPath), 0, 0)];
        var selection = Selection(regionValues.Select(region => region.Source).ToArray());
        return new IndexResultBuilder().Create(
            new IndexResultFormation
            {
                Workspace = Workspace(),
                Mode = mode,
                Selection = selection,
                Regions = regionValues,
                Recovery = recovery ?? IndexRecovery.NotRequired,
                Findings = findings ?? [],
            });
    }

    internal static IndexFinding Finding(
        IndexFindingCode code,
        string cause = "The bounded Index condition was observed.")
        => new(
            code,
            sourceOccurrence: null,
            source: null,
            cause: cause,
            candidates: []);

    internal static CliPresentationRequest<IndexResult> Presentation(
        IndexResult result,
        CliDetail view = CliDetail.Standard)
        => CliPresentationStage.Create(
            result,
            new CliPresentation(CliFormat.Text, view));

    internal static CliWorkspace Workspace()
    {
        var root = Path.GetFullPath(Path.Combine(Path.GetTempPath(), "index-unit-evidence"));
        return new CliWorkspace(root, root, CliWorkspaceSelectionMethod.ExplicitWorkspace);
    }

    internal static string RecoveryPath(string name)
        => Path.GetFullPath(Path.Combine(Path.GetTempPath(), "open-forge-index-unit", name));

    private static IndexSelectionScope ReadScope(IReadOnlyList<IndexLogicalSource> sources)
    {
        var rooted = sources.Count(source => source.Scope == IndexLogicalSourceScope.Rooted);
        return rooted switch
        {
            0 => IndexSelectionScope.Detached,
            _ when rooted == sources.Count => IndexSelectionScope.Rooted,
            _ => IndexSelectionScope.Mixed,
        };
    }
}
