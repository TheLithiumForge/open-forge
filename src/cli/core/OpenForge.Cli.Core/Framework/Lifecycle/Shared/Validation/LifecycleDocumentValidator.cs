using System.Security.Cryptography;
using OpenForge.Cli.Core.Framework.Extensions.Identity;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Filesystem.Shared.Paths;
using OpenForge.Cli.Core.Framework.Lifecycle.Models.Document;
using OpenForge.Cli.Core.Framework.Lifecycle.Models.Reading;
using OpenForge.Cli.Core.Framework.Lifecycle.Shared.Validation.Models;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Files;
using OpenForge.Cli.Core.Framework.Workspace.Models;

namespace OpenForge.Cli.Core.Framework.Lifecycle.Shared.Validation;

internal static class LifecycleDocumentValidator
{
    internal static void ValidateSnapshotPath(
        CliWorkspace workspace,
        FileStateSnapshot file)
    {
        var canonicalPath = FileExpectation.NormalizeAbsolutePath(
            Path.Combine(workspace.LexicalRoot, LifecycleSchema.RelativePath),
            nameof(file));
        if (!string.Equals(file.LogicalPath, canonicalPath, PathComparison()))
        {
            throw new ArgumentException(
                "A lifecycle file snapshot must use the canonical workspace lifecycle path.",
                nameof(file));
        }

        if (file.PhysicalPath is { } physicalPath
            && !PhysicalContainment.Contains(workspace.PhysicalRoot, physicalPath))
        {
            throw new ArgumentException(
                "A lifecycle file snapshot physical path must be contained by the workspace.",
                nameof(file));
        }
    }

    internal static LifecycleReadResult ValidateExtensions(
        CliWorkspace workspace,
        LifecycleEnvelopeV1 document,
        ExtensionLifecycleState? extensions)
    {
        var common = ValidateCommon(workspace, document);
        if (common.State == LifecycleCommonValidationState.Invalid)
        {
            return InvalidCoverage(
                common.Cause ?? "The lifecycle common envelope is invalid.",
                common.WorkspaceBinding,
                document.FingerprintPolicy);
        }

        if (common.State == LifecycleCommonValidationState.Blocked)
        {
            return Blocked(
                common.Cause ?? "The lifecycle common envelope is blocked.",
                workspaceBinding: common.WorkspaceBinding,
                fingerprintPolicy: document.FingerprintPolicy);
        }

        if (extensions is null)
        {
            return LifecycleReadResult.Create(
                state: LifecycleReadState.Missing,
                trust: LifecycleExtensionTrust.Incomplete,
                packages: [],
                cause: "The lifecycle Extension section is missing.",
                coverageFacts: new LifecycleCoverageFacts
                {
                    Paths = [],
                    Coverage = LifecycleCoverageState.Incomplete,
                    WorkspaceBinding = LifecycleWorkspaceBinding.Matched,
                    FingerprintPolicy = document.FingerprintPolicy,
                });
        }

        return ValidateExtensionState(extensions, document.FingerprintPolicy);
    }

