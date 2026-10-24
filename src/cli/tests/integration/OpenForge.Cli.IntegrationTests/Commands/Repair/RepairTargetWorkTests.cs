using OpenForge.Cli.Core.Framework.Settings.Shared.Observation;
using OpenForge.Cli.Core.Commands.Repair.Models.Selection;
using OpenForge.Cli.Core.Commands.Repair.Shared.Planning;
using OpenForge.Cli.Core.Framework.Sources.Inventory;
using OpenForge.Cli.Core.Framework.Sources.Models.References;
using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.TestSupport;
using OpenForge.Cli.Core.Framework.Sources.Shared.Acquisition;
using OpenForge.Cli.Core.Framework.Documents.Markdown;
using OpenForge.Cli.Core.Framework.Filesystem.TypedReads;
using OpenForge.Cli.Core.Commands.Repair.Shared.Diagnosis;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Sources.Sharing;

namespace OpenForge.Cli.IntegrationTests.Commands.Repair;

public sealed class RepairTargetWorkTests
{
    [Fact(DisplayName = "Repair candidate batches acquire every target freshly with bounded workers and stable ordering"),
        Trait("Feature", "repair"), Trait("Evidence", "Integration"), Trait("Boundary", "OS")]
    public async Task CandidateBatchesKeepFreshReadsAndStableOrder()
    {
        using var temporary = TemporaryWorkspace.Create("repair-candidate-batch");
        for (var index = 0; index < 32; index++)
            temporary.WriteText($".agents/guidance/source-{index}.md", $"# Source {index}\n\n[Details](missing.md#details)\n");
        temporary.WriteText(".agents/guidance/target-0.md", "# Details\n");
        temporary.WriteText(".agents/guidance/target-1.md", "# Details\n");
        var workspace = new CliWorkspace(temporary.Path, temporary.Path, CliWorkspaceSelectionMethod.ExplicitWorkspace);
        var token = TestContext.Current.CancellationToken;
        var paths = new PhysicalPathResolver();
        var observation = await new WorkspaceLayoutReader(new PhysicalPathResolver()).ReadAsync(workspace, token);
        var contributors = RepairOperationalContributorFactory.Create(paths);
        var routes = await contributors.Routes.ReadDoctorAsync(observation, new SourceSharing([]), token);
        var view = await contributors.LocalReferences.ReadDoctorAsync(observation, token, routes);
        var active = 0;
        var maximum = 0;
        var acquisition = new SourceFileAcquisition(read: async (physical, logical, cancellation) =>
        {
            var current = Interlocked.Increment(ref active);
            int observed;
            do { observed = Volatile.Read(ref maximum); }
            while (current > observed && Interlocked.CompareExchange(ref maximum, current, observed) != observed);
            try
            {
                await Task.Yield();
                return await StrictUtf8FileReader.ReadAsync(physical, logical, cancellation);
            }
            finally { Interlocked.Decrement(ref active); }
        });
        var targetReader = new RepairTargetReader(view.Catalogue, acquisition: acquisition);
        var reader = new RepairCatalogueReader(paths, targets: targetReader);

        var first = await reader.ReadAsync(observation, view, token);
        Assert.Equal(32, first.Proposals.Count);
        Assert.All(first.Proposals, input => Assert.Equal(
            [".agents/guidance/target-0.md", ".agents/guidance/target-1.md"],
            input.Proposal.Candidates?.Items.Select(candidate => candidate.Target.CanonicalTargetPath)));
        Assert.Equal(32 * 33, acquisition.ReadCount);
        Assert.InRange(maximum, 2, RepairCatalogueReader.MaximumConcurrentProposals);
        Assert.Equal(0, active);

        temporary.ReplaceText(".agents/guidance/target-0.md", "# Changed\n");
        var second = await reader.ReadAsync(observation, view, token);
        Assert.Equal(2 * 32 * 33, acquisition.ReadCount);
        Assert.All(second.Proposals, input => Assert.Equal(".agents/guidance/target-1.md",
            Assert.Single(input.Proposal.Candidates?.Items ?? []).Target.CanonicalTargetPath));
    }

    [Fact(DisplayName = "Repair resolves every occurrence freshly and observes changed headings in the same planning stage"),
        Trait("Feature", "repair"), Trait("Evidence", "Integration"), Trait("Boundary", "OS")]
    public async Task TargetWorkIsBoundedByDistinctSelections()
    {
        using var temporary = TemporaryWorkspace.Create("repair-target-work");
        temporary.WriteText(".agents/target.md", "# Details\n");
        var workspace = new CliWorkspace(temporary.Path, temporary.Path, CliWorkspaceSelectionMethod.ExplicitWorkspace);
        var catalogue = await new SourceCatalogueReader().ReadAsync(new(workspace, [".agents"]), TestContext.Current.CancellationToken);
        var resolutions = 0;
        var formats = 0;
        var parses = 0;
        var acquisition = new SourceFileAcquisition(parse: text => { parses++; return new MarkdownDocumentParser().Parse(text); });
        acquisition.RetainDocument(new MarkdownDocumentParser().Parse("# Details\n"));
        var reader = new RepairTargetReader(catalogue, () => resolutions++, acquisition, (source, path) =>
        {
            formats++;
            return RepairDestinationFormatter.FormatPath(source, path);
        });
        var target = new RepairTargetSelection(".agents/target.md", "details");
        for (var index = 0; index < 30; index++)
        {
            var facts = await reader.ResolveAsync(workspace, $".agents/nested/source-{index}.md", target, TestContext.Current.CancellationToken);
            Assert.Equal(SourceLinkTargetResolution.Complete, facts.Target.Resolution);
        }
        Assert.Equal(30, resolutions);
        Assert.Equal(1, formats);
        Assert.Equal(0, parses);
        Assert.Equal(30, acquisition.ReadCount);
        File.WriteAllText(temporary.Combine(".agents/target.md"), "# Changed\n");
        var fresh = await reader.ResolveAsync(workspace, ".agents/source.md", target, TestContext.Current.CancellationToken);
        Assert.Equal(SourceLinkTargetResolution.FragmentMissing, fresh.Target.Resolution);
        Assert.Equal(31, resolutions);
        Assert.Equal(2, formats);
        Assert.Equal(1, parses);
        Assert.Equal(31, acquisition.ReadCount);
    }
}
