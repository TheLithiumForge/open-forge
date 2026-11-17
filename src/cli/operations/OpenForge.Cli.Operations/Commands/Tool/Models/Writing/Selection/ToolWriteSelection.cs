using OpenForge.Cli.Core.Framework.Tools.Models.Catalogue;
using OpenForge.Cli.Core.Framework.Tools.Models.Records;

namespace OpenForge.Cli.Core.Commands.Tool.Models.Writing.Selection;

internal sealed record ToolWriteSelection
{
    public required ToolCatalogueEntry Entry { get; init; }
    public required ToolVersionSelection Version { get; init; }
    public string? RequiredChannel { get; init; }
    public ToolSetting? Declaration { get; init; }
    public ToolLockRecord? Lock { get; init; }
}
