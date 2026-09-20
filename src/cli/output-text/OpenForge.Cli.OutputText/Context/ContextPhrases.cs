using System.Globalization;

namespace OpenForge.Cli.OutputText.Context;

internal static class ContextPhrases
{
    // @OpenForgeText context.phrase.cannot-read-context
    internal static string FormatCannotReadContext(string trimEndText)
        => $"Cannot read context: {trimEndText}.";

    // @OpenForgeText context.phrase.context-stopped-because-of-an-unexpected-error
    internal static string FormatContextStoppedBecauseOfAnUnexpectedError(string trimEndText)
        => $"Context stopped because of an unexpected error: {trimEndText}.";

    // @OpenForgeText context.phrase.for
    internal static string FormatFor(string referenceText)
        => $" for {referenceText}";

    // @OpenForgeText context.phrase.at-depth
    internal static string FormatAtDepth(string valueText, string depthText)
        => $"{valueText} at depth {depthText}";

    // @OpenForgeText context.phrase.content-is-not-a-known-part-use-metadata-paths-frontmatter-headings-body-or-section-name
    internal static string FormatContentIsNotAKnownPartUseMetadataPathsFrontmatterHeadingsBodyOrSectionName(string valueText)
        => $"--content {valueText} is not a known part. Use metadata, paths, frontmatter, headings, body, or section:<name>.";

    // @OpenForgeText context.phrase.the-link-at-could-point-to-more-than-one-file-it-was-not-followed
    internal static string FormatTheLinkAtCouldPointToMoreThanOneFileItWasNotFollowed(string locationText)
        => $"The link at {locationText} could point to more than one file. It was not followed.";

    // @OpenForgeText context.phrase.the-link-at-points-outside-the-workspace-it-was-not-followed
    internal static string FormatTheLinkAtPointsOutsideTheWorkspaceItWasNotFollowed(string locationText)
        => $"The link at {locationText} points outside the workspace. It was not followed.";

    // @OpenForgeText context.phrase.the-startup-files-could-not-be-resolved
    internal static string FormatTheStartupFilesCouldNotBeResolved(string reasonText)
        => $"The startup files could not be resolved: {reasonText}.";

    // @OpenForgeText context.phrase.could-not-be-read-so-it-was-not-included
    internal static string FormatCouldNotBeReadSoItWasNotIncluded(string pathText)
        => $"{pathText} could not be read, so it was not included.";

    // @OpenForgeText context.phrase.is-not-valid-utf-8-so-it-was-not-included
    internal static string FormatIsNotValidUtf8SoItWasNotIncluded(string pathText)
        => $"{pathText} is not valid UTF-8, so it was not included.";

    // @OpenForgeText context.phrase.could-not-be-parsed-as-markdown-so-its-was-not-produced
    internal static string FormatCouldNotBeParsedAsMarkdownSoItsWasNotProduced(string pathText, string valueText)
        => $"{pathText} could not be parsed as Markdown, so its {valueText} was not produced.";

    // @OpenForgeText context.phrase.the-link-at-points-to-which-does-not-exist-it-was-not-followed
    internal static string FormatTheLinkAtPointsToWhichDoesNotExistItWasNotFollowed(string locationText, string valueText)
        => $"The link at {locationText} points to {valueText}, which does not exist. It was not followed.";

    // @OpenForgeText context.phrase.the-link-at-points-to-which-has-no-heading-it-was-not-followed
    internal static string FormatTheLinkAtPointsToWhichHasNoHeadingItWasNotFollowed(string locationText, string pathText, string valueText)
        => $"The link at {locationText} points to {pathText}, which has no heading {valueText}. It was not followed.";

    // @OpenForgeText context.phrase.the-link-at-has-an-encoding-that-cannot-be-resolved-it-was-not-followed
    internal static string FormatTheLinkAtHasAnEncodingThatCannotBeResolvedItWasNotFollowed(string locationText)
        => $"The link at {locationText} has an encoding that cannot be resolved. It was not followed.";

    // @OpenForgeText context.phrase.the-link-at-points-to-which-could-not-be-read-it-was-not-followed
    internal static string FormatTheLinkAtPointsToWhichCouldNotBeReadItWasNotFollowed(string locationText, string pathText)
        => $"The link at {locationText} points to {pathText}, which could not be read. It was not followed.";

    // @OpenForgeText context.phrase.has-more-than-one-section-named-none-was-included
    internal static string FormatHasMoreThanOneSectionNamedNoneWasIncluded(string pathText, string valueText)
        => $"{pathText} has more than one section named {valueText}. None was included.";

    // @OpenForgeText context.phrase.the-link-at-is-written-but-the-file-is-named
    internal static string FormatTheLinkAtIsWrittenButTheFileIsNamed(string locationText, string valueText, string pathText)
        => $"The link at {locationText} is written {valueText}, but the file is named {pathText}.";

    // @OpenForgeText context.phrase.has-no-frontmatter
    internal static string FormatHasNoFrontmatter(string pathText)
        => $"{pathText} has no frontmatter.";
}
