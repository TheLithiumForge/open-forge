namespace OpenForge.Cli.Core.Framework.Tools.Models.Registration;

internal sealed record ToolEntryRewriteRequest
{
    public required string WorkspaceRoot { get; init; }
    public required ToolEntryRewrite[] Rewrites { get; init; }
    public required ToolOptionalComponent[] DeclinedComponents { get; init; }
    public required IReadOnlyDictionary<string, string> Values { get; init; }
    public IReadOnlyDictionary<string, byte[]?>? BeforeInstallerFiles { get; init; }
    public ToolHookGroupReceipt[] CreatedHookGroups { get; init; } = [];
}

internal enum ToolEntryState { Original, Matching, Declined, Missing, Drifted, Unavailable }
internal sealed record ToolEntryCheck(string Id, string Host, string File, ToolEntryState State, string? Cause);
internal sealed record ToolEntryEdit(string File, byte[] Before, byte[] After, string[] EntryIds);
internal sealed record ToolEntryPlan(ToolEntryEdit[] Edits, ToolEntryCheck[] Checks);

internal delegate ValueTask<ToolEntryPlan> ToolPlanEntryRewrites(ToolEntryRewriteRequest request, CancellationToken token);
internal delegate ValueTask<ToolEntryCheck[]> ToolObserveEntries(ToolEntryRewriteRequest request, CancellationToken token);
internal sealed record ToolEntryServices(ToolPlanEntryRewrites Plan, ToolObserveEntries Observe);
