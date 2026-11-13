using System.Text.Json;
using OpenForge.Cli.TestSupport;
using OpenForge.Cli.Core.Commands.Doctor;
using OpenForge.Cli.Core.Commands.References;
using OpenForge.Cli.Core.Commands.References.Models.Result;
using OpenForge.Cli.Core.Framework.Workspace.Models.Layout;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Operation;
using OpenForge.Cli.IntegrationTests.Commands.Shared.Snapshots;
using OpenForge.Cli.IntegrationTests.Commands.Shared.Snapshots.Models;
using OpenForge.Cli.TestSupport.Snapshots;

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
        var snapshots = new Dictionary<string, string>(StringComparer.Ordinal);
        await new ReadCommandOutputCapture(workspace.Path).MatchDetailsAsync(new ReadOutputScenario
        {
            Situation = doctor ? "doctor" : "references",
            Arguments = doctor ? [DoctorDefinitions.DoctorCommand.Name]
                : [ReferencesDefinitions.CommandIdentity, "docs/guide", ReferencesDefinitions.Direction.Name, ReferencesDefinitions.Out],
            ExitCode = 2,
        }, snapshotCollector: snapshots);
        Assert.Equal(before, workspace.Snapshot());
        if (!doctor)
        {
            using var json = JsonDocument.Parse(snapshots[$"references{CommandOutputSnapshot.JsonContentNameSegment}{CliPresentationDefinitions.Full}"]);
            var findings = json.RootElement.GetProperty("findings").EnumerateArray().ToArray();
            foreach (var (code, line) in new[] { (ReferencesFindingCode.LineOutOfRange, 12), (ReferencesFindingCode.SymbolMissing, 11) })
            {
                var finding = Assert.Single(findings, value => value.GetProperty("code").GetString() == ReferencesDefinitions.Read(code).MachineName);
                var subject = finding.GetProperty("subject");
                Assert.Equal($"{mainRoot}/docs/guide.md", subject.GetProperty("path").GetString());
                Assert.Equal(line, subject.GetProperty("location").GetProperty("line").GetInt32());
                Assert.Equal(1, subject.GetProperty("location").GetProperty("column").GetInt32());
                var action = Assert.Single(finding.GetProperty("actions").EnumerateArray());
                Assert.Equal(CliNextActionKind.Sentence.ToString().ToLowerInvariant(), action.GetProperty("kind").GetString());
            }
            Assert.Equal(CliNextActionKind.Sentence.ToString().ToLowerInvariant(), json.RootElement.GetProperty("next").GetProperty("kind").GetString());
        }
        CommandOutputSnapshot.MatchDetailSnapshot(snapshots, testName: $"{nameof(CodeFragments)}_{doctor}");
    }
}
