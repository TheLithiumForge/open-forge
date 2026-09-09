using OpenForge.Cli.Core.Shell.Pipeline;
using OpenForge.Cli.Core.Commands.Library.Attach.Models.Result;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Presentation;

namespace OpenForge.Cli.Core.Commands.Library.Attach.Models.Binding;

internal sealed record LibraryAttachBindingComponents
{
    public required LibraryAttachOperation Operation { get; init; }

    public required CliHelpContent Help { get; init; }

    public required CliRendererSet<LibraryAttachResult> Renderers { get; init; }
}
