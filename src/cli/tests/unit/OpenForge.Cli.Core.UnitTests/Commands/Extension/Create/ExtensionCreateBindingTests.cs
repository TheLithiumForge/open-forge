using System.CommandLine;
using OpenForge.Cli.Core.Commands.Extension;
using OpenForge.Cli.Core.Commands.Extension.Create;
using OpenForge.Cli.Core.Commands.Extension.Create.Models.Request;
using OpenForge.Cli.Core.Commands.Extension.Create.Models.Result;
using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.Core.Shell.Composition.Models;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Invocation.Models;
using OpenForge.Cli.Core.Shell.Presentation.Models;

namespace OpenForge.Cli.Core.UnitTests.Commands.Extension.Create;

public sealed class ExtensionCreateBindingTests
{
    [Fact(DisplayName = "Extension Create symbols expose one stable ID, singleton metadata, repeated dependencies, and idempotent flags"), Trait("Feature", "extension-create"), Trait("Evidence", "Unit")]
    public void SymbolsExposeExactGrammar()
    {
        var group = ExtensionBinding.CreateGroup();
        var symbols = ExtensionCreateBinding.CreateSymbols(group);

        Assert.Equal("create", symbols.CreateCommand.Name);
        Assert.Empty(symbols.CreateCommand.Aliases);
        Assert.Empty(symbols.CreateCommand.Subcommands);
        Assert.Single(symbols.CreateCommand.Arguments);
        Assert.Equal("stable-id", symbols.StableId.Name);
        Assert.Equal(ArgumentArity.ZeroOrOne, symbols.StableId.Arity);
        Assert.Equal(typeof(string), symbols.StableId.ValueType);
        Assert.Equal(
            ["--path", "--name", "--description", "--package-version", "--dependency", "--automatic", "--dry-run"],
            symbols.CreateCommand.Options.Select(option => option.Name));
        Assert.Equal(ArgumentArity.ZeroOrOne, symbols.Path.Arity);
        Assert.Equal(ArgumentArity.ZeroOrOne, symbols.Name.Arity);
        Assert.Equal(ArgumentArity.ZeroOrOne, symbols.Description.Arity);
        Assert.Equal(ArgumentArity.ZeroOrOne, symbols.PackageVersion.Arity);
        Assert.Equal(ArgumentArity.ZeroOrMore, symbols.Dependency.Arity);
        Assert.False(symbols.Dependency.AllowMultipleArgumentsPerToken);
        Assert.Equal(ArgumentArity.Zero, symbols.Automatic.Arity);
        Assert.Equal(ArgumentArity.Zero, symbols.DryRun.Arity);
    }

    [Theory(DisplayName = "Extension Create parser accepts every native value delimiter for singleton and repeated options"), Trait("Feature", "extension-create"), Trait("Evidence", "Unit")]
    [InlineData("separate")]
    [InlineData("equals")]
    [InlineData("colon")]
    public void SymbolsAcceptNativeValueForms(string form)
    {
        var symbols = ExtensionCreateBinding.CreateSymbols(ExtensionBinding.CreateGroup());
        string[] arguments = form switch
        {
            "separate" => ["development-toolkit", "--path", "/catalogue", "--name", "Toolkit", "--dependency", "alpha", "--dependency", "beta"],
            "equals" => ["development-toolkit", "--path=/catalogue", "--name=Toolkit", "--dependency=alpha", "--dependency=beta"],
            "colon" => ["development-toolkit", "--path:/catalogue", "--name:Toolkit", "--dependency:alpha", "--dependency:beta"],
            _ => throw new ArgumentOutOfRangeException(nameof(form), form, "The native option form is not defined."),
        };

        var parse = symbols.CreateCommand.Parse(arguments);

        Assert.Empty(parse.Errors);
        Assert.Equal("development-toolkit", parse.GetValue(symbols.StableId));
        Assert.Equal("/catalogue", parse.GetValue(symbols.Path));
        Assert.Equal("Toolkit", parse.GetValue(symbols.Name));
        Assert.Equal(["alpha", "beta"], parse.GetValue(symbols.Dependency) ?? []);
    }

    [Fact(DisplayName = "Extension Create binding preserves a complete immutable raw request and derives human interaction policy"), Trait("Feature", "extension-create"), Trait("Evidence", "Unit")]
    public void BindingPreservesCompleteRequestAndInteractionPolicy()
    {
        var symbols = ExtensionCreateBinding.CreateSymbols(ExtensionBinding.CreateGroup());
        string[] arguments =
        [
            "development-toolkit",
            "--path", "/catalogue",
            "--name", "Development Toolkit",
            "--description", "Adds workflows",
            "--package-version", "0.2-preview",
            "--dependency", "zeta",
            "--dependency", "alpha",
            "--dry-run",
        ];
        var parse = symbols.CreateCommand.Parse(arguments);

        var bound = new ExtensionCreateRequestBinder(symbols).Bind(
            new CliBindingParse(parse, arguments),
            Invocation(CliOutputFormat.Human, CliVerbosity.Normal));

        var request = Assert.IsType<ExtensionCreateRequest>(bound.Request);
        Assert.Null(bound.InvalidResult);
        Assert.Equal("development-toolkit", request.StableId);
        Assert.Equal("/catalogue", request.CataloguePath);
        Assert.Equal("Development Toolkit", request.Name);
        Assert.Equal("Adds workflows", request.Description);
        Assert.Equal("0.2-preview", request.PackageVersion);
        Assert.Equal(["zeta", "alpha"], request.Dependencies);
        Assert.True(request.AllowInteraction);
        Assert.Equal(ExtensionCreateMode.DryRun, request.Mode);
    }

