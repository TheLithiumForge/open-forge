using OpenForge.Cli.Core.Framework.Filesystem;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;

namespace OpenForge.Cli.Core.Commands.Route.List.Shared.Filesystem;

internal enum RouteListFilesystemEntryState
{
    File,
    Directory,
    Missing,
    Inaccessible,
    Unsupported,
    InputOutputFailure,
}

internal sealed record RouteListFilesystemEntry
{
    internal RouteListFilesystemEntry(
        RouteListFilesystemEntryState state,
        string canonicalLogicalPath,
        FilesystemFailure? failure = null)
    {
        if (!Enum.IsDefined(state))
        {
            throw new ArgumentOutOfRangeException(nameof(state), state, "The filesystem entry state is not defined.");
        }

        if (!RouteListLogicalPath.IsCanonical(canonicalLogicalPath))
        {
            throw new ArgumentException("The filesystem entry logical path is not canonical.", nameof(canonicalLogicalPath));
        }

        var expectedFailureKind = state switch
        {
            RouteListFilesystemEntryState.Inaccessible => FilesystemFailureKind.AccessDenied,
            RouteListFilesystemEntryState.Unsupported => FilesystemFailureKind.Unsupported,
            RouteListFilesystemEntryState.InputOutputFailure => FilesystemFailureKind.InputOutput,
            _ => (FilesystemFailureKind?)null,
        };
        if (expectedFailureKind is null && failure is not null
            || expectedFailureKind is not null && failure?.Kind != expectedFailureKind)
        {
            throw new ArgumentException("The filesystem entry state and failure do not agree.", nameof(failure));
        }

        State = state;
        CanonicalLogicalPath = canonicalLogicalPath;
        Failure = failure;
    }

    internal RouteListFilesystemEntryState State { get; }

    internal string CanonicalLogicalPath { get; }

    internal FilesystemFailure? Failure { get; }
}

internal static class RouteListFilesystemEntryReader
{
    internal static RouteListFilesystemEntry Read(
        string provenPhysicalPath,
        string canonicalLogicalPath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(provenPhysicalPath);
        if (!RouteListLogicalPath.IsCanonical(canonicalLogicalPath))
        {
            throw new ArgumentException("The filesystem entry logical path is not canonical.", nameof(canonicalLogicalPath));
        }

        var component = LinkTargetReader.Read(provenPhysicalPath);
        if (component.State == PathComponentState.Ordinary)
        {
            var attributes = component.Attributes
                ?? throw new InvalidOperationException("An ordinary filesystem entry requires attributes.");
            if ((attributes & FileAttributes.Device) != 0)
            {
                return Failed(
                    RouteListFilesystemEntryState.Unsupported,
                    canonicalLogicalPath,
                    FilesystemFailureKind.Unsupported,
                    "The resolved filesystem entry is not an ordinary file or directory.");
            }

            var state = (attributes & FileAttributes.Directory) != 0
                ? RouteListFilesystemEntryState.Directory
                : RouteListFilesystemEntryState.File;
            return new RouteListFilesystemEntry(state, canonicalLogicalPath);
        }

        if (component.State == PathComponentState.Link)
        {
            return Failed(
                RouteListFilesystemEntryState.Unsupported,
                canonicalLogicalPath,
                FilesystemFailureKind.Unsupported,
                "The resolved filesystem entry changed to a link after physical containment was proved.");
        }

        return component.State switch
        {
            PathComponentState.Missing => new RouteListFilesystemEntry(
                RouteListFilesystemEntryState.Missing,
                canonicalLogicalPath),
            PathComponentState.Inaccessible => new RouteListFilesystemEntry(
                RouteListFilesystemEntryState.Inaccessible,
                canonicalLogicalPath,
                component.Failure),
            PathComponentState.Unsupported => new RouteListFilesystemEntry(
                RouteListFilesystemEntryState.Unsupported,
                canonicalLogicalPath,
                component.Failure),
            PathComponentState.InputOutputFailure => new RouteListFilesystemEntry(
                RouteListFilesystemEntryState.InputOutputFailure,
                canonicalLogicalPath,
                component.Failure),
            _ => throw new ArgumentOutOfRangeException(nameof(component), component.State, "The path component state is not defined."),
        };
    }

    private static RouteListFilesystemEntry Failed(
        RouteListFilesystemEntryState state,
        string canonicalLogicalPath,
        FilesystemFailureKind failureKind,
        string cause)
    {
        return new RouteListFilesystemEntry(
            state,
            canonicalLogicalPath,
            new FilesystemFailure(failureKind, cause));
    }
}
