using OpenForge.Cli.Core.Commands.Index;
using OpenForge.Cli.Core.Commands.Library;
using OpenForge.Cli.Core.Commands.Library.Attach;
using OpenForge.Cli.Core.Commands.Remove;
using OpenForge.Cli.Core.Commands.Shared.RouteScaffolding;
using OpenForge.Cli.Core.Commands.Stripping;
using OpenForge.Cli.Core.Commands.Stripping.Models.Planning;
using OpenForge.Cli.Core.Commands.Stripping.Models.Request;
using OpenForge.Cli.Core.Commands.Stripping.Models.Result;
using OpenForge.Cli.Core.Framework.Documents.Markdown;
using OpenForge.Cli.Core.Framework.Documents.Markdown.Shared.ManagedHosts;
using OpenForge.Cli.Core.Framework.Workspace.Models.Layout;
using OpenForge.Cli.Core.Framework.Workspace.Shared.Runtime;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.IntegrationTests.Commands.Stripping;

namespace OpenForge.Cli.IntegrationTests.Commands.Uninstall;

// Covers the uninstall Behavior contract's Required Evidence beyond the ordinary removal boundary.
public sealed class UninstallBoundaryIntegrationTests
{
    private const string Docs = "docs";
    private const string Heading = "# Team\n\nOur rules.\n";
    private const string LibraryId = "shared";
    private const string LibrarySource = "vendor/" + LibraryId;

    [Fact(DisplayName = "Uninstall keeps a legacy-marked host section as edited and removes only that section when it is named"), Trait("Feature", "uninstall"), Trait("Evidence", "Integration")]
    public async Task LegacyHostSection()
    {
        using var workspace = StripIntegrationWorkspace.Create("uninstall-legacy-host");
        await workspace.InstallAsync();
        var installed = workspace.Read(WorkspaceLayoutNames.AgentHostFileName);
        var section = new MarkdownDocumentParser().Parse(installed).ManagedHost.Span!;
        var footer = $"**{MarkdownManagedHostReader.FooterText}**";
        var body = installed[section.Start..section.End].Replace(footer, string.Empty, StringComparison.Ordinal).TrimEnd('\n');
        workspace.Write(WorkspaceLayoutNames.AgentHostFileName,
            $"{Heading}\n{MarkdownManagedHostReader.LegacyStart}\n{body}\n{MarkdownManagedHostReader.LegacyEnd}\n");
        var request = workspace.Request(StripCommandKind.Uninstall, []);

        var preview = await workspace.UninstallAsync(request with { DryRun = true });
        var applied = await workspace.UninstallAsync(request with { RemoveEdited = [WorkspaceLayoutNames.AgentHostFileName] });

        Assert.Equal(WorkspaceLayoutNames.AgentHostFileName, Assert.Single(preview.Kept).Path);
        Assert.Equal(CliSemanticStatus.Complete, applied.Status);
        Assert.Equal(Heading, workspace.Read(WorkspaceLayoutNames.AgentHostFileName));
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

    [Fact(DisplayName = "Uninstall stops before any write while a Library is attached"), Trait("Feature", "uninstall"), Trait("Evidence", "Integration")]
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
        Assert.Equal(before, workspace.SnapshotHashes());
    }

    [Fact(DisplayName = "Uninstall keeps the runtime folder when it still holds earlier recovery data"), Trait("Feature", "uninstall"), Trait("Evidence", "Integration")]
    public async Task RuntimeFolderWithEarlierRecovery()
    {
        using var workspace = StripIntegrationWorkspace.Create("uninstall-runtime-retained");
        await workspace.InstallAsync();
        workspace.Write("scratch.txt", "scratch\n");
        var removed = await workspace.RunAsync([RemoveDefinitions.CommandIdentity, "scratch.txt", RemoveDefinitions.AutomaticOption]);
        Assert.Equal(CliSemanticStatus.Complete, removed.Status);

        var result = await workspace.UninstallAsync(workspace.Request(StripCommandKind.Uninstall, []));

        Assert.Equal(CliSemanticStatus.Complete, result.Status);
        Assert.Contains(result.Findings, finding => finding.Code == StripFindingCode.RuntimeRetained);
        Assert.True(workspace.Exists(WorkspaceRuntimePaths.DirectoryName));
        Assert.False(workspace.Exists(WorkspaceLayout.DefaultMainRoot));
    }

    // Mirrors the demo-evals harness: a frozen spec placed into scope folders with metadata and relinked
    // links, routed and indexed, must strip back to the frozen bytes at the docs root and nothing else.
    [Fact(DisplayName = "Uninstall with metadata stripping and flatten returns a scoped workspace to its frozen documents"), Trait("Feature", "uninstall"), Trait("Evidence", "Integration")]
    public async Task ScopedWorkspaceReturnsToFrozenDocuments()
    {
        using var workspace = StripIntegrationWorkspace.Create("uninstall-eval-round-trip");
        var frozen = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            [Docs + "/README.md"] = "# Game\n\nRead the [Rules](rules.md#moves) and [play](#play).\n\n## Play\n\nHave fun.\n",
            [Docs + "/rules.md"] = "# Rules\n\nBack to the [game](README.md). See [React](https://react.dev/versions).\n\n## Moves\n\nTurn two cards.\n",
        };
        await workspace.InstallAsync();
        workspace.Write(Docs + "/README.md", Metadata("Game map", "LoadNow, Game") + frozen[Docs + "/README.md"].Replace("](rules.md", "](gameplay/rules.md", StringComparison.Ordinal));
        workspace.Write(Docs + "/gameplay/rules.md", Metadata("Game rules", "Game") + frozen[Docs + "/rules.md"].Replace("](README.md", "](../README.md", StringComparison.Ordinal));
        workspace.Write(Docs + "/gameplay/_gameplay.md", Metadata("Gameplay scope", "Game") + $"# Gameplay\n\nGameplay scope.\n\n{RouteScaffoldComposer.AxiomsHeading}\n\n{RouteScaffoldComposer.InheritedAxiomsMarker}\n\n## Entries\n");
        await workspace.IncludeAsync(Docs);
        Assert.Equal(CliSemanticStatus.Complete, (await workspace.RunAsync([IndexDefinitions.CommandIdentity])).Status);

        var result = await workspace.UninstallAsync(workspace.Request(StripCommandKind.Uninstall, [StripPart.Metadata], flatten: Docs));

        Assert.Equal(CliSemanticStatus.Complete, result.Status);
        Assert.Equal(frozen.Keys.Order(StringComparer.Ordinal), workspace.SnapshotHashes().Keys.Order(StringComparer.Ordinal));
        Assert.All(frozen, pair => Assert.Equal(pair.Value, workspace.Read(pair.Key)));
    }

    private static string Metadata(string description, string tags)
        => $"---\nopen-forge:\n  description: {description}\n  tags: [{tags}]\n---\n";
}
