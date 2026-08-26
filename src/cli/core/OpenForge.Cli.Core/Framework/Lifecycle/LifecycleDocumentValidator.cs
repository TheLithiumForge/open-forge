using System.Security.Cryptography;
using OpenForge.Cli.Core.Framework.Extensions.Identity;
using OpenForge.Cli.Core.Framework.Lifecycle.Models;
using OpenForge.Cli.Core.Framework.Workspace;

namespace OpenForge.Cli.Core.Framework.Lifecycle;

internal static class LifecycleDocumentValidator
{
    internal static LifecycleReadResult Validate(
        CliWorkspace workspace,
        LifecycleDocumentV1 document)
    {
        if (document.SchemaVersion != 1)
        {
            return InvalidCoverage("The lifecycle schema version is unsupported.");
        }

        if (!string.Equals(document.FingerprintPolicy, LifecycleDocumentReader.FingerprintPolicy, StringComparison.Ordinal))
        {
            return InvalidCoverage("The lifecycle fingerprint policy is unsupported.");
        }

        if (string.IsNullOrWhiteSpace(document.WorkspacePath))
        {
            return Blocked("The lifecycle workspace binding is invalid.");
        }

        string documentWorkspace;
        try
        {
            if (!Path.IsPathFullyQualified(document.WorkspacePath))
            {
                return Blocked("The lifecycle workspace binding is not a canonical absolute path.");
            }

            documentWorkspace = NormalizeRoot(document.WorkspacePath);
        }
        catch (Exception exception) when (exception is ArgumentException or NotSupportedException or PathTooLongException)
        {
            return Blocked("The lifecycle workspace binding is invalid.");
        }

        if (!string.Equals(document.WorkspacePath, documentWorkspace, StringComparison.Ordinal)
            || !string.Equals(documentWorkspace, NormalizeRoot(workspace.LexicalRoot), PathComparison()))
        {
            return Blocked("The lifecycle workspace binding does not match the selected workspace.");
        }

        if (document.Extensions is null)
        {
            return InvalidCoverage("The lifecycle Extension section is missing.");
        }

        return ValidateExtensions(document.Extensions);
    }

    private static LifecycleReadResult ValidateExtensions(LifecycleExtensionsSectionV1 extensions)
    {
        if (extensions.Packages is null || extensions.Paths is null)
        {
            return InvalidCoverage("The lifecycle Extension coverage is incomplete.");
        }

        var packages = new List<LifecycleInstalledPackage>();
        var packageById = new Dictionary<string, LifecycleExtensionPackageV1>(StringComparer.Ordinal);
        var dependenciesById = new Dictionary<string, string[]>(StringComparer.Ordinal);
        string? previousPackageId = null;
        foreach (var package in extensions.Packages)
        {
            if (package is null
                || !ExtensionIdentity.IsValidStableId(package.Id)
                || string.IsNullOrWhiteSpace(package.Source)
                || package.Dependencies is null
                || package.Paths is null)
            {
                return Blocked("A lifecycle package identity is malformed.", packages);
            }

            if (previousPackageId is not null
                && string.CompareOrdinal(previousPackageId, package.Id) >= 0)
            {
                return Blocked("Lifecycle package identities are duplicated or not in stable order.", packages);
            }

            if (!packageById.TryAdd(package.Id, package)
                || HasInvalidIds(package.Dependencies)
                || !IsDistinctOrdered(package.Dependencies)
                || !TryNormalizePaths(package.Paths, out var normalizedPaths))
            {
                return Blocked("A lifecycle package contains ambiguous dependency or path identity.", packages);
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
                || !ExtensionTargetPath.TryNormalize(path.Path, out var normalizedPath)
                || path.Owners is null
                || path.Owners.Length == 0
                || HasInvalidIds(path.Owners)
                || !IsDistinctOrdered(path.Owners)
                || string.IsNullOrWhiteSpace(path.BaselineFingerprint)
                || !IsLowerHexSha256(path.BaselineFingerprint)
                || path.FingerprintKind is not ("semantic" or "exact-bytes"))
            {
                return Blocked("A lifecycle path identity or fingerprint is malformed.", packages);
            }

            if ((previousPath is not null && string.CompareOrdinal(previousPath, normalizedPath) >= 0)
                || !pathByIdentity.TryAdd(ExtensionTargetPath.CreatePortableKey(normalizedPath), path))
            {
                return Blocked("Lifecycle path identities are duplicated or not in stable order.", packages);
            }

            previousPath = normalizedPath;
        }

        foreach (var package in packageById.Values)
        {
            if (package.Dependencies.Contains(package.Id, StringComparer.Ordinal)
                || package.Dependencies.Any(dependency => !packageById.ContainsKey(dependency))
                || package.Paths.Any(path => !pathByIdentity.TryGetValue(
                        ExtensionTargetPath.CreatePortableKey(path),
                        out var record)
                    || !string.Equals(path, record.Path, StringComparison.Ordinal)
                    || !record.Owners.Contains(package.Id, StringComparer.Ordinal)))
            {
                return Blocked("Lifecycle package dependency or path ownership is not reciprocal.", packages);
            }
        }

        foreach (var record in pathByIdentity.Values)
        {
            if (record.Owners.Any(owner => !packageById.TryGetValue(owner, out var package)
                || !package.Paths.Contains(record.Path, StringComparer.Ordinal)))
            {
                return Blocked("Lifecycle path ownership is not reciprocal.", packages);
            }
        }

        if (HasDependencyCycle(dependenciesById))
        {
            return Blocked("The lifecycle package dependency graph contains a cycle.", packages);
        }

        if (!string.Equals(extensions.Coverage, "complete", StringComparison.Ordinal))
        {
            return new(
                state: LifecycleReadState.Invalid,
                trust: LifecycleExtensionTrust.Untrusted,
                packages: packages,
                cause: "The lifecycle Extension coverage is incomplete.");
        }

        return new(
            state: LifecycleReadState.Complete,
            trust: packages.Count == 0 ? LifecycleExtensionTrust.Absent : LifecycleExtensionTrust.Trusted,
            packages: packages,
            cause: null);
    }

    private static bool TryNormalizePaths(string[] paths, out string[] normalized)
    {
        normalized = new string[paths.Length];
        for (var index = 0; index < paths.Length; index++)
        {
            if (!ExtensionTargetPath.TryNormalize(paths[index], out normalized[index]))
            {
                return false;
            }
        }

        return IsDistinctOrdered(normalized)
            && normalized.Select(ExtensionTargetPath.CreatePortableKey).Distinct(StringComparer.Ordinal).Count()
                == normalized.Length;
    }

    private static bool HasInvalidIds(IEnumerable<string> values)
        => values.Any(value => !ExtensionIdentity.IsValidStableId(value));

    private static bool IsDistinctOrdered(IReadOnlyList<string> values)
    {
        for (var index = 1; index < values.Count; index++)
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

    private static LifecycleReadResult InvalidCoverage(string cause)
        => new(
            state: LifecycleReadState.Invalid,
            trust: LifecycleExtensionTrust.Incomplete,
            packages: [],
            cause: cause);

    private static LifecycleReadResult Blocked(
        string cause,
        IEnumerable<LifecycleInstalledPackage>? packages = null)
        => new(
            state: LifecycleReadState.Invalid,
            trust: LifecycleExtensionTrust.Blocked,
            packages: packages ?? [],
            cause: cause);
}
