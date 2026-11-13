using OpenForge.Cli.Core.Framework.Tools.Models.Catalogue;
using OpenForge.Cli.Core.Framework.Tools.Models.Launchers;
using OpenForge.Cli.Core.Framework.Tools.Models.Records;

namespace OpenForge.Cli.Core.Framework.Tools.Models.Services;

internal sealed record ToolSetupRequest
{
    public required ToolSetupSpec Setup { get; init; }
    public required ToolCommandTarget Invocation { get; init; }
    public required string WorkspacePath { get; init; }
    public required IReadOnlyList<ToolWorkspaceEditRecord> Previous { get; init; }
}

internal sealed record ToolSetupResult
{
    public required IReadOnlyList<ToolWorkspaceEditRecord> Outputs { get; init; }
    public ToolProcessResult? Process { get; init; }
    public required bool AlreadyConfigured { get; init; }
    public required IReadOnlyList<string> Unfinished { get; init; }
}
