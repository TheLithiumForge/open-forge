using System.Text;
using OpenForge.Cli.Core.Framework.Distribution;
using OpenForge.Cli.Core.Framework.Extensions;
using OpenForge.Cli.Core.Framework.Extensions.Operational;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Distribution.Operational;
using OpenForge.Cli.Core.Framework.OperationalContributors.Models;
using OpenForge.Cli.Core.Framework.Ownership;
using OpenForge.Cli.Core.Framework.Ownership.Models.Document;
using OpenForge.Cli.Core.Framework.Ownership.Shared.Observation;
using OpenForge.Cli.Core.Framework.Ownership.Shared.Serialization;
using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.TestSupport;

namespace OpenForge.Cli.IntegrationTests.Framework.Ownership;

[Trait("Feature", "ownership-operational-readers"), Trait("Evidence", "Integration")]
public sealed class OwnershipOperationalReaderIntegrationTests
{
    [Trait("Boundary", "OS")]
    [Theory(DisplayName = "Framework compares lock-owned targets with the running payload and preserves actual read boundaries")]
    [InlineData("normalized", "current")]
    [InlineData("changed", "changed")]
    [InlineData("missing", "missing")]
    [InlineData("directory", "unavailable")]
    [InlineData("external", "blocked")]
    [InlineData("scoped", "current")]
    [InlineData("authored-host", "current")]
    public async Task FrameworkCurrentness(string scenario, string expected)
    {
        using var files = TemporaryWorkspace.Create("framework-lock-currentness");
        using var outside = TemporaryWorkspace.Create("framework-lock-outside");
        var workspace = Workspace(files);
        var path = scenario == "scoped" ? ".agents/memory/team/working/_working.md"
            : scenario == "authored-host" ? "AGENTS.md" : ".agents/loader.md";
        var sourcePath = scenario == "scoped" ? ".agents/memory/working/_working.md" : path;
        var payload = EmbeddedFrameworkPayloadReader.Read().Payload!;
        var content = Encoding.UTF8.GetString(payload.Find(sourcePath)!.Bytes.AsSpan());
        var ownership = new FrameworkOwnership(new("open-forge", "older-release"),
            scenario == "authored-host" ? [] : [path],
            scenario == "authored-host" ? [new(path, "open-forge")] : []);
        WriteLock(files, new(1, ownership, [], []));
        files.WriteText(".agents/open-forge.lifecycle.json", "unreadable old state must be ignored");
        if (scenario == "directory")
        {
            files.CreateDirectory(path);
        }
        else if (scenario == "external")
        {
            var externalPath = outside.CreateFile("outside.md", content);
            files.CreateFileSymbolicLink(path, externalPath);
        }
        else if (scenario != "missing")
        {
            var actual = scenario == "changed" ? content.Replace("## Entries", "Authored change.\n\n## Entries", StringComparison.Ordinal)
                : scenario == "authored-host" ? "# Personal instructions\n\n" + content + "\nKeep this tail.\n"
                : content.Replace("\r\n", "\n", StringComparison.Ordinal).Replace("\n", "\r\n", StringComparison.Ordinal);
            files.WriteText(path, actual);
        }
        var before = files.SnapshotHashes();
        var outsideBefore = outside.SnapshotHashes();
        var resolver = new PhysicalPathResolver();
        var contributor = new FrameworkLifecycleOperationalContributor(resolver, new(resolver));
        var read = await WorkspaceOwnershipReader.ReadAsync(resolver, workspace, TestContext.Current.CancellationToken);

        var status = await contributor.ReadStatusAsync(workspace, read, TestContext.Current.CancellationToken);
        var doctor = await contributor.ReadDoctorAsync(workspace, TestContext.Current.CancellationToken);

        var target = Assert.Single(status.Targets);
        Assert.Equal(expected, target.State.ToString().ToLowerInvariant());
        Assert.Equal(target, Assert.Single(doctor.Targets).Target);
        Assert.Equal(sourcePath, target.SourceAssetPath);
        Assert.Equal(before, files.SnapshotHashes());
        Assert.Equal(outsideBefore, outside.SnapshotHashes());
    }

    [Trait("Boundary", "OS")]
    [Theory(DisplayName = "Extension currentness uses current source bytes instead of recorded version or legacy content")]
    [InlineData("normalized", "current")]
    [InlineData("changed", "changed")]
    [InlineData("final-newline", "changed")]
    [InlineData("retired", "unavailable")]
    [InlineData("missing-retired", "missing")]
    public async Task ExtensionCurrentness(string scenario, string expected)
    {
        using var files = TemporaryWorkspace.Create("extension-lock-currentness");
        using var source = TemporaryWorkspace.Create("extension-current-payload");
        const string path = ".agents/example.md";
        const string content = "# Example\n\nCurrent release content.\n";
        source.WriteText("extension.json", """{"id":"example","name":"Example","description":"Comparison fixture","version":"2.0.0","dependencies":[]}""");
        source.WriteText($"content/{(scenario.Contains("retired", StringComparison.Ordinal) ? ".agents/other.md" : path)}", content);
        WriteLock(files, new(1, null, [new("example", "1.0.0", source.Path, [], [path], [])], []));
        files.WriteText(".agents/open-forge.lifecycle.json", "old state must be ignored");
        if (scenario != "missing-retired")
        {
            files.WriteText(path, scenario == "changed" ? content + "\nAn edit.\n" : scenario == "final-newline" ? content.TrimEnd('\n') : content.Replace("\n", "\r\n", StringComparison.Ordinal));
        }
        var before = files.SnapshotHashes();
        var sourceBefore = source.SnapshotHashes();
        var resolver = new PhysicalPathResolver();
        var contributor = new ExtensionLifecycleOperationalContributor(resolver, new ExtensionSourceReader(resolver), new(resolver));
        var workspace = Workspace(files);
        var read = await WorkspaceOwnershipReader.ReadAsync(resolver, workspace, TestContext.Current.CancellationToken);

        var status = await contributor.ReadStatusAsync(workspace, read, TestContext.Current.CancellationToken);
        var doctor = await contributor.ReadDoctorAsync(workspace, TestContext.Current.CancellationToken);

        Assert.Equal(expected, Assert.Single(status.Targets).State.ToString().ToLowerInvariant());
        Assert.Equal(expected, Assert.Single(doctor.Targets).Target.State.ToString().ToLowerInvariant());
        Assert.Equal("1.0.0", Assert.Single(status.Installed).Version);
        Assert.Equal(before, files.SnapshotHashes());
        Assert.Equal(sourceBefore, source.SnapshotHashes());
    }

    private static CliWorkspace Workspace(TemporaryWorkspace files)
        => new(files.Path, files.Path, CliWorkspaceSelectionMethod.ExplicitWorkspace);

    private static void WriteLock(TemporaryWorkspace files, WorkspaceOwnershipDocument document)
        => files.WriteBytes(WorkspaceOwnershipDefinitions.RelativePath, WorkspaceOwnershipCodec.Write(document));
}
