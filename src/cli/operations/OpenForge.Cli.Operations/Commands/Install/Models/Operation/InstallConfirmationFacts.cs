using OpenForge.Cli.Core.Commands.Install.Models.Planning;
using OpenForge.Cli.Core.Commands.Install.Models.Result;

namespace OpenForge.Cli.Core.Commands.Install.Models.Operation;

internal sealed record InstallConfirmationFacts(int ReplacementCount)
{
    internal static InstallConfirmationFacts From(InstallPlan plan)
    {
        ArgumentNullException.ThrowIfNull(plan);
        var replacementPaths = plan.TargetEffects
            .Where(effect => effect.Identity.Kind == InstallEffectKind.File
                && effect.Identity.Action == InstallEffectAction.Replace
                && effect.Identity.ContentChange == InstallEffectContentChange.WholeFile)
            .Select(effect => effect.Identity.Path)
            .Distinct(StringComparer.Ordinal);
        return new(replacementPaths.Count());
    }
}
