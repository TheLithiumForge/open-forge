using System.Text;
using System.Text.Json;
using OpenForge.Cli.Core.Framework.Filesystem.Models;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths.Models;
using OpenForge.Cli.Core.Framework.Mutation.Locking.Models;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Files;
using OpenForge.Cli.Core.Framework.Mutation.Validation.Models;
using OpenForge.Cli.Core.Framework.Settings;
using OpenForge.Cli.Core.Framework.Settings.Models.Observation;
using OpenForge.Cli.Core.Framework.Settings.Shared.Mutation;
using OpenForge.Cli.Core.Framework.Settings.Shared.Observation;
using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.Core.Framework.Workspace.Models.Layout;
using OpenForge.Cli.Core.Framework.Workspace.Shared.Paths;
using OpenForge.Cli.Core.Framework.Workspace.Shared.Runtime;
using OpenForge.Cli.IntegrationTests.TestSupport;
using OpenForge.Cli.TestSupport;

namespace OpenForge.Cli.IntegrationTests.Framework.Settings.Shared.Observation;

[Trait("Feature", "workspace-layout"), Trait("Evidence", "Integration"), Trait("Boundary", "OS")]
public sealed class WorkspaceOutsideRouteReaderIntegrationTests
{
    private const string Operation = "outside-observation";
    private const string MainParent = "knowledge";
    private const string MainRoot = MainParent + "/main";
    private const string Parent = "docs";
    private const string Route = Parent + "/team";
    private const string OtherRoute = "reference";
    private const string Alias = "alias";
    private const string Nested = "nested";
    private const string RetiredMain = "retired-main";
    private const string Content = "Authored catalogue bytes\r\n---\r\nnot valid metadata: [\r\n";
    private const string ChangedContent = "Changed catalogue bytes\n";

    [Theory(DisplayName = "Outside directories retain declaration order and exact ordinary catalogue bytes without effects")]
    [InlineData(false), InlineData(true)]
    public async Task AvailableDirectories(bool customRoot)
    {
        using var temporary = TemporaryWorkspace.Create(Operation);
        Settings(temporary, [OtherRoute, Route], customRoot ? MainRoot : WorkspaceLayout.DefaultMainRoot);
        temporary.CreateDirectory(OtherRoute);
        temporary.CreateFile(Anchor(Route), Content);
        temporary.CreateFile(Route + "/" + Nested + "/unobserved.txt", ChangedContent);
        var before = temporary.SnapshotHashes();
        var directories = Directory.GetDirectories(temporary.Path, "*", SearchOption.AllDirectories);

        var read = await Read(temporary);

        Assert.Equal(WorkspaceLayoutReadState.Ready, read.State);
        Assert.Empty(read.Issues);
        var layout = Assert.IsType<WorkspaceLayout>(read.Layout);
        Assert.Equal(new[] { OtherRoute, Route }, read.OutsideRoutes.Select(route => route.Descriptor.DirectoryPath));
        Assert.Equal(layout.OutsideRoutes, read.OutsideRoutes.Select(route => route.Descriptor));
        Assert.All(read.OutsideRoutes, route => Assert.Equal(WorkspaceOutsideRouteReadState.Available, route.State));
        Assert.Equal(temporary.Combine(Route), read.OutsideRoutes[1].PhysicalDirectory);
        Assert.Equal(Encoding.UTF8.GetBytes(Content), read.OutsideRoutes[1].CatalogueSnapshot?.Bytes.ToArray());
        Assert.Equal(temporary.Combine(Anchor(Route)), read.OutsideRoutes[1].CatalogueResolution?.ResolvedPhysicalPath);
        Assert.Equal(FileExpectationKind.Missing, read.OutsideRoutes[0].CatalogueSnapshot?.Kind);
        Assert.Contains(read.Expectations, value => value.LogicalPath == temporary.Combine(Parent) && value.Kind == FileExpectationKind.Directory);
        Assert.DoesNotContain(read.Expectations, value => value.LogicalPath == temporary.Combine(Route + "/" + Nested));
        Assert.Equal(before, temporary.SnapshotHashes());
        Assert.Equal(directories, Directory.GetDirectories(temporary.Path, "*", SearchOption.AllDirectories));
        Assert.False(Directory.Exists(temporary.Combine(WorkspaceRuntimePaths.DirectoryName)));
        Assert.False(layout.IsImplicitContentDestination(Route));
    }

