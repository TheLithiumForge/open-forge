using OpenForge.Cli.Core.Commands.Extension;
using OpenForge.Cli.Core.Commands.Extension.Install;
using OpenForge.Cli.Core.Commands.Library;
using OpenForge.Cli.Core.Commands.Library.Attach;
using OpenForge.Cli.Core.Commands.Remove;
using OpenForge.Cli.Core.Commands.Shared;
using OpenForge.Cli.Core.Commands.Shared.RouteScaffolding;
using OpenForge.Cli.Core.Commands.Stripping;
using OpenForge.Cli.Core.Commands.Stripping.Models.Planning;
using OpenForge.Cli.Core.Commands.Stripping.Models.Request;
using OpenForge.Cli.Core.Commands.Stripping.Models.Result;
using OpenForge.Cli.Core.Commands.Stripping.Shared.Application;
using OpenForge.Cli.Core.Framework.Workspace.Models.Layout;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.IntegrationTests.Commands.Extension.Install;
using OpenForge.Cli.IntegrationTests.Commands.Stripping;

namespace OpenForge.Cli.IntegrationTests.Commands.Uninstall;

// Covers the uninstall Behavior contract's Required Evidence beyond the ordinary removal boundary.
public sealed class UninstallBoundaryIntegrationTests
{
    private const string Docs = "docs";
    private const string Heading = "# Team\n\nOur rules.\n";
    private const string LibraryId = "shared";
    private const string LibrarySource = "vendor/" + LibraryId;
    private const string SharedTarget = WorkspaceLayout.DefaultMainRoot + "/shared.txt";
    private const string SharedContents = "shared package bytes\n";
    private const string Alpha = "alpha";
    private const string Beta = "beta";

    [Fact(DisplayName = "Uninstall removes the section from a host with user text and deletes a host with nothing else"), Trait("Feature", "uninstall"), Trait("Evidence", "Integration")]
    public async Task RootHostsWithAndWithoutUserText()
    {
        using var workspace = StripIntegrationWorkspace.Create("uninstall-root-hosts");
        await workspace.InstallAsync();
        workspace.Write(WorkspaceLayoutNames.ClaudeHostFileName, $"{Heading}\n{workspace.Read(WorkspaceLayoutNames.ClaudeHostFileName)}");

        var result = await workspace.UninstallAsync(workspace.Request(StripCommandKind.Uninstall, []));

        Assert.Equal(CliSemanticStatus.Complete, result.Status);
        var claude = Assert.Single(result.Plan, entry => entry.Path == WorkspaceLayoutNames.ClaudeHostFileName);
        var agents = Assert.Single(result.Plan, entry => entry.Path == WorkspaceLayoutNames.AgentHostFileName);
        Assert.Equal((StripPlanAction.Rewrite, StripPlanReason.RootHost), (claude.Action, claude.Reason));
        Assert.Equal([StripRewritePart.ManagedSection], claude.Parts);
        Assert.Equal((StripPlanAction.Delete, StripPlanReason.RootHost), (agents.Action, agents.Reason));
        Assert.Equal(Heading, workspace.Read(WorkspaceLayoutNames.ClaudeHostFileName));
        Assert.False(workspace.Exists(WorkspaceLayoutNames.AgentHostFileName));
    }

    [Fact(DisplayName = "Uninstall plans no effect for an owned target that is already missing"), Trait("Feature", "uninstall"), Trait("Evidence", "Integration")]
    public async Task MissingTargetNeedsNoEffect()
    {
        using var workspace = StripIntegrationWorkspace.Create("uninstall-missing-target");
        await workspace.InstallAsync();
        var request = workspace.Request(StripCommandKind.Uninstall, []);
        var missing = (await workspace.UninstallAsync(request with { DryRun = true })).Plan
            .First(entry => entry.Reason == StripPlanReason.Framework).Path;
        File.Delete(workspace.Combine(missing));

        var result = await workspace.UninstallAsync(request);

        Assert.Equal(CliSemanticStatus.Complete, result.Status);
        Assert.DoesNotContain(result.Plan, entry => entry.Path == missing);
        Assert.DoesNotContain(result.Findings, finding => finding.Path == missing);
        Assert.False(workspace.Exists(WorkspaceLayout.DefaultMainRoot));
    }

