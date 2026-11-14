namespace OpenForge.Cli.Composition.Shared.ToolHomes.Models;

internal sealed record ToolClientRootRecipe(string Key, bool Directory, Func<ToolClientRootResolver, string?> Resolve);
