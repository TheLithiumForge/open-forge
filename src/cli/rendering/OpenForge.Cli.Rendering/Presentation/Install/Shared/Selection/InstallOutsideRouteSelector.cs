using OpenForge.Cli.Core.Commands.Install.Models.Result;
using OpenForge.Cli.Core.Presentation.Shared.Text;
using OpenForge.Cli.OutputText.Install;

namespace OpenForge.Cli.Core.Presentation.Install.Shared.Selection;

/// <summary>Reports the docs folder chosen at the first-installation question once Install has applied.</summary>
internal static class InstallOutsideRouteSelector
{
    private const string IncludeCommand = "open-forge route include";

    internal static IReadOnlyList<string> Select(InstallResult result)
    {
        if (result.OutsideRoute is not { } outcome) return [];
        var folder = CliText.Escape(outcome.Folder);
        var retry = $"{IncludeCommand} {folder}";
        return outcome.State switch
        {
            InstallOutsideRouteState.Registered => [InstallSetupText.OutsideRouteRegistered(folder, CliText.Escape(outcome.Catalogue ?? string.Empty))],
            InstallOutsideRouteState.AlreadyRegistered => [InstallSetupText.OutsideRouteKnown(folder)],
            InstallOutsideRouteState.Failed => [InstallSetupText.OutsideRouteFailed(folder, CliText.Escape(outcome.Cause ?? string.Empty), retry)],
            InstallOutsideRouteState.NotAttempted => [InstallSetupText.OutsideRouteNotAttempted(folder, retry)],
            _ => throw new ArgumentOutOfRangeException(nameof(result)),
        };
    }
}
