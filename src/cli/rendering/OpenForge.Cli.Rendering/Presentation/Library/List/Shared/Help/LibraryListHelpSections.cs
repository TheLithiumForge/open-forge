using OpenForge.Cli.Core.Presentation.Library.List.Shared.Wording;
using OpenForge.Cli.Core.Presentation.Shared.Wording;
using OpenForge.Cli.Core.Shell.Presentation.Models;

namespace OpenForge.Cli.Core.Presentation.Library.List.Shared.Help;

internal static class LibraryListHelpSections
{
    internal static CliHelpContent Create() => new(
    [
        new CliHelpSection(global::OpenForge.Cli.OutputText.Shared.SharedText.HelpHeadingSyntax(), LibraryListWording.HelpSyntax()),
        new CliHelpSection(global::OpenForge.Cli.OutputText.Shared.SharedText.HelpHeadingInspection(), LibraryListWording.HelpInspection()),
        new CliHelpSection(global::OpenForge.Cli.OutputText.Shared.SharedText.HelpHeadingGlobalOptions(), LibraryListWording.HelpGlobalOptions()),
        new CliHelpSection(global::OpenForge.Cli.OutputText.Shared.SharedText.HelpHeadingExamples(), LibraryListWording.HelpExamples()),
        new CliHelpSection(global::OpenForge.Cli.OutputText.Shared.SharedText.HelpHeadingResultsAndStreams(), CliResultHelp.ResultsAndStreams()),
    ]);
}
