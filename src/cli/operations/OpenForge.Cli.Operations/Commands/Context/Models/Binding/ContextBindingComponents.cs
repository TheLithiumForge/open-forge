using OpenForge.Cli.Core.Commands.Context.Models.Result;
using OpenForge.Cli.Core.Shell.Pipeline;
using OpenForge.Cli.Core.Shell.Presentation.Models;

namespace OpenForge.Cli.Core.Commands.Context.Models.Binding;

internal sealed class ContextBindingComponents
{
    public required CliHelpContent Help { get; init; }

    public required ContextOperation Operation { get; init; }
}
