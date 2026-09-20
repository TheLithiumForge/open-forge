using System.Globalization;

namespace OpenForge.Cli.OutputText.Route.Inspect;

internal static class RouteInspectText
{
    // @OpenForgeText route.inspect.message.a-source-reference-is-required
    internal static string MessageASourceReferenceIsRequired()
        => "A source reference is required.";

    // @OpenForgeText route.inspect.message.route-inspect-takes-one-source
    internal static string MessageRouteInspectTakesOneSource()
        => "Route inspect takes one source.";

    // @OpenForgeText route.inspect.message.the-loader-is-not-a-route-inspect-one-of-its-routes-instead
    internal static string MessageTheLoaderIsNotARouteInspectOneOfItsRoutesInstead()
        => "The Loader is not a route. Inspect one of its routes instead.";

    // @OpenForgeText route.inspect.message.route-inspect-was-cancelled
    internal static string MessageRouteInspectWasCancelled()
        => "Route inspect was cancelled.";

    // @OpenForgeText route.inspect.title.route-chain
    internal static string TitleRouteChain()
        => "Route chain";

    // @OpenForgeText route.inspect.label.none-loader-root
    internal static string LabelNoneLoaderRoot()
        => "none (Loader root)";

    // @OpenForgeText route.inspect.title.direct-children
    internal static string TitleDirectChildren()
        => "Direct children";

    // @OpenForgeText route.inspect.title.entrypoint-name
    internal static string TitleEntrypointName()
        => "Entrypoint name";

    // @OpenForgeText route.inspect.title.overwrite-file
    internal static string TitleOverwriteFile()
        => "Overwrite file";

    // @OpenForgeText route.inspect.title.at-task-start-or-resume
    internal static string TitleAtTaskStartOrResume()
        => "At task start or resume";

    // @OpenForgeText route.inspect.title.read-automatically-after-another-route
    internal static string TitleReadAutomaticallyAfterAnotherRoute()
        => "Read automatically after another route";

    // @OpenForgeText route.inspect.title.read-when-this-route-is-selected
    internal static string TitleReadWhenThisRouteIsSelected()
        => "Read when this route is selected";

    // @OpenForgeText route.inspect.title.read-automatically-when
    internal static string TitleReadAutomaticallyWhen()
        => "Read automatically when";

    // @OpenForgeText route.inspect.title.read-automatically
    internal static string TitleReadAutomatically()
        => "Read automatically";

    // @OpenForgeText route.inspect.title.may-be-read-again
    internal static string TitleMayBeReadAgain()
        => "May be read again";

    // @OpenForgeText route.inspect.title.this-file
    internal static string TitleThisFile()
        => "This file";

    // @OpenForgeText route.inspect.title.selecting-this-route-adds
    internal static string TitleSelectingThisRouteAdds()
        => "Selecting this route adds";

    // @OpenForgeText route.inspect.label.nothing-already-in-startup-context
    internal static string LabelNothingAlreadyInStartupContext()
        => "nothing (already in startup context)";

    // @OpenForgeText route.inspect.title.read-automatically-below-it-through-load-now
    internal static string TitleReadAutomaticallyBelowItThroughLoadNow()
        => "Read automatically below it through #LoadNow";

    // @OpenForgeText route.inspect.title.selected-context
    internal static string TitleSelectedContext()
        => "Selected context";

    // @OpenForgeText route.inspect.title.already-in-startup-context
    internal static string TitleAlreadyInStartupContext()
        => "Already in startup context";

    // @OpenForgeText route.inspect.title.inherited-rules-from
    internal static string TitleInheritedRulesFrom()
        => "Inherited rules from";

    // @OpenForgeText route.inspect.title.local-rules
    internal static string TitleLocalRules()
        => "Local rules";

    // @OpenForgeText route.inspect.label.source-id
    internal static string LabelSourceId()
        => "source ID";

    // @OpenForgeText route.inspect.label.source-path
    internal static string LabelSourcePath()
        => "source path";

    // @OpenForgeText route.inspect.label.automatic-id
    internal static string LabelAutomaticId()
        => "automatic ID";

    // @OpenForgeText route.inspect.label.exact-path
    internal static string LabelExactPath()
        => "exact path";

    // @OpenForgeText route.inspect.label.interactive-selection
    internal static string LabelInteractiveSelection()
        => "interactive selection";

    // @OpenForgeText route.inspect.label.another-source
    internal static string LabelAnotherSource()
        => "another source";

    // @OpenForgeText route.inspect.title.route-facts
    internal static string TitleRouteFacts()
        => "Route facts";

    // @OpenForgeText route.inspect.title.id-is-not-unique
    internal static string TitleIdIsNotUnique()
        => "ID is not unique";

    // @OpenForgeText route.inspect.title.source-is-detached
    internal static string TitleSourceIsDetached()
        => "Source is detached";

    // @OpenForgeText route.inspect.title.source-is-not-routed
    internal static string TitleSourceIsNotRouted()
        => "Source is not routed";

    // @OpenForgeText route.inspect.title.overwrite-is-valid
    internal static string TitleOverwriteIsValid()
        => "Overwrite is valid";

