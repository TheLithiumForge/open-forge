using OpenForge.Cli.Core.Commands.Extension.Update.Models.Result;
using OpenForge.Cli.Core.Shell.Interaction.Models;

namespace OpenForge.Cli.Core.Commands.Extension.Update.Models.Interaction;

/// <summary>
/// Presentation supplies the typed interaction capabilities used by Extension Update.
/// </summary>
internal sealed record ExtensionUpdateInteraction
{
    internal required CliPrompt<CliMultiSelectQuestion<string>, CliMultiSelection<string>> SelectPackages { get; init; }

    internal required string SelectionQuestion { get; init; }

    internal required CliPrompt<CliPermissionQuestion, CliPermissionChoice> Permission { get; init; }

    internal required CliConfirmQuestion ApplyQuestion { get; init; }

    internal required Func<int, CliConfirmQuestion> PruneQuestion { get; init; }

    internal required CliPlanConfirmation<ExtensionUpdateResult, CliConfirmQuestion> Apply { get; init; }
}
