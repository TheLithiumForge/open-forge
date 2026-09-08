using OpenForge.Cli.Core.Commands.Extension.Shared.Permissions;
using System.Security.Cryptography;
using OpenForge.Cli.Core.Commands.Extension.Inspect.Models.Result;
using OpenForge.Cli.Core.Framework.Documents.Markdown;
using OpenForge.Cli.Core.Framework.Documents.Markdown.Models;
using OpenForge.Cli.Core.Framework.Extensions.Models;
using OpenForge.Cli.Core.Framework.Lifecycle;
using OpenForge.Cli.Core.Framework.Lifecycle.Models;

namespace OpenForge.Cli.Core.Commands.Extension.Inspect.Shared.Result;

internal sealed class ExtensionInspectFingerprintBuilder(MarkdownFingerprintReader fingerprintReader)
{
    private readonly MarkdownFingerprintReader _fingerprintReader = fingerprintReader;

    internal static IReadOnlyList<LifecycleInstalledPath> ReadBaselineRecords(
        LifecycleReadResult lifecycle,
        LifecycleInstalledPackage? package)
    {
        if (package is null || lifecycle.Trust is not (LifecycleExtensionTrust.Trusted or LifecycleExtensionTrust.Untrusted))
        {
            return [];
        }

        var paths = ExtensionInspectInstalledClosureReader.Read(lifecycle.Packages, package)
            .SelectMany(value => value.Paths)
            .ToHashSet(StringComparer.Ordinal);
        return [.. lifecycle.Paths
            .Where(path => paths.Contains(path.Path))
            .OrderBy(path => path.Path, StringComparer.Ordinal)];
    }

    internal static IReadOnlyList<ExtensionInspectFingerprintFact> ReadBaselineFingerprints(
        IReadOnlyList<LifecycleInstalledPath> records)
        => [.. records
            .Select(record => new ExtensionInspectFingerprintFact
            {
                Path = record.Path,
                Fingerprint = new ExtensionInspectFingerprint
                {
                    Kind = record.FingerprintKind == "semantic"
                        ? ExtensionInspectFingerprintKind.Semantic
                        : ExtensionInspectFingerprintKind.ExactBytes,
                    Policy = ExtensionInspectDefinitions.FingerprintPolicy,
                    Sha256 = record.BaselineFingerprint,
                    Origin = ExtensionInspectFingerprintOrigin.PersistedBaseline,
                },
            })
            .OrderBy(fact => fact.Path, StringComparer.Ordinal)];

    internal IReadOnlyList<ExtensionInspectFingerprintFact> ReadCurrentFingerprints(
        IReadOnlyList<ExtensionInspectCurrentPath> paths,
        ICollection<ExtensionInspectFinding> findings,
        IDictionary<string, MarkdownFingerprintFacts> markdownFacts)
    {
        var values = new List<ExtensionInspectFingerprintFact>();
        foreach (var path in paths.Where(path => path.State == ExtensionInspectCurrentPathState.Present))
        {
            if (path.Bytes is not { } bytes || path.ExactSha256 is null)
            {
                ExtensionInspectFindingPolicy.Add(findings, new ExtensionInspectFindingInput
                {
                    Code = ExtensionInspectFindingCode.FingerprintUnavailable,
                    Path = path.Path,
                    Cause = "Current bytes were not retained for fingerprinting.",
                });
                continue;
            }

            var fingerprint = ReadOperationFingerprint(
                path.Path,
                bytes,
                ExtensionInspectFingerprintOrigin.OperationTimeCurrent,
                findings,
                markdownFacts);
            values.Add(new ExtensionInspectFingerprintFact
            {
                Path = path.Path,
                Fingerprint = fingerprint,
            });
        }

        return [.. values.OrderBy(fact => fact.Path, StringComparer.Ordinal)];
    }

