using System.Text;
using OpenForge.Cli.Core.Framework.Documents.Markdown.Shared.ManagedHosts;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Mutation.Locking.Models;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Files;
using OpenForge.Cli.Core.Framework.Mutation.Validation.Models;
using OpenForge.Cli.Core.Framework.Ownership;
using OpenForge.Cli.Core.Framework.Ownership.Models.Document;
using OpenForge.Cli.Core.Framework.Ownership.Shared.Serialization;
using OpenForge.Cli.Core.Framework.Settings;
using OpenForge.Cli.Core.Framework.Settings.Models.Observation;
using OpenForge.Cli.Core.Framework.Settings.Shared.Mutation;
using OpenForge.Cli.Core.Framework.Settings.Shared.Observation;
using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.Core.Framework.Workspace.Models.Layout;
using OpenForge.Cli.Core.Framework.Workspace.Shared.Runtime;
using OpenForge.Cli.IntegrationTests.TestSupport;
using OpenForge.Cli.TestSupport;

namespace OpenForge.Cli.IntegrationTests.Framework.Settings.Shared.Observation;

[Trait("Feature", "workspace-layout"), Trait("Evidence", "Integration"), Trait("Boundary", "OS")]
public sealed class WorkspaceLayoutReaderIntegrationTests
{
    private const string ParentRoot = "docs";
    private const string CustomRoot = ParentRoot + "/forge";
    private const string CustomLoader = CustomRoot + "/" + WorkspaceLayoutNames.LoaderFileName;
    private const string DefaultLoader = WorkspaceLayout.DefaultMainRoot + "/" + WorkspaceLayoutNames.LoaderFileName;
    private const string OldLoader = "old/" + WorkspaceLayoutNames.LoaderFileName;
    private const string SpacedLoader = "old root/" + WorkspaceLayoutNames.LoaderFileName;
    private const string LoaderContent = "# Loader\n";
    private const string Operation = "layout-test";
    private const string Package = "fixture-framework";
    private const string Version = "1";
    private const string InvalidOwnership = "{broken";
    private const string ChangedSettings = "{}";

    [Theory(DisplayName = "Recorded loader removal remains ready without concealing an unexpected missing loader")]
    [InlineData(false), InlineData(true)]
    public async Task MissingSelectedLoader(bool removed)
    {
        using var temporary = TemporaryWorkspace.Create(Operation);
        var exclusions = removed ? $"\"{CustomLoader}\"" : string.Empty;
        temporary.CreateFile(WorkspaceSettingsDefinitions.RelativePath, $$"""
            {"{{WorkspaceSettingsDefinitions.SchemaVersionProperty}}":{{WorkspaceSettingsDefinitions.SchemaVersion}},"{{WorkspaceSettingsDefinitions.MainRootProperty}}":"{{CustomRoot}}","{{WorkspaceSettingsDefinitions.RemovedFilesProperty}}":[{{exclusions}}]}
            """);
        temporary.CreateDirectory(CustomRoot);
        temporary.CreateFile(WorkspaceLayoutNames.AgentHostFileName, ManagedHost($"Read `{CustomLoader}`."));
        var before = Directory.GetFiles(temporary.Path, "*", SearchOption.AllDirectories).ToDictionary(path => path, File.ReadAllBytes);

        var read = await Read(temporary);

        Assert.Equal(WorkspaceLayoutReadState.Ready, read.State);
        Assert.Equal(CustomRoot, read.Layout?.MainRoot);
        Assert.Equal(!removed, read.Issues.Any(issue => issue.Code == WorkspaceLayoutIssueCode.MissingLoader));
        Assert.Contains(read.Expectations, value => value.LogicalPath == temporary.Combine(CustomLoader) && value.Kind == FileExpectationKind.Missing);
        Assert.Equal(before.Keys.Order(), Directory.GetFiles(temporary.Path, "*", SearchOption.AllDirectories).Order());
        foreach (var (path, bytes) in before) Assert.Equal(bytes, File.ReadAllBytes(path));
    }

