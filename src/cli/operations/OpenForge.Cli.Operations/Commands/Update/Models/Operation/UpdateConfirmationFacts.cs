using OpenForge.Cli.Core.Commands.Update.Models.Effects;
using OpenForge.Cli.Core.Commands.Update.Models.Planning;

namespace OpenForge.Cli.Core.Commands.Update.Models.Operation;

internal sealed record UpdateConfirmationFacts(int DeletionCount)
{
    internal static UpdateConfirmationFacts From(UpdatePlanExecution execution)
    {
        ArgumentNullException.ThrowIfNull(execution);
        var deletionPaths = execution.Effects
            .Where(effect => effect.ResultEffect.Action == UpdatePhysicalEffectAction.Delete)
            .Select(effect => effect.ResultEffect.Path)
            .Distinct(StringComparer.Ordinal);
        return new(deletionPaths.Count());
    }
}
