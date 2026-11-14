using System.Text.Json;
using OpenForge.Cli.Core.Commands.Tool.Shared.Models.Provider;

namespace OpenForge.Cli.Core.Commands.Tool.Shared.Models.Writing;

internal sealed record ToolWriteData
{
    public required ToolWriteMode Mode { get; init; }
    public required string? Id { get; init; }
    public required string? Role { get; init; }
    public required string? Pin { get; init; }
    public ToolPlannedChange[] Plan { get; init; } = [];
    public ToolDestinationDecision[] Permissions { get; init; } = [];
    public ToolConsent[] Consents { get; init; } = [];
    public ToolVerification[] Verification { get; init; } = [];
    public ToolRemainingWork[] Remaining { get; init; } = [];
    public ToolInstruction[] Instructions { get; init; } = [];
    public ToolAppliedChange[] Effects { get; init; } = [];
    public ToolWriteBackupMode? BackupMode { get; init; }
}

internal sealed record ToolPlannedChange(string Kind, string Subject, string Action, JsonElement? Before, JsonElement? After, string[] Requires, string[] Verification);
internal sealed record ToolDestinationDecision(string Path, bool Allowed, bool SaveGrant, string? Cause);
internal sealed record ToolConsent(string Kind, string Subject, ToolConsentDecision Decision, string Scope)
{
    public ToolArtifactConsent? Artifact { get; init; }
    public string? ScopeSha256 { get; init; }
}
internal sealed record ToolVerification(string Check, string Subject, ToolVerificationState State, string? Cause);
internal sealed record ToolRemainingWork(string Subject, string Action, string Cause);
internal sealed record ToolAppliedChange(string Kind, string Subject, string Action, JsonElement? Before, JsonElement? After);
internal sealed record ToolArtifactConsent(Uri Url, long Bytes, string Integrity);