    [Fact(DisplayName = "Uninstall deletes a file two Extensions share once"), Trait("Feature", "uninstall"), Trait("Evidence", "Integration")]
    public async Task SharedPackageFileGoesOnce()
    {
        using var workspace = StripIntegrationWorkspace.Create("uninstall-shared-package-file");
        await workspace.InstallAsync();
        using var source = ExtensionInstallCatalogue.Create("uninstall-shared-package-file-source");
        source.AddPackage(Alpha, [], (SharedTarget, SharedContents));
        source.AddPackage(Beta, [], (SharedTarget, SharedContents));
        var installed = await workspace.RunAsync([
            ExtensionDefinitions.ExtensionGroup.Name, ExtensionInstallDefinitions.InstallCommand.Name, ExtensionInstallDefinitions.All.Name,
            ExtensionInstallDefinitions.Source.Name, source.Path, ExtensionInstallDefinitions.Automatic.Name]);
        Assert.Equal(CliSemanticStatus.Complete, installed.Status);

        var result = await workspace.UninstallAsync(workspace.Request(StripCommandKind.Uninstall, []));

        Assert.Equal(CliSemanticStatus.Complete, result.Status);
        var shared = Assert.Single(result.Plan, entry => entry.Path == SharedTarget);
        Assert.Equal((StripPlanAction.Delete, StripPlanReason.Extension), (shared.Action, shared.Reason));
        Assert.False(workspace.Exists(SharedTarget));
    }

    [Fact(DisplayName = "Uninstall flattens a folder whose managed files are all being removed"), Trait("Feature", "uninstall"), Trait("Evidence", "Integration")]
    public async Task FlattenAfterManagedFilesAreRemoved()
    {
        using var workspace = StripIntegrationWorkspace.Create("uninstall-flatten-managed-removed");
        await workspace.InstallAsync();
        var note = WorkspaceLayout.DefaultMainRoot + "/team/rules/note.md";
        workspace.Write(note, "# Note\n");

        var result = await workspace.UninstallAsync(workspace.Request(StripCommandKind.Uninstall, [], WorkspaceLayout.DefaultMainRoot));

        Assert.Equal(CliSemanticStatus.Complete, result.Status);
        var moved = Assert.Single(result.Plan, entry => entry.Action == StripPlanAction.Move);
        Assert.Equal((note, WorkspaceLayout.DefaultMainRoot + "/note.md"), (moved.Path, moved.To));
        Assert.True(workspace.Exists(moved.To!));
        Assert.DoesNotContain(result.Findings, finding => finding.Code == StripFindingCode.FlattenManaged);
    }

    [Fact(DisplayName = "Uninstall refuses to flatten a folder that still holds an edited managed file it keeps"), Trait("Feature", "uninstall"), Trait("Evidence", "Integration")]
    public async Task FlattenRefusesKeptManagedFile()
    {
        using var workspace = StripIntegrationWorkspace.Create("uninstall-flatten-managed-kept");
        await workspace.InstallAsync();
        var loader = WorkspaceLayout.DefaultMainRoot + "/" + WorkspaceLayoutNames.LoaderFileName;
        workspace.Write(loader, workspace.Read(loader) + "\nOur own rule.\n");
        var before = workspace.SnapshotHashes();

        var result = await workspace.UninstallAsync(workspace.Request(StripCommandKind.Uninstall, [], WorkspaceLayout.DefaultMainRoot));

        Assert.Equal(CliSemanticStatus.Blocked, result.Status);
        var finding = Assert.Single(result.Findings);
        Assert.Equal((StripFindingCode.FlattenManaged, loader), (finding.Code, finding.Path));
        Assert.Equal(before, workspace.SnapshotHashes());
    }

