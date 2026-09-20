using OpenForge.Cli.Core.Commands.References.Models.Occurrence;
using OpenForge.Cli.Core.Commands.References.Models.Request;
using OpenForge.Cli.Core.Commands.References.Models.Result;
using OpenForge.Cli.Core.Presentation.References.Shared.Selection;
using OpenForge.Cli.Core.Presentation.References.Shared.Wording;

namespace OpenForge.Cli.Core.UnitTests.Commands.References.Shared.Rendering;

/// <summary>
/// The typed vocabularies References publishes. Every named value is mapped, and an undefined
/// runtime value is rejected rather than printed, because a switch over an enum's named members
/// is not closed over its runtime value space.
/// </summary>
public sealed class ReferencesVocabularyTests
{
    [Trait("Boundary", "Output")]
    [Fact(DisplayName = "References resolution labels cover every typed distinction and keep a fine link wordless"), Trait("Feature", "references"), Trait("Evidence", "Unit")]
    public void RowStateCoversEveryResolution()
    {
        var labels = Enum.GetValues<ReferencesTargetResolution>()
            .ToDictionary(resolution => resolution, ReferencesWording.RowState);

        Assert.Null(labels[ReferencesTargetResolution.Complete]);
        Assert.Equal("missing", labels[ReferencesTargetResolution.Missing]);
        Assert.Equal("heading not found", labels[ReferencesTargetResolution.FragmentMissing]);
        Assert.Equal("not checked", labels[ReferencesTargetResolution.ExternalUnchecked]);
        Assert.Equal("not followed", labels[ReferencesTargetResolution.Unsupported]);
        Assert.Equal("target could not be read", labels[ReferencesTargetResolution.Unreadable]);
        Assert.All(
            labels.Where(pair => pair.Key != ReferencesTargetResolution.Complete),
            pair => Assert.False(string.IsNullOrWhiteSpace(pair.Value)));
    }

    [Trait("Boundary", "Output")]
    [Fact(DisplayName = "References wire vocabularies map every named value to a distinct machine name"), Trait("Feature", "references"), Trait("Evidence", "Unit")]
    public void WireVocabulariesAreCompleteAndDistinct()
    {
        var resolutions = Enum.GetValues<ReferencesTargetResolution>()
            .Select(ReferencesWording.WireResolution)
            .ToArray();
        Assert.Equal(resolutions.Length, resolutions.Distinct(StringComparer.Ordinal).Count());
        Assert.All(resolutions, value => Assert.False(string.IsNullOrWhiteSpace(value)));

        var codes = Enum.GetValues<ReferencesFindingCode>()
            .Select(ReferencesWireVocabulary.Name)
            .ToArray();
        Assert.Equal(codes.Length, codes.Distinct(StringComparer.Ordinal).Count());
        Assert.All(codes, value => Assert.StartsWith("references.", value, StringComparison.Ordinal));

        var titles = Enum.GetValues<ReferencesFindingCode>()
            .Select(ReferencesWording.FindingTitle)
            .ToArray();
        Assert.All(titles, value => Assert.False(string.IsNullOrWhiteSpace(value)));

        Assert.Equal("base", ReferencesWording.WireLayer(ReferencesLayer.Base));
        Assert.Equal("overwrite", ReferencesWording.WireLayer(ReferencesLayer.Overwrite));
        Assert.Equal("in", ReferencesWording.WireDirection(ReferencesDirection.In));
        Assert.Equal("out", ReferencesWording.WireDirection(ReferencesDirection.Out));
        Assert.Equal("both", ReferencesWording.WireDirection(ReferencesDirection.Both));
    }

    [Trait("Boundary", "Output")]
    [Fact(DisplayName = "References identity findings use their distinct messages and retain missing-path honesty"), Trait("Feature", "references"), Trait("Evidence", "Unit")]
    public void IdentityFindingWordingIsDistinctAndFactBounded()
    {
        Assert.Equal(
            "The ID docs/guide matches more than one file. Use the exact path.",
            ReferencesWording.IdentityCollision("docs/guide"));
        Assert.Equal(
            ".agents/alias.md and .agents/target.md resolve to the same physical file.",
            ReferencesWording.PhysicalAlias([".agents/alias.md", ".agents/target.md"]));
        Assert.Equal(
            "The identity of .agents/.md could not be determined.",
            ReferencesWording.IdentityUnavailable(".agents/.md"));
        Assert.Equal(
            "The physical alias involving .agents/alias.md could not be described because one of its paths is unavailable.",
            ReferencesWording.PhysicalAlias([".agents/alias.md"]));
    }

    [Trait("Boundary", "Output")]
    [Fact(DisplayName = "References vocabularies reject an undefined runtime value instead of printing it"), Trait("Feature", "references"), Trait("Evidence", "Unit")]
    public void UndefinedRuntimeValuesAreRejected()
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () => ReferencesWording.RowState((ReferencesTargetResolution)9999));
        Assert.Throws<ArgumentOutOfRangeException>(
            () => ReferencesWording.WireResolution((ReferencesTargetResolution)9999));
        Assert.Throws<ArgumentOutOfRangeException>(
            () => ReferencesWording.WireLayer((ReferencesLayer)9999));
        Assert.Throws<ArgumentOutOfRangeException>(
            () => ReferencesWording.WireDirection((ReferencesDirection)9999));
        Assert.Throws<ArgumentOutOfRangeException>(
            () => ReferencesWireVocabulary.Name((ReferencesFindingCode)9999));
        Assert.Throws<ArgumentOutOfRangeException>(
            () => ReferencesWording.FindingTitle((ReferencesFindingCode)9999));
    }

    [Trait("Boundary", "Output")]
    [Fact(DisplayName = "References row-state codes are exactly the ones an out row already prints inline"), Trait("Feature", "references"), Trait("Evidence", "Unit")]
    public void RowStateCodesMatchTheInlineRowFacts()
    {
        var rowStates = Enum.GetValues<ReferencesFindingCode>()
            .Where(ReferencesWireVocabulary.IsRowState)
            .ToArray();

        Assert.Equal(
            [
                ReferencesFindingCode.DestinationMalformed,
                ReferencesFindingCode.DestinationUnsupported,
                ReferencesFindingCode.TargetMissing,
                ReferencesFindingCode.FragmentMissing,
                ReferencesFindingCode.TargetUnreadable,
            ],
            rowStates);
        Assert.All(rowStates, code => Assert.True(ReferencesWireVocabulary.IsRowState(ReferencesWireVocabulary.Name(code))));
        Assert.False(ReferencesWireVocabulary.IsRowState(ReferencesWireVocabulary.Name(ReferencesFindingCode.TargetUnsafe)));
    }
}
