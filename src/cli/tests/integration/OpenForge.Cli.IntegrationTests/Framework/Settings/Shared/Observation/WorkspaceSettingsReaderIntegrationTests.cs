using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Files;
using OpenForge.Cli.Core.Framework.Settings.Models.Document;
using OpenForge.Cli.Core.Framework.Settings.Models.Observation;
using OpenForge.Cli.Core.Framework.Settings.Shared.Observation;
using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.TestSupport;

namespace OpenForge.Cli.IntegrationTests.Framework.Settings.Shared.Observation;

[Trait("Feature", "workspace-permissions"), Trait("Evidence", "Integration")]
public sealed class WorkspaceSettingsReaderIntegrationTests
{
    private const string RelativePath = ".agents/open-forge.json";
    private const string EmptyDocument = """{"allowInstallPaths":[]}""";

    [Trait("Boundary", "OS")]
    [Theory]
    [InlineData(false), InlineData(true)]
    public static async Task ReadsExactConsumerFileWithoutWriting(bool present)
    {
        using var temporary = TemporaryWorkspace.Create("permission-read");
        temporary.CreateDirectory(".agents");
        temporary.CreateFile("open-forge.json", "root lookalike is not permission");
        if (present)
        {
            temporary.CreateFile(RelativePath, EmptyDocument);
        }
        var before = temporary.SnapshotHashes();

        var result = await WorkspaceSettingsReader.ReadAsync(
            new PhysicalPathResolver(), Workspace(temporary), TestContext.Current.CancellationToken);

        Assert.Equal(present ? WorkspaceSettingsReadState.Complete : WorkspaceSettingsReadState.Absent, result.State);
        var snapshot = Assert.IsType<FileStateSnapshot>(result.Snapshot);
        Assert.Equal(present ? FileExpectationKind.File : FileExpectationKind.Missing, snapshot.Kind);
        if (present)
        {
            var document = Assert.IsType<WorkspaceSettingsDocument>(result.Document);
            Assert.Empty(document.AllowInstallPaths);
            Assert.Equal(System.Text.Encoding.UTF8.GetBytes(EmptyDocument), snapshot.Bytes);
        }
        Assert.Null(result.Cause);
        Assert.Equal(before, temporary.SnapshotHashes());
    }

    [Trait("Boundary", "OS")]
    [Theory]
    [InlineData("leaf"), InlineData("parent"), InlineData("directory"), InlineData("dangling")]
    public static async Task UnsafeStorageCannotSupplyApproval(string scenario)
    {
        using var temporary = TemporaryWorkspace.Create("permission-boundary");
        var actual = temporary.CreateFile("actual/open-forge.json", EmptyDocument);
        switch (scenario)
        {
            case "leaf":
                temporary.CreateFileSymbolicLink(RelativePath, "../actual/open-forge.json");
                break;
            case "parent":
                temporary.CreateDirectorySymbolicLink(".agents", "actual");
                break;
            case "directory":
                temporary.CreateDirectory(RelativePath);
                break;
            case "dangling":
                temporary.CreateFileSymbolicLink(RelativePath, "missing.json");
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(scenario));
        }

        var result = await WorkspaceSettingsReader.ReadAsync(
            new PhysicalPathResolver(), Workspace(temporary), TestContext.Current.CancellationToken);

        Assert.Equal(WorkspaceSettingsReadState.Unavailable, result.State);
        Assert.Empty(result.Document.AllowInstallPaths);
        Assert.NotNull(result.Cause);
        Assert.Equal(EmptyDocument, await File.ReadAllTextAsync(actual, TestContext.Current.CancellationToken));
    }

    [Trait("Boundary", "OS")]
    [Fact]
    public async Task MalformedStorageIsNotMissingAndPreservesExactBytes()
    {
        using var temporary = TemporaryWorkspace.Create("permission-malformed");
        temporary.CreateFile(RelativePath, "{ broken consumer file\n");
        var before = temporary.SnapshotHashes();

        var result = await WorkspaceSettingsReader.ReadAsync(
            new PhysicalPathResolver(), Workspace(temporary), TestContext.Current.CancellationToken);

        Assert.Equal(WorkspaceSettingsReadState.Invalid, result.State);
        Assert.Empty(result.Document.AllowInstallPaths);
        Assert.NotNull(result.Snapshot);
        Assert.NotNull(result.Cause);
        Assert.Equal(before, temporary.SnapshotHashes());
    }

    [Trait("Boundary", "OS")]
    [Fact]
    public async Task CancellationIsNotPermissionAbsence()
    {
        using var temporary = TemporaryWorkspace.Create("permission-cancelled");
        using var cancellation = new CancellationTokenSource();
        await cancellation.CancelAsync();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(async () =>
            await WorkspaceSettingsReader.ReadAsync(new PhysicalPathResolver(), Workspace(temporary), cancellation.Token));
    }

    [Trait("Boundary", "OS")]
    [Fact]
    public async Task MissingParentDoesNotCreatePermissionInfrastructure()
    {
        using var temporary = TemporaryWorkspace.Create("permission-missing-parent");

        var result = await WorkspaceSettingsReader.ReadAsync(
            new PhysicalPathResolver(), Workspace(temporary), TestContext.Current.CancellationToken);

        Assert.Equal(WorkspaceSettingsReadState.Absent, result.State);
        Assert.Empty(result.Document.AllowInstallPaths);
        Assert.Equal(FileExpectationKind.Missing, Assert.IsType<FileStateSnapshot>(result.Snapshot).Kind);
        Assert.False(Directory.Exists(temporary.Combine(".agents")));
    }

    [Trait("Boundary", "OS")]
    [Fact]
    public async Task InaccessiblePermissionDoesNotBecomeEmptyApproval()
    {
        if (OperatingSystem.IsWindows())
        {
            Assert.Skip("This read boundary uses Unix file permissions.");
            return;
        }
        using var temporary = TemporaryWorkspace.Create("permission-unreadable");
        var path = temporary.CreateFile(RelativePath, EmptyDocument);
        var mode = File.GetUnixFileMode(path);
        try
        {
            File.SetUnixFileMode(path, UnixFileMode.None);

            var result = await WorkspaceSettingsReader.ReadAsync(
                new PhysicalPathResolver(), Workspace(temporary), TestContext.Current.CancellationToken);

            Assert.Equal(WorkspaceSettingsReadState.Unavailable, result.State);
            Assert.Empty(result.Document.AllowInstallPaths);
            Assert.NotNull(result.Cause);
        }
        finally
        {
            File.SetUnixFileMode(path, mode);
        }
        Assert.Equal(EmptyDocument, File.ReadAllText(path));
    }

    private static CliWorkspace Workspace(TemporaryWorkspace temporary)
        => new(temporary.Path, temporary.Path, CliWorkspaceSelectionMethod.ExplicitWorkspace);
}
