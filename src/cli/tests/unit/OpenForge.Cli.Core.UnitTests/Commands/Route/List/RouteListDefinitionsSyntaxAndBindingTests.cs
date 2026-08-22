using System.CommandLine;
using OpenForge.Cli.Core.Commands.Route;
using OpenForge.Cli.Core.Commands.Route.List;
using OpenForge.Cli.Core.Commands.Route.List.Shared.Selection;
using OpenForge.Cli.Core.Shell.Composition;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Parsing;
using OpenForge.Cli.Core.Shell.Pipeline;
using OpenForge.Cli.Core.Shell.Presentation;

namespace OpenForge.Cli.Core.UnitTests.Commands.Route.List;

public sealed class RouteListDefinitionsSyntaxAndBindingTests
{
    [Fact(DisplayName = "Route list definitions own canonical syntax and finding codes")]
    [Trait("Feature", "route-list"), Trait("Evidence", "Unit")]
    public void DefinitionsOwnCanonicalSyntaxAndFindingCodes()
    {
        Assert.Equal("route", RouteDefinitions.RouteGroup.Name);
        Assert.Equal("list", RouteListDefinitions.ListCommand.Name);
        Assert.Equal("source-reference", RouteListDefinitions.SourceReference.Name);
        Assert.Equal("--depth", RouteListDefinitions.Depth.Name);
        Assert.Equal(CliOptionArity.ExactlyOne, RouteListDefinitions.Depth.Arity);
        Assert.Equal("1", RouteListDefinitions.Depth.DefaultValue);
        Assert.Equal("route list", RouteListDefinitions.CommandIdentity);
        Assert.Equal(1, RouteListDefinitions.SchemaVersion);
        Assert.Equal(
            Enum.GetValues<RouteListFindingCode>().Length,
            Enum.GetValues<RouteListFindingCode>()
                .Select(RouteListDefinitions.ReadFindingCode)
                .Distinct(StringComparer.Ordinal)
                .Count());
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            RouteListDefinitions.ReadFindingCode((RouteListFindingCode)int.MaxValue));
    }

    [Fact(DisplayName = "Route list depth represents finite default and all states")]
    [Trait("Feature", "route-list"), Trait("Evidence", "Unit")]
    public void DepthRepresentsFiniteDefaultAndAllStates()
    {
        Assert.Equal(RouteListDepth.Finite(0), RouteListDepth.Finite(0));
        Assert.Equal(1, RouteListDepth.Default.Value);
        Assert.Equal("1", RouteListDepth.Default.MachineValue);
        Assert.Equal(RouteListDepthKind.All, RouteListDepth.All.Kind);
        Assert.Null(RouteListDepth.All.Value);
        Assert.Equal("all", RouteListDepth.All.MachineValue);
        Assert.Throws<ArgumentOutOfRangeException>(() => RouteListDepth.Finite(-1));
    }

    [Fact(DisplayName = "Route list request requires workspace depth and a non-empty optional subject")]
    [Trait("Feature", "route-list"), Trait("Evidence", "Unit")]
    public void RequestRequiresWorkspaceDepthAndNonEmptyOptionalSubject()
    {
        var workspace = RouteListContractTestData.Workspace();

        var allRoots = new RouteListRequest(workspace, null, RouteListDepth.Default);
        var selected = new RouteListRequest(workspace, "memory", RouteListDepth.All);

        Assert.Same(workspace, allRoots.Workspace);
        Assert.Null(allRoots.SourceReference);
        Assert.Equal("memory", selected.SourceReference);
        Assert.Same(RouteListDepth.All, selected.RequestedDepth);
        Assert.Throws<ArgumentNullException>(() => new RouteListRequest(null!, null, RouteListDepth.Default));
        Assert.ThrowsAny<ArgumentException>(() => new RouteListRequest(workspace, "", RouteListDepth.Default));
        Assert.Throws<ArgumentNullException>(() => new RouteListRequest(workspace, null, null!));
    }

    [Fact(DisplayName = "Route list binding closes command-local request and result types")]
    [Trait("Feature", "route-list"), Trait("Evidence", "Unit")]
    public void BindingClosesCommandLocalRequestAndResultTypes()
    {
        var symbols = RouteListBinding.CreateSymbols(RouteBinding.CreateGroup());
        var fallback = InvalidResult();
        var binding = RouteListBinding.Close(
            symbols,
            CliHelpContent.Empty,
            (parse, invocation) => CliBindResult<RouteListRequest, RouteListResult>.Invalid(fallback),
            input => fallback,
            (request, cancellationToken) => ValueTask.FromResult(fallback),
            new CliRendererSet<RouteListResult>(presentation => "human", presentation => "{}"));

        Assert.Equal("route", symbols.RouteGroup.Name);
        Assert.Equal("list", symbols.ListCommand.Name);
        Assert.Same(symbols.ListCommand, binding.Command);
        Assert.Equal(CliWorkspaceRequirement.Required, binding.WorkspaceRequirement);
        Assert.Equal(ArgumentArity.ZeroOrOne, symbols.SourceReference.Arity);
        Assert.Equal(ArgumentArity.ZeroOrOne, symbols.Depth.Arity);
        Assert.Empty(symbols.ListCommand.Aliases);
        Assert.Single(symbols.DelimiterPolicies);
        var omitted = symbols.RouteGroup.Parse(["list"]);
        var explicitDepth = symbols.RouteGroup.Parse(["list", "--depth=2"]);
        var repeated = symbols.RouteGroup.Parse(["list", "--depth=1", "--depth=2"]);
        Assert.Equal("1", omitted.GetValue(symbols.Depth));
        Assert.Equal("2", explicitDepth.GetValue(symbols.Depth));
        Assert.NotEmpty(repeated.Errors);
        Assert.NotNull(CliDelimiterGuard.Validate(
            ["list", "--depth", "2"],
            symbols.DelimiterPolicies));
    }

    [Fact(DisplayName = "Route-list raw depth scanning stops at the delimiter before an option-like source")]
    [Trait("Feature", "route-list"), Trait("Evidence", "Unit")]
    public void RawDepthScanStopsAtTheDelimiter()
    {
        var symbols = RouteListBinding.CreateSymbols(RouteBinding.CreateGroup());
        string[] arguments = ["list", "--", "--depth="];
        var parse = symbols.RouteGroup.Parse(arguments);

        Assert.Empty(parse.Errors);
        Assert.Equal("--depth=", parse.GetValue(symbols.SourceReference));
        Assert.Equal(
            RouteListDefinitions.Depth.DefaultValue,
            RouteListBindingInputPolicy.ReadDepthSpelling(arguments, parse, symbols.Depth));
    }

    private static RouteListFinding Finding(RouteListFindingCode code, CliSemanticStatus status)
    {
        return new RouteListFinding(code, status, "memory", "A bounded route-list finding.");
    }

    private static RouteListResult InvalidResult()
    {
        return RouteListResult.Create(
            CliSemanticStatus.Invalid,
            RouteListContractTestData.Workspace(),
            RouteListSelectionFactory.AttemptedId("invalid"),
            RouteListCoverage.NotStarted(null),
            [],
            [Finding(RouteListFindingCode.InvalidSourceReference, CliSemanticStatus.Invalid)],
            new CliNextAction("open-forge route list --help", "Review valid route-list input."));
    }
}
