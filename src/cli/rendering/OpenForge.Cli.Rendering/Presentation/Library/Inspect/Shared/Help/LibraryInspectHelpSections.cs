using OpenForge.Cli.Core.Presentation.Library.Inspect.Shared.Wording;
using OpenForge.Cli.Core.Presentation.Shared.Wording;
using OpenForge.Cli.Core.Shell.Presentation.Models;

namespace OpenForge.Cli.Core.Presentation.Library.Inspect.Shared.Help;

internal static class LibraryInspectHelpSections
{
    internal static CliHelpContent Create() => new(
    [
        new CliHelpSection(global::OpenForge.Cli.OutputText.Shared.SharedText.HelpHeadingSyntax(), LibraryInspectWording.HelpSyntax()),
        new CliHelpSection(global::OpenForge.Cli.OutputText.Shared.SharedText.HelpHeadingInspection(), LibraryInspectWording.HelpInspection()),
        new CliHelpSection(global::OpenForge.Cli.OutputText.Shared.SharedText.HelpHeadingGlobalOptions(), LibraryInspectWording.HelpGlobalOptions()),
        new CliHelpSection(global::OpenForge.Cli.OutputText.Shared.SharedText.HelpHeadingExamples(), LibraryInspectWording.HelpExamples()),
        new CliHelpSection(global::OpenForge.Cli.OutputText.Shared.SharedText.HelpHeadingResultsAndStreams(), CliResultHelp.ResultsAndStreams()),
    ]);
}
