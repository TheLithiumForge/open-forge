using OpenForge.Cli.Core.Presentation.Doctor.Shared.Wording;
using OpenForge.Cli.Core.Presentation.Shared.Wording;
using OpenForge.Cli.Core.Shell.Presentation.Models;

namespace OpenForge.Cli.Core.Presentation.Doctor.Shared.Help;

internal static class DoctorHelpSections
{
    internal static CliHelpContent Create()
        => new(
        [
            new CliHelpSection(global::OpenForge.Cli.OutputText.Shared.SharedText.HelpHeadingSyntax(), DoctorWording.HelpSyntax()),
            new CliHelpSection(global::OpenForge.Cli.OutputText.Shared.SharedText.HelpHeadingInspection(), DoctorWording.HelpInspection()),
            new CliHelpSection(global::OpenForge.Cli.OutputText.Shared.SharedText.HelpHeadingGlobalOptions(), DoctorWording.HelpGlobalOptions()),
            new CliHelpSection(global::OpenForge.Cli.OutputText.Shared.SharedText.HelpHeadingExamples(), DoctorWording.HelpExamples()),
            new CliHelpSection(global::OpenForge.Cli.OutputText.Shared.SharedText.HelpHeadingResultsAndStreams(), CliResultHelp.ResultsAndStreams()),
        ]);
}
