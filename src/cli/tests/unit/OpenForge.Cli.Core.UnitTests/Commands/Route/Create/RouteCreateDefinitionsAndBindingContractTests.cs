using System.CommandLine;
using OpenForge.Cli.Core.Commands.Route;
using OpenForge.Cli.Core.Commands.Route.Create;
using OpenForge.Cli.Core.Commands.Route.Create.Models.Request;
using OpenForge.Cli.Core.Commands.Route.Create.Models.Result;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.Core.UnitTests.Commands.Route.Create;

public sealed class RouteCreateDefinitionsAndBindingContractTests
{
    [Fact(DisplayName = "Route Create definitions expose the accepted grammar"), Trait("Feature", "route-create"), Trait("Evidence", "UnitContract")]
    public void DefinitionsExposeAcceptedGrammar()
    {
        Assert.Equal("route create", RouteCreateDefinitions.CommandIdentity);
        Assert.Equal(1, RouteCreateDefinitions.SchemaVersion);
        Assert.Equal("create", RouteCreateDefinitions.CreateCommand.Name);
        Assert.Equal("file-target", RouteCreateDefinitions.FileTarget.Name);
        Assert.Equal("--description", RouteCreateDefinitions.Description.Name);
        Assert.Equal("--tag", RouteCreateDefinitions.Tag.Name);
        Assert.Equal("--responsibility", RouteCreateDefinitions.Responsibility.Name);
        Assert.Equal("--template", RouteCreateDefinitions.Template.Name);
        Assert.Equal("--dry-run", RouteCreateDefinitions.DryRun.Name);
        Assert.Equal(CliOptionArity.ExactlyOne, RouteCreateDefinitions.Description.Arity);
        Assert.Equal(CliOptionArity.ExactlyOne, RouteCreateDefinitions.Tag.Arity);
        Assert.Equal(CliOptionArity.ExactlyOne, RouteCreateDefinitions.Responsibility.Arity);
        Assert.Equal(CliOptionArity.ExactlyOne, RouteCreateDefinitions.Template.Arity);
        Assert.Equal(CliOptionArity.None, RouteCreateDefinitions.DryRun.Arity);
    }

    [Fact(DisplayName = "Route Create symbols preserve exact arities and repeated tag syntax"), Trait("Feature", "route-create"), Trait("Evidence", "UnitContract")]
    public void SymbolsPreserveExactAritiesAndRepeatedTagSyntax()
    {
        var route = RouteBinding.CreateGroup();
        var symbols = RouteCreateBinding.CreateSymbols(route);

        Assert.Same(symbols.CreateCommand, Assert.Single(route.Subcommands));
        Assert.Empty(symbols.CreateCommand.Aliases);
        Assert.Equal(ArgumentArity.ZeroOrOne, symbols.FileTarget.Arity);
        Assert.Equal(ArgumentArity.ZeroOrOne, symbols.Description.Arity);
        Assert.Equal(ArgumentArity.ZeroOrMore, symbols.Tag.Arity);
        Assert.False(symbols.Tag.AllowMultipleArgumentsPerToken);
        Assert.Equal(ArgumentArity.ZeroOrOne, symbols.Responsibility.Arity);
        Assert.Equal(ArgumentArity.ZeroOrOne, symbols.Template.Arity);
        Assert.Equal(ArgumentArity.Zero, symbols.DryRun.Arity);
        Assert.Equal("--tag", Assert.Single(symbols.DelimiterPolicies).OptionName);
    }

    [Fact(DisplayName = "Route Create binding forms one typed dry-run request"), Trait("Feature", "route-create"), Trait("Evidence", "UnitContract")]
    public void BindingFormsTypedDryRunRequest()
    {
        var route = RouteBinding.CreateGroup();
        var symbols = RouteCreateBinding.CreateSymbols(route);
        var parse = route.Parse(
        [
            "create",
            RouteCreateTestData.TargetId,
            "--description=Project overview",
            "--tag=Docs",
            "--tag=Overview",
            "--responsibility=Explains the project",
            "--template=templates/route",
            "--dry-run",
        ]);

        Assert.Empty(parse.Errors);
        var bound = RouteCreateBinding.Bind(
            parse,
            RouteCreateTestData.Invocation(),
            symbols);
        var request = Assert.IsType<RouteCreateRequest>(bound.Request);

        Assert.Null(bound.InvalidResult);
        Assert.Equal(RouteCreateTestData.TargetId, request.FileTarget);
        Assert.Equal("Project overview", request.Metadata.Description);
        Assert.Equal(["Docs", "Overview"], request.Metadata.Tags);
        Assert.Equal("Explains the project", request.Metadata.Responsibility);
        Assert.Equal("templates/route", request.TemplateReference);
        Assert.Equal(RouteCreateMode.DryRun, request.Mode);
    }

