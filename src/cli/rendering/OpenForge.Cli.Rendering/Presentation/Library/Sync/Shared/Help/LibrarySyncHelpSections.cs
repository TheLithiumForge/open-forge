using OpenForge.Cli.Core.Presentation.Library.Sync.Shared.Wording;
using OpenForge.Cli.Core.Presentation.Shared.Wording;
using OpenForge.Cli.Core.Shell.Presentation.Models;

namespace OpenForge.Cli.Core.Presentation.Library.Sync.Shared.Help;

internal static class LibrarySyncHelpSections
{
    internal static CliHelpContent Create() => new(
    [
        new CliHelpSection(global::OpenForge.Cli.OutputText.Shared.SharedText.HelpHeadingSyntax(), LibrarySyncWording.HelpSyntax()),
        new CliHelpSection(global::OpenForge.Cli.OutputText.Shared.SharedText.HelpHeadingWritePolicy(), LibrarySyncWording.HelpWritePolicy()),
        new CliHelpSection(global::OpenForge.Cli.OutputText.Shared.SharedText.HelpHeadingPermissions(), LibrarySyncWording.HelpPermissions()),
        new CliHelpSection(global::OpenForge.Cli.OutputText.Shared.SharedText.HelpHeadingGlobalOptions(), LibrarySyncWording.HelpGlobalOptions()),
        new CliHelpSection(global::OpenForge.Cli.OutputText.Shared.SharedText.HelpHeadingExamples(), LibrarySyncWording.HelpExamples()),
        new CliHelpSection(global::OpenForge.Cli.OutputText.Shared.SharedText.HelpHeadingResultsAndStreams(), CliResultHelp.ResultsAndStreams()),
    ]);
}
