using OpenForge.Cli.Core.Framework.OperationalContributors.Models;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;

namespace OpenForge.Cli.Core.Framework.Workspace.Operational.Models;

internal sealed record WorkspaceEntryStatusView(
    OperationalViewState State,
    OperationalInstallationState Installation,
    string? EntryPath,
    string? LoaderPath);

internal sealed record WorkspaceEntryDoctorSummary(
    OperationalViewState State,
    OperationalInstallationState Installation,
    string? EntryPath,
    string? LoaderPath);

internal sealed record WorkspacePathObservationSet(
    WorkspacePathObservation Root,
    WorkspacePathObservation Agents,
    WorkspacePathObservation Entry,
    WorkspacePathObservation Loader);

internal sealed record WorkspaceEntryDoctorView(
    WorkspaceEntryDoctorSummary Summary,
    WorkspacePathObservationSet Paths)
{
    internal OperationalViewState State => Summary.State;

    internal OperationalInstallationState Installation => Summary.Installation;

    internal string? EntryPath => Summary.EntryPath;

    internal string? LoaderPath => Summary.LoaderPath;

    internal WorkspacePathObservation Agents => Paths.Agents;

    internal WorkspacePathObservation Root => Paths.Root;

    internal WorkspacePathObservation Entry => Paths.Entry;

    internal WorkspacePathObservation Loader => Paths.Loader;
}

internal sealed class WorkspacePathObservation
{
    private WorkspacePathObservation(
        string path,
        PhysicalPathState resolution,
        PathComponentState? component,
        FileAttributes? attributes)
    {
        Path = path;
        Resolution = resolution;
        Component = component;
        Attributes = attributes;
    }

    internal string Path { get; }

    internal PhysicalPathState Resolution { get; }

    internal PathComponentState? Component { get; }

    internal FileAttributes? Attributes { get; }

    internal static WorkspacePathObservation Unresolved(
        string path,
        PhysicalPathState resolution)
    {
        if (resolution == PhysicalPathState.Contained)
        {
            throw new ArgumentException(
                "An unresolved workspace path cannot be contained.",
                nameof(resolution));
        }

        return new(path, resolution, component: null, attributes: null);
    }

    internal static WorkspacePathObservation Contained(
        string path,
        PathComponentState component,
        FileAttributes? attributes)
    {
        var requiresAttributes = component is PathComponentState.Ordinary
            or PathComponentState.Unsupported;
        if (requiresAttributes != (attributes is not null))
        {
            throw new ArgumentException(
                "Workspace path attributes must match the contained component state.",
                nameof(attributes));
        }

        return new(path, PhysicalPathState.Contained, component, attributes);
    }
}
