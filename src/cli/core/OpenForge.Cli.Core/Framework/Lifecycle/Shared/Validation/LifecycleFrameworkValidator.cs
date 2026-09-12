using OpenForge.Cli.Core.Framework.Filesystem.Shared.Paths;
using System.Security.Cryptography;
using OpenForge.Cli.Core.Framework.Extensions.Identity;
using OpenForge.Cli.Core.Framework.Lifecycle.Models.Document;
using OpenForge.Cli.Core.Framework.Lifecycle.Shared.Validation.Models;

namespace OpenForge.Cli.Core.Framework.Lifecycle.Shared.Validation;

internal static class LifecycleFrameworkValidator
{
    internal static LifecycleSectionValidation Validate(FrameworkLifecycleState framework)
    {
        ArgumentNullException.ThrowIfNull(framework);
        if (!string.Equals(
                framework.Coverage,
                LifecycleSchema.CompleteCoverage,
                StringComparison.Ordinal)
            || framework.Source is null
            || framework.Targets is null
            || framework.GeneratedRegions is null)
        {
            return LifecycleSectionValidation.Blocked(
                "The lifecycle Framework coverage is incomplete.");
        }

        if (!ExtensionIdentity.IsValidStableId(framework.Source.Id)
            || framework.Source.Version is not null
                && string.IsNullOrWhiteSpace(framework.Source.Version)
            || !IsLowerHexSha256(framework.Source.InventoryFingerprint))
        {
            return LifecycleSectionValidation.Blocked(
                "The lifecycle Framework source identity is malformed.");
        }

        if (framework.Targets.Length == 0)
        {
            return LifecycleSectionValidation.Blocked(
                "Complete lifecycle Framework coverage requires managed targets.");
        }

        var targetKeys = new HashSet<(string Path, string? Region)>();
        var portableTargetKeys = new HashSet<(string Path, string? Region)>();
        (string Path, string? Region)? previousTarget = null;
        foreach (var target in framework.Targets)
        {
            if (target is null
                || !TryNormalizePath(target.Path, out var path)
                || !string.Equals(path, target.Path, StringComparison.Ordinal)
                || !IsValidRegion(target.Region)
                || !IsLowerHexSha256(target.BaselineFingerprint)
                || target.FingerprintKind is not (
                    LifecycleSchema.SemanticFingerprintKind
                    or LifecycleSchema.ExactBytesFingerprintKind))
            {
                return LifecycleSectionValidation.Blocked(
                    "A lifecycle Framework target identity is malformed.");
            }

            if (target.SourceAssetPath is { } sourceAssetPath
                && (!TryNormalizePath(sourceAssetPath, out var normalizedSourceAssetPath)
                    || !string.Equals(
                        normalizedSourceAssetPath,
                        sourceAssetPath,
                        StringComparison.Ordinal)))
            {
                return LifecycleSectionValidation.Blocked(
                    "A lifecycle Framework source-asset path is malformed.");
            }

            var key = (path, target.Region);
            if (previousTarget is { } previous && Compare(previous, key) >= 0
                || !targetKeys.Add(key)
                || !portableTargetKeys.Add((
                    PortableWorkspacePath.CreatePortableKey(path),
                    target.Region)))
            {
                return LifecycleSectionValidation.Blocked(
                    "Lifecycle Framework targets are duplicated or not in stable order.");
            }

            previousTarget = key;
        }

        var generatedKeys = new HashSet<(string Path, string? Region)>();
        var portableGeneratedKeys = new HashSet<(string Path, string? Region)>();
        (string Path, string? Region)? previousGenerated = null;
        foreach (var generated in framework.GeneratedRegions)
        {
            if (generated is null
                || !TryNormalizePath(generated.Path, out var path)
                || !string.Equals(path, generated.Path, StringComparison.Ordinal)
                || string.IsNullOrWhiteSpace(generated.Region))
            {
                return LifecycleSectionValidation.Blocked(
                    "A lifecycle Framework generated-region identity is malformed.");
            }

            var key = (path, generated.Region);
            if (previousGenerated is { } previous && Compare(previous, key) >= 0
                || !generatedKeys.Add(key)
                || !portableGeneratedKeys.Add((
                    PortableWorkspacePath.CreatePortableKey(path),
                    generated.Region))
                || !targetKeys.Contains(key))
            {
                return LifecycleSectionValidation.Blocked(
                    "Lifecycle Framework generated regions must be unique, ordered, and managed targets.");
            }

            previousGenerated = key;
        }

        foreach (var target in framework.Targets)
        {
            var isGeneratedTarget = generatedKeys.Contains((target.Path, target.Region));
            if (isGeneratedTarget != (target.SourceAssetPath is null))
            {
                return LifecycleSectionValidation.Blocked(
                    "Lifecycle Framework generated targets require null source provenance, and all other managed targets require source provenance.");
            }
        }

        return LifecycleSectionValidation.Valid();
    }

    internal static LifecycleSectionValidation ValidateNoCrossSectionCollisions(
        FrameworkLifecycleState? framework,
        ExtensionLifecycleState? extensions)
    {
        if (framework is null || extensions is null)
        {
            return LifecycleSectionValidation.Valid();
        }

        var frameworkPaths = framework.Targets
            .Select(target => PortableWorkspacePath.CreatePortableKey(target.Path))
            .Concat(framework.GeneratedRegions.Select(
                region => PortableWorkspacePath.CreatePortableKey(region.Path)))
            .ToHashSet(StringComparer.Ordinal);
        return extensions.Paths.Any(path => frameworkPaths.Contains(
                PortableWorkspacePath.CreatePortableKey(path.Path)))
            ? LifecycleSectionValidation.Blocked(
                "Framework and Extension lifecycle sections cannot claim the same path.")
            : LifecycleSectionValidation.Valid();
    }

    private static bool TryNormalizePath(string path, out string normalized)
        => PortableWorkspacePath.TryNormalize(path, out normalized);

    private static bool IsValidRegion(string? region)
        => region is null || !string.IsNullOrWhiteSpace(region);

    private static bool IsLowerHexSha256(string? value)
        => value is not null
            && value.Length == SHA256.HashSizeInBytes * 2
            && value.All(character => character is >= '0' and <= '9' or >= 'a' and <= 'f');

    private static int Compare(
        (string Path, string? Region) left,
        (string Path, string? Region) right)
    {
        var path = string.CompareOrdinal(left.Path, right.Path);
        return path != 0
            ? path
            : string.CompareOrdinal(left.Region, right.Region);
    }
}
