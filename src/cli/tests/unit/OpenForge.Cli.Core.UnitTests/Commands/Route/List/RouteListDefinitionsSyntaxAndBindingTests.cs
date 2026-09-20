using System.CommandLine;
using System.CommandLine.Parsing;
using OpenForge.Cli.Core.Commands.Route;
using OpenForge.Cli.Core.Commands.Route.List;
using OpenForge.Cli.Core.Commands.Route.List.Models.Binding;
using OpenForge.Cli.Core.Commands.Route.List.Models.Result;
using OpenForge.Cli.Core.Commands.Route.List.Shared.Selection;
using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.Core.Shell.Composition.Models;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Invocation.Models;
using OpenForge.Cli.Core.Shell.Parsing;
using OpenForge.Cli.Core.Shell.Parsing.Models.CommandTree;
using OpenForge.Cli.Core.Shell.Parsing.Models.Input;
using OpenForge.Cli.Core.Shell.Pipeline;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Operation;
using OpenForge.Cli.Core.Shell.Presentation.Models;

namespace OpenForge.Cli.Core.UnitTests.Commands.Route.List;

public sealed class RouteListDefinitionsSyntaxAndBindingTests
{
    [Trait("Boundary", "Processing")]
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

    [Trait("Boundary", "Processing")]
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

    [Trait("Boundary", "Input")]
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

    [Trait("Boundary", "Input")]
    [Fact(DisplayName = "Route list binding closes command-local request and result types")]
    [Trait("Feature", "route-list"), Trait("Evidence", "Unit")]
    public void BindingClosesCommandLocalRequestAndResultTypes()
    {
        var symbols = RouteListBinding.CreateSymbols(RouteBinding.CreateGroup());
        var fallback = InvalidResult();
        var binding = RouteListBinding.CreateRequestBinding(
            symbols,
            new RouteListBindingComponents
            {
                Help = CliHelpContent.Empty,
                Operation = (request, cancellationToken) => ValueTask.FromResult(fallback),
            });

        Assert.Equal("route", symbols.RouteGroup.Name);
        Assert.Equal("list", symbols.ListCommand.Name);
        Assert.Same(symbols.ListCommand, binding.Command);
        Assert.Equal(CliWorkspaceRequirement.Required, binding.WorkspaceRequirement);
        Assert.Equal(ArgumentArity.ZeroOrOne, symbols.SourceReference.Arity);
        Assert.Equal(ArgumentArity.ZeroOrOne, symbols.Depth.Arity);
        Assert.Empty(symbols.ListCommand.Aliases);
        var omitted = symbols.RouteGroup.Parse(["list"]);
        var explicitDepth = symbols.RouteGroup.Parse(["list", "--depth=2"]);
        var repeated = symbols.RouteGroup.Parse(["list", "--depth=1", "--depth=2"]);
        Assert.Equal("1", omitted.GetValue(symbols.Depth));
        Assert.Equal("2", explicitDepth.GetValue(symbols.Depth));
        Assert.NotEmpty(repeated.Errors);
        Assert.Equal("2", symbols.RouteGroup.Parse(["list", "--depth", "2"]).GetValue(symbols.Depth));
    }

    [Trait("Boundary", "Input")]
    [Fact(DisplayName = "CLI option result facts expose explicit occurrence and value counts")]
    [Trait("Feature", "cli-parser"), Trait("Evidence", "Unit")]
    public void CliOptionResultFactsExposeExplicitOccurrenceAndValueCounts()
    {
        var tree = CreateRouteListTree(out var symbols);

        var omitted = tree.Parse(["route", "list"]);
        var omittedFacts = CliOptionResultFactsReader.Read(omitted.Result, symbols.Depth);
        Assert.False(omittedFacts.IsExplicit);
        Assert.Equal(0, omittedFacts.IdentifierCount);
        Assert.Equal(0, omittedFacts.ValueCount);

        var explicitValue = tree.Parse(["route", "list", "--depth=2"]);
        var explicitValueFacts = CliOptionResultFactsReader.Read(explicitValue.Result, symbols.Depth);
        Assert.True(explicitValueFacts.IsExplicit);
        Assert.Equal(1, explicitValueFacts.IdentifierCount);
        Assert.Equal(1, explicitValueFacts.ValueCount);

        var explicitNoValue = tree.Parse(["route", "list", "--depth=", "--format=json"]);
        var explicitNoValueFacts = CliOptionResultFactsReader.Read(explicitNoValue.Result, symbols.Depth);
        var jsonFacts = CliOptionResultFactsReader.Read(explicitNoValue.Result, tree.Options.Format);
        Assert.True(explicitNoValueFacts.IsExplicit);
        Assert.Equal(1, explicitNoValueFacts.IdentifierCount);
        Assert.Equal(0, explicitNoValueFacts.ValueCount);
        Assert.True(jsonFacts.IsExplicit);
        Assert.Equal(1, jsonFacts.IdentifierCount);
        Assert.Equal(1, jsonFacts.ValueCount);

        var repeated = tree.Parse(["route", "list", "--depth=1", "--depth=2"]);
        Assert.NotEmpty(repeated.Result.Errors);
        var repeatedFacts = CliOptionResultFactsReader.Read(repeated.Result, symbols.Depth);
        Assert.True(repeatedFacts.IsExplicit);
        Assert.Equal(2, repeatedFacts.IdentifierCount);
        Assert.Equal(2, repeatedFacts.ValueCount);
    }