    [Theory(DisplayName = "Missing and empty destinations are uninstalled observations without effects")]
    [InlineData(WorkspaceLayout.DefaultMainRoot, false), InlineData(WorkspaceLayout.DefaultMainRoot, true)]
    [InlineData(CustomRoot, false), InlineData(CustomRoot, true)]
    public async Task UninstalledDestinations(string root, bool exists)
    {
        using var temporary = TemporaryWorkspace.Create("layout-uninstalled");
        if (root != WorkspaceLayout.DefaultMainRoot) Settings(temporary, root);
        if (exists) temporary.CreateDirectory(root);
        var before = temporary.SnapshotHashes();
        var directories = Directory.GetDirectories(temporary.Path, "*", SearchOption.AllDirectories);
        var read = await Read(temporary);
        Assert.Equal(WorkspaceLayoutReadState.Uninstalled, read.State);
        Assert.Equal(root, read.Layout?.MainRoot);
        Assert.Empty(read.Issues);
        Assert.Equal(before, temporary.SnapshotHashes());
        Assert.Equal(directories, Directory.GetDirectories(temporary.Path, "*", SearchOption.AllDirectories));
        Assert.False(Directory.Exists(temporary.Combine(WorkspaceRuntimePaths.DirectoryName)));
    }

    [Theory(DisplayName = "Selected loaders are ready with optional hosts and native Skills absent")]
    [InlineData(WorkspaceLayout.DefaultMainRoot), InlineData(CustomRoot)]
    public async Task ReadyWithoutOptionalHosts(string root)
    {
        using var temporary = TemporaryWorkspace.Create("layout-ready");
        if (root != WorkspaceLayout.DefaultMainRoot) Settings(temporary, root);
        temporary.CreateFile(root + "/" + WorkspaceLayoutNames.LoaderFileName, LoaderContent);
        var before = temporary.SnapshotHashes();
        var read = await Read(temporary);
        Assert.Equal(WorkspaceLayoutReadState.Ready, read.State);
        Assert.Empty(read.Issues);
        Assert.Contains(read.Expectations, value => value.LogicalPath == temporary.Combine(root + "/" + WorkspaceLayoutNames.LoaderFileName) && value.Kind == FileExpectationKind.File);
        Assert.Equal(before, temporary.SnapshotHashes());
    }

    [Theory(DisplayName = "Invalid settings never select the existing default loader")]
    [InlineData("{broken"), InlineData("{\"" + WorkspaceSettingsDefinitions.MainRootProperty + "\":null}"), InlineData("{\"" + WorkspaceSettingsDefinitions.MainRootProperty + "\":\"" + WorkspaceLayout.DefaultMainRoot + "/nested\"}")]
    public async Task InvalidSettingsHaveNoFallback(string json)
    {
        using var temporary = TemporaryWorkspace.Create("layout-invalid");
        temporary.CreateFile(WorkspaceSettingsDefinitions.RelativePath, json);
        temporary.CreateFile(DefaultLoader, LoaderContent);
        var read = await Read(temporary);
        Assert.Equal(WorkspaceLayoutReadState.Invalid, read.State);
        Assert.Null(read.Layout);
        Assert.Empty(read.OutsideRoutes);
        Assert.Contains(read.Issues, issue => issue.Code == WorkspaceLayoutIssueCode.InvalidSetting);
    }

    [Fact(DisplayName = "A settings edit does not relocate an existing default installation")]
    public async Task ConflictingDefaultLoader()
    {
        using var temporary = TemporaryWorkspace.Create("layout-conflict");
        Settings(temporary, CustomRoot);
        temporary.CreateFile(DefaultLoader, LoaderContent);
        var read = await Read(temporary);
        Assert.Equal(WorkspaceLayoutReadState.Mismatch, read.State);
        Assert.Equal(CustomRoot, read.Layout?.MainRoot);
        Assert.Contains(read.Issues, issue => issue.Code == WorkspaceLayoutIssueCode.ConflictingLoader);
        Assert.Contains(read.Issues, issue => issue.Code == WorkspaceLayoutIssueCode.MissingLoader);
    }

