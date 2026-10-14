using OpenForge.Cli.Core.Presentation.Shared.Prompts;
using OpenForge.Cli.Core.Presentation.Shared.Text;
using OpenForge.Cli.Core.Shell.Interaction.Models;
using OpenForge.Cli.OutputText.Extension.Create;

namespace OpenForge.Cli.Core.Presentation.Extension.Create.Shared.Prompts;

internal sealed class ExtensionCreateFolderPrompt(CliPrompts prompts, CliTerminalWrite write)
{
    internal async ValueTask<CliPromptReply<string>> AskAsync(
        CliTextQuestion<string> question,
        CliPromptPolicy policy,
        CancellationToken cancellationToken)
    {
        if (!prompts.CanPrompt(policy)) return CliPromptReply<string>.Unavailable();
        try
        {
            var heading = CliText.Escape(ExtensionCreateText.HeadingPackageFolder()) + Environment.NewLine;
            await write(heading.AsMemory(), cancellationToken).ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return CliPromptReply<string>.Cancelled();
        }
        return await prompts.TextAsync(question, policy, cancellationToken).ConfigureAwait(false);
    }
}
