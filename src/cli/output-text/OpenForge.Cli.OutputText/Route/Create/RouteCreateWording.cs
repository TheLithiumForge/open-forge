using System.Globalization;

namespace OpenForge.Cli.OutputText.Route.Create;

internal static class RouteCreateWording
{
    // @OpenForgeText route.create.result.created-source
    internal static string Created(string path, string id) => $"Created {path}  ({id})";

    // @OpenForgeText route.create.result.would-create-source
    internal static string WouldCreate(string path, string id) => $"Would create {path}  ({id})";

    // @OpenForgeText route.create.wording.already-has-the-requested-content-nothing-to-do
    internal static string AlreadyMatching(string path)
        => $"{path} already has the requested content. Nothing to do.";

    // @OpenForgeText route.create.wording.route-create-stopped-after-of-changes
    internal static string Failed(int completed, int total)
        => string.Create(
            CultureInfo.InvariantCulture,
            $"Route create stopped after {completed} of {total} changes.");

    internal static string Listed(string path) => global::OpenForge.Cli.OutputText.Route.Shared.CanonicalPhrases.Listed(path);

    internal static string WouldList(string path) => global::OpenForge.Cli.OutputText.Route.Shared.CanonicalPhrases.WouldList(path);

    internal static string BodyCopied(string reference) => global::OpenForge.Cli.OutputText.Route.Shared.CanonicalPhrases.BodyCopied(reference);

    internal static string CreatedEffect(string path) => global::OpenForge.Cli.OutputText.Route.Shared.CanonicalPhrases.Created(path);

    internal static string WouldCreateEffect(string path) => global::OpenForge.Cli.OutputText.Shared.CanonicalPhrases.FormatWouldCreate(path);

    internal static string NotStarted(string path) => global::OpenForge.Cli.OutputText.Route.Shared.CanonicalPhrases.FormatNotStarted(path);

    internal static string Unknown(string path) => global::OpenForge.Cli.OutputText.Route.Shared.CanonicalPhrases.FormatFinalStateUnknown(path);

    internal static string FailedEffect(string path) => global::OpenForge.Cli.OutputText.Route.Shared.CanonicalPhrases.FormatFailed(path);

    internal static string MetadataDescription(string value) => global::OpenForge.Cli.OutputText.Shared.CanonicalPhrases.MetadataDescription(value);

    internal static string MetadataResponsibility(string value) => global::OpenForge.Cli.OutputText.Route.Shared.CanonicalPhrases.MetadataResponsibility(value);

    internal static string TemplatePath(string path) => global::OpenForge.Cli.OutputText.Route.Shared.CanonicalPhrases.TemplatePath(path);

    internal static string FileHeader(string path) => global::OpenForge.Cli.OutputText.Route.Shared.CanonicalPhrases.FileHeader(path);

    internal static string EntriesSection(string path) => global::OpenForge.Cli.OutputText.Route.Shared.CanonicalPhrases.EntriesSection(path);

    internal static string Verification(string value) => global::OpenForge.Cli.OutputText.Route.Shared.CanonicalPhrases.Verification(value);

    internal static string InvalidTemplate(string reference)
        => global::OpenForge.Cli.OutputText.Route.Shared.CanonicalPhrases.InvalidTemplate(reference);

    // @OpenForgeText route.create.wording.has-no-entrypoint-so-the-file-cannot-be-listed-create-the-route-first
    internal static string ParentMissing(string folder)
        => $"{folder} has no entrypoint, so the file cannot be listed. Create the route first.";

    internal static string TargetContentDiffers(string path)
        => global::OpenForge.Cli.OutputText.Shared.CanonicalPhrases.TargetContentDiffers(path);

    internal static string TemplateUnsafe(string reference)
        => global::OpenForge.Cli.OutputText.Route.Shared.CanonicalPhrases.FormatTheTemplateCouldNotBeVerifiedSafely(reference);

    internal static string TemplateUnavailable(string reference)
        => global::OpenForge.Cli.OutputText.Route.Shared.CanonicalPhrases.FormatTheTemplateCouldNotBeRead(reference);

    internal static string IdentityCollision(string id)
        => global::OpenForge.Cli.OutputText.Route.Shared.CanonicalPhrases.IdentityCollision(id);

    // @OpenForgeText route.create.title.optional-metadata-is-missing
    internal static string OptionalMetadataTitle()
        => "Optional metadata is missing";

    // @OpenForgeText route.create.message.would-create-without-optional-metadata
    internal static string WouldCreateWithoutOptionalMetadata(string path)
        => $"Would create {path} without optional description or tags.";

    // @OpenForgeText route.create.message.created-without-optional-metadata
    internal static string CreatedWithoutOptionalMetadata(string path)
        => $"Created {path} without optional description or tags.";

    // @OpenForgeText route.create.message.current-without-optional-metadata
    internal static string CurrentWithoutOptionalMetadata(string path)
        => $"{path} has no optional description or tags.";

    // @OpenForgeText route.create.message.optional-metadata-is-missing
    internal static string OptionalMetadataMissing(string path)
        => $"Optional metadata is missing for {path}.";

    // @OpenForgeText route.create.next.add-optional-description-or-tag
    internal static string AddOptionalDescriptionOrTag()
        => "Add an optional description or tag with open-forge route update when useful.";

    // @OpenForgeText route.create.next.optional-metadata-inline
    internal static string OptionalMetadataNext(string command)
        => $"Next: {command} (add an optional description or tag when useful).";
}
