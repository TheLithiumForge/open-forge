using OpenForge.Cli.Core.Commands.References;
using OpenForge.Cli.Core.Commands.References.Models.Request;
using OpenForge.Cli.Core.Commands.References.Models.Result;
using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.Core.Framework.Workspace.Models.Layout;
using OpenForge.Cli.Core.Framework.Sources.Models.References;
using OpenForge.Cli.Core.Framework.Sources.References.Shared.Comments;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.TestSupport;

namespace OpenForge.Cli.IntegrationTests.Commands.References;

[Trait("Feature", "comment-links"), Trait("Evidence", "Integration"), Trait("Boundary", "OS")]
public sealed class CommentLinkIntegrationTests
{
    private const string CodePath = "src/Order.cs";
    private static string GuidePath => $"{WorkspaceLayout.DefaultMainRoot}/guide.md";

    [Fact(DisplayName = "References connects code and Markdown comments in both directions without changing bytes")]
    public async Task BothDirections()
    {
        using var workspace = TemporaryWorkspace.Create("comment-links");
        workspace.WriteText(GuidePath, $"# Guide\n\n## Rule\n\n<!-- {SourceCommentLinkReader.RuneMarker} ../{CodePath}#Order.Save implements the rule -->\n");
        workspace.WriteText(CodePath, $"// {SourceCommentLinkReader.OpenForgeMarker} {GuidePath}#rule explains the behavior\nclass Order {{ void Save() {{}} }}\n");
        var before = workspace.SnapshotHashes();
        var result = await Read(workspace, "guide", ReferencesDirection.Both);

        Assert.Equal(CliSemanticStatus.Complete, result.Status);
        var incoming = Assert.Single(result.Incoming?.Occurrences ?? []);
        var outgoing = Assert.Single(result.Outgoing?.Occurrences ?? []);
        Assert.Equal(CodePath, incoming.Source.Path);
        Assert.Equal(GuidePath, incoming.Target.Path);
        Assert.Equal(CodePath, outgoing.Target.Path);
        Assert.Equal(before, workspace.SnapshotHashes());
        var code = await Read(workspace, $"./{CodePath}", ReferencesDirection.Out);
        Assert.Equal(CliSemanticStatus.Complete, code.Status);
        Assert.Equal(GuidePath, Assert.Single(code.Outgoing?.Occurrences ?? []).Target.Path);
    }

    [Fact(DisplayName = "Comment dot paths resolve beside the code file and keep target kinds distinct")]
    public async Task FileRelativePaths()
    {
        using var workspace = TemporaryWorkspace.Create("comment-relative");
        workspace.WriteText(GuidePath, "# Guide\n");
        workspace.WriteText("src/near.cs", "class Near {}\n");
        workspace.WriteText(CodePath, $"// {SourceCommentLinkReader.OpenForgeMarker} ../{GuidePath} reason\n// {SourceCommentLinkReader.RuneMarker} ./near.cs#Near reason\n");
        var result = await Read(workspace, $"./{CodePath}", ReferencesDirection.Out);

        Assert.Equal(CliSemanticStatus.Complete, result.Status);
        Assert.Equal([GuidePath, "src/near.cs"], result.Outgoing?.Occurrences.Select(item => item.Target.Path));
    }

    [Fact(DisplayName = "Legacy comment IDs report a finding and a real suggested path")]
    public async Task LegacyId()
    {
        using var workspace = TemporaryWorkspace.Create("comment-legacy");
        workspace.WriteText(GuidePath, "# Guide\n");
        workspace.WriteText(CodePath, $"// {SourceCommentLinkReader.RuneMarker} guide old annotation\n");
        var result = await Read(workspace, $"./{CodePath}", ReferencesDirection.Out);

        var finding = Assert.Single(result.Findings, finding => finding.Code == ReferencesFindingCode.LegacyAnchor);
        Assert.Equal(GuidePath, Assert.Single(finding.Candidates).Path);
        Assert.Equal(CliSemanticStatus.Attention, result.Status);
    }

    [Fact(DisplayName = "Comment paths cannot leave the selected workspace")]
    public async Task WorkspaceEscape()
    {
        using var workspace = TemporaryWorkspace.Create("comment-escape");
        workspace.WriteText(CodePath, $"// {SourceCommentLinkReader.OpenForgeMarker} ../../outside.md reason\n");
        var result = await Read(workspace, $"./{CodePath}", ReferencesDirection.Out);

        Assert.Equal(CliSemanticStatus.Blocked, result.Status);
        Assert.Single(result.Findings, finding => finding.Code == ReferencesFindingCode.TargetUnsafe);
    }

    private static ValueTask<ReferencesResult> Read(TemporaryWorkspace workspace, string source, ReferencesDirection direction)
        => ReferencesOperationFactory.Create().ExecuteAsync(new(new CliWorkspace(workspace.Path, workspace.Path,
            CliWorkspaceSelectionMethod.ExplicitWorkspace), source, direction, []), TestContext.Current.CancellationToken);
}
