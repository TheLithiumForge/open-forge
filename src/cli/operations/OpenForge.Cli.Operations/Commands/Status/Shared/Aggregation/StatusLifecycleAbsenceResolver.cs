using OpenForge.Cli.Core.Commands.Status.Models.Operation;
using OpenForge.Cli.Core.Framework.OperationalContributors;
using OpenForge.Cli.Core.Framework.OperationalContributors.Models;

namespace OpenForge.Cli.Core.Commands.Status.Shared.Aggregation;

internal static class StatusLifecycleAbsenceResolver
{
    internal static bool IsProven(StatusObservationSet observations)
        => OperationalLifecycleAbsenceProof.IsProven(
            OperationalLifecycleAbsenceEvidence.FromStatus(
                observations.WorkspaceEntry,
                observations.Routes,
                observations.RecoveryResiduals,
                observations.FrameworkLifecycle,
                observations.ExtensionLifecycle));
}
