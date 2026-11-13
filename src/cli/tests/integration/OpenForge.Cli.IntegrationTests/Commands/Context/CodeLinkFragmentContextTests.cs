using System.Text.Json;
using OpenForge.Cli.Core.Commands.Context;
using OpenForge.Cli.Core.Commands.Context.Models.Result;
using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Framework.Sources.References.Shared.Comments;
using OpenForge.Cli.Core.Framework.Workspace.Models.Layout;
using OpenForge.Cli.Core.Presentation.Context.Shared.Wording;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Operation;
using OpenForge.Cli.IntegrationTests.Commands.Shared.Snapshots;
using OpenForge.Cli.IntegrationTests.Commands.Shared.Snapshots.Models;
using OpenForge.Cli.TestSupport;
using OpenForge.Cli.TestSupport.Snapshots;

namespace OpenForge.Cli.IntegrationTests.Commands.Context;

[Trait("Feature", "context"), Trait("Evidence", "Integration"), Trait("Boundary", "Output")]
public sealed class CodeLinkFragmentContextTests
{
    private const string CodePath = "Code.cs";

    [Theory(DisplayName = "Context checks code files without selecting their bodies and gives manual fragment corrections")]
    [InlineData(null, null, false, false)]
    [InlineData("L2-L4", null, false, false)]
    [InlineData("Type.Member", null, false, false)]
    [InlineData("L4-L5", nameof(ContextFindingCode.LineOutOfRange), false, false)]
    [InlineData("Type.Missing", nameof(ContextFindingCode.SymbolMissing), false, false)]
    [InlineData("L4-L5", nameof(ContextFindingCode.LineOutOfRange), true, false)]
    [InlineData("Type.Missing", nameof(ContextFindingCode.SymbolMissing), true, false)]
    [InlineData("L2-L4", null, false, true)]
    [InlineData("Type.Member", null, false, true)]
    [InlineData("L4-L5", nameof(ContextFindingCode.LineOutOfRange), true, true)]
    [InlineData("Type.Missing", nameof(ContextFindingCode.SymbolMissing), false, true)]
    public async Task CodeFragments(string? fragment, string? codeName, bool overwrite, bool comment)
    {
        var code = codeName is null ? (ContextFindingCode?)null : Enum.Parse<ContextFindingCode>(codeName);
        using var workspace = new RoutedOutputWorkspace();
        var mainRoot = WorkspaceLayout.DefaultMainRoot;
        workspace.Replace($"{mainRoot}/{WorkspaceLayoutNames.LoaderFileName}", OpenForgeDocumentSeed.GeneratedEntries(new GeneratedEntriesSeed
        {
            Entries = "- [Documents](docs/_docs.md) - #Docs",
            Prefix = "# Open Forge Loader\n\n## Axioms\n\n- inherited - Follow the workspace instructions.",
            IncludeFinalLineEnding = true,
        }));
        workspace.Write(CodePath, "class Type\n{\n    void Member() {}\n}\n");
        var destination = (comment ? CodePath : "../../" + CodePath) + (fragment is null ? string.Empty : "#" + fragment);
        var basePath = $"{mainRoot}/docs/guide.md";
        var sourcePath = overwrite ? SourceOverwritePath.ReadAdjacentPath(basePath) : basePath;
        if (overwrite)
            workspace.Replace(basePath, OpenForgeDocumentSeed.Metadata(description: "Guide", tags: ["Docs", "Guide"], body: "# Guide\n"));
        var link = comment ? $"<!-- {SourceCommentLinkReader.OpenForgeMarker} {destination} explains the code -->" : $"[Code]({destination})";
        var guide = OpenForgeDocumentSeed.Metadata(
            description: "Guide", tags: ["Docs", "Guide"], body: $"# Guide\n\n{link}\n");
        if (overwrite) workspace.Write(sourcePath, guide);
        else workspace.Replace(sourcePath, guide);
        var before = workspace.Snapshot();
        var snapshots = new Dictionary<string, string>(StringComparer.Ordinal);
        await new ReadCommandOutputCapture(workspace.Path).MatchDetailsAsync(new ReadOutputScenario
        {
            Situation = "code-fragment",
            Arguments = [ContextDefinitions.CommandIdentity, "docs/guide", ContextDefinitions.FollowLinks.Name, "1"],
            ExitCode = code is null ? 0 : 3,
        }, snapshotCollector: snapshots);
        Assert.Equal(before, workspace.Snapshot());
        using var json = JsonDocument.Parse(snapshots[$"code-fragment{CommandOutputSnapshot.JsonContentNameSegment}{CliPresentationDefinitions.Full}"]);
        var root = json.RootElement;
        Assert.DoesNotContain(root.GetProperty("data").GetProperty("sources").EnumerateArray(), source => source.GetProperty("path").GetString() == CodePath);
        var checkedLink = Assert.Single(root.GetProperty("data").GetProperty("links").EnumerateArray());
        Assert.Equal(CodePath, checkedLink.GetProperty("resolvedPath").GetString());
        Assert.False(checkedLink.GetProperty("followed").GetBoolean());
        if (code is { } findingCode)
        {
            var finding = Assert.Single(root.GetProperty("findings").EnumerateArray());
            Assert.Equal(ContextDefinitions.Read(findingCode).Code, finding.GetProperty("code").GetString());
            var subject = finding.GetProperty("subject");
            Assert.Equal(sourcePath, subject.GetProperty("path").GetString());
            Assert.Equal(8, subject.GetProperty("location").GetProperty("line").GetInt32());
            Assert.Equal(comment ? 6 : 1, subject.GetProperty("location").GetProperty("column").GetInt32());
            var action = Assert.Single(finding.GetProperty("actions").EnumerateArray());
            Assert.Equal(CliNextActionKind.Sentence.ToString().ToLowerInvariant(), action.GetProperty("kind").GetString());
            Assert.Equal(CliNextActionKind.Sentence.ToString().ToLowerInvariant(), root.GetProperty("next").GetProperty("kind").GetString());
        }
        else
        {
            Assert.Empty(root.GetProperty("findings").EnumerateArray());
            Assert.Equal(ContextWording.Resolution(ContextLinkResolution.Complete), checkedLink.GetProperty("resolution").GetString());
            Assert.Equal(JsonValueKind.Null, root.GetProperty("next").ValueKind);
        }
        CommandOutputSnapshot.MatchDetailSnapshot(snapshots, testName: $"{nameof(CodeFragments)}_{fragment ?? "File"}{(overwrite ? "_Overwrite" : string.Empty)}{(comment ? "_Comment" : string.Empty)}");
    }
}
