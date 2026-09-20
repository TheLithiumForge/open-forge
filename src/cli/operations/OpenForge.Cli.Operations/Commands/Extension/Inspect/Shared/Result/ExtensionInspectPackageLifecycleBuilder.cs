using OpenForge.Cli.Core.Commands.Extension.Inspect.Models.Result;
using OpenForge.Cli.Core.Framework.Extensions.Models;
using OpenForge.Cli.Core.Framework.Ownership.Models.Observation;
using OpenForge.Cli.Core.Framework.Ownership.Models.Document;
using OpenForge.Cli.Core.Framework.Ownership;
using OpenForge.Cli.Core.Framework.Ownership.Shared.Observation;

namespace OpenForge.Cli.Core.Commands.Extension.Inspect.Shared.Result;

internal static class ExtensionInspectPackageLifecycleBuilder
{
    internal static ExtensionInspectLifecycle ReadLifecycle(WorkspaceOwnershipRead ownership)
        => new()
        {
            DocumentPath = WorkspaceOwnershipDefinitions.RelativePath,
            ReadState = ExtensionInspectResultMappings.ReadLifecycleState(ownership),
            Trust = !ownership.IsTrustworthy ? ExtensionInspectLifecycleTrust.Incomplete
                : ownership.Document.Extensions.IsEmpty ? ExtensionInspectLifecycleTrust.Absent
                : ExtensionInspectLifecycleTrust.Trusted,
            Coverage = ownership.IsTrustworthy ? ExtensionInspectCoverageState.Complete : ExtensionInspectCoverageState.Incomplete,
        };

    internal static ExtensionInspectInstalled ReadInstalled(
        WorkspaceOwnershipRead lifecycle,
        ExtensionOwnership? package)
    {
        var state = ReadInstalledState(lifecycle, package);
        return new ExtensionInspectInstalled
        {
            State = state,
            Package = ReadInstalledPackage(package),
        };
    }

    internal static ExtensionInspectAvailable ReadAvailable(
        ExtensionSourceReadResult source,
        ExtensionPackageFact? package,
        int sourceMatches)
    {
        var state = ReadAvailableState(source, package, sourceMatches);
        return new ExtensionInspectAvailable
        {
            State = state,
            Package = ReadAvailablePackage(package),
        };
    }

    internal static void AddBoundaryFindings(
        ExtensionInspectSubjectPackageInput input,
        ExtensionInspectSubjectPackageSelection selection,
        int sourceMatches)
    {
        var subjectId = input.Request.StableId;
        var source = input.Source;
        var lifecycle = input.Ownership;
        if (source.State != ExtensionSourceReadState.Complete)
        {
            var code = ReadSourceFailureFindingCode(source.FailureKind, source.State);
            ExtensionInspectFindingPolicy.Add(input.Findings, new ExtensionInspectFindingInput
            {
                Code = code,
                Subject = source.Identity,
                Cause = source.Cause ?? "The selected Extension source is unavailable.",
            });
        }

        if (!lifecycle.IsTrustworthy)
        {
            ExtensionInspectFindingPolicy.Add(input.Findings, new ExtensionInspectFindingInput
            {
                Code = ExtensionInspectFindingCode.OwnershipObservation,
                Subject = WorkspaceOwnershipDefinitions.RelativePath,
                Cause = lifecycle.Cause ?? "No Extension ownership is recorded; installed packages cannot be established.",
            });
        }

        if (source.State == ExtensionSourceReadState.Complete && sourceMatches > 1)
        {
            ExtensionInspectFindingPolicy.Add(input.Findings, new ExtensionInspectFindingInput
            {
                Code = ExtensionInspectFindingCode.IdentityAmbiguous,
                Subject = subjectId,
                PackageId = subjectId,
                Cause = "The selected source contains more than one active package identity for the supplied stable ID.",
                Candidates = ExtensionInspectSubjectSourceBuilder.ReadCandidates(source.Packages, subjectId),
            });
        }

        if (source.State == ExtensionSourceReadState.Complete && selection.AvailablePackage is null)
        {
            ExtensionInspectFindingPolicy.Add(input.Findings, new ExtensionInspectFindingInput
            {
                Code = ExtensionInspectFindingCode.PackageUnavailable,
                Subject = subjectId,
                PackageId = subjectId,
                Cause = "The requested package is not available from the selected source.",
            });
        }
    }

