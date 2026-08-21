using System.CommandLine;
using OpenForge.Cli.Commands.Route.List;
using OpenForge.Cli.Commands.Route.List.Parsing;
using OpenForge.Cli.Definitions;
using OpenForge.Cli.Parsing;
using OpenForge.Cli.Pipeline;

namespace OpenForge.Cli.UnitTests.Commands.Route.List;

public sealed class RouteListHelpRefinementTests
{
    [Fact(DisplayName = "Route-list help agrees with the actual symbol graph and complete public grammar"), Trait("Feature", "route-list"), Trait("Refinement", "route-list-acceptance"), Trait("Evidence", "Unit")]
    public async Task LeafHelpAgreesWithSymbolGraph()
    {
        var plugin = RouteListCommandPlugin.Create();
        var tree = CliRootTree.Create(plugin.Branch);
        var output = await RunApplicationAsync("route", "list", "--help");

        Assert.Equal(0, output.ExitCode);
        Assert.Equal(string.Empty, output.StandardError);
        Assert.Equal(RouteListDefinitions.RouteCommand.Description, plugin.Symbols.Route.Description);
        Assert.Equal(RouteListDefinitions.ListCommand.Description, plugin.Symbols.List.Description);
        Assert.Equal(RouteListDefinitions.SourceArgument.Description, plugin.Symbols.Source.Description);
        Assert.Equal(RouteListDefinitions.DepthOption.Description, plugin.Symbols.Depth.Description);
        Assert.Equal(RouteListDefinitions.DepthOption.Name, plugin.Symbols.Depth.Name);
        Assert.Equal(CliDefinitions.Options.Workspace.Name, tree.Options.Workspace.Name);
        Assert.Equal(CliDefinitions.Options.Json.Name, tree.Options.Json.Name);
        Assert.Equal(CliDefinitions.Options.View.Name, tree.Options.View.Name);
        Assert.Equal(CliDefinitions.Options.Verbose.Name, tree.Options.Verbose.Name);
        Assert.Equal(CliDefinitions.Options.Help.Name, tree.Options.Help.Name);
        Assert.Equal(CliDefinitions.Options.Version.Name, tree.Options.Version.Name);
        Assert.Equal(RouteListDefinitions.DepthOption.ValueName, plugin.Symbols.Depth.HelpName);
        Assert.Equal(CliDefinitions.Options.Workspace.ValueName, tree.Options.Workspace.HelpName);
        Assert.Equal(CliDefinitions.Options.View.ValueName, tree.Options.View.HelpName);
        Assert.Empty(plugin.Symbols.Depth.Aliases);
        Assert.All(tree.Root.Options, option => Assert.Empty(option.Aliases));

        AssertHelpLineContains(
            output.StandardOutput,
            CliDefinitions.Process.Executable.Name,
            plugin.Symbols.Route.Name,
            plugin.Symbols.List.Name,
            plugin.Symbols.Source.Name,
            plugin.Symbols.Depth.Name);
        AssertHelpLineContains(
            output.StandardOutput,
            plugin.Symbols.Source.Name,
            Assert.IsType<string>(plugin.Symbols.Source.Description));
        AssertOptionHelpAgrees(output.StandardOutput, plugin.Symbols.Depth, RouteListDefinitions.DepthOption);
        AssertOptionHelpAgrees(output.StandardOutput, tree.Options.Workspace, CliDefinitions.Options.Workspace);
        AssertOptionHelpAgrees(output.StandardOutput, tree.Options.Json, CliDefinitions.Options.Json);
        AssertOptionHelpAgrees(output.StandardOutput, tree.Options.View, CliDefinitions.Options.View);
        AssertOptionHelpAgrees(output.StandardOutput, tree.Options.Verbose, CliDefinitions.Options.Verbose);
        AssertOptionHelpAgrees(output.StandardOutput, tree.Options.Help, CliDefinitions.Options.Help);
        AssertOptionHelpAgrees(output.StandardOutput, tree.Options.Version, CliDefinitions.Options.Version);
        Assert.Equal(1, CountOptionDeclarations(output.StandardOutput, RouteListDefinitions.DepthOption.Name));
        Assert.Equal(1, CountOptionDeclarations(output.StandardOutput, CliDefinitions.Options.Workspace.Name));
        Assert.Equal(1, CountOptionDeclarations(output.StandardOutput, CliDefinitions.Options.Json.Name));
        Assert.Equal(1, CountOptionDeclarations(output.StandardOutput, CliDefinitions.Options.View.Name));
        Assert.Equal(1, CountOptionDeclarations(output.StandardOutput, CliDefinitions.Options.Verbose.Name));
        Assert.Equal(1, CountOptionDeclarations(output.StandardOutput, CliDefinitions.Options.Help.Name));
        Assert.Equal(1, CountOptionDeclarations(output.StandardOutput, CliDefinitions.Options.Version.Name));
        Assert.Contains("--depth=<non-negative-integer|all>", output.StandardOutput, StringComparison.Ordinal);
        Assert.Contains("Default: 1", output.StandardOutput, StringComparison.Ordinal);
        Assert.Contains("Examples:", output.StandardOutput, StringComparison.Ordinal);
        Assert.Contains("Related commands:", output.StandardOutput, StringComparison.Ordinal);
    }

