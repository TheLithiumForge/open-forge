using OpenForge.Cli.Core.Framework.Filesystem.Models;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths.Models;
using OpenForge.Cli.Core.Framework.Filesystem.TypedReads.Models;
using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;
using OpenForge.Cli.Core.Framework.Sources.Models.Reading;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.Core.Commands.Route.List.Shared.Filesystem;

internal static class RouteListFilesystemFindingPolicy
{
    internal static RouteListFilesystemFinding? FromFile<T>(FileReadResult<T> result)
    {
        ArgumentNullException.ThrowIfNull(result);
        if (!SourceLogicalPath.IsCanonicalSource(result.LogicalPath))
        {
            throw new ArgumentException("The file read logical path is not canonical.", nameof(result));
        }

        return result.State switch
        {
            FileReadState.Complete => null,
            FileReadState.Missing => ReadUnavailable(result.LogicalPath, "The source file is unavailable."),
            FileReadState.InvalidEncoding => ReadUnavailable(
                result.LogicalPath,
                ReadFailureCause(ReadFailure(result), "The source file is not valid UTF-8.")),
            FileReadState.AccessDenied => ReadUnavailable(
                result.LogicalPath,
                ReadFailureCause(ReadFailure(result), "Source file access was denied.")),
            FileReadState.InputOutputFailure => ReadUnavailable(
                result.LogicalPath,
                ReadFailureCause(ReadFailure(result), "The source file read failed.")),
            FileReadState.Cancelled => Interrupted(result.LogicalPath),
            FileReadState.InvalidSyntax => throw new ArgumentOutOfRangeException(
                nameof(result),
                result.State,
                "Route-list metadata syntax is classified by its command-local parser."),
            _ => throw new ArgumentOutOfRangeException(nameof(result), result.State, "The file read state is not defined."),
        };
    }

    internal static RouteListFilesystemFinding? FromCatalogueIssue(
        SourceCatalogueIssue issue,
        SourceCandidate? candidate)
    {
        ArgumentNullException.ThrowIfNull(issue);
        return issue.Code switch
        {
            SourceCatalogueIssueCode.RootMissing => ReadUnavailable(
                issue.AttemptedCanonicalPath,
                "The filesystem candidate is unavailable."),
            SourceCatalogueIssueCode.RootUnsafe => PhysicalBoundary(
                issue.AttemptedCanonicalPath,
                ReadPhysicalCause(candidate?.PhysicalState, issue.Failure)),
            SourceCatalogueIssueCode.RootUnavailable => issue.Failure is null
                ? DirectoryExpected(issue.AttemptedCanonicalPath)
                : PhysicalBoundary(
                    issue.AttemptedCanonicalPath,
                    ReadFailureCause(issue.Failure, "The physical path operation failed.")),
            SourceCatalogueIssueCode.DirectoryUnavailable => ReadUnavailable(
                issue.AttemptedCanonicalPath,
                ReadFailureCause(issue.Failure, "Directory enumeration failed.")),
            SourceCatalogueIssueCode.CandidateUnsafe => ReadCandidateUnsafe(issue, candidate),
            SourceCatalogueIssueCode.CandidateUnavailable => PhysicalBoundary(
                issue.AttemptedCanonicalPath,
                ReadFailureCause(issue.Failure, "Filesystem entry inspection failed.")),
            SourceCatalogueIssueCode.IdentityUnavailable
                when candidate?.PhysicalState == PhysicalPathState.Contained
                    && candidate.Form.HasValue
                    && candidate.Form.Value != SourceDocumentForm.OverwriteCompanion =>
                UnsupportedSourceForm(issue.AttemptedCanonicalPath),
            SourceCatalogueIssueCode.IdentityUnavailable => null,
            SourceCatalogueIssueCode.OrphanOverwrite => OrphanOverwrite(issue.AttemptedCanonicalPath),
            SourceCatalogueIssueCode.UnsupportedSource
                or SourceCatalogueIssueCode.EntrypointAmbiguous
                or SourceCatalogueIssueCode.EntrypointCompatibilityCollision
                or SourceCatalogueIssueCode.IdentityCollision
                or SourceCatalogueIssueCode.PhysicalAlias => null,
            _ => throw new ArgumentOutOfRangeException(nameof(issue), issue.Code, "The source catalogue issue code is not defined."),
        };
    }

    internal static RouteListFilesystemFinding? FromDocumentRead(SourceDocumentReadResult result)
    {
        ArgumentNullException.ThrowIfNull(result);
        if (result.Verification.State is SourceLayerVerificationState.Verified
            or SourceLayerVerificationState.Missing)
        {
            var read = result.Read
                ?? throw new InvalidOperationException("A verified or missing source layer requires its typed read result.");
            return read.State == FileReadState.Cancelled
                ? null
                : FromFile(read);
        }

        return result.Verification.State switch
        {
            SourceLayerVerificationState.Unsafe => PhysicalBoundary(
                result.Layer.CanonicalPath,
                "The source layer no longer has a safely contained physical identity."),
            SourceLayerVerificationState.Unavailable => PhysicalBoundary(
                result.Layer.CanonicalPath,
                ReadFailureCause(
                    result.Verification.Failure,
                    "The source layer physical identity is unavailable.")),
            SourceLayerVerificationState.Changed => PhysicalBoundary(
                result.Layer.CanonicalPath,
                "The source layer physical identity changed after catalogue formation."),
            SourceLayerVerificationState.Cancelled => null,
            _ => throw new ArgumentOutOfRangeException(
                nameof(result),
                result.Verification.State,
                "The source layer verification state is not defined."),
        };
    }

    internal static RouteListFilesystemFinding DirectoryExpected(string canonicalLogicalSubject)
    {
        return ReadUnavailable(
            canonicalLogicalSubject,
            "The inventory root is not a directory.");
    }

    internal static RouteListFilesystemFinding MetadataMissing(string canonicalLogicalSubject)
    {
        return new RouteListFilesystemFinding(
            RouteListFindingCode.MetadataMissing,
            CliSemanticStatus.Attention,
            canonicalLogicalSubject,
            "Required source metadata is missing.");
    }

    internal static RouteListFilesystemFinding MetadataMalformed(string canonicalLogicalSubject)
    {
        return new RouteListFilesystemFinding(
            RouteListFindingCode.MetadataMalformed,
            CliSemanticStatus.Attention,
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

    private static RouteListFilesystemFinding ReadCandidateUnsafe(
        SourceCatalogueIssue issue,
        SourceCandidate? candidate)
    {
        return candidate?.PhysicalState == PhysicalPathState.Missing
            ? ReadUnavailable(issue.AttemptedCanonicalPath, "The filesystem candidate is unavailable.")
            : PhysicalBoundary(
                issue.AttemptedCanonicalPath,
                ReadPhysicalCause(candidate?.PhysicalState, issue.Failure));
    }

    private static string ReadPhysicalCause(
        PhysicalPathState? state,
        FilesystemFailure? failure)
    {
        if (failure is not null)
        {
            return ReadFailureCause(failure, "The physical path operation failed.");
        }

        return state switch
        {
            PhysicalPathState.Dangling => "The physical link target is unavailable.",
            PhysicalPathState.Cycle => "The physical link chain contains a cycle.",
            _ => "The physical path leaves the selected workspace.",
        };
    }

    private static FilesystemFailure ReadFailure<T>(FileReadResult<T> result)
    {
        return result.Failure
            ?? throw new InvalidOperationException("A failed file read requires a failure.");
    }

    private static string ReadFailureCause(FilesystemFailure? failure, string fallback)
    {
        if (failure is null)
        {
            return fallback;
        }

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
