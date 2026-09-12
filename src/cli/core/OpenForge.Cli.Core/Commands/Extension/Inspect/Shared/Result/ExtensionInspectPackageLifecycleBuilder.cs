using OpenForge.Cli.Core.Commands.Extension.Inspect.Models.Result;
using OpenForge.Cli.Core.Framework.Extensions.Models;
using OpenForge.Cli.Core.Framework.Lifecycle.Models.Reading;
using OpenForge.Cli.Core.Framework.Lifecycle;

namespace OpenForge.Cli.Core.Commands.Extension.Inspect.Shared.Result;

internal static class ExtensionInspectPackageLifecycleBuilder
{
    internal static ExtensionInspectLifecycle ReadLifecycle(LifecycleReadResult lifecycle)
        => new()
        {
            DocumentPath = LifecycleSchema.RelativePath,
            ReadState = ExtensionInspectResultMappings.ReadLifecycleState(lifecycle),
            Trust = ExtensionInspectResultMappings.ReadLifecycleTrust(lifecycle.Trust),
            Coverage = ExtensionInspectResultMappings.ReadCoverage(lifecycle.Coverage),
            WorkspaceBinding = ExtensionInspectResultMappings.ReadWorkspaceBinding(lifecycle.WorkspaceBinding),
            FingerprintPolicy = lifecycle.FingerprintPolicy == ExtensionInspectDefinitions.FingerprintPolicy
                ? lifecycle.FingerprintPolicy
                : null,
        };

    internal static ExtensionInspectInstalled ReadInstalled(
        LifecycleReadResult lifecycle,
        LifecycleInstalledPackage? package)
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
        var lifecycle = input.Lifecycle;
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

        if (lifecycle.State == LifecycleReadState.Cancelled)
        {
            ExtensionInspectFindingPolicy.Add(input.Findings, new ExtensionInspectFindingInput
            {
                Code = ExtensionInspectFindingCode.Interrupted,
                Subject = LifecycleSchema.RelativePath,
                Cause = lifecycle.Cause ?? "Lifecycle inspection was interrupted.",
            });
        }
        else if (lifecycle.Trust == LifecycleExtensionTrust.Blocked)
        {
            ExtensionInspectFindingPolicy.Add(input.Findings, new ExtensionInspectFindingInput
            {
                Code = ExtensionInspectFindingCode.LifecycleBlocked,
                Subject = LifecycleSchema.RelativePath,
                Cause = lifecycle.Cause ?? "Extension lifecycle evidence is blocked.",
            });
        }
        else if (lifecycle.State is LifecycleReadState.Missing or LifecycleReadState.Unavailable)
        {
            ExtensionInspectFindingPolicy.Add(input.Findings, new ExtensionInspectFindingInput
            {
                Code = ExtensionInspectFindingCode.LifecycleUnavailable,
                Subject = LifecycleSchema.RelativePath,
                Cause = lifecycle.Cause ?? "Extension lifecycle evidence is unavailable.",
            });
        }
        else if (lifecycle.State == LifecycleReadState.Invalid)
        {
            ExtensionInspectFindingPolicy.Add(input.Findings, new ExtensionInspectFindingInput
            {
                Code = ExtensionInspectFindingCode.LifecycleInvalid,
                Subject = LifecycleSchema.RelativePath,
                Cause = lifecycle.Cause ?? "Extension lifecycle evidence is invalid.",
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
        LifecycleReadResult lifecycle,
        LifecycleInstalledPackage? package)
    {
        if (package is not null)
        {
            return ExtensionInspectInstalledState.Present;
        }

        if (lifecycle.Trust is LifecycleExtensionTrust.Absent
            && lifecycle.Coverage == LifecycleCoverageState.Complete)
        {
            return ExtensionInspectInstalledState.Absent;
        }

        if (lifecycle.State == LifecycleReadState.Cancelled)
        {
            return ExtensionInspectInstalledState.Interrupted;
        }

        if (lifecycle.Trust == LifecycleExtensionTrust.Blocked)
        {
            return ExtensionInspectInstalledState.Blocked;
        }

        if (lifecycle.State == LifecycleReadState.Invalid)
        {
            return ExtensionInspectInstalledState.Invalid;
        }

        return ExtensionInspectInstalledState.Unavailable;
    }

    private static ExtensionInspectInstalledPackage? ReadInstalledPackage(
        LifecycleInstalledPackage? package)
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
