using OpenForge.Cli.Composition;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Invocation.Models;
using OpenForge.Cli.Core.Shell.Parsing;
using OpenForge.Cli.Core.Shell.Parsing.Models.Results;
using OpenForge.Cli.IntegrationTests.Commands.Extension.Install;
using OpenForge.Cli.IntegrationTests.Commands.Shared.Composition;

namespace OpenForge.Cli.IntegrationTests.Commands.Extension.Remove;

public sealed class ExtensionRemoveCompositionIntegrationTests
{
    [Trait("Boundary", "Host")]
    [Fact(
        DisplayName = "Composed root registers Extension Remove as the final Extension leaf"),
     Trait("Feature", "extension-remove"), Trait("Evidence", "Integration")]
    public void ComposedRootRegistersExtensionRemoveLeaf()
    {
        var application = CliCompositionRoot.Create(
            new CliProcessIdentity("open-forge", "test"));
        var tree = CliCoreApplicationAccess.Tree(application);
        var extension = Assert.Single(
            tree.Root.Subcommands,
            command => command.Name == "extension");

        Assert.Equal(
            ["list", "inspect", "create", "install", "update", "remove"],
            extension.Subcommands.Select(command => command.Name));
        var remove = Assert.Single(
            extension.Subcommands,
            command => command.Name == "remove");
        var parse = tree.Parse(["extension", "remove", "toolkit"]);
        var selection = CliBindingSelector.Select(parse);

        Assert.Same(remove, selection.Command);
        Assert.Equal(CliBindingSelectionState.Leaf, selection.State);
        Assert.NotNull(selection.Binding);
    }

    [Trait("Boundary", "Host")]
    [Fact(
        DisplayName = "Composed Extension Remove help is terminal and bypasses workspace effects"),
     Trait("Feature", "extension-remove"), Trait("Evidence", "Integration")]
    public async Task RemoveHelpIsTerminalAndWriteFree()
    {
        using var workspace = ExtensionInstallIntegrationWorkspace.Create(
            "extension-remove-composed-help");
        var before = workspace.Snapshot();

        var run = await workspace.RunAsync(
        [
            "extension", "remove", "--help",
            "--workspace", workspace.Combine("missing-workspace"),
        ]);

        Assert.Equal(0, run.ExitCode);
        Assert.Equal(CliSemanticStatus.Complete, run.Status);
        Assert.Equal(string.Empty, run.StandardError);
        Assert.Contains(
            "open-forge extension remove [<stable-id>...]",
            run.StandardOutput,
            StringComparison.Ordinal);
        Assert.DoesNotContain("--prune", run.StandardOutput, StringComparison.Ordinal);
        Assert.Contains("--dry-run", run.StandardOutput, StringComparison.Ordinal);
        Assert.DoesNotContain("--source", run.StandardOutput, StringComparison.Ordinal);
        Assert.DoesNotContain("--force", run.StandardOutput, StringComparison.Ordinal);
        Assert.Equal(before, workspace.Snapshot());
        Assert.False(workspace.LockInfrastructureExists);
    }

    [Trait("Boundary", "Host")]
    [Fact(
        DisplayName = "Composed Extension Remove binds one explicit workspace without root discovery"),
     Trait("Feature", "extension-remove"), Trait("Evidence", "Integration")]
    public async Task ExplicitWorkspaceIsBoundWithoutDiscovery()
    {
        using var workspace = ExtensionInstallIntegrationWorkspace.Create(
            "extension-remove-explicit-workspace");
        await workspace.SeedFrameworkAsync();
        using var source = ExtensionInstallCatalogue.Create(
            "extension-remove-explicit-workspace-source");
        source.AddPackage(
            "toolkit",
            [],
            (".agents/toolkit.md", Document("Toolkit")));
        await InstallAsync(workspace, source);

        var run = await workspace.RunAsync(
        [
            "extension", "remove", "toolkit",
            "--workspace", workspace.Path,
            "--automatic", "--dry-run", "--format", "json",
        ]);

        Assert.Equal(0, run.ExitCode);
        Assert.Equal(CliSemanticStatus.Complete, run.Status);
        Assert.Equal(string.Empty, run.StandardError);
        using var document = System.Text.Json.JsonDocument.Parse(run.StandardOutput);
        var root = document.RootElement;
        Assert.Equal(workspace.Path, root.GetProperty("workspace").GetProperty("path").GetString());
        Assert.Equal(
            "explicit-workspace",
            root.GetProperty("workspace").GetProperty("selectedBy").GetString());
        Assert.Equal("toolkit", Assert.Single(
            root.GetProperty("data").GetProperty("packages").EnumerateArray())
            .GetProperty("id").GetString());
    }

    private static async Task InstallAsync(
        ExtensionInstallIntegrationWorkspace workspace,
        ExtensionInstallCatalogue source)
    {
        var run = await workspace.RunAsync(
        [
            "extension", "install", "toolkit",
            "--source", source.Path,
            "--automatic", "--format", "json",
        ]);

        Assert.Equal(0, run.ExitCode);
        Assert.Equal(CliSemanticStatus.Complete, run.Status);
        Assert.Equal(string.Empty, run.StandardError);
    }

    private static string Document(string heading)
        => OpenForge.Cli.TestSupport.OpenForgeDocumentSeed.Metadata(
            heading,
            ["Extension"],
            $"# {heading}\n");
}
