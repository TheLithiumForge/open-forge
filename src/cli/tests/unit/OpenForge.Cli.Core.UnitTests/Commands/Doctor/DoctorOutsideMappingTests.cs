using OpenForge.Cli.Core.Commands.Doctor.Models.Result;
using OpenForge.Cli.Core.Commands.Doctor.Shared.Domains;
using OpenForge.Cli.Core.Presentation.Doctor.Shared.Wording;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Operation;
using OpenForge.Cli.TestSupport.Snapshots;

namespace OpenForge.Cli.Core.UnitTests.Commands.Doctor;

[Trait("Boundary", "Processing"), Trait("Feature", "doctor-outside"), Trait("Evidence", "Unit")]
public sealed class DoctorOutsideMappingTests
{
    [Fact]
    public void EveryOutsideConditionHasReviewedHumanWording()
    {
        var rows = Enum.GetValues<DoctorOutsideRouteState>().Select(state =>
        {
            var finding = Finding(state);
            return $"{state}\n{DoctorWording.Title(finding)}\n{DoctorWording.Message(finding)}\n{string.Join("\n", DoctorWording.Actions(finding).Select(action => action.Command))}\n";
        });
        CommandOutputSnapshot.MatchSnapshot(string.Join("\n", rows), "outside-conditions");
    }

    [Fact]
    public void OutsideConditionsHaveExplicitClosedWordingAndRejectUnknownValues()
    {
        Assert.Equal(new[] { DoctorOutsideRouteState.MissingDirectory, DoctorOutsideRouteState.MissingAnchor,
            DoctorOutsideRouteState.Unlisted, DoctorOutsideRouteState.UnsafeBoundary, DoctorOutsideRouteState.PhysicalAlias,
            DoctorOutsideRouteState.Unavailable, DoctorOutsideRouteState.AmbiguousAnchor, DoctorOutsideRouteState.StaleLoaderTarget },
            Enum.GetValues<DoctorOutsideRouteState>());
        foreach (var state in Enum.GetValues<DoctorOutsideRouteState>())
        {
            var finding = Finding(state);
            Assert.False(finding.IsIncludedFile);
            Assert.All(DoctorWording.Actions(finding), action => Assert.Equal(
                state == DoctorOutsideRouteState.MissingAnchor ? CliNextActionKind.Command : CliNextActionKind.Sentence, action.Kind));
            DoctorWording.Title(finding);
            DoctorWording.Message(finding);
        }
        var undefined = Finding((DoctorOutsideRouteState)int.MaxValue);
        Assert.Throws<ArgumentOutOfRangeException>(() => DoctorWording.Title(undefined));
        Assert.Throws<ArgumentOutOfRangeException>(() => DoctorWording.Message(undefined));
    }

    private static DoctorFinding Finding(DoctorOutsideRouteState state)
    {
        var descriptor = state == DoctorOutsideRouteState.Unlisted
            ? DoctorDomainSupport.Information(DoctorFindingKind.WorkspaceDetached, "Typed unlisted fixture")
            : DoctorDomainSupport.Warning(DoctorFindingKind.WorkspaceRootMissing, "Typed outside fixture", DoctorResolutionLane.ManualDecision);
        var path = state == DoctorOutsideRouteState.MissingAnchor ? "docs/team/_team.md" : "docs/team";
        return DoctorDomainSupport.Create(descriptor,
            DoctorDomainSupport.Subject(DoctorSubjectKind.Path, path),
            DoctorDomainSupport.Provenance(DoctorDomainKind.WorkspaceEntry, DoctorProvenanceSource.WorkspaceEntry),
            [new DoctorStateEvidence(DoctorObservedState.Unavailable)]) with
        { OutsideRoute = new(state, InitTarget: state == DoctorOutsideRouteState.MissingAnchor ? path : null) };
    }
}
