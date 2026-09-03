using OpenForge.Cli.Core.Commands.Route.Move.Models.Operation;
using OpenForge.Cli.Core.Commands.Route.Move.Models.Planning;
using OpenForge.Cli.Core.Commands.Route.Move.Models.Result;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem;

namespace OpenForge.Cli.Core.Commands.Route.Move.Shared.Application;

internal static class RouteMoveApplicationResultProjector
{
    internal static RouteMoveResultFormation Project(
        RouteMovePlan plan,
        RouteMoveApplicationProgress progress)
        => plan.Preview with
        {
            Effects = [.. plan.Preview.Effects.Select(effect => ProjectEffect(plan, progress, effect))],
            Recovery = progress.Recovery,
            Verification = progress.Verification,
            Findings = [.. plan.Preview.Findings, .. progress.Findings],
        };

    private static RouteMoveEffect ProjectEffect(
        RouteMovePlan plan,
        RouteMoveApplicationProgress progress,
        RouteMoveEffect effect)
    {
        var receipt = progress.Receipts.SingleOrDefault(candidate =>
            PathEquals(plan, ReceiptPath(candidate), effect.Path));
        if (receipt is null)
        {
            var unknown = progress.Verification == RouteMoveVerificationState.Unknown;
            return effect with
            {
                Outcome = unknown
                    ? RouteMoveEffectOutcome.CompletionUnknown
                    : RouteMoveEffectOutcome.NotStarted,
                Residual = unknown
                    ? RouteMoveEffectResidual.Unknown
                    : RouteMoveEffectResidual.None,
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

    private static string ReceiptPath(RouteMoveApplicationReceipt receipt)
        => receipt switch
        {
            RouteMoveDirectoryCreationReceipt creation => creation.Receipt.Creation.LogicalPath,
            RouteMoveFileChangeReceipt change => change.Receipt.Change.LogicalPath,
            RouteMoveDirectoryDeletionReceipt deletion => deletion.Receipt.Deletion.LogicalPath,
            _ => throw new ArgumentOutOfRangeException(
                nameof(receipt), receipt, "The Route Move receipt kind is not defined."),
        };

    private static (FilesystemEffectState Effect,
        FilesystemVerificationState Verification,
        FileStateSnapshot? After) ReceiptState(RouteMoveApplicationReceipt receipt)
        => receipt switch
        {
            RouteMoveDirectoryCreationReceipt creation => (
                creation.Receipt.EffectState,
                creation.Receipt.VerificationState,
                creation.Receipt.After),
            RouteMoveFileChangeReceipt change => (
                change.Receipt.EffectState,
                change.Receipt.VerificationState,
                change.Receipt.After),
            RouteMoveDirectoryDeletionReceipt deletion => (
                deletion.Receipt.State.EffectState,
                deletion.Receipt.State.VerificationState,
                deletion.Receipt.After),
            _ => throw new ArgumentOutOfRangeException(
                nameof(receipt), receipt, "The Route Move receipt kind is not defined."),
        };

    private static RouteMoveEffectOutcome ReadOutcome(
        FilesystemEffectState effect,
        FilesystemVerificationState verification)
        => (effect, verification) switch
        {
            (FilesystemEffectState.Applied, FilesystemVerificationState.Verified) =>
                RouteMoveEffectOutcome.Verified,
            (FilesystemEffectState.Applied, FilesystemVerificationState.Failed) =>
                RouteMoveEffectOutcome.VerificationFailed,
            (FilesystemEffectState.Unknown, _) => RouteMoveEffectOutcome.CompletionUnknown,
            (FilesystemEffectState.NotStarted, FilesystemVerificationState.NotStarted) =>
                RouteMoveEffectOutcome.NotStarted,
            _ => throw new InvalidOperationException(
                "The Route Move filesystem receipt is incoherent."),
        };

    private static RouteMoveEffectResidual ReadResidual(
        RouteMoveEffectOutcome outcome,
        FileStateSnapshot? after,
        RouteMoveVerificationState verification)
        => outcome switch
        {
            RouteMoveEffectOutcome.Verified => verification == RouteMoveVerificationState.Verified
                ? RouteMoveEffectResidual.None
                : RouteMoveEffectResidual.Retained,
            RouteMoveEffectOutcome.VerificationFailed => after is null
                ? RouteMoveEffectResidual.Unknown
                : RouteMoveEffectResidual.Retained,
            RouteMoveEffectOutcome.CompletionUnknown => RouteMoveEffectResidual.Unknown,
            RouteMoveEffectOutcome.NotStarted => RouteMoveEffectResidual.None,
            RouteMoveEffectOutcome.Planned => throw new ArgumentOutOfRangeException(
                nameof(outcome), outcome, "An application cannot preserve a planned effect."),
            _ => throw new ArgumentOutOfRangeException(
                nameof(outcome), outcome, "The Route Move effect outcome is not defined."),
        };

    private static bool PathEquals(
        RouteMovePlan plan,
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
