using System.Text;
using OpenForge.Cli.Core.Commands.Status.Models.Result;
using OpenForge.Cli.Core.Framework.OperationalContributors.Models;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.Core.Commands.Status.Shared.Rendering;

internal static class StatusFindingHumanRenderer
{
    internal static void Append(StringBuilder builder, StatusResult result)
    {
        var navigationPaths = result.Facts.Structure.GeneratedNavigation
            .Where(target => target.State != OperationalGeneratedNavigationState.Current)
            .Select(target => target.Path)
            .ToHashSet(StringComparer.Ordinal);
        foreach (var group in result.Findings.GroupBy(finding => (finding.Code, finding.Status, finding.Cause)))
        {
            var status = group.Key.Status == CliSemanticStatus.Attention ? "requires attention" : CliStatusDefinitions.Read(group.Key.Status).MachineName;
            builder.AppendLine($"{status.ToUpperInvariant()}: {StatusHumanRenderer.Text(group.Key.Cause)} [{StatusDefinitions.ReadFindingCode(group.Key.Code)}]");
            var shownUnderRoutes = false;
            foreach (var subject in group.Select(finding => finding.Subject).OfType<string>().Distinct(StringComparer.Ordinal))
            {
                if (IsNavigation(group.Key.Code) && navigationPaths.Contains(subject))
                {
                    shownUnderRoutes = true;
                }
                else
                {
                    builder.AppendLine($"  {StatusHumanRenderer.Text(subject)}");
                }
            }

            if (shownUnderRoutes)
            {
                builder.AppendLine("  Affected navigation paths are listed under Routes.");
            }
        }
    }

    private static bool IsNavigation(StatusFindingCode code)
        => code is StatusFindingCode.GeneratedNavigationChanged or StatusFindingCode.GeneratedNavigationMissing
            or StatusFindingCode.GeneratedNavigationUnavailable or StatusFindingCode.GeneratedNavigationBlocked;
}
