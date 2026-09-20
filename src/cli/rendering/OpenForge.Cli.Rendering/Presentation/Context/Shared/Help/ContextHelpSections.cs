using OpenForge.Cli.Core.Presentation.Context.Shared.Wording;
using OpenForge.Cli.Core.Presentation.Shared.Wording;
using OpenForge.Cli.Core.Shell.Presentation.Models;

namespace OpenForge.Cli.Core.Presentation.Context.Shared.Help;

internal static class ContextHelpSections
{
    internal static CliHelpContent Create()
        => new(
        [
            new CliHelpSection(global::OpenForge.Cli.OutputText.Shared.SharedText.HelpHeadingSyntax(), ContextWording.HelpSyntax()),
            new CliHelpSection(global::OpenForge.Cli.OutputText.Shared.SharedText.HelpHeadingSelection(), ContextWording.HelpSelection()),
            new CliHelpSection(global::OpenForge.Cli.OutputText.Context.ContextText.HelpHeadingContent(), ContextWording.HelpContent()),
            new CliHelpSection(global::OpenForge.Cli.OutputText.Shared.SharedText.HelpHeadingLinks(), ContextWording.HelpLinks()),
            new CliHelpSection(global::OpenForge.Cli.OutputText.Shared.SharedText.HelpHeadingGlobalOptions(), ContextWording.HelpGlobalOptions()),
            new CliHelpSection(global::OpenForge.Cli.OutputText.Shared.SharedText.HelpHeadingExamples(), ContextWording.HelpExamples()),
            new CliHelpSection(global::OpenForge.Cli.OutputText.Shared.SharedText.HelpHeadingRelatedCommands(), ContextWording.HelpRelatedCommands()),
            new CliHelpSection(global::OpenForge.Cli.OutputText.Shared.SharedText.HelpHeadingResultsAndStreams(), CliResultHelp.ResultsAndStreams()),
            new CliHelpSection(global::OpenForge.Cli.OutputText.Shared.SharedText.HelpHeadingNotes(), ContextWording.HelpNotes()),
        ]);
}
