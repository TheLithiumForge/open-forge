using System.Globalization;

namespace OpenForge.Cli.OutputText.Install;

internal static class InstallPhrases
{
    // @OpenForgeText install.phrase.replace-the-existing-listed-above-y-n
    internal static string FormatReplaceTheExistingListedAboveYN(string replacementCountText, string nounText)
        => $"Replace the {replacementCountText} existing {nounText} listed above? [y/N]";

    // @OpenForgeText install.phrase.installed-the-open-forge-framework-into-replacing-existing
    internal static string FormatInstalledTheOpenForgeFrameworkIntoReplacingExisting(string workspaceText, string replacementsText, string cliTextPluralText)
        => $"Installed the Open Forge Framework into {workspaceText}, replacing {replacementsText} existing {cliTextPluralText}.";

    // @OpenForgeText install.phrase.installed-the-open-forge-framework-into
    internal static string FormatInstalledTheOpenForgeFrameworkInto(string workspaceText)
        => $"Installed the Open Forge Framework into {workspaceText}.";

    // @OpenForgeText install.phrase.would-install-the-open-forge-framework-into-replacing-existing
    internal static string FormatWouldInstallTheOpenForgeFrameworkIntoReplacingExisting(string workspaceText, string replacementsText, string cliTextPluralText)
        => $"Would install the Open Forge Framework into {workspaceText}, replacing {replacementsText} existing {cliTextPluralText}.";

    // @OpenForgeText install.phrase.would-install-the-open-forge-framework-into
    internal static string FormatWouldInstallTheOpenForgeFrameworkInto(string workspaceText)
        => $"Would install the Open Forge Framework into {workspaceText}.";

    // @OpenForgeText install.phrase.install-could-not-start-nothing-was-changed
    internal static string FormatInstallCouldNotStartNothingWasChanged(string trimSentenceText)
        => $"Install could not start: {trimSentenceText}. Nothing was changed.";

    // @OpenForgeText install.phrase.cannot-install-already-where-the-framework-would-write
    internal static string FormatCannotInstallAlreadyWhereTheFrameworkWouldWrite(string countText, string cliTextPluralText, string verbText)
        => $"Cannot install: {countText} {cliTextPluralText} already {verbText} where the Framework would write.";

    // @OpenForgeText install.phrase.cannot-install-framework-changed-since-they-were-installed
    internal static string FormatCannotInstallFrameworkChangedSinceTheyWereInstalled(string countText, string cliTextPluralText, string verbText)
        => $"Cannot install: {countText} Framework {cliTextPluralText} {verbText} changed since they were installed.";

    // @OpenForgeText install.phrase.created-and-under-agents
    internal static string FormatCreatedAndUnderAgents(string filesText, string cliTextPluralText, string directoriesText, string cliTextPluralText2)
        => $"Created {filesText} {cliTextPluralText} and {directoriesText} {cliTextPluralText2} under .agents";

    // @OpenForgeText install.phrase.would-create-and-under-agents
    internal static string FormatWouldCreateAndUnderAgents(string filesText, string cliTextPluralText, string directoriesText, string cliTextPluralText2)
        => $"Would create {filesText} {cliTextPluralText} and {directoriesText} {cliTextPluralText2} under .agents";

    // @OpenForgeText install.phrase.writing-failed
    internal static string FormatWritingFailed(string pathText, string trimSentenceText)
        => $"Writing {pathText} failed: {trimSentenceText}.";

    // @OpenForgeText install.phrase.files-were-created
    internal static string FormatFilesWereCreated(string filesText)
        => $"{filesText} files were created.";

    // @OpenForgeText install.phrase.directories-were-created
    internal static string FormatDirectoriesWereCreated(string directoriesText)
        => $"{directoriesText} directories were created.";

    // @OpenForgeText install.phrase.files-and-directories-were-created
    internal static string FormatFilesAndDirectoriesWereCreated(string totalText)
        => $"{totalText} files and directories were created.";

    // @OpenForgeText install.phrase.would-create
    internal static string FormatWouldCreate(string countText, string nounText)
        => $"Would create {countText} {nounText}.";

    // @OpenForgeText install.phrase.created
    internal static string FormatCreated(string countText, string nounText)
        => $"Created {countText} {nounText}.";

    // @OpenForgeText install.phrase.would-add
    internal static string FormatWouldAdd(string countText, string nounText)
        => $"Would add {countText} {nounText}.";

    // @OpenForgeText install.phrase.added
    internal static string FormatAdded(string countText, string nounText)
        => $"Added {countText} {nounText}.";

    // @OpenForgeText install.phrase.ownership-record-lifecycle-is
    internal static string FormatOwnershipRecordLifecycleIs(string nameText, string nameText2)
        => $"Ownership record lifecycle is {nameText} / {nameText2}.";
}
