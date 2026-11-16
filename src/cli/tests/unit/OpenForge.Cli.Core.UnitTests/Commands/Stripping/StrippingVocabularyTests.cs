using OpenForge.Cli.Core.Commands.Stripping;
using OpenForge.Cli.Core.Commands.Stripping.Models.Planning;
using OpenForge.Cli.Core.Commands.Stripping.Models.Request;
using OpenForge.Cli.Core.Commands.Stripping.Models.Result;
using OpenForge.Cli.Core.Presentation.Stripping.Shared.Selection;

namespace OpenForge.Cli.Core.UnitTests.Commands.Stripping;

public sealed class StrippingVocabularyTests
{
    [Fact(DisplayName = "Strip parts read comma lists and repeated values in their fixed order without duplicates"), Trait("Feature", "strip"), Trait("Evidence", "Unit")]
    public void PartsReadInFixedOrder()
    {
        var links = StrippingDefinitions.PartName(StripPart.Links);
        var metadata = StrippingDefinitions.PartName(StripPart.Metadata);
        var entries = StrippingDefinitions.PartName(StripPart.Entries);

        Assert.True(StripPartGrammar.TryRead([$"{links}{StrippingDefinitions.PartSeparator}{metadata}", links, entries], out var parts));
        Assert.Equal([StripPart.Metadata, StripPart.Entries, StripPart.Links], parts);
        Assert.True(StripPartGrammar.TryRead([], out var none));
        Assert.Empty(none);
    }

    [Theory(DisplayName = "Strip parts reject unknown and empty members"), Trait("Feature", "strip"), Trait("Evidence", "Unit")]
    [InlineData(true)]
    [InlineData(false)]
    public void PartsRejectInvalidMembers(bool emptyMember)
    {
        var name = StrippingDefinitions.PartName(emptyMember ? StripPart.Metadata : StripPart.Links);
        var value = emptyMember ? name + StrippingDefinitions.PartSeparator : name.ToUpperInvariant();

        Assert.False(StripPartGrammar.TryRead([value], out _));
    }

    [Fact(DisplayName = "Strip vocabulary names every finite value once and rejects undefined values"), Trait("Feature", "strip"), Trait("Evidence", "Unit")]
    public void VocabularyIsExhaustive()
    {
        AssertDistinct(Enum.GetValues<StripPart>().Select(StrippingDefinitions.PartName));
        AssertDistinct(Enum.GetValues<StripRewritePart>().Select(StripWireVocabulary.RewritePartName));
        AssertDistinct(Enum.GetValues<StripPlanAction>().Select(StripWireVocabulary.ActionName));
        AssertDistinct(Enum.GetValues<StripPlanReason>().Select(StripWireVocabulary.ReasonName));
        AssertDistinct(Enum.GetValues<StripKeptReason>().Select(StripWireVocabulary.KeptReasonName));
        foreach (var kind in Enum.GetValues<StripCommandKind>())
        {
            AssertDistinct(Enum.GetValues<StripFindingCode>().Select(code => StripWireVocabulary.FindingCode(StrippingDefinitions.CommandIdentity(kind), code)));
        }

        Assert.NotEqual(StrippingDefinitions.CommandIdentity(StripCommandKind.Strip), StrippingDefinitions.CommandIdentity(StripCommandKind.Uninstall));
        Assert.Throws<ArgumentOutOfRangeException>(() => StrippingDefinitions.PartName((StripPart)int.MaxValue));
        Assert.Throws<ArgumentOutOfRangeException>(() => StripWireVocabulary.RewritePartName((StripRewritePart)int.MaxValue));
        Assert.Throws<ArgumentOutOfRangeException>(() => StripWireVocabulary.ActionName((StripPlanAction)int.MaxValue));
        Assert.Throws<ArgumentOutOfRangeException>(() => StripWireVocabulary.ReasonName((StripPlanReason)int.MaxValue));
        Assert.Throws<ArgumentOutOfRangeException>(() => StripWireVocabulary.KeptReasonName((StripKeptReason)int.MaxValue));
        Assert.Throws<ArgumentOutOfRangeException>(() => StrippingDefinitions.CommandIdentity((StripCommandKind)int.MaxValue));
        Assert.Throws<ArgumentOutOfRangeException>(() => StripWireVocabulary.FindingCode(StrippingDefinitions.StripCommandIdentity, (StripFindingCode)int.MaxValue));
    }

