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
        Assert.Equal(
        [
            ("extension-update.invalid-input", CliSemanticStatus.Invalid),
            ("extension-update.selection-required", CliSemanticStatus.Invalid),
            ("extension-update.interaction-ended", CliSemanticStatus.Invalid),
            ("extension-update.confirmation-required", CliSemanticStatus.Invalid),
            ("extension-update.source-unavailable", CliSemanticStatus.Incomplete),
            ("extension-update.source-invalid", CliSemanticStatus.Invalid),
            ("extension-update.source-overlap", CliSemanticStatus.Blocked),
            ("extension-update.source-identity-conflict", CliSemanticStatus.Blocked),
            ("extension-update.framework-unavailable", CliSemanticStatus.Incomplete),
            ("extension-update.framework-unsafe", CliSemanticStatus.Blocked),
            ("extension-update.lifecycle-unavailable", CliSemanticStatus.Incomplete),
            ("extension-update.lifecycle-blocked", CliSemanticStatus.Blocked),
            ("extension-update.managed-divergence", CliSemanticStatus.Attention),
            ("extension-update.ownership-conflict", CliSemanticStatus.Blocked),
            ("extension-update.permission-required", CliSemanticStatus.Blocked),
            ("extension-update.permission-declined", CliSemanticStatus.Blocked),
            ("extension-update.permissions-invalid", CliSemanticStatus.Blocked),
            ("extension-update.permissions-unavailable", CliSemanticStatus.Incomplete),
            ("extension-update.permissions-changed", CliSemanticStatus.Blocked),
            ("extension-update.permission-write-failed", CliSemanticStatus.Failed),
            ("extension-update.target-unsafe", CliSemanticStatus.Blocked),
            ("extension-update.projection-unavailable", CliSemanticStatus.Incomplete),
            ("extension-update.generated-region-unsafe", CliSemanticStatus.Blocked),
            ("extension-update.workspace-lock-unavailable", CliSemanticStatus.Blocked),
            ("extension-update.target-changed", CliSemanticStatus.Blocked),
            ("extension-update.recovery-conflict", CliSemanticStatus.Blocked),
            ("extension-update.recovery-unavailable", CliSemanticStatus.Incomplete),
            ("extension-update.lifecycle-observation", CliSemanticStatus.Attention),
            ("extension-update.recovery-artifact-retained", CliSemanticStatus.Attention),
            ("extension-update.write-failed", CliSemanticStatus.Failed),
            ("extension-update.topology-verification-failed", CliSemanticStatus.Failed),
            ("extension-update.lifecycle-publication-failed", CliSemanticStatus.Failed),
            ("extension-update.verification-failed", CliSemanticStatus.Failed),
            ("extension-update.recovery-failed", CliSemanticStatus.Failed),
            ("extension-update.operation-failed", CliSemanticStatus.Failed),
            ("extension-update.interrupted", CliSemanticStatus.Interrupted),
            ("extension-update.settings-invalid", CliSemanticStatus.Blocked),
            ("extension-update.settings-unavailable", CliSemanticStatus.Incomplete),
            ("extension-update.removed-extension", CliSemanticStatus.Blocked),
            ("extension-update.bulk-excluded", CliSemanticStatus.Attention),
            ("extension-update.path-excluded", CliSemanticStatus.Complete),
            ("extension-update.excluded-ancestor", CliSemanticStatus.Blocked),
        ],
            ExtensionUpdateDefinitions.FindingCodes.Select(code => (
                ExtensionUpdateDefinitions.ReadMachineName(code),
                ExtensionUpdateDefinitions.ReadStatus(code))));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            ExtensionUpdateDefinitions.ReadMachineName((ExtensionUpdateFindingCode)int.MaxValue));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            ExtensionUpdateDefinitions.ReadStatus((ExtensionUpdateFindingCode)int.MaxValue));
    }
}
