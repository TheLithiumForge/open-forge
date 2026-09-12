using OpenForge.Cli.Core.Commands.Route.Update.Models.Operation;
using OpenForge.Cli.Core.Commands.Route.Update.Models.Planning;
using OpenForge.Cli.Core.Commands.Route.Update.Models.Result;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Files;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Receipts;

namespace OpenForge.Cli.Core.Commands.Route.Update.Shared.Application;

internal static class RouteUpdateApplicationResultFactory
{
    internal static RouteUpdateResultFormation Build(
        RouteUpdatePlan plan,
        RouteUpdateApplicationProgress progress)
        => plan.Preview with
        {
            Effects = [.. plan.Preview.Effects.Select(effect => Apply(plan, progress, effect))],
            Recovery = progress.Recovery,
            Verification = progress.Verification,
            Findings = [.. plan.Preview.Findings.Concat(progress.Findings)],
        };

    private static RouteUpdateEffect Apply(
        RouteUpdatePlan plan,
        RouteUpdateApplicationProgress progress,
        RouteUpdateEffect effect)
    {
        var receipt = progress.Receipts.FirstOrDefault(candidate =>
            PathEquals(plan, candidate.Change.LogicalPath, effect.Path));
        if (receipt is not null)
        {
            var outcome = ReadOutcome(receipt);
            return effect with
            {
                Outcome = outcome,
                Residual = ReadResidual(receipt, outcome, progress.Verification),
            };
        }

        var attempted = progress.UncertainAttempt is { } uncertain
            && PathEquals(plan, uncertain.LogicalPath, effect.Path);
        if (attempted)
        {
            return effect with
            {
                Outcome = RouteUpdateEffectOutcome.CompletionUnknown,
                Residual = RouteUpdateEffectResidual.Unknown,
            };
        }

        return effect with
        {
            Outcome = RouteUpdateEffectOutcome.NotStarted,
            Residual = RouteUpdateEffectResidual.None,
        };
    }

    private static RouteUpdateEffectOutcome ReadOutcome(FileChangeReceipt receipt)
        => (receipt.EffectState, receipt.VerificationState) switch
        {
            (FilesystemEffectState.Applied, FilesystemVerificationState.Verified) =>
                RouteUpdateEffectOutcome.Verified,
            (FilesystemEffectState.Applied, FilesystemVerificationState.Failed) =>
                RouteUpdateEffectOutcome.VerificationFailed,
            (FilesystemEffectState.Unknown, _) => RouteUpdateEffectOutcome.CompletionUnknown,
            (FilesystemEffectState.NotStarted, FilesystemVerificationState.NotStarted) =>
                RouteUpdateEffectOutcome.NotStarted,
            _ => throw new InvalidOperationException(
                "The Route Update filesystem receipt is incoherent."),
        };

    private static RouteUpdateEffectResidual ReadResidual(
        FileChangeReceipt receipt,
        RouteUpdateEffectOutcome outcome,
        RouteUpdateVerificationState verification)
    {
        if (outcome == RouteUpdateEffectOutcome.Verified)
        {
            return verification == RouteUpdateVerificationState.Verified
                ? RouteUpdateEffectResidual.None
                : RouteUpdateEffectResidual.Retained;
        }

        return outcome switch
        {
            RouteUpdateEffectOutcome.VerificationFailed => receipt.After is null
                ? RouteUpdateEffectResidual.Unknown
                : RouteUpdateEffectResidual.Retained,
            RouteUpdateEffectOutcome.CompletionUnknown => RouteUpdateEffectResidual.Unknown,
            RouteUpdateEffectOutcome.NotStarted => RouteUpdateEffectResidual.None,
            RouteUpdateEffectOutcome.Planned => throw new ArgumentOutOfRangeException(
                nameof(outcome),
                outcome,
                "An application receipt cannot preserve one planned Route Update effect."),
            RouteUpdateEffectOutcome.Verified => throw new InvalidOperationException(
                "Verified Route Update residuals are handled before the exhaustive mapping."),
            _ => throw new ArgumentOutOfRangeException(
                nameof(outcome),
                outcome,
                "The Route Update effect outcome is not defined."),
        };
    }

    private static bool PathEquals(
        RouteUpdatePlan plan,
        string absolutePath,
        string canonicalPath)
        => string.Equals(
            Path.GetRelativePath(plan.Request.Workspace.LexicalRoot, absolutePath)
                .Replace(Path.DirectorySeparatorChar, '/')
                .Replace(Path.AltDirectorySeparatorChar, '/'),
            canonicalPath,
            OperatingSystem.IsWindows()
                ? StringComparison.OrdinalIgnoreCase
                : StringComparison.Ordinal);
}
