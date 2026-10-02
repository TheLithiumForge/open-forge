using OpenForge.Cli.Core.Framework.Sources.Models.Identity;

namespace OpenForge.Cli.Core.Commands.Shared.WorkspaceAdoption.Models;

internal static class WorkspaceAdoptionTopologyState
{
    internal sealed record SourceFact(
        string Path,
        string PathKey,
        string Directory,
        string DirectoryKey,
        SourceDocumentForm Form);

    internal sealed record SelectedRoot(string Path, string Key);

    internal sealed record SkillDirectory(string Path, string Key);

    internal sealed record EntryPointHost(string DirectoryKey, string PathKey, string Path);

    internal sealed record RouteWalk(string StartDirectory, string StopDirectoryKey);

    internal enum SourceOwner
    {
        None,
        Framework,
        Extension,
        Library,
    }
}
