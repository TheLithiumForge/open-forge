using System.CommandLine;
using OpenForge.Cli.Core.Commands.Route;
using OpenForge.Cli.Core.Commands.Route.Create;
using OpenForge.Cli.Core.Commands.Route.Create.Models.Request;
using OpenForge.Cli.Core.Commands.Route.Create.Models.Result;
using OpenForge.Cli.Core.Commands.Route.Create.Shared.Binding;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.Core.UnitTests.Commands.Route.Create;

public sealed class RouteCreateDefinitionsAndBindingContractTests
{
    [Theory(DisplayName = "Route Create reports the specific apply-to failure"), Trait("Boundary", "Input"), Trait("Feature", "route-create"), Trait("Evidence", "UnitContract")]
    [InlineData("docs/", "--apply-to <glob> must contain a non-empty pattern with no empty path segments. Use docs/** to match files under docs/.")]
    [InlineData("", "--apply-to <glob> must contain a non-empty pattern with no empty path segments. Use docs/** to match files under docs/.")]
    [InlineData("/docs/**", "--apply-to <glob> must be workspace-relative.")]
    [InlineData("../docs/**", "--apply-to <glob> must not contain . or .. path segments.")]
    [InlineData("docs\\file.cs", "--apply-to <glob> contains unsupported pattern syntax.")]
    public void InvalidApplyToPatternReportsSpecificCause(string pattern, string expectedCause)
    {
        var route = RouteBinding.CreateGroup();
        var symbols = RouteCreateBinding.CreateSymbols(route);
        var parse = route.Parse(["create", RouteCreateTestData.TargetId, "--apply-to", pattern]);
        var bound = RouteCreateRequestBinder.Bind(parse, RouteCreateTestData.Invocation(), symbols);

        Assert.Null(bound.Request);
        var result = Assert.IsType<RouteCreateResult>(bound.InvalidResult);
        Assert.Equal(CliSemanticStatus.Invalid, result.Status);
        var finding = Assert.Single(result.Findings);
        Assert.Equal(RouteCreateFindingCode.InvalidMetadata, finding.Code);
        Assert.Equal(expectedCause, finding.Cause);
    }

    [Fact(DisplayName = "Route Create appends one hint after joined metadata problems"), Trait("Boundary", "Input"), Trait("Feature", "route-create"), Trait("Evidence", "UnitContract")]
    public void ApplyToHintFollowsJoinedMetadataProblemsOnce()
    {
        var route = RouteBinding.CreateGroup();
        var symbols = RouteCreateBinding.CreateSymbols(route);
        var parse = route.Parse(
        [
            "create", RouteCreateTestData.TargetId,
            "--description=", "--apply-to=docs/", "--apply-to=src/", "--responsibility",
        ]);
        var bound = RouteCreateRequestBinder.Bind(parse, RouteCreateTestData.Invocation(), symbols);

        Assert.Null(bound.Request);
        var result = Assert.IsType<RouteCreateResult>(bound.InvalidResult);
        var finding = Assert.Single(result.Findings);
        Assert.Equal(RouteCreateFindingCode.InvalidMetadata, finding.Code);
        Assert.Equal(
            "--description is missing and --apply-to <glob> must contain a non-empty pattern with no empty path segments and --responsibility accepts exactly one value. Use docs/** to match files under docs/.",
            finding.Cause);
    }

    [Trait("Boundary", "Input")]
    [Fact(DisplayName = "Route Create definitions expose the accepted grammar"), Trait("Feature", "route-create"), Trait("Evidence", "UnitContract")]
    public void DefinitionsExposeAcceptedGrammar()
    {
        Assert.Equal("route create", RouteCreateDefinitions.CommandIdentity);
        Assert.Equal(1, RouteCreateDefinitions.SchemaVersion);
        Assert.Equal("create", RouteCreateDefinitions.CreateCommand.Name);
        Assert.Equal("file-target", RouteCreateDefinitions.FileTarget.Name);
        Assert.Equal("--description", RouteCreateDefinitions.Description.Name);
        Assert.Equal("--tag", RouteCreateDefinitions.Tag.Name);
        Assert.Equal("--apply-to", RouteCreateDefinitions.ApplyTo.Name);
        Assert.Equal("--responsibility", RouteCreateDefinitions.Responsibility.Name);
        Assert.Equal("--template", RouteCreateDefinitions.Template.Name);
        Assert.Equal("--dry-run", RouteCreateDefinitions.DryRun.Name);
        Assert.Equal(CliOptionArity.ExactlyOne, RouteCreateDefinitions.Description.Arity);
        Assert.Equal(CliOptionArity.ExactlyOne, RouteCreateDefinitions.Tag.Arity);
        Assert.Equal(CliOptionArity.ExactlyOne, RouteCreateDefinitions.ApplyTo.Arity);
        Assert.Equal(CliOptionArity.ExactlyOne, RouteCreateDefinitions.Responsibility.Arity);
        Assert.Equal(CliOptionArity.ExactlyOne, RouteCreateDefinitions.Template.Arity);
        Assert.Equal(CliOptionArity.None, RouteCreateDefinitions.DryRun.Arity);
    }

