using OpenForge.Cli.Core.Commands.Extension.Create.Models.Result;
using OpenForge.Cli.Core.Shell.Interaction.Models;

namespace OpenForge.Cli.Core.Commands.Extension.Create.Models.Interaction;

/// <summary>
/// Presentation supplies the typed interaction capabilities used by Extension Create.
/// The command layer deliberately receives callables instead of a terminal or renderer.
/// </summary>
internal sealed record ExtensionCreateInteraction
{
    internal required CliPrompt<CliTextQuestion<string>, string> Text { get; init; }

    internal required string ExtensionIdLabel { get; init; }

    internal required string ExtensionIdRule { get; init; }

    internal required Func<string, string> InvalidExtensionId { get; init; }

    internal required string PackageFolderLabel { get; init; }

    internal required string PackageFolderRule { get; init; }

    internal required CliConfirmQuestion ConfirmationQuestion { get; init; }

    internal required CliPlanConfirmation<ExtensionCreateResult, CliConfirmQuestion> Confirm { get; init; }
}
