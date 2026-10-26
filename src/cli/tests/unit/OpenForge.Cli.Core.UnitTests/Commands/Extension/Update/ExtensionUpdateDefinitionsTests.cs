using OpenForge.Cli.TestSupport.Snapshots;
using System.CommandLine;
using OpenForge.Cli.Core.Commands.Extension;
using OpenForge.Cli.Core.Commands.Extension.Update;
using OpenForge.Cli.Core.Commands.Extension.Update.Models.Binding;
using OpenForge.Cli.Core.Commands.Extension.Update.Models.Result;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.Core.UnitTests.Commands.Extension.Update;

public sealed class ExtensionUpdateDefinitionsTests
{
    [Trait("Boundary", "Processing")]
    [Fact(DisplayName = "Extension Update definitions expose the accepted source, selection, authority, and mode grammar"), Trait("Feature", "extension-update"), Trait("Evidence", "Unit")]
    public void DefinitionsExposeAcceptedGrammar()
    {
        Assert.Equal("extension update", ExtensionUpdateDefinitions.CommandIdentity);
        Assert.Equal(1, ExtensionUpdateDefinitions.SchemaVersion);
        Assert.Equal("update", ExtensionUpdateDefinitions.UpdateCommand.Name);
        Assert.Equal("--source", ExtensionUpdateDefinitions.Source.Name);
        Assert.Equal("--all", ExtensionUpdateDefinitions.All.Name);
        Assert.Equal("--force", ExtensionUpdateDefinitions.Force.Name);
        Assert.Equal(
            "Accepted for compatibility. Ordinary update already replaces or restores eligible managed content. Adds no authority or safety bypass.",
            ExtensionUpdateDefinitions.Force.Description);
        Assert.Equal("--prune", ExtensionUpdateDefinitions.Prune.Name);
        Assert.Equal("--automatic", ExtensionUpdateDefinitions.Automatic.Name);
        Assert.Equal("--dry-run", ExtensionUpdateDefinitions.DryRun.Name);
        Assert.Equal(CliOptionArity.ExactlyOne, ExtensionUpdateDefinitions.Source.Arity);
        Assert.Equal(CliOptionArity.None, ExtensionUpdateDefinitions.All.Arity);
        Assert.Equal(CliOptionArity.None, ExtensionUpdateDefinitions.Force.Arity);
        Assert.Equal(CliOptionArity.None, ExtensionUpdateDefinitions.Prune.Arity);
        Assert.Equal(CliOptionArity.None, ExtensionUpdateDefinitions.Automatic.Arity);
        Assert.Equal(CliOptionArity.None, ExtensionUpdateDefinitions.DryRun.Arity);
    }

    [Trait("Boundary", "Processing")]
    [Fact(DisplayName = "Extension Update symbols preserve repeatable IDs and idempotent authority flags"), Trait("Feature", "extension-update"), Trait("Evidence", "Unit")]
    public void SymbolsExposeRepeatableIdsAndAuthorityFlags()
    {
        var group = ExtensionBinding.CreateGroup();
        var symbols = ExtensionUpdateBinding.CreateSymbols(group);

        Assert.Same(symbols.Command, Assert.Single(group.Subcommands));
        Assert.Empty(symbols.Command.Aliases);
        Assert.Equal(ArgumentArity.ZeroOrMore, symbols.StableIds.Arity);
        Assert.Equal(typeof(string[]), symbols.StableIds.ValueType);
        Assert.Equal(
            ["--source", "--all", "--force", "--prune", "--automatic", "--dry-run", "--allow-path"],
            symbols.Command.Options.Select(option => option.Name));
        Assert.Equal(ArgumentArity.ZeroOrOne, symbols.Source.Arity);
        Assert.Equal(ArgumentArity.Zero, symbols.All.Arity);
        Assert.Equal(ArgumentArity.Zero, symbols.Force.Arity);
        Assert.Equal(ArgumentArity.Zero, symbols.Prune.Arity);
        Assert.Equal(ArgumentArity.Zero, symbols.Automatic.Arity);
        Assert.Equal(ArgumentArity.Zero, symbols.DryRun.Arity);

        var parse = symbols.Command.Parse(
        [
            "toolkit",
            "--source=/catalogue",
            "--force",
            "--prune",
            "--automatic",
            "--dry-run",
            "--dry-run",
        ]);

        Assert.Empty(parse.Errors);
        var stableIds = parse.GetValue(symbols.StableIds);
        Assert.NotNull(stableIds);
        Assert.Equal(["toolkit"], stableIds);
        Assert.Equal("/catalogue", parse.GetValue(symbols.Source));
        Assert.True(parse.GetValue(symbols.Force));
        Assert.True(parse.GetValue(symbols.Prune));
        Assert.True(parse.GetValue(symbols.Automatic));
        Assert.True(parse.GetValue(symbols.DryRun));
    }