    [Theory(DisplayName = "Extension Create binding disables prompting for JSON and automatic requests while retaining explicit input"), Trait("Feature", "extension-create"), Trait("Evidence", "Unit")]
    [InlineData("json", false)]
    [InlineData("automatic", false)]
    [InlineData("human", true)]
    public void BindingSelectsPromptPolicyFromInvocation(string presentation, bool expectedInteraction)
    {
        var symbols = ExtensionCreateBinding.CreateSymbols(ExtensionBinding.CreateGroup());
        string[] arguments = presentation == "automatic"
            ? ["development-toolkit", "--path", "/catalogue", "--automatic"]
            : ["development-toolkit", "--path", "/catalogue"];
        var parse = symbols.CreateCommand.Parse(arguments);
        var format = presentation == "json" ? CliOutputFormat.Json : CliOutputFormat.Human;

        var bound = new ExtensionCreateRequestBinder(symbols).Bind(
            new CliBindingParse(parse, arguments),
            Invocation(format, CliVerbosity.Normal));

        var request = Assert.IsType<ExtensionCreateRequest>(bound.Request);
        Assert.Equal(expectedInteraction, request.AllowInteraction);
        Assert.Equal("development-toolkit", request.StableId);
        Assert.Equal("/catalogue", request.CataloguePath);
    }

    [Theory(DisplayName = "Extension Create binding rejects repeated singleton values, repeated operands, and unsupported symbols"), Trait("Feature", "extension-create"), Trait("Evidence", "Unit")]
    [InlineData("repeated-id")]
    [InlineData("repeated-path")]
    [InlineData("repeated-name")]
    [InlineData("repeated-description")]
    [InlineData("repeated-package-version")]
    [InlineData("unknown-option")]
    [InlineData("unsupported-operand")]
    [InlineData("invalid-id-uppercase")]
    [InlineData("invalid-id-leading-hyphen")]
    [InlineData("invalid-id-trailing-hyphen")]
    [InlineData("invalid-id-underscore")]
    [InlineData("invalid-id-unicode")]
    public void BindingFormsInvalidResultForInvalidGrammar(string scenario)
    {
        var symbols = ExtensionCreateBinding.CreateSymbols(ExtensionBinding.CreateGroup());
        var arguments = InvalidArguments(scenario);
        var parse = symbols.CreateCommand.Parse(arguments);

        var bound = new ExtensionCreateRequestBinder(symbols).Bind(
            new CliBindingParse(parse, arguments),
            Invocation(CliOutputFormat.Human, CliVerbosity.Normal));

        var result = Assert.IsType<ExtensionCreateResult>(bound.InvalidResult);
        Assert.Null(bound.Request);
        Assert.Equal(CliSemanticStatus.Invalid, result.Status);
        Assert.Contains(result.Findings, finding => finding.Code == ExtensionCreateFindingCode.InvalidInput);
    }

    [Fact(DisplayName = "Extension Create binding accepts repeated automatic and dry-run flags as one idempotent mode"), Trait("Feature", "extension-create"), Trait("Evidence", "Unit")]
    public void BindingCollapsesIdempotentFlags()
    {
        var symbols = ExtensionCreateBinding.CreateSymbols(ExtensionBinding.CreateGroup());
        string[] arguments =
        [
            "development-toolkit", "--path", "/catalogue",
            "--automatic", "--automatic", "--dry-run", "--dry-run",
        ];
        var parse = symbols.CreateCommand.Parse(arguments);

        var bound = new ExtensionCreateRequestBinder(symbols).Bind(
            new CliBindingParse(parse, arguments),
            Invocation(CliOutputFormat.Human, CliVerbosity.Normal));

        var request = Assert.IsType<ExtensionCreateRequest>(bound.Request);
        Assert.Equal(ExtensionCreateMode.DryRun, request.Mode);
        Assert.False(request.AllowInteraction);
    }

    private static string[] InvalidArguments(string scenario)
        => scenario switch
        {
            "repeated-id" => ["alpha", "beta", "--path", "/catalogue"],
            "repeated-path" => ["alpha", "--path", "/catalogue", "--path", "/other"],
            "repeated-name" => ["alpha", "--path", "/catalogue", "--name", "A", "--name", "B"],
            "repeated-description" => ["alpha", "--path", "/catalogue", "--description", "A", "--description", "B"],
            "repeated-package-version" => ["alpha", "--path", "/catalogue", "--package-version", "1", "--package-version", "2"],
            "unknown-option" => ["alpha", "--path", "/catalogue", "--unknown"],
            "unsupported-operand" => ["alpha", "extra", "--path", "/catalogue"],
            "invalid-id-uppercase" => ["Alpha", "--path", "/catalogue"],
            "invalid-id-leading-hyphen" => ["-alpha", "--path", "/catalogue"],
            "invalid-id-trailing-hyphen" => ["alpha-", "--path", "/catalogue"],
            "invalid-id-underscore" => ["alpha_beta", "--path", "/catalogue"],
            "invalid-id-unicode" => ["café", "--path", "/catalogue"],
            _ => throw new ArgumentOutOfRangeException(nameof(scenario), scenario, "The invalid Extension Create grammar case is not defined."),
        };

    private static CliInvocation Invocation(CliOutputFormat format, CliVerbosity verbosity)
    {
        var path = Path.GetFullPath(Path.Combine(Path.GetTempPath(), "extension-create-binding"));
        return new CliInvocation(
            new CliProcessIdentity("open-forge", "0.0.0-dev"),
            new CliPresentation(format, CliView.Expanded, verbosity),
            CliTerminalMode.None,
            new CliWorkspaceRequest(null, path),
            null);
    }
}
