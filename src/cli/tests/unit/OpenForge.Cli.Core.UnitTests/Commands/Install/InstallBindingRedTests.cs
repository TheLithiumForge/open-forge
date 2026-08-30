using System.CommandLine;
using OpenForge.Cli.Core.Commands.Install;
using OpenForge.Cli.Core.Commands.Install.Models.Binding;
using OpenForge.Cli.Core.Commands.Install.Models.Request;
using OpenForge.Cli.Core.Commands.Install.Models.Result;
using OpenForge.Cli.Core.Framework.Workspace;
using OpenForge.Cli.Core.Shell.Composition.Models;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Invocation;

namespace OpenForge.Cli.Core.UnitTests.Commands.Install;

public sealed class InstallBindingRedTests
{
    [Fact(DisplayName = "Install exposes the exact direct zero-operand command and three boolean options"), Trait("Feature", "install-command"), Trait("Evidence", "Unit")]
    public void SymbolsExposeExactDirectGrammar()
    {
        var symbols = InstallBinding.CreateSymbols();

        Assert.Equal("install", symbols.InstallCommand.Name);
        Assert.Empty(symbols.InstallCommand.Aliases);
        Assert.Empty(symbols.InstallCommand.Subcommands);
        Assert.Empty(symbols.InstallCommand.Arguments);
        Assert.Equal(
            ["--force", "--automatic", "--dry-run"],
            symbols.InstallCommand.Options.Select(option => option.Name));
        Assert.Equal(ArgumentArity.Zero, symbols.Force.Arity);
        Assert.Equal(ArgumentArity.Zero, symbols.Automatic.Arity);
        Assert.Equal(ArgumentArity.Zero, symbols.DryRun.Arity);

        var operandParse = symbols.InstallCommand.Parse(["unexpected-operand"]);
        Assert.NotEmpty(operandParse.Errors);
    }

    [Fact(DisplayName = "Install binding collapses repeated boolean options without changing independent dimensions"), Trait("Feature", "install-command"), Trait("Evidence", "Unit")]
    public void BindingNormalizesRepeatedBooleans()
    {
        var symbols = InstallBinding.CreateSymbols();
        var bound = new InstallRequestBinder(symbols).Bind(
            new CliBindingParse(symbols.InstallCommand.Parse(
            [
                "--force",
                "--force",
                "--automatic",
                "--automatic",
                "--dry-run",
                "--dry-run",
            ])),
            Invocation(CliOutputFormat.Human));

        var request = Assert.IsType<InstallRequest>(bound.Request);
        Assert.Null(bound.InvalidResult);
        Assert.True(request.Force);
        Assert.True(request.Automatic);
        Assert.Equal(InstallMode.DryRun, request.Mode);
        Assert.False(request.AllowsInteractiveConfirmation);
    }

    [Theory(DisplayName = "Install binding normalizes interaction policy from presentation and mode"), Trait("Feature", "install-command"), Trait("Evidence", "Unit")]
    [InlineData("human", false, false, true)]
    [InlineData("human", true, false, false)]
    [InlineData("human", false, true, false)]
    [InlineData("json", false, false, false)]
    [InlineData("json", true, false, false)]
    public void BindingNormalizesInteractionPolicy(
        string format,
        bool automatic,
        bool dryRun,
        bool allowsInteractiveConfirmation)
    {
        var symbols = InstallBinding.CreateSymbols();
        var arguments = new List<string>();
        if (automatic)
        {
            arguments.Add("--automatic");
        }

        if (dryRun)
        {
            arguments.Add("--dry-run");
        }

        var bound = new InstallRequestBinder(symbols).Bind(
            new CliBindingParse(symbols.InstallCommand.Parse([.. arguments])),
            Invocation(ReadFormat(format)));

        var request = Assert.IsType<InstallRequest>(bound.Request);
        Assert.Equal(allowsInteractiveConfirmation, request.AllowsInteractiveConfirmation);
        Assert.Equal(automatic, request.Automatic);
        Assert.Equal(dryRun ? InstallMode.DryRun : InstallMode.Apply, request.Mode);
    }

    [Fact(DisplayName = "Install request rejects an undefined mode"), Trait("Feature", "install-command"), Trait("Evidence", "Unit")]
    public void RequestRejectsUndefinedMode()
    {
        var workspace = Workspace();

        Assert.Throws<ArgumentOutOfRangeException>(() => new InstallRequest(
            workspace: workspace,
            mode: (InstallMode)int.MaxValue,
            force: false,
            automatic: false,
            allowsInteractiveConfirmation: false));
        Assert.Throws<ArgumentException>(() => new InstallRequest(
            workspace,
            InstallMode.Apply,
            force: false,
            automatic: true,
            allowsInteractiveConfirmation: true));
        Assert.Throws<ArgumentException>(() => new InstallRequest(
            workspace,
            InstallMode.DryRun,
            force: false,
            automatic: false,
            allowsInteractiveConfirmation: true));
    }

    [Theory(DisplayName = "Install confirmation-required result directs the same request to automatic mode"), Trait("Feature", "install-command"), Trait("Evidence", "Unit")]
    [InlineData(false, "open-forge install --automatic")]
    [InlineData(true, "open-forge install --force --automatic")]
    public void ConfirmationRequiredUsesSameCommandAutomaticNextAction(
        bool force,
        string expectedCommand)
    {
        var input = new InstallBindingInput(
            Force: force,
            Automatic: false,
            Mode: InstallMode.Apply);
        var result = InstallResult.Invalid(
            input,
            Workspace(),
            [new InstallFinding(
                InstallFindingCode.ConfirmationRequired,
                "Install requires confirmation before writing.")]);

        Assert.Equal(CliSemanticStatus.Invalid, result.Status);
        var next = Assert.IsType<CliNextAction>(result.Next);
        Assert.Equal(expectedCommand, next.Command);
    }

    [Fact(DisplayName = "Install managed divergence directs callers to the root update operation"), Trait("Feature", "install-command"), Trait("Evidence", "Unit")]
    public void ManagedDivergenceUsesUpdateNextAction()
    {
        var result = InstallResult.Invalid(
            new InstallBindingInput(
                Force: true,
                Automatic: true,
                Mode: InstallMode.Apply),
            Workspace(),
            [new InstallFinding(
                InstallFindingCode.ManagedDivergence,
                "The trusted managed Framework state has diverged.")]);

        Assert.Equal(CliSemanticStatus.Blocked, result.Status);
        var next = Assert.IsType<CliNextAction>(result.Next);
        Assert.Equal("open-forge update", next.Command);
    }

    private static CliInvocation Invocation(CliOutputFormat format)
    {
        var workspace = Workspace();
        return new CliInvocation(
            new CliProcessIdentity("open-forge", "1.0.0"),
            new CliPresentation(format, CliView.Expanded, CliVerbosity.Normal),
            CliTerminalMode.None,
            new CliWorkspaceRequest(null, workspace.LexicalRoot),
            workspace);
    }

    private static CliOutputFormat ReadFormat(string format)
        => format switch
        {
            "human" => CliOutputFormat.Human,
            "json" => CliOutputFormat.Json,
            _ => throw new ArgumentOutOfRangeException(nameof(format), format, "The test format is not defined."),
        };

    private static CliWorkspace Workspace()
    {
        var root = Path.GetFullPath(Path.Combine(Path.GetTempPath(), "install-binding-red"));
        return new CliWorkspace(root, root, CliWorkspaceSelectionMethod.ExplicitWorkspace);
    }
}
