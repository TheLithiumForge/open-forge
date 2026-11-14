using OpenForge.Cli.Core.Framework.Tools.Models.Catalogue;
using OpenForge.Cli.Core.Framework.Tools.Models.Launchers;

namespace OpenForge.Cli.Core.Framework.Tools.Models.Services;

internal sealed record ToolSetupRequest
{
    public required ToolSetupSpec Setup { get; init; }
    public required ToolCommandTarget Invocation { get; init; }
    public required string WorkspacePath { get; init; }
    public required bool DescribeOnly { get; init; }
}

internal sealed record ToolSetupResult
{
    public ToolProcessResult? Process { get; init; }
    public required bool Verified { get; init; }
    public required IReadOnlyList<string> Unfinished { get; init; }
}
