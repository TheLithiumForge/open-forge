using OpenForge.Cli.Core.Commands.Shared.Serialization;
using OpenForge.Cli.Core.Framework.Libraries.Models.Observation;
using OpenForge.Cli.Core.Framework.OperationalContributors.Models;
using OpenForge.Cli.Core.Framework.Recovery.Models.Catalogue;
using OpenForge.Cli.Core.Framework.Recovery.Models.Comparison;
using OpenForge.Cli.Core.Framework.Recovery.Models.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;

namespace OpenForge.Cli.Core.UnitTests.Commands.Shared.Rendering;

/// <summary>
/// One oracle per shared wire mapping. The commands that report these facts must
/// not restate the expected names; that is the duplication these owners replaced.
/// </summary>
public sealed class SharedWireVocabularyTests
{
    [Trait("Boundary", "Output")]
    [Fact(DisplayName = "Recovery wire names cover every member of every recovery enum"), Trait("Feature", "command-presentation"), Trait("Evidence", "UnitContract")]
    public void RecoveryNamesAreComplete()
    {
        Assert.Equal(
            ["framework", "extension", "index", "route", "repair", "library"],
            Enum.GetValues<RecoveryBundleProducer>().Select(RecoveryWireVocabulary.Producer));
        Assert.Equal(
            ["install", "index", "create", "init", "move", "update", "remove", "repair", "attach", "sync", "detach"],
            Enum.GetValues<RecoveryBundleOperation>().Select(RecoveryWireVocabulary.Operation));
        Assert.Equal(
            ["final", "draft"],
            Enum.GetValues<RecoveryBundleCandidateKind>().Select(RecoveryWireVocabulary.CandidateKind));
        Assert.Equal(
            ["verified", "malformed", "unsupported", "unavailable", "incomplete"],
            Enum.GetValues<RecoveryBundleIntegrity>().Select(RecoveryWireVocabulary.Integrity));
        Assert.Equal(
            ["prior", "intended", "third", "unavailable", "blocked"],
            Enum.GetValues<RecoveryBundleTargetComparisonState>().Select(RecoveryWireVocabulary.TargetComparison));
    }

    [Trait("Boundary", "Output")]
    [Fact(DisplayName = "Source wire names cover every member of every source enum"), Trait("Feature", "command-presentation"), Trait("Evidence", "UnitContract")]
    public void SourceNamesAreComplete()
    {
        Assert.Equal(
            ["base", "overwrite"],
            Enum.GetValues<SourceLayerKind>().Select(SourceWireVocabulary.Layer));
        Assert.Equal(
            ["source-id", "source-path"],
            Enum.GetValues<SourceReferenceKind>().Select(SourceWireVocabulary.ReferenceKind));
        Assert.Equal(
            ["resolved", "invalid", "unknown", "unsupported", "ambiguous", "unsafe"],
            Enum.GetValues<SourceReferenceResolutionState>().Select(SourceWireVocabulary.ReferenceResolution));
    }

    [Trait("Boundary", "Output")]
    [Fact(DisplayName = "Operational and Library wire names cover every member of their enums"), Trait("Feature", "command-presentation"), Trait("Evidence", "UnitContract")]
    public void OperationalAndLibraryNamesAreComplete()
    {
        Assert.Equal(
            ["available", "unavailable", "not-applicable"],
            Enum.GetValues<OperationalValueState>().Select(OperationalWireVocabulary.ValueState));
        Assert.Equal(
            ["absent", "trusted", "untrusted", "incomplete", "blocked"],
            Enum.GetValues<OperationalLifecycleState>().Select(OperationalWireVocabulary.LifecycleState));
        Assert.Equal(
            ["available", "unavailable", "not-applicable"],
            Enum.GetValues<OperationalSourceAvailability>().Select(OperationalWireVocabulary.SourceAvailability));
        Assert.Equal(
            ["current", "missing", "changed", "blocked", "unavailable"],
            Enum.GetValues<LibraryMappingObservationState>().Select(LibraryWireVocabulary.MappingObservation));
    }

    [Trait("Boundary", "Output")]
    [Fact(DisplayName = "Every shared wire mapping rejects an undefined enum value"), Trait("Feature", "command-presentation"), Trait("Evidence", "UnitContract")]
    public void UndefinedValuesFailClosed()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => RecoveryWireVocabulary.Producer((RecoveryBundleProducer)int.MaxValue));
        Assert.Throws<ArgumentOutOfRangeException>(() => RecoveryWireVocabulary.Operation((RecoveryBundleOperation)int.MaxValue));
        Assert.Throws<ArgumentOutOfRangeException>(() => RecoveryWireVocabulary.CandidateKind((RecoveryBundleCandidateKind)int.MaxValue));
        Assert.Throws<ArgumentOutOfRangeException>(() => RecoveryWireVocabulary.Integrity((RecoveryBundleIntegrity)int.MaxValue));
        Assert.Throws<ArgumentOutOfRangeException>(() => RecoveryWireVocabulary.TargetComparison((RecoveryBundleTargetComparisonState)int.MaxValue));
        Assert.Throws<ArgumentOutOfRangeException>(() => SourceWireVocabulary.Layer((SourceLayerKind)int.MaxValue));
        Assert.Throws<ArgumentOutOfRangeException>(() => SourceWireVocabulary.ReferenceKind((SourceReferenceKind)int.MaxValue));
        Assert.Throws<ArgumentOutOfRangeException>(() => SourceWireVocabulary.ReferenceResolution((SourceReferenceResolutionState)int.MaxValue));
        Assert.Throws<ArgumentOutOfRangeException>(() => OperationalWireVocabulary.ValueState((OperationalValueState)int.MaxValue));
        Assert.Throws<ArgumentOutOfRangeException>(() => OperationalWireVocabulary.LifecycleState((OperationalLifecycleState)int.MaxValue));
        Assert.Throws<ArgumentOutOfRangeException>(() => OperationalWireVocabulary.SourceAvailability((OperationalSourceAvailability)int.MaxValue));
        Assert.Throws<ArgumentOutOfRangeException>(() => LibraryWireVocabulary.MappingObservation((LibraryMappingObservationState)int.MaxValue));
    }
}