    [Theory(DisplayName = "Blocked primary evidence retains the settings-selected outside registrations")]
    [InlineData(false), InlineData(true)]
    public async Task BlockedPrimaryRetainsRegistrations(bool unsafeMain)
    {
        using var temporary = TemporaryWorkspace.Create(Operation);
        Settings(temporary, [Route]);
        temporary.CreateDirectory(Route);
        if (unsafeMain)
        {
            Directory.Move(temporary.Combine(MainRoot), temporary.Combine(RetiredMain));
        }
        else temporary.CreateFile(WorkspaceLayout.DefaultMainRoot + "/" + WorkspaceLayoutNames.LoaderFileName, Content);
        try
        {
            if (unsafeMain) File.WriteAllText(temporary.Combine(MainRoot), Content);
            var read = await Read(temporary);

            Assert.Equal(unsafeMain ? WorkspaceLayoutReadState.Invalid : WorkspaceLayoutReadState.Mismatch, read.State);
            var layout = Assert.IsType<WorkspaceLayout>(read.Layout);
            Assert.Equal(MainRoot, layout.MainRoot);
            Assert.Equal(Route, Assert.Single(layout.OutsideRoutes).DirectoryPath);
            Assert.Equal(WorkspaceOutsideRouteReadState.Available, Assert.Single(read.OutsideRoutes).State);
        }
        finally
        {
            if (unsafeMain)
            {
                File.Delete(temporary.Combine(MainRoot));
                Directory.Move(temporary.Combine(RetiredMain), temporary.Combine(MainRoot));
            }
        }
    }

    [Theory(DisplayName = "Missing outside directories retain their registrations and safe missing ancestry")]
    [InlineData(false), InlineData(true)]
    public async Task MissingDirectory(bool parentExists)
    {
        using var temporary = TemporaryWorkspace.Create(Operation);
        Settings(temporary, [Route]);
        if (parentExists) temporary.CreateDirectory(Parent);
        var before = temporary.SnapshotHashes();
        var read = await Read(temporary);
        var route = Assert.Single(read.OutsideRoutes);

        Assert.Equal(WorkspaceLayoutReadState.Ready, read.State);
        Assert.Equal(WorkspaceOutsideRouteReadState.Missing, route.State);
        Assert.Null(route.PhysicalDirectory);
        Assert.Null(route.CatalogueObservation);
        Assert.Equal(NoFollowLeafState.Missing, route.DirectoryObservation.State);
        Assert.Equal(temporary.Combine(parentExists ? Route : Parent), route.DirectoryObservation.LogicalPath);
        Assert.Equal(WorkspaceLayoutIssueCode.OutsideDirectoryMissing, Assert.Single(route.Issues).Code);
        Assert.Contains(read.Expectations, value => value.LogicalPath == route.DirectoryObservation.LogicalPath && value.Kind == FileExpectationKind.Missing);
        Assert.Equal(before, temporary.SnapshotHashes());
        Assert.False(Directory.Exists(temporary.Combine(Route)));
    }

