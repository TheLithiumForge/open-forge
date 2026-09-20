using OpenForge.Cli.Core.Commands.Install.Models.Planning;
using OpenForge.Cli.Core.Commands.Install.Models.Result;

namespace OpenForge.Cli.Core.Commands.Install.Models.Operation;

internal sealed record InstallConfirmationFacts(int ReplacementCount)
{
    internal static InstallConfirmationFacts From(InstallPlan plan)
    {
        ArgumentNullException.ThrowIfNull(plan);
        var replacementPaths = plan.TargetEffects
            .Where(effect => effect.Identity.Action == InstallEffectAction.Replace)
            .Select(effect => effect.Identity.Path)
            .Distinct(StringComparer.Ordinal);
        return new(replacementPaths.Count());
    }
}