    [Fact(DisplayName = "Route Create binding rejects missing required values and invalid metadata"), Trait("Feature", "route-create"), Trait("Evidence", "UnitContract")]
    public void BindingRejectsMissingRequiredValuesAndInvalidMetadata()
    {
        var invalidCases = new[]
        {
            (Arguments: new[] { "create", "--description=Overview", "--tag=Docs" }, Code: RouteCreateFindingCode.InvalidTarget),
            (Arguments: new[] { "create", RouteCreateTestData.TargetId, "--tag=Docs" }, Code: RouteCreateFindingCode.InvalidMetadata),
            (Arguments: new[] { "create", RouteCreateTestData.TargetId, "--description=Overview" }, Code: RouteCreateFindingCode.InvalidMetadata),
            (Arguments: new[] { "create", RouteCreateTestData.TargetId, "--description=", "--tag=Docs" }, Code: RouteCreateFindingCode.InvalidMetadata),
            (Arguments: new[] { "create", RouteCreateTestData.TargetId, "--description=Overview", "--tag=#Docs" }, Code: RouteCreateFindingCode.InvalidMetadata),
            (Arguments: new[] { "create", RouteCreateTestData.TargetId, "--description=Overview", "--tag=Docs", "--tag=Docs" }, Code: RouteCreateFindingCode.InvalidMetadata),
            (Arguments: new[] { "create", RouteCreateTestData.TargetId, "--description=Overview", "--tag=Docs", "--responsibility=   " }, Code: RouteCreateFindingCode.InvalidMetadata),
            (Arguments: new[] { "create", RouteCreateTestData.TargetId, "--description=Overview", "--tag=Docs", "--template=" }, Code: RouteCreateFindingCode.InvalidTemplate),
        };

        foreach (var invalidCase in invalidCases)
        {
            var route = RouteBinding.CreateGroup();
            var symbols = RouteCreateBinding.CreateSymbols(route);
            var bound = RouteCreateBinding.Bind(
                route.Parse(invalidCase.Arguments),
                RouteCreateTestData.Invocation(),
                symbols);
            var invalid = Assert.IsType<OpenForge.Cli.Core.Commands.Route.Create.Models.Result.RouteCreateResult>(bound.InvalidResult);

            Assert.Null(bound.Request);
            Assert.Equal(CliSemanticStatus.Invalid, invalid.Status);
            Assert.Contains(
                invalid.Findings,
                finding => finding.Code == invalidCase.Code);
        }
    }

    [Fact(DisplayName = "Route Create binding normalizes an explicit empty responsibility to omission"), Trait("Feature", "route-create"), Trait("Evidence", "UnitContract")]
    public void BindingNormalizesExplicitEmptyResponsibilityToOmission()
    {
        var route = RouteBinding.CreateGroup();
        var symbols = RouteCreateBinding.CreateSymbols(route);
        var parse = route.Parse(
        [
            "create",
            RouteCreateTestData.TargetId,
            "--description=Project overview",
            "--tag=Docs",
            "--responsibility",
            string.Empty,
        ]);
        var bound = RouteCreateBinding.Bind(
            parse,
            RouteCreateTestData.Invocation(),
            symbols);
        var request = Assert.IsType<RouteCreateRequest>(bound.Request);

        Assert.Null(bound.InvalidResult);
        Assert.Null(request.Metadata.Responsibility);
        Assert.Equal(RouteCreateMode.Apply, request.Mode);
    }

    [Fact(DisplayName = "Route Create binding rejects responsibility without an explicit value"), Trait("Feature", "route-create"), Trait("Evidence", "UnitContract")]
    public void BindingRejectsResponsibilityWithoutExplicitValue()
    {
        var route = RouteBinding.CreateGroup();
        var symbols = RouteCreateBinding.CreateSymbols(route);
        var parse = route.Parse(
        [
            "create",
            RouteCreateTestData.TargetId,
            "--description=Project overview",
            "--tag=Docs",
            "--responsibility",
        ]);

        var bound = RouteCreateBinding.Bind(
            parse,
            RouteCreateTestData.Invocation(),
            symbols);

        Assert.Null(bound.Request);
        Assert.Equal(CliSemanticStatus.Invalid, bound.InvalidResult?.Status);
    }

    [Fact(DisplayName = "Route Create singleton options reject repeated values"), Trait("Feature", "route-create"), Trait("Evidence", "UnitContract")]
    public void SingletonOptionsRejectRepeatedValues()
    {
        var route = RouteBinding.CreateGroup();
        var symbols = RouteCreateBinding.CreateSymbols(route);
        var cases = new[]
        {
            new[] { "--description=One", "--description=One" },
            new[] { "--responsibility=One", "--responsibility=One" },
            new[] { "--template=templates/one", "--template=templates/one" },
        };

        foreach (var repeated in cases)
        {
            var arguments = new[]
            {
                "create",
                RouteCreateTestData.TargetId,
                "--description=Project overview",
                "--tag=Docs",
            }.Concat(repeated).ToArray();
            var parse = route.Parse(arguments);
            var bound = RouteCreateBinding.Bind(
                parse,
                RouteCreateTestData.Invocation(),
                symbols);

            Assert.Null(bound.Request);
            Assert.Equal(CliSemanticStatus.Invalid, bound.InvalidResult?.Status);
        }
    }
}
