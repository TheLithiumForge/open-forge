using OpenForge.Cli.Core.Shell.Presentation.Models;
using OpenForge.Cli.Core.Presentation.Shared.Wording;

namespace OpenForge.Cli.Core.Presentation.Extension.Inspect.Shared.Help;

internal static class ExtensionInspectHelpSections
{
    internal static CliHelpContent Create()
        => new(
        [
            new CliHelpSection(
                global::OpenForge.Cli.OutputText.Shared.SharedText.HelpHeadingSyntax(),
                ("  " + global::OpenForge.Cli.OutputText.Extension.Inspect.ExtensionInspectText.HelpSyntax())),
            new CliHelpSection(
                global::OpenForge.Cli.OutputText.Extension.Inspect.ExtensionInspectText.HelpHeadingSubjectAndSource(),
                ("  " + global::OpenForge.Cli.OutputText.Extension.Inspect.ExtensionInspectText.HelpSubjectAndSource())),
            new CliHelpSection(
                global::OpenForge.Cli.OutputText.Shared.SharedText.HelpHeadingInspection(),
                ("  " + global::OpenForge.Cli.OutputText.Extension.Inspect.ExtensionInspectText.HelpInspection())),
            new CliHelpSection(
                global::OpenForge.Cli.OutputText.Shared.SharedText.HelpHeadingGlobalOptions(),
                ("  " + global::OpenForge.Cli.OutputText.Shared.SharedText.HelpGlobalOptionsWithDetailSelection())),
            new CliHelpSection(
                global::OpenForge.Cli.OutputText.Shared.SharedText.HelpHeadingExamples(),
                ("  " + global::OpenForge.Cli.OutputText.Extension.Inspect.ExtensionInspectText.HelpExamples())),
            new CliHelpSection(
                global::OpenForge.Cli.OutputText.Shared.SharedText.HelpHeadingResultsAndStreams(),
                CliResultHelp.ResultsAndStreams()),
            new CliHelpSection(
                global::OpenForge.Cli.OutputText.Extension.Inspect.ExtensionInspectText.HelpHeadingFingerprintBoundary(),
                ("  " + global::OpenForge.Cli.OutputText.Extension.Inspect.ExtensionInspectText.HelpFingerprintBoundary())),
        ]);
}
