using OpenForge.Cli.Core.Commands.Route.Inspect.Models.Operation;
using System.CommandLine;
using OpenForge.Cli.Core.Commands.Route;
using OpenForge.Cli.Core.Commands.Route.Inspect;
using OpenForge.Cli.Core.Commands.Route.Inspect.Models.Binding;
using OpenForge.Cli.Core.Commands.Route.Inspect.Models.Result;
using OpenForge.Cli.Core.Commands.Route.List;
using OpenForge.Cli.Core.Presentation.Legacy.Route.Shared.Rendering;
using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Invocation.Models;
using OpenForge.Cli.Core.Shell.Presentation.Models;
using OpenForge.Cli.Core.UnitTests.Commands.Route.Inspect.Shared.Presentation;

namespace OpenForge.Cli.Core.UnitTests.Commands.Route.Inspect;

public sealed class RouteInspectBindingAndCompositionTests
{
    [Fact(DisplayName = "Manual Route graph composes List and Inspect with current group notes")]
    [Trait("Feature", "route-inspect"), Trait("Evidence", "Unit"), Trait("Boundary", "Input")]
    public void RouteFamilyComposesOneGroupWithExactChildren()
    {
        var route = RouteBinding.CreateGroup();
        var list = RouteListBinding.CreateSymbols(route);
        var inspect = RouteInspectBinding.CreateSymbols(route);

        Assert.Same(route, list.RouteGroup);
        Assert.Same(route, inspect.RouteGroup);
        Assert.Equal(["list", "inspect"], route.Subcommands.Select(command => command.Name));
        Assert.Same(list.ListCommand, route.Subcommands[0]);
        Assert.Same(inspect.InspectCommand, route.Subcommands[1]);

        var help = RouteHelpSections.CreateGroup();
        var section = Assert.Single(help.Sections);
        Assert.Equal("Command help", section.Heading);
        var text = string.Join(Environment.NewLine, help.Sections.Select(section => section.Body));
        Assert.DoesNotContain(" available", text, StringComparison.Ordinal);
        Assert.DoesNotContain("list", text, StringComparison.Ordinal);
        Assert.DoesNotContain("inspect", text, StringComparison.Ordinal);
        Assert.DoesNotContain("init", text, StringComparison.Ordinal);
        Assert.DoesNotContain("create", text, StringComparison.Ordinal);
        Assert.Contains("Use open-forge route <command> --help", text, StringComparison.Ordinal);
        Assert.DoesNotContain("update", text, StringComparison.Ordinal);
        Assert.DoesNotContain("Planned but unavailable operation: remove.", text, StringComparison.Ordinal);
    }

    [Fact(DisplayName = "Route Inspect owns one parser argument with omission and typed semantic cardinality")]
    [Trait("Feature", "route-inspect"), Trait("Evidence", "Unit"), Trait("Boundary", "Input")]
    public void InspectArgumentUsesParserOwnedValuesAndTypedCardinality()
    {
        var route = new Command("route");
        var symbols = RouteInspectBinding.CreateSymbols(route);
        var invocation = Invocation(RouteInspectPresentationTestDataWorkspace());

        Assert.Equal(ArgumentArity.ZeroOrMore, symbols.SourceReferences.Arity);
        Assert.Equal(typeof(string[]), symbols.SourceReferences.ValueType);

        var missing = RouteInspectBinding.Bind(
            route.Parse(["inspect"]),
            invocation,
            symbols);
        var missingResult = Assert.IsType<RouteInspectResult>(missing.InvalidResult);
        Assert.Null(missing.Request);
        Assert.Equal(CliSemanticStatus.Invalid, missingResult.Status);
        Assert.Equal(RouteInspectConditionCode.MissingSource, Assert.Single(missingResult.Conditions).Code);

        var multiple = RouteInspectBinding.Bind(
            route.Parse(["inspect", "first", "second"]),
            invocation,
            symbols);
        var multipleResult = Assert.IsType<RouteInspectResult>(multiple.InvalidResult);
        Assert.Null(multiple.Request);
        Assert.Equal(CliSemanticStatus.Invalid, multipleResult.Status);
        Assert.Equal(RouteInspectConditionCode.MultipleSources, Assert.Single(multipleResult.Conditions).Code);

        var one = RouteInspectBinding.Bind(
            route.Parse(["inspect", "one"]),
            invocation,
            symbols);
        var request = Assert.IsType<OpenForge.Cli.Core.Commands.Route.Inspect.Models.Operation.RouteInspectRequest>(one.Request);
        Assert.Null(one.InvalidResult);
        Assert.Equal("one", request.SourceReference);
        Assert.True(request.AllowInteractiveSourceSelection);

        var json = RouteInspectBinding.Bind(
            route.Parse(["inspect", "one"]),
            Invocation(RouteInspectPresentationTestDataWorkspace(), CliFormat.Json),
            symbols);
        var jsonRequest = Assert.IsType<RouteInspectRequest>(json.Request);
        Assert.False(jsonRequest.AllowInteractiveSourceSelection);

        var optionLike = RouteInspectBinding.Bind(
            route.Parse(["inspect", "--", "--view"]),
            invocation,
            symbols);
        var optionLikeRequest = Assert.IsType<OpenForge.Cli.Core.Commands.Route.Inspect.Models.Operation.RouteInspectRequest>(optionLike.Request);
        Assert.Equal("--view", optionLikeRequest.SourceReference);
    }

    private static CliWorkspace RouteInspectPresentationTestDataWorkspace()
    {
        var root = Path.GetFullPath(Path.Combine(Path.GetTempPath(), "route-inspect-presentation-unit"));
        return new CliWorkspace(root, root, CliWorkspaceSelectionMethod.ExplicitWorkspace);
    }

    private static CliInvocation Invocation(
        CliWorkspace workspace,
        CliFormat outputFormat = CliFormat.Text)
    {
        return new CliInvocation(
            new CliProcessIdentity("open-forge", "test"),
            new CliPresentation(outputFormat, CliDetail.Standard, null),
            CliTerminalMode.None,
            new CliWorkspaceRequest(workspace.LexicalRoot, workspace.LexicalRoot),
            workspace);
    }
}
