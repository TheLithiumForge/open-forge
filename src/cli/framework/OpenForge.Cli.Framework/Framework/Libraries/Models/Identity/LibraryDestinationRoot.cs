using OpenForge.Cli.Core.Framework.Filesystem.Shared.Paths;

namespace OpenForge.Cli.Core.Framework.Libraries.Models.Identity;

internal sealed record LibraryDestinationRoot
{
    internal const string WorkspaceRootValue = ".";

    private LibraryDestinationRoot(string value) => Value = value;

    internal string Value { get; }

    internal static LibraryDestinationRoot Create(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);
        if (value != WorkspaceRootValue
            && (!PortableWorkspacePath.TryNormalize(value, out var normalized) || normalized != value))
        {
            throw new ArgumentException("A Library destination root must be . or a canonical portable workspace-relative directory.", nameof(value));
        }
        return new(value);
    }
}