    [Trait("Boundary", "Input")]
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
        Assert.Equal(ArgumentArity.ZeroOrMore, symbols.ApplyTo.Arity);
        Assert.False(symbols.ApplyTo.AllowMultipleArgumentsPerToken);
        Assert.Equal(ArgumentArity.ZeroOrOne, symbols.Responsibility.Arity);
        Assert.Equal(ArgumentArity.ZeroOrOne, symbols.Template.Arity);
        Assert.Equal(ArgumentArity.Zero, symbols.DryRun.Arity);
    }

    [Trait("Boundary", "Input")]
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
            "--apply-to=**/*.cs",
            "--apply-to=docs/**",
            "--responsibility=Explains the project",
            "--template=templates/route",
            "--dry-run",
        ]);

        Assert.Empty(parse.Errors);
        var bound = RouteCreateRequestBinder.Bind(
            parse,
            RouteCreateTestData.Invocation(),
            symbols);
        var request = Assert.IsType<RouteCreateRequest>(bound.Request);

        Assert.Null(bound.InvalidResult);
        Assert.Equal(RouteCreateTestData.TargetId, request.FileTarget);
        Assert.Equal("Project overview", request.Metadata.Description);
        Assert.Equal(["Docs", "Overview"], request.Metadata.Tags);
        Assert.Equal(["**/*.cs", "docs/**"], request.Metadata.ApplyTo.Select(pattern => pattern.Text));
        Assert.Equal("Explains the project", request.Metadata.Responsibility);
        Assert.Equal("templates/route", request.TemplateReference);
        Assert.Equal(RouteCreateMode.DryRun, request.Mode);
    }

    [Trait("Boundary", "Input")]
    [Fact(DisplayName = "Route Create binding accepts omitted optional metadata and rejects invalid metadata"), Trait("Feature", "route-create"), Trait("Evidence", "UnitContract")]
    public void BindingAcceptsOmittedOptionalMetadataAndRejectsInvalidMetadata()
    {
        (string[] Arguments, string? Description, string[] Tags)[] partialCases =
        [
            (["create", RouteCreateTestData.TargetId], null, []),
            (["create", RouteCreateTestData.TargetId, "--description=Overview"], "Overview", []),
            (["create", RouteCreateTestData.TargetId, "--tag=Docs"], null, ["Docs"]),
        ];

        foreach (var (arguments, expectedDescription, expectedTags) in partialCases)
        {
            var route = RouteBinding.CreateGroup();
            var symbols = RouteCreateBinding.CreateSymbols(route);
            var bound = RouteCreateRequestBinder.Bind(
                route.Parse(arguments),
                RouteCreateTestData.Invocation(),
                symbols);
            var request = Assert.IsType<RouteCreateRequest>(bound.Request);

            Assert.Null(bound.InvalidResult);
            Assert.Equal(expectedDescription, request.Metadata.Description);
            Assert.Equal(expectedTags, request.Metadata.Tags);
            Assert.Empty(request.Metadata.ApplyTo);
        }

        (string[] Arguments, RouteCreateFindingCode Code)[] invalidCases =
        [
            (Arguments: ["create", "--description=Overview", "--tag=Docs"], Code: RouteCreateFindingCode.InvalidTarget),
            (Arguments: ["create", RouteCreateTestData.TargetId, "--description=", "--tag=Docs"], Code: RouteCreateFindingCode.InvalidMetadata),
            (Arguments: ["create", RouteCreateTestData.TargetId, "--description", "--tag=Docs"], Code: RouteCreateFindingCode.InvalidMetadata),
            (Arguments: ["create", RouteCreateTestData.TargetId, "--description=Overview", "--tag"], Code: RouteCreateFindingCode.InvalidMetadata),
            (Arguments: ["create", RouteCreateTestData.TargetId, "--description=Overview", "--tag=#Docs"], Code: RouteCreateFindingCode.InvalidMetadata),
            (Arguments: ["create", RouteCreateTestData.TargetId, "--description=Overview", "--tag=Docs", "--tag=Docs"], Code: RouteCreateFindingCode.InvalidMetadata),
            (Arguments: ["create", RouteCreateTestData.TargetId, "--apply-to=../outside.cs"], Code: RouteCreateFindingCode.InvalidMetadata),
            (Arguments: ["create", RouteCreateTestData.TargetId, "--apply-to"], Code: RouteCreateFindingCode.InvalidMetadata),
            (Arguments: ["create", RouteCreateTestData.TargetId, "--description=Overview", "--tag=Docs", "--responsibility=   "], Code: RouteCreateFindingCode.InvalidMetadata),
            (Arguments: ["create", RouteCreateTestData.TargetId, "--description=Overview", "--tag=Docs", "--template="], Code: RouteCreateFindingCode.InvalidTemplate),
        ];

        foreach (var (arguments, expectedCode) in invalidCases)
        {
            var route = RouteBinding.CreateGroup();
            var symbols = RouteCreateBinding.CreateSymbols(route);
            var bound = RouteCreateRequestBinder.Bind(
                route.Parse(arguments),
                RouteCreateTestData.Invocation(),
                symbols);
            var invalid = Assert.IsType<OpenForge.Cli.Core.Commands.Route.Create.Models.Result.RouteCreateResult>(bound.InvalidResult);

            Assert.Null(bound.Request);
            Assert.Equal(CliSemanticStatus.Invalid, invalid.Status);
            Assert.Contains(
                invalid.Findings,
                finding => finding.Code == expectedCode);
        }
    }

    [Trait("Boundary", "Input")]
    [Fact(DisplayName = "Route Create binding normalizes repeated equivalent apply-to patterns"), Trait("Feature", "route-create"), Trait("Evidence", "UnitContract")]
    public void BindingNormalizesRepeatedEquivalentApplyToPatterns()
    {
        var route = RouteBinding.CreateGroup();
        var symbols = RouteCreateBinding.CreateSymbols(route);
        var parse = route.Parse(
        [
            "create",
            RouteCreateTestData.TargetId,
            "--apply-to=docs/**",
            "--apply-to=**/*.cs",
            "--apply-to=**/*.cs",
        ]);

        var bound = RouteCreateRequestBinder.Bind(parse, RouteCreateTestData.Invocation(), symbols);
        var request = Assert.IsType<RouteCreateRequest>(bound.Request);

        Assert.Null(bound.InvalidResult);
        Assert.Equal(["**/*.cs", "docs/**"], request.Metadata.ApplyTo.Select(pattern => pattern.Text));
    }

    [Trait("Boundary", "Input")]
    [Fact(DisplayName = "Route Create binding expands expressions into sorted distinct atomic apply-to patterns"), Trait("Feature", "route-create"), Trait("Evidence", "UnitContract")]
    public void BindingExpandsApplyToExpressionsIntoAtomicPatterns()
    {
        var route = RouteBinding.CreateGroup();
        var symbols = RouteCreateBinding.CreateSymbols(route);
        var parse = route.Parse(
        [
            "create",
            RouteCreateTestData.TargetId,
            "--apply-to=src/a.cs,src/b.cs",
            "--apply-to=**/*.{ts,tsx}",
            "--apply-to=src/a\\,b.cs",
        ]);

        var bound = RouteCreateRequestBinder.Bind(parse, RouteCreateTestData.Invocation(), symbols);
        var request = Assert.IsType<RouteCreateRequest>(bound.Request);

        Assert.Null(bound.InvalidResult);
        Assert.Equal(
            ["**/*.{ts,tsx}", "src/a,b.cs", "src/a.cs", "src/b.cs"],
            request.Metadata.ApplyTo.Select(pattern => pattern.Text));
    }

    [Trait("Boundary", "Input")]
    [Fact(DisplayName = "Route Create binding accepts embedded double-star syntax"), Trait("Feature", "route-create"), Trait("Evidence", "UnitContract")]
    public void BindingAcceptsEmbeddedDoubleStarSyntax()
    {
        var route = RouteBinding.CreateGroup();
        var symbols = RouteCreateBinding.CreateSymbols(route);
        var parse = route.Parse(
        [
            "create",
            RouteCreateTestData.TargetId,
            "--apply-to=src/**Order.cs",
        ]);

        var bound = RouteCreateRequestBinder.Bind(parse, RouteCreateTestData.Invocation(), symbols);
        var request = Assert.IsType<RouteCreateRequest>(bound.Request);

        Assert.Null(bound.InvalidResult);
        Assert.Equal(["src/**Order.cs"], request.Metadata.ApplyTo.Select(pattern => pattern.Text));
    }

    [Trait("Boundary", "Input")]
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
        var bound = RouteCreateRequestBinder.Bind(
            parse,
            RouteCreateTestData.Invocation(),
            symbols);
        var request = Assert.IsType<RouteCreateRequest>(bound.Request);

        Assert.Null(bound.InvalidResult);
        Assert.Null(request.Metadata.Responsibility);
        Assert.Equal(RouteCreateMode.Apply, request.Mode);
    }

    [Trait("Boundary", "Input")]
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

        var bound = RouteCreateRequestBinder.Bind(
            parse,
            RouteCreateTestData.Invocation(),
            symbols);

        Assert.Null(bound.Request);
        Assert.Equal(CliSemanticStatus.Invalid, bound.InvalidResult?.Status);
    }

    [Trait("Boundary", "Input")]
    [Fact(DisplayName = "Route Create singleton options reject repeated values"), Trait("Feature", "route-create"), Trait("Evidence", "UnitContract")]
    public void SingletonOptionsRejectRepeatedValues()
    {
        var route = RouteBinding.CreateGroup();
        var symbols = RouteCreateBinding.CreateSymbols(route);
        string[][] cases =
        [
            ["--description=One", "--description=One"],
            ["--responsibility=One", "--responsibility=One"],
            ["--template=templates/one", "--template=templates/one"],
        ];

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
            var bound = RouteCreateRequestBinder.Bind(
                parse,
                RouteCreateTestData.Invocation(),
                symbols);

            Assert.Null(bound.Request);
            Assert.Equal(CliSemanticStatus.Invalid, bound.InvalidResult?.Status);
        }
    }
}
