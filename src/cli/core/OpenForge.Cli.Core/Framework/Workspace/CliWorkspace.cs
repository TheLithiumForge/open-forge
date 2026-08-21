namespace OpenForge.Cli.Core.Framework.Workspace;

internal sealed record CliWorkspaceRequest(
    string? ExplicitPath,
    string CurrentDirectory)
{
    internal bool IsExplicit => ExplicitPath is not null;
}

internal enum CliWorkspaceSelectionMethod
{
    CurrentDirectory,
    ExplicitWorkspace,
}

internal sealed record CliWorkspace
{
    internal CliWorkspace(
        string lexicalRoot,
        string physicalRoot,
        CliWorkspaceSelectionMethod selectedBy)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(lexicalRoot);
        ArgumentException.ThrowIfNullOrWhiteSpace(physicalRoot);
        if (!Enum.IsDefined(selectedBy))
        {
            throw new ArgumentOutOfRangeException(nameof(selectedBy), selectedBy, "The workspace selection method is not defined.");
        }

        LexicalRoot = Path.GetFullPath(lexicalRoot);
        PhysicalRoot = Path.GetFullPath(physicalRoot);
        SelectedBy = selectedBy;
    }

    internal string LexicalRoot { get; }

    internal string PhysicalRoot { get; }

    internal CliWorkspaceSelectionMethod SelectedBy { get; }
}
