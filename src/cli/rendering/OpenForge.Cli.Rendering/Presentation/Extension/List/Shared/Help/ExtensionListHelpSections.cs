using OpenForge.Cli.Core.Presentation.Extension.List.Shared.Wording;
using OpenForge.Cli.Core.Presentation.Shared.Wording;
using OpenForge.Cli.Core.Shell.Presentation.Models;

namespace OpenForge.Cli.Core.Presentation.Extension.List.Shared.Help;

internal static class ExtensionListHelpSections
{
    internal static CliHelpContent Create()
        => new(
        [
            new CliHelpSection(
                global::OpenForge.Cli.OutputText.Shared.SharedText.HelpHeadingSyntax(),
                ("  " + global::OpenForge.Cli.OutputText.Extension.List.ExtensionListText.HelpSyntax())),
            new CliHelpSection(
                global::OpenForge.Cli.OutputText.Extension.List.ExtensionListText.HelpHeadingSections(),
                ("  " + global::OpenForge.Cli.OutputText.Extension.List.ExtensionListText.HelpSections())),
            new CliHelpSection(
                global::OpenForge.Cli.OutputText.Extension.List.ExtensionListText.HelpHeadingSourceAndWorkspace(),
                ("  " + global::OpenForge.Cli.OutputText.Extension.List.ExtensionListText.HelpSourceAndWorkspace())),
            new CliHelpSection(
                global::OpenForge.Cli.OutputText.Shared.SharedText.HelpHeadingGlobalOptions(),
                ("  " + global::OpenForge.Cli.OutputText.Shared.SharedText.HelpGlobalOptionsWithDetailSelection())),
            new CliHelpSection(
                global::OpenForge.Cli.OutputText.Shared.SharedText.HelpHeadingExamples(),
                ("  " + global::OpenForge.Cli.OutputText.Extension.List.ExtensionListText.HelpExamples())),
            new CliHelpSection(
                global::OpenForge.Cli.OutputText.Shared.SharedText.HelpHeadingResultsAndStreams(),
                CliResultHelp.ResultsAndStreams()),
            new CliHelpSection(
                global::OpenForge.Cli.OutputText.Shared.SharedText.HelpHeadingNotes(),
                ("  " + global::OpenForge.Cli.OutputText.Extension.List.ExtensionListText.HelpNotes())),
        ]);
}
