using OpenForge.Cli.Core.Commands.Doctor;
using OpenForge.Cli.Core.Commands.References;
using OpenForge.Cli.Core.Framework.Sources.References.Shared.Comments;
using OpenForge.Cli.Core.Framework.Workspace.Models.Layout;
using OpenForge.Cli.IntegrationTests.Commands.Shared.Snapshots;
using OpenForge.Cli.IntegrationTests.Commands.Shared.Snapshots.Models;
using OpenForge.Cli.TestSupport;

namespace OpenForge.Cli.IntegrationTests.Commands.References;

[Trait("Feature", "comment-links"), Trait("Evidence", "Integration"), Trait("Boundary", "Output")]
public sealed class CommentLinkOutputSnapshotTests
{
    [Theory(DisplayName = "References and Doctor show comment targets and legacy path suggestions"), InlineData(false), InlineData(true)]
    public async Task CommentFindings(bool doctor)
    {
        using var workspace = new RoutedOutputWorkspace();
        const string codePath = "src/Order.cs";
        var root = WorkspaceLayout.DefaultMainRoot;
        SetLoader(workspace);
        workspace.Write(codePath, $"// {SourceCommentLinkReader.OpenForgeMarker} {root}/docs/guide.md#rules explains the rule\n// {SourceCommentLinkReader.RuneMarker} docs/reference legacy annotation\nclass Order {{}}\n");
        var before = workspace.Snapshot();
        await new ReadCommandOutputCapture(workspace.Path).MatchDetailsAsync(new ReadOutputScenario
        {
            Situation = doctor ? "doctor-comments" : "references-comments",
            Arguments = doctor ? [DoctorDefinitions.DoctorCommand.Name]
                : [ReferencesDefinitions.CommandIdentity, $"./{codePath}", ReferencesDefinitions.Direction.Name, ReferencesDefinitions.Out],
            ExitCode = 2,
        }, testName: $"{nameof(CommentFindings)}_{doctor}");
        Assert.Equal(before, workspace.Snapshot());
    }

    [Fact(DisplayName = "Doctor lists a weak heading suggestion with its target fragment")]
    public async Task HeadingSuggestion()
    {
        using var workspace = new RoutedOutputWorkspace();
        SetLoader(workspace);
        var root = WorkspaceLayout.DefaultMainRoot;
        workspace.Write("src/Order.cs", $"// {SourceCommentLinkReader.OpenForgeMarker} {root}/docs/guide.md#reference explains this behavior\n");
        var before = workspace.Snapshot();
        await new ReadCommandOutputCapture(workspace.Path).MatchDetailsAsync(new ReadOutputScenario
        {
            Situation = "doctor-heading-suggestion",
            Arguments = [DoctorDefinitions.DoctorCommand.Name],
            ExitCode = 2,
        });
        Assert.Equal(before, workspace.Snapshot());
    }

    private static void SetLoader(RoutedOutputWorkspace workspace)
        => workspace.Replace($"{WorkspaceLayout.DefaultMainRoot}/{WorkspaceLayoutNames.LoaderFileName}",
            OpenForgeDocumentSeed.GeneratedEntries(new GeneratedEntriesSeed
            {
                Prefix = "# Fixture loader\n\n## Axioms\n\n- Keep the fixture rules.",
                Entries = "- [Documents](docs/_docs.md) - #Docs",
                IncludeFinalLineEnding = true,
            }));
}
