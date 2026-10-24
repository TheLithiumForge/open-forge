using OpenForge.Cli.Core.Commands.Doctor.Models.Result;
using OpenForge.Cli.Core.Commands.Doctor.Shared.Aggregation;
using OpenForge.Cli.Core.Commands.Doctor.Shared.Domains;
using OpenForge.Cli.Core.Framework.Workspace.Models.Layout;
using OpenForge.Cli.Core.Presentation.Doctor.Shared.Wording;
using OpenForge.Cli.TestSupport.Snapshots;

namespace OpenForge.Cli.Core.UnitTests.Commands.Doctor;

[Trait("Feature", "doctor-command"), Trait("Evidence", "Unit"), Trait("Boundary", "Processing")]
public sealed class DoctorLayoutPresentationTests
{
    private const string Root = "docs/forge";
    private const string WorkspaceLabel = "workspace";
    private const string Cause = "Observed missing path";
    private const string SnapshotName = "custom-root";

    [Fact(DisplayName = "Doctor presents the selected nested context folder and loader")]
    public void CustomRootWording()
    {
        var layout = WorkspaceLayout.Create(DoctorOperationTestSupport.Workspace(), Root);
        var folder = Finding(DoctorFindingKind.WorkspaceAgentsMissing);
        var loader = Finding(DoctorFindingKind.WorkspaceLoaderMissing);
        folder = DoctorLayoutPresentation.Project(layout, folder);
        loader = DoctorLayoutPresentation.Project(layout, loader);

        Assert.Equal(Root, folder.CustomMainRoot);
        Assert.Equal(Root + "/" + WorkspaceLayoutNames.LoaderFileName, loader.CustomLoaderPath);
        CommandOutputSnapshot.MatchSnapshot(
            $"{DoctorWording.Title(folder)}\n{DoctorWording.Message(folder)}\n{DoctorWording.Message(loader)}\n", SnapshotName);
    }

    private static DoctorFinding Finding(DoctorFindingKind kind)
        => DoctorDomainSupport.Create(
            DoctorDomainSupport.Warning(kind, Cause, DoctorResolutionLane.ManualDecision),
            DoctorDomainSupport.Subject(DoctorSubjectKind.Path, WorkspaceLabel),
            DoctorDomainSupport.Provenance(DoctorDomainKind.WorkspaceEntry, DoctorProvenanceSource.WorkspaceEntry),
            [new DoctorStateEvidence(DoctorObservedState.Missing)]);
}