    // The strip and uninstall Interface contracts specify these wire names. Each map must name every enum value.
    [Trait("Boundary", "Output")]
    [Fact(DisplayName = "Strip wire vocabulary matches the names the strip and uninstall contracts specify"), Trait("Feature", "strip"), Trait("Evidence", "Unit")]
    public void VocabularyMatchesTheContract()
    {
        AssertSpecified<StripPart>(StrippingDefinitions.PartName, new()
        {
            [StripPart.Metadata] = "metadata",
            [StripPart.Entries] = "entries",
            [StripPart.Links] = "links",
        });
        AssertSpecified<StripRewritePart>(StripWireVocabulary.RewritePartName, new()
        {
            [StripRewritePart.Metadata] = "metadata",
            [StripRewritePart.Entries] = "entries",
            [StripRewritePart.Links] = "links",
            [StripRewritePart.Relink] = "relink",
            [StripRewritePart.ManagedSection] = "managed-section",
        });
        AssertSpecified<StripPlanAction>(StripWireVocabulary.ActionName, new()
        {
            [StripPlanAction.Delete] = "delete",
            [StripPlanAction.Rewrite] = "rewrite",
            [StripPlanAction.Move] = "move",
            [StripPlanAction.DeleteDirectory] = "delete-directory",
        });
        AssertSpecified<StripPlanReason>(StripWireVocabulary.ReasonName, new()
        {
            [StripPlanReason.Framework] = "framework",
            [StripPlanReason.Extension] = "extension",
            [StripPlanReason.RootHost] = "root-host",
            [StripPlanReason.Scaffold] = "scaffold",
            [StripPlanReason.State] = "state",
            [StripPlanReason.Strip] = "strip",
            [StripPlanReason.Flatten] = "flatten",
            [StripPlanReason.EmptyFolder] = "empty-folder",
        });
        AssertSpecified<StripKeptReason>(StripWireVocabulary.KeptReasonName, new()
        {
            [StripKeptReason.Edited] = "edited",
            [StripKeptReason.RecoveryData] = "recovery-data",
            [StripKeptReason.Link] = "link",
            [StripKeptReason.Unreadable] = "unreadable",
        });
        AssertSpecified<StripFindingCode>(StripWireVocabulary.FindingSuffix, new()
        {
            [StripFindingCode.InvalidInput] = "invalid-input",
            [StripFindingCode.WorkspaceUnavailable] = "workspace-unavailable",
            [StripFindingCode.DocumentUnsupported] = "document-unsupported",
            [StripFindingCode.FlattenCollision] = "flatten-collision",
            [StripFindingCode.FlattenManaged] = "flatten-managed",
            [StripFindingCode.LinkedFileSkipped] = "linked-file-skipped",
            [StripFindingCode.TargetChanged] = "target-changed",
            [StripFindingCode.TargetChangedDuringApply] = "target-changed-during-apply",
            [StripFindingCode.WorkspaceLockUnavailable] = "workspace-lock-unavailable",
            [StripFindingCode.ConfirmationRequired] = "confirmation-required",
            [StripFindingCode.PermissionDeclined] = "permission-declined",
            [StripFindingCode.RecoveryUnavailable] = "recovery-unavailable",
            [StripFindingCode.WriteFailed] = "write-failed",
            [StripFindingCode.RecoveryArtifactRetained] = "recovery-artifact-retained",
            [StripFindingCode.Interrupted] = "interrupted",
            [StripFindingCode.OwnershipUnavailable] = "ownership-unavailable",
            [StripFindingCode.LibraryAttached] = "library-attached",
            [StripFindingCode.TargetUnavailable] = "target-unavailable",
            [StripFindingCode.EditedKept] = "edited-kept",
            [StripFindingCode.RuntimeRetained] = "runtime-retained",
        });
    }

    private static void AssertSpecified<T>(Func<T, string> name, Dictionary<T, string> specified)
        where T : struct, Enum
    {
        Assert.Equal(Enum.GetValues<T>(), specified.Keys.Order());
        Assert.All(specified, pair => Assert.Equal(pair.Value, name(pair.Key)));
    }

    private static void AssertDistinct(IEnumerable<string> names)
    {
        var values = names.ToArray();
        Assert.All(values, value => Assert.False(string.IsNullOrWhiteSpace(value)));
        Assert.Equal(values.Length, values.Distinct(StringComparer.Ordinal).Count());
    }
}
