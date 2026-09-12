using OpenForge.Cli.Core.Framework.Filesystem.Models;

namespace OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths.Models;

internal enum PhysicalPathState
{
    Contained,
    Missing,
    Dangling,
    Inaccessible,
    External,
    Cycle,
    Invalid,
    Unsupported,
    InputOutputFailure,
}

internal sealed class PhysicalPathResolution
{
    private PhysicalPathResolution(
        PhysicalPathState state,
        string logicalPath,
        string? resolvedPhysicalPath,
        FilesystemFailure? failure)
    {
        State = state;
        LogicalPath = logicalPath;
        ResolvedPhysicalPath = resolvedPhysicalPath;
        Failure = failure;
    }

    internal PhysicalPathState State { get; }

    internal string LogicalPath { get; }

    internal string? ResolvedPhysicalPath { get; }

    internal FilesystemFailure? Failure { get; }

    internal string GetContainedPhysicalPath()
    {
        if (State != PhysicalPathState.Contained || ResolvedPhysicalPath is null)
        {
            throw new InvalidOperationException("A contained physical resolution requires its resolved path.");
        }

        return ResolvedPhysicalPath;
    }

    internal static PhysicalPathResolution Contained(string logicalPath, string physicalPath)
    {
        ValidatePath(logicalPath);
        ValidatePath(physicalPath);
        return new PhysicalPathResolution(PhysicalPathState.Contained, logicalPath, physicalPath, null);
    }

    internal static PhysicalPathResolution Classified(
        PhysicalPathState state,
        string logicalPath,
        string? resolvedPhysicalPath = null)
    {
        ValidatePath(logicalPath);
        if (state is PhysicalPathState.Contained
            or PhysicalPathState.Inaccessible
            or PhysicalPathState.Invalid
            or PhysicalPathState.Unsupported
            or PhysicalPathState.InputOutputFailure
            || !Enum.IsDefined(state))
        {
            throw new ArgumentOutOfRangeException(nameof(state), state, "The state requires another factory.");
        }

        if (state is PhysicalPathState.External or PhysicalPathState.Dangling or PhysicalPathState.Cycle)
        {
            ValidatePath(resolvedPhysicalPath);
        }
        else if (resolvedPhysicalPath is not null)
        {
            throw new ArgumentException("This classified state cannot carry a resolved physical path.", nameof(resolvedPhysicalPath));
        }

        return new PhysicalPathResolution(state, logicalPath, resolvedPhysicalPath, null);
    }

    internal static PhysicalPathResolution Failed(
        PhysicalPathState state,
        string logicalPath,
        FilesystemFailure failure)
    {
        ValidatePath(logicalPath);
        ArgumentNullException.ThrowIfNull(failure);
        if (state is not (PhysicalPathState.Inaccessible
            or PhysicalPathState.Invalid
            or PhysicalPathState.Unsupported
            or PhysicalPathState.InputOutputFailure))
        {
            throw new ArgumentOutOfRangeException(nameof(state), state, "The state is not a failure state.");
        }

        var expectedFailureKind = state switch
        {
            PhysicalPathState.Inaccessible => FilesystemFailureKind.AccessDenied,
            PhysicalPathState.Invalid => FilesystemFailureKind.InvalidPath,
            PhysicalPathState.Unsupported => FilesystemFailureKind.Unsupported,
            PhysicalPathState.InputOutputFailure => FilesystemFailureKind.InputOutput,
            _ => throw new ArgumentOutOfRangeException(nameof(state), state, "The state is not a failure state."),
        };
        if (failure.Kind != expectedFailureKind)
        {
            throw new ArgumentException("The filesystem failure kind does not match the physical path state.", nameof(failure));
        }

        return new PhysicalPathResolution(state, logicalPath, null, failure);
    }

    private static void ValidatePath(string? path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
    }
}
