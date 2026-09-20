using System.Collections.ObjectModel;
using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.Identity;

namespace OpenForge.Cli.Core.Commands.Route.Shared.Models.Source;

internal enum RouteOverwriteState
{
    Paired,
    Orphan,
    Ambiguous,
}

internal sealed class RouteOverwriteFact
{
    internal RouteOverwriteFact(
        RouteOverwriteState state,
        RouteSourceDocument overwrite,
        IEnumerable<string> candidateBasePaths)
    {
        if (!Enum.IsDefined(state))
        {
            throw new ArgumentOutOfRangeException(nameof(state), state, "The overwrite state is not defined.");
        }

        ArgumentNullException.ThrowIfNull(overwrite);
        if (overwrite.Form != SourceDocumentForm.OverwriteCompanion)
        {
            throw new ArgumentException("An overwrite fact requires an overwrite companion document.", nameof(overwrite));
        }

        ArgumentNullException.ThrowIfNull(candidateBasePaths);
        var paths = candidateBasePaths.OrderBy(path => path, StringComparer.Ordinal).ToArray();
        if (paths.Any(path => !IsBasePath(path))
            || paths.Distinct(StringComparer.Ordinal).Count() != paths.Length)
        {
            throw new ArgumentException("Overwrite candidate paths must be unique canonical base paths.", nameof(candidateBasePaths));
        }

        var validCount = state switch
        {
            RouteOverwriteState.Paired => paths.Length == 1,
            RouteOverwriteState.Orphan => true,
            RouteOverwriteState.Ambiguous => paths.Length >= 2,
            _ => false,
        };
        if (!validCount)
        {
            throw new ArgumentException("Overwrite candidate paths do not match the overwrite state.", nameof(candidateBasePaths));
        }

        State = state;
        Overwrite = overwrite;
        CandidateBasePaths = new ReadOnlyCollection<string>(paths);
    }

    internal RouteOverwriteState State { get; }

    internal RouteSourceDocument Overwrite { get; }

    internal string CanonicalPath => Overwrite.CanonicalLogicalPath;

    internal string PhysicalPath => Overwrite.PhysicalPath;

    internal IReadOnlyList<string> CandidateBasePaths { get; }

    private static bool IsBasePath(string? path)
    {
        if (path is not { } canonicalPath
            || !SourceLogicalPath.IsCanonical(canonicalPath)
            || canonicalPath.EndsWith(".overwrite.md", StringComparison.Ordinal))
        {
            return false;
        }

        return true;
    }
}
