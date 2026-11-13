using OpenForge.Cli.Core.Framework.Tools.Models.Catalogue;
using OpenForge.Cli.Core.Framework.Tools.Models.Configuration;

namespace OpenForge.Cli.Core.Framework.Tools.Models.Records;

internal enum ToolEditVerification { Pending, Verified, Conflict, Failed }
internal enum ToolEditSource { OpenForge, UpstreamSetup }

internal sealed record ToolWorkspaceEditRecord
{
    public required string ChangeId { get; init; }
    public required string ProfileDigest { get; init; }
    public required ToolEffectScope Scope { get; init; }
    public required ToolEditSource Source { get; init; }
    public ToolConfigChange? Recipe { get; init; }
    public required string Target { get; init; }
    public string? Selector { get; init; }
    public required ToolRecordedValue Prior { get; init; }
    public required ToolRecordedValue Applied { get; init; }
    public required bool Borrowed { get; init; }
    public required ToolEditVerification Verification { get; init; }
    public IReadOnlyList<string>? RestoreArguments { get; init; }
}

internal sealed record ToolWorkspaceState
{
    public required int SchemaVersion { get; init; }
    public required string ToolId { get; init; }
    public required IReadOnlyList<ToolWorkspaceEditRecord> Edits { get; init; }
}