    internal static LifecycleCommonValidation ValidateCommon(
        CliWorkspace workspace,
        LifecycleEnvelopeV1 document)
    {
        ArgumentNullException.ThrowIfNull(workspace);
        ArgumentNullException.ThrowIfNull(document);
        if (document.SchemaVersion != LifecycleSchema.Version)
        {
            return LifecycleCommonValidation.Invalid(
                "The lifecycle schema version is unsupported.");
        }

        if (!string.Equals(
            document.FingerprintPolicy,
            LifecycleSchema.FingerprintPolicy,
            StringComparison.Ordinal))
        {
            return LifecycleCommonValidation.Invalid(
                "The lifecycle fingerprint policy is unsupported.");
        }

        if (string.IsNullOrWhiteSpace(document.WorkspacePath))
        {
            return LifecycleCommonValidation.Blocked(
                "The lifecycle workspace binding is invalid.",
                LifecycleWorkspaceBinding.Unavailable);
        }

        string documentWorkspace;
        try
        {
            if (!Path.IsPathFullyQualified(document.WorkspacePath))
            {
                return LifecycleCommonValidation.Blocked(
                    "The lifecycle workspace binding is not a canonical absolute path.",
                    LifecycleWorkspaceBinding.Unavailable);
            }

            documentWorkspace = NormalizeRoot(document.WorkspacePath);
        }
        catch (Exception exception) when (exception is ArgumentException
            or NotSupportedException
            or PathTooLongException)
        {
            return LifecycleCommonValidation.Blocked(
                "The lifecycle workspace binding is invalid.",
                LifecycleWorkspaceBinding.Unavailable);
        }

        return !string.Equals(document.WorkspacePath, documentWorkspace, StringComparison.Ordinal)
            || !string.Equals(
                documentWorkspace,
                NormalizeRoot(workspace.LexicalRoot),
                PathComparison())
            ? LifecycleCommonValidation.Blocked(
                "The lifecycle workspace binding does not match the selected workspace.",
                LifecycleWorkspaceBinding.Mismatched)
            : LifecycleCommonValidation.Valid();
    }

