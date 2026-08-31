using System.CommandLine;
using System.Runtime.CompilerServices;
using OpenForge.Cli.Core.Shell.Composition;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Parsing;
using OpenForge.Cli.Core.Shell.Parsing.Models;
using OpenForge.Cli.Composition;
using OpenForge.Cli.IntegrationTests.Hosting;
using OpenForge.Cli.TestSupport;

namespace OpenForge.Cli.IntegrationTests.Commands.Route.Init;

public sealed class RouteInitApplicationIntegrationTests
{
    [Fact(DisplayName = "Composed root registers Route Init as one nested route leaf"), Trait("Feature", "route-init-presentation"), Trait("Evidence", "Integration")]
    public void ComposedRootRegistersOneNestedRouteInitLeaf()
    {
        var application = CliCompositionRoot.Create(
            new CliProcessIdentity("open-forge", "test"));
        var parser = CliCoreApplicationAccess.Parser(application);
        var tree = CliParserAccess.Tree(parser);
        var parse = tree.Parse(["route", "init"]);
        var selection = CliBindingSelector.Select(parse);

        Assert.Equal("init", selection.Command.Name);
        Assert.Equal(CliBindingSelectionState.Leaf, selection.State);
        Assert.False(tree.IsGroup(selection.Command));
        Assert.NotNull(selection.Binding);
        Assert.Same(selection.Command, selection.Binding!.Command);
    }

    [Fact(DisplayName = "Composed Route parser accepts equals tag values and rejects separated tag values"), Trait("Feature", "route-init-presentation"), Trait("Evidence", "Integration")]
    public void ComposedRouteParserUsesTheFrozenTagDelimiter()
    {
        var application = CliCompositionRoot.Create(
            new CliProcessIdentity("open-forge", "test"));
        var parser = CliCoreApplicationAccess.Parser(application);
        var tree = CliParserAccess.Tree(parser);
        var equals = tree.Parse(
            ["route", "init", "memory/project-alpha/documents", "--tag=Memory"]);
        var equalsSelection = CliBindingSelector.Select(equals);

        Assert.Equal(
            ["--depth", "--tag"],
            equals.DelimiterPolicies.Select(policy => policy.OptionName));
        Assert.Equal(
            1,
            equals.DelimiterPolicies.Count(policy => policy.OptionName == "--tag"));
        Assert.Equal(CliBindingSelectionState.Leaf, equalsSelection.State);
        Assert.NotNull(equalsSelection.Binding);
        Assert.Null(CliTerminalValidator.Validate(equals).InvalidInput);
        var tag = Assert.Single(equalsSelection.Command.Options.OfType<Option<string[]>>());
        var tagValues = Assert.IsType<string[]>(equals.Result.GetValue(tag));
        Assert.Equal(["Memory"], tagValues);

        var separated = tree.Parse(
            ["route", "init", "memory/project-alpha/documents", "--tag", "Memory"]);
        var separatedResolution = CliTerminalValidator.Validate(separated);

        var invalid = Assert.IsType<CliInvalidInput>(separatedResolution.InvalidInput);
        Assert.Equal("cli.delimiter.invalid", invalid.Code);
        Assert.Equal(CliInvalidInputSource.Delimiter, invalid.Source);
    }

    [Fact(DisplayName = "Composed root help exposes Route Init exactly once in Discovery"), Trait("Feature", "route-init-presentation"), Trait("Evidence", "Integration")]
    public async Task ComposedRootHelpExposesRouteInitOnceInDiscovery()
    {
        using var workspace = TemporaryWorkspace.Create("route-init-composed-help");

        var result = await CliHostCapture.RunAsync(
            ["--help"],
            workspace.Path);

        Assert.Equal(0, result.ExitCode);
        Assert.Equal(string.Empty, result.Error);
        var discovery = result.Output
            .Split(Environment.NewLine, StringSplitOptions.None)
            .SkipWhile(line => !line.Equals("Discovery:", StringComparison.Ordinal))
            .Skip(1)
            .TakeWhile(line => line.StartsWith("  ", StringComparison.Ordinal))
            .Where(line => line.TrimStart().StartsWith("route init", StringComparison.Ordinal))
            .ToArray();
        Assert.Single(discovery);
    }

    [Fact(DisplayName = "Composed Route Init help comes from its registered binding without workspace effects"), Trait("Feature", "route-init-presentation"), Trait("Evidence", "Integration")]
    public async Task ComposedRouteInitHelpComesFromRegisteredBinding()
    {
        using var workspace = TemporaryWorkspace.Create("route-init-leaf-help");

        var result = await CliHostCapture.RunAsync(
            ["route", "init", "--help"],
            workspace.Path);

        Assert.Equal(0, result.ExitCode);
        Assert.Equal(string.Empty, result.Error);
        Assert.Contains("open-forge route init", result.Output, StringComparison.Ordinal);
        Assert.Contains("Scaffold mode", result.Output, StringComparison.Ordinal);
        Assert.Contains("Results and streams", result.Output, StringComparison.Ordinal);
    }

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
