using OpenForge.Cli.Core.Commands.Repair.Models.Planning;
using OpenForge.Cli.Core.Commands.Repair.Models.Result;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem;

namespace OpenForge.Cli.Core.Commands.Repair.Shared.Result;

internal static class RepairStepOutcomeReader
{
    internal static RepairPlan Project(RepairResultInput input)
        => new(input.Plan.Request, input.Plan.Selection,
            input.Plan.Steps.Select(step => new RepairStep(
                step.Ordinal, step.SelectedProposal, step.Dependency, step.Verification, step.Recovery,
                step.Effect, step.NoOp, Read(step, input))), input.Plan.Conflicts);

    private static RepairStepOutcome Read(RepairStep step, RepairResultInput input)
    {
        if (step.Effect is null)
        {
            return step.Outcome;
        }

        var verified = input.Verification.Effects.SingleOrDefault(value => ReferenceEquals(value.Effect, step.Effect));
        if (verified is not null)
        {
            if (verified.Targets is RepairVerificationState.Failed or RepairVerificationState.Unknown
                || verified.ResultingBytes is RepairVerificationState.Failed or RepairVerificationState.Unknown)
            {
                return RepairStepOutcome.Failed;
            }

            if (verified.Receipt?.EffectState == FilesystemEffectState.Applied)
            {
                return verified.Targets == RepairVerificationState.Verified
                    && verified.ResultingBytes == RepairVerificationState.Verified
                    && input.Verification.PostConditions == RepairVerificationState.Verified
                    ? RepairStepOutcome.Verified : RepairStepOutcome.Applied;
            }
        }

        return input.Application.State == RepairApplicationState.Interrupted
            ? RepairStepOutcome.Interrupted : RepairStepOutcome.Planned;
    }
}
