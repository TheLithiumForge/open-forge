using System.Globalization;

namespace OpenForge.Cli.OutputText.References;

internal static class ReferencesText
{
    // @OpenForgeText references.message.invalid-direction-value
    internal static string InvalidDirection(string value) => $"Invalid --direction value: {value}. Use in, out, or both.";

    // @OpenForgeText references.message.the-scan-is-incomplete
    internal static string MessageTheScanIsIncomplete()
        => "The scan is incomplete.";

    // @OpenForgeText references.message.direction-must-be-in-out-or-both
    internal static string MessageDirectionMustBeInOutOrBoth()
        => "--direction must be in, out, or both.";

    // @OpenForgeText references.message.include-and-exclude-apply-to-incoming-links-use-direction-in-or-both
    internal static string MessageIncludeAndExcludeApplyToIncomingLinksUseDirectionInOrBoth()
        => "--include and --exclude apply to incoming links. Use --direction in or both.";

    // @OpenForgeText references.label.check-the-workspace-for-broken-links
    internal static string LabelCheckTheWorkspaceForBrokenLinks()
        => "check the workspace for broken links";

    // @OpenForgeText references.label.list-the-sources-that-exist
    internal static string LabelListTheSourcesThatExist()
        => "list the sources that exist";

    // @OpenForgeText references.help.next-reason
    internal static string HelpNextReason()
        => "see the accepted options";

    // @OpenForgeText references.label.see-the-diagnostics-for-this-failure
    internal static string LabelSeeTheDiagnosticsForThisFailure()
        => "see the diagnostics for this failure";

    // @OpenForgeText references.label.run-it-again
    internal static string LabelRunItAgain()
        => "run it again";

    // @OpenForgeText references.label.incoming
    internal static string LabelIncoming()
        => "incoming";

    // @OpenForgeText references.label.outgoing
    internal static string LabelOutgoing()
        => "outgoing";

    // @OpenForgeText references.label.sources-scanned
    internal static string LabelSourcesScanned()
        => "sources scanned";

    // @OpenForgeText references.label.the-link
    internal static string LabelTheLink()
        => "the link";

    // @OpenForgeText references.label.not-a-resolvable-link
    internal static string LabelNotAResolvableLink()
        => "not a resolvable link";

    // @OpenForgeText references.label.heading-not-found
    internal static string LabelHeadingNotFound()
        => "heading not found";

    // @OpenForgeText references.label.target-could-not-be-read
    internal static string LabelTargetCouldNotBeRead()
        => "target could not be read";

    // @OpenForgeText references.label.matches-the-same-automatic-id
    internal static string LabelMatchesTheSameAutomaticId()
        => "matches the same automatic ID";

    // @OpenForgeText references.label.resolves-to-the-same-physical-file
    internal static string LabelResolvesToTheSamePhysicalFile()
        => "resolves to the same physical file";

    // @OpenForgeText references.label.list-references
    internal static string LabelListReferences()
        => "list references";

    // @OpenForgeText references.title.references
    internal static string TitleReferences()
        => "References";

    // @OpenForgeText references.message.the-source-identity-collision-could-not-be-described-because-its-id-is-unavailable
    internal static string MessageTheSourceIdentityCollisionCouldNotBeDescribedBecauseItsIdIsUnavailable()
        => "The source identity collision could not be described because its ID is unavailable.";

    // @OpenForgeText references.message.the-physical-alias-could-not-be-described-because-its-paths-are-unavailable
    internal static string MessageThePhysicalAliasCouldNotBeDescribedBecauseItsPathsAreUnavailable()
        => "The physical alias could not be described because its paths are unavailable.";

    // @OpenForgeText references.message.the-identity-of-the-source-could-not-be-determined-because-its-path-is-unavailable
    internal static string MessageTheIdentityOfTheSourceCouldNotBeDeterminedBecauseItsPathIsUnavailable()
        => "The identity of the source could not be determined because its path is unavailable.";

    // @OpenForgeText references.label.points-outside-the-workspace
    internal static string LabelPointsOutsideTheWorkspace()
        => "points outside the workspace";

    // @OpenForgeText references.label.could-point-to-more-than-one-file
    internal static string LabelCouldPointToMoreThanOneFile()
        => "could point to more than one file";

    // @OpenForgeText references.title.direction-is-invalid
    internal static string TitleDirectionIsInvalid()
        => "Direction is invalid";

    // @OpenForgeText references.title.filter-is-invalid
    internal static string TitleFilterIsInvalid()
        => "Filter is invalid";

    // @OpenForgeText references.title.filter-is-ambiguous
    internal static string TitleFilterIsAmbiguous()
        => "Filter is ambiguous";

