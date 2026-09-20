using OpenForge.Cli.Core.Commands.Library.List.Models.Result;
using OpenForge.Cli.Core.Shell.Pipeline;
using OpenForge.Cli.Core.Shell.Presentation.Models;

namespace OpenForge.Cli.Core.Commands.Library.List.Models.Binding;

internal sealed record LibraryListBindingComponents
{
    public required CliHelpContent Help { get; init; }

    public required LibraryListOperation Operation { get; init; }
}
