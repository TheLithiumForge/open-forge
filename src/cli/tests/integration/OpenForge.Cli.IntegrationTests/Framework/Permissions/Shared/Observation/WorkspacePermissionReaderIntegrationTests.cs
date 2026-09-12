using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Files;
using OpenForge.Cli.Core.Framework.Permissions.Models.Document;
using OpenForge.Cli.Core.Framework.Permissions.Models.Observation;
using OpenForge.Cli.Core.Framework.Permissions.Shared.Observation;
using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.TestSupport;

namespace OpenForge.Cli.IntegrationTests.Framework.Permissions.Shared.Observation;

[Trait("Feature", "workspace-permissions"), Trait("Evidence", "Integration")]
public sealed class WorkspacePermissionReaderIntegrationTests
{
    private const string RelativePath = ".agents/open-forge.permissions.json";
    private const string EmptyDocument = """{"schemaVersion":1,"extensions":[],"libraries":[]}""";

    [Theory]
    [InlineData(false), InlineData(true)]
    public static async Task ReadsExactConsumerFileWithoutWriting(bool present)
    {
        using var temporary = TemporaryWorkspace.Create("permission-read");
        temporary.CreateDirectory(".agents");
        temporary.CreateFile("open-forge.permissions.json", "root lookalike is not permission");
        if (present)
        {
            temporary.CreateFile(RelativePath, EmptyDocument);
        }
        var before = temporary.SnapshotHashes();

        var result = await WorkspacePermissionReader.ReadAsync(
            new PhysicalPathResolver(), Workspace(temporary), TestContext.Current.CancellationToken);

        Assert.Equal(present ? WorkspacePermissionReadState.Complete : WorkspacePermissionReadState.Missing, result.State);
        var snapshot = Assert.IsType<FileStateSnapshot>(result.Snapshot);
        Assert.Equal(present ? FileExpectationKind.File : FileExpectationKind.Missing, snapshot.Kind);
        if (present)
        {
            var document = Assert.IsType<WorkspacePermissionDocument>(result.Document);
            Assert.Empty(document.Extensions);
            Assert.Empty(document.Libraries);
            Assert.Equal(System.Text.Encoding.UTF8.GetBytes(EmptyDocument), snapshot.Bytes);
        }
        Assert.Null(result.Cause);
        Assert.Equal(before, temporary.SnapshotHashes());
    }

    [Theory]
    [InlineData("leaf"), InlineData("parent"), InlineData("directory"), InlineData("dangling")]
    public static async Task UnsafeStorageCannotSupplyApproval(string scenario)
    {
        using var temporary = TemporaryWorkspace.Create("permission-boundary");
        var actual = temporary.CreateFile("actual/open-forge.permissions.json", EmptyDocument);
        switch (scenario)
        {
            case "leaf":
                temporary.CreateFileSymbolicLink(RelativePath, "../actual/open-forge.permissions.json");
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

        var result = await WorkspacePermissionReader.ReadAsync(
            new PhysicalPathResolver(), Workspace(temporary), TestContext.Current.CancellationToken);

        Assert.Equal(WorkspacePermissionReadState.Blocked, result.State);
        Assert.Null(result.Document);
        Assert.NotNull(result.Cause);
        Assert.Equal(EmptyDocument, await File.ReadAllTextAsync(actual, TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task MalformedStorageIsNotMissingAndPreservesExactBytes()
    {
        using var temporary = TemporaryWorkspace.Create("permission-malformed");
        temporary.CreateFile(RelativePath, "{ broken consumer file\n");
        var before = temporary.SnapshotHashes();

        var result = await WorkspacePermissionReader.ReadAsync(
            new PhysicalPathResolver(), Workspace(temporary), TestContext.Current.CancellationToken);

        Assert.Equal(WorkspacePermissionReadState.Invalid, result.State);
        Assert.Null(result.Document);
        Assert.NotNull(result.Snapshot);
        Assert.NotNull(result.Cause);
        Assert.Equal(before, temporary.SnapshotHashes());
    }

    [Fact]
    public async Task CancellationIsNotPermissionAbsence()
    {
        using var temporary = TemporaryWorkspace.Create("permission-cancelled");
        using var cancellation = new CancellationTokenSource();
        await cancellation.CancelAsync();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(async () =>
            await WorkspacePermissionReader.ReadAsync(new PhysicalPathResolver(), Workspace(temporary), cancellation.Token));
    }

    [Fact]
    public async Task MissingParentDoesNotCreatePermissionInfrastructure()
    {
        using var temporary = TemporaryWorkspace.Create("permission-missing-parent");

        var result = await WorkspacePermissionReader.ReadAsync(
            new PhysicalPathResolver(), Workspace(temporary), TestContext.Current.CancellationToken);

        Assert.Equal(WorkspacePermissionReadState.Missing, result.State);
        Assert.Null(result.Document);
        Assert.Equal(FileExpectationKind.Missing, Assert.IsType<FileStateSnapshot>(result.Snapshot).Kind);
        Assert.False(Directory.Exists(temporary.Combine(".agents")));
    }

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

            var result = await WorkspacePermissionReader.ReadAsync(
                new PhysicalPathResolver(), Workspace(temporary), TestContext.Current.CancellationToken);

            Assert.Equal(WorkspacePermissionReadState.Unavailable, result.State);
            Assert.Null(result.Document);
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
