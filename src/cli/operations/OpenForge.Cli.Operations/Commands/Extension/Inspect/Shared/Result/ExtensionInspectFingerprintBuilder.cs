using System.Security.Cryptography;
using OpenForge.Cli.Core.Commands.Extension.Inspect.Models.Result;
using OpenForge.Cli.Core.Framework.Documents.Markdown;
using OpenForge.Cli.Core.Framework.Documents.Markdown.Models;
using OpenForge.Cli.Core.Framework.Extensions.Models;
using OpenForge.Cli.Core.Framework.Ownership.Models.Observation;
using OpenForge.Cli.Core.Framework.Ownership.Models.Document;

namespace OpenForge.Cli.Core.Commands.Extension.Inspect.Shared.Result;

internal sealed class ExtensionInspectFingerprintBuilder(MarkdownFingerprintReader fingerprintReader)
{
    private readonly MarkdownFingerprintReader _fingerprintReader = fingerprintReader;

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
        if (ExtensionInspectFingerprintPolicy.ReadKind(path) == ExtensionInspectFingerprintKind.ExactBytes)
        {
            return new ExtensionInspectFingerprint
            {
                Kind = ExtensionInspectFingerprintKind.ExactBytes,
                Policy = ExtensionInspectDefinitions.FingerprintPolicy,
                Sha256 = Convert.ToHexStringLower(SHA256.HashData(bytes.Span)),
                Origin = origin,
            };
        }

        var facts = _fingerprintReader.Read(bytes.Span, supportedMarkdown: true);
        markdownFacts[path] = facts;

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

}
