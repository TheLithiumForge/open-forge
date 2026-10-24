using System.Globalization;

namespace OpenForge.Cli.OutputText.Route.List;

internal static class RouteListText
{
    // @OpenForgeText route.list.label.registered-include-unlisted
    internal static string RegisteredIncludeUnlisted() => "Registered include (not in the loader)";

    // @OpenForgeText route.list.label.missing-include
    internal static string MissingInclude(string description) => $"{description} (missing)";

    // @OpenForgeText route.list.message.invalid-depth-value
    internal static string InvalidDepth(string value) => $"Invalid --depth value: {value}. Use a whole number from 0, or all.";

    // @OpenForgeText route.list.message.depth-help
    internal static string DepthHelp() => "Choose a supported depth using Route List help.";

    // @OpenForgeText route.list.message.the-listing-is-incomplete
    internal static string MessageTheListingIsIncomplete()
        => "The listing is incomplete.";

    // @OpenForgeText route.list.message.route-list-was-cancelled
    internal static string MessageRouteListWasCancelled()
        => "Route list was cancelled.";

    // @OpenForgeText route.list.message.depth-must-be-a-whole-number-or-all
    internal static string MessageDepthMustBeAWholeNumberOrAll()
        => "--depth must be a whole number or all.";

    // @OpenForgeText route.list.message.the-loader-is-the-root-of-every-route-run-route-list-without-an-operand
    internal static string MessageTheLoaderIsTheRootOfEveryRouteRunRouteListWithoutAnOperand()
        => "The Loader is the root of every route. Run route list without an operand.";

    internal static string MessageLoaderCouldNotBeRead(string loaderPath)
        => $"{loaderPath} could not be read.";

    internal static string MessageLoaderHasNoUsableEntriesSection(string loaderPath)
        => $"{loaderPath} has no usable Entries section.";

    // @OpenForgeText route.list.message.agents-loader-md-could-not-be-read
    internal static string MessageAgentsLoaderMdCouldNotBeRead()
        => ".agents/loader.md could not be read.";

    // @OpenForgeText route.list.message.agents-loader-md-has-no-usable-entries-section
    internal static string MessageAgentsLoaderMdHasNoUsableEntriesSection()
        => ".agents/loader.md has no usable Entries section.";

    // @OpenForgeText route.list.title.selected-as
    internal static string TitleSelectedAs()
        => "Selected as";

    // @OpenForgeText route.list.label.routes
    internal static string LabelRoutes()
        => "routes";

    // @OpenForgeText route.list.label.roots
    internal static string LabelRoots()
        => "roots";

    // @OpenForgeText route.list.label.depth
    internal static string LabelDepth()
        => "depth";

    // @OpenForgeText route.list.title.invalid-depth
    internal static string TitleInvalidDepth()
        => "Invalid depth";

    // @OpenForgeText route.list.title.source-was-not-found
    internal static string TitleSourceWasNotFound()
        => "Source was not found";

    // @OpenForgeText route.list.title.the-loader-is-not-a-route
    internal static string TitleTheLoaderIsNotARoute()
        => "The Loader is not a route";

    // @OpenForgeText route.list.title.loader-is-unavailable
    internal static string TitleLoaderIsUnavailable()
        => "Loader is unavailable";

    // @OpenForgeText route.list.title.loader-is-malformed
    internal static string TitleLoaderIsMalformed()
        => "Loader is malformed";

    // @OpenForgeText route.list.title.description-is-missing
    internal static string TitleDescriptionIsMissing()
        => "Description is missing";

    // @OpenForgeText route.list.title.source-metadata-is-malformed
    internal static string TitleSourceMetadataIsMalformed()
        => "Source metadata is malformed";

    // @OpenForgeText route.list.title.route-boundary-is-unreadable
    internal static string TitleRouteBoundaryIsUnreadable()
        => "Route boundary is unreadable";

    // @OpenForgeText route.list.title.route-boundary-is-outside-the-workspace
    internal static string TitleRouteBoundaryIsOutsideTheWorkspace()
        => "Route boundary is outside the workspace";

    // @OpenForgeText route.list.title.route-list-failed
    internal static string TitleRouteListFailed()
        => "Route list failed";

    // @OpenForgeText route.list.title.route-list-was-cancelled
    internal static string TitleRouteListWasCancelled()
        => "Route list was cancelled";

    // @OpenForgeText route.list.label.the-supplied-source
    internal static string LabelTheSuppliedSource()
        => "the supplied source";

    // @OpenForgeText route.list.title.route-list
    internal static string TitleRouteList()
        => "Route list";