    [Trait("Boundary", "Input")]
    [Theory(DisplayName = "Route List depth accepts native delimiters and rejects a missing value")]
    [Trait("Feature", "route-list"), Trait("Evidence", "Unit")]
    [InlineData("--depth=1", null, false)]
    [InlineData("--depth", null, true)]
    [InlineData("--depth", "1", false)]
    [InlineData("--depth:1", null, false)]
    public void DepthUsesTypedValuesBeforeTerminator(
        string option,
        string? separateValue,
        bool rejected)
    {
        var symbols = RouteListBinding.CreateSymbols(RouteBinding.CreateGroup());
        string[] arguments = separateValue is null
            ? ["list", option]
            : ["list", option, separateValue];
        var parse = symbols.RouteGroup.Parse(arguments);

        Assert.Empty(parse.Errors);
        var bound = RouteListBinding.Bind(parse, Invocation(RouteListContractTestData.Workspace()), symbols);
        Assert.Equal(rejected, bound.InvalidResult is not null);
        if (!rejected)
        {
            Assert.Equal(RouteListDepth.Finite(1), Assert.IsType<RouteListRequest>(bound.Request).RequestedDepth);
        }
    }

    [Trait("Boundary", "Input")]
    [Theory(DisplayName = "Route List binding maps omitted and boundary depth values to typed requests")]
    [Trait("Feature", "route-list"), Trait("Evidence", "Unit")]
    [InlineData(null, "Finite", 1)]
    [InlineData("0", "Finite", 0)]
    [InlineData("2147483647", "Finite", 2147483647)]
    [InlineData("all", "All", 0)]
    public void BindingMapsTypedDepthBoundaries(
        string? spelling,
        string expectedKind,
        int expectedValue)
    {
        var symbols = RouteListBinding.CreateSymbols(RouteBinding.CreateGroup());
        var workspace = RouteListContractTestData.Workspace();
        string[] arguments = spelling is null
            ? ["list"]
            : ["list", $"--depth={spelling}"];
        var parse = symbols.RouteGroup.Parse(arguments);

        Assert.Empty(parse.Errors);
        var bound = RouteListBinding.Bind(parse, Invocation(workspace), symbols);
        var request = Assert.IsType<RouteListRequest>(bound.Request);
        Assert.Null(bound.InvalidResult);
        Assert.Equal(expectedKind, request.RequestedDepth.Kind.ToString());
        Assert.Equal(
            expectedKind == "All"
                ? null
                : expectedValue,
            request.RequestedDepth.Value);
    }

    [Trait("Boundary", "Input")]
    [Fact(DisplayName = "Route List binding derives depth from typed parser facts")]
    [Trait("Feature", "cli-parser"), Trait("Evidence", "Unit")]
    public void BindingUsesTypedDepthFromParserFacts()
    {
        var symbols = RouteListBinding.CreateSymbols(RouteBinding.CreateGroup());
        var workspace = RouteListContractTestData.Workspace();
        var parse = symbols.RouteGroup.Parse(["list", "--depth=2"]);

        Assert.Empty(parse.Errors);
        var bound = RouteListBinding.Bind(parse, Invocation(workspace), symbols);
        var request = Assert.IsType<RouteListRequest>(bound.Request);
        Assert.Null(bound.InvalidResult);
        Assert.Equal(RouteListDepth.Finite(2), request.RequestedDepth);
    }

    [Trait("Boundary", "Input")]
    [Theory(DisplayName = "Route List binding rejects invalid typed depth values")]
    [Trait("Feature", "route-list"), Trait("Evidence", "Unit")]
    [InlineData("-1")]
    [InlineData("2147483648")]
    [InlineData("unknown")]
    [InlineData("")]
    public void BindingRejectsInvalidTypedDepthValues(string spelling)
    {
        var symbols = RouteListBinding.CreateSymbols(RouteBinding.CreateGroup());
        var workspace = RouteListContractTestData.Workspace();
        var arguments = new[] { "list", $"--depth={spelling}" };
        var parse = symbols.RouteGroup.Parse(arguments);

        Assert.Empty(parse.Errors);
        var bound = RouteListBinding.Bind(parse, Invocation(workspace), symbols);
        var result = Assert.IsType<RouteListResult>(bound.InvalidResult);
        Assert.Null(bound.Request);
        Assert.Equal(CliSemanticStatus.Invalid, result.Status);
        Assert.Equal(RouteListFindingCode.InvalidDepth, Assert.Single(result.Findings).Code);
    }