    [Theory(DisplayName = "Wrong outside directory or catalogue object kinds stay unsafe without invalidating the primary layout")]
    [InlineData(0), InlineData(1), InlineData(2)]
    public async Task WrongObjectKinds(int boundary)
    {
        using var temporary = TemporaryWorkspace.Create(Operation);
        Settings(temporary, [Route]);
        if (boundary == 0) temporary.CreateFile(Parent, Content);
        else if (boundary == 1) temporary.CreateFile(Route, Content);
        else temporary.CreateDirectory(Anchor(Route));
        var before = temporary.SnapshotHashes();
        var read = await Read(temporary);
        var route = Assert.Single(read.OutsideRoutes);

        Assert.Equal(WorkspaceLayoutReadState.Ready, read.State);
        Assert.Empty(read.Issues);
        Assert.Equal(WorkspaceOutsideRouteReadState.Unsafe, route.State);
        Assert.Equal(boundary == 2 ? WorkspaceLayoutIssueCode.OutsideCatalogueUnsafe : WorkspaceLayoutIssueCode.OutsideDirectoryUnsafe,
            Assert.Single(route.Issues).Code);
        if (boundary == 2) Assert.Equal(NoFollowLeafState.Directory, route.CatalogueObservation?.State);
        else Assert.Equal(NoFollowLeafState.OrdinaryFile, route.DirectoryObservation.State);
        Assert.Equal(before, temporary.SnapshotHashes());
    }

    [Theory(DisplayName = "Static directory aliases expose symmetric resolved overlaps with every reserved location")]
    [InlineData(MainRoot), InlineData(WorkspaceLayout.DefaultMainRoot), InlineData(WorkspaceRuntimePaths.DirectoryName)]
    [InlineData(WorkspaceLayoutNames.GitMetadataDirectoryName)]
    [InlineData(WorkspaceLayout.DefaultMainRoot + "/" + Nested)]
    [InlineData(MainParent), InlineData(OtherRoute + "/" + WorkspaceLayoutNames.GitMetadataDirectoryName)]
    public async Task ReservedAliases(string target)
    {
        using var temporary = TemporaryWorkspace.Create(Operation);
        Settings(temporary, [Alias]);
        temporary.CreateDirectorySymbolicLink(Alias, temporary.CreateDirectory(target));
        var read = await Read(temporary);
        var route = Assert.Single(read.OutsideRoutes);

        Assert.Equal(WorkspaceLayoutReadState.Ready, read.State);
        Assert.Equal(WorkspaceOutsideRouteReadState.Unsafe, route.State);
        Assert.Equal(temporary.Combine(target), route.PhysicalDirectory);
        Assert.NotNull(route.DirectoryObservation.Link ?? (object?)route.DirectoryObservation.RelativeFileLink);
        Assert.Single(route.Issues.Where(issue => issue.Code == WorkspaceLayoutIssueCode.OutsideOverlap));
        Assert.Null(route.CatalogueObservation);
    }

    [Theory(DisplayName = "Registered directory aliases reject equal descendant and ancestor overlaps on both observations")]
    [InlineData(0), InlineData(1), InlineData(2)]
    public async Task RegisteredAliases(int relation)
    {
        using var temporary = TemporaryWorkspace.Create(Operation);
        Settings(temporary, [Route, Alias]);
        temporary.CreateDirectory(Route + "/" + Nested);
        var target = relation switch { 0 => Route, 1 => Route + "/" + Nested, _ => Parent };
        temporary.CreateDirectorySymbolicLink(Alias, temporary.Combine(target));
        var read = await Read(temporary);

        Assert.Equal(WorkspaceLayoutReadState.Ready, read.State);
        Assert.All(read.OutsideRoutes, route =>
        {
            Assert.Equal(WorkspaceOutsideRouteReadState.Unsafe, route.State);
            Assert.Contains(route.Issues, issue => issue.Code == WorkspaceLayoutIssueCode.OutsideOverlap);
            Assert.Null(route.CatalogueObservation);
        });
    }

