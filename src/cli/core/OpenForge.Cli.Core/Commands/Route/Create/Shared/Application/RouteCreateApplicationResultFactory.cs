using OpenForge.Cli.Core.Commands.Route.Create.Models.Operation;
using OpenForge.Cli.Core.Commands.Route.Create.Models.Planning;
using OpenForge.Cli.Core.Commands.Route.Create.Models.Result;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem;

namespace OpenForge.Cli.Core.Commands.Route.Create.Shared.Application;

internal static class RouteCreateApplicationResultFactory
{
    internal static RouteCreateResultFormation Build(
        RouteCreatePlan plan,
        RouteCreateApplicationProgress progress)
    {
        var preview = plan.Preview;
        return preview with
        {
            Effects = [.. preview.Effects.Select(effect => Apply(effect, plan, progress))],
            Recovery = progress.Recovery,
            Verification = progress.Verification,
            Findings = [.. preview.Findings.Concat(progress.Findings)],
        };
    }

    private static RouteCreateEffect Apply(
        RouteCreateEffect effect,
        RouteCreatePlan plan,
        RouteCreateApplicationProgress progress)
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
        return effect with
        {
            Outcome = attempted
                ? RouteCreateEffectOutcome.CompletionUnknown
                : RouteCreateEffectOutcome.NotStarted,
            Residual = attempted
                ? RouteCreateEffectResidual.Unknown
                : RouteCreateEffectResidual.None,
        };
    }

    private static RouteCreateEffectOutcome ReadOutcome(FileChangeReceipt receipt)
        => (receipt.EffectState, receipt.VerificationState) switch
        {
            (FilesystemEffectState.Applied, FilesystemVerificationState.Verified) =>
                RouteCreateEffectOutcome.Verified,
            (FilesystemEffectState.Applied, FilesystemVerificationState.Failed) =>
                RouteCreateEffectOutcome.VerificationFailed,
            (FilesystemEffectState.Unknown, _) => RouteCreateEffectOutcome.CompletionUnknown,
            (FilesystemEffectState.NotStarted, FilesystemVerificationState.NotStarted) =>
                RouteCreateEffectOutcome.NotStarted,
            _ => throw new InvalidOperationException(
                "The Route Create filesystem receipt is incoherent."),
        };

    private static RouteCreateEffectResidual ReadResidual(
        FileChangeReceipt receipt,
        RouteCreateEffectOutcome outcome,
        RouteCreateVerificationState verification)
        => outcome switch
        {
            RouteCreateEffectOutcome.Verified => verification == RouteCreateVerificationState.Verified
                ? RouteCreateEffectResidual.None
                : RouteCreateEffectResidual.Retained,
            RouteCreateEffectOutcome.VerificationFailed => receipt.After is null
                ? RouteCreateEffectResidual.Unknown
                : RouteCreateEffectResidual.Retained,
            RouteCreateEffectOutcome.CompletionUnknown => RouteCreateEffectResidual.Unknown,
            RouteCreateEffectOutcome.NotStarted => RouteCreateEffectResidual.None,
            RouteCreateEffectOutcome.Planned => throw new ArgumentOutOfRangeException(
                nameof(outcome),
                outcome,
                "A receipt cannot preserve a planned Route Create effect."),
            _ => throw new ArgumentOutOfRangeException(
                nameof(outcome),
                outcome,
                "The Route Create effect outcome is not defined."),
        };

    private static bool PathEquals(
        RouteCreatePlan plan,
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
