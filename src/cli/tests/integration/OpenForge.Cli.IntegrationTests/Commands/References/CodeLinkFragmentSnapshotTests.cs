using OpenForge.Cli.TestSupport;
using OpenForge.Cli.Core.Commands.Doctor;
using OpenForge.Cli.Core.Commands.References;
using OpenForge.Cli.Core.Framework.Workspace.Models.Layout;
using OpenForge.Cli.IntegrationTests.Commands.Shared.Snapshots;
using OpenForge.Cli.IntegrationTests.Commands.Shared.Snapshots.Models;

namespace OpenForge.Cli.IntegrationTests.Commands.References;

[Trait("Feature", "references"), Trait("Evidence", "Integration"), Trait("Boundary", "Output")]
public sealed class CodeLinkFragmentSnapshotTests
{
    [Theory(DisplayName = "Code fragments retain their distinct findings through References and Doctor snapshots"), InlineData(false), InlineData(true)]
    public async Task CodeFragments(bool doctor)
    {
        using var workspace = new RoutedOutputWorkspace();
        var mainRoot = WorkspaceLayout.DefaultMainRoot;
        workspace.Replace($"{mainRoot}/{WorkspaceLayoutNames.LoaderFileName}", OpenForgeDocumentSeed.GeneratedEntries(new GeneratedEntriesSeed
        {
            Entries = "- [Documents](docs/_docs.md) - #Docs",
            Prefix = "# Open Forge Loader\n\n## Axioms\n\n- inherited - Follow the workspace instructions.",
            IncludeFinalLineEnding = true,
        }));
        workspace.Write("Code.cs", "class Type\n{\n    void Member() {}\n}\n");
        workspace.Replace($"{mainRoot}/docs/guide.md", OpenForgeDocumentSeed.Metadata(
            description: "Guide", tags: ["Docs", "Guide"], body: """
            # Guide

            [File](../../Code.cs)
            [Lines](../../Code.cs#L2-L4)
            [Symbol](../../Code.cs#Type.Member)
            [Missing symbol](../../Code.cs#Type.Missing)
            [Outside lines](../../Code.cs#L4-L5)
            [Heading](reference.md#details)
            """));
        var before = workspace.Snapshot();
        await new ReadCommandOutputCapture(workspace.Path).MatchDetailsAsync(new ReadOutputScenario
        {
            Situation = doctor ? "doctor" : "references",
            Arguments = doctor ? [DoctorDefinitions.DoctorCommand.Name]
                : [ReferencesDefinitions.CommandIdentity, "docs/guide", ReferencesDefinitions.Direction.Name, ReferencesDefinitions.Out],
            ExitCode = 2,
        }, testName: $"{nameof(CodeFragments)}_{doctor}");
        Assert.Equal(before, workspace.Snapshot());
    }
}
