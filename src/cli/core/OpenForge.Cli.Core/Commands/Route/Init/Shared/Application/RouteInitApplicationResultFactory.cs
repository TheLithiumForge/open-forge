using OpenForge.Cli.Core.Commands.Route.Init.Models.Operation;
using OpenForge.Cli.Core.Commands.Route.Init.Models.Planning;
using OpenForge.Cli.Core.Commands.Route.Init.Models.Result;
using OpenForge.Cli.Core.Framework.Lifecycle;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Directories;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Files;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Receipts;

namespace OpenForge.Cli.Core.Commands.Route.Init.Shared.Application;

internal static class RouteInitApplicationResultFactory
{
    internal static RouteInitApplicationOutcome Build(
        RouteInitPlan plan,
        IReadOnlyList<DirectoryCreationReceipt> directoryReceipts,
        IReadOnlyList<FileChangeReceipt> fileReceipts,
        RouteInitRecovery recovery,
        RouteInitVerificationState verification,
        RouteInitFinding? finding,
        RouteInitApplicationAttempt? uncertainAttempt)
    {
        ArgumentNullException.ThrowIfNull(plan);
        ArgumentNullException.ThrowIfNull(directoryReceipts);
        ArgumentNullException.ThrowIfNull(fileReceipts);
        ArgumentNullException.ThrowIfNull(recovery);

        var preview = plan.Preview;
        var entrypoints = preview.Entrypoints
            .Select(entrypoint => Apply(entrypoint, plan, fileReceipts, uncertainAttempt))
            .ToArray();
        var effects = preview.Effects
            .Select(effect => Apply(
                effect,
                plan,
                directoryReceipts,
                fileReceipts,
                verification,
                uncertainAttempt))
            .ToArray();
        var lifecycle = ApplyLifecycle(preview.Lifecycle, plan, fileReceipts, uncertainAttempt);
        var findings = new List<RouteInitFinding>();
        if (entrypoints.Any(entrypoint => entrypoint.Outcome == RouteInitEntrypointOutcome.Created))
        {
            findings.AddRange(preview.Findings.Where(candidate =>
                candidate.Code == RouteInitFindingCode.NeedsAuthoring));
        }

        if (finding is not null)
        {
            findings.Add(finding);
        }

        return new RouteInitApplicationOutcome(new RouteInitResultFormation(
            preview.Workspace,
            preview.Mode,
            preview.Scaffold,
            preview.Target,
            preview.Plan,
            preview.Framework,
            entrypoints,
            effects,
            preview.UnchangedPaths,
            lifecycle,
            recovery,
            verification,
            findings));
    }

    internal static RouteInitFinding Finding(
        RouteInitPlan plan,
        RouteInitFindingCode code,
        string cause,
        string? target = null)
        => new(
            code,
            cause,
            target ?? plan.Preview.Target.Id ?? plan.Request.RouteTarget);

    private static RouteInitEntrypoint Apply(
        RouteInitEntrypoint entrypoint,
        RouteInitPlan plan,
        IReadOnlyList<FileChangeReceipt> receipts,
        RouteInitApplicationAttempt? uncertainAttempt)
    {
        if (entrypoint.Outcome != RouteInitEntrypointOutcome.Planned)
        {
            return entrypoint;
        }

        var receipt = FindReceipt(plan, receipts, entrypoint.Path);
        var outcome = receipt is null
            ? IsAttempt(
                plan,
                uncertainAttempt,
                RouteInitApplicationAttemptKind.File,
                entrypoint.Path)
                ? RouteInitEntrypointOutcome.CompletionUnknown
                : RouteInitEntrypointOutcome.NotStarted
            : ReadEntrypointOutcome(receipt);
        return entrypoint with { Outcome = outcome };
    }

