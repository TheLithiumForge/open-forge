using OpenForge.Cli.Core.Presentation.Shared.Prompts;
using OpenForge.Cli.Core.Shell.Interaction;

namespace OpenForge.Cli.Composition.Models;

internal sealed record CliInteractionComposition(CliTerminal Terminal, CliPrompts Prompts);