    [Fact(DisplayName = "Root help orders Discovery after usage and before global options"), Trait("Feature", "route-list"), Trait("Refinement", "route-list-acceptance"), Trait("Evidence", "Unit")]
    public async Task RootHelpOrdersDiscovery()
    {
        var output = await RunApplicationAsync("--help");

        var usage = output.StandardOutput.IndexOf("Usage:", StringComparison.Ordinal);
        var discovery = output.StandardOutput.IndexOf("Discovery:", StringComparison.Ordinal);
        var options = output.StandardOutput.IndexOf("Options:", StringComparison.Ordinal);
        Assert.True(usage >= 0);
        Assert.True(discovery > usage);
        Assert.True(options > discovery);
        Assert.Contains("open-forge route list", output.StandardOutput, StringComparison.Ordinal);
    }

    private static int CountOptionDeclarations(string help, string optionName)
    {
        return help
            .Split('\n', StringSplitOptions.None)
            .Count(line => line.TrimStart().StartsWith(optionName, StringComparison.Ordinal));
    }

    private static void AssertOptionHelpAgrees<TValue>(
        string help,
        Option<TValue> option,
        CliOptionDefinition definition)
    {
        Assert.Equal(definition.Name, option.Name);
        Assert.Equal(definition.Description, option.Description);
        Assert.Empty(option.Aliases);
        if (definition.ValueName is not null)
        {
            var helpName = Assert.IsType<string>(option.HelpName);
            Assert.Equal(definition.ValueName, helpName);
            AssertHelpLineContains(help, option.Name, helpName, Assert.IsType<string>(option.Description));
            return;
        }

        AssertHelpLineContains(help, option.Name, Assert.IsType<string>(option.Description));
    }

    private static void AssertHelpLineContains(string help, params string[] values)
    {
        Assert.Contains(
            help.Split('\n', StringSplitOptions.None),
            line => values.All(value => line.Contains(value, StringComparison.Ordinal)));
    }

    private static async Task<ApplicationOutput> RunApplicationAsync(params string[] arguments)
    {
        using var standardOutput = new StringWriter();
        using var standardError = new StringWriter();
        var exitCode = await CliApplication.RunAsync(
            arguments,
            new CliOutputWriters(standardOutput, standardError),
            TestContext.Current.CancellationToken);
        return new ApplicationOutput(exitCode, standardOutput.ToString(), standardError.ToString());
    }

    private sealed record ApplicationOutput(int ExitCode, string StandardOutput, string StandardError);
}