    [Fact(DisplayName = "Uninstall deletes the route scaffold it wrote and keeps an entrypoint the user wrote in"), Trait("Feature", "uninstall"), Trait("Evidence", "Integration")]
    public async Task RouteScaffolds()
    {
        using var workspace = StripIntegrationWorkspace.Create("uninstall-route-scaffolds");
        await workspace.InstallAsync();
        var authored = $"---\nopen-forge:\n  description: Team notes\n  tags: [Team]\n---\n# Team\n\nTeam notes.\n\nAsk the team lead before changing these notes.\n\n{RouteScaffoldComposer.AxiomsHeading}\n\n{RouteScaffoldComposer.InheritedAxiomsMarker}\n";
        workspace.Write(Docs + "/team/_team.md", authored);
        workspace.Write(Docs + "/team/notes.md", "# Notes\n");
        await workspace.IncludeAsync(Docs);
        var catalogue = Docs + "/_" + Docs + ".md";
        Assert.True(workspace.Exists(catalogue));

        var result = await workspace.UninstallAsync(workspace.Request(StripCommandKind.Uninstall, []));

        Assert.Equal(CliSemanticStatus.Complete, result.Status);
        Assert.Contains(result.Plan, entry => entry.Path == catalogue && entry.Reason == StripPlanReason.Scaffold);
        Assert.False(workspace.Exists(catalogue));
        Assert.Equal(authored, workspace.Read(Docs + "/team/_team.md"));
    }

    [Fact(DisplayName = "A second uninstall writes nothing, even over an entrypoint with the route scaffold's shape"), Trait("Feature", "uninstall"), Trait("Evidence", "Integration")]
    public async Task SecondRunWritesNothing()
    {
        using var workspace = StripIntegrationWorkspace.Create("uninstall-second-run");
        await workspace.InstallAsync();
        workspace.Write(Docs + "/team/notes.md", "# Notes\n");
        await workspace.IncludeAsync(Docs);
        var request = workspace.Request(StripCommandKind.Uninstall, []);
        Assert.Equal(CliSemanticStatus.Complete, (await workspace.UninstallAsync(request)).Status);
        var catalogue = Docs + "/_" + Docs + ".md";
        Assert.False(workspace.Exists(catalogue));
        workspace.Write(catalogue, $"# Our docs\n\nWhat we wrote.\n\n{RouteScaffoldComposer.AxiomsHeading}\n\n{RouteScaffoldComposer.InheritedAxiomsMarker}\n");
        var before = workspace.SnapshotHashes();

        var result = await workspace.UninstallAsync(request);

        Assert.Equal(CliSemanticStatus.Complete, result.Status);
        Assert.True(result.NoOp);
        Assert.Equal(before, workspace.SnapshotHashes());
        Assert.False(workspace.Exists(StripRuntimeFolder.Path));
    }

    [Fact(DisplayName = "Uninstall without ownership records changes nothing, even a file with the scaffold's shape"), Trait("Feature", "uninstall"), Trait("Evidence", "Integration")]
    public async Task AbsentOwnershipChangesNothing()
    {
        using var workspace = StripIntegrationWorkspace.Create("uninstall-absent-ownership");
        workspace.Write(WorkspaceLayout.DefaultMainRoot + "/notes/_notes.md",
            $"# Notes\n\nNotes.\n\n{RouteScaffoldComposer.AxiomsHeading}\n\n{RouteScaffoldComposer.InheritedAxiomsMarker}\n");
        var before = workspace.SnapshotHashes();

        var result = await workspace.UninstallAsync(workspace.Request(StripCommandKind.Uninstall, []));

        Assert.Equal(CliSemanticStatus.Complete, result.Status);
        Assert.True(result.NoOp);
        Assert.Equal(before, workspace.SnapshotHashes());
    }

