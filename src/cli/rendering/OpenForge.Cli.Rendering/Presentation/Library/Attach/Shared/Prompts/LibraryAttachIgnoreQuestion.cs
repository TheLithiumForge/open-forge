using OpenForge.Cli.Core.Shell.Interaction.Models;
using OpenForge.Cli.OutputText.Library.Shared;

namespace OpenForge.Cli.Core.Presentation.Library.Attach.Shared.Prompts;

internal static class LibraryAttachIgnoreQuestion
{
    internal static CliSelectQuestion<bool> Create() => new(LibraryGitIgnoreText.Question(),
    [
        new(false, LibraryGitIgnoreText.No(), LibraryGitIgnoreText.NoDescription()),
        new(true, LibraryGitIgnoreText.Yes(), LibraryGitIgnoreText.YesDescription()),
    ]);
}
