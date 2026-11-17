namespace OpenForge.Cli.Core.Framework.Tools.Models.Configuration;

internal sealed record ToolManagedFileChange : ToolConfigChange
{
    public required string PathTemplate { get; init; }
    public required string TemplateResource { get; init; }
}
