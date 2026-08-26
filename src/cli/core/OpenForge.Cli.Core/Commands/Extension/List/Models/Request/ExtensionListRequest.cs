using OpenForge.Cli.Core.Framework.Workspace;

namespace OpenForge.Cli.Core.Commands.Extension.List.Models.Request;

internal sealed record ExtensionListSelection
{
    private ExtensionListSelection(bool installed, bool available)
    {
        Installed = installed;
        Available = available;
    }

    public bool Installed { get; }

    public bool Available { get; }

    internal static ExtensionListSelection Create(bool installedFlag, bool availableFlag)
        => new(
            installed: installedFlag || !availableFlag,
            available: availableFlag || !installedFlag);
}

internal sealed record ExtensionListRequest
{
    public required CliWorkspace Workspace { get; init; }

    public required ExtensionListSelection Selection { get; init; }

    public required string? ExplicitSource { get; init; }
}
