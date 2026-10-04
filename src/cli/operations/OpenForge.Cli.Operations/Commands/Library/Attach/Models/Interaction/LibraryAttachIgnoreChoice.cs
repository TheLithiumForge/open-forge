using OpenForge.Cli.Core.Shell.Interaction.Models;

namespace OpenForge.Cli.Core.Commands.Library.Attach.Models.Interaction;

internal sealed record LibraryAttachIgnoreChoice(
    bool CanPrompt,
    Func<CancellationToken, ValueTask<CliPromptReply<bool>>> AskAsync);
