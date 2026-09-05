using OpenForge.Cli.Core.Framework.Filesystem.TypedReads;
using OpenForge.Cli.Core.Framework.OperationalContributors.Models;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;
using OpenForge.Cli.Core.Framework.Sources.Models.Reading;
using OpenForge.Cli.Core.Framework.Sources.Models.Routing;
using OpenForge.Cli.Core.Framework.Sources.Operational.Shared.Routes.Models;

namespace OpenForge.Cli.Core.Framework.Sources.Operational.Shared.Routes;

internal static class RouteSourceInspectionPolicy
{
    internal static OperationalViewState ReadViewState(
        SourceCatalogue catalogue,
        SourceRouteFacts routes,
        RouteWorkspaceEntryObservation entry,
        IReadOnlyList<RouteSourceObservation> sources)
    {
        if (catalogue.IsCancelled
            || routes.IsCancelled
            || entry.State == OperationalViewState.Interrupted
            || sources.SelectMany(source => source.Layers)
                .Any(layer => layer.State == FileReadState.Cancelled))
        {
            return OperationalViewState.Interrupted;
        }

        if (entry.State == OperationalViewState.Blocked
            || catalogue.Issues.Any(issue => issue.Code is SourceCatalogueIssueCode.RootUnsafe
                or SourceCatalogueIssueCode.CandidateUnsafe
                or SourceCatalogueIssueCode.IdentityCollision
                or SourceCatalogueIssueCode.PhysicalAlias)
            || routes.Issues.Any(issue => issue.Code is SourceRouteIssueCode.LoaderUnsafe
                or SourceRouteIssueCode.RouteAmbiguous))
        {
            return OperationalViewState.Blocked;
        }

        var entryIsOrdinary = IsOrdinaryEntry(catalogue, entry);
        return !IsCatalogueComplete(catalogue)
            || routes.Issues.Count != 0
            || !routes.AreLoaderRootFactsComplete
            || !entryIsOrdinary
            || sources.Any(source => !source.IsReadable)
            ? OperationalViewState.Incomplete
            : OperationalViewState.Complete;
    }

    internal static bool IsCatalogueComplete(SourceCatalogue catalogue)
        => catalogue.Issues.Count == 0 || IsSafelyAbsentSourceInventory(catalogue);

    internal static bool IsSafelyAbsentSourceInventory(SourceCatalogue catalogue)
        => catalogue.Sources.Count == 0
            && catalogue.Issues.All(issue =>
                issue.Code == SourceCatalogueIssueCode.RootMissing);

    private static bool IsOrdinaryEntry(
        SourceCatalogue catalogue,
        RouteWorkspaceEntryObservation entry)
    {
        var isComplete = entry.State switch
        {
            OperationalViewState.Complete => true,
            OperationalViewState.Incomplete
                or OperationalViewState.Blocked
                or OperationalViewState.Interrupted => false,
            _ => throw new ArgumentOutOfRangeException(
                nameof(entry),
                entry.State,
                "The workspace-entry operational state is not defined."),
        };
        return entry.Layer.State switch
        {
            FileReadState.Complete => isComplete,
            FileReadState.Missing => IsSafelyAbsentSourceInventory(catalogue),
            FileReadState.InvalidEncoding
                or FileReadState.InvalidSyntax
                or FileReadState.AccessDenied
                or FileReadState.InputOutputFailure
                or FileReadState.Cancelled => false,
            _ => throw new ArgumentOutOfRangeException(
                nameof(entry),
                entry.Layer.State,
                "The workspace-entry read state is not defined."),
        };
    }

    internal static FileReadState ReadLayerState(SourceDocumentReadResult read)
    {
        if (read.Read is { } value)
        {
            return value.State;
        }

        return read.Verification.State switch
        {
            SourceLayerVerificationState.Missing => FileReadState.Missing,
            SourceLayerVerificationState.Cancelled => FileReadState.Cancelled,
            SourceLayerVerificationState.Verified => throw new InvalidOperationException(
                "A verified source layer requires a read result."),
            SourceLayerVerificationState.Unsafe
                or SourceLayerVerificationState.Unavailable
                or SourceLayerVerificationState.Changed => FileReadState.InputOutputFailure,
            _ => throw new ArgumentOutOfRangeException(
                nameof(read),
                read.Verification.State,
                "The source-layer verification state is not defined."),
        };
    }
}
