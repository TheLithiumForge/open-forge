using OpenForge.Cli.Core.Framework.Workspace.Models;

namespace OpenForge.Cli.Core.Commands.Extension.Inspect.Models.Request;

internal sealed record ExtensionInspectRequest
{
    internal ExtensionInspectRequest(
        CliWorkspace workspace,
        string stableId,
        string? explicitSource)
    {
        Workspace = workspace;
        StableId = stableId;
        ExplicitSource = explicitSource;
    }

    internal CliWorkspace Workspace { get; }

    internal string StableId { get; }

    internal string? ExplicitSource { get; }
}