    private static LifecycleReadResult ValidateExtensionState(
        ExtensionLifecycleState extensions,
        string fingerprintPolicy)
    {
        if (extensions.Packages is null || extensions.Paths is null)
        {
            return InvalidCoverage("The lifecycle Extension coverage is incomplete.", LifecycleWorkspaceBinding.Matched, fingerprintPolicy);
        }

        var packages = new List<LifecycleInstalledPackage>();
        var packageById = new Dictionary<string, LifecycleExtensionPackageV1>(StringComparer.Ordinal);
        var dependenciesById = new Dictionary<string, string[]>(StringComparer.Ordinal);
        string? previousPackageId = null;
        foreach (var package in extensions.Packages)
        {
            if (package is null
                || !ExtensionIdentity.IsValidStableId(package.Id)
                || package.Dependencies is null
                || package.Paths is null)
            {
                return Blocked("A lifecycle package identity is malformed.", packages, workspaceBinding: LifecycleWorkspaceBinding.Matched, fingerprintPolicy: fingerprintPolicy);
            }

            if (previousPackageId is not null
                && string.CompareOrdinal(previousPackageId, package.Id) >= 0)
            {
                return Blocked("Lifecycle package identities are duplicated or not in stable order.",
                    packages, workspaceBinding: LifecycleWorkspaceBinding.Matched, fingerprintPolicy: fingerprintPolicy);
            }

            if (!packageById.TryAdd(package.Id, package)
                || HasInvalidIds(package.Dependencies)
                || !IsDistinctOrdered(package.Dependencies)
                || !TryNormalizePaths(package.Paths, out var normalizedPaths))
            {
                return Blocked("A lifecycle package contains ambiguous dependency or path identity.",
                    packages, workspaceBinding: LifecycleWorkspaceBinding.Matched, fingerprintPolicy: fingerprintPolicy);
            }

            previousPackageId = package.Id;
            dependenciesById.Add(package.Id, package.Dependencies);
            packages.Add(new LifecycleInstalledPackage
            {
                Id = package.Id,
                Version = package.Version,
                Source = package.Source,
                Dependencies = Array.AsReadOnly(package.Dependencies.ToArray()),
                Paths = Array.AsReadOnly(normalizedPaths),
            });
        }

        var pathByIdentity = new Dictionary<string, LifecycleExtensionPathV1>(StringComparer.Ordinal);
        string? previousPath = null;
        foreach (var path in extensions.Paths)
        {
            if (path is null
                || !PortableWorkspacePath.TryNormalize(path.Path, out var normalizedPath)
                || path.Owners is null
                || path.Owners.Length == 0
                || HasInvalidIds(path.Owners)
                || !IsDistinctOrdered(path.Owners)
                || string.IsNullOrWhiteSpace(path.BaselineFingerprint)
                || !IsLowerHexSha256(path.BaselineFingerprint)
                || path.FingerprintKind is not (LifecycleSchema.SemanticFingerprintKind or LifecycleSchema.ExactBytesFingerprintKind))
            {
                return Blocked("A lifecycle path identity or fingerprint is malformed.", packages, workspaceBinding: LifecycleWorkspaceBinding.Matched, fingerprintPolicy: fingerprintPolicy);
            }

            if ((previousPath is not null && string.CompareOrdinal(previousPath, normalizedPath) >= 0)
                || !pathByIdentity.TryAdd(PortableWorkspacePath.CreatePortableKey(normalizedPath), path))
            {
                return Blocked("Lifecycle path identities are duplicated or not in stable order.", packages, workspaceBinding: LifecycleWorkspaceBinding.Matched, fingerprintPolicy: fingerprintPolicy);
            }

            previousPath = normalizedPath;
        }

        foreach (var package in packageById.Values)
        {
            if (package.Dependencies.Contains(package.Id, StringComparer.Ordinal)
                || package.Dependencies.Any(dependency => !packageById.ContainsKey(dependency))
                || package.Paths.Any(path => !pathByIdentity.TryGetValue(
                        PortableWorkspacePath.CreatePortableKey(path),
                        out var record)
                    || !string.Equals(path, record.Path, StringComparison.Ordinal)
                    || !record.Owners.Contains(package.Id, StringComparer.Ordinal)))
            {
                return Blocked("Lifecycle package dependency or path ownership is not reciprocal.",
                    packages, workspaceBinding: LifecycleWorkspaceBinding.Matched, fingerprintPolicy: fingerprintPolicy);
            }
        }

        foreach (var record in pathByIdentity.Values)
        {
            if (record.Owners.Any(owner => !packageById.TryGetValue(owner, out var package)
                || !package.Paths.Contains(record.Path, StringComparer.Ordinal)))
            {
                return Blocked("Lifecycle path ownership is not reciprocal.", packages, workspaceBinding: LifecycleWorkspaceBinding.Matched, fingerprintPolicy: fingerprintPolicy);
            }
        }

        if (HasDependencyCycle(dependenciesById))
        {
            return Blocked("The lifecycle package dependency graph contains a cycle.", packages, workspaceBinding: LifecycleWorkspaceBinding.Matched, fingerprintPolicy: fingerprintPolicy);
        }

        if (!string.Equals(extensions.Coverage, LifecycleSchema.CompleteCoverage, StringComparison.Ordinal))
        {
            return LifecycleReadResult.Create(
                state: LifecycleReadState.Invalid,
                trust: LifecycleExtensionTrust.Untrusted,
                packages: packages,
                cause: "The lifecycle Extension coverage is incomplete.",
                coverageFacts: new LifecycleCoverageFacts
                {
                    Paths = ReadPaths(pathByIdentity),
                    Coverage = LifecycleCoverageState.Incomplete,
                    WorkspaceBinding = LifecycleWorkspaceBinding.Matched,
                    FingerprintPolicy = fingerprintPolicy,
                });
        }

        return LifecycleReadResult.Create(
            state: LifecycleReadState.Complete,
            trust: packages.Count == 0 ? LifecycleExtensionTrust.Absent : LifecycleExtensionTrust.Trusted,
            packages: packages,
            cause: null,
            coverageFacts: new LifecycleCoverageFacts
            {
                Paths = ReadPaths(pathByIdentity),
                Coverage = LifecycleCoverageState.Complete,
                WorkspaceBinding = LifecycleWorkspaceBinding.Matched,
                FingerprintPolicy = fingerprintPolicy,
            });
    }

    private static IReadOnlyList<LifecycleInstalledPath> ReadPaths(
        IReadOnlyDictionary<string, LifecycleExtensionPathV1> paths)
        => [.. paths.Values
            .OrderBy(path => path.Path, StringComparer.Ordinal)
            .Select(path => new LifecycleInstalledPath
            {
                Path = path.Path,
                Owners = Array.AsReadOnly(path.Owners.ToArray()),
                BaselineFingerprint = path.BaselineFingerprint,
                FingerprintKind = path.FingerprintKind,
            })];

