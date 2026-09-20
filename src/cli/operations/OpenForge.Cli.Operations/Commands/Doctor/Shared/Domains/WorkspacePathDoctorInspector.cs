using OpenForge.Cli.Core.Commands.Doctor.Models.Result;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths.Models;
using OpenForge.Cli.Core.Framework.Workspace.Operational.Models;

namespace OpenForge.Cli.Core.Commands.Doctor.Shared.Domains;

internal static class WorkspacePathDoctorInspector
{
    internal static void Inspect(
        WorkspaceEntryDoctorView view,
        ICollection<DoctorFinding> findings)
    {
        Add(findings, view.Root, ReadRoot(view.Root));
        Add(findings, view.Agents, ReadAgents(view.Agents));
        Add(findings, view.Loader, ReadLoader(view.Loader));
        Add(findings, view.Entry, ReadEntry(view.Entry));
    }

    private static DoctorFindingDescriptor? ReadRoot(WorkspacePathObservation path)
    {
        if (path.Resolution is PhysicalPathState.Missing
            or PhysicalPathState.Inaccessible
            or PhysicalPathState.InputOutputFailure)
        {
            return DoctorDomainSupport.Error(
                DoctorFindingKind.WorkspaceUnavailable,
                "The selected workspace cannot be accessed.");
        }

        if (path.Resolution == PhysicalPathState.Contained
            && path.Component == PathComponentState.Ordinary
            && path.Attributes is { } attributes
            && (attributes & FileAttributes.Directory) == 0)
        {
            return DoctorDomainSupport.Error(
                DoctorFindingKind.WorkspaceNotDirectory,
                "The selected workspace is not a directory.");
        }

        return ReadUnsafePath(path);
    }

    private static DoctorFindingDescriptor? ReadAgents(WorkspacePathObservation path)
        => IsMissing(path)
            ? DoctorDomainSupport.Information(
                DoctorFindingKind.WorkspaceAgentsMissing,
                "The .agents boundary is missing.")
            : IsUnavailable(path)
                ? DoctorDomainSupport.Error(
                    DoctorFindingKind.WorkspaceAgentsInaccessible,
                    "The .agents boundary cannot be inspected safely.")
                : ReadUnsafePath(path);

    private static DoctorFindingDescriptor? ReadLoader(WorkspacePathObservation path)
        => IsMissing(path)
            ? DoctorDomainSupport.Error(
                DoctorFindingKind.WorkspaceLoaderMissing,
                "The required Loader is missing.")
            : IsUnavailable(path) || IsWrongComponent(path, expectDirectory: false)
                ? DoctorDomainSupport.Error(
                    DoctorFindingKind.WorkspaceLoaderUnreadable,
                    "The required Loader cannot be read as one ordinary file.")
                : ReadUnsafePath(path);

    private static DoctorFindingDescriptor? ReadEntry(WorkspacePathObservation path)
        => IsMissing(path)
            ? DoctorDomainSupport.Warning(
                DoctorFindingKind.WorkspaceEntryMissing,
                "The recognized workspace entrypoint is missing.",
                DoctorResolutionLane.ManualDecision)
            : IsUnavailable(path) || IsWrongComponent(path, expectDirectory: false)
                ? DoctorDomainSupport.Information(
                    DoctorFindingKind.WorkspaceParseIncomplete,
                    "The workspace entrypoint could not be read completely.")
                : ReadUnsafePath(path);

    private static DoctorFindingDescriptor? ReadUnsafePath(WorkspacePathObservation path)
        => path.Resolution switch
        {
            PhysicalPathState.Contained => path.Component switch
            {
                PathComponentState.Ordinary or PathComponentState.Missing => null,
                PathComponentState.Unsupported => DoctorDomainSupport.Information(
                    DoctorFindingKind.WorkspaceUnsupportedSource,
                    "A workspace path has an unsupported component kind."),
                PathComponentState.Link => DoctorDomainSupport.Error(
                    DoctorFindingKind.WorkspacePathContainment,
                    "A workspace path cannot prove safe physical containment."),
                PathComponentState.Inaccessible or PathComponentState.InputOutputFailure => null,
                _ => throw new ArgumentOutOfRangeException(nameof(path), path.Component, "The workspace component state is not defined."),
            },
            PhysicalPathState.Invalid => DoctorDomainSupport.Error(
                DoctorFindingKind.WorkspacePathInvalid,
                "A workspace path is invalid for its declared role."),
            PhysicalPathState.External or PhysicalPathState.Dangling or PhysicalPathState.Cycle =>
                DoctorDomainSupport.Error(
                    DoctorFindingKind.WorkspacePathContainment,
                    "A workspace path cannot prove safe physical containment."),
            PhysicalPathState.Unsupported => DoctorDomainSupport.Information(
                DoctorFindingKind.WorkspaceUnsupportedSource,
                "A workspace path has an unsupported physical kind."),
            PhysicalPathState.Missing or PhysicalPathState.Inaccessible or PhysicalPathState.InputOutputFailure => null,
            _ => throw new ArgumentOutOfRangeException(nameof(path), path.Resolution, "The workspace path state is not defined."),
        };

    private static bool IsMissing(WorkspacePathObservation path)
        => path.Resolution == PhysicalPathState.Missing
            || path is { Resolution: PhysicalPathState.Contained, Component: PathComponentState.Missing };

    private static bool IsUnavailable(WorkspacePathObservation path)
        => path.Resolution is PhysicalPathState.Inaccessible or PhysicalPathState.InputOutputFailure
            || path is { Resolution: PhysicalPathState.Contained, Component: PathComponentState.Inaccessible or PathComponentState.InputOutputFailure };

    private static bool IsWrongComponent(WorkspacePathObservation path, bool expectDirectory)
        => path is { Resolution: PhysicalPathState.Contained, Component: PathComponentState.Ordinary, Attributes: { } attributes }
            && ((attributes & FileAttributes.Directory) != 0) != expectDirectory;

    private static void Add(
        ICollection<DoctorFinding> findings,
        WorkspacePathObservation path,
        DoctorFindingDescriptor? descriptor)
    {
        if (descriptor is null)
        {
            return;
        }

        findings.Add(DoctorDomainSupport.Create(
            descriptor,
            DoctorDomainSupport.Subject(DoctorSubjectKind.Path, path.Path),
            DoctorDomainSupport.Provenance(
                DoctorDomainKind.WorkspaceEntry,
                DoctorProvenanceSource.WorkspaceEntry,
                path.Path),
            [new DoctorStateEvidence(ReadState(path))]));
    }

    private static DoctorObservedState ReadState(WorkspacePathObservation path)
        => IsMissing(path)
            ? DoctorObservedState.Missing
            : IsUnavailable(path)
                ? DoctorObservedState.Unavailable
                : path.Resolution == PhysicalPathState.Contained
                    ? DoctorObservedState.Present
                    : DoctorObservedState.Blocked;
}
