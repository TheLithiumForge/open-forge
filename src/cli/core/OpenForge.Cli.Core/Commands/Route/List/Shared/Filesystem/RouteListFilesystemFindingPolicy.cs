using OpenForge.Cli.Core.Framework.Filesystem;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Filesystem.TypedReads;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.Core.Commands.Route.List.Shared.Filesystem;

internal static class RouteListFilesystemFindingPolicy
{
    internal static RouteListFilesystemFinding? FromPhysical(
        string canonicalLogicalSubject,
        PhysicalPathResolution resolution)
    {
        ArgumentNullException.ThrowIfNull(resolution);
        return resolution.State switch
        {
            PhysicalPathState.Contained => null,
            PhysicalPathState.Missing => ReadUnavailable(
                canonicalLogicalSubject,
                "The filesystem candidate is unavailable."),
            PhysicalPathState.External => PhysicalBoundary(
                canonicalLogicalSubject,
                "The physical path leaves the selected workspace."),
            PhysicalPathState.Dangling => PhysicalBoundary(
                canonicalLogicalSubject,
                "The physical link target is unavailable."),
            PhysicalPathState.Cycle => PhysicalBoundary(
                canonicalLogicalSubject,
                "The physical link chain contains a cycle."),
            PhysicalPathState.Inaccessible => PhysicalBoundary(
                canonicalLogicalSubject,
                ReadFailureCause(resolution.Failure!, "Physical path access was denied.")),
            PhysicalPathState.Invalid => PhysicalBoundary(
                canonicalLogicalSubject,
                ReadFailureCause(resolution.Failure!, "The physical path is invalid.")),
            PhysicalPathState.Unsupported => PhysicalBoundary(
                canonicalLogicalSubject,
                ReadFailureCause(resolution.Failure!, "The physical path operation is unsupported.")),
            PhysicalPathState.InputOutputFailure => PhysicalBoundary(
                canonicalLogicalSubject,
                ReadFailureCause(resolution.Failure!, "The physical path operation failed.")),
            _ => throw new ArgumentOutOfRangeException(
                nameof(resolution),
                resolution.State,
                "The physical path state is not defined."),
        };
    }

    internal static RouteListFilesystemFinding? FromDirectory(RouteListDirectoryEnumeration result)
    {
        ArgumentNullException.ThrowIfNull(result);
        return result.State switch
        {
            DirectoryEnumerationState.Complete => null,
            DirectoryEnumerationState.Missing => ReadUnavailable(
                result.CanonicalLogicalPath,
                "The directory is unavailable."),
            DirectoryEnumerationState.AccessDenied => ReadUnavailable(
                result.CanonicalLogicalPath,
                ReadFailureCause(result.Failure!, "Directory access was denied.")),
            DirectoryEnumerationState.InputOutputFailure => ReadUnavailable(
                result.CanonicalLogicalPath,
                ReadFailureCause(result.Failure!, "Directory enumeration failed.")),
            DirectoryEnumerationState.Cancelled => Interrupted(result.CanonicalLogicalPath),
            _ => throw new ArgumentOutOfRangeException(nameof(result), result.State, "The directory state is not defined."),
        };
    }

    internal static RouteListFilesystemFinding? FromEntry(RouteListFilesystemEntry result)
    {
        ArgumentNullException.ThrowIfNull(result);
        return result.State switch
        {
            RouteListFilesystemEntryState.File or RouteListFilesystemEntryState.Directory => null,
            RouteListFilesystemEntryState.Missing => ReadUnavailable(
                result.CanonicalLogicalPath,
                "The filesystem candidate is unavailable."),
            RouteListFilesystemEntryState.Inaccessible => PhysicalBoundary(
                result.CanonicalLogicalPath,
                ReadFailureCause(result.Failure!, "Filesystem entry access was denied.")),
            RouteListFilesystemEntryState.Unsupported => PhysicalBoundary(
                result.CanonicalLogicalPath,
                ReadFailureCause(result.Failure!, "The filesystem entry is unsupported.")),
            RouteListFilesystemEntryState.InputOutputFailure => PhysicalBoundary(
                result.CanonicalLogicalPath,
                ReadFailureCause(result.Failure!, "Filesystem entry inspection failed.")),
            _ => throw new ArgumentOutOfRangeException(nameof(result), result.State, "The filesystem entry state is not defined."),
        };
    }

