using System.Net.Sockets;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Libraries.Models.Identity;
using OpenForge.Cli.Core.Framework.Libraries.Models.Inventory;
using OpenForge.Cli.Core.Framework.Libraries.Shared.Inventory;
using OpenForge.Cli.Core.Framework.Workspace;
using OpenForge.Cli.TestSupport;

namespace OpenForge.Cli.IntegrationTests.Framework.Libraries.Shared.Inventory;

[Trait("Feature", "library-foundation"), Trait("Evidence", "Integration")]
public sealed class LibraryInventoryReaderIntegrationTests
{
    [Fact(DisplayName = "Library inventory includes opaque external files and excludes dot-agents controls and links")]
    public async Task InventoriesAllEligibleFilesWithoutTraversingExcludedObjects()
    {
        using var temporary = TemporaryWorkspace.Create("library-inventory");
        var source = CreateSource(temporary);
        temporary.CreateFile("shared/team/README.md", "ordinary root content");
        temporary.CreateFile("shared/team/docs/_docs.md", "opaque external entrypoint name");
        temporary.CreateFile("shared/team/docs/a.overwrite.md", "opaque external companion name");
        temporary.CreateFile("shared/team/.agents/z.txt", "z");
        var ordinary = temporary.CreateFile("shared/team/.agents/directives/a.md", "a");
        var controls = new[] { "loader.md", "open-forge.libraries.json", "open-forge.lifecycle.json", "directives/_directives.md", "directives/a.overwrite.md" };
        foreach (var control in controls)
        {
            temporary.CreateFile($"shared/team/.agents/{control}", "control");
        }

        temporary.CreateFileSymbolicLink("shared/team/.agents/linked.md", "directives/a.md");
        temporary.CreateFileSymbolicLink("shared/team/.agents/dangling.md", "absent.md");
        temporary.CreateDirectorySymbolicLink("shared/team/.agents/linked-directory", "directives");
        var before = await File.ReadAllBytesAsync(ordinary, TestContext.Current.CancellationToken);

        var result = await LibraryInventoryReader.ReadAsync(new PhysicalPathResolver(), source, TestContext.Current.CancellationToken);
        var repeated = await LibraryInventoryReader.ReadAsync(new PhysicalPathResolver(), source, TestContext.Current.CancellationToken);

        var inventory = Assert.IsType<LibraryInventory>(result.Inventory);
        Assert.Equal(LibraryInventoryState.Complete, inventory.State);
        Assert.Equal([".agents/directives/a.md", ".agents/z.txt", "README.md", "docs/_docs.md", "docs/a.overwrite.md"], inventory.Entries.Select(entry => entry.SourcePath.Value));
        Assert.Equal(ordinary, inventory.Entries[0].PhysicalPath);
        Assert.Equal(inventory.Entries, Assert.IsType<LibraryInventory>(repeated.Inventory).Entries);
        Assert.Empty(result.UnavailablePaths);
        Assert.Equal(8, result.ExcludedPaths.Length);
        Assert.Contains(result.ExcludedPaths, path => path.Path == ".agents/loader.md" && path.Kind == LibraryInventoryExclusionKind.Loader);
        Assert.Contains(result.ExcludedPaths, path => path.Path == ".agents/directives/_directives.md" && path.Kind == LibraryInventoryExclusionKind.Entrypoint);
        Assert.Contains(result.ExcludedPaths, path => path.Path == ".agents/directives/a.overwrite.md" && path.Kind == LibraryInventoryExclusionKind.Overwrite);
        Assert.Contains(result.ExcludedPaths, path => path.Path == ".agents/open-forge.libraries.json" && path.Kind == LibraryInventoryExclusionKind.ManagerControl);
        Assert.Contains(result.ExcludedPaths, path => path.Path == ".agents/dangling.md" && path.Kind == LibraryInventoryExclusionKind.Link);
        Assert.Equal(before, await File.ReadAllBytesAsync(ordinary, TestContext.Current.CancellationToken));
    }

    [Fact(DisplayName = "An ordinary empty source root produces a complete empty inventory")]
    public async Task DistinguishesEmptyFromUnavailable()
    {
        using var temporary = TemporaryWorkspace.Create("library-inventory-empty");
        var source = CreateSource(temporary);

        var result = await LibraryInventoryReader.ReadAsync(new PhysicalPathResolver(), source, TestContext.Current.CancellationToken);

        var inventory = Assert.IsType<LibraryInventory>(result.Inventory);
        Assert.Equal(LibraryInventoryState.Complete, inventory.State);
        Assert.Empty(inventory.Entries);
        Assert.Empty(result.UnavailablePaths);
    }

