using System.Globalization;
using OpenForge.Cli.Core.Commands.Route.Move.Models.Result;
using OpenForge.Cli.Core.Presentation.Shared.Wording;

namespace OpenForge.Cli.Core.Presentation.Route.Move.Shared.Wording;

internal static class RouteMoveWording
{
    internal static string MovedFile(string id, string path)
        => global::OpenForge.Cli.OutputText.Route.Move.RouteMoveWording.MovedFile(id, path);

    internal static string MovedCategory(string id, string folder, int count)
        => global::OpenForge.Cli.OutputText.Route.Move.RouteMovePhrases.FormatMovedTheRouteTo(string.Create(global::System.Globalization.CultureInfo.InvariantCulture, $"{id}"), string.Create(global::System.Globalization.CultureInfo.InvariantCulture, $"{folder}"), string.Create(global::System.Globalization.CultureInfo.InvariantCulture, $"{count}"), string.Create(global::System.Globalization.CultureInfo.InvariantCulture, $"{Plural(count, "file")}"));

    internal static string WouldMove(string id, string path)
        => global::OpenForge.Cli.OutputText.Route.Move.RouteMoveWording.WouldMove(id, path);

    internal static string Incomplete(string id, string limitation)
        => global::OpenForge.Cli.OutputText.Route.Move.RouteMovePhrases.FormatCouldNotBeMovedNothingWasChanged($"{id}", $"{TrimSentence(limitation)}");

    internal static string Invalid(string reference, string problem)
        => global::OpenForge.Cli.OutputText.Route.Move.RouteMovePhrases.FormatCannotMove($"{reference}", $"{TrimSentence(problem)}");

    internal static string Blocked(string id, string reason)
        => global::OpenForge.Cli.OutputText.Route.Move.RouteMovePhrases.FormatCannotMove($"{id}", $"{TrimSentence(reason)}");

    internal static string Failed(int completed, int total)
        => global::OpenForge.Cli.OutputText.Route.Move.RouteMoveWording.Failed(completed, total);

    internal static string Cancelled()
        => global::OpenForge.Cli.OutputText.Route.Move.RouteMoveText.MessageRouteMoveWasCancelledNothingWasChanged();

    internal static string Moved(string from, string to, bool dryRun)
        => dryRun ? global::OpenForge.Cli.OutputText.Route.Move.RouteMovePhrases.FormatWouldMove($"{from}", $"{to}") : $"{from} -> {to}";

    internal static string EntryUpdated(string path)
        => global::OpenForge.Cli.OutputText.Route.Move.RouteMoveWording.EntryUpdated(path);

    internal static string EntryRemoved(string path)
        => global::OpenForge.Cli.OutputText.Route.Move.RouteMoveWording.EntryRemoved(path);

    internal static string EntryAdded(string path)
        => global::OpenForge.Cli.OutputText.Route.Move.RouteMoveWording.EntryAdded(path);

    internal static string RewriteHeading(int count, bool category)
        => global::OpenForge.Cli.OutputText.Route.Move.RouteMovePhrases.FormatRewroteThatPointedAtThe(string.Create(global::System.Globalization.CultureInfo.InvariantCulture, $"{count}"), string.Create(global::System.Globalization.CultureInfo.InvariantCulture, $"{Plural(count, "link")}"), string.Create(global::System.Globalization.CultureInfo.InvariantCulture, $"{(category ? "old paths" : "old path")}"));

    internal static string RewriteLocation(string path, int line, int column)
        => string.Create(CultureInfo.InvariantCulture, $"{path}:{line}:{column}");

    internal static string RewriteDestination(string from, string to)
        => $"{from} -> {to}";

    internal static string DryRunComplete() => global::OpenForge.Cli.OutputText.Shared.SharedText.MessageNoFilesWereChanged();

    internal static string ScanSummary(int files, int occurrences)
        => global::OpenForge.Cli.OutputText.Route.Move.RouteMovePhrases.FormatScannedFound(string.Create(global::System.Globalization.CultureInfo.InvariantCulture, $"{files}"), string.Create(global::System.Globalization.CultureInfo.InvariantCulture, $"{Plural(files, "file")}"), string.Create(global::System.Globalization.CultureInfo.InvariantCulture, $"{occurrences}"), string.Create(global::System.Globalization.CultureInfo.InvariantCulture, $"{Plural(occurrences, "link")}"));

    internal static string Effect(string path, string action, string outcome)
        => $"{path}: {action} {outcome}";

    internal static string Before(string value) => global::OpenForge.Cli.OutputText.Route.Move.RouteMoveWording.Before(value);

    internal static string After(string value) => global::OpenForge.Cli.OutputText.Route.Move.RouteMoveWording.After(value);

