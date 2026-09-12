using System.Collections.Immutable;
using OpenForge.Cli.Core.Commands.Repair.Models.Application;
using OpenForge.Cli.Core.Commands.Repair.Models.Planning;
using OpenForge.Cli.Core.Framework.Recovery.Models.Entries;

namespace OpenForge.Cli.Core.Commands.Repair.Shared.Application;

internal static class RepairAtomicEffectOrder
{
    internal static ImmutableArray<RepairAtomicEffect> Read(RepairPlan plan)
    {
        var ordered = ImmutableArray.CreateBuilder<RepairAtomicEffect>();
        foreach (var effect in plan.Effects.OrderBy(
            effect => effect.SourceCanonicalPath,
            StringComparer.Ordinal))
        {
            ordered.Add(new RepairAtomicEffect(
                ordered.Count,
                RepairAtomicEffectKind.Reference,
                effect,
                libraryRecovery: null));
        }

        foreach (var effect in plan.LibrarySteps
            .Select(step => step.Effect)
            .OfType<RepairLibraryRecoveryEffect>()
            .OrderBy(effect => effect.Entry.Kind is RecoveryEntryKind.RelativeFileLinkCreate
                or RecoveryEntryKind.RelativeFileLinkDelete ? 1 : 0)
            .ThenBy(effect => effect.Entry.TargetPath, StringComparer.Ordinal))
        {
            ordered.Add(new RepairAtomicEffect(
                ordered.Count,
                RepairAtomicEffectKind.LibraryRecovery,
                reference: null,
                effect));
        }

        return ordered.ToImmutable();
    }
}