    [Theory(DisplayName = "Alias ancestry and dangling or external mappings are unsafe rather than safe absence")]
    [InlineData(0), InlineData(1), InlineData(2)]
    public async Task UnsafeMappings(int mapping)
    {
        using var temporary = TemporaryWorkspace.Create(Operation);
        using var external = TemporaryWorkspace.Create(Operation);
        Settings(temporary, [Route]);
        var target = mapping switch
        {
            0 => temporary.CreateDirectory(OtherRoute + "/" + Nested),
            1 => temporary.Combine(OtherRoute),
            _ => external.Path,
        };
        temporary.CreateDirectorySymbolicLink(Parent, target);
        var read = await Read(temporary);
        var route = Assert.Single(read.OutsideRoutes);

        Assert.Equal(WorkspaceLayoutReadState.Ready, read.State);
        Assert.Equal(WorkspaceOutsideRouteReadState.Unsafe, route.State);
        Assert.Equal(temporary.Combine(Parent), route.DirectoryObservation.LogicalPath);
        Assert.Null(route.CatalogueObservation);
        Assert.DoesNotContain(read.Expectations, value => value.LogicalPath == temporary.Combine(Parent));
    }

    [Fact(DisplayName = "Outside catalogue links retain their no-follow identity and never authorize anchor reads")]
    public async Task CatalogueAlias()
    {
        using var temporary = TemporaryWorkspace.Create(Operation);
        Settings(temporary, [Route]);
        temporary.CreateDirectory(Route);
        var target = temporary.CreateFile(OtherRoute + "/source.md", Content);
        temporary.CreateFileSymbolicLink(Anchor(Route), target);
        var read = await Read(temporary);
        var route = Assert.Single(read.OutsideRoutes);

        Assert.Equal(WorkspaceLayoutReadState.Ready, read.State);
        Assert.Equal(WorkspaceOutsideRouteReadState.Unsafe, route.State);
        Assert.NotNull(route.CatalogueObservation?.Link);
        Assert.Null(route.CatalogueSnapshot);
        Assert.DoesNotContain(read.Expectations, value => value.LogicalPath == temporary.Combine(Anchor(Route)));
        Assert.Equal(Encoding.UTF8.GetBytes(Content), File.ReadAllBytes(target));
    }

    [Fact(DisplayName = "Inaccessible outside directories remain unavailable with typed failure facts")]
    public async Task InaccessibleDirectory()
    {
        if (OperatingSystem.IsWindows())
        {
            Assert.Skip("This evidence requires Unix directory permissions.");
            return;
        }
        using var temporary = TemporaryWorkspace.Create(Operation);
        Settings(temporary, [Route]);
        var directory = temporary.CreateDirectory(Route);
        var mode = File.GetUnixFileMode(directory);
        try
        {
            File.SetUnixFileMode(directory, UnixFileMode.None);
            var read = await Read(temporary);
            var route = Assert.Single(read.OutsideRoutes);
            Assert.Equal(WorkspaceLayoutReadState.Ready, read.State);
            Assert.Equal(WorkspaceOutsideRouteReadState.Unavailable, route.State);
            Assert.Equal(FilesystemFailureKind.AccessDenied, route.Failure?.Kind);
            Assert.Equal(temporary.Combine(Route), route.PhysicalDirectory);
            Assert.Null(route.CatalogueObservation);
        }
        finally { File.SetUnixFileMode(directory, mode); }
    }

