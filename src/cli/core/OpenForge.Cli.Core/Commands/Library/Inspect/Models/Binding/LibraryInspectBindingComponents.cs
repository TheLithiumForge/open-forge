using OpenForge.Cli.Core.Commands.Library.Inspect.Models.Result;
using OpenForge.Cli.Core.Shell.Pipeline;
using OpenForge.Cli.Core.Shell.Presentation.Models;

namespace OpenForge.Cli.Core.Commands.Library.Inspect.Models.Binding;

internal sealed record LibraryInspectBindingComponents
{
    public required CliHelpContent Help { get; init; }

    public required LibraryInspectOperation Operation { get; init; }

    public required CliRendererSet<LibraryInspectResult> Renderers { get; init; }
}
