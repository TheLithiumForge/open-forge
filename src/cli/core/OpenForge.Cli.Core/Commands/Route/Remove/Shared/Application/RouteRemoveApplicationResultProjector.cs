using OpenForge.Cli.Core.Commands.Route.Remove.Models.Operation;
using OpenForge.Cli.Core.Commands.Route.Remove.Models.Planning;
using OpenForge.Cli.Core.Commands.Route.Remove.Models.Result;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem;

namespace OpenForge.Cli.Core.Commands.Route.Remove.Shared.Application;

internal static class RouteRemoveApplicationResultProjector
{
    internal static RouteRemoveResultFormation Project(
        RouteRemovePlan plan,
        RouteRemoveApplicationProgress progress)
        => plan.Preview with
        {
            Effects = [.. plan.Preview.Effects.Select(effect => ProjectEffect(plan, progress, effect))],
            Recovery = progress.Recovery,
            Verification = progress.Verification,
            Findings = [.. plan.Preview.Findings, .. progress.Findings],
        };

    private static RouteRemoveEffect ProjectEffect(
        RouteRemovePlan plan,
        RouteRemoveApplicationProgress progress,
        RouteRemoveEffect effect)
    {
        var receipt = progress.Receipts.SingleOrDefault(candidate =>
            PathEquals(plan, ReceiptPath(candidate), effect.Path));
        if (receipt is null)
        {
            var unknown = progress.Verification == RouteRemoveVerificationState.Unknown;
            return effect with
            {
                Outcome = unknown
                    ? RouteRemoveEffectOutcome.CompletionUnknown
                    : RouteRemoveEffectOutcome.NotStarted,
                Residual = unknown
                    ? RouteRemoveEffectResidual.Unknown
                    : RouteRemoveEffectResidual.None,
            };
        }

        var state = ReceiptState(receipt);
        var outcome = ReadOutcome(state.Effect, state.Verification);
        return effect with
        {
            Outcome = outcome,
            Residual = ReadResidual(outcome, state.After, progress.Verification),
        };
    }

    private static string ReceiptPath(RouteRemoveApplicationReceipt receipt)
        => receipt switch
        {
            RouteRemoveFileChangeReceipt change => change.Receipt.Change.LogicalPath,
            RouteRemoveDirectoryDeletionReceipt deletion => deletion.Receipt.Deletion.LogicalPath,
            _ => throw new ArgumentOutOfRangeException(
                nameof(receipt), receipt, "The Route Remove receipt kind is not defined."),
        };

    private static (FilesystemEffectState Effect,
        FilesystemVerificationState Verification,
        FileStateSnapshot? After) ReceiptState(RouteRemoveApplicationReceipt receipt)
        => receipt switch
        {
            RouteRemoveFileChangeReceipt change => (
                change.Receipt.EffectState,
                change.Receipt.VerificationState,
                change.Receipt.After),
            RouteRemoveDirectoryDeletionReceipt deletion => (
                deletion.Receipt.State.EffectState,
                deletion.Receipt.State.VerificationState,
                deletion.Receipt.After),
            _ => throw new ArgumentOutOfRangeException(
                nameof(receipt), receipt, "The Route Remove receipt kind is not defined."),
        };

    private static RouteRemoveEffectOutcome ReadOutcome(
        FilesystemEffectState effect,
        FilesystemVerificationState verification)
        => (effect, verification) switch
        {
            (FilesystemEffectState.Applied, FilesystemVerificationState.Verified) =>
                RouteRemoveEffectOutcome.Verified,
            (FilesystemEffectState.Applied, FilesystemVerificationState.Failed) =>
                RouteRemoveEffectOutcome.VerificationFailed,
            (FilesystemEffectState.Unknown, _) => RouteRemoveEffectOutcome.CompletionUnknown,
            (FilesystemEffectState.NotStarted, FilesystemVerificationState.NotStarted) =>
                RouteRemoveEffectOutcome.NotStarted,
            _ => throw new InvalidOperationException(
                "The Route Remove filesystem receipt is incoherent."),
        };

    private static RouteRemoveEffectResidual ReadResidual(
        RouteRemoveEffectOutcome outcome,
        FileStateSnapshot? after,
        RouteRemoveVerificationState verification)
        => outcome switch
        {
            RouteRemoveEffectOutcome.Verified => verification == RouteRemoveVerificationState.Verified
                ? RouteRemoveEffectResidual.None
                : RouteRemoveEffectResidual.Retained,
            RouteRemoveEffectOutcome.VerificationFailed => after is null
                ? RouteRemoveEffectResidual.Unknown
                : RouteRemoveEffectResidual.Retained,
            RouteRemoveEffectOutcome.CompletionUnknown => RouteRemoveEffectResidual.Unknown,
            RouteRemoveEffectOutcome.NotStarted => RouteRemoveEffectResidual.None,
            RouteRemoveEffectOutcome.Planned => throw new ArgumentOutOfRangeException(
                nameof(outcome), outcome, "An application cannot preserve a planned effect."),
            _ => throw new ArgumentOutOfRangeException(
                nameof(outcome), outcome, "The Route Remove effect outcome is not defined."),
        };

    private static bool PathEquals(
        RouteRemovePlan plan,
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
