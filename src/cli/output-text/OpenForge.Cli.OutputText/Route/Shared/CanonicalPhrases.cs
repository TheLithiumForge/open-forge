namespace OpenForge.Cli.OutputText.Route.Shared;

internal static class CanonicalPhrases
{
    // @OpenForgeText route.shared.wording.listed-in
    internal static string Listed(string path)
        => $"Listed in {path}";

    // @OpenForgeText route.shared.wording.would-list-it-in
    internal static string WouldList(string path)
        => $"Would list it in {path}";

    // @OpenForgeText route.shared.wording.body-copied-from-the-template
    internal static string BodyCopied(string reference)
        => $"Body copied from the Template {reference}";

    // @OpenForgeText route.shared.wording.created
    internal static string Created(string path)
        => $"Created {path}";

    // @OpenForgeText route.shared.phrase.not-started
    internal static string FormatNotStarted(string pathText)
        => $"{pathText}  not started";

    // @OpenForgeText route.shared.phrase.final-state-unknown
    internal static string FormatFinalStateUnknown(string pathText)
        => $"{pathText}  final state unknown";

    // @OpenForgeText route.shared.phrase.failed
    internal static string FormatFailed(string pathText)
        => $"{pathText}  failed";

    // @OpenForgeText route.shared.wording.responsibility
    internal static string MetadataResponsibility(string value)
        => $"responsibility: {value}";

    // @OpenForgeText route.shared.wording.template
    internal static string TemplatePath(string path)
        => $"Template: {path}";

    // @OpenForgeText route.shared.wording.new-file
    internal static string FileHeader(string path)
        => $"--- {path} (new file) ---";

    // @OpenForgeText route.shared.wording.entries-section
    internal static string EntriesSection(string path)
        => $"Entries section: {path}";

    // @OpenForgeText route.shared.wording.verification
    internal static string Verification(string value)
        => $"Verification: {value}";

    // @OpenForgeText route.shared.wording.template-is-not-a-routed-template
    internal static string InvalidTemplate(string reference)
        => $"--template {reference} is not a routed Template.";

    // @OpenForgeText route.shared.phrase.the-template-could-not-be-verified-safely
    internal static string FormatTheTemplateCouldNotBeVerifiedSafely(string templateText)
        => $"The Template {templateText} could not be verified safely.";

    // @OpenForgeText route.shared.phrase.the-template-could-not-be-read
    internal static string FormatTheTemplateCouldNotBeRead(string templateText)
        => $"The Template {templateText} could not be read.";

    // @OpenForgeText route.shared.wording.the-new-id-would-collide-with-another-source
    internal static string IdentityCollision(string id)
        => $"The new ID {id} would collide with another source.";

    // @OpenForgeText route.shared.wording.entry-updated-in
    internal static string EntryUpdated(string path)
        => $"Entry updated in {path}";

    // @OpenForgeText route.shared.wording.entry-removed-from
    internal static string EntryRemoved(string path)
        => $"Entry removed from {path}";
}