    internal static RouteListFilesystemFinding? FromFile<T>(FileReadResult<T> result)
    {
        ArgumentNullException.ThrowIfNull(result);
        if (!RouteListLogicalPath.IsCanonical(result.LogicalPath))
        {
            throw new ArgumentException("The file read logical path is not canonical.", nameof(result));
        }

        return result.State switch
        {
            FileReadState.Complete => null,
            FileReadState.Missing => ReadUnavailable(result.LogicalPath, "The source file is unavailable."),
            FileReadState.InvalidEncoding => ReadUnavailable(
                result.LogicalPath,
                ReadFailureCause(result.Failure!, "The source file is not valid UTF-8.")),
            FileReadState.AccessDenied => ReadUnavailable(
                result.LogicalPath,
                ReadFailureCause(result.Failure!, "Source file access was denied.")),
            FileReadState.InputOutputFailure => ReadUnavailable(
                result.LogicalPath,
                ReadFailureCause(result.Failure!, "The source file read failed.")),
            FileReadState.Cancelled => Interrupted(result.LogicalPath),
            FileReadState.InvalidSyntax => throw new ArgumentOutOfRangeException(
                nameof(result),
                result.State,
                "Route-list metadata syntax is classified by its command-local parser."),
            _ => throw new ArgumentOutOfRangeException(nameof(result), result.State, "The file read state is not defined."),
        };
    }

    internal static RouteListFilesystemFinding CandidateMissing(string canonicalLogicalSubject)
    {
        return ReadUnavailable(
            canonicalLogicalSubject,
            "The contained filesystem candidate is no longer available.");
    }

    internal static RouteListFilesystemFinding DirectoryExpected(string canonicalLogicalSubject)
    {
        return ReadUnavailable(
            canonicalLogicalSubject,
            "The inventory root is not a directory.");
    }

    internal static RouteListFilesystemFinding ContainedDirectoryCycle(string canonicalLogicalSubject)
    {
        return PhysicalBoundary(
            canonicalLogicalSubject,
            "The contained directory alias repeats an active traversal boundary.");
    }

    internal static RouteListFilesystemFinding MetadataMissing(string canonicalLogicalSubject)
    {
        return new RouteListFilesystemFinding(
            RouteListFindingCode.MetadataMissing,
            CliSemanticStatus.Incomplete,
            canonicalLogicalSubject,
            "Required source metadata is missing.");
    }

    internal static RouteListFilesystemFinding MetadataMalformed(string canonicalLogicalSubject)
    {
        return new RouteListFilesystemFinding(
            RouteListFindingCode.MetadataMalformed,
            CliSemanticStatus.Incomplete,
            canonicalLogicalSubject,
            "The source frontmatter or metadata shape is malformed.");
    }

    internal static RouteListFilesystemFinding CompatibilityEntrypoint(string canonicalLogicalSubject)
    {
        return new RouteListFilesystemFinding(
            RouteListFindingCode.AuthoredForm,
            CliSemanticStatus.Attention,
            canonicalLogicalSubject,
            "The entrypoint uses a supported compatibility filename.");
    }

    internal static RouteListFilesystemFinding OrphanOverwrite(string canonicalLogicalSubject)
    {
        return new RouteListFilesystemFinding(
            RouteListFindingCode.AuthoredForm,
            CliSemanticStatus.Attention,
            canonicalLogicalSubject,
            "The overwrite companion has no adjacent base source.");
    }

    internal static RouteListFilesystemFinding UnsupportedSourceForm(string canonicalLogicalSubject)
    {
        return new RouteListFilesystemFinding(
            RouteListFindingCode.AuthoredForm,
            CliSemanticStatus.Attention,
            canonicalLogicalSubject,
            "The source path has no supported automatic identity.");
    }

    internal static RouteListFilesystemFinding IdentityCollision(string canonicalLogicalSubject)
    {
        return new RouteListFilesystemFinding(
            RouteListFindingCode.IdentityCollision,
            CliSemanticStatus.Attention,
            canonicalLogicalSubject,
            "The source repeats another logical source's contained physical identity.");
    }

    internal static RouteListFilesystemFinding Interrupted(string canonicalLogicalSubject)
    {
        return new RouteListFilesystemFinding(
            RouteListFindingCode.Interrupted,
            CliSemanticStatus.Interrupted,
            canonicalLogicalSubject,
            "Route-list filesystem inventory was interrupted.");
    }

    private static RouteListFilesystemFinding ReadUnavailable(string subject, string cause)
    {
        return new RouteListFilesystemFinding(
            RouteListFindingCode.ReadUnavailable,
            CliSemanticStatus.Incomplete,
            subject,
            cause);
    }

    private static RouteListFilesystemFinding PhysicalBoundary(string subject, string cause)
    {
        return new RouteListFilesystemFinding(
            RouteListFindingCode.PhysicalBoundary,
            CliSemanticStatus.Blocked,
            subject,
            cause);
    }

    private static string ReadFailureCause(FilesystemFailure failure, string fallback)
    {
        ArgumentNullException.ThrowIfNull(failure);
        var cause = failure.DirectCause;
        var technicalPrefixEnd = cause.IndexOf("): ", StringComparison.Ordinal);
        if (technicalPrefixEnd >= 0)
        {
            cause = cause[(technicalPrefixEnd + 3)..];
        }

        return string.IsNullOrWhiteSpace(cause)
            ? fallback
            : cause;
    }
}