    // @OpenForgeText route.inspect.title.too-many-sources
    internal static string TitleTooManySources()
        => "Too many sources";

    // @OpenForgeText route.inspect.title.loader-is-not-a-route
    internal static string TitleLoaderIsNotARoute()
        => "Loader is not a route";

    // @OpenForgeText route.inspect.title.source-file-is-missing
    internal static string TitleSourceFileIsMissing()
        => "Source file is missing";

    // @OpenForgeText route.inspect.title.overwrite-is-orphaned
    internal static string TitleOverwriteIsOrphaned()
        => "Overwrite is orphaned";

    // @OpenForgeText route.inspect.title.route-is-incomplete
    internal static string TitleRouteIsIncomplete()
        => "Route is incomplete";

    // @OpenForgeText route.inspect.title.route-fact-is-unavailable
    internal static string TitleRouteFactIsUnavailable()
        => "Route fact is unavailable";

    // @OpenForgeText route.inspect.title.route-inspect-failed
    internal static string TitleRouteInspectFailed()
        => "Route inspect failed";

    // @OpenForgeText route.inspect.title.route-inspect-was-cancelled
    internal static string TitleRouteInspectWasCancelled()
        => "Route inspect was cancelled";

    // @OpenForgeText route.inspect.label.own-bytes
    internal static string LabelOwnBytes()
        => "own bytes";

    // @OpenForgeText route.inspect.label.own-tokens
    internal static string LabelOwnTokens()
        => "own tokens";

    // @OpenForgeText route.inspect.label.added-files
    internal static string LabelAddedFiles()
        => "added files";

    // @OpenForgeText route.inspect.label.added-bytes
    internal static string LabelAddedBytes()
        => "added bytes";

    // @OpenForgeText route.inspect.label.added-tokens
    internal static string LabelAddedTokens()
        => "added tokens";

    // @OpenForgeText route.inspect.title.load-now-files
    internal static string TitleLoadNowFiles()
        => "LoadNow files";

    // @OpenForgeText route.inspect.title.load-now-bytes
    internal static string TitleLoadNowBytes()
        => "LoadNow bytes";

    // @OpenForgeText route.inspect.title.load-now-tokens
    internal static string TitleLoadNowTokens()
        => "LoadNow tokens";

    // @OpenForgeText route.inspect.label.direct-children
    internal static string LabelDirectChildren()
        => "direct children";

    // @OpenForgeText route.inspect.label.descendants
    internal static string LabelDescendants()
        => "descendants";

    // @OpenForgeText route.inspect.label.the-loader-is-read
    internal static string LabelTheLoaderIsRead()
        => "the Loader is read";

    // @OpenForgeText route.inspect.label.this-route-is-selected
    internal static string LabelThisRouteIsSelected()
        => "this route is selected";

    // @OpenForgeText route.inspect.label.the-base-source-is-read
    internal static string LabelTheBaseSourceIsRead()
        => "the base source is read";

    // @OpenForgeText route.inspect.title.read-when-this-route-is-selected-yes
    internal static string TitleReadWhenThisRouteIsSelectedYes()
        => "Read when this route is selected: yes";

    // @OpenForgeText route.inspect.label.after-context-restoration
    internal static string LabelAfterContextRestoration()
        => "after context restoration";

    // @OpenForgeText route.inspect.label.before-handoff-or-closeout
    internal static string LabelBeforeHandoffOrCloseout()
        => "before handoff or closeout";

    // @OpenForgeText route.inspect.label.after-a-change-that-may-affect-its-follow-up-work
    internal static string LabelAfterAChangeThatMayAffectItsFollowUpWork()
        => "after a change that may affect its follow-up work";

    // @OpenForgeText route.inspect.label.at-the-defined-later-review-occasions
    internal static string LabelAtTheDefinedLaterReviewOccasions()
        => "at the defined later review occasions";

    // @OpenForgeText route.inspect.label.substantive-local-axioms
    internal static string LabelSubstantiveLocalAxioms()
        => "substantive local Axioms";

    // @OpenForgeText route.inspect.label.inherits-ancestor-axioms-without-local-rules
    internal static string LabelInheritsAncestorAxiomsWithoutLocalRules()
        => "inherits ancestor Axioms without local rules";

    // @OpenForgeText route.inspect.label.empty-local-axioms
    internal static string LabelEmptyLocalAxioms()
        => "empty local Axioms";

    // @OpenForgeText route.inspect.label.no-local-axioms-section
    internal static string LabelNoLocalAxiomsSection()
        => "no local Axioms section";

    // @OpenForgeText route.inspect.label.all-applicable-route-facts-are-available
    internal static string LabelAllApplicableRouteFactsAreAvailable()
        => "all applicable route facts are available";

    // @OpenForgeText route.inspect.label.the-source-is-safe-and-complete-but-its-automatic-id-is-not-unique
    internal static string LabelTheSourceIsSafeAndCompleteButItsAutomaticIdIsNotUnique()
        => "the source is safe and complete but its automatic ID is not unique";

