using System.CommandLine;
using OpenForge.Cli.Core.Commands.Repair;
using OpenForge.Cli.Core.Commands.Repair.Models.Binding;
using OpenForge.Cli.Core.Commands.Repair.Models.Request;
using OpenForge.Cli.Core.Commands.Repair.Models.Result;
using OpenForge.Cli.Core.Commands.Repair.Models.Selection;
using OpenForge.Cli.Core.Shell.Composition;
using OpenForge.Cli.Core.Shell.Composition.Models;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Parsing;
using OpenForge.Cli.Core.Shell.Presentation;

namespace OpenForge.Cli.Core.UnitTests.Commands.Repair;

public sealed class RepairBindingAndRequestTests
{
    [Fact(DisplayName = "Repair symbols expose the exact operand-free command and operation-specific options"), Trait("Feature", "repair"), Trait("Evidence", "Unit")]
    public void SymbolsExposeExactRepairGrammar()
    {
        var symbols = RepairBinding.CreateSymbols();

        Assert.Equal("repair", symbols.RepairCommand.Name);
        Assert.Empty(symbols.RepairCommand.Aliases);
        Assert.Empty(symbols.RepairCommand.Arguments);
        Assert.Empty(symbols.RepairCommand.Subcommands);
        Assert.Equal(
            ["--automatic", "--relink", "--dry-run"],
            symbols.RepairCommand.Options.Select(option => option.Name));
        Assert.Equal(typeof(bool), symbols.Automatic.ValueType);
        Assert.Equal(ArgumentArity.Zero, symbols.Automatic.Arity);
        Assert.Equal(typeof(string[]), symbols.Relink.ValueType);
        Assert.Equal(new ArgumentArity(3, 3), symbols.Relink.Arity);
        Assert.True(symbols.Relink.AllowMultipleArgumentsPerToken);
        Assert.Equal(
            "source-location expected-destination target-path",
            symbols.Relink.HelpName);
        Assert.Equal(typeof(bool), symbols.DryRun.ValueType);
        Assert.Equal(ArgumentArity.Zero, symbols.DryRun.Arity);
    }

    [Fact(DisplayName = "Repair binder preserves typed mode, Boolean selection, and relink tuple order")]
    [Trait("Feature", "repair"), Trait("Evidence", "Unit")]
    public void BinderPreservesTypedRequestFacts()
    {
        var symbols = RepairBinding.CreateSymbols();
        string[] arguments =
        [
            "repair",
            "--automatic",
            "--dry-run",
            "--relink",
            ".agents/docs/guide.md@2:8",
            "../old.md#Old",
            ".agents/docs/new.md#New",
        ];
        var parse = Parse(symbols, arguments);

        Assert.Empty(parse.Result.Errors);
        var bound = new RepairRequestBinder(symbols).Bind(
            new CliBindingParse(parse.Result, parse.OriginalArguments),
            RepairTestData.Invocation());

        var request = Assert.IsType<RepairRequest>(bound.Request);
        Assert.Null(bound.InvalidResult);
        Assert.Equal(RepairMode.DryRun, request.Mode);
        Assert.True(request.Automatic);
        Assert.Equal(RepairSelectionMode.AutomaticAndExplicit, request.SelectionMode);
        var relink = Assert.Single(request.Relinks);
        Assert.Equal(".agents/docs/guide.md", relink.SourceCanonicalPath);
        Assert.Equal(2, relink.Line);
        Assert.Equal(8, relink.Column);
        Assert.Equal("../old.md#Old", relink.ExpectedDestination);
        Assert.Equal(".agents/docs/new.md", relink.SelectedTargetPath);
        Assert.Equal("New", relink.SelectedTargetFragment);
    }

    [Theory(DisplayName = "Repair request derives each finite selection mode from authority and interaction")]
    [InlineData(true, false, false, (int)RepairSelectionMode.Automatic, true)]
    [InlineData(false, true, false, (int)RepairSelectionMode.ExplicitRelinks, true)]
    [InlineData(true, true, false, (int)RepairSelectionMode.AutomaticAndExplicit, true)]
    [InlineData(false, false, true, (int)RepairSelectionMode.InteractiveWizard, true)]
    [InlineData(false, false, false, (int)RepairSelectionMode.NonInteractiveBlocked, false)]
    [Trait("Feature", "repair"), Trait("Evidence", "Unit")]
    public void RequestSelectionModesAreExplicit(
        bool automatic,
        bool includeRelink,
        bool allowInteraction,
        int expectedMode,
        bool expectedAuthority)
    {
        var request = new RepairRequest(
            RepairTestData.Workspace(),
            RepairMode.Apply,
            automatic,
            includeRelink ? [RepairTestData.Relink()] : [],
            allowInteraction);

        Assert.Equal((RepairSelectionMode)expectedMode, request.SelectionMode);
        Assert.Equal(expectedAuthority, request.HasSelectionAuthority);
        Assert.Equal(RepairMode.Apply, request.Mode);
    }

