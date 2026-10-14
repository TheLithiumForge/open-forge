using OpenForge.Cli.Core.Commands.Shared.LibraryLinks.Models;
using OpenForge.Cli.Core.Framework.Libraries.Models.Identity;
using OpenForge.Cli.Core.Framework.Workspace.Models;

namespace OpenForge.Cli.Core.Commands.Shared.LibraryLinks;

internal static class LibraryLinkRecreationReader
{
    internal static LibraryLinkRecreation Read(LibraryRegistration registration, CliWorkspace workspace)
        => new()
        {
            Workspace = workspace.LexicalRoot,
            Id = registration.Id.Value,
            SourceRoot = registration.SourceRoot.Value,
            DestinationRoot = registration.DestinationRoot.Value,
            GitIgnore = registration.GitIgnore,
        };
}
