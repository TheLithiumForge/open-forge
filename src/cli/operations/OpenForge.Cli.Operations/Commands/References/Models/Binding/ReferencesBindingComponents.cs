using OpenForge.Cli.Core.Commands.References;
using OpenForge.Cli.Core.Commands.References.Models.Result;
using OpenForge.Cli.Core.Shell.Pipeline;
using OpenForge.Cli.Core.Shell.Presentation.Models;

namespace OpenForge.Cli.Core.Commands.References.Models.Binding;

internal sealed class ReferencesBindingComponents
{
    internal required CliHelpContent Help { get; init; }

    internal required ReferencesOperation Operation { get; init; }
}
