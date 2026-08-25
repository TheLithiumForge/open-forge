using System.Collections.ObjectModel;

namespace OpenForge.Cli.Core.Framework.Sources.Models.Routing;

internal enum SourceRouteIssueCode
{
    LoaderUnavailable,
    LoaderMalformed,
    LoaderUnsafe,
    RouteAmbiguous,
    RouteSupportUnavailable,
}

internal sealed class SourceRouteIssue
{
    private const int MaximumCauseLength = 256;

    internal SourceRouteIssue(
        SourceRouteIssueCode code,
        string canonicalPath,
        IEnumerable<string> relatedPaths,
        int occurrence,
        string cause)
    {
        if (!Enum.IsDefined(code))
        {
            throw new ArgumentOutOfRangeException(nameof(code), code, "The source route issue code is not defined.");
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(canonicalPath);
        ArgumentNullException.ThrowIfNull(relatedPaths);
        var materializedRelatedPaths = relatedPaths.ToArray();
        if (materializedRelatedPaths.Any(string.IsNullOrWhiteSpace)
            || materializedRelatedPaths.Distinct(StringComparer.Ordinal).Count() != materializedRelatedPaths.Length)
        {
            throw new ArgumentException("Related source route issue paths must be unique and nonempty.", nameof(relatedPaths));
        }

        if (occurrence < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(occurrence), occurrence, "A source route issue occurrence cannot be negative.");
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(cause);
        Code = code;
        CanonicalPath = canonicalPath;
        RelatedPaths = new ReadOnlyCollection<string>(materializedRelatedPaths.OrderBy(path => path, StringComparer.Ordinal).ToArray());
        Occurrence = occurrence;
        Cause = cause.Length <= MaximumCauseLength ? cause : cause[..MaximumCauseLength];
    }

    internal SourceRouteIssueCode Code { get; }

    internal string CanonicalPath { get; }

    internal IReadOnlyList<string> RelatedPaths { get; }

    internal int Occurrence { get; }

    internal string Cause { get; }
}
