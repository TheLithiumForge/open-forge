using System.Globalization;

namespace OpenForge.Cli.OutputText.Context;

internal static class ContextText
{
    // @OpenForgeText context.help.syntax
    internal static string HelpSyntax()
        => "open-forge context [source-reference...] [--additions-only] [--content <part>[,<part>...]] [--follow-links <positive-depth|all>] [global options]";

    // @OpenForgeText context.help.selection
    internal static string HelpSelection()
        => "With no source reference, Context returns all required startup context. Each source ID or exact .agents path adds the source and its required route context. --additions-only requires an explicit source and removes the startup set from the result.";

    // @OpenForgeText context.help.content
    internal static string HelpContent()
        => "Parts are metadata, paths, frontmatter, headings, body, and section:<name>. Default: frontmatter,body. Use one --content value; comma and backslash may be escaped inside section names.";

    // @OpenForgeText context.help.links
    internal static string HelpLinks()
        => "--follow-links accepts a positive base-10 depth or all. It follows contained local Markdown links breadth-first, never fetches external URLs, and records broken edges.";

    // @OpenForgeText context.help.examples
    internal static string HelpExamples()
        => "open-forge context\nopen-forge context memory --content body\nopen-forge context .agents/memory/_memory.md --format json";

    // @OpenForgeText context.help.notes
    internal static string HelpNotes()
        => "Context preserves authored source bytes and adds generated delimiters around each physical layer. It never changes source files or follows external links.";

    // @OpenForgeText context.message.context-is-complete
    internal static string MessageContextIsComplete()
        => "Context is complete.";

    // @OpenForgeText context.message.context-completed-with-warnings
    internal static string MessageContextCompletedWithWarnings()
        => "Context completed with warnings.";

    // @OpenForgeText context.message.context-could-not-be-read-completely
    internal static string MessageContextCouldNotBeReadCompletely()
        => "Context could not be read completely.";

    // @OpenForgeText context.message.context-was-cancelled
    internal static string MessageContextWasCancelled()
        => "Context was cancelled.";

    // @OpenForgeText context.message.no-additional-context-the-selected-sources-are-already-read-at-startup
    internal static string MessageNoAdditionalContextTheSelectedSourcesAreAlreadyReadAtStartup()
        => "No additional context. The selected sources are already read at startup.";

    // @OpenForgeText context.message.list-source-ids-and-exact-paths-then-rerun-context
    internal static string MessageListSourceIdsAndExactPathsThenRerunContext()
        => "List source IDs and exact paths, then rerun Context.";

    // @OpenForgeText context.message.inspect-the-unavailable-closure-source-link-or-projection-facts-before-relying-on-this-context-result
    internal static string MessageInspectTheUnavailableClosureSourceLinkOrProjectionFactsBeforeRelyingOnThisContextResult()
        => "Inspect the unavailable closure, source, link, or projection facts before relying on this Context result.";

    // @OpenForgeText context.message.run-repair-with-automatic-to-correct-the-link-target-casing-then-rerun-context
    internal static string MessageRunRepairWithAutomaticToCorrectTheLinkTargetCasingThenRerunContext()
        => "Run Repair with --automatic to correct the link target casing, then rerun Context.";

    // @OpenForgeText context.heading.links
    internal static string HeadingLinks()
        => "Links:";

    // @OpenForgeText context.label.the-requested-sources-could-not-be-resolved
    internal static string LabelTheRequestedSourcesCouldNotBeResolved()
        => "the requested sources could not be resolved";

    // @OpenForgeText context.label.tokens
    internal static string LabelTokens()
        => "tokens";

    // @OpenForgeText context.label.bytes
    internal static string LabelBytes()
        => "bytes";

    // @OpenForgeText context.label.links-followed
    internal static string LabelLinksFollowed()
        => "links followed";

    // @OpenForgeText context.label.links-not-followed
    internal static string LabelLinksNotFollowed()
        => "links not followed";

    // @OpenForgeText context.label.workspace-entry
    internal static string LabelWorkspaceEntry()
        => "workspace entry";

    // @OpenForgeText context.title.loader
    internal static string TitleLoader()
        => "Loader";

    // @OpenForgeText context.label.load-now
    internal static string LabelLoadNow()
        => "#LoadNow";

    // @OpenForgeText context.label.keep-in-mind
    internal static string LabelKeepInMind()
        => "#KeepInMind";

    // @OpenForgeText context.label.ancestor-required
    internal static string LabelAncestorRequired()
        => "ancestor required";

    // @OpenForgeText context.label.selected-source
    internal static string LabelSelectedSource()
        => "selected source";

    // @OpenForgeText context.label.scope-local-loading
    internal static string LabelScopeLocalLoading()
        => "scope-local loading";

    // @OpenForgeText context.label.linked-source
    internal static string LabelLinkedSource()
        => "linked source";

    // @OpenForgeText context.label.overwrite-companion
    internal static string LabelOverwriteCompanion()
        => "overwrite companion";

    // @OpenForgeText context.title.invalid-content
    internal static string TitleInvalidContent()
        => "Invalid content";

    // @OpenForgeText context.title.invalid-link-depth
    internal static string TitleInvalidLinkDepth()
        => "Invalid link depth";

    // @OpenForgeText context.title.link-target-is-ambiguous
    internal static string TitleLinkTargetIsAmbiguous()
        => "Link target is ambiguous";

    // @OpenForgeText context.title.link-target-is-unsafe
    internal static string TitleLinkTargetIsUnsafe()
        => "Link target is unsafe";

    // @OpenForgeText context.title.source-markdown-is-unavailable
    internal static string TitleSourceMarkdownIsUnavailable()
        => "Source Markdown is unavailable";

    // @OpenForgeText context.title.link-heading-is-missing
    internal static string TitleLinkHeadingIsMissing()
        => "Link heading is missing";

    // @OpenForgeText context.title.link-target-casing-differs
    internal static string TitleLinkTargetCasingDiffers()
        => "Link target casing differs";

    // @OpenForgeText context.title.frontmatter-is-missing
    internal static string TitleFrontmatterIsMissing()
        => "Frontmatter is missing";

    // @OpenForgeText context.title.context-failed
    internal static string TitleContextFailed()
        => "Context failed";

    // @OpenForgeText context.title.context-was-cancelled
    internal static string TitleContextWasCancelled()
        => "Context was cancelled";

    // @OpenForgeText context.label.the-requested-source
    internal static string LabelTheRequestedSource()
        => "the requested source";

    // @OpenForgeText context.message.follow-links-must-be-a-positive-number-or-all
    internal static string MessageFollowLinksMustBeAPositiveNumberOrAll()
        => "--follow-links must be a positive number or all.";

    // @OpenForgeText context.title.context
    internal static string TitleContext()
        => "Context";

    // @OpenForgeText context.label.followed
    internal static string LabelFollowed()
        => "followed";

    // @OpenForgeText context.label.id
    internal static string LabelId()
        => "id";

    // @OpenForgeText context.label.description
    internal static string LabelDescription()
        => "description";

    // @OpenForgeText context.label.tags
    internal static string LabelTags()
        => "tags";

    // @OpenForgeText context.help.heading.content
    internal static string HelpHeadingContent()
        => "Content";
}
