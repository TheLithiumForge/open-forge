using System.Collections.Immutable;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Files;

namespace OpenForge.Cli.Core.Commands.Remove.Models.Planning;

internal abstract record RemovePathFileCapture
{
    private RemovePathFileCapture()
    {
    }

    internal sealed record Captured(FileStateSnapshot Snapshot) : RemovePathFileCapture;

    internal sealed record Unavailable(string Cause) : RemovePathFileCapture;

    internal sealed record Cancelled : RemovePathFileCapture;
}

internal abstract record RemovePathDirectoryInventory
{
    private RemovePathDirectoryInventory()
    {
    }

    internal sealed record Complete(
        ImmutableArray<FileStateSnapshot> Files,
        ImmutableArray<RemovePathLibraryLink> Links,
        ImmutableArray<FileStateSnapshot> Directories) : RemovePathDirectoryInventory;

    internal sealed record Unavailable(string Cause) : RemovePathDirectoryInventory;

    internal sealed record Cancelled(string Cause) : RemovePathDirectoryInventory;
}
