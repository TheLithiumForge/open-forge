using System.Text.Json.Serialization;
using OpenForge.Cli.Core.Framework.Tools.Models.Supply;
using OpenForge.Cli.Core.Framework.Tools.Models.Configuration;
using OpenForge.Cli.Core.Framework.Tools.Models.Lifecycle;
using OpenForge.Cli.Core.Framework.Tools.Models.Execution;

using OpenForge.Cli.Core.Framework.Tools.Models.Registration;

namespace OpenForge.Cli.Core.Framework.Tools.Models.Manifest;

[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
internal sealed record ToolManifest
{
    public required int Schema { get; init; }

    public required string Id { get; init; }

    public required string Name { get; init; }

    public required string Version { get; init; }

    public required string License { get; init; }

    public required string Origin { get; init; }

    public required ToolSource Source { get; init; }

    public required ToolAdapter Adapter { get; init; }

    public required string Admission { get; init; }

    public required string? AdmissionReason { get; init; }

    public required string[] Capabilities { get; init; }

    public required string DownloadBase { get; init; }

    public required Dictionary<string, ToolArtifact> Artifacts { get; init; }

    public required Dictionary<string, ToolTarget> Targets { get; init; }

    public required ToolRuntime Runtime { get; init; }

    public required ToolConfiguration Configuration { get; init; }

    public required ToolProcesses Processes { get; init; }

    public required ToolIndex Index { get; init; }

    public required ToolMcp Mcp { get; init; }

    public required ToolSkill? Skill { get; init; }

    public required string[] Checks { get; init; }

    public required ToolQualification Qualification { get; init; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public ToolLifecycle? Lifecycle { get; init; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public ToolRegistration? Registration { get; init; }
}
