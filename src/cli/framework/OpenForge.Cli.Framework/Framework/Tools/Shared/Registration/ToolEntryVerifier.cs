using OpenForge.Cli.Core.Framework.Tools.Models.Registration;

namespace OpenForge.Cli.Core.Framework.Tools.Shared.Registration;

internal sealed class ToolEntryVerifier
{
    internal async ValueTask<ToolEntryCheck[]> RecheckAsync(ToolEntryRewriteRequest request, ToolEntryEdit edit, CancellationToken token)
    {
        var prior = new ToolEntryPlanner().Plan(request, new Dictionary<string, byte[]?> { [edit.File] = edit.Before });
        var checks = prior.Checks.Where(check => check.File == edit.File).ToArray();
        if (checks.Length == 0) throw new ArgumentException("The edit has no selected registration entries.", nameof(edit));
        try
        {
            var current = await ToolRegistrationFiles.ReadAsync(request.WorkspaceRoot, edit.File, token).ConfigureAwait(false);
            if (current is not null && current.AsSpan().SequenceEqual(edit.Before)) return checks;
            return [.. checks.Select(check => check with { State = ToolEntryState.Drifted, Cause = "The whole file changed after planning." })];
        }
        catch (Exception exception) when (ToolRegistrationFiles.IsReadFailure(exception))
        {
            return [.. checks.Select(check => check with { State = ToolEntryState.Unavailable, Cause = exception.Message })];
        }
    }

    internal async ValueTask<(ToolEntryCheck[] Checks, ToolRewriteReceipt[] Rewrites, ToolComponentDeclineReceipt[] DeclinedComponents)> VerifyAsync(
        ToolEntryRewriteRequest request, CancellationToken token)
        => Receipts(request, await new ToolEntryPlanner().ObserveAsync(request, token).ConfigureAwait(false));

    internal (ToolEntryCheck[] Checks, ToolRewriteReceipt[] Rewrites, ToolComponentDeclineReceipt[] DeclinedComponents) Verify(
        ToolEntryRewriteRequest request, IReadOnlyDictionary<string, byte[]?> files)
        => Receipts(request, new ToolEntryPlanner().Observe(request, files));

    internal ValueTask<ToolEntryCheck[]> ObserveReceiptsAsync(string workspaceRoot, ToolRewriteReceipt[] rewrites,
        ToolComponentDeclineReceipt[] declinedComponents, CancellationToken token)
        => new ToolEntryPlanner().ObserveAsync(new()
        {
            WorkspaceRoot = workspaceRoot,
            Rewrites = [.. rewrites.Where(receipt => !receipt.Declined).Select(receipt => receipt.Rewrite)],
            DeclinedComponents = [.. declinedComponents.Select(receipt => new ToolOptionalComponent
            {
                Id = receipt.Id, Flag = ToolComponentFlags.PromptHook, Description = receipt.Id, DefaultEnabled = false, DeclinedEntries = receipt.AbsentEntries,
            })],
            Values = new Dictionary<string, string>(StringComparer.Ordinal),
        }, token);

    private static (ToolEntryCheck[] Checks, ToolRewriteReceipt[] Rewrites, ToolComponentDeclineReceipt[] DeclinedComponents) Receipts(
        ToolEntryRewriteRequest request, ToolEntryCheck[] checks)
    {
        if (checks.Any(check => check.State is not (ToolEntryState.Matching or ToolEntryState.Declined))) return (checks, [], []);
        var resolved = ToolEntryDeclarations.Resolve(request);
        return (checks, [.. resolved.Rewrites.Select(rewrite => new ToolRewriteReceipt { Id = rewrite.Id, Rewrite = rewrite, Declined = false })],
            [.. resolved.DeclinedComponents.Select(component => new ToolComponentDeclineReceipt(component.Id, component.DeclinedEntries))]);
    }
}
