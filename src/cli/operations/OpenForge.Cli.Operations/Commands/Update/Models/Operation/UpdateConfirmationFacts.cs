using OpenForge.Cli.Core.Commands.Update.Models.Comparison;
using OpenForge.Cli.Core.Commands.Update.Models.Effects;
using OpenForge.Cli.Core.Commands.Update.Models.Planning;

namespace OpenForge.Cli.Core.Commands.Update.Models.Operation;

internal sealed record UpdateConfirmationFacts(int ReplacementCount, int DeletionCount)
{
    internal static UpdateConfirmationFacts From(UpdatePlanExecution execution)
    {
        ArgumentNullException.ThrowIfNull(execution);
        var deletionPaths = execution.Effects
            .Where(effect => effect.ResultEffect.Action == UpdatePhysicalEffectAction.Delete)
            .Select(effect => effect.ResultEffect.Path)
            .Distinct(StringComparer.Ordinal);
        var replacementPaths = execution.Effects
            .Where(effect => effect.ResultEffect.Action == UpdatePhysicalEffectAction.Replace
                && !execution.Migrations.Any(migration => migration.Path == effect.ResultEffect.Path && migration.IsUserOwnedSource)
                && effect.ResultEffect.Changes.Any(change => change.Kind == UpdateComparisonTargetKind.File
                    && change.Action == UpdateLogicalChangeAction.Replace))
            .Select(effect => effect.ResultEffect.Path)
            .Distinct(StringComparer.Ordinal);
        return new(ReplacementCount: replacementPaths.Count(), DeletionCount: deletionPaths.Count());
    }
}
