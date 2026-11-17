using OpenForge.Cli.Core.Framework.Tools.Models.Channels;
using OpenForge.Cli.Core.Framework.Tools.Models.Services;

namespace OpenForge.Cli.Core.Framework.Tools.Shared.Supply.Models;

internal sealed record ToolNpmPlanInput
{
    public required ToolChannelRequest Request { get; init; }
    public required ToolNpmChannel Channel { get; init; }
    public required string Version { get; init; }
    public required string Entry { get; init; }
    public ToolPackageManagerFacts? Manager { get; init; }
    public ToolPrivateNodeSpec? Node { get; init; }
    public required bool UsedNetwork { get; init; }
}
