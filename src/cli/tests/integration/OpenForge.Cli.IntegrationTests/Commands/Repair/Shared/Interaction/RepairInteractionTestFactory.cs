using OpenForge.Cli.Core.Commands.Repair.Models.Interaction;
using OpenForge.Cli.Core.Presentation.Repair;
using OpenForge.Cli.Core.Presentation.Shared.Prompts;
using OpenForge.Cli.Core.Shell.Interaction;
using OpenForge.Cli.Core.Shell.Interaction.Models;

namespace OpenForge.Cli.IntegrationTests.Commands.Repair.Shared.Interaction;

internal static class RepairInteractionTestFactory
{
    internal static RepairInteraction Create(TextReader input, TextWriter output)
    {
        ArgumentNullException.ThrowIfNull(input);
        ArgumentNullException.ThrowIfNull(output);
        var terminal = new CliTerminal(
            new CliTerminalCapabilities(canPrompt: true, canReadKeys: false, canRedraw: false),
            (content, cancellationToken) => new ValueTask(output.WriteAsync(content, cancellationToken)),
            cancellationToken => input.ReadLineAsync(cancellationToken),
            _ => throw new InvalidOperationException("The Repair interaction fixture uses line input."));
        return RepairPromptAdapters.Create(new CliPrompts(terminal), RepairPresentation.Rendering);
    }
}
