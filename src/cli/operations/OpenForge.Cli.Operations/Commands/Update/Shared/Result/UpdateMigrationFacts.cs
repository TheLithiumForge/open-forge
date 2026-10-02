using OpenForge.Cli.Core.Commands.Update.Models.Effects;
using OpenForge.Cli.Core.Commands.Update.Models.Planning;
using OpenForge.Cli.Core.Commands.Update.Models.Result;
using OpenForge.Cli.Core.Framework.Filesystem.Shared.Paths;

namespace OpenForge.Cli.Core.Commands.Update.Shared.Result;

internal static class UpdateMigrationFacts
{
    internal static IReadOnlyList<UpdateMigration> Create(
        IReadOnlyList<UpdateMigrationPlan> plans,
        IReadOnlyList<UpdatePhysicalEffect> effects)
    {
        ArgumentNullException.ThrowIfNull(plans);
        ArgumentNullException.ThrowIfNull(effects);

        var planValues = plans
            .Select(plan => plan ?? throw new ArgumentException(
                "Update migration plans cannot contain null members.",
                nameof(plans)))
            .ToArray();
        var planPaths = new HashSet<string>(StringComparer.Ordinal);
        foreach (var plan in planValues)
        {
            if (!PortableWorkspacePath.TryNormalize(plan.Path, out var normalizedPath)
                || !string.Equals(plan.Path, normalizedPath, StringComparison.Ordinal))
            {
                throw new ArgumentException(
                    "Update migration plan paths must be canonical workspace-relative paths.",
                    nameof(plans));
            }
            if (!planPaths.Add(plan.Path))
            {
                throw new ArgumentException(
                    "Update migration plans cannot contain duplicate paths.",
                    nameof(plans));
            }
        }

        var effectValues = effects
            .Select(effect => effect ?? throw new ArgumentException(
                "Update physical effects cannot contain null members.",
                nameof(effects)))
            .ToArray();
        var effectPaths = new HashSet<string>(StringComparer.Ordinal);
        foreach (var effect in effectValues)
        {
            if (!effectPaths.Add(effect.Path))
            {
                throw new ArgumentException(
                    "Update physical effects cannot contain duplicate paths.",
                    nameof(effects));
            }
        }

        if (planValues.Length == 0)
        {
            return [];
        }

        var appliedPaths = effectValues
            .Where(effect => effect.Kind != UpdatePhysicalEffectKind.Directory
                && effect.Outcome == UpdatePhysicalEffectOutcome.Verified)
            .Select(effect => effect.Path)
            .ToHashSet(StringComparer.Ordinal);

        return planValues
            .OrderBy(plan => plan.Path, StringComparer.Ordinal)
            .Select(plan => new UpdateMigration(
                plan.Path,
                plan.Actions,
                plan.Fields,
                plan.Derivation,
                appliedPaths.Contains(plan.Path)
                    ? UpdateMigrationOutcome.Applied
                    : UpdateMigrationOutcome.Planned)
            {
                IsUserOwnedSource = plan.IsUserOwnedSource,
            })
            .ToArray();
    }
}
