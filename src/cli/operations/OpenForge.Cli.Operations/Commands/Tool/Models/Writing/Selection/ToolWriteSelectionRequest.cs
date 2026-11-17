using OpenForge.Cli.Core.Framework.Tools.Models.Catalogue;
using OpenForge.Cli.Core.Framework.Tools.Models.Records;

namespace OpenForge.Cli.Core.Commands.Tool.Models.Writing.Selection;

internal enum ToolWriteSelectionKind { Install, Update }

internal sealed record ToolWriteSelectionRequest
{
    public required ToolWriteSelectionKind Kind { get; init; }
    public ToolSpec? Spec { get; init; }
    public string? ChannelOverride { get; init; }
    public required ToolPortableRecords Records { get; init; }
}
