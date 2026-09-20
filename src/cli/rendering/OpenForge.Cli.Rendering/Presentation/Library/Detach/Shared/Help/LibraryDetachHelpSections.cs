using OpenForge.Cli.Core.Presentation.Library.Detach.Shared.Wording;
using OpenForge.Cli.Core.Presentation.Shared.Wording;
using OpenForge.Cli.Core.Shell.Presentation.Models;

namespace OpenForge.Cli.Core.Presentation.Library.Detach.Shared.Help;

internal static class LibraryDetachHelpSections
{
    internal static CliHelpContent Create() => new(
    [
        new CliHelpSection(global::OpenForge.Cli.OutputText.Shared.SharedText.HelpHeadingSyntax(), LibraryDetachWording.HelpSyntax()),
        new CliHelpSection(global::OpenForge.Cli.OutputText.Shared.SharedText.HelpHeadingWritePolicy(), LibraryDetachWording.HelpPolicy()),
        new CliHelpSection(global::OpenForge.Cli.OutputText.Shared.SharedText.HelpHeadingPermissions(), LibraryDetachWording.HelpPermissions()),
        new CliHelpSection(global::OpenForge.Cli.OutputText.Shared.SharedText.HelpHeadingGlobalOptions(), LibraryDetachWording.HelpGlobalOptions()),
        new CliHelpSection(global::OpenForge.Cli.OutputText.Shared.SharedText.HelpHeadingExamples(), LibraryDetachWording.HelpExamples()),
        new CliHelpSection(global::OpenForge.Cli.OutputText.Shared.SharedText.HelpHeadingResultsAndStreams(), CliResultHelp.ResultsAndStreams()),
    ]);
}