    internal static string InvalidSource(string reference)
        => global::OpenForge.Cli.OutputText.Route.Move.RouteMoveWording.InvalidSource(reference);

    internal static string InvalidSubject(string reference, bool overwrite)
        => overwrite
            ? global::OpenForge.Cli.OutputText.Route.Move.RouteMovePhrases.FormatIsAnOverwriteFileMoveItsBaseFile($"{reference}")
            : global::OpenForge.Cli.OutputText.Route.Move.RouteMovePhrases.FormatIsTheLoaderAndCannotBeMoved($"{reference}");

    internal static string InvalidDestination(string target, bool category)
        => category
            ? global::OpenForge.Cli.OutputText.Route.Move.RouteMovePhrases.FormatMustBeAnEntrypointPathWhenMovingARoute($"{target}")
            : global::OpenForge.Cli.OutputText.Route.Move.RouteMovePhrases.FormatMustBeAMarkdownFilePathUnderAgents($"{target}");

    internal static string DestinationOccupied(string path)
        => global::OpenForge.Cli.OutputText.Route.Move.RouteMoveWording.DestinationOccupied(path);

    internal static string DestinationParentMissing(string folder)
        => global::OpenForge.Cli.OutputText.Route.Move.RouteMoveWording.DestinationParentMissing(folder);

    internal static string CategoryUnsafe(string folder, string path, string reason)
        => global::OpenForge.Cli.OutputText.Route.Move.RouteMovePhrases.FormatContainsAFileThatCannotBeMovedSafely($"{folder}", $"{path}", $"{TrimSentence(reason)}");

    internal static string OverwriteAmbiguous(string name)
        => global::OpenForge.Cli.OutputText.Route.Move.RouteMoveWording.OverwriteAmbiguous(name);

    internal static string ReferenceUnsafe(string path, int line, int column, string reason)
        => global::OpenForge.Cli.OutputText.Route.Move.RouteMovePhrases.ReferenceCannotRewriteLocation($"{path}", $"{line}", $"{column}", $"{TrimSentence(reason)}");

    internal static string ReferenceUnsafe(string path, string reason)
        => global::OpenForge.Cli.OutputText.Route.Move.RouteMovePhrases.ReferenceCannotRewritePath($"{path}", $"{TrimSentence(reason)}");

    internal static string CategoryInventoryIncomplete(string folder)
        => global::OpenForge.Cli.OutputText.Route.Move.RouteMoveWording.CategoryInventoryIncomplete(folder);

    internal static string ReferenceCoverageIncomplete(string path)
        => global::OpenForge.Cli.OutputText.Route.Move.RouteMoveWording.ReferenceCoverageIncomplete(path);

