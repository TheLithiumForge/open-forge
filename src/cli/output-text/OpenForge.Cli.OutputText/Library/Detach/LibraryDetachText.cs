using System.Globalization;

namespace OpenForge.Cli.OutputText.Library.Detach;

internal static class LibraryDetachText
{
    // @OpenForgeText library.detach.message.library-detach-was-cancelled-nothing-was-changed
    internal static string MessageLibraryDetachWasCancelledNothingWasChanged()
        => "Library detach was cancelled. Nothing was changed.";

    // @OpenForgeText library.detach.message.this-system-cannot-handle-the-file-links-libraries-use
    internal static string MessageThisSystemCannotHandleTheFileLinksLibrariesUse()
        => "This system cannot handle the file links Libraries use.";

    // @OpenForgeText library.detach.message.it-writes-outside-agents-and-no-grant-allows-that
    internal static string MessageItWritesOutsideAgentsAndNoGrantAllowsThat()
        => "it writes outside .agents and no grant allows that.";

    // @OpenForgeText library.detach.help.syntax
    internal static string HelpSyntax()
        => "open-forge library detach <library-id> [--dry-run] [--automatic] [--allow-path <path>] [global options]";

    // @OpenForgeText library.detach.help.policy
    internal static string HelpPolicy()
        => "Remove the registered destination links while preserving source files. Use --dry-run to preview all changes without writing, or --automatic for a non-interactive apply.";

    // @OpenForgeText library.detach.help.examples
    internal static string HelpExamples()
        => "open-forge library detach shared --dry-run\n  open-forge library detach shared --automatic";

    // @OpenForgeText library.detach.message.review-the-detached-library-state-if-needed
    internal static string MessageReviewTheDetachedLibraryStateIfNeeded()
        => "Review the detached Library state if needed.";

    // @OpenForgeText library.detach.message.retry-the-same-library-detach-request-with-bounded-diagnostics
    internal static string MessageRetryTheSameLibraryDetachRequestWithBoundedDiagnostics()
        => "Retry the same Library detach request with bounded diagnostics.";

    // @OpenForgeText library.detach.message.rerun-the-same-library-detach-request
    internal static string MessageRerunTheSameLibraryDetachRequest()
        => "Rerun the same Library detach request.";

    // @OpenForgeText library.detach.label.the-detach-could-not-be-checked
    internal static string LabelTheDetachCouldNotBeChecked()
        => "the detach could not be checked";

    // @OpenForgeText library.detach.label.the-detach-is-blocked
    internal static string LabelTheDetachIsBlocked()
        => "the detach is blocked";

    // @OpenForgeText library.detach.message.grant-the-destination-path-then-rerun-the-detach-request
    internal static string MessageGrantTheDestinationPathThenRerunTheDetachRequest()
        => "Grant the destination path, then rerun the detach request.";

    // @OpenForgeText library.detach.message.rerun-the-same-library-detach-request-with-explicit-automatic-mode
    internal static string MessageRerunTheSameLibraryDetachRequestWithExplicitAutomaticMode()
        => "Rerun the same Library Detach request with explicit automatic mode.";

    // @OpenForgeText library.detach.label.link-removed
    internal static string LabelLinkRemoved()
        => "link removed";

    // @OpenForgeText library.detach.label.detach
    internal static string LabelDetach()
        => "detach";

    // @OpenForgeText library.detach.title.library-detach
    internal static string TitleLibraryDetach()
        => "Library detach";

    // @OpenForgeText library.detach.label.would-remove
    internal static string LabelWouldRemove()
        => "would remove";

    // @OpenForgeText library.detach.label.registration-would-be-removed
    internal static string LabelRegistrationWouldBeRemoved()
        => "registration would be removed";

    // @OpenForgeText library.detach.label.registration-removed
    internal static string LabelRegistrationRemoved()
        => "registration removed";

    // @OpenForgeText library.detach.label.no-residual-effects
    internal static string LabelNoResidualEffects()
        => "no residual effects";

    // @OpenForgeText library.detach.label.was-not-requested
    internal static string LabelWasNotRequested()
        => "was not requested";

    // @OpenForgeText library.detach.label.all-changed-targets-were-verified
    internal static string LabelAllChangedTargetsWereVerified()
        => "all changed targets were verified";

    // @OpenForgeText library.detach.label.changed-targets-did-not-verify
    internal static string LabelChangedTargetsDidNotVerify()
        => "changed targets did not verify";

    // @OpenForgeText library.detach.label.could-not-be-completed
    internal static string LabelCouldNotBeCompleted()
        => "could not be completed";

    // @OpenForgeText library.detach.label.was-not-published
    internal static string LabelWasNotPublished()
        => "was not published";

    // @OpenForgeText library.detach.label.was-published-and-verified
    internal static string LabelWasPublishedAndVerified()
        => "was published and verified";

    // @OpenForgeText library.detach.label.publication-failed
    internal static string LabelPublicationFailed()
        => "publication failed";

    // @OpenForgeText library.detach.label.final-publication-state-is-unknown
    internal static string LabelFinalPublicationStateIsUnknown()
        => "final publication state is unknown";

    // @OpenForgeText library.detach.title.library-consumer-is-blocked
    internal static string TitleLibraryConsumerIsBlocked()
        => "Library consumer is blocked";

    // @OpenForgeText library.detach.title.library-destination-is-protected
    internal static string TitleLibraryDestinationIsProtected()
        => "Library destination is protected";

    // @OpenForgeText library.detach.title.detach-verification-failed
    internal static string TitleDetachVerificationFailed()
        => "Detach verification failed";

    // @OpenForgeText library.detach.title.library-detach-failed
    internal static string TitleLibraryDetachFailed()
        => "Library detach failed";

    // @OpenForgeText library.detach.title.library-detach-was-cancelled
    internal static string TitleLibraryDetachWasCancelled()
        => "Library detach was cancelled";

    // @OpenForgeText library.detach.label.a-folder
    internal static string LabelAFolder()
        => "a folder";
}
