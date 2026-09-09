using OpenForge.Cli.Core.Shell.Pipeline;
using OpenForge.Cli.Core.Commands.Library.Sync.Models.Result;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Presentation;

namespace OpenForge.Cli.Core.Commands.Library.Sync.Models.Binding;

internal sealed record LibrarySyncBindingComponents
{
    public required LibrarySyncOperation Operation { get; init; }

    public required CliHelpContent Help { get; init; }

    public required CliRendererSet<LibrarySyncResult> Renderers { get; init; }
}
