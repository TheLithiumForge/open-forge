using System.Collections.ObjectModel;

namespace OpenForge.Cli.Core.Commands.Route.Inspect.Models.Resolution;

internal enum RouteInspectReferenceKind
{
    Missing,
    SourceId,
    SourcePath,
    Invalid,
}

internal enum RouteInspectSelectionMethod
{
    Unresolved,
    AutomaticId,
    ExactPath,
    Interactive,
}

internal sealed class RouteInspectSelection
{
    internal RouteInspectSelection(
        RouteInspectReferenceKind referenceKind,
        RouteInspectSelectionMethod selectionMethod,
        string? requestedReference,
        IEnumerable<string> candidatePaths)
    {
        if (!Enum.IsDefined(referenceKind))
        {
            throw new ArgumentOutOfRangeException(
                nameof(referenceKind),
                referenceKind,
                "The route-inspect reference kind is not defined.");
        }

        if (!Enum.IsDefined(selectionMethod))
        {
            throw new ArgumentOutOfRangeException(
                nameof(selectionMethod),
                selectionMethod,
                "The route-inspect selection method is not defined.");
        }

        if (requestedReference is not null)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(requestedReference);
        }

        ValidateReferenceAndMethod(referenceKind, selectionMethod, requestedReference);
        var materializedCandidates = MaterializeCandidatePaths(candidatePaths);
        if ((selectionMethod != RouteInspectSelectionMethod.Unresolved
                || referenceKind != RouteInspectReferenceKind.SourceId)
            && materializedCandidates.Count != 0)
        {
            throw new ArgumentException(
                "Candidate paths are allowed only for an unresolved source ID selection.",
                nameof(candidatePaths));
        }

        ReferenceKind = referenceKind;
        SelectionMethod = selectionMethod;
        RequestedReference = requestedReference;
        CandidatePaths = materializedCandidates;
    }

    internal RouteInspectReferenceKind ReferenceKind { get; }

    internal RouteInspectSelectionMethod SelectionMethod { get; }

    internal string? RequestedReference { get; }

    internal IReadOnlyList<string> CandidatePaths { get; }

    internal bool IsResolved => SelectionMethod != RouteInspectSelectionMethod.Unresolved;

    private static void ValidateReferenceAndMethod(
        RouteInspectReferenceKind referenceKind,
        RouteInspectSelectionMethod selectionMethod,
        string? requestedReference)
    {
        if (referenceKind is RouteInspectReferenceKind.Missing or RouteInspectReferenceKind.Invalid
            && selectionMethod != RouteInspectSelectionMethod.Unresolved)
        {
            throw new ArgumentException(
                "Missing and invalid references require unresolved selection.",
                nameof(selectionMethod));
        }

        var requiredReferenceKind = selectionMethod switch
        {
            RouteInspectSelectionMethod.AutomaticId => RouteInspectReferenceKind.SourceId,
            RouteInspectSelectionMethod.ExactPath => RouteInspectReferenceKind.SourcePath,
            RouteInspectSelectionMethod.Interactive => RouteInspectReferenceKind.SourceId,
            _ => (RouteInspectReferenceKind?)null,
        };
        if (requiredReferenceKind is { } required
            && referenceKind != required)
        {
            throw new ArgumentException(
                $"{selectionMethod} selection requires {required} reference kind.",
                nameof(referenceKind));
        }

        if (referenceKind == RouteInspectReferenceKind.Missing)
        {
            if (requestedReference is not null)
            {
                throw new ArgumentException(
                    "A missing source reference cannot contain a requested reference.",
                    nameof(requestedReference));
            }
        }
        else if (requestedReference is null)
        {
            throw new ArgumentException(
                "This source reference kind requires a requested reference.",
                nameof(requestedReference));
        }
    }

    private static IReadOnlyList<string> MaterializeCandidatePaths(IEnumerable<string> candidatePaths)
    {
        ArgumentNullException.ThrowIfNull(candidatePaths);
        var materialized = candidatePaths.ToArray();
        var seen = new HashSet<string>(StringComparer.Ordinal);
        for (var index = 0; index < materialized.Length; index++)
        {
            var candidatePath = materialized[index];
            if (candidatePath is null)
            {
                throw new ArgumentException("Candidate paths cannot contain null.", nameof(candidatePaths));
            }

            ArgumentException.ThrowIfNullOrWhiteSpace(candidatePath);
            if (!seen.Add(candidatePath))
            {
                throw new ArgumentException("Candidate paths must be unique.", nameof(candidatePaths));
            }

            if (index > 0 && string.CompareOrdinal(materialized[index - 1], candidatePath) >= 0)
            {
                throw new ArgumentException(
                    "Candidate paths must use strict ordinal order.",
                    nameof(candidatePaths));
            }
        }

        return new ReadOnlyCollection<string>(materialized);
    }
}
