using System.Globalization;
using OpenForge.Cli.Core.Presentation.Shared.Text;

namespace OpenForge.Cli.Core.Presentation.Index.Shared.Wording;

internal static class IndexWording
{
    internal static string HelpSyntax() => ("  " + global::OpenForge.Cli.OutputText.Index.IndexText.HelpSyntax());
    internal static string HelpSelection() => ("  " + global::OpenForge.Cli.OutputText.Index.IndexText.HelpSelection());
    internal static string HelpWritePolicy() => ("  " + global::OpenForge.Cli.OutputText.Index.IndexText.HelpWritePolicy());
    internal static string HelpGlobalOptions() => ("  " + global::OpenForge.Cli.OutputText.Shared.SharedText.HelpGlobalOptionsWithDetailSelection());
    internal static string HelpExamples() => ("  " + global::OpenForge.Cli.OutputText.Index.IndexText.HelpExamples());
    internal static string HelpRelatedCommands() => ("  " + global::OpenForge.Cli.OutputText.Index.IndexText.MessageOpenForgeDoctorInspectBlockedTopologyMetadataOrGeneratedRegionFactsOpenForgeCleanupRemoveAReportedRetainedRecoveryArtifactAfterReview());
    internal static string HelpNotes() => ("  " + global::OpenForge.Cli.OutputText.Index.IndexText.HelpNotes());
    internal static string ChooseSource() => global::OpenForge.Cli.OutputText.Index.IndexText.MessageChooseASourceIdOrExactPath();
    internal static string CorrectInput() => global::OpenForge.Cli.OutputText.Index.IndexText.MessageCorrectTheNamedInput();
    internal static string RetryLock() => global::OpenForge.Cli.OutputText.Index.IndexText.MessageRetryWhenTheOtherCommandFinishes();
    internal static string InspectRecovery() => global::OpenForge.Cli.OutputText.Index.IndexText.MessageReviewTheRetainedRecoveryBundle();
    internal static string Retry() => global::OpenForge.Cli.OutputText.Index.IndexText.MessageRerunTheSameIndexRequest();
    internal static string Inspect() => global::OpenForge.Cli.OutputText.Index.IndexText.MessageInspectTheReportedProblemBeforeRerunningIndex();
    internal static string FindingTitle(OpenForge.Cli.Core.Commands.Index.Models.Result.IndexFindingCode code) => code switch
    {
        OpenForge.Cli.Core.Commands.Index.Models.Result.IndexFindingCode.InvalidInput => global::OpenForge.Cli.OutputText.Shared.SharedText.TitleInvalidInput(),
        OpenForge.Cli.Core.Commands.Index.Models.Result.IndexFindingCode.InvalidSource => global::OpenForge.Cli.OutputText.Shared.SharedText.TitleInvalidSource(),
        OpenForge.Cli.Core.Commands.Index.Models.Result.IndexFindingCode.WorkspaceUnavailable => global::OpenForge.Cli.OutputText.Shared.SharedText.TitleWorkspaceIsUnavailable(),
        OpenForge.Cli.Core.Commands.Index.Models.Result.IndexFindingCode.WorkspaceUnsafe => global::OpenForge.Cli.OutputText.Shared.SharedText.TitleWorkspaceIsUnsafe(),
        OpenForge.Cli.Core.Commands.Index.Models.Result.IndexFindingCode.SourceAmbiguous => global::OpenForge.Cli.OutputText.Shared.SharedText.TitleSourceIsAmbiguous(),
        OpenForge.Cli.Core.Commands.Index.Models.Result.IndexFindingCode.SourceUnsafe => global::OpenForge.Cli.OutputText.Shared.SharedText.TitleSourceIsUnsafe(),
        OpenForge.Cli.Core.Commands.Index.Models.Result.IndexFindingCode.TopologyAmbiguous => global::OpenForge.Cli.OutputText.Shared.SharedText.TitleRoutesAreAmbiguous(),
        OpenForge.Cli.Core.Commands.Index.Models.Result.IndexFindingCode.TargetUnexposed => global::OpenForge.Cli.OutputText.Index.IndexText.TitleSourceIsNotListed(),
        OpenForge.Cli.Core.Commands.Index.Models.Result.IndexFindingCode.TargetUnsafe => global::OpenForge.Cli.OutputText.Shared.SharedText.TitleTargetIsUnsafe(),
        OpenForge.Cli.Core.Commands.Index.Models.Result.IndexFindingCode.MetadataUnsafe => global::OpenForge.Cli.OutputText.Shared.SharedText.TitleFrontmatterIsInvalid(),
        OpenForge.Cli.Core.Commands.Index.Models.Result.IndexFindingCode.MetadataOptional => global::OpenForge.Cli.OutputText.Index.IndexText.TitleOptionalMetadataIsMissing(),
        OpenForge.Cli.Core.Commands.Index.Models.Result.IndexFindingCode.GeneratedRegionUnsafe => global::OpenForge.Cli.OutputText.Shared.SharedText.TitleEntriesSectionIsUnsafe(),
        OpenForge.Cli.Core.Commands.Index.Models.Result.IndexFindingCode.WorkspaceLockUnavailable => global::OpenForge.Cli.OutputText.Shared.SharedText.TitleWorkspaceLockIsHeld(),
        OpenForge.Cli.Core.Commands.Index.Models.Result.IndexFindingCode.TargetChanged => global::OpenForge.Cli.OutputText.Shared.SharedText.TitleTargetChanged(),
        OpenForge.Cli.Core.Commands.Index.Models.Result.IndexFindingCode.RecoveryConflict => global::OpenForge.Cli.OutputText.Shared.SharedText.TitleRecoveryDataBlocksTheChange(),
        OpenForge.Cli.Core.Commands.Index.Models.Result.IndexFindingCode.DiscoveryIncomplete => global::OpenForge.Cli.OutputText.Index.IndexText.TitleRoutesCouldNotBeRead(),
        OpenForge.Cli.Core.Commands.Index.Models.Result.IndexFindingCode.MetadataIncomplete => global::OpenForge.Cli.OutputText.Shared.SharedText.TitleFrontmatterCouldNotBeRead(),
        OpenForge.Cli.Core.Commands.Index.Models.Result.IndexFindingCode.MetadataSkipped => global::OpenForge.Cli.OutputText.Index.IndexText.TitleMetadataWasSkipped(),
        OpenForge.Cli.Core.Commands.Index.Models.Result.IndexFindingCode.ProjectionIncomplete => global::OpenForge.Cli.OutputText.Shared.SharedText.TitleEntriesContentIsUnavailable(),
        OpenForge.Cli.Core.Commands.Index.Models.Result.IndexFindingCode.RecoveryUnavailable => global::OpenForge.Cli.OutputText.Shared.SharedText.TitleRecoveryDataIsUnavailable(),
        OpenForge.Cli.Core.Commands.Index.Models.Result.IndexFindingCode.RecoveryArtifactRetained => global::OpenForge.Cli.OutputText.Shared.SharedText.TitleRecoveryBundleWasRetained(),
        OpenForge.Cli.Core.Commands.Index.Models.Result.IndexFindingCode.TargetChangedDuringApply => global::OpenForge.Cli.OutputText.Shared.SharedText.TitleTargetChangedDuringTheWrite(),
        OpenForge.Cli.Core.Commands.Index.Models.Result.IndexFindingCode.WriteFailed => global::OpenForge.Cli.OutputText.Shared.SharedText.TitleWriteFailed(),
        OpenForge.Cli.Core.Commands.Index.Models.Result.IndexFindingCode.VerificationFailed => global::OpenForge.Cli.OutputText.Shared.SharedText.TitleVerificationFailed(),
        OpenForge.Cli.Core.Commands.Index.Models.Result.IndexFindingCode.RecoveryFailed => global::OpenForge.Cli.OutputText.Shared.SharedText.TitleRecoveryStateIsUnknown(),
        OpenForge.Cli.Core.Commands.Index.Models.Result.IndexFindingCode.OperationFailed => global::OpenForge.Cli.OutputText.Index.IndexText.TitleIndexFailed(),
        OpenForge.Cli.Core.Commands.Index.Models.Result.IndexFindingCode.Interrupted => global::OpenForge.Cli.OutputText.Index.IndexText.TitleIndexWasCancelled(),
        _ => throw new ArgumentOutOfRangeException(nameof(code)),
    };
    internal static string Current(long files) => files == 1 ? global::OpenForge.Cli.OutputText.Index.IndexText.MessageTheEntriesSectionIsCurrentIn1FileNothingToDo()
        : global::OpenForge.Cli.OutputText.Index.IndexPhrases.FormatEntriesSectionsAreCurrentInAllFilesNothingToDo(string.Create(global::System.Globalization.CultureInfo.InvariantCulture, $"{files}"));
    internal static string Updated(long updated, long files) => global::OpenForge.Cli.OutputText.Index.IndexPhrases.FormatUpdatedTheEntriesSectionInOf(string.Create(global::System.Globalization.CultureInfo.InvariantCulture, $"{updated}"), string.Create(global::System.Globalization.CultureInfo.InvariantCulture, $"{files}"), string.Create(global::System.Globalization.CultureInfo.InvariantCulture, $"{CliText.Plural(files, "file")}"));
    internal static string Preview(long updated, long files) => global::OpenForge.Cli.OutputText.Index.IndexPhrases.FormatWouldUpdateTheEntriesSectionInOf(string.Create(global::System.Globalization.CultureInfo.InvariantCulture, $"{updated}"), string.Create(global::System.Globalization.CultureInfo.InvariantCulture, $"{files}"), string.Create(global::System.Globalization.CultureInfo.InvariantCulture, $"{CliText.Plural(files, "file")}"));
    internal static string Partial(long updated, long files, bool preview) => preview
        ? global::OpenForge.Cli.OutputText.Index.IndexPhrases.FormatWouldUpdateTheEntriesSectionInOfOtherSourcesWereSkipped(string.Create(global::System.Globalization.CultureInfo.InvariantCulture, $"{updated}"), string.Create(global::System.Globalization.CultureInfo.InvariantCulture, $"{files}"), string.Create(global::System.Globalization.CultureInfo.InvariantCulture, $"{CliText.Plural(files, "file")}"))
        : global::OpenForge.Cli.OutputText.Index.IndexPhrases.FormatUpdatedTheEntriesSectionInOfOtherSourcesWereSkipped(string.Create(global::System.Globalization.CultureInfo.InvariantCulture, $"{updated}"), string.Create(global::System.Globalization.CultureInfo.InvariantCulture, $"{files}"), string.Create(global::System.Globalization.CultureInfo.InvariantCulture, $"{CliText.Plural(files, "file")}"));
    internal static string Incomplete() => global::OpenForge.Cli.OutputText.Index.IndexText.MessageTheEntriesSectionsCouldNotBeRebuiltCompletelyNothingWasChanged();
    internal static string MetadataOptional(string path) => global::OpenForge.Cli.OutputText.Index.IndexPhrases.FormatOptionalMetadataIsMissingForObservedValuesWereUsed(path);
    internal static string MetadataSkipped(string path) => global::OpenForge.Cli.OutputText.Index.IndexPhrases.FormatSkippedBecauseAuthoredMetadataIsMalformed(path);
    internal static string CannotIndex(string operand, string reason) => global::OpenForge.Cli.OutputText.Index.IndexWording.CannotIndex(operand, reason);
    internal static string Blocked(string parent) => global::OpenForge.Cli.OutputText.Index.IndexWording.Blocked(parent);
    internal static string Failed(long updated, long files) => global::OpenForge.Cli.OutputText.Index.IndexPhrases.FormatIndexStoppedAfterOf(string.Create(global::System.Globalization.CultureInfo.InvariantCulture, $"{updated}"), string.Create(global::System.Globalization.CultureInfo.InvariantCulture, $"{files}"), string.Create(global::System.Globalization.CultureInfo.InvariantCulture, $"{CliText.Plural(files, "file")}"));
    internal static string Cancelled() => global::OpenForge.Cli.OutputText.Index.IndexText.MessageIndexWasCancelledNothingWasChanged();
    internal static string CancelledAfter(long updated, long files) => global::OpenForge.Cli.OutputText.Index.IndexPhrases.FormatIndexWasCancelledStoppedAfterOf(string.Create(global::System.Globalization.CultureInfo.InvariantCulture, $"{updated}"), string.Create(global::System.Globalization.CultureInfo.InvariantCulture, $"{files}"), string.Create(global::System.Globalization.CultureInfo.InvariantCulture, $"{CliText.Plural(files, "file")}"));
    internal static string NoChanges() => global::OpenForge.Cli.OutputText.Shared.SharedText.MessageNoFilesWereChanged();
    internal static string OtherCurrent(long files) => global::OpenForge.Cli.OutputText.Index.IndexPhrases.FormatNothingWasWrittenTheOtherCurrent(string.Create(global::System.Globalization.CultureInfo.InvariantCulture, $"{files}"), string.Create(global::System.Globalization.CultureInfo.InvariantCulture, $"{CliText.Plural(files, "section")}"), string.Create(global::System.Globalization.CultureInfo.InvariantCulture, $"{CliText.Plural(files, "is", "are")}"));
    internal static string Unchanged(long files) => global::OpenForge.Cli.OutputText.Index.IndexPhrases.FormatCurrent(string.Create(global::System.Globalization.CultureInfo.InvariantCulture, $"{files}"), string.Create(global::System.Globalization.CultureInfo.InvariantCulture, $"{CliText.Plural(files, "file")}"), string.Create(global::System.Globalization.CultureInfo.InvariantCulture, $"{CliText.Plural(files, "is", "are")}"));
    internal static string EntryCounts(int? before, int? after) => global::OpenForge.Cli.OutputText.Index.IndexPhrases.FormatEntries($"{before?.ToString(CultureInfo.InvariantCulture) ?? "unknown"}", $"{after?.ToString(CultureInfo.InvariantCulture) ?? "unknown"}");
    internal static string NotStarted() => global::OpenForge.Cli.OutputText.Shared.SharedText.LabelNotStarted();
    internal static string Unknown() => global::OpenForge.Cli.OutputText.Shared.SharedText.LabelFinalStateUnknown();
    internal static string CurrentRow() => global::OpenForge.Cli.OutputText.Shared.SharedText.LabelCurrent();
    internal static string EntriesSection(string path) => global::OpenForge.Cli.OutputText.Index.IndexWording.EntriesSection(path);
    internal static string LoaderSelection() => global::OpenForge.Cli.OutputText.Index.IndexText.TitleSelectionLoaderRoots();
    internal static string ExplicitSelection(string sources) => global::OpenForge.Cli.OutputText.Index.IndexWording.ExplicitSelection(sources);
    internal static string NoRecovery() => global::OpenForge.Cli.OutputText.Shared.SharedText.MessageNoRecoveryBundleWasNeeded();
    internal static string RecoveryNotCreated() => global::OpenForge.Cli.OutputText.Shared.SharedText.MessageNoRecoveryBundleWasCreated();
    internal static string RecoveryRemoved() => global::OpenForge.Cli.OutputText.Shared.SharedText.MessageTheRecoveryBundleWasRemoved();
    internal static string RecoveryRetained(string path) => global::OpenForge.Cli.OutputText.Index.IndexWording.RecoveryRetained(path);
    internal static string RecoveryUnknown() => global::OpenForge.Cli.OutputText.Shared.SharedText.MessageTheFinalStateOfTheRecoveryBundleIsUnknown();
    internal static string FrontmatterInvalidTitle() => global::OpenForge.Cli.OutputText.Shared.SharedText.TitleFrontmatterIsInvalid();
    internal static string FrontmatterUnclosed() => global::OpenForge.Cli.OutputText.Index.IndexText.MessageTheFrontmatterBlockIsNotClosed();
    internal static string UnknownSource(string operand) => global::OpenForge.Cli.OutputText.Index.IndexWording.UnknownSource(operand);
    internal static string FolderReason() => global::OpenForge.Cli.OutputText.Index.IndexText.LabelItIsAFolderNotASource();
    internal static string FolderSource(string operand) => global::OpenForge.Cli.OutputText.Index.IndexWording.FolderSource(operand);
    internal static string CorrectedSource(string id) => global::OpenForge.Cli.OutputText.Index.IndexWording.CorrectedSource(id);
    internal static string TopologyAmbiguous(string source, string reason) => global::OpenForge.Cli.OutputText.Index.IndexWording.TopologyAmbiguous(source, reason);
    internal static string TargetUnexposed(string path) => global::OpenForge.Cli.OutputText.Index.IndexWording.TargetUnexposed(path);
    internal static string DiscoveryIncomplete(string path) => global::OpenForge.Cli.OutputText.Index.IndexWording.DiscoveryIncomplete(path);
}
