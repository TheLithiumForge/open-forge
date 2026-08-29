using OpenForge.Cli.Core.Commands.Index;
using OpenForge.Cli.Core.Commands.Index.Models.Operation;
using OpenForge.Cli.Core.Commands.Index.Models.Planning;
using OpenForge.Cli.Core.Commands.Index.Models.Request;
using OpenForge.Cli.Core.Commands.Index.Models.Selection;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.Core.UnitTests.Commands.Index;

public sealed class IndexDefinitionsTests
{
    [Fact(DisplayName = "Index definitions expose the exact ordered finding vocabulary and fixed statuses"), Trait("Feature", "index-command"), Trait("Evidence", "Unit")]
    public void FindingVocabularyIsExact()
    {
        Assert.Equal(
        [
            ("index.invalid-input", CliSemanticStatus.Invalid),
            ("index.invalid-source", CliSemanticStatus.Invalid),
            ("index.workspace-unavailable", CliSemanticStatus.Blocked),
            ("index.workspace-unsafe", CliSemanticStatus.Blocked),
            ("index.source-ambiguous", CliSemanticStatus.Blocked),
            ("index.source-unsafe", CliSemanticStatus.Blocked),
            ("index.topology-ambiguous", CliSemanticStatus.Blocked),
            ("index.target-unexposed", CliSemanticStatus.Blocked),
            ("index.target-unsafe", CliSemanticStatus.Blocked),
            ("index.metadata-unsafe", CliSemanticStatus.Blocked),
            ("index.generated-region-unsafe", CliSemanticStatus.Blocked),
            ("index.workspace-lock-unavailable", CliSemanticStatus.Blocked),
            ("index.target-changed", CliSemanticStatus.Blocked),
            ("index.recovery-conflict", CliSemanticStatus.Blocked),
            ("index.discovery-incomplete", CliSemanticStatus.Incomplete),
            ("index.metadata-incomplete", CliSemanticStatus.Incomplete),
            ("index.projection-incomplete", CliSemanticStatus.Incomplete),
            ("index.recovery-unavailable", CliSemanticStatus.Incomplete),
            ("index.recovery-artifact-retained", CliSemanticStatus.Attention),
            ("index.target-changed-during-apply", CliSemanticStatus.Failed),
            ("index.write-failed", CliSemanticStatus.Failed),
            ("index.verification-failed", CliSemanticStatus.Failed),
            ("index.recovery-failed", CliSemanticStatus.Failed),
            ("index.operation-failed", CliSemanticStatus.Failed),
            ("index.interrupted", CliSemanticStatus.Interrupted),
        ],
            IndexDefinitions.FindingCodes
                .Select(IndexDefinitions.Read)
                .Select(definition => (definition.MachineName, definition.Status)));
    }

    [Fact(DisplayName = "Index definitions centralize every exact public finite spelling"), Trait("Feature", "index-command"), Trait("Evidence", "Unit")]
    public void FiniteSpellingsAreExact()
    {
        Assert.Equal(["apply", "dry-run"], Enum.GetValues<IndexMode>().Select(IndexDefinitions.ReadMachineName));
        Assert.Equal(["automatic-loader", "explicit-sources"], Enum.GetValues<IndexSelectionOrigin>().Select(IndexDefinitions.ReadMachineName));
        Assert.Equal(["not-established", "rooted", "detached", "mixed"], Enum.GetValues<IndexSelectionScope>().Select(IndexDefinitions.ReadMachineName));
        Assert.Equal(["rooted", "detached"], Enum.GetValues<IndexLogicalSourceScope>().Select(IndexDefinitions.ReadMachineName));
        Assert.Equal(["not-established", "unchanged", "update"], Enum.GetValues<IndexRegionAction>().Select(IndexDefinitions.ReadMachineName));
        Assert.Equal(
            ["not-established", "already-current", "not-requested", "not-started", "applied", "verified", "unknown"],
            Enum.GetValues<IndexRegionOutcome>().Select(IndexDefinitions.ReadMachineName));
        Assert.Equal(
            ["not-required", "not-created", "removed", "retained", "unknown"],
            Enum.GetValues<IndexRecoveryState>().Select(IndexDefinitions.ReadMachineName));
    }

    [Fact(DisplayName = "Index finite mappings reject undefined runtime values"), Trait("Feature", "index-command"), Trait("Evidence", "Unit")]
    public void FiniteMappingsRejectUndefinedValues()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            IndexDefinitions.Read((IndexFindingCode)int.MaxValue));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            IndexDefinitions.ReadMachineName((IndexFindingCode)int.MaxValue));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            IndexDefinitions.ReadMachineName((IndexMode)int.MaxValue));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            IndexDefinitions.ReadMachineName((IndexSelectionOrigin)int.MaxValue));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            IndexDefinitions.ReadMachineName((IndexSelectionScope)int.MaxValue));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            IndexDefinitions.ReadMachineName((IndexLogicalSourceScope)int.MaxValue));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            IndexDefinitions.ReadMachineName((IndexRegionAction)int.MaxValue));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            IndexDefinitions.ReadMachineName((IndexRegionOutcome)int.MaxValue));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            IndexDefinitions.ReadMachineName((IndexRecoveryState)int.MaxValue));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            IndexDefinitions.ReadNextAction((CliSemanticStatus)int.MaxValue, []));
    }
}
