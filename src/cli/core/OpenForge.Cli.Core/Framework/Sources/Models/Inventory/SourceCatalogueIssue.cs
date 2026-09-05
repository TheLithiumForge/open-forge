using System.Collections.ObjectModel;
using OpenForge.Cli.Core.Framework.Filesystem;

namespace OpenForge.Cli.Core.Framework.Sources.Models.Inventory;

internal enum SourceCatalogueIssueStage
{
    Root,
    Directory,
    Candidate,
    Identity,
    Pairing,
}

internal enum SourceCatalogueIssueCode
{
    RootMissing,
    RootUnsafe,
    RootUnavailable,
    DirectoryUnavailable,
    CandidateUnsafe,
    CandidateUnavailable,
    UnsupportedSource,
    IdentityUnavailable,
    EntrypointAmbiguous,
    EntrypointCompatibilityCollision,
    IdentityCollision,
    PhysicalAlias,
    OrphanOverwrite,
}

internal sealed class SourceCatalogueIssue
{
    internal SourceCatalogueIssue(
        SourceCatalogueIssueCode code,
        string attemptedCanonicalPath,
        IEnumerable<string> relatedPaths,
        string? scopePhysicalPath,
        FilesystemFailure? failure)
    {
        if (!Enum.IsDefined(code))
        {
            throw new ArgumentOutOfRangeException(nameof(code), code, "The source catalogue issue code is not defined.");
        }

        var stage = ReadStage(code);

        ArgumentException.ThrowIfNullOrWhiteSpace(attemptedCanonicalPath);
        ArgumentNullException.ThrowIfNull(relatedPaths);
        var materializedRelatedPaths = relatedPaths.ToArray();
        if (materializedRelatedPaths.Any(string.IsNullOrWhiteSpace)
            || materializedRelatedPaths.Distinct(StringComparer.Ordinal).Count() != materializedRelatedPaths.Length)
        {
            throw new ArgumentException("Related catalogue issue paths must be unique and nonempty.", nameof(relatedPaths));
        }

        if (stage == SourceCatalogueIssueStage.Root && scopePhysicalPath is not null)
        {
            throw new ArgumentException("Root catalogue issues cannot carry a scope physical path.", nameof(scopePhysicalPath));
        }

        var normalizedScopePhysicalPath = NormalizeOptionalPhysicalPath(scopePhysicalPath);
        if (stage != SourceCatalogueIssueStage.Root && normalizedScopePhysicalPath is null)
        {
            throw new ArgumentException("Non-root catalogue issues require a scope physical path.", nameof(scopePhysicalPath));
        }

        Stage = stage;
        Code = code;
        AttemptedCanonicalPath = attemptedCanonicalPath;
        RelatedPaths = new ReadOnlyCollection<string>(materializedRelatedPaths.OrderBy(path => path, StringComparer.Ordinal).ToArray());
        ScopePhysicalPath = normalizedScopePhysicalPath;
        Failure = failure;
    }

    internal SourceCatalogueIssueStage Stage { get; }

    internal SourceCatalogueIssueCode Code { get; }

    internal string AttemptedCanonicalPath { get; }

    internal IReadOnlyList<string> RelatedPaths { get; }

    internal string? ScopePhysicalPath { get; }

    internal FilesystemFailure? Failure { get; }

    private static SourceCatalogueIssueStage ReadStage(SourceCatalogueIssueCode code)
    {
        return code switch
        {
            SourceCatalogueIssueCode.RootMissing
                or SourceCatalogueIssueCode.RootUnsafe
                or SourceCatalogueIssueCode.RootUnavailable => SourceCatalogueIssueStage.Root,
            SourceCatalogueIssueCode.DirectoryUnavailable => SourceCatalogueIssueStage.Directory,
            SourceCatalogueIssueCode.CandidateUnsafe
                or SourceCatalogueIssueCode.CandidateUnavailable
                or SourceCatalogueIssueCode.UnsupportedSource => SourceCatalogueIssueStage.Candidate,
            SourceCatalogueIssueCode.IdentityUnavailable
                or SourceCatalogueIssueCode.EntrypointAmbiguous
                or SourceCatalogueIssueCode.EntrypointCompatibilityCollision
                or SourceCatalogueIssueCode.IdentityCollision
                or SourceCatalogueIssueCode.PhysicalAlias => SourceCatalogueIssueStage.Identity,
            SourceCatalogueIssueCode.OrphanOverwrite => SourceCatalogueIssueStage.Pairing,
            _ => throw new ArgumentOutOfRangeException(nameof(code), code, "The source catalogue issue code is not defined."),
        };
    }

    private static string? NormalizeOptionalPhysicalPath(string? path)
    {
        if (path is null)
        {
            return null;
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        var normalized = Path.GetFullPath(path);
        if (!Path.IsPathRooted(path)
            || !string.Equals(normalized, path, StringComparison.Ordinal))
        {
            throw new ArgumentException("A scope physical path must be absolute and normalized.", nameof(path));
        }

        return normalized;
    }
}