    // @OpenForgeText references.title.filter-is-unsafe
    internal static string TitleFilterIsUnsafe()
        => "Filter is unsafe";

    // @OpenForgeText references.title.two-sources-share-an-identity
    internal static string TitleTwoSourcesShareAnIdentity()
        => "Two sources share an identity";

    // @OpenForgeText references.title.sources-share-a-physical-file
    internal static string TitleSourcesShareAPhysicalFile()
        => "Sources share a physical file";

    // @OpenForgeText references.title.source-identity-is-unavailable
    internal static string TitleSourceIdentityIsUnavailable()
        => "Source identity is unavailable";

    // @OpenForgeText references.title.source-could-not-be-scanned-safely
    internal static string TitleSourceCouldNotBeScannedSafely()
        => "Source could not be scanned safely";

    // @OpenForgeText references.title.entries-section-could-not-be-identified
    internal static string TitleEntriesSectionCouldNotBeIdentified()
        => "Entries section could not be identified";

    // @OpenForgeText references.title.link-is-not-resolvable
    internal static string TitleLinkIsNotResolvable()
        => "Link is not resolvable";

    // @OpenForgeText references.title.link-was-not-followed
    internal static string TitleLinkWasNotFollowed()
        => "Link was not followed";

    // @OpenForgeText references.title.heading-was-not-found
    internal static string TitleHeadingWasNotFound()
        => "Heading was not found";

    // @OpenForgeText references.title.link-points-outside-the-workspace
    internal static string TitleLinkPointsOutsideTheWorkspace()
        => "Link points outside the workspace";

    // @OpenForgeText references.title.link-could-point-to-more-than-one-file
    internal static string TitleLinkCouldPointToMoreThanOneFile()
        => "Link could point to more than one file";

    // @OpenForgeText references.title.references-failed
    internal static string TitleReferencesFailed()
        => "References failed";

    // @OpenForgeText references.title.references-was-cancelled
    internal static string TitleReferencesWasCancelled()
        => "References was cancelled";

    // @OpenForgeText references.message.the-incoming-scan-used-every-source
    internal static string MessageTheIncomingScanUsedEverySource()
        => "The incoming scan used every source.";

    // @OpenForgeText references.help.syntax
    internal static string HelpSyntax()
        => "open-forge references <source-reference> [options]";

    // @OpenForgeText references.help.heading.source-and-direction
    internal static string HelpHeadingSourceAndDirection()
        => "Source and direction";

    // @OpenForgeText references.help.source-and-direction
    internal static string HelpSourceAndDirection()
        => "source-reference accepts one source ID or exact .agents/... Path. --direction selects in, out, or both. Default: both. Direction values are exact and case-sensitive.";

    // @OpenForgeText references.help.heading.incoming-filters
    internal static string HelpHeadingIncomingFilters()
        => "Incoming filters";

    // @OpenForgeText references.help.incoming-filters
    internal static string HelpIncomingFilters()
        => "--include and --exclude each consume one source reference each time. All includes are combined, exclusions take priority, and filters apply only to incoming references. They cannot be used with --direction out.";

    // @OpenForgeText references.help.examples
    internal static string HelpExamples()
        => "open-forge references memory\nopen-forge references skills/open-forge-cli --direction out\nopen-forge references memory --direction in";

    // @OpenForgeText references.help.related-commands
    internal static IReadOnlyList<(string Label, string? Description)> HelpRelatedCommands()
        =>
        [
            ("context --help", "Read agent instructions and context."),
            ("doctor", "Diagnose workspace problems."),
            ("repair --help", "Correct verified link and Library problems."),
        ];

    // @OpenForgeText references.help.notes
    internal static string HelpNotes()
        => "References is a deterministic, read-only report of direct references. It does not fetch URLs, load target bodies, follow links, write files, build an index, or repair destinations.";

    // @OpenForgeText references.references.help.behavior
    internal static string HelpBehavior()
        => "Supply one source ID or exact .agents path. The default direction is both.\nGenerated Entries links are excluded. Incoming filters select which sources are inspected.\nThis command changes no files and asks no questions.";
    // @OpenForgeText references.help.details-1
    internal static string HelpDirectionAndFiltersDetails()
        => "Direction values in, out, and both are exact and case-sensitive. Each --include or --exclude consumes one source reference. Includes combine and exclusions take priority. Filters affect only incoming references and cannot accompany --direction out.";

    // @OpenForgeText references.help.details-2
    internal static string HelpBoundaryDetails()
        => "This deterministic report does not fetch URLs, load target bodies, follow links, build an index, or repair destinations.";
}
