using System.Text.Json.Serialization;

namespace OpenForge.Cli.Core.Framework.Tools.Models.Execution;

[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
internal sealed record ToolQualification
{
    public required string Attestation { get; init; }

    public required string? Workflow { get; init; }

    public required string[] Targets { get; init; }

    public required string Accuracy { get; init; }
}
