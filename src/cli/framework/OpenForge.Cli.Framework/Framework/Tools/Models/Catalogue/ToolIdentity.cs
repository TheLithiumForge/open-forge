namespace OpenForge.Cli.Core.Framework.Tools.Models.Catalogue;

internal enum ToolOrigin { FirstParty, ThirdParty }

internal sealed record ToolIdentity
{
    public required string Id { get; init; }
    public required string Name { get; init; }
    public required ToolOrigin Origin { get; init; }
    public required Uri Upstream { get; init; }
    public required string Licence { get; init; }
}
