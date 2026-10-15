using OpenForge.Cli.Core.Commands.Index.Models.Planning;
using OpenForge.Cli.Core.Commands.Index.Models.Request;
using OpenForge.Cli.Core.Commands.Index.Shared.Planning;
using OpenForge.Cli.Core.Commands.Index.Shared.Projection;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.TestSupport;

namespace OpenForge.Cli.IntegrationTests.Commands.Index;

[Trait("Feature", "index-command"), Trait("Evidence", "Integration"), Trait("Boundary", "OS")]
public sealed class IndexValidationWorkIntegrationTests
{
    [Fact(DisplayName = "Index revalidation observes ordinary ancestors once per pass while leaves and bytes stay fresh")]
    public async Task ValidationPassSharesOnlyOrdinaryAncestors()
    {
        using var temporary = TemporaryWorkspace.Create("index-validation-work");
        temporary.WriteText(".agents/loader.md", "# Loader\n\n## Entries\n\n- [Guidance](guidance/_guidance.md)\n");
        temporary.WriteText(".agents/guidance/_guidance.md", "---\ndescription: Guidance\ntags: [Guidance]\n---\n# Guidance\n\n## Entries\n");
        for (var index = 0; index < 32; index++)
            temporary.WriteText($".agents/guidance/deep/notes/note-{index}.md", $"# Note {index}\n");
        var observations = new Dictionary<string, int>(StringComparer.Ordinal);
        var paths = new PhysicalPathResolver(path =>
        {
            observations[path] = observations.GetValueOrDefault(path) + 1;
            return LinkTargetReader.Read(path);
        });
        var workspace = new CliWorkspace(temporary.Path, temporary.Path, CliWorkspaceSelectionMethod.ExplicitWorkspace);
        var request = new IndexRequest(workspace, [], IndexMode.Apply);
        var reader = new IndexProjectionReader(paths);
        var token = TestContext.Current.CancellationToken;
        var projection = await reader.ReadAsync(request, token);
        var plan = new IndexPlanBuilder().Build(new IndexPlanningInput
        {
            Request = request,
            Projection = projection.Projection ?? throw new InvalidOperationException(),
        });

        for (var pass = 0; pass < 2; pass++)
        {
            observations.Clear();
            Assert.True(await reader.VerifyOwnershipAsync(plan, token));
            Assert.Equal(1, observations[temporary.Combine(".agents/guidance/deep")]);
            Assert.Equal(2, observations[temporary.Combine(".agents/guidance/deep/notes/note-0.md")]);
        }

        File.AppendAllText(temporary.Combine(".agents/guidance/deep/notes/note-0.md"), "Changed after planning.\n");
        Assert.False(await reader.VerifyOwnershipAsync(plan, token));
    }
}
