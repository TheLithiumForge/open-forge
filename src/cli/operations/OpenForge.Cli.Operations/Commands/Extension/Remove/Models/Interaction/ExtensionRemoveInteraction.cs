using OpenForge.Cli.Core.Commands.Extension.Remove.Models.Result;
using OpenForge.Cli.Core.Shell.Interaction.Models;

namespace OpenForge.Cli.Core.Commands.Extension.Remove.Models.Interaction;

/// <summary>
/// Presentation supplies the typed interaction capabilities used by Extension Remove.
/// </summary>
internal sealed record ExtensionRemoveInteraction
{
    internal required CliPrompt<CliMultiSelectQuestion<string>, CliMultiSelection<string>> SelectPackages { get; init; }

    internal required string SelectionQuestion { get; init; }

    internal required CliPrompt<CliPermissionQuestion, CliPermissionChoice> Permission { get; init; }

    internal required Func<int, CliConfirmQuestion> DeleteQuestion { get; init; }

    internal required CliPlanConfirmation<ExtensionRemoveResult, CliConfirmQuestion> Apply { get; init; }
}
