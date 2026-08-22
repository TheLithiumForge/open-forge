using OpenForge.Cli.Core.Commands.Route.List;
using OpenForge.Cli.Core.Commands.Route.Shared.Models.Source;
using OpenForge.Cli.Core.Commands.Route.Shared.Source;

namespace OpenForge.Cli.Core.Commands.Route.List.Shared.Selection;

internal static class RouteListSelectionFactory
{
    private static readonly object ConstructionToken = new();

    internal static bool OwnsConstructionToken(object? constructionToken)
    {
        return ReferenceEquals(constructionToken, ConstructionToken);
    }

    internal static RouteListSelection LoaderRoots()
    {
        return new RouteListSelection(
            ConstructionToken,
            RouteListSelectionKind.LoaderRoots,
            null,
            null,
            null,
            null);
    }

    internal static RouteListSelection Attempted(string? sourceReference)
    {
        if (sourceReference is null)
        {
            return LoaderRoots();
        }

        var parsed = RouteSourceReferenceParser.Parse(sourceReference);
        return parsed.Kind switch
        {
            RouteSourceReferenceKind.SourceId when parsed.AttemptedId is not null
                => AttemptedId(parsed.AttemptedId),
            RouteSourceReferenceKind.SourcePath when parsed.AttemptedPath is not null
                => AttemptedPath(parsed.AttemptedPath),
            _ => LoaderRoots(),
        };
    }

    internal static RouteListSelection AttemptedId(string attemptedId)
    {
        ValidateIdentity(attemptedId, nameof(attemptedId));
        return new RouteListSelection(
            ConstructionToken,
            RouteListSelectionKind.SourceId,
            attemptedId,
            null,
            null,
            null);
    }

    internal static RouteListSelection AttemptedPath(string attemptedPath)
    {
        ValidateIdentity(attemptedPath, nameof(attemptedPath));
        return new RouteListSelection(
            ConstructionToken,
            RouteListSelectionKind.SourcePath,
            null,
            attemptedPath,
            null,
            null);
    }

    internal static RouteListSelection ResolvedId(string attemptedId, RouteSource source)
    {
        ArgumentNullException.ThrowIfNull(source);
        if (!string.Equals(attemptedId, source.Id, StringComparison.Ordinal))
        {
            throw new ArgumentException("The attempted source ID must match the resolved source.", nameof(attemptedId));
        }

        ValidateId(attemptedId, nameof(attemptedId));
        return new RouteListSelection(
            ConstructionToken,
            RouteListSelectionKind.SourceId,
            attemptedId,
            null,
            source.Id,
            source.CanonicalPath);
    }

    internal static RouteListSelection ResolvedPath(string attemptedPath, RouteSource source)
    {
        ArgumentNullException.ThrowIfNull(source);
        if (!string.Equals(attemptedPath, source.CanonicalPath, StringComparison.Ordinal)
            && !string.Equals(attemptedPath, source.OverwritePath, StringComparison.Ordinal))
        {
            throw new ArgumentException("The attempted source path must identify the base or adjacent overwrite.", nameof(attemptedPath));
        }

        ValidatePath(attemptedPath, nameof(attemptedPath));
        return new RouteListSelection(
            ConstructionToken,
            RouteListSelectionKind.SourcePath,
            null,
            attemptedPath,
            source.Id,
            source.CanonicalPath);
    }

    private static void ValidateIdentity(string value, string parameterName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value, parameterName);
    }

    private static void ValidateId(string value, string parameterName)
    {
        if (!RouteSourceIdentity.IsValidId(value))
        {
            throw new ArgumentException("The source ID is not valid.", parameterName);
        }
    }

    private static void ValidatePath(string value, string parameterName)
    {
        if (!RouteSourceReferenceParser.IsValidCanonicalPath(value))
        {
            throw new ArgumentException("The source path is not canonical.", parameterName);
        }
    }
}
