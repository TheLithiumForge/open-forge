using System.Globalization;

namespace OpenForge.Cli.OutputText.Status;

internal static class StatusWording
{
    // @OpenForgeText status.wording.included-files
    internal static string IncludedFiles(int listed, int unlisted, int missing, int handWritten)
        => string.Create(CultureInfo.InvariantCulture, $"Included files: {listed} listed, {unlisted} unlisted, {missing} missing, {handWritten} hand-written loader {(handWritten == 1 ? "row" : "rows")}.");

    // @OpenForgeText status.wording.included-startup
    internal static string IncludedStartup(string measurement)
        => $"Startup outside .agents/: {measurement}";

    // @OpenForgeText status.wording.included-pending
    internal static string IncludedPending(string path)
        => $"Pending included file: {path}. Supply working files to context to resolve its conditions.";

    // @OpenForgeText status.wording.open-forge-is-not-installed-in
    internal static string NotInstalled(string workspace)
        => $"Open Forge is not installed in {workspace}.";

    // @OpenForgeText status.wording.extension-files
    internal static string ExtensionFilesHeading(string id)
        => $"Extension {id} files";

    // @OpenForgeText status.wording.library-links
    internal static string LibraryLinksHeading(string id)
        => $"Library {id} links";

    // @OpenForgeText status.wording.extensions
    internal static string ExtensionList(string names)
        => $"  Extensions: {names}";

    // @OpenForgeText status.wording.libraries
    internal static string LibraryList(string names)
        => $"  Libraries: {names}";

    // @OpenForgeText status.wording.generated-navigation-metadata-invalid
    internal static string GeneratedNavigationMetadataInvalid(string path, string cause)
        => $"The generated navigation metadata in {path} is malformed: {cause}.";

    // @OpenForgeText status.title.generated-navigation-metadata-invalid
    internal static string GeneratedNavigationMetadataInvalidTitle()
        => "Generated navigation metadata is malformed";
}