    internal static string FindingTitle(RouteMoveFindingCode code)
        => code switch
        {
            RouteMoveFindingCode.InvalidInput => global::OpenForge.Cli.OutputText.Shared.SharedText.TitleInvalidInput(),
            RouteMoveFindingCode.InvalidSource => global::OpenForge.Cli.OutputText.Shared.SharedText.TitleInvalidSource(),
            RouteMoveFindingCode.SourceNotFound => global::OpenForge.Cli.OutputText.Shared.SharedText.TitleSourceIsUnknown(),
            RouteMoveFindingCode.InvalidSubject => global::OpenForge.Cli.OutputText.Route.Move.RouteMoveText.TitleInvalidSubject(),
            RouteMoveFindingCode.InvalidDestination => global::OpenForge.Cli.OutputText.Route.Move.RouteMoveText.TitleInvalidDestination(),
            RouteMoveFindingCode.WorkspaceUnsafe => global::OpenForge.Cli.OutputText.Shared.SharedText.TitleWorkspaceIsUnsafe(),
            RouteMoveFindingCode.SourceUnsafe => global::OpenForge.Cli.OutputText.Shared.SharedText.TitleSourceIsUnsafe(),
            RouteMoveFindingCode.RouteAmbiguous => global::OpenForge.Cli.OutputText.Shared.SharedText.TitleRoutesAreAmbiguous(),
            RouteMoveFindingCode.IdentityCollision => global::OpenForge.Cli.OutputText.Route.Shared.RouteSharedText.TitleIdentityCollision(),
            RouteMoveFindingCode.OverwriteAmbiguous => global::OpenForge.Cli.OutputText.Shared.SharedText.TitleOverwriteIsAmbiguous(),
            RouteMoveFindingCode.CategoryUnsafe => global::OpenForge.Cli.OutputText.Route.Shared.RouteSharedText.TitleCategoryIsUnsafe(),
            RouteMoveFindingCode.OwnershipUnavailable => global::OpenForge.Cli.OutputText.Route.Move.RouteMoveText.TitleOwnershipCouldNotBeRead(),
            RouteMoveFindingCode.OwnershipClaimed => global::OpenForge.Cli.OutputText.Route.Shared.RouteSharedText.TitleSourceIsManaged(),
            RouteMoveFindingCode.DestinationUnsafe => global::OpenForge.Cli.OutputText.Route.Move.RouteMoveText.TitleDestinationIsUnsafe(),
            RouteMoveFindingCode.DestinationParentMissing => global::OpenForge.Cli.OutputText.Route.Move.RouteMoveText.TitleDestinationParentIsMissing(),
            RouteMoveFindingCode.DestinationOccupied => global::OpenForge.Cli.OutputText.Route.Move.RouteMoveText.TitleDestinationAlreadyExists(),
            RouteMoveFindingCode.SelfMove => global::OpenForge.Cli.OutputText.Route.Move.RouteMoveText.TitleSourceAndDestinationAreTheSame(),
            RouteMoveFindingCode.DestinationInsideSource => global::OpenForge.Cli.OutputText.Route.Move.RouteMoveText.TitleDestinationIsInsideSource(),
            RouteMoveFindingCode.ReferenceUnsafe => global::OpenForge.Cli.OutputText.Route.Move.RouteMoveText.TitleReferenceCouldNotBeRewritten(),
            RouteMoveFindingCode.GeneratedRegionUnsafe => global::OpenForge.Cli.OutputText.Shared.SharedText.TitleEntriesSectionIsUnsafe(),
            RouteMoveFindingCode.WorkspaceLockUnavailable => global::OpenForge.Cli.OutputText.Shared.SharedText.TitleWorkspaceLockIsHeld(),
            RouteMoveFindingCode.TargetChanged => global::OpenForge.Cli.OutputText.Shared.SharedText.TitleTargetChanged(),
            RouteMoveFindingCode.RecoveryConflict => global::OpenForge.Cli.OutputText.Shared.SharedText.TitleRecoveryDataBlocksTheChange(),
            RouteMoveFindingCode.WorkspaceUnavailable => global::OpenForge.Cli.OutputText.Shared.SharedText.TitleWorkspaceIsUnavailable(),
            RouteMoveFindingCode.CategoryInventoryIncomplete => global::OpenForge.Cli.OutputText.Route.Shared.RouteSharedText.TitleCategoryInventoryIsIncomplete(),
            RouteMoveFindingCode.ReferenceCoverageIncomplete => global::OpenForge.Cli.OutputText.Route.Shared.RouteSharedText.TitleReferenceScanIsIncomplete(),
            RouteMoveFindingCode.ProjectionIncomplete => global::OpenForge.Cli.OutputText.Shared.SharedText.TitleEntriesContentIsUnavailable(),
            RouteMoveFindingCode.RecoveryUnavailable => global::OpenForge.Cli.OutputText.Shared.SharedText.TitleRecoveryDataIsUnavailable(),
            RouteMoveFindingCode.InspectionIncomplete => global::OpenForge.Cli.OutputText.Route.Shared.RouteSharedText.TitleInspectionIsIncomplete(),
            RouteMoveFindingCode.RecoveryArtifactRetained => global::OpenForge.Cli.OutputText.Shared.SharedText.TitleRecoveryBundleWasRetained(),
            RouteMoveFindingCode.TargetChangedDuringApply => global::OpenForge.Cli.OutputText.Shared.SharedText.TitleTargetChangedDuringTheWrite(),
            RouteMoveFindingCode.WriteFailed => global::OpenForge.Cli.OutputText.Shared.SharedText.TitleWriteFailed(),
            RouteMoveFindingCode.VerificationFailed => global::OpenForge.Cli.OutputText.Shared.SharedText.TitleVerificationFailed(),
            RouteMoveFindingCode.RecoveryFailed => global::OpenForge.Cli.OutputText.Shared.SharedText.TitleRecoveryStateIsUnknown(),
            RouteMoveFindingCode.OperationFailed => global::OpenForge.Cli.OutputText.Route.Move.RouteMoveText.TitleRouteMoveFailed(),
            RouteMoveFindingCode.Interrupted => global::OpenForge.Cli.OutputText.Route.Move.RouteMoveText.TitleRouteMoveWasCancelled(),
            _ => throw new ArgumentOutOfRangeException(nameof(code), code, "The Route Move finding code is not defined."),
        };

    private static string Plural(int count, string singular)
        => count == 1 ? singular : singular + "s";

    internal static string TrimSentence(string value)
        => CliFindingWording.PlainCause(value);
}
