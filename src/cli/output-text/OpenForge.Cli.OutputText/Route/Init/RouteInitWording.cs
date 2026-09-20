using System.Globalization;

namespace OpenForge.Cli.OutputText.Route.Init;

internal static class RouteInitWording
{
    internal static string Created(string path)
        => global::OpenForge.Cli.OutputText.Route.Shared.CanonicalPhrases.Created(path);

    // @OpenForgeText route.init.wording.created-entrypoints-for
    internal static string CreatedMany(int count, string id)
        => string.Create(CultureInfo.InvariantCulture, $"Created {count} entrypoints for {id}.");

    internal static string WouldCreate(string path)
        => global::OpenForge.Cli.OutputText.Shared.CanonicalPhrases.FormatWouldCreate(path);

    // @OpenForgeText route.init.wording.would-create-entrypoints-for
    internal static string WouldCreateMany(int count, string id)
        => string.Create(CultureInfo.InvariantCulture, $"Would create {count} entrypoints for {id}.");

    // @OpenForgeText route.init.wording.is-already-initialized-nothing-to-do
    internal static string AlreadyInitialized(string id)
        => $"{id} is already initialized. Nothing to do.";

    // @OpenForgeText route.init.wording.route-init-stopped-after-of-changes
    internal static string Failed(int completed, int total)
        => string.Create(CultureInfo.InvariantCulture,
            $"Route init stopped after {completed} of {total} changes.");

    internal static string Listed(string path)
        => global::OpenForge.Cli.OutputText.Route.Shared.CanonicalPhrases.Listed(path);

    internal static string WouldList(string path)
        => global::OpenForge.Cli.OutputText.Route.Shared.CanonicalPhrases.WouldList(path);

    internal static string MetadataDescription(string value)
        => global::OpenForge.Cli.OutputText.Shared.CanonicalPhrases.MetadataDescription(value);

    internal static string MetadataResponsibility(string value)
        => global::OpenForge.Cli.OutputText.Route.Shared.CanonicalPhrases.MetadataResponsibility(value);

    // @OpenForgeText route.init.wording.scaffold
    internal static string Scaffold(string value)
        => $"Scaffold: {value}";

    internal static string FileHeader(string path)
        => global::OpenForge.Cli.OutputText.Route.Shared.CanonicalPhrases.FileHeader(path);

    internal static string EntriesSection(string path)
        => global::OpenForge.Cli.OutputText.Route.Shared.CanonicalPhrases.EntriesSection(path);

    internal static string Verification(string value)
        => global::OpenForge.Cli.OutputText.Route.Shared.CanonicalPhrases.Verification(value);

    internal static string FrameworkFingerprint(string value)
        => global::OpenForge.Cli.OutputText.Shared.CanonicalPhrases.FrameworkFingerprint(value);

    internal static string IdentityCollision(string id)
        => global::OpenForge.Cli.OutputText.Route.Shared.CanonicalPhrases.IdentityCollision(id);
}