    [Theory(DisplayName = "Managed host pointers agree with the selected layout")]
    [InlineData(WorkspaceLayoutNames.AgentHostFileName, "Read `" + OldLoader + "`.", false)]
    [InlineData(WorkspaceLayoutNames.ClaudeHostFileName, "@" + WorkspaceLayoutNames.AgentHostFileName + "\n@" + OldLoader, false)]
    [InlineData(WorkspaceLayoutNames.ClaudeHostFileName, "@" + WorkspaceLayoutNames.AgentHostFileName + "\n@" + SpacedLoader, false)]
    [InlineData(WorkspaceLayoutNames.AgentHostFileName, "Read [the loader](" + OldLoader + ").", false)]
    [InlineData(WorkspaceLayoutNames.AgentHostFileName, "Read `" + CustomLoader + "`.", true)]
    [InlineData(WorkspaceLayoutNames.ClaudeHostFileName, "@" + WorkspaceLayoutNames.AgentHostFileName + "\n@" + CustomLoader, true)]
    public async Task ManagedPointers(string host, string pointer, bool matches)
    {
        using var temporary = TemporaryWorkspace.Create("layout-pointers");
        Settings(temporary, CustomRoot);
        temporary.CreateFile(CustomLoader, LoaderContent);
        temporary.CreateFile(host, ManagedHost(pointer));
        var read = await Read(temporary);
        Assert.Equal(matches ? WorkspaceLayoutReadState.Ready : WorkspaceLayoutReadState.Mismatch, read.State);
        Assert.Equal(!matches, read.Issues.Any(issue => issue.Code == WorkspaceLayoutIssueCode.PointerMismatch));
    }

    [Fact(DisplayName = "Unmanaged mentions and unknown directories are not installation evidence")]
    public async Task IgnoresUnmanagedPointers()
    {
        using var temporary = TemporaryWorkspace.Create("layout-unmanaged");
        Settings(temporary, CustomRoot);
        temporary.CreateFile(WorkspaceLayoutNames.AgentHostFileName, "Read `" + OldLoader + "`.\n");
        temporary.CreateFile(OldLoader, "# Old\n");
        var read = await Read(temporary);
        Assert.Equal(WorkspaceLayoutReadState.Uninstalled, read.State);
        Assert.Empty(read.Issues);
    }

    [Fact(DisplayName = "Recorded Framework loader ownership exposes a missing selected installation")]
    public async Task OwnershipMismatch()
    {
        using var temporary = TemporaryWorkspace.Create("layout-ownership");
        Settings(temporary, CustomRoot);
        var document = WorkspaceOwnershipDocument.Empty with
        {
            Framework = new FrameworkOwnership(new OwnedSource(Package, Version), [OldLoader], []),
        };
        temporary.CreateFile(WorkspaceOwnershipDefinitions.RelativePath, Encoding.UTF8.GetString(WorkspaceOwnershipCodec.Write(document)));
        var read = await Read(temporary);
        Assert.Equal(WorkspaceLayoutReadState.Mismatch, read.State);
        Assert.Contains(read.Issues, issue => issue.Code == WorkspaceLayoutIssueCode.OwnershipMismatch && issue.Path == OldLoader);
        Assert.Contains(read.Issues, issue => issue.Code == WorkspaceLayoutIssueCode.MissingLoader);
    }

