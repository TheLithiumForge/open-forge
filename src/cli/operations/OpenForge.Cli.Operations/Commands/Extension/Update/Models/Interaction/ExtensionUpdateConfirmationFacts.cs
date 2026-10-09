using OpenForge.Cli.Core.Commands.Extension.Update.Models.Result;
using OpenForge.Cli.Core.Commands.Extension.Update.Models.Planning;

namespace OpenForge.Cli.Core.Commands.Extension.Update.Models.Interaction;

internal sealed record ExtensionUpdateConfirmationFacts(int ReplacementCount, int DeletionCount)
{
    internal static ExtensionUpdateConfirmationFacts From(ExtensionUpdatePlan plan)
    {
        ArgumentNullException.ThrowIfNull(plan);
        var replacements = plan.Effects
            .Where(effect => effect.Result.Action == ExtensionUpdateEffectAction.Replace
                && effect.Result.Changes.Any(change => change.Kind == ExtensionUpdateComparisonTargetKind.PackageFile
                    && change.Action == ExtensionUpdateChangeAction.Replace))
            .Select(effect => effect.Result.Path)
            .Distinct(StringComparer.Ordinal)
            .Count();
        var deletions = plan.Effects
            .Where(effect => effect.Result.Action == ExtensionUpdateEffectAction.Delete)
            .Select(effect => effect.Result.Path)
            .Distinct(StringComparer.Ordinal)
            .Count();
        return new(ReplacementCount: replacements, DeletionCount: deletions);
    }
}
