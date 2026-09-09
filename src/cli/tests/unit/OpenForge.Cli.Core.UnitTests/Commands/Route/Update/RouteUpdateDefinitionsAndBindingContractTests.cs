using System.CommandLine;
using OpenForge.Cli.Core.Commands.Route;
using OpenForge.Cli.Core.Commands.Route.Update;
using OpenForge.Cli.Core.Commands.Route.Update.Models.Request;
using OpenForge.Cli.Core.Commands.Route.Update.Models.Result;
using OpenForge.Cli.Core.Shell.Composition;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.Core.UnitTests.Commands.Route.Update;

public sealed class RouteUpdateDefinitionsAndBindingContractTests
{
    [Fact(DisplayName = "Route Update definitions expose only the accepted grammar"), Trait("Feature", "route-update"), Trait("Evidence", "UnitContract")]
    public void DefinitionsExposeAcceptedGrammar()
    {
        Assert.Equal("route update", RouteUpdateDefinitions.CommandIdentity);
        Assert.Equal(1, RouteUpdateDefinitions.SchemaVersion);
        Assert.Equal("update", RouteUpdateDefinitions.UpdateCommand.Name);
        Assert.Equal("source-reference", RouteUpdateDefinitions.SourceReference.Name);
        Assert.Equal("--description", RouteUpdateDefinitions.Description.Name);
        Assert.Equal("--tag", RouteUpdateDefinitions.Tag.Name);
        Assert.Equal("--responsibility", RouteUpdateDefinitions.Responsibility.Name);
        Assert.Equal("--template", RouteUpdateDefinitions.Template.Name);
        Assert.Equal("--dry-run", RouteUpdateDefinitions.DryRun.Name);
        Assert.Equal(CliOptionArity.ExactlyOne, RouteUpdateDefinitions.Description.Arity);
        Assert.Equal(CliOptionArity.ExactlyOne, RouteUpdateDefinitions.Tag.Arity);
        Assert.Equal(CliOptionArity.ExactlyOne, RouteUpdateDefinitions.Responsibility.Arity);
        Assert.Equal(CliOptionArity.ExactlyOne, RouteUpdateDefinitions.Template.Arity);
        Assert.Equal(CliOptionArity.None, RouteUpdateDefinitions.DryRun.Arity);
    }

    [Fact(DisplayName = "Route Update symbols preserve singleton, tag-list, and delimiter policy"), Trait("Feature", "route-update"), Trait("Evidence", "UnitContract")]
    public void SymbolsPreserveSingletonTagListAndDelimiterPolicy()
    {
        var route = RouteBinding.CreateGroup();
        var symbols = RouteUpdateBinding.CreateSymbols(route);

        Assert.Same(symbols.UpdateCommand, Assert.Single(route.Subcommands));
        Assert.Empty(symbols.UpdateCommand.Aliases);
        Assert.Equal(ArgumentArity.ZeroOrOne, symbols.SourceReference.Arity);
        Assert.Equal(ArgumentArity.ZeroOrOne, symbols.Description.Arity);
        Assert.Equal(ArgumentArity.ZeroOrMore, symbols.Tag.Arity);
        Assert.False(symbols.Tag.AllowMultipleArgumentsPerToken);
        Assert.Equal(ArgumentArity.ZeroOrOne, symbols.Responsibility.Arity);
        Assert.Equal(ArgumentArity.ZeroOrOne, symbols.Template.Arity);
        Assert.Equal(ArgumentArity.Zero, symbols.DryRun.Arity);
        var delimiter = Assert.Single(symbols.DelimiterPolicies);
        Assert.Equal("--tag", delimiter.OptionName);
        Assert.Equal(OpenForge.Cli.Core.Shell.Parsing.CliDelimiterShape.Equals, delimiter.RequiredShape);
    }

    [Fact(DisplayName = "Route Update binding forms one complete typed dry-run request"), Trait("Feature", "route-update"), Trait("Evidence", "UnitContract")]
    public void BindingFormsCompleteTypedDryRunRequest()
    {
        var route = RouteBinding.CreateGroup();
        var symbols = RouteUpdateBinding.CreateSymbols(route);
        string[] arguments =
        [
            "update",
            RouteUpdateTestData.TargetId,
            "--description=After",
            "--tag=Memory",
            "--tag=Decision",
            "--responsibility=Owns the revised decision",
            "--template=templates/topic",
            "--dry-run",
            "--dry-run",
        ];
        var parse = route.Parse(arguments);

        Assert.Empty(parse.Errors);
        var bound = new RouteUpdateBinding(symbols).BindRequest(
            parse,
            arguments,
            RouteUpdateTestData.Invocation());
        var request = Assert.IsType<RouteUpdateRequest>(bound.Request);

        Assert.Null(bound.InvalidResult);
        Assert.Equal(RouteUpdateTestData.TargetId, request.SourceReference);
        Assert.Equal("After", request.Patch.Description.Value);
        Assert.Equal(["Memory", "Decision"], request.Patch.Tags.Values);
        Assert.Equal(RouteUpdateResponsibilityOperation.Set, request.Patch.Responsibility.Operation);
        Assert.Equal("Owns the revised decision", request.Patch.Responsibility.Value);
        Assert.Equal("templates/topic", request.TemplateReference);
        Assert.Equal(RouteUpdateMode.DryRun, request.Mode);
    }