    [Theory(DisplayName = "Files and competing Skills directories cannot supply content ancestry")]
    [InlineData(ParentRoot), InlineData(CustomRoot), InlineData(CustomRoot + "/" + WorkspaceLayoutNames.SkillsDirectoryName)]
    public async Task InvalidContentDirectories(string path)
    {
        using var temporary = TemporaryWorkspace.Create("layout-ancestry");
        Settings(temporary, CustomRoot);
        if (path.EndsWith(WorkspaceLayoutNames.SkillsDirectoryName, StringComparison.Ordinal)) temporary.CreateDirectory(path);
        else temporary.CreateFile(path, "not a directory");
        var read = await Read(temporary);
        Assert.Equal(WorkspaceLayoutReadState.Invalid, read.State);
        Assert.Equal(CustomRoot, read.Layout?.MainRoot);
        Assert.Contains(read.Issues, issue => issue.Code == WorkspaceLayoutIssueCode.UnsafeRoot);
    }

    [Theory(DisplayName = "Directory aliases in selected ancestry and native Skills are refused")]
    [InlineData(ParentRoot), InlineData(CustomRoot), InlineData(WorkspaceLayout.NativeSkillsPath)]
    public async Task RejectsDirectoryAliases(string path)
    {
        using var temporary = TemporaryWorkspace.Create("layout-alias");
        Settings(temporary, CustomRoot);
        var target = temporary.CreateDirectory("actual");
        temporary.CreateDirectorySymbolicLink(path, target);
        var read = await Read(temporary);
        Assert.Equal(WorkspaceLayoutReadState.Invalid, read.State);
        Assert.Equal(CustomRoot, read.Layout?.MainRoot);
        Assert.Contains(read.Issues, issue => issue.Code == WorkspaceLayoutIssueCode.UnsafeRoot);
    }

    [Theory(DisplayName = "Under-lease validation rejects changes to every observed layout fact")]
    [InlineData(WorkspaceSettingsDefinitions.RelativePath), InlineData(CustomLoader)]
    [InlineData(DefaultLoader), InlineData(WorkspaceLayoutNames.AgentHostFileName), InlineData(WorkspaceOwnershipDefinitions.RelativePath)]
    public async Task RejectsStaleObservation(string changedPath)
    {
        using var temporary = TemporaryWorkspace.Create("layout-stale");
        using var store = WorkspaceLockTestStore.Create("layout-lock");
        Settings(temporary, CustomRoot);
        temporary.CreateFile(CustomLoader, LoaderContent);
        var workspace = Workspace(temporary);
        var read = await Read(temporary);
        var acquired = await store.AcquireAsync(new(workspace, Operation, Guid.NewGuid()), TestContext.Current.CancellationToken);
        await using var lease = Assert.IsType<WorkspaceLockLease>(acquired.Lease);
        var validator = new WorkspaceLayoutRevalidator(new PhysicalPathResolver());
        Assert.Equal(MutationValidationState.Valid, (await validator.ValidateAsync(lease, read, TestContext.Current.CancellationToken)).State);
        if (File.Exists(temporary.Combine(changedPath)))
            temporary.ReplaceText(changedPath, "changed after observation");
        else temporary.CreateFile(changedPath, "changed after observation");
        var before = temporary.SnapshotHashes();
        var validation = await validator.ValidateAsync(lease, read, TestContext.Current.CancellationToken);
        Assert.Equal(MutationValidationState.Mismatched, validation.State);
        Assert.Equal(before, temporary.SnapshotHashes());
        Assert.Equal(CustomRoot, read.Layout?.MainRoot);
    }

    [Fact(DisplayName = "Revalidation requires the same held workspace lease")]
    public async Task RejectsWrongOrReleasedLease()
    {
        using var temporary = TemporaryWorkspace.Create("layout-lease");
        using var other = TemporaryWorkspace.Create("layout-other");
        using var store = WorkspaceLockTestStore.Create("layout-lease-store");
        var read = await Read(temporary);
        var acquired = await store.AcquireAsync(new(Workspace(other), Operation, Guid.NewGuid()), TestContext.Current.CancellationToken);
        await using var lease = Assert.IsType<WorkspaceLockLease>(acquired.Lease);
        var validator = new WorkspaceLayoutRevalidator(new PhysicalPathResolver());
        Assert.Equal(MutationValidationState.Blocked, (await validator.ValidateAsync(lease, read, TestContext.Current.CancellationToken)).State);
        var otherRead = await Read(other);
        await lease.DisposeAsync();
        Assert.Equal(MutationValidationState.Blocked, (await validator.ValidateAsync(lease, otherRead, TestContext.Current.CancellationToken)).State);
    }

