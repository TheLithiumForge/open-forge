using OpenForge.Cli.Core.Framework.Tools.Models.Catalogue;

namespace OpenForge.Cli.Core.Framework.Tools.Models.Configuration;

internal enum ToolInitialConfigState { AbsentMember, JsonArrayOrMissing, UniqueMarkedBlock, AbsentFile, ReadableNativeValue }

internal abstract record ToolConfigChange
{
    public required string Id { get; init; }
    public required ToolEffectScope Scope { get; init; }
    public required ToolInitialConfigState InitialState { get; init; }
    public required string Description { get; init; }
}