    [Fact(DisplayName = "An unreadable ordinary outside catalogue is an outside failure only")]
    public async Task InaccessibleCatalogue()
    {
        if (!OperatingSystem.IsWindows())
        {
            Assert.Skip("This owned-file read denial requires Windows file sharing.");
            return;
        }
        using var temporary = TemporaryWorkspace.Create(Operation);
        Settings(temporary, [Route]);
        var anchor = temporary.CreateFile(Anchor(Route), Content);
        using var held = new FileStream(anchor, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
        var read = await Read(temporary);
        var route = Assert.Single(read.OutsideRoutes);

        Assert.Equal(WorkspaceLayoutReadState.Ready, read.State);
        Assert.Empty(read.Issues);
        Assert.Equal(WorkspaceOutsideRouteReadState.Unavailable, route.State);
        Assert.Equal(NoFollowLeafState.OrdinaryFile, route.CatalogueObservation?.State);
        Assert.NotNull(route.Failure);
        Assert.Null(route.CatalogueSnapshot);
    }

    [Theory(DisplayName = "Under-lease revalidation rejects stale outside settings anchor directory and ancestry observations")]
    [InlineData(0), InlineData(1), InlineData(2), InlineData(3), InlineData(4), InlineData(5), InlineData(6)]
    public async Task StaleObservation(int change)
    {
        using var temporary = TemporaryWorkspace.Create(Operation);
        using var store = WorkspaceLockTestStore.Create(Operation);
        Settings(temporary, [Route]);
        temporary.CreateDirectory(change == 6 ? Parent : Route);
        if (change is not (2 or 6)) temporary.CreateFile(Anchor(Route), Content);
        var read = await Read(temporary);
        var acquired = await store.AcquireAsync(new(Workspace(temporary), Operation, Guid.NewGuid()), TestContext.Current.CancellationToken);
        await using var lease = Assert.IsType<WorkspaceLockLease>(acquired.Lease);
        var validator = new WorkspaceLayoutRevalidator(new PhysicalPathResolver());
        Assert.Equal(MutationValidationState.Valid, (await validator.ValidateAsync(lease, read, TestContext.Current.CancellationToken)).State);

        if (change == 0) temporary.ReplaceText(WorkspaceSettingsDefinitions.RelativePath, SettingsJson([OtherRoute], MainRoot));
        else if (change == 1) temporary.ReplaceText(Anchor(Route), ChangedContent);
        else if (change == 2) temporary.CreateFile(Anchor(Route), ChangedContent);
        else if (change == 6) temporary.CreateDirectory(Route);
        else
        {
            var moved = change == 4 ? Parent : Route;
            Directory.Move(temporary.Combine(moved), temporary.Combine(OtherRoute));
        }
        try
        {
            var moved = change == 4 ? Parent : Route;
            if (change == 5) File.WriteAllText(temporary.Combine(moved), ChangedContent);
            else if (change is 3 or 4) Directory.CreateSymbolicLink(temporary.Combine(moved), temporary.Combine(OtherRoute));
            var before = temporary.SnapshotHashes();
            var validation = await validator.ValidateAsync(lease, read, TestContext.Current.CancellationToken);

            Assert.NotEqual(MutationValidationState.Valid, validation.State);
            Assert.Contains(validation.Checks, check => check.State != FileExpectationValidationState.Matched);
            Assert.Equal(before, temporary.SnapshotHashes());
            Assert.Equal(Route, Assert.Single(read.OutsideRoutes).Descriptor.DirectoryPath);
        }
        finally
        {
            if (change is 3 or 4 or 5)
            {
                var moved = change == 4 ? Parent : Route;
                if (change == 5 || !OperatingSystem.IsWindows()) File.Delete(temporary.Combine(moved));
                else Directory.Delete(temporary.Combine(moved));
                Directory.Move(temporary.Combine(OtherRoute), temporary.Combine(moved));
            }
        }
    }

    [Theory(DisplayName = "Stable missing and unsafe outside observations permit unrelated lease-protected maintenance")]
    [InlineData(0), InlineData(1), InlineData(2)]
    public async Task DiagnosticOutsideDoesNotBlockRevalidation(int boundary)
    {
        using var temporary = TemporaryWorkspace.Create(Operation);
        using var store = WorkspaceLockTestStore.Create(Operation);
        Settings(temporary, [Route]);
        if (boundary == 1) temporary.CreateFile(Route, Content);
        if (boundary == 2) temporary.CreateDirectory(Anchor(Route));
        var read = await Read(temporary);
        var acquired = await store.AcquireAsync(new(Workspace(temporary), Operation, Guid.NewGuid()), TestContext.Current.CancellationToken);
        await using var lease = Assert.IsType<WorkspaceLockLease>(acquired.Lease);

        var validation = await new WorkspaceLayoutRevalidator(new PhysicalPathResolver()).ValidateAsync(lease, read, TestContext.Current.CancellationToken);

        Assert.Equal(MutationValidationState.Valid, validation.State);
        Assert.Equal(boundary == 0 ? WorkspaceOutsideRouteReadState.Missing : WorkspaceOutsideRouteReadState.Unsafe, Assert.Single(read.OutsideRoutes).State);
    }

    [Fact(DisplayName = "Lease revalidation rejects an outside directory that became inaccessible after observation")]
    public async Task ChangedDirectoryAccess()
    {
        if (OperatingSystem.IsWindows())
        {
            Assert.Skip("This evidence requires Unix directory permissions.");
            return;
        }
        using var temporary = TemporaryWorkspace.Create(Operation);
        using var store = WorkspaceLockTestStore.Create(Operation);
        Settings(temporary, [Route]);
        var directory = temporary.CreateDirectory(Route);
        var read = await Read(temporary);
        var acquired = await store.AcquireAsync(new(Workspace(temporary), Operation, Guid.NewGuid()), TestContext.Current.CancellationToken);
        await using var lease = Assert.IsType<WorkspaceLockLease>(acquired.Lease);
        var mode = File.GetUnixFileMode(directory);
        try
        {
            File.SetUnixFileMode(directory, UnixFileMode.None);
            var validation = await new WorkspaceLayoutRevalidator(new PhysicalPathResolver()).ValidateAsync(lease, read, TestContext.Current.CancellationToken);
            Assert.NotEqual(MutationValidationState.Valid, validation.State);
        }
        finally { File.SetUnixFileMode(directory, mode); }
    }

    [Fact(DisplayName = "Outside observations retain the selected settings snapshot when settings change during their read")]
    public async Task SettingsChangeDuringObservation()
    {
        using var temporary = TemporaryWorkspace.Create(Operation);
        Settings(temporary, [Route]);
        temporary.CreateDirectory(Route);
        var changed = false;
        var paths = new PhysicalPathResolver(path =>
        {
            if (!changed && path == temporary.Combine(Route))
            {
                changed = true;
                temporary.ReplaceText(WorkspaceSettingsDefinitions.RelativePath, SettingsJson([OtherRoute], MainRoot));
            }
            return LinkTargetReader.Read(path);
        });
        var read = await new WorkspaceLayoutReader(paths).ReadAsync(Workspace(temporary), TestContext.Current.CancellationToken);

        Assert.True(changed);
        Assert.Equal(WorkspaceLayoutReadState.Unavailable, read.State);
        Assert.Equal(Route, Assert.Single(Assert.IsType<WorkspaceLayout>(read.Layout).OutsideRoutes).DirectoryPath);
        Assert.Equal(Route, Assert.Single(read.OutsideRoutes).Descriptor.DirectoryPath);
    }

    [Fact(DisplayName = "An outside anchor changed during observation makes only that outside observation unavailable")]
    public async Task AnchorChangeDuringObservation()
    {
        using var temporary = TemporaryWorkspace.Create(Operation);
        Settings(temporary, [Route]);
        temporary.CreateFile(Anchor(Route), Content);
        var visits = 0;
        var paths = new PhysicalPathResolver(path =>
        {
            if (path == temporary.Combine(Anchor(Route)) && ++visits == 2)
                temporary.ReplaceText(Anchor(Route), ChangedContent);
            return LinkTargetReader.Read(path);
        });
        var read = await new WorkspaceLayoutReader(paths).ReadAsync(Workspace(temporary), TestContext.Current.CancellationToken);

        Assert.True(visits >= 2);
        Assert.Equal(WorkspaceLayoutReadState.Ready, read.State);
        Assert.Equal(WorkspaceOutsideRouteReadState.Unavailable, Assert.Single(read.OutsideRoutes).State);
        Assert.Empty(read.Issues);
    }

    [Fact(DisplayName = "Outside revalidation preserves wrong lease released lease and cancelled validation behavior")]
    public async Task LeaseAndCancellation()
    {
        using var temporary = TemporaryWorkspace.Create(Operation);
        using var other = TemporaryWorkspace.Create(Operation);
        using var store = WorkspaceLockTestStore.Create(Operation);
        Settings(temporary, [Route]);
        temporary.CreateDirectory(Route);
        var read = await Read(temporary);
        var acquired = await store.AcquireAsync(new(Workspace(other), Operation, Guid.NewGuid()), TestContext.Current.CancellationToken);
        await using var wrong = Assert.IsType<WorkspaceLockLease>(acquired.Lease);
        var validator = new WorkspaceLayoutRevalidator(new PhysicalPathResolver());
        Assert.Equal(MutationValidationState.Blocked, (await validator.ValidateAsync(wrong, read, TestContext.Current.CancellationToken)).State);
        var correct = await store.AcquireAsync(new(Workspace(temporary), Operation, Guid.NewGuid()), TestContext.Current.CancellationToken);
        await using var lease = Assert.IsType<WorkspaceLockLease>(correct.Lease);
        using var cancellation = new CancellationTokenSource();
        await cancellation.CancelAsync();
        Assert.Equal(MutationValidationState.Cancelled, (await validator.ValidateAsync(lease, read, cancellation.Token)).State);
        await lease.DisposeAsync();
        Assert.Equal(MutationValidationState.Blocked, (await validator.ValidateAsync(lease, read, TestContext.Current.CancellationToken)).State);
    }

    [Fact(DisplayName = "Cancellation while observing outside ancestry propagates without creating filesystem state")]
    public async Task Cancellation()
    {
        using var temporary = TemporaryWorkspace.Create(Operation);
        Settings(temporary, [Route]);
        temporary.CreateDirectory(Route);
        var before = temporary.SnapshotHashes();
        using var cancellation = new CancellationTokenSource();
        var paths = new PhysicalPathResolver(path =>
        {
            if (path == temporary.Combine(Route)) cancellation.Cancel();
            return LinkTargetReader.Read(path);
        });
        await Assert.ThrowsAnyAsync<OperationCanceledException>(async () =>
            await new WorkspaceLayoutReader(paths).ReadAsync(Workspace(temporary), cancellation.Token));
        Assert.Equal(before, temporary.SnapshotHashes());
        Assert.False(Directory.Exists(temporary.Combine(WorkspaceRuntimePaths.DirectoryName)));
    }

    private static string Anchor(string route) => WorkspaceRoutePath.CanonicalEntrypointPath(route);

    private static void Settings(TemporaryWorkspace temporary, string[] routes, string root = MainRoot)
    {
        temporary.CreateFile(WorkspaceSettingsDefinitions.RelativePath, SettingsJson(routes, root));
        temporary.CreateFile(root + "/" + WorkspaceLayoutNames.LoaderFileName, Content);
    }

    private static string SettingsJson(string[] routes, string root)
    {
        var encodedRoutes = string.Join(',', routes.Select(route => JsonEncodedText.Encode(route).ToString()).Select(route => $"\"{route}\""));
        return $$"""{"{{WorkspaceSettingsDefinitions.MainRootProperty}}":"{{root}}","{{WorkspaceSettingsDefinitions.OutsideRoutesProperty}}":[{{encodedRoutes}}]}""";
    }

    private static CliWorkspace Workspace(TemporaryWorkspace temporary)
        => new(temporary.Path, temporary.Path, CliWorkspaceSelectionMethod.ExplicitWorkspace);

    private static ValueTask<WorkspaceLayoutRead> Read(TemporaryWorkspace temporary)
        => new WorkspaceLayoutReader(new PhysicalPathResolver()).ReadAsync(Workspace(temporary), TestContext.Current.CancellationToken);
}
