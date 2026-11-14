using System.Text.Json;
using System.Text.Json.Serialization;
using OpenForge.Cli.Core.Commands.Tool.Choose.Models;
using OpenForge.Cli.Core.Commands.Tool.Configure.Models;
using OpenForge.Cli.Core.Commands.Tool.Install.Models;
using OpenForge.Cli.Core.Commands.Tool.Models.Provider;
using OpenForge.Cli.Core.Commands.Tool.Remove.Models;
using OpenForge.Cli.Core.Commands.Tool.Shared.Reporting;
using OpenForge.Cli.Core.Commands.Tool.Update.Models;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.Core.Commands.Tool.Models.Writing;

internal sealed record ToolWriteReport(ToolWriteReportData Data, ToolWriteReportIssue[] Issues)
{
    internal static ToolWriteReport From(ToolInstallResult result) => ToolWriteReportProjection.Read(result);
    internal static ToolWriteReport From(ToolConfigureResult result) => ToolWriteReportProjection.Read(result);
    internal static ToolWriteReport From(ToolChooseResult result) => ToolWriteReportProjection.Read(result);
    internal static ToolWriteReport From(ToolUpdateResult result) => ToolWriteReportProjection.Read(result);
    internal static ToolWriteReport From(ToolRemoveResult result) => ToolWriteReportProjection.Read(result);
}

internal sealed record ToolWriteReportData
{
    public required string Mode { get; init; }
    public required string? Id { get; init; }
    public required string? Role { get; init; }
    public required string? Pin { get; init; }
    public required ToolReportPlannedChange[] Plan { get; init; }
    public required ToolReportDestination[] Permissions { get; init; }
    public required ToolReportConsent[] Consents { get; init; }
    public required ToolReportVerification[] Verification { get; init; }
    public required ToolReportRemaining[] Remaining { get; init; }
    public required ToolInstruction[] Instructions { get; init; }
    public required ToolReportAppliedChange[] Effects { get; init; }
    public required string? BackupMode { get; init; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public ToolReportInstallItem[]? Tools { get; init; }
}

internal sealed record ToolReportPlannedChange(string Kind, string Subject, string Action, JsonElement? Before, JsonElement? After, string[] Requires, string[] Verification);
internal sealed record ToolReportDestination(string Path, bool Allowed, bool SaveGrant, string? Cause);
internal sealed record ToolReportConsent(string Kind, string Subject, string Decision, string Scope, ToolReportArtifact? Artifact, string? ScopeSha256);
internal sealed record ToolReportArtifact(Uri Url, long Bytes, string Integrity);
internal sealed record ToolReportVerification(string Check, string Subject, string State, string? Cause);
internal sealed record ToolReportRemaining(string Subject, string Action, string Cause);
internal sealed record ToolReportAppliedChange(string Kind, string Subject, string Action, JsonElement? Before, JsonElement? After);
internal sealed record ToolReportInstallItem(string Id, string Status, ToolWriteReportData Data, ToolWriteReportIssue[] Issues);
internal sealed record ToolWriteReportIssue(string Code, [property: JsonIgnore] CliSemanticStatus SemanticStatus, string Subject, string Cause)
{
    public string Status => CliStatusDefinitions.Read(SemanticStatus).MachineName;
}