    [Fact(DisplayName = "Ancestry captured before planning cannot change under the lease")]
    public async Task RejectsChangedAncestry()
    {
        using var temporary = TemporaryWorkspace.Create("layout-parent-change");
        using var store = WorkspaceLockTestStore.Create("layout-parent-lock");
        Settings(temporary, CustomRoot);
        var read = await Read(temporary);
        var acquired = await store.AcquireAsync(new(Workspace(temporary), Operation, Guid.NewGuid()), TestContext.Current.CancellationToken);
        await using var lease = Assert.IsType<WorkspaceLockLease>(acquired.Lease);
        temporary.CreateFile(ParentRoot, "changed ancestry");
        var validation = await new WorkspaceLayoutRevalidator(new PhysicalPathResolver()).ValidateAsync(lease, read, TestContext.Current.CancellationToken);
        Assert.Equal(MutationValidationState.Mismatched, validation.State);
    }

    [Fact(DisplayName = "Unreadable native Skills remain unavailable rather than optional absence")]
    public async Task InaccessibleSkills()
    {
        if (OperatingSystem.IsWindows())
        {
            Assert.Skip("This evidence requires Unix directory permissions.");
            return;
        }
        using var temporary = TemporaryWorkspace.Create("layout-unreadable-skills");
        Settings(temporary, CustomRoot);
        var directory = temporary.CreateDirectory(WorkspaceLayout.NativeSkillsPath);
        var mode = File.GetUnixFileMode(directory);
        try
        {
            File.SetUnixFileMode(directory, UnixFileMode.None);
            var read = await Read(temporary);
            Assert.Equal(WorkspaceLayoutReadState.Unavailable, read.State);
            Assert.Equal(CustomRoot, read.Layout?.MainRoot);
            Assert.Contains(read.Issues, issue => issue.Code == WorkspaceLayoutIssueCode.ObservationUnavailable && issue.Path == WorkspaceLayout.NativeSkillsPath);
        }
        finally { File.SetUnixFileMode(directory, mode); }
    }

    [Fact(DisplayName = "Cancellation does not become an uninstalled layout")]
    public async Task CancellationPropagates()
    {
        using var temporary = TemporaryWorkspace.Create("layout-cancelled");
        using var cancellation = new CancellationTokenSource();
        await cancellation.CancelAsync();
        await Assert.ThrowsAnyAsync<OperationCanceledException>(async () =>
            await new WorkspaceLayoutReader(new PhysicalPathResolver()).ReadAsync(Workspace(temporary), cancellation.Token));
    }

    [Fact(DisplayName = "A recognized host is positive installation evidence when its selected loader is missing")]
    public async Task PointerWithoutLoader()
    {
        using var temporary = TemporaryWorkspace.Create("layout-missing-loader");
        Settings(temporary, CustomRoot);
        temporary.CreateFile(WorkspaceLayoutNames.ClaudeHostFileName, ManagedHost("@" + WorkspaceLayoutNames.AgentHostFileName + "\n@" + CustomLoader));
        var read = await Read(temporary);
        Assert.Equal(WorkspaceLayoutReadState.Ready, read.State);
        Assert.Equal(WorkspaceLayoutIssueCode.MissingLoader, Assert.Single(read.Issues).Code);
    }

    [Fact(DisplayName = "Unavailable settings never yield a default layout")]
    public async Task UnavailableSettings()
    {
        using var temporary = TemporaryWorkspace.Create("layout-unavailable");
        temporary.CreateDirectory(WorkspaceSettingsDefinitions.RelativePath);
        var read = await Read(temporary);
        Assert.Equal(WorkspaceLayoutReadState.Unavailable, read.State);
        Assert.Null(read.Layout);
        Assert.Empty(read.OutsideRoutes);
        Assert.Contains(read.Issues, issue => issue.Code == WorkspaceLayoutIssueCode.ObservationUnavailable);
    }