    // @OpenForgeText route.inspect.label.a-required-route-fact-or-measurement-is-unavailable
    internal static string LabelARequiredRouteFactOrMeasurementIsUnavailable()
        => "a required route fact or measurement is unavailable";

    // @OpenForgeText route.inspect.label.the-source-reference-was-not-accepted
    internal static string LabelTheSourceReferenceWasNotAccepted()
        => "the source reference was not accepted";

    // @OpenForgeText route.inspect.label.a-safe-source-or-route-boundary-could-not-be-established
    internal static string LabelASafeSourceOrRouteBoundaryCouldNotBeEstablished()
        => "a safe source or route boundary could not be established";

    // @OpenForgeText route.inspect.label.an-unexpected-failure-stopped-route-inspection
    internal static string LabelAnUnexpectedFailureStoppedRouteInspection()
        => "an unexpected failure stopped route inspection";

    // @OpenForgeText route.inspect.label.route-inspection-was-interrupted-before-completion
    internal static string LabelRouteInspectionWasInterruptedBeforeCompletion()
        => "route inspection was interrupted before completion";

    // @OpenForgeText route.inspect.label.the-defined-automatic-reading-events
    internal static string LabelTheDefinedAutomaticReadingEvents()
        => "the defined automatic-reading events";

    // @OpenForgeText route.inspect.label.its-exposing-parent-is-read
    internal static string LabelItsExposingParentIsRead()
        => "its exposing parent is read";

    // @OpenForgeText route.inspect.label.its-route-is-selected
    internal static string LabelItsRouteIsSelected()
        => "its route is selected";

    // @OpenForgeText route.inspect.label.it-is-visible-from-task-start-routing
    internal static string LabelItIsVisibleFromTaskStartRouting()
        => "it is visible from task-start routing";

    // @OpenForgeText route.inspect.label.its-scope-is-selected
    internal static string LabelItsScopeIsSelected()
        => "its scope is selected";

    // @OpenForgeText route.inspect.label.it-is-needed-as-an-ancestor
    internal static string LabelItIsNeededAsAnAncestor()
        => "it is needed as an ancestor";

    // @OpenForgeText route.inspect.label.a-task-review-point-is-reached-while-its-scope-is-active
    internal static string LabelATaskReviewPointIsReachedWhileItsScopeIsActive()
        => "a task review point is reached while its scope is active";

    // @OpenForgeText route.inspect.label.a-later-review-point-is-reached-while-its-scope-is-active
    internal static string LabelALaterReviewPointIsReachedWhileItsScopeIsActive()
        => "a later review point is reached while its scope is active";

    // @OpenForgeText route.inspect.title.tags
    internal static string TitleTags()
        => "Tags";

    // @OpenForgeText route.inspect.title.descendants
    internal static string TitleDescendants()
        => "Descendants";

    // @OpenForgeText route.inspect.label.no
    internal static string LabelNo()
        => "no";

    // @OpenForgeText route.inspect.label.yes
    internal static string LabelYes()
        => "yes";

    // @OpenForgeText route.inspect.title.when
    internal static string TitleWhen()
        => "When";

    // @OpenForgeText route.inspect.title.axioms
    internal static string TitleAxioms()
        => "Axioms";

    // @OpenForgeText route.inspect.title.why
    internal static string TitleWhy()
        => "Why";

    // @OpenForgeText route.inspect.label.unresolved
    internal static string LabelUnresolved()
        => "unresolved";

    // @OpenForgeText route.inspect.help.syntax
    internal static string HelpSyntax()
        => "open-forge route inspect <source-reference> [global options]";

    // @OpenForgeText route.inspect.help.source-references
    internal static string HelpSourceReferences()
        => "Use one source-id or one exact .agents/... or ./.agents/... path. Supply a source reference to run this command.";

    // @OpenForgeText route.inspect.help.inherited-global-options
    internal static string HelpInheritedGlobalOptions()
        => "--workspace <path>, --format <text|json>, --detail <minimal|standard|full|debug>, --detail-filter <error|warning|info|all>, --help, and --version.\n  You can also use --detail=<minimal|standard|full|debug> or --detail:<minimal|standard|full|debug>.\n  --detail selects detail in text and JSON.";

    // @OpenForgeText route.inspect.help.examples
    internal static string HelpExamples()
        => "open-forge route inspect memory/working\n  open-forge route inspect .agents/memory/_memory.md --detail minimal\n  open-forge route inspect memory/working --format json --detail standard\n  open-forge route inspect --help\n  open-forge route inspect --version";

    // @OpenForgeText route.inspect.help.related-commands
    internal static string HelpRelatedCommands()
        => "route list — list routed sources and descendants.\n  context — use open-forge context [source-reference...] to read selected source content.\n  doctor — diagnose workspace conditions without changing them.";

    // @OpenForgeText route.inspect.help.notes
    internal static string HelpNotes()
        => "Inspect reports one route profile without authored source bodies, ordinary links, diagnosis, recommendations, or mutation. JSON always writes one complete result document to stdout; verbose diagnostics are bounded and use stderr.";
}
