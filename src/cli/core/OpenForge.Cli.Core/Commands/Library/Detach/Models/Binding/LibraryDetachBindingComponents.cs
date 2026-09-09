using OpenForge.Cli.Core.Shell.Pipeline;
using OpenForge.Cli.Core.Commands.Library.Detach.Models.Result;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Presentation;

namespace OpenForge.Cli.Core.Commands.Library.Detach.Models.Binding;

internal sealed record LibraryDetachBindingComponents
{
    public required LibraryDetachOperation Operation { get; init; }

    public required CliHelpContent Help { get; init; }

    public required CliRendererSet<LibraryDetachResult> Renderers { get; init; }
}