    [Fact(DisplayName = "Uninstall stops before any write while a Library is attached and points at library detach"), Trait("Feature", "uninstall"), Trait("Evidence", "Integration")]
    public async Task AttachedLibraryBlocks()
    {
        using var workspace = StripIntegrationWorkspace.Create("uninstall-library-attached");
        await workspace.InstallAsync();
        workspace.Write(LibrarySource + "/note.md", "# Shared\n");
        var attached = await workspace.RunAsync([
            LibraryDefinitions.Group.Name, LibraryAttachDefinitions.Command.Name, LibraryId, LibrarySource,
            LibraryDefinitions.DestinationRoot.Name, WorkspaceLayout.DefaultMainRoot + "/" + LibraryId, LibraryAttachDefinitions.Automatic.Name]);
        Assert.Equal(CliSemanticStatus.Complete, attached.Status);
        var before = workspace.SnapshotHashes();

        var result = await workspace.UninstallAsync(workspace.Request(StripCommandKind.Uninstall, []));

        Assert.Equal(CliSemanticStatus.Blocked, result.Status);
        Assert.Equal(StripFindingCode.LibraryAttached, Assert.Single(result.Findings).Code);
        Assert.Equal(StripResultFactory.LibraryDetach(LibraryId), result.Next?.Command);
        Assert.Equal(before, workspace.SnapshotHashes());
    }

    [Fact(DisplayName = "Uninstall ends its plan with the runtime folder and removes it after the lease"), Trait("Feature", "uninstall"), Trait("Evidence", "Integration")]
    public async Task RuntimeFolderIsThePlansLastStep()
    {
        using var workspace = StripIntegrationWorkspace.Create("uninstall-runtime-step");
        await workspace.InstallAsync();
        var request = workspace.Request(StripCommandKind.Uninstall, []);

        var preview = await workspace.UninstallAsync(request with { DryRun = true });
        var applied = await workspace.UninstallAsync(request);

        var planned = preview.Plan[^1];
        Assert.Equal((StripPlanAction.DeleteDirectory, StripRuntimeFolder.Path, StripPlanReason.State), (planned.Action, planned.Path, planned.Reason));
        Assert.Equal(CliSemanticStatus.Complete, applied.Status);
        Assert.Equal(StripEffectState.Done, applied.Plan[^1].State);
        Assert.False(workspace.Exists(StripRuntimeFolder.Path));
    }

    [Fact(DisplayName = "Uninstall keeps the runtime folder as a plan entry when it still holds earlier recovery data"), Trait("Feature", "uninstall"), Trait("Evidence", "Integration")]
    public async Task RuntimeFolderWithEarlierRecovery()
    {
        using var workspace = StripIntegrationWorkspace.Create("uninstall-runtime-retained");
        await workspace.InstallAsync();
        workspace.Write("scratch.txt", "scratch\n");
        var removed = await workspace.RunAsync([RemoveDefinitions.CommandIdentity, "scratch.txt", RemoveDefinitions.AutomaticOption]);
        Assert.Equal(CliSemanticStatus.Complete, removed.Status);

        var result = await workspace.UninstallAsync(workspace.Request(StripCommandKind.Uninstall, []));

        Assert.Equal(CliSemanticStatus.Complete, result.Status);
        Assert.Contains(result.Kept, kept => kept.Path == StripRuntimeFolder.Path && kept.Reason == StripKeptReason.RecoveryData);
        Assert.Contains(result.Findings, finding => finding.Code == StripFindingCode.RuntimeRetained);
        Assert.DoesNotContain(result.Plan, entry => entry.Path == StripRuntimeFolder.Path);
        Assert.Equal(CommandLines.Cleanup, result.Next?.Command);
        Assert.True(workspace.Exists(StripRuntimeFolder.Path));
        Assert.False(workspace.Exists(WorkspaceLayout.DefaultMainRoot));
    }
}
