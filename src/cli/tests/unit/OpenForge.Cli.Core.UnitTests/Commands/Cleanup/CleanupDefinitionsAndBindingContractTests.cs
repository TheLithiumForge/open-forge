using System.CommandLine;
using OpenForge.Cli.Core.Commands.Cleanup;
using OpenForge.Cli.Core.Commands.Cleanup.Models.Binding;
using OpenForge.Cli.Core.Commands.Cleanup.Models.Request;
using OpenForge.Cli.Core.Commands.Cleanup.Models.Result;
using OpenForge.Cli.Core.Shell.Composition.Models;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.Core.UnitTests.Commands.Cleanup;

public sealed class CleanupDefinitionsAndBindingContractTests
{
    [Fact(DisplayName = "Cleanup definitions expose the exact direct-root write-policy grammar"),
     Trait("Feature", "cleanup"), Trait("Evidence", "UnitContract")]
    public void DefinitionsExposeExactGrammar()
    {
        Assert.Equal("cleanup", CleanupDefinitions.CommandIdentity);
        Assert.Equal(1, CleanupDefinitions.SchemaVersion);
        Assert.Equal("cleanup", CleanupDefinitions.CleanupCommand.Name);
        Assert.Equal(
            "Remove recognized recovery bundles and drafts for the selected workspace.",
            CleanupDefinitions.CleanupCommand.Description);
        Assert.Equal("--dry-run", CleanupDefinitions.DryRun.Name);
        Assert.Equal(
            "Preview the complete cleanup plan without acquiring a lease or writing.",
            CleanupDefinitions.DryRun.Description);
        Assert.Equal(CliOptionArity.None, CleanupDefinitions.DryRun.Arity);
        Assert.False(CleanupDefinitions.DryRun.DefaultValue);
        Assert.Empty(CleanupDefinitions.DryRun.FiniteSpellings);
        Assert.Equal("open-forge cleanup", CleanupDefinitions.CleanupCommandLine);
        Assert.Equal("open-forge cleanup --help", CleanupDefinitions.CleanupHelpCommand);
        Assert.Equal("open-forge cleanup --verbose", CleanupDefinitions.VerboseCleanupCommand);
        Assert.Equal("open-forge doctor", CleanupDefinitions.DoctorCommand);
    }

    [Fact(DisplayName = "Cleanup symbols retain one direct command and one idempotent Boolean option"),
     Trait("Feature", "cleanup"), Trait("Evidence", "UnitContract")]
    public void SymbolsRetainDirectCommandAndDryRunOption()
    {
        var symbols = CleanupBinding.CreateSymbols();

        Assert.Same(symbols.CleanupCommand, symbols.Command);
        Assert.Equal("cleanup", symbols.Command.Name);
        Assert.Empty(symbols.Command.Aliases);
        Assert.Same(symbols.DryRun, Assert.Single(symbols.Command.Options));
        Assert.Equal("--dry-run", symbols.DryRun.Name);
        Assert.Equal(ArgumentArity.Zero, symbols.DryRun.Arity);
        Assert.False(symbols.DryRun.AllowMultipleArgumentsPerToken);
        Assert.Empty(symbols.Command.Arguments);
        Assert.Empty(symbols.Command.Subcommands);
        Assert.Empty(symbols.Command.Parse([]).Errors);
        Assert.NotEmpty(symbols.Command.Parse(["unexpected-operand"]).Errors);
        Assert.NotEmpty(symbols.Command.Parse(["--unknown"]).Errors);
    }

    [Fact(DisplayName = "Cleanup binding forms one complete typed apply or repeated dry-run request"),
     Trait("Feature", "cleanup"), Trait("Evidence", "UnitContract")]
    public void BindingFormsCompleteTypedRequest()
    {
        var symbols = CleanupBinding.CreateSymbols();
        var workspace = CleanupTestData.Workspace("binding");

        var applyArguments = Array.Empty<string>();
        var apply = CleanupBinding.Bind(
            new CliBindingParse(
                symbols.Command.Parse(applyArguments),
                applyArguments),
            CleanupTestData.Invocation(workspace),
            symbols);

        var dryRunArguments = new[] { "--dry-run", "--dry-run" };
        var dryRun = CleanupBinding.Bind(
            new CliBindingParse(
                symbols.Command.Parse(dryRunArguments),
                dryRunArguments),
            CleanupTestData.Invocation(workspace),
            symbols);

        var applyRequest = Assert.IsType<CleanupRequest>(apply.Request);
        var dryRunRequest = Assert.IsType<CleanupRequest>(dryRun.Request);
        Assert.Null(apply.InvalidResult);
        Assert.Null(dryRun.InvalidResult);
        Assert.Same(workspace, applyRequest.Workspace);
        Assert.Equal(CleanupMode.Apply, applyRequest.Mode);
        Assert.False(applyRequest.IsDryRun);
        Assert.Same(workspace, dryRunRequest.Workspace);
        Assert.Equal(CleanupMode.DryRun, dryRunRequest.Mode);
        Assert.True(dryRunRequest.IsDryRun);
    }