    [Theory(DisplayName = "Library inventory rechecks vanished or replaced source boundaries and never reports a safe prefix complete")]
    [InlineData(false), InlineData(true)]
    public static async Task RejectsSourceBoundaryRace(bool replaceWithLink)
    {
        using var temporary = TemporaryWorkspace.Create("library-inventory-race");
        var source = CreateSource(temporary);
        var agents = temporary.Combine("shared/team");
        Directory.Delete(agents, recursive: true);
        if (replaceWithLink)
        {
            temporary.CreateDirectory("elsewhere");
            Directory.CreateSymbolicLink(agents, "../elsewhere");
        }

        try
        {
            var result = await LibraryInventoryReader.ReadAsync(new PhysicalPathResolver(), source, TestContext.Current.CancellationToken);

            Assert.True(result.Inventory is null || result.Inventory.State != LibraryInventoryState.Complete);
            Assert.NotEmpty(result.UnavailablePaths);
        }
        finally
        {
            if (replaceWithLink)
            {
                Directory.Delete(agents);
            }
            Directory.CreateDirectory(agents);
        }
    }

    [Fact(DisplayName = "Unreadable eligible source files prevent complete inventory and preserve readable siblings")]
    public async Task InaccessibleEligibleFileDoesNotBecomeCompletePrefix()
    {
        if (OperatingSystem.IsWindows())
        {
            Assert.Skip("This evidence requires Unix permissions and Unix-domain sockets.");
            return;
        }
        using var temporary = TemporaryWorkspace.Create("library-inventory-access");
        var source = CreateSource(temporary);
        temporary.CreateFile("shared/team/.agents/a.md", "readable");
        var unreadable = temporary.CreateFile("shared/team/.agents/z.md", "unreadable");
        var mode = File.GetUnixFileMode(unreadable);
        File.SetUnixFileMode(unreadable, UnixFileMode.None);
        try
        {
            var result = await LibraryInventoryReader.ReadAsync(new PhysicalPathResolver(), source, TestContext.Current.CancellationToken);

            Assert.True(result.Inventory is null || result.Inventory.State != LibraryInventoryState.Complete);
            Assert.Contains(result.UnavailablePaths, path => path.Path == ".agents/z.md");
        }
        finally
        {
            File.SetUnixFileMode(unreadable, mode);
        }
    }

    [Fact(DisplayName = "Library inventory excludes a real socket without reading it as ordinary source bytes")]
    public async Task ExcludesSpecialSourceObject()
    {
        if (OperatingSystem.IsWindows())
        {
            Assert.Skip("This evidence requires Unix permissions and Unix-domain sockets.");
            return;
        }
        using var temporary = TemporaryWorkspace.Create("library-special");
        var source = CreateSource(temporary);
        temporary.CreateFile("shared/team/.agents/a.md", "ordinary");
        var path = temporary.Combine("shared/team/.agents/socket");
        using var socket = new Socket(AddressFamily.Unix, SocketType.Stream, ProtocolType.Unspecified);
        socket.Bind(new UnixDomainSocketEndPoint(path));
        try
        {
            var result = await LibraryInventoryReader.ReadAsync(new PhysicalPathResolver(), source, TestContext.Current.CancellationToken);

            var inventory = Assert.IsType<LibraryInventory>(result.Inventory);
            Assert.Equal(LibraryInventoryState.Complete, inventory.State);
            Assert.Equal(".agents/a.md", Assert.Single(inventory.Entries).SourcePath.Value);
            Assert.Contains(result.ExcludedPaths, item => item.Path == ".agents/socket" && item.Kind == LibraryInventoryExclusionKind.Special);
        }
        finally
        {
            socket.Dispose();
            File.Delete(path);
        }
    }

    private static LibrarySourceRootObservation CreateSource(TemporaryWorkspace temporary)
    {
        temporary.CreateDirectory(".agents");
        var physical = temporary.CreateDirectory("shared/team");
        _ = temporary.CreateDirectory("shared/team/.agents");
        return new LibrarySourceRootObservation
        {
            Request = new LibrarySourceRootRequest
            {
                Workspace = new CliWorkspace(temporary.Path, temporary.Path, CliWorkspaceSelectionMethod.ExplicitWorkspace),
                SourceRoot = WorkspaceRelativeDirectory.Create("shared/team"),
            },
            State = LibrarySourceRootState.Available,
            LexicalSourceRoot = physical,
            PhysicalSourceRoot = physical,
            LexicallyContained = true,
            PhysicallyContained = true,
            Cause = null,
        };
    }
}
