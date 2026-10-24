using OpenForge.Cli.Core.Commands.Route.Shared.Recovery.Models;
using OpenForge.Cli.Core.Presentation.Shared.Models;
using OpenForge.Cli.Core.Presentation.Shared.Wording;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.Core.Presentation.Route.Shared.Recovery;

internal static class RouteRecoveryHistoryFindings
{
    internal static IEnumerable<CliFinding> Read(RouteRecoveryHistory history, string historyCode, string unavailableCode)
    {
        foreach (var path in history.Paths)
        {
            var message = CliFindingWording.RecoveryHistory(path);
            yield return new CliFinding
            {
                Code = historyCode,
                Severity = CliSeverity.Warning,
                Title = global::OpenForge.Cli.OutputText.Route.Shared.RouteSharedText.TitleRecoveryHistoryRetained(),
                Message = message,
                Subject = new CliSubject(CliSubjectKind.File, path),
                Resolution = CliResolution.Informational,
            };
        }

        foreach (var unavailable in history.UnavailableStores)
        {
            var message = CliFindingWording.RecoveryCoverageUnavailable(unavailable.Path, CliFindingWording.PlainCause(unavailable.Cause));
            yield return new CliFinding
            {
                Code = unavailableCode,
                Severity = CliSeverity.Warning,
                Title = global::OpenForge.Cli.OutputText.Route.Shared.RouteSharedText.TitleRecoveryHistoryUnavailable(),
                Message = message,
                Subject = new CliSubject(CliSubjectKind.Directory, unavailable.Path),
                Resolution = CliResolution.Informational,
            };
        }
    }
}
