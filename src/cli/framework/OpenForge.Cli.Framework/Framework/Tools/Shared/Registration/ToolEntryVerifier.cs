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
    {
        var checks = await new ToolEntryPlanner().ObserveAsync(request, token).ConfigureAwait(false);
        var files = new Dictionary<string, byte[]?>(StringComparer.Ordinal);
        foreach (var file in ToolEntryDeclarations.Resolve(request).Rewrites.OfType<ToolHookCommandRewrite>().Where(hook => hook.MatchCommand)
            .Select(hook => hook.File).Distinct(StringComparer.Ordinal))
        {
            try { files[file] = await ToolRegistrationFiles.ReadAsync(request.WorkspaceRoot, file, token).ConfigureAwait(false); }
            catch (Exception exception) when (ToolRegistrationFiles.IsReadFailure(exception))
            {
                checks = [.. checks.Select(check => check.File == file ? check with { State = ToolEntryState.Unavailable, Cause = exception.Message } : check)];
            }
        }
        return Receipts(request, checks, files);
    }

    internal (ToolEntryCheck[] Checks, ToolRewriteReceipt[] Rewrites, ToolComponentDeclineReceipt[] DeclinedComponents) Verify(
        ToolEntryRewriteRequest request, IReadOnlyDictionary<string, byte[]?> files)
        => Receipts(request, new ToolEntryPlanner().Observe(request, files), files);

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
        ToolEntryRewriteRequest request, ToolEntryCheck[] checks, IReadOnlyDictionary<string, byte[]?> files)
    {
        if (checks.Any(check => check.State is not (ToolEntryState.Matching or ToolEntryState.Declined))) return (checks, [], []);
        var resolved = ToolEntryDeclarations.Resolve(request);
        var receipts = new List<ToolRewriteReceipt>();
        foreach (var rewrite in resolved.Rewrites)
        {
            ToolHookGroupReceipt? group = null;
            if (rewrite is ToolHookCommandRewrite { MatchCommand: true } hook)
            {
                try
                {
                    if (!files.TryGetValue(hook.File, out var bytes) || bytes is null) throw new InvalidDataException("The hook file disappeared during verification.");
                    using var json = new ToolJsonEntries(bytes);
                    if (!json.Matches(hook)) throw new InvalidDataException("The hook changed during verification.");
                    var selected = ToolHookEntries.Find(json, hook.Pointer, hook.Replacement) ?? throw new InvalidDataException();
                    group = ToolHookEntries.CreatedGroup(resolved, file: hook.File, eventPointer: hook.Pointer, selected: selected, current: json);
                }
                catch (Exception exception) when (ToolRegistrationFiles.IsShapeFailure(exception))
                {
                    return ([.. checks.Select(check => check.Id == hook.Id ? check with { State = ToolEntryState.Drifted, Cause = exception.Message } : check)], [], []);
                }
            }
            receipts.Add(new() { Id = rewrite.Id, Rewrite = rewrite, Declined = false, CreatedHookGroup = group });
        }
        return (checks, [.. receipts],
            [.. resolved.DeclinedComponents.Select(component => new ToolComponentDeclineReceipt(component.Id, component.DeclinedEntries))]);
    }
}