    [Fact(DisplayName = "Repair request deduplicates identical relinks and rejects contradictory occurrence intent"), Trait("Feature", "repair"), Trait("Evidence", "Unit")]
    public void RelinkRepetitionIsIdempotentAndContradictionsFailClosed()
    {
        var first = RepairTestData.Relink();
        var duplicate = new RepairRelinkRequest(
            first.SourceLocation,
            first.ExpectedDestination,
            first.Target);
        var request = new RepairRequest(
            RepairTestData.Workspace(),
            RepairMode.DryRun,
            automatic: false,
            relinks: [first, duplicate],
            allowInteraction: false);

        var repeated = Assert.Single(request.Relinks);
        Assert.Equal(first, repeated);

        var exception = Assert.Throws<ArgumentException>(() => new RepairRequest(
            RepairTestData.Workspace(),
            RepairMode.Apply,
            automatic: false,
            relinks:
            [
                RepairTestData.Relink(expectedDestination: "old.md"),
                RepairTestData.Relink(expectedDestination: "other.md"),
            ],
            allowInteraction: false));
        Assert.Equal("relinks", exception.ParamName);
    }

    [Fact(DisplayName = "Repair source locations parse the terminal positive suffix while preserving an earlier at-sign"), Trait("Feature", "repair"), Trait("Evidence", "Unit")]
    public void SourceLocationGrammarIsTerminalAndOneBased()
    {
        var parsed = RepairSourceLocation.Parse(".agents/docs/guide@draft.md@12:8");

        Assert.Equal(".agents/docs/guide@draft.md", parsed.SourceCanonicalPath);
        Assert.Equal(12, parsed.Line);
        Assert.Equal(8, parsed.Column);
        Assert.Equal(".agents/docs/guide@draft.md@12:8", parsed.ToString());
    }

    [Theory(DisplayName = "Repair source locations reject malformed and non-positive coordinates")]
    [InlineData("")]
    [InlineData(".agents/docs/guide.md")]
    [InlineData(".agents/docs/guide.md@1")]
    [InlineData(".agents/docs/guide.md@1:2:3")]
    [InlineData(".agents/docs/guide.md@0:1")]
    [InlineData(".agents/docs/guide.md@1:0")]
    [InlineData(".agents/docs/guide.md@x:1")]
    [Trait("Feature", "repair"), Trait("Evidence", "Unit")]
    public void SourceLocationGrammarRejectsInvalidValues(string value)
    {
        Assert.ThrowsAny<ArgumentException>(() => RepairSourceLocation.Parse(value));
    }

    [Theory(DisplayName = "Repair target grammar preserves contained path and one optional non-empty fragment")]
    [InlineData(".agents/docs/new.md", ".agents/docs/new.md", null)]
    [InlineData(".agents/docs/new.md#New-guide", ".agents/docs/new.md", "New-guide")]
    [InlineData(".agents/docs/hash%23name.md#Heading", ".agents/docs/hash%23name.md", "Heading")]
    [Trait("Feature", "repair"), Trait("Evidence", "Unit")]
    public void TargetGrammarPreservesPathAndFragment(
        string value,
        string expectedPath,
        string? expectedFragment)
    {
        var target = RepairTargetSelection.Parse(value);

        Assert.Equal(expectedPath, target.CanonicalTargetPath);
        Assert.Equal(expectedFragment, target.TargetFragment);
        Assert.Empty(target.CandidateProvenance);
    }

    [Theory(DisplayName = "Repair target grammar rejects queries empty or repeated fragments and non-canonical delimiters")]
    [InlineData(".agents/docs/new.md?")]
    [InlineData(".agents/docs/new.md?query#Heading")]
    [InlineData(".agents/docs/new.md#")]
    [InlineData(".agents/docs/new.md#one#two")]
    [InlineData(".agents/docs/new.md#Heading\n")]
    [InlineData("#Heading")]
    [Trait("Feature", "repair"), Trait("Evidence", "Unit")]
    public void TargetGrammarRejectsInvalidValues(string value)
    {
        Assert.ThrowsAny<ArgumentException>(() => RepairTargetSelection.Parse(value));
    }

    private static CliParseOutcome Parse(RepairSymbols symbols, string[] arguments)
        => CliCommandTree.Create(
            CliHelpContent.Empty,
            [new CliRootBranch(symbols.RepairCommand, CliHelpContent.Empty, [])],
            [])
            .Parse(arguments);
}