    internal IReadOnlyList<ExtensionInspectFingerprintFact> ReadIntendedFingerprints(
        IReadOnlyList<ExtensionInspectDeclaredPathProjection> paths,
        ICollection<ExtensionInspectFinding> findings,
        IDictionary<string, MarkdownFingerprintFacts> markdownFacts)
    {
        var values = new List<ExtensionInspectFingerprintFact>();
        foreach (var path in paths.Where(path => path.State == ExtensionInspectDeclaredPathState.Available))
        {
            if (path.HasConflict
                || path.Files.FirstOrDefault(file => file.State == ExtensionPackageFileReadState.Available)?.Bytes is not { } bytes)
            {
                ExtensionInspectFindingPolicy.Add(findings, new ExtensionInspectFindingInput
                {
                    Code = ExtensionInspectFindingCode.FingerprintUnavailable,
                    Subject = path.Owners.Count == 0 ? null : path.Owners[0],
                    PackageId = path.Owners.Count == 0 ? null : path.Owners[0],
                    Path = path.Path,
                    Cause = "Intended package bytes were not retained for fingerprinting.",
                });
                continue;
            }

            values.Add(new ExtensionInspectFingerprintFact
            {
                Path = path.Path,
                Fingerprint = ReadOperationFingerprint(
                    path.Path,
                    bytes,
                    ExtensionInspectFingerprintOrigin.OperationTimeIntended,
                    findings,
                    markdownFacts),
            });
        }

        return [.. values.OrderBy(fact => fact.Path, StringComparer.Ordinal)];
    }

    private ExtensionInspectFingerprint ReadOperationFingerprint(
        string path,
        ReadOnlyMemory<byte> bytes,
        ExtensionInspectFingerprintOrigin origin,
        ICollection<ExtensionInspectFinding> findings,
        IDictionary<string, MarkdownFingerprintFacts> markdownFacts)
    {
        if (!ExtensionDestinationPolicy.IsImplicit(path))
        {
            return new ExtensionInspectFingerprint
            {
                Kind = ExtensionInspectFingerprintKind.ExactBytes,
                Policy = ExtensionInspectDefinitions.FingerprintPolicy,
                Sha256 = Convert.ToHexStringLower(SHA256.HashData(bytes.Span)),
                Origin = origin,
            };
        }

        var markdown = IsMarkdown(path);
        var facts = _fingerprintReader.Read(bytes.Span, supportedMarkdown: markdown);
        if (markdown)
        {
            markdownFacts[path] = facts;
        }

        if (facts.State == MarkdownFingerprintState.ExactBytes)
        {
            ExtensionInspectFindingPolicy.Add(findings, new ExtensionInspectFindingInput
            {
                Code = ExtensionInspectFindingCode.FingerprintFallback,
                Path = path,
                Cause = facts.Cause ?? "The Markdown path uses exact-byte fallback.",
            });
        }

        if (facts.Sha256 is null)
        {
            ExtensionInspectFindingPolicy.Add(findings, new ExtensionInspectFindingInput
            {
                Code = ExtensionInspectFindingCode.FingerprintUnavailable,
                Path = path,
                Cause = "The Markdown fingerprint is unavailable.",
            });
            return new ExtensionInspectFingerprint
            {
                Kind = ExtensionInspectFingerprintKind.ExactBytes,
                Policy = ExtensionInspectDefinitions.FingerprintPolicy,
                Sha256 = Convert.ToHexStringLower(SHA256.HashData(bytes.Span)),
                Origin = origin,
            };
        }

        return new ExtensionInspectFingerprint
        {
            Kind = facts.State == MarkdownFingerprintState.Semantic
                ? ExtensionInspectFingerprintKind.Semantic
                : ExtensionInspectFingerprintKind.ExactBytes,
            Policy = ExtensionInspectDefinitions.FingerprintPolicy,
            Sha256 = facts.Sha256,
            Origin = origin,
        };
    }

    private static bool IsMarkdown(string path)
        => path.EndsWith(".md", StringComparison.OrdinalIgnoreCase)
            || path.EndsWith(".markdown", StringComparison.OrdinalIgnoreCase);
}
