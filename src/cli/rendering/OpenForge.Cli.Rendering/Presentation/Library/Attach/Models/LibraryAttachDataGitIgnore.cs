namespace OpenForge.Cli.Core.Presentation.Library.Attach.Models;

internal sealed record LibraryAttachDataGitIgnore(string Path, string Action, IReadOnlyList<string> Paths);