    private static RouteInitEffect Apply(
        RouteInitEffect effect,
        RouteInitPlan plan,
        IReadOnlyList<DirectoryCreationReceipt> directoryReceipts,
        IReadOnlyList<FileChangeReceipt> fileReceipts,
        RouteInitVerificationState verification,
        RouteInitApplicationAttempt? uncertainAttempt)
    {
        if (effect.Kind == RouteInitEffectKind.Directory)
        {
            var receipt = directoryReceipts.FirstOrDefault(candidate =>
                PathEquals(plan, candidate.Creation.LogicalPath, effect.Path));
            if (receipt is null)
            {
                var attempted = IsAttempt(
                    plan,
                    uncertainAttempt,
                    RouteInitApplicationAttemptKind.Directory,
                    effect.Path);
                return effect with
                {
                    Outcome = attempted
                        ? RouteInitEffectOutcome.CompletionUnknown
                        : RouteInitEffectOutcome.NotStarted,
                    Residual = attempted
                        ? RouteInitEffectResidual.Unknown
                        : RouteInitEffectResidual.None,
                };
            }

            var outcome = ReadEffectOutcome(receipt);
            return effect with
            {
                Outcome = outcome,
                Residual = ReadResidual(receipt, outcome, verification),
            };
        }

        var fileReceipt = FindReceipt(plan, fileReceipts, effect.Path);
        if (fileReceipt is null)
        {
            var attempted = IsAttempt(
                plan,
                uncertainAttempt,
                RouteInitApplicationAttemptKind.File,
                effect.Path);
            return effect with
            {
                Outcome = attempted
                    ? RouteInitEffectOutcome.CompletionUnknown
                    : RouteInitEffectOutcome.NotStarted,
                Residual = attempted
                    ? RouteInitEffectResidual.Unknown
                    : RouteInitEffectResidual.None,
            };
        }

        var fileOutcome = ReadEffectOutcome(fileReceipt);
        return effect with
        {
            Outcome = fileOutcome,
            Residual = ReadResidual(fileReceipt, fileOutcome, verification),
        };
    }

    private static RouteInitLifecycle ApplyLifecycle(
        RouteInitLifecycle lifecycle,
        RouteInitPlan plan,
        IReadOnlyList<FileChangeReceipt> receipts,
        RouteInitApplicationAttempt? uncertainAttempt)
    {
        if (lifecycle.Action != RouteInitLifecycleAction.Publish)
        {
            return lifecycle;
        }

        var receipt = receipts.FirstOrDefault(candidate => IsLifecyclePath(plan, candidate.Change.LogicalPath));
        var outcome = receipt is null
            ? uncertainAttempt is
            {
                Kind: RouteInitApplicationAttemptKind.File,
            } attempt
                && IsLifecyclePath(plan, attempt.LogicalPath)
                    ? RouteInitLifecycleOutcome.CompletionUnknown
                    : RouteInitLifecycleOutcome.NotStarted
            : ReadLifecycleOutcome(receipt);
        return lifecycle with { Outcome = outcome };
    }

    private static FileChangeReceipt? FindReceipt(
        RouteInitPlan plan,
        IReadOnlyList<FileChangeReceipt> receipts,
        string canonicalPath)
        => receipts.FirstOrDefault(candidate => PathEquals(plan, candidate.Change.LogicalPath, canonicalPath));

    private static bool PathEquals(
        RouteInitPlan plan,
        string logicalPath,
        string canonicalPath)
        => string.Equals(
            Relative(plan, logicalPath),
            canonicalPath,
            OperatingSystem.IsWindows() ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal);

    private static bool IsLifecyclePath(RouteInitPlan plan, string logicalPath)
        => PathEquals(plan, logicalPath, LifecycleSchema.RelativePath);

    private static bool IsAttempt(
        RouteInitPlan plan,
        RouteInitApplicationAttempt? attempt,
        RouteInitApplicationAttemptKind kind,
        string canonicalPath)
        => attempt is not null
            && attempt.Kind == kind
            && PathEquals(plan, attempt.LogicalPath, canonicalPath);

    private static string Relative(RouteInitPlan plan, string logicalPath)
        => Path.GetRelativePath(plan.Request.Workspace.LexicalRoot, logicalPath)
            .Replace(Path.DirectorySeparatorChar, '/')
            .Replace(Path.AltDirectorySeparatorChar, '/');

    private static RouteInitEntrypointOutcome ReadEntrypointOutcome(FileChangeReceipt receipt)
        => (receipt.EffectState, receipt.VerificationState) switch
        {
            (FilesystemEffectState.Applied, FilesystemVerificationState.Verified) => RouteInitEntrypointOutcome.Created,
            (FilesystemEffectState.Applied, FilesystemVerificationState.Failed) => RouteInitEntrypointOutcome.VerificationFailed,
            (FilesystemEffectState.Unknown, _) => RouteInitEntrypointOutcome.CompletionUnknown,
            (FilesystemEffectState.NotStarted, FilesystemVerificationState.NotStarted) => RouteInitEntrypointOutcome.NotStarted,
            _ => throw new InvalidOperationException("The Route Init entrypoint receipt is incoherent."),
        };