    // @OpenForgeText route.list.message.inspect-the-route-boundary-with-open-forge-doctor
    internal static string MessageInspectTheRouteBoundaryWithOpenForgeDoctor()
        => "Inspect the route boundary with open-forge doctor.";

    // @OpenForgeText route.list.message.add-the-missing-route-description-then-rerun-the-listing
    internal static string MessageAddTheMissingRouteDescriptionThenRerunTheListing()
        => "Add the missing route description, then rerun the listing.";

    // @OpenForgeText route.list.message.repair-the-route-metadata-then-rerun-the-listing
    internal static string MessageRepairTheRouteMetadataThenRerunTheListing()
        => "Repair the route metadata, then rerun the listing.";

    // @OpenForgeText route.list.message.rename-the-compatibility-entrypoint-then-rerun-the-listing
    internal static string MessageRenameTheCompatibilityEntrypointThenRerunTheListing()
        => "Rename the compatibility entrypoint, then rerun the listing.";

    // @OpenForgeText route.list.message.retry-the-same-route-listing-with-bounded-diagnostics
    internal static string MessageRetryTheSameRouteListingWithBoundedDiagnostics()
        => "Retry the same route listing with bounded diagnostics.";

    // @OpenForgeText route.list.message.rerun-the-same-route-listing
    internal static string MessageRerunTheSameRouteListing()
        => "Rerun the same route listing.";

    // @OpenForgeText route.list.message.the-loader-exposes-no-routes
    internal static string MessageTheLoaderExposesNoRoutes()
        => "The Loader exposes no routes.";

    // @OpenForgeText route.list.label.entrypoint
    internal static string LabelEntrypoint()
        => "entrypoint";

    // @OpenForgeText route.list.label.loader-root
    internal static string LabelLoaderRoot()
        => "loader root";

    // @OpenForgeText route.list.label.explicit-root
    internal static string LabelExplicitRoot()
        => "explicit root";

    // @OpenForgeText route.list.label.detached-root
    internal static string LabelDetachedRoot()
        => "detached root";

    // @OpenForgeText route.list.label.descendant
    internal static string LabelDescendant()
        => "descendant";

    // @OpenForgeText route.list.help.heading.depth
    internal static string HelpHeadingDepth()
        => "Depth";

    // @OpenForgeText route.list.title.kind
    internal static string TitleKind()
        => "Kind";

    // @OpenForgeText route.list.title.overwrite
    internal static string TitleOverwrite()
        => "Overwrite";

    // @OpenForgeText route.list.help.syntax
    internal static string HelpSyntax()
        => "open-forge route list [<source-reference>] [options]";

    // @OpenForgeText route.list.help.source-references
    internal static string HelpSourceReferences()
        => "source-reference is either an exact source ID such as memory or an exact .agents/... Or ./.agents/... Path such as .agents/memory/_memory.md. IDs use the current workspace and are not fuzzy or path guesses.";

    // @OpenForgeText route.list.help.depth
    internal static string HelpDepth()
        => "The default depth is 1. Use --depth 0 for selected roots only, a whole number from 0 to 2147483647 for a bounded descendant depth, or --depth all for all routed descendants. Both --depth 1 and --depth=1 are accepted.";

    // @OpenForgeText route.list.help.inherited-global-options
    internal static string HelpInheritedGlobalOptions()
        => "Available on this command: --workspace <path>, --format <text|json>, --detail <minimal|standard|full|debug>, --detail-filter <error|warning|info|all>, --help, and --version. --detail selects detail in text and JSON.";

    // @OpenForgeText route.list.help.examples
    internal static string HelpExamples()
        => "open-forge route list\nopen-forge route list memory\nopen-forge route list memory --depth all";

    // @OpenForgeText route.list.help.related-commands
    internal static IReadOnlyList<(string Label, string? Description)> HelpRelatedCommands()
        =>
        [
            ("route inspect --help", "Inspect one routed source."),
            ("context --help", "Read agent instructions and context."),
            ("find --help", "Find sources by tags or headings."),
        ];

    // @OpenForgeText route.list.routelist.help.behavior
    internal static string HelpBehavior()
        => "With no source, list the root routes and their direct children.\nThe default descendant depth is 1. --depth 0 lists selected roots only.\nThis command changes no files and asks no questions.";
    // @OpenForgeText route.list.help.details-1
    internal static string HelpSourcesAndDepthDetails()
        => "Exact .agents/... and ./.agents/... paths are accepted, such as .agents/memory/_memory.md. IDs belong to the current workspace and are not fuzzy matches or path guesses. Depth accepts integers from 0 to 2147483647 or all. Both --depth 1 and --depth=1 are accepted.";
}
