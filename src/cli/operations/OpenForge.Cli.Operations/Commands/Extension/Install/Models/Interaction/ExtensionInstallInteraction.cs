using OpenForge.Cli.Core.Commands.Extension.Install.Models.Result;
using OpenForge.Cli.Core.Shell.Interaction.Models;

namespace OpenForge.Cli.Core.Commands.Extension.Install.Models.Interaction;

/// <summary>
/// Presentation supplies the typed interaction capabilities used by Extension Install.
/// </summary>
internal sealed record ExtensionInstallInteraction
{
    internal required CliPrompt<CliMultiSelectQuestion<string>, CliMultiSelection<string>> SelectPackages { get; init; }

    internal required string SelectionQuestion { get; init; }

    internal required CliPrompt<CliPermissionQuestion, CliPermissionChoice> Permission { get; init; }

    internal required CliPlanConfirmation<ExtensionInstallResult, CliConfirmQuestion> Force { get; init; }

    internal required Func<IReadOnlyList<string>, CliConfirmQuestion> ForceQuestion { get; init; }

    internal required CliConfirmQuestion ApplyQuestion { get; init; }

    internal required CliPlanConfirmation<ExtensionInstallResult, CliConfirmQuestion> Apply { get; init; }
}