    [Fact(DisplayName = "Cleanup binding rejects an invocation without the selected workspace"),
     Trait("Feature", "cleanup"), Trait("Evidence", "UnitContract")]
    public void BindingRequiresSelectedWorkspace()
    {
        var symbols = CleanupBinding.CreateSymbols();
        var arguments = Array.Empty<string>();
        var invocation = CleanupTestData.Invocation() with { Workspace = null };

        Assert.Throws<InvalidOperationException>(() => CleanupBinding.Bind(
            new CliBindingParse(symbols.Command.Parse(arguments), arguments),
            invocation,
            symbols));
    }

    [Fact(DisplayName = "Cleanup finding mappings preserve exact status and next-action boundaries"),
     Trait("Feature", "cleanup"), Trait("Evidence", "UnitContract")]
    public void FindingMappingsPreserveExactStatusAndNextAction()
    {
        var expectedStatuses = new Dictionary<CleanupFindingCode, CliSemanticStatus>
        {
            [CleanupFindingCode.InvalidInput] = CliSemanticStatus.Invalid,
            [CleanupFindingCode.WorkspaceUnavailable] = CliSemanticStatus.Blocked,
            [CleanupFindingCode.WorkspaceNotDirectory] = CliSemanticStatus.Blocked,
            [CleanupFindingCode.WorkspaceUnsafe] = CliSemanticStatus.Blocked,
            [CleanupFindingCode.CatalogueIncomplete] = CliSemanticStatus.Incomplete,
            [CleanupFindingCode.RecoveryFinalMalformed] = CliSemanticStatus.Blocked,
            [CleanupFindingCode.RecoveryFinalUnsupported] = CliSemanticStatus.Blocked,
            [CleanupFindingCode.RecoveryFinalUnavailable] = CliSemanticStatus.Blocked,
            [CleanupFindingCode.RecoveryDraftUnsafe] = CliSemanticStatus.Blocked,
            [CleanupFindingCode.WorkspaceLockUnavailable] = CliSemanticStatus.Blocked,
            [CleanupFindingCode.CatalogueChangedDuringApply] = CliSemanticStatus.Blocked,
            [CleanupFindingCode.CandidateChangedDuringApply] = CliSemanticStatus.Blocked,
            [CleanupFindingCode.DeletionFailed] = CliSemanticStatus.Failed,
            [CleanupFindingCode.VerificationFailed] = CliSemanticStatus.Failed,
            [CleanupFindingCode.OperationFailed] = CliSemanticStatus.Failed,
            [CleanupFindingCode.Interrupted] = CliSemanticStatus.Interrupted,
        };

        Assert.Equal(Enum.GetValues<CleanupFindingCode>(), expectedStatuses.Keys);
        foreach (var item in expectedStatuses)
        {
            Assert.Equal(item.Value, CleanupDefinitions.ReadStatus(item.Key));
        }

        Assert.Null(CleanupDefinitions.ReadNextAction(CliSemanticStatus.Complete));
        Assert.Null(CleanupDefinitions.ReadNextAction(CliSemanticStatus.Attention));
        Assert.Equal(
            (CleanupDefinitions.CleanupHelpCommand, "Correct the Cleanup input, then rerun the request."),
            ReadNext(CliSemanticStatus.Invalid));
        Assert.Equal(
            (CleanupDefinitions.CleanupCommandLine, "Resolve the blocked cleanup boundary, then rerun Cleanup from a fresh catalogue."),
            ReadNext(CliSemanticStatus.Blocked));
        Assert.Equal(
            (CleanupDefinitions.DoctorCommand, "Inspect the unavailable workspace or recovery facts before relying on this Cleanup result."),
            ReadNext(CliSemanticStatus.Incomplete));
        Assert.Equal(
            (CleanupDefinitions.VerboseCleanupCommand, "Report the failure and retry the same Cleanup request with bounded diagnostics."),
            ReadNext(CliSemanticStatus.Failed));
        Assert.Equal(
            (CleanupDefinitions.CleanupCommandLine, "Rerun the same Cleanup request."),
            ReadNext(CliSemanticStatus.Interrupted));
    }

    [Fact(DisplayName = "Cleanup finite status mapping rejects undefined values"),
     Trait("Feature", "cleanup"), Trait("Evidence", "UnitContract")]
    public void UndefinedStatusMappingThrows()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            CleanupDefinitions.ReadStatus((CleanupFindingCode)int.MaxValue));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            CleanupDefinitions.ReadNextAction((CliSemanticStatus)int.MaxValue));
    }

    private static (string Command, string Reason) ReadNext(CliSemanticStatus status)
    {
        var next = Assert.IsType<CliNextAction>(CleanupDefinitions.ReadNextAction(status));
        return (next.Command, next.Reason);
    }
}
