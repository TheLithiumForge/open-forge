using System.Globalization;

namespace OpenForge.Cli.OutputText.Library.Shared;

internal static class LibraryGitIgnoreText
{
    // @OpenForgeText library.git-ignore.blocked
    internal static string Blocked() => "Library Git-ignore rules are unsafe";
    // @OpenForgeText library.git-ignore.unavailable
    internal static string Unavailable() => "Library Git-ignore rules are unavailable";
    // @OpenForgeText library.git-ignore.question
    internal static string Question() => "Add Git-ignore rules for this Library's links?";
    // @OpenForgeText library.git-ignore.no
    internal static string No() => "No";
    // @OpenForgeText library.git-ignore.yes
    internal static string Yes() => "Yes";
    // @OpenForgeText library.git-ignore.no-description
    internal static string NoDescription() => "Attach the links without changing .gitignore.";
    // @OpenForgeText library.git-ignore.yes-description
    internal static string YesDescription() => "Add ignore rules for the Library's individual links. These rules do not cover source files or shared folders. Already tracked files stay tracked.";
    // @OpenForgeText library.git-ignore.would-update
    internal static string WouldUpdate() => "Would update Library rules in .gitignore";
    // @OpenForgeText library.git-ignore.updated
    internal static string Updated() => "Updated Library rules in .gitignore";
    // @OpenForgeText library.git-ignore.unknown
    internal static string Unknown() => "The Library rules in .gitignore could not be verified";
    // @OpenForgeText library.git-ignore.failed
    internal static string Failed() => "The Library rules in .gitignore could not be updated";
    // @OpenForgeText library.git-ignore.not-started
    internal static string NotStarted() => "The Library rules in .gitignore were not changed";
    // @OpenForgeText library.git-ignore.files-updated
    internal static string FilesUpdated() => "Git-ignore files updated";
    // @OpenForgeText library.git-ignore.stopped-after-changes
    internal static string StoppedAfter(string operation, int verified, int total)
        => string.Create(CultureInfo.InvariantCulture, $"Library {operation} stopped after {verified} of {total} changes were verified.");
    // @OpenForgeText library.git-ignore.cancelled-after-changes
    internal static string CancelledAfter(string operation, int verified, int total)
        => string.Create(CultureInfo.InvariantCulture, $"Library {operation} was cancelled after {verified} of {total} changes were verified.");
}
