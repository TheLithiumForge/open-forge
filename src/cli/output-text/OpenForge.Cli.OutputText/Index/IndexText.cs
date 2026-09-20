using System.Globalization;

namespace OpenForge.Cli.OutputText.Index;

internal static class IndexText
{
    // @OpenForgeText index.help.syntax
    internal static string HelpSyntax()
        => "open-forge index [source-reference...] [--dry-run] [global options]";

    // @OpenForgeText index.help.selection
    internal static string HelpSelection()
        => "With no source operands, Index rebuilds the Loader and every reachable entrypoint region. An entrypoint selects its subtree and direct exposing parent; a routed leaf selects its direct exposing parent.";

    // @OpenForgeText index.help.write-policy
    internal static string HelpWritePolicy()
        => "Omit --dry-run to apply bounded generated-interior changes. --dry-run previews the same complete checked plan and writes nothing. Repeating the command makes no further changes.";

    // @OpenForgeText index.help.examples
    internal static string HelpExamples()
        => "open-forge index\n  open-forge index memory --dry-run\n  open-forge index .agents/memory/_memory.md --format json";

    // @OpenForgeText index.help.notes
    internal static string HelpNotes()
        => "Index rewrites the body of one unique ## Entries section and removes retired generated guard comments there. It does not format complete files, modify overwrites, search for another workspace, or create Git commits.";

    // @OpenForgeText index.message.choose-a-source-id-or-exact-path
    internal static string MessageChooseASourceIdOrExactPath()
        => "Choose a source ID or exact path.";

    // @OpenForgeText index.message.correct-the-named-input
    internal static string MessageCorrectTheNamedInput()
        => "Correct the named input.";

    // @OpenForgeText index.message.retry-when-the-other-command-finishes
    internal static string MessageRetryWhenTheOtherCommandFinishes()
        => "Retry when the other command finishes.";

    // @OpenForgeText index.message.review-the-retained-recovery-bundle
    internal static string MessageReviewTheRetainedRecoveryBundle()
        => "Review the retained recovery bundle.";

    // @OpenForgeText index.message.rerun-the-same-index-request
    internal static string MessageRerunTheSameIndexRequest()
        => "Rerun the same Index request.";

    // @OpenForgeText index.message.inspect-the-reported-problem-before-rerunning-index
    internal static string MessageInspectTheReportedProblemBeforeRerunningIndex()
        => "Inspect the reported problem before rerunning Index.";

    // @OpenForgeText index.message.the-entries-sections-could-not-be-rebuilt-completely-nothing-was-changed
    internal static string MessageTheEntriesSectionsCouldNotBeRebuiltCompletelyNothingWasChanged()
        => "The Entries sections could not be rebuilt completely. Nothing was changed.";

    // @OpenForgeText index.message.index-was-cancelled-nothing-was-changed
    internal static string MessageIndexWasCancelledNothingWasChanged()
        => "Index was cancelled. Nothing was changed.";

    // @OpenForgeText index.title.selection-loader-roots
    internal static string TitleSelectionLoaderRoots()
        => "Selection: loader roots";

    // @OpenForgeText index.message.the-frontmatter-block-is-not-closed
    internal static string MessageTheFrontmatterBlockIsNotClosed()
        => "The frontmatter block is not closed.";

    // @OpenForgeText index.title.optional-metadata-is-missing
    internal static string TitleOptionalMetadataIsMissing()
        => "Optional metadata is missing";

    // @OpenForgeText index.title.metadata-was-skipped
    internal static string TitleMetadataWasSkipped()
        => "Metadata was skipped";

    // @OpenForgeText index.label.it-is-a-folder-not-a-source
    internal static string LabelItIsAFolderNotASource()
        => "it is a folder, not a source";

    // @OpenForgeText index.label.files-checked
    internal static string LabelFilesChecked()
        => "files checked";

    // @OpenForgeText index.label.files-updated
    internal static string LabelFilesUpdated()
        => "files updated";

    // @OpenForgeText index.label.files-current
    internal static string LabelFilesCurrent()
        => "files current";

    // @OpenForgeText index.message.open-forge-doctor-inspect-blocked-topology-metadata-or-generated-region-facts-open-forge-cleanup-remove-a-reported-retained-recovery-artifact-after-review
    internal static string MessageOpenForgeDoctorInspectBlockedTopologyMetadataOrGeneratedRegionFactsOpenForgeCleanupRemoveAReportedRetainedRecoveryArtifactAfterReview()
        => "open-forge doctor — inspect blocked topology, metadata, or generated-region facts.\n  open-forge cleanup — remove a reported retained recovery artifact after review.";

    // @OpenForgeText index.title.source-is-not-listed
    internal static string TitleSourceIsNotListed()
        => "Source is not listed";

    // @OpenForgeText index.title.routes-could-not-be-read
    internal static string TitleRoutesCouldNotBeRead()
        => "Routes could not be read";

    // @OpenForgeText index.title.index-failed
    internal static string TitleIndexFailed()
        => "Index failed";

    // @OpenForgeText index.title.index-was-cancelled
    internal static string TitleIndexWasCancelled()
        => "Index was cancelled";

    // @OpenForgeText index.message.the-entries-section-is-current-in-1-file-nothing-to-do
    internal static string MessageTheEntriesSectionIsCurrentIn1FileNothingToDo()
        => "The Entries section is current in 1 file. Nothing to do.";

    // @OpenForgeText index.label.index
    internal static string LabelIndex()
        => "index";

    // @OpenForgeText index.title.index
    internal static string TitleIndex()
        => "Index";
}
