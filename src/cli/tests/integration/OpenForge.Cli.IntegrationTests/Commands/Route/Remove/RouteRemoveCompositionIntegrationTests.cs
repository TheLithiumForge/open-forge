using OpenForge.Cli.Composition;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Invocation.Models;
using OpenForge.Cli.Core.Shell.Parsing;
using OpenForge.Cli.Core.Shell.Parsing.Models.Results;
using OpenForge.Cli.IntegrationTests.Commands.Shared.Composition;

namespace OpenForge.Cli.IntegrationTests.Commands.Route.Remove;

public sealed class RouteRemoveCompositionIntegrationTests
{
    [Trait("Boundary", "Host")]
    [Fact(DisplayName = "Composed root registers Route Remove as the final Route leaf"),
     Trait("Feature", "route-remove"), Trait("Evidence", "Integration")]
    public void ComposedRootRegistersRouteRemoveLeaf()
    {
        var application = CliCompositionRoot.Create(
            new CliProcessIdentity("open-forge", "test"));
        var tree = CliCoreApplicationAccess.Tree(application);
        var route = Assert.Single(
            tree.Root.Subcommands,
            command => command.Name == "route");

        Assert.Equal(
            ["list", "inspect", "init", "create", "update", "move", "remove"],
            route.Subcommands.Select(command => command.Name));
        var remove = Assert.Single(route.Subcommands, command => command.Name == "remove");
        var parse = tree.Parse(["route", "remove"]);
        var selection = CliBindingSelector.Select(parse);

        Assert.Same(remove, selection.Command);
        Assert.Equal(CliBindingSelectionState.Leaf, selection.State);
        Assert.NotNull(selection.Binding);
    }

    [Trait("Boundary", "Host")]
    [Fact(DisplayName = "Composed Route Remove help is terminal and bypasses workspace effects"),
     Trait("Feature", "route-remove"), Trait("Evidence", "Integration")]
    public async Task RemoveHelpIsTerminalAndWriteFree()
    {
        using var workspace = RouteRemoveIntegrationWorkspace.Create("route-remove-composed-help");
        var before = workspace.SnapshotHashes();
        var standardOutput = new StringWriter();
        var standardError = new StringWriter();

        var completion = await workspace.RunAsync(
            ["route", "remove", "--help", "--workspace", workspace.Combine("missing")],
            standardOutput,
            standardError);

        Assert.Equal(0, completion.ExitCode);
        Assert.Equal(CliSemanticStatus.Complete, completion.Status);
        Assert.Equal(string.Empty, standardError.ToString());
        Assert.Contains("open-forge route remove <source-reference>", standardOutput.ToString(), StringComparison.Ordinal);
        Assert.Contains("--dry-run", standardOutput.ToString(), StringComparison.Ordinal);
        Assert.Contains("--automatic", standardOutput.ToString(), StringComparison.Ordinal);
        Assert.DoesNotContain("--force", standardOutput.ToString(), StringComparison.Ordinal);
        Assert.DoesNotContain("--recursive", standardOutput.ToString(), StringComparison.Ordinal);
        Assert.Equal(before, workspace.SnapshotHashes());
        workspace.AssertNoLockInfrastructure();
    }
}
