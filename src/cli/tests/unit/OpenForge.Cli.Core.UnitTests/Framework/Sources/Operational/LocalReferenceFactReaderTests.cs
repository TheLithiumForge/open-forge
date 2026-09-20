using OpenForge.Cli.Core.Framework.Sources.Models.References;
using OpenForge.Cli.Core.Framework.Sources.Operational;
using OpenForge.Cli.Core.Framework.Sources.Operational.Models.References;

namespace OpenForge.Cli.Core.UnitTests.Framework.Sources.Operational;

public sealed class LocalReferenceFactReaderTests
{
    private const string TargetPath = ".agents/target.md";
    private const string Fragment = "Section%20Name";

    [Trait("Boundary", "Processing")]
    [Theory(DisplayName = "Named target resolutions preserve exact fragment observation mappings"),
        InlineData((int)SourceLinkTargetResolution.Complete, TargetPath, (int)LocalReferenceFragmentState.Verified, Fragment),
        InlineData((int)SourceLinkTargetResolution.Missing, TargetPath, (int)LocalReferenceFragmentState.NotRequested, null),
        InlineData((int)SourceLinkTargetResolution.FragmentMissing, TargetPath, (int)LocalReferenceFragmentState.Missing, Fragment),
        InlineData((int)SourceLinkTargetResolution.Malformed, TargetPath, (int)LocalReferenceFragmentState.NotRequested, null),
        InlineData((int)SourceLinkTargetResolution.Absolute, TargetPath, (int)LocalReferenceFragmentState.NotRequested, null),
        InlineData((int)SourceLinkTargetResolution.Query, TargetPath, (int)LocalReferenceFragmentState.NotRequested, null),
        InlineData((int)SourceLinkTargetResolution.EncodingUnsupported, TargetPath, (int)LocalReferenceFragmentState.Unverified, Fragment),
        InlineData((int)SourceLinkTargetResolution.EncodingUnsupported, null, (int)LocalReferenceFragmentState.NotRequested, null),
        InlineData((int)SourceLinkTargetResolution.OutsideWorkspace, TargetPath, (int)LocalReferenceFragmentState.NotRequested, null),
        InlineData((int)SourceLinkTargetResolution.PhysicalEscape, TargetPath, (int)LocalReferenceFragmentState.NotRequested, null),
        InlineData((int)SourceLinkTargetResolution.Ambiguous, TargetPath, (int)LocalReferenceFragmentState.NotRequested, null),
        InlineData((int)SourceLinkTargetResolution.Unreadable, TargetPath, (int)LocalReferenceFragmentState.Unverified, Fragment),
        InlineData((int)SourceLinkTargetResolution.Unreadable, null, (int)LocalReferenceFragmentState.NotRequested, null),
        InlineData((int)SourceLinkTargetResolution.Unsupported, TargetPath, (int)LocalReferenceFragmentState.NotRequested, null),
        InlineData((int)SourceLinkTargetResolution.ExternalUnchecked, TargetPath, (int)LocalReferenceFragmentState.NotRequested, null),
        Trait("Feature", "references"), Trait("Evidence", "Unit")]
    public void NamedResolutionsPreserveFragmentObservations(
        int resolution,
        string? path,
        int expectedState,
        string? expectedAuthored)
    {
        var facts = CreateFacts(
            resolution: (SourceLinkTargetResolution)resolution,
            path: path,
            fragment: Fragment);

        var observation = LocalReferenceFactReader.ReadFragment(facts);

        Assert.Equal((LocalReferenceFragmentState)expectedState, observation.State);
        Assert.Equal(expectedAuthored, observation.Authored);
        Assert.Null(observation.Canonical);
    }

    [Trait("Boundary", "Processing")]
    [Theory(DisplayName = "An absent fragment returns before named or undefined resolution mapping"),
        InlineData((int)SourceLinkTargetResolution.Complete), InlineData(int.MaxValue),
        Trait("Feature", "references"), Trait("Evidence", "Unit")]
    public void AbsentFragmentReturnsBeforeResolutionMapping(int resolution)
    {
        var facts = CreateFacts(
            resolution: (SourceLinkTargetResolution)resolution,
            path: TargetPath,
            fragment: null);

        var observation = LocalReferenceFactReader.ReadFragment(facts);

        Assert.Equal(LocalReferenceFragmentState.NotRequested, observation.State);
        Assert.Null(observation.Authored);
        Assert.Null(observation.Canonical);
    }

    [Trait("Boundary", "Processing")]
    [Theory(DisplayName = "An established correction returns before named or undefined resolution mapping"),
        InlineData((int)SourceLinkTargetResolution.FragmentMissing), InlineData(int.MaxValue),
        Trait("Feature", "references"), Trait("Evidence", "Unit")]
    public void EstablishedCorrectionReturnsBeforeResolutionMapping(int resolution)
    {
        var established = CreateFacts(
            resolution: SourceLinkTargetResolution.FragmentMissing,
            path: TargetPath,
            fragment: "HEADING") with
        {
            Finding = new SourceLinkDestinationFinding
            {
                Code = SourceLinkDestinationFindingCode.FragmentMissing,
                Cause = "The authored fragment differs from the canonical spelling.",
                Candidates = [],
            },
        };
        established = established.WithCanonicalFragment(new SourceLinkCanonicalFragment(authored: "HEADING", canonical: "heading"));
        var facts = established with
        {
            Target = established.Target with { Resolution = (SourceLinkTargetResolution)resolution },
        };
        Assert.Equal((SourceLinkTargetResolution)resolution, facts.Target.Resolution);
        Assert.Same(established.CanonicalFragment, facts.CanonicalFragment);

        var observation = LocalReferenceFactReader.ReadFragment(facts);

        Assert.Equal(LocalReferenceFragmentState.CanonicalCorrection, observation.State);
        Assert.Equal("HEADING", observation.Authored);
        Assert.Equal("heading", observation.Canonical);
    }

    [Trait("Boundary", "Processing")]
    [Fact(DisplayName = "An admitted undefined resolution with a fragment is rejected at the mapping boundary"), Trait("Feature", "references"), Trait("Evidence", "Unit")]
    public void UndefinedResolutionWithFragmentIsRejected()
    {
        var undefined = (SourceLinkTargetResolution)int.MaxValue;
        var facts = CreateFacts(resolution: undefined, path: TargetPath, fragment: Fragment);
        Assert.Equal(undefined, facts.Target.Resolution);
        Assert.Equal(Fragment, facts.Fragment);
        Assert.Null(facts.CanonicalFragment);

        var exception = Assert.Throws<ArgumentOutOfRangeException>(() => LocalReferenceFactReader.ReadFragment(facts));

        Assert.Equal("facts", exception.ParamName);
        Assert.Equal(undefined, exception.ActualValue);
        Assert.StartsWith("The source-link target resolution is not defined.", exception.Message, StringComparison.Ordinal);
    }

    private static SourceLinkDestinationFacts CreateFacts(
        SourceLinkTargetResolution resolution,
        string? path,
        string? fragment)
        => new()
        {
            Fragment = fragment,
            Target = new SourceLinkTarget
            {
                Kind = SourceLinkTargetKind.Local,
                Id = null,
                Path = path,
                PhysicalPath = null,
                Layer = null,
                Resolution = resolution,
                Network = null,
            },
            Finding = null,
        };
}
