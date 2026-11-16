using OpenForge.Cli.Core.Framework.Tools.Models.Records;

namespace OpenForge.Cli.Core.Commands.Tool.Models.Writing.Application;

internal sealed record ToolWriteAuxiliaryPublication
{
    public ToolPathState? Path { get; init; }
    public IReadOnlyList<string> RemovedToolIds { get; init; } = [];
    public IReadOnlyList<string> RemovedRuntimeVersions { get; init; } = [];
}