    [Theory(DisplayName = "Route Update exact attached-empty responsibility forms removal"),
     InlineData("--responsibility="),
     InlineData("--responsibility:"),
     Trait("Feature", "route-update"), Trait("Evidence", "UnitContract")]
    public void ExactEmptyResponsibilityFormsRemoval(string responsibility)
    {
        var bound = Bind(
            "update",
            RouteUpdateTestData.TargetId,
            responsibility);
        var request = Assert.IsType<RouteUpdateRequest>(bound.Request);

        Assert.Null(bound.InvalidResult);
        Assert.Equal(RouteUpdateResponsibilityOperation.Remove, request.Patch.Responsibility.Operation);
        Assert.Null(request.Patch.Responsibility.Value);
    }

    [Fact(DisplayName = "Route Update binding rejects missing operations and invalid patch values"), Trait("Feature", "route-update"), Trait("Evidence", "UnitContract")]
    public void BindingRejectsMissingOperationsAndInvalidPatchValues()
    {
        var cases = new[]
        {
            (Arguments: new[] { "update", "--description=After" }, Code: RouteUpdateFindingCode.InvalidTarget),
            (Arguments: ["update", RouteUpdateTestData.TargetId], Code: RouteUpdateFindingCode.InvalidInput),
            (Arguments: ["update", RouteUpdateTestData.TargetId, "--description="], Code: RouteUpdateFindingCode.InvalidPatch),
            (Arguments: ["update", RouteUpdateTestData.TargetId, "--description=   "], Code: RouteUpdateFindingCode.InvalidPatch),
            (Arguments: ["update", RouteUpdateTestData.TargetId, "--tag="], Code: RouteUpdateFindingCode.InvalidPatch),
            (Arguments: ["update", RouteUpdateTestData.TargetId, "--tag=#Memory"], Code: RouteUpdateFindingCode.InvalidPatch),
            (Arguments: ["update", RouteUpdateTestData.TargetId, "--tag=Memory", "--tag=Memory"], Code: RouteUpdateFindingCode.InvalidPatch),
            (Arguments: ["update", RouteUpdateTestData.TargetId, "--responsibility"], Code: RouteUpdateFindingCode.InvalidPatch),
            (Arguments: ["update", RouteUpdateTestData.TargetId, "--", "--responsibility="], Code: RouteUpdateFindingCode.InvalidInput),
            (Arguments: ["update", RouteUpdateTestData.TargetId, "--responsibility=   "], Code: RouteUpdateFindingCode.InvalidPatch),
            (Arguments: ["update", RouteUpdateTestData.TargetId, "--template="], Code: RouteUpdateFindingCode.InvalidTemplate),
        };

        foreach (var (Arguments, Code) in cases)
        {
            var bound = Bind(Arguments);
            var invalid = Assert.IsType<RouteUpdateResult>(bound.InvalidResult);

            Assert.Null(bound.Request);
            Assert.Equal(CliSemanticStatus.Invalid, invalid.Status);
            Assert.Contains(invalid.Findings, finding => finding.Code == Code);
        }
    }

    [Fact(DisplayName = "Route Update bare responsibility preserves the exact invalid-patch cause"),
     Trait("Feature", "route-update"), Trait("Evidence", "UnitContract")]
    public void BareResponsibilityRetainsExactInvalidCause()
    {
        var bound = Bind("update", RouteUpdateTestData.TargetId, "--responsibility");
        var result = Assert.IsType<RouteUpdateResult>(bound.InvalidResult);

        Assert.Null(bound.Request);
        Assert.Equal(CliSemanticStatus.Invalid, result.Status);
        var finding = Assert.Single(result.Findings);
        Assert.Equal(RouteUpdateFindingCode.InvalidPatch, finding.Code);
        Assert.Equal("--responsibility accepts exactly one value.", finding.Cause);
    }

    [Fact(DisplayName = "Route Update singleton options reject every repeated value"), Trait("Feature", "route-update"), Trait("Evidence", "UnitContract")]
    public void SingletonOptionsRejectEveryRepeatedValue()
    {
        var cases = new[]
        {
            new[] { "--description=One", "--description=One" },
            ["--description=One", "--description=Two"],
            ["--responsibility=One", "--responsibility=One"],
            ["--responsibility=One", "--responsibility=Two"],
            ["--template=templates/topic", "--template=templates/topic"],
            ["--template=templates/topic", "--template=templates/other"],
        };

        foreach (var repeated in cases)
        {
            var bound = Bind(
                ["update", RouteUpdateTestData.TargetId, .. repeated]);

            Assert.Null(bound.Request);
            Assert.Equal(CliSemanticStatus.Invalid, bound.InvalidResult?.Status);
        }
    }

    [Fact(DisplayName = "Route Update requires exactly one source operand"), Trait("Feature", "route-update"), Trait("Evidence", "UnitContract")]
    public void RequiresExactlyOneSourceOperand()
    {
        var missing = Bind("update", "--description=After");
        var extra = Bind(
            "update",
            RouteUpdateTestData.TargetId,
            "memory/other",
            "--description=After");

        Assert.Null(missing.Request);
        Assert.Equal(CliSemanticStatus.Invalid, missing.InvalidResult?.Status);
        Assert.Null(extra.Request);
        Assert.Equal(CliSemanticStatus.Invalid, extra.InvalidResult?.Status);
        Assert.Contains(
            extra.InvalidResult?.Findings ?? [],
            finding => finding.Code == RouteUpdateFindingCode.InvalidInput);
    }

    private static CliBindResult<RouteUpdateRequest, RouteUpdateResult> Bind(
        params string[] arguments)
    {
        var route = RouteBinding.CreateGroup();
        var symbols = RouteUpdateBinding.CreateSymbols(route);
        return new RouteUpdateBinding(symbols).BindRequest(
            route.Parse(arguments),
            arguments,
            RouteUpdateTestData.Invocation());
    }
}
