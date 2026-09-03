using System.CommandLine;
using OpenForge.Cli.Core.Commands.Route;
using OpenForge.Cli.Core.Commands.Route.Move;
using OpenForge.Cli.Core.Commands.Route.Move.Models.Binding;
using OpenForge.Cli.Core.Commands.Route.Move.Models.Request;
using OpenForge.Cli.Core.Commands.Route.Move.Models.Result;
using OpenForge.Cli.Core.Commands.Route.Move.Shared.Binding;

namespace OpenForge.Cli.Core.UnitTests.Commands.Route.Move;

public sealed class RouteMoveBindingTests
{
    [Fact(DisplayName = "Route Move symbols expose the exact two-operand command grammar"), Trait("Feature", "route-move"), Trait("Evidence", "Unit")]
    public void SymbolsExposeExactTwoOperandCommandGrammar()
    {
        var routeGroup = RouteBinding.CreateGroup();
        var symbols = RouteMoveBindingTestData.CreateBinding().CreateSymbols(routeGroup);

        Assert.Same(routeGroup, symbols.RouteGroup);
        Assert.Same(symbols.MoveCommand, Assert.Single(routeGroup.Subcommands));
        Assert.Equal("move", symbols.MoveCommand.Name);
        Assert.Empty(symbols.MoveCommand.Aliases);
        Assert.Equal([symbols.SourceReference, symbols.DestinationTarget], symbols.MoveCommand.Arguments);
        Assert.Equal("source-reference", symbols.SourceReference.Name);
        Assert.Equal(ArgumentArity.ZeroOrOne, symbols.SourceReference.Arity);
        Assert.Equal("destination-target", symbols.DestinationTarget.Name);
        Assert.Equal(ArgumentArity.ZeroOrOne, symbols.DestinationTarget.Arity);
        Assert.Equal("--dry-run", symbols.DryRun.Name);
        Assert.Equal(ArgumentArity.Zero, symbols.DryRun.Arity);
        Assert.Contains(symbols.DryRun, symbols.MoveCommand.Options);
    }

    [Theory(DisplayName = "Route Move parser rejects every repeated positional operand")]
    [InlineData("memory/docs/_docs.md", "memory/docs/_docs.md", "memory/archive/_docs.md")]
    [InlineData("memory/docs/_docs.md", "memory/archive/_docs.md", "memory/archive/_docs.md")]
    [Trait("Feature", "route-move"), Trait("Evidence", "Unit")]
    public void ParserRejectsRepeatedPositionalOperands(
        string firstOperand,
        string secondOperand,
        string repeatedOperand)
    {
        var routeGroup = RouteBinding.CreateGroup();
        _ = RouteMoveBindingTestData.CreateBinding().CreateSymbols(routeGroup);

        var parseResult = routeGroup.Parse(
        [
            "move",
            firstOperand,
            secondOperand,
            repeatedOperand,
        ]);

        Assert.NotEmpty(parseResult.Errors);
    }

    [Theory(DisplayName = "Route Move validator preserves complete apply and dry-run facts")]
    [InlineData(false)]
    [InlineData(true)]
    [Trait("Feature", "route-move"), Trait("Evidence", "Unit")]
    public void ValidatorPreservesCompleteApplyAndDryRunFacts(bool dryRun)
    {
        var mode = dryRun ? RouteMoveMode.DryRun : RouteMoveMode.Apply;
        var validation = new RouteMoveBindingValidator().Validate(
            new RouteMoveBindingInput
            {
                SourceReference = "memory/docs/_docs.md",
                DestinationTarget = "memory/archive/_docs.md",
                ParserErrors = [],
            },
            mode);

        var facts = Assert.IsType<RouteMoveBindingFacts>(validation.Facts);
        Assert.Null(validation.Failure);
        Assert.Equal("memory/docs/_docs.md", facts.SourceReference);
        Assert.Equal("memory/archive/_docs.md", facts.DestinationTarget);
        Assert.Equal(mode, facts.Mode);
    }

    [Theory(DisplayName = "Route Move validator rejects missing, blank, and parser-invalid input")]
    [InlineData(null, "memory/archive/_docs.md", false)]
    [InlineData("", "memory/archive/_docs.md", false)]
    [InlineData("   ", "memory/archive/_docs.md", false)]
    [InlineData("memory/docs/_docs.md", null, false)]
    [InlineData("memory/docs/_docs.md", "", false)]
    [InlineData("memory/docs/_docs.md", "   ", false)]
    [InlineData("memory/docs/_docs.md", "memory/archive/_docs.md", true)]
    [Trait("Feature", "route-move"), Trait("Evidence", "Unit")]
    public void ValidatorRejectsMissingBlankAndParserInvalidInput(
        string? sourceReference,
        string? destinationTarget,
        bool hasParserError)
    {
        IReadOnlyList<string> parserErrors = hasParserError
            ? ["The parser rejected repeated operands."]
            : [];
        var validation = new RouteMoveBindingValidator().Validate(
            new RouteMoveBindingInput
            {
                SourceReference = sourceReference,
                DestinationTarget = destinationTarget,
                ParserErrors = parserErrors,
            },
            RouteMoveMode.Apply);

        Assert.Null(validation.Facts);
        var failure = Assert.IsType<RouteMoveBindingFailure>(validation.Failure);
        Assert.Equal(RouteMoveFindingCode.InvalidInput, failure.Code);
        Assert.False(string.IsNullOrWhiteSpace(failure.Cause));
    }
}
