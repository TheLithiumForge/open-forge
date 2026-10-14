using OpenForge.Cli.Core.Shell.Definitions.Shared.Commands;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Operation;
using OpenForge.Cli.OutputText.Library.Shared;

namespace OpenForge.Cli.Core.Presentation.Shared.Wording;

internal static class LibraryLinkRecreationWording
{
    // Project primitive presentation facts at each command boundary. Shared rendering cannot import command models.
    internal static CliNextAction Next(string workspace, string id, string sourceRoot, string destinationRoot, bool gitIgnore)
    {
        var prefix = $"open-forge --workspace {Quote(workspace)} library";
        var detach = $"{prefix} detach {id} --automatic";
        var ignore = gitIgnore ? "true" : "false";
        var attach = $"{prefix} attach {id} {Quote(sourceRoot)} --to {Quote(destinationRoot)} --git-ignore {ignore} --automatic";
        var settings = System.IO.Path.Combine(workspace, ".agents", "open-forge.json");
        return new CliNextAction(LibraryLinkRecreationText.Steps(detach: detach, id: id, settings: Quote(settings), attach: attach), LibraryLinkRecreationText.Reason())
        {
            Kind = CliNextActionKind.Sentence,
        };
    }

    private static string Quote(string value) => CliCommandText.Quote(value);
}
