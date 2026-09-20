using OpenForge.Cli.Core.Commands.Library.Attach.Models.Result;
using OpenForge.Cli.Core.Shell.Pipeline;
using OpenForge.Cli.Core.Shell.Presentation.Models;

namespace OpenForge.Cli.Core.Commands.Library.Attach.Models.Binding;

internal sealed record LibraryAttachBindingComponents
{
    public required LibraryAttachOperation Operation { get; init; }

    public required CliHelpContent Help { get; init; }
}