    [Trait("Boundary", "Processing")]
    [Fact(DisplayName = "Extension Update maps every finding to its stable machine code and semantic status"), Trait("Feature", "extension-update"), Trait("Evidence", "Unit")]
    public void FindingDefinitionsMapEveryCodeAndStatus()
    {
        (ExtensionUpdateFindingCode Code, CliSemanticStatus Status)[] expected =
        [
            (ExtensionUpdateFindingCode.InvalidInput, CliSemanticStatus.Invalid),
            (ExtensionUpdateFindingCode.SelectionRequired, CliSemanticStatus.Invalid),
            (ExtensionUpdateFindingCode.InteractionEnded, CliSemanticStatus.Invalid),
            (ExtensionUpdateFindingCode.ConfirmationRequired, CliSemanticStatus.Invalid),
            (ExtensionUpdateFindingCode.SourceUnavailable, CliSemanticStatus.Incomplete),
            (ExtensionUpdateFindingCode.SourceInvalid, CliSemanticStatus.Invalid),
            (ExtensionUpdateFindingCode.SourceOverlap, CliSemanticStatus.Blocked),
            (ExtensionUpdateFindingCode.SourceIdentityConflict, CliSemanticStatus.Blocked),
            (ExtensionUpdateFindingCode.FrameworkUnavailable, CliSemanticStatus.Incomplete),
            (ExtensionUpdateFindingCode.FrameworkUnsafe, CliSemanticStatus.Blocked),
            (ExtensionUpdateFindingCode.LifecycleUnavailable, CliSemanticStatus.Incomplete),
            (ExtensionUpdateFindingCode.LifecycleBlocked, CliSemanticStatus.Blocked),
            (ExtensionUpdateFindingCode.ManagedDivergence, CliSemanticStatus.Attention),
            (ExtensionUpdateFindingCode.OwnershipConflict, CliSemanticStatus.Blocked),
            (ExtensionUpdateFindingCode.PermissionRequired, CliSemanticStatus.Blocked),
            (ExtensionUpdateFindingCode.PermissionDeclined, CliSemanticStatus.Blocked),
            (ExtensionUpdateFindingCode.PermissionsInvalid, CliSemanticStatus.Blocked),
            (ExtensionUpdateFindingCode.PermissionsUnavailable, CliSemanticStatus.Incomplete),
            (ExtensionUpdateFindingCode.PermissionsChanged, CliSemanticStatus.Blocked),
            (ExtensionUpdateFindingCode.PermissionWriteFailed, CliSemanticStatus.Failed),
            (ExtensionUpdateFindingCode.TargetUnsafe, CliSemanticStatus.Blocked),
            (ExtensionUpdateFindingCode.ProjectionUnavailable, CliSemanticStatus.Incomplete),
            (ExtensionUpdateFindingCode.GeneratedRegionUnsafe, CliSemanticStatus.Blocked),
            (ExtensionUpdateFindingCode.WorkspaceLockUnavailable, CliSemanticStatus.Blocked),
            (ExtensionUpdateFindingCode.TargetChanged, CliSemanticStatus.Blocked),
            (ExtensionUpdateFindingCode.RecoveryConflict, CliSemanticStatus.Blocked),
            (ExtensionUpdateFindingCode.RecoveryUnavailable, CliSemanticStatus.Incomplete),
            (ExtensionUpdateFindingCode.LifecycleObservation, CliSemanticStatus.Attention),
            (ExtensionUpdateFindingCode.RecoveryArtifactRetained, CliSemanticStatus.Attention),
            (ExtensionUpdateFindingCode.RecoveryHistory, CliSemanticStatus.Attention),
            (ExtensionUpdateFindingCode.RecoveryCoverageUnavailable, CliSemanticStatus.Attention),
            (ExtensionUpdateFindingCode.WriteFailed, CliSemanticStatus.Failed),
            (ExtensionUpdateFindingCode.TopologyVerificationFailed, CliSemanticStatus.Failed),
            (ExtensionUpdateFindingCode.LifecyclePublicationFailed, CliSemanticStatus.Failed),
            (ExtensionUpdateFindingCode.VerificationFailed, CliSemanticStatus.Failed),
            (ExtensionUpdateFindingCode.RecoveryFailed, CliSemanticStatus.Failed),
            (ExtensionUpdateFindingCode.OperationFailed, CliSemanticStatus.Failed),
            (ExtensionUpdateFindingCode.Interrupted, CliSemanticStatus.Interrupted),
            (ExtensionUpdateFindingCode.SettingsInvalid, CliSemanticStatus.Blocked),
            (ExtensionUpdateFindingCode.SettingsUnavailable, CliSemanticStatus.Incomplete),
            (ExtensionUpdateFindingCode.RemovedExtension, CliSemanticStatus.Blocked),
            (ExtensionUpdateFindingCode.BulkExcluded, CliSemanticStatus.Attention),
            (ExtensionUpdateFindingCode.PathExcluded, CliSemanticStatus.Complete),
            (ExtensionUpdateFindingCode.ExcludedAncestor, CliSemanticStatus.Blocked),
        ];
        Assert.Equal(expected.Select(item => item.Code), Enum.GetValues<ExtensionUpdateFindingCode>());
        Assert.Equal(expected.Select(item => item.Code), ExtensionUpdateDefinitions.FindingCodes);
        Assert.All(expected, item => Assert.Equal(item.Status, ExtensionUpdateDefinitions.ReadStatus(item.Code)));
        CommandOutputSnapshot.MatchSnapshot(string.Join("\n", ExtensionUpdateDefinitions.FindingCodes.Select(code =>
            $"{code}: {ExtensionUpdateDefinitions.ReadMachineName(code)}")), "finding-codes");
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            ExtensionUpdateDefinitions.ReadMachineName((ExtensionUpdateFindingCode)int.MaxValue));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            ExtensionUpdateDefinitions.ReadStatus((ExtensionUpdateFindingCode)int.MaxValue));
    }
}