    private static bool TryNormalizePaths(string[] paths, out string[] normalized)
    {
        normalized = new string[paths.Length];
        for (var index = 0; index < paths.Length; index++)
        {
            if (!PortableWorkspacePath.TryNormalize(paths[index], out normalized[index]))
            {
                return false;
            }
        }

        return IsDistinctOrdered(normalized)
            && normalized.Select(PortableWorkspacePath.CreatePortableKey).Distinct(StringComparer.Ordinal).Count()
                == normalized.Length;
    }

    private static bool HasInvalidIds(IEnumerable<string> values)
        => values.Any(value => !ExtensionIdentity.IsValidStableId(value));

    private static bool IsDistinctOrdered(string[] values)
    {
        for (var index = 1; index < values.Length; index++)
        {
            if (string.CompareOrdinal(values[index - 1], values[index]) >= 0)
            {
                return false;
            }
        }

        return true;
    }

    private static bool IsLowerHexSha256(string value)
        => value.Length == SHA256.HashSizeInBytes * 2
            && value.All(character => character is >= '0' and <= '9' or >= 'a' and <= 'f');

    private static bool HasDependencyCycle(
        IReadOnlyDictionary<string, string[]> dependenciesById)
    {
        var completed = new HashSet<string>(StringComparer.Ordinal);
        var active = new HashSet<string>(StringComparer.Ordinal);
        foreach (var packageId in dependenciesById.Keys)
        {
            if (Visit(packageId, dependenciesById, completed, active))
            {
                return true;
            }
        }

        return false;
    }

    private static bool Visit(
        string packageId,
        IReadOnlyDictionary<string, string[]> dependenciesById,
        ISet<string> completed,
        ISet<string> active)
    {
        if (completed.Contains(packageId))
        {
            return false;
        }

        if (!active.Add(packageId))
        {
            return true;
        }

        foreach (var dependency in dependenciesById[packageId])
        {
            if (Visit(dependency, dependenciesById, completed, active))
            {
                return true;
            }
        }

        active.Remove(packageId);
        completed.Add(packageId);
        return false;
    }

    private static string NormalizeRoot(string path)
        => Path.TrimEndingDirectorySeparator(Path.GetFullPath(path));

    private static StringComparison PathComparison()
        => OperatingSystem.IsWindows() ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;

    private static LifecycleReadResult InvalidCoverage(
        string cause,
        LifecycleWorkspaceBinding workspaceBinding = LifecycleWorkspaceBinding.NotChecked,
        string? fingerprintPolicy = null)
        => LifecycleReadResult.Create(
            state: LifecycleReadState.Invalid,
            trust: LifecycleExtensionTrust.Incomplete,
            packages: [],
            cause: cause,
            coverageFacts: new LifecycleCoverageFacts
            {
                Paths = [],
                Coverage = LifecycleCoverageState.Incomplete,
                WorkspaceBinding = workspaceBinding,
                FingerprintPolicy = fingerprintPolicy,
            });

    private static LifecycleReadResult Blocked(
        string cause,
        IEnumerable<LifecycleInstalledPackage>? packages = null,
        IEnumerable<LifecycleInstalledPath>? paths = null,
        LifecycleWorkspaceBinding workspaceBinding = LifecycleWorkspaceBinding.NotChecked,
        string? fingerprintPolicy = null)
        => LifecycleReadResult.Create(
            state: LifecycleReadState.Invalid,
            trust: LifecycleExtensionTrust.Blocked,
            packages: packages ?? [],
            cause: cause,
            coverageFacts: new LifecycleCoverageFacts
            {
                Paths = [.. (paths ?? [])],
                Coverage = LifecycleCoverageState.Blocked,
                WorkspaceBinding = workspaceBinding,
                FingerprintPolicy = fingerprintPolicy,
            });
}