    internal static ExtensionInspectFindingCode ReadSourceFailureFindingCode(
        ExtensionSourceFailureKind failureKind,
        ExtensionSourceReadState sourceState)
        => failureKind switch
        {
            ExtensionSourceFailureKind.None => ReadFallbackSourceFindingCode(sourceState),
            ExtensionSourceFailureKind.Unavailable => ReadFallbackSourceFindingCode(sourceState),
            ExtensionSourceFailureKind.Invalid => ReadFallbackSourceFindingCode(sourceState),
            ExtensionSourceFailureKind.Overlap => ExtensionInspectFindingCode.SourceOverlap,
            ExtensionSourceFailureKind.Ambiguous => ExtensionInspectFindingCode.SourceAmbiguous,
            ExtensionSourceFailureKind.DependencyIncomplete => ExtensionInspectFindingCode.DependencyIncomplete,
            ExtensionSourceFailureKind.DependencyCycle => ExtensionInspectFindingCode.DependencyCycle,
            ExtensionSourceFailureKind.DependencyConflict => ExtensionInspectFindingCode.DependencyConflict,
            ExtensionSourceFailureKind.PackageUnavailable => ReadFallbackSourceFindingCode(sourceState),
            ExtensionSourceFailureKind.PackageInvalid => ExtensionInspectFindingCode.PackageInvalid,
            ExtensionSourceFailureKind.IdentityAmbiguous => ExtensionInspectFindingCode.IdentityAmbiguous,
            _ => throw new ArgumentOutOfRangeException(
                nameof(failureKind),
                failureKind,
                "The Extension source failure kind is not defined."),
        };

    private static ExtensionInspectInstalledState ReadInstalledState(
        WorkspaceOwnershipRead ownership, ExtensionOwnership? package)
        => package is not null ? ExtensionInspectInstalledState.Present
            : ownership.IsTrustworthy ? ExtensionInspectInstalledState.Absent
            : ExtensionInspectInstalledState.Unavailable;

    private static ExtensionInspectInstalledPackage? ReadInstalledPackage(
        ExtensionOwnership? package)
    {
        if (package is null)
        {
            return null;
        }

        return new ExtensionInspectInstalledPackage
        {
            Id = package.Id,
            Version = package.Version,
            Source = package.Source,
            Dependencies = package.Dependencies.ToArray(),
            Paths = package.Paths.Order(StringComparer.Ordinal).ToArray(),
        };
    }

    private static ExtensionInspectAvailableState ReadAvailableState(
        ExtensionSourceReadResult source,
        ExtensionPackageFact? package,
        int sourceMatches)
    {
        if (source.State != ExtensionSourceReadState.Complete)
        {
            return ExtensionInspectResultMappings.ReadAvailableState(source.State);
        }

        if (sourceMatches > 1)
        {
            return ExtensionInspectAvailableState.Blocked;
        }

        return package is null
            ? ExtensionInspectAvailableState.Absent
            : ExtensionInspectAvailableState.Present;
    }

    private static ExtensionInspectAvailablePackage? ReadAvailablePackage(
        ExtensionPackageFact? package)
    {
        if (package is null)
        {
            return null;
        }

        return new ExtensionInspectAvailablePackage
        {
            Id = package.Id,
            Name = package.Name,
            Description = package.Description,
            Version = package.Version,
            ManifestPath = package.ManifestPath,
            Dependencies = package.Dependencies.ToArray(),
            Payload = package.Payload
                .Select(file => new ExtensionInspectPackageFile
                {
                    Path = file.Path,
                    TargetPath = file.TargetPath,
                    State = ExtensionInspectResultMappings.ReadPackageFileState(file.State),
                    ByteLength = file.ByteLength,
                    Sha256 = file.Sha256,
                    Bytes = file.Bytes,
                })
                .ToArray(),
        };
    }

    private static ExtensionInspectFindingCode ReadFallbackSourceFindingCode(
        ExtensionSourceReadState state)
        => state is ExtensionSourceReadState.Missing or ExtensionSourceReadState.Unavailable
            ? ExtensionInspectFindingCode.SourceUnavailable
            : ExtensionInspectFindingCode.SourceInvalid;
}