    [Trait("Boundary", "Input")]
    [Fact(DisplayName = "Route List repeated depth occurrences remain one parser-owned scalar error")]
    [Trait("Feature", "route-list"), Trait("Evidence", "Unit")]
    public void RepeatedDepthOccurrencesRemainParserOwnedScalarError()
    {
        var symbols = RouteListBinding.CreateSymbols(RouteBinding.CreateGroup());
        var arguments = new[] { "list", "--depth=1", "--depth=2" };
        var parse = symbols.RouteGroup.Parse(arguments);

        Assert.NotEmpty(parse.Errors);
        var optionResult = Assert.IsType<OptionResult>(parse.GetResult(symbols.Depth));
        Assert.Equal(2, optionResult.IdentifierTokenCount);
        Assert.Equal(2, optionResult.Tokens.Count(token => token.Type == TokenType.Argument));
    }

    [Trait("Boundary", "Input")]
    [Fact(DisplayName = "Route List binding preserves an option-like source after the terminator")]
    [Trait("Feature", "route-list"), Trait("Evidence", "Unit")]
    public void BindingPreservesOptionLikeSourceAfterTerminator()
    {
        var symbols = RouteListBinding.CreateSymbols(RouteBinding.CreateGroup());
        var workspace = RouteListContractTestData.Workspace();
        string[] arguments = ["list", "--", "--depth="];
        var parse = symbols.RouteGroup.Parse(arguments);

        Assert.Empty(parse.Errors);
        Assert.Equal("--depth=", parse.GetValue(symbols.SourceReference));

        var bound = RouteListBinding.Bind(parse, Invocation(workspace), symbols);
        var request = Assert.IsType<RouteListRequest>(bound.Request);
        Assert.Null(bound.InvalidResult);
        Assert.Equal("--depth=", request.SourceReference);
        Assert.Equal(RouteListDepth.Default, request.RequestedDepth);
    }

    [Trait("Boundary", "Input")]
    [Fact(DisplayName = "Terminal modes reject Route List domain and local input before effects")]
    [Trait("Feature", "cli-parser"), Trait("Evidence", "Unit")]
    public void TerminalModesRejectDomainAndLocalInput()
    {
        var tree = CreateRouteListTree(out _);

        var help = tree.Parse(["route", "list", "root", "--help", "--depth=0"]);
        var helpResolution = CliTerminalValidator.Validate(help);
        var helpInvalid = Assert.IsType<CliInvalidInput>(helpResolution.InvalidInput);
        Assert.Null(helpResolution.Input);
        Assert.Equal(CliInvalidInputSource.Semantic, helpInvalid.Source);
        Assert.Single(helpInvalid.Diagnostics);

        var version = tree.Parse(["route", "list", "--version", "--depth=0"]);
        var versionResolution = CliTerminalValidator.Validate(version);
        var versionInvalid = Assert.IsType<CliInvalidInput>(versionResolution.InvalidInput);
        Assert.Null(versionResolution.Input);
        Assert.Equal(CliInvalidInputSource.Semantic, versionInvalid.Source);
        Assert.Single(versionInvalid.Diagnostics);

        var validGlobals = tree.Parse(
            [
                "route", "list", "--help", "--format=json", "--detail-filter=all", "--detail=minimal",
                "--workspace", Path.GetTempPath(),
            ]);
        var validResolution = CliTerminalValidator.Validate(validGlobals);
        var validInput = Assert.IsType<CliGlobalInput>(validResolution.Input);
        Assert.Null(CliTerminalInputValidator.Validate(validGlobals, validInput));
    }

    private static CliCommandTree CreateRouteListTree(out RouteListSymbols symbols)
    {
        symbols = RouteListBinding.CreateSymbols(RouteBinding.CreateGroup());
        return CliCommandTree.Create(
            CliHelpContent.Empty,
            [new CliRootBranch(symbols.RouteGroup, CliHelpContent.Empty)],
            []);
    }

    private static CliInvocation Invocation(CliWorkspace workspace)
    {
        return new CliInvocation(
            new CliProcessIdentity("open-forge", "test"),
            new CliPresentation(CliFormat.Json, CliDetail.Standard, null),
            CliTerminalMode.None,
            new CliWorkspaceRequest(null, workspace.LexicalRoot),
            workspace);
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
