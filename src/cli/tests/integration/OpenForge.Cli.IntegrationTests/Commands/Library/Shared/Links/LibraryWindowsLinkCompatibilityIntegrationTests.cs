using System.Text.Json;
using OpenForge.Cli.Core.Commands.Library.Detach;
using OpenForge.Cli.Core.Commands.Library.Inspect;
using OpenForge.Cli.Core.Commands.Library.Inspect.Models.Request;
using OpenForge.Cli.Core.Commands.Library.Models.Request;
using OpenForge.Cli.Core.Commands.Library.Models.Result.Coordinates.Observation;
using OpenForge.Cli.Core.Commands.Library.Sync;
using OpenForge.Cli.Core.Framework.Libraries.Models.Identity;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.IntegrationTests.Commands.Library.Shared.Mutation;
using OpenForge.Cli.IntegrationTests.Hosting;

namespace OpenForge.Cli.IntegrationTests.Commands.Library.Shared.Links;

[Trait("Evidence", "Integration"), Trait("Feature", "library-mapping")]
public sealed class LibraryWindowsLinkCompatibilityIntegrationTests
{
    private const string DestinationRoot = ".agents/libraries/team";
    private const string Leaf = "kind.md";

    [Theory, Trait("Boundary", "OS")]
    [InlineData(false), InlineData(true)]
    public async Task NativeLinksAreCurrentAndLegacyLinksRequireReattachment(bool legacy)
    {
        using var workspace = new LibraryMutationWorkspace();
        workspace.Source(Leaf);
        workspace.RecordAt(DestinationRoot, Leaf);
        workspace.Directory(DestinationRoot);
        var source = workspace.Absolute($"{LibraryMutationWorkspace.SourceRoot}/{Leaf}");
        var destination = workspace.Absolute($"{DestinationRoot}/{Leaf}");
        var parent = Path.GetDirectoryName(destination) ?? throw new InvalidOperationException("Missing link parent.");
        var portable = Path.GetRelativePath(parent, source).Replace('\\', '/');
        var raw = legacy ? portable : portable.Replace('/', Path.DirectorySeparatorChar);
        workspace.RawLink($"{DestinationRoot}/{Leaf}", raw);
        var unreadable = legacy && OperatingSystem.IsWindows();
        var before = workspace.Snapshot();
        var inspect = await new LibraryInspectOperation().ExecuteAsync(new LibraryInspectRequest
        {
            Workspace = workspace.Workspace,
            LibraryId = LibraryId.Create("team-knowledge"),
        }, TestContext.Current.CancellationToken);
        Assert.Equal(unreadable ? CliSemanticStatus.Attention : CliSemanticStatus.Complete, inspect.Status);
        var comparison = Assert.Single(inspect.Result.Projection.Comparisons);
        Assert.Equal(unreadable ? LibraryComparisonRelation.Changed : LibraryComparisonRelation.Current, comparison.Relation);
        Assert.Equal(raw, comparison.ObservedRelativeLink);
        Assert.Equal(unreadable, inspect.LinkRecreation is not null);
        foreach (var mode in new[] { LibraryMode.DryRun, LibraryMode.Apply })
        {
            var sync = await new LibrarySyncOperation(workspace.Permissions).ExecuteAsync(workspace.Sync(mode), TestContext.Current.CancellationToken);
            Assert.Equal(unreadable ? CliSemanticStatus.Blocked : CliSemanticStatus.Complete, sync.Status);
            var mapping = Assert.Single(sync.Result.Projection.Mappings);
            Assert.Equal(unreadable ? LibraryLinkViewState.Changed : LibraryLinkViewState.Current, mapping.State);
            Assert.Equal(raw, mapping.ObservedRelativeLink);
            Assert.Equal(unreadable, sync.LinkRecreation is not null);
            Assert.Empty(sync.Result.Plan.Links);
            Assert.Empty(sync.Result.Plan.Directories);
            Assert.Empty(sync.Result.Plan.GeneratedRegions);
            Assert.Equal(before, workspace.Snapshot());
        }
        if (!unreadable)
        {
            Assert.Equal(LibraryMutationWorkspace.SourceBytes, File.ReadAllText(destination));
        }
        var detach = await new LibraryDetachOperation(workspace.Permissions).ExecuteAsync(workspace.Detach(LibraryMode.Apply), TestContext.Current.CancellationToken);
        Assert.Equal(CliSemanticStatus.Complete, detach.Status);
        Assert.Null(new FileInfo(destination).LinkTarget);
        Assert.Equal(LibraryMutationWorkspace.SourceBytes, File.ReadAllText(source));
    }

    [Theory, Trait("Boundary", "Host")]
    [InlineData("library", "inspect"), InlineData("library", "sync"), InlineData("doctor", null)]
    public async Task LegacyDiagnosticsPreserveRawEvidenceAndExactRecreationCommands(string command, string? operation)
    {
        using var workspace = new LibraryMutationWorkspace();
        workspace.ConsumerRoute();
        workspace.Source(Leaf);
        workspace.RecordAt(DestinationRoot, Leaf);
        workspace.Directory(DestinationRoot);
        const string raw = "../../../shared/team-knowledge/kind.md";
        workspace.RawLink($"{DestinationRoot}/{Leaf}", raw);
        var before = workspace.Snapshot();
        List<string> arguments = [command];
        if (operation is not null)
        {
            arguments.AddRange([operation, "team-knowledge"]);
            if (operation == "sync") arguments.Add("--dry-run");
        }
        arguments.AddRange(["--format", "json", "--detail", "full"]);
        var run = await CliHostCapture.RunAsync(arguments.ToArray(), workspace.Path);
        using var json = JsonDocument.Parse(run.Output);
        var root = json.RootElement;
        if (OperatingSystem.IsWindows())
        {
            Assert.Contains(root.GetProperty("findings").EnumerateArray(), finding =>
                finding.GetProperty("message").GetString()?.Contains("unreadable on Windows", StringComparison.Ordinal) == true);
            var next = operation is null
                ? root.GetProperty("findings").EnumerateArray().First(finding => finding.GetProperty("code").GetString() == "library.projection-retargeted").GetProperty("actions")[0]
                : root.GetProperty("next");
            var advice = Assert.IsType<string>(next.GetProperty("command").GetString());
            Assert.Contains("library detach team-knowledge --automatic", advice, StringComparison.Ordinal);
            Assert.Contains("library attach team-knowledge shared/team-knowledge --to .agents/libraries/team --git-ignore false --automatic", advice, StringComparison.Ordinal);
            Assert.Contains(workspace.Path.Replace("'", "''", StringComparison.Ordinal), advice, StringComparison.Ordinal);
            Assert.Contains("Remove Library ID 'team-knowledge' from removedLibraries", advice, StringComparison.Ordinal);
            Assert.Contains(Path.Combine(workspace.Path, ".agents", "open-forge.json").Replace("'", "''", StringComparison.Ordinal), advice, StringComparison.Ordinal);
            Assert.Contains(raw, run.Output, StringComparison.Ordinal);
        }
        Assert.Equal(before, workspace.Snapshot());
    }
}
