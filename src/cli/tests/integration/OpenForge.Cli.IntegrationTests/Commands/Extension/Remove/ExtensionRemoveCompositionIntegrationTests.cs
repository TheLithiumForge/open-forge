using System.Runtime.CompilerServices;
using OpenForge.Cli.Composition;
using OpenForge.Cli.Core.Shell.Composition;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Parsing;
using OpenForge.Cli.IntegrationTests.Commands.Extension.Install;

namespace OpenForge.Cli.IntegrationTests.Commands.Extension.Remove;

public sealed class ExtensionRemoveCompositionIntegrationTests
{
    [Fact(
        DisplayName = "Composed root registers Extension Remove as the final Extension leaf"),
     Trait("Feature", "extension-remove"), Trait("Evidence", "Integration")]
    public void ComposedRootRegistersExtensionRemoveLeaf()
    {
        var application = CliCompositionRoot.Create(
            new CliProcessIdentity("open-forge", "test"));
        var parser = CliCoreApplicationAccess.Parser(application);
        var tree = CliParserAccess.Tree(parser);
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
        Assert.Contains("--prune", run.StandardOutput, StringComparison.Ordinal);
        Assert.Contains("--dry-run", run.StandardOutput, StringComparison.Ordinal);
        Assert.DoesNotContain("--source", run.StandardOutput, StringComparison.Ordinal);
        Assert.DoesNotContain("--force", run.StandardOutput, StringComparison.Ordinal);
        Assert.Equal(before, workspace.Snapshot());
        Assert.False(workspace.LockInfrastructureExists);
    }

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
            "--automatic", "--dry-run", "--json",
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
            root.GetProperty("result").GetProperty("selection").GetProperty("ids")
                .EnumerateArray()).GetString());
    }

    private static async Task InstallAsync(
        ExtensionInstallIntegrationWorkspace workspace,
        ExtensionInstallCatalogue source)
    {
        var run = await workspace.RunAsync(
        [
            "extension", "install", "toolkit",
            "--source", source.Path,
            "--automatic", "--json",
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

    private static class CliCoreApplicationAccess
    {
        [UnsafeAccessor(UnsafeAccessorKind.Field, Name = "_parser")]
        internal static extern ref CliParser Parser(CliCoreApplication application);
    }

    private static class CliParserAccess
    {
        [UnsafeAccessor(UnsafeAccessorKind.Field, Name = "_tree")]
        internal static extern ref CliCommandTree Tree(CliParser parser);
    }
}