    private static RouteInitEffectOutcome ReadEffectOutcome(DirectoryCreationReceipt receipt)
        => ReadEffectOutcome(receipt.EffectState, receipt.VerificationState);

    private static RouteInitEffectOutcome ReadEffectOutcome(FileChangeReceipt receipt)
        => ReadEffectOutcome(receipt.EffectState, receipt.VerificationState);

    private static RouteInitEffectOutcome ReadEffectOutcome(
        FilesystemEffectState effect,
        FilesystemVerificationState verification)
        => (effect, verification) switch
        {
            (FilesystemEffectState.Applied, FilesystemVerificationState.Verified) => RouteInitEffectOutcome.Verified,
            (FilesystemEffectState.Applied, FilesystemVerificationState.Failed) => RouteInitEffectOutcome.VerificationFailed,
            (FilesystemEffectState.Unknown, _) => RouteInitEffectOutcome.CompletionUnknown,
            (FilesystemEffectState.NotStarted, FilesystemVerificationState.NotStarted) => RouteInitEffectOutcome.NotStarted,
            _ => throw new InvalidOperationException("The Route Init filesystem receipt is incoherent."),
        };

    internal static RouteInitEffectResidual ReadResidual(
        DirectoryCreationReceipt receipt,
        RouteInitEffectOutcome outcome,
        RouteInitVerificationState verification)
    {
        ValidateVerificationState(verification);
        return outcome switch
        {
            RouteInitEffectOutcome.Planned
                or RouteInitEffectOutcome.NotStarted => RouteInitEffectResidual.None,
            RouteInitEffectOutcome.Verified => verification == RouteInitVerificationState.Verified
                ? RouteInitEffectResidual.None
                : RouteInitEffectResidual.Retained,
            RouteInitEffectOutcome.VerificationFailed => receipt.After is null
                ? RouteInitEffectResidual.Unknown
                : RouteInitEffectResidual.Retained,
            RouteInitEffectOutcome.CompletionUnknown => RouteInitEffectResidual.Unknown,
            _ => throw new ArgumentOutOfRangeException(
                nameof(outcome),
                outcome,
                "The Route Init effect outcome is not defined."),
        };
    }

    internal static RouteInitEffectResidual ReadResidual(
        FileChangeReceipt receipt,
        RouteInitEffectOutcome outcome,
        RouteInitVerificationState verification)
    {
        ValidateVerificationState(verification);
        return outcome switch
        {
            RouteInitEffectOutcome.Planned
                or RouteInitEffectOutcome.NotStarted => RouteInitEffectResidual.None,
            RouteInitEffectOutcome.Verified => verification == RouteInitVerificationState.Verified
                ? RouteInitEffectResidual.None
                : RouteInitEffectResidual.Retained,
            RouteInitEffectOutcome.VerificationFailed => receipt.After is null
                ? RouteInitEffectResidual.Unknown
                : RouteInitEffectResidual.Retained,
            RouteInitEffectOutcome.CompletionUnknown => RouteInitEffectResidual.Unknown,
            _ => throw new ArgumentOutOfRangeException(
                nameof(outcome),
                outcome,
                "The Route Init effect outcome is not defined."),
        };
    }

    private static void ValidateVerificationState(RouteInitVerificationState verification)
    {
        _ = verification switch
        {
            RouteInitVerificationState.NotRequested
                or RouteInitVerificationState.Verified
                or RouteInitVerificationState.Failed
                or RouteInitVerificationState.Unknown => true,
            _ => throw new ArgumentOutOfRangeException(
                nameof(verification),
                verification,
                "The Route Init verification state is not defined."),
        };
    }

    private static RouteInitLifecycleOutcome ReadLifecycleOutcome(FileChangeReceipt receipt)
        => (receipt.EffectState, receipt.VerificationState) switch
        {
            (FilesystemEffectState.Applied, FilesystemVerificationState.Verified) => RouteInitLifecycleOutcome.Verified,
            (FilesystemEffectState.Applied, FilesystemVerificationState.Failed) => RouteInitLifecycleOutcome.VerificationFailed,
            (FilesystemEffectState.Unknown, _) => RouteInitLifecycleOutcome.CompletionUnknown,
            (FilesystemEffectState.NotStarted, FilesystemVerificationState.NotStarted) => RouteInitLifecycleOutcome.NotStarted,
            _ => throw new InvalidOperationException("The Route Init lifecycle receipt is incoherent."),
        };
}
