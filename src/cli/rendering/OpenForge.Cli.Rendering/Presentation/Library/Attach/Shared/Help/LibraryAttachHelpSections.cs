using OpenForge.Cli.Core.Presentation.Library.Attach.Shared.Wording;
using OpenForge.Cli.Core.Presentation.Shared.Wording;
using OpenForge.Cli.Core.Shell.Presentation.Models;

namespace OpenForge.Cli.Core.Presentation.Library.Attach.Shared.Help;

internal static class LibraryAttachHelpSections
{
    internal static CliHelpContent Create() => new(
    [
        new CliHelpSection(global::OpenForge.Cli.OutputText.Shared.SharedText.HelpHeadingSyntax(), LibraryAttachWording.HelpSyntax()),
        new CliHelpSection(global::OpenForge.Cli.OutputText.Library.Attach.LibraryAttachText.HelpHeadingSourceAndDestination(), LibraryAttachWording.HelpSourceAndDestination()),
        new CliHelpSection(global::OpenForge.Cli.OutputText.Library.Attach.LibraryAttachText.HelpHeadingPreviewAndPermissions(), LibraryAttachWording.HelpPreviewAndPermissions()),
        new CliHelpSection(global::OpenForge.Cli.OutputText.Shared.SharedText.HelpHeadingGlobalOptions(), LibraryAttachWording.GlobalOptions()),
        new CliHelpSection(global::OpenForge.Cli.OutputText.Shared.SharedText.HelpHeadingExamples(), LibraryAttachWording.HelpExamples()),
        new CliHelpSection(global::OpenForge.Cli.OutputText.Shared.SharedText.HelpHeadingResultsAndStreams(), CliResultHelp.ResultsAndStreams()),
    ]);
}