    [Theory(DisplayName = "Invalid and unavailable ownership retain the selected diagnostic layout")]
    [InlineData(false), InlineData(true)]
    public async Task UnavailableOwnershipRetainsLayout(bool unavailable)
    {
        using var temporary = TemporaryWorkspace.Create(Operation);
        Settings(temporary, CustomRoot);
        temporary.CreateFile(CustomLoader, LoaderContent);
        if (unavailable) temporary.CreateDirectory(WorkspaceOwnershipDefinitions.RelativePath);
        else temporary.CreateFile(WorkspaceOwnershipDefinitions.RelativePath, InvalidOwnership);
        var before = Directory.GetFiles(temporary.Path, "*", SearchOption.AllDirectories)
            .ToDictionary(path => path, File.ReadAllBytes);

        var read = await Read(temporary);

        Assert.Equal(WorkspaceLayoutReadState.Ready, read.State);
        Assert.Equal(CustomRoot, read.Layout?.MainRoot);
        Assert.Contains(read.Issues, issue => issue.Code == WorkspaceLayoutIssueCode.ObservationUnavailable
            && issue.Path == WorkspaceOwnershipDefinitions.RelativePath);
        Assert.Equal(before.Keys.Order(), Directory.GetFiles(temporary.Path, "*", SearchOption.AllDirectories).Order());
        foreach (var (path, bytes) in before) Assert.Equal(bytes, File.ReadAllBytes(path));
    }

    [Fact(DisplayName = "A layout changed during observation retains the root selected from its settings snapshot")]
    public async Task ChangedObservationRetainsLayout()
    {
        using var temporary = TemporaryWorkspace.Create(Operation);
        Settings(temporary, CustomRoot);
        temporary.CreateFile(CustomLoader, LoaderContent);
        var changed = false;
        var paths = new PhysicalPathResolver(path =>
        {
            if (!changed && PhysicalIdentityTracker.PathComparer.Equals(path, temporary.Combine(CustomLoader)))
            {
                changed = true;
                temporary.ReplaceText(WorkspaceSettingsDefinitions.RelativePath, ChangedSettings);
            }
            return LinkTargetReader.Read(path);
        });

        var read = await new WorkspaceLayoutReader(paths).ReadAsync(Workspace(temporary), TestContext.Current.CancellationToken);

        Assert.True(changed);
        Assert.Equal(WorkspaceLayoutReadState.Unavailable, read.State);
        Assert.Equal(CustomRoot, read.Layout?.MainRoot);
        Assert.Contains(read.Issues, issue => issue.Code == WorkspaceLayoutIssueCode.ObservationUnavailable
            && issue.Path == WorkspaceSettingsDefinitions.RelativePath);
    }

    private static string ManagedHost(string pointer)
        => $"# {MarkdownManagedHostReader.HeadingText}\n\n{pointer}\n\n**{MarkdownManagedHostReader.FooterText}**\n";

    private static void Settings(TemporaryWorkspace temporary, string root)
        => temporary.CreateFile(WorkspaceSettingsDefinitions.RelativePath, $$$"""{"{{{WorkspaceSettingsDefinitions.MainRootProperty}}}":"{{{root}}}"}""");

    private static CliWorkspace Workspace(TemporaryWorkspace temporary)
        => new(temporary.Path, temporary.Path, CliWorkspaceSelectionMethod.ExplicitWorkspace);

    private static ValueTask<WorkspaceLayoutRead> Read(TemporaryWorkspace temporary)
        => new WorkspaceLayoutReader(new PhysicalPathResolver()).ReadAsync(Workspace(temporary), TestContext.Current.CancellationToken);
}
