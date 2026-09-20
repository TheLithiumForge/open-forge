using System.CommandLine;
using OpenForge.Cli.Core.Commands.Cleanup.Models.Result;
using OpenForge.Cli.Core.Shell.Composition;
using OpenForge.Cli.Core.Shell.Pipeline;
using OpenForge.Cli.Core.Shell.Presentation.Models;

namespace OpenForge.Cli.Core.Commands.Cleanup.Models.Binding;

internal sealed record CleanupSymbols(Command CleanupCommand, Option<bool> DryRun)
{
    internal Command Command => CleanupCommand;

    internal static CleanupSymbols Create()
    {
        var command = new Command(
            CleanupDefinitions.CleanupCommand.Name,
            CleanupDefinitions.CleanupCommand.Description);
        var dryRun = new Option<bool>(CleanupDefinitions.DryRun.Name)
        {
            Description = CleanupDefinitions.DryRun.Description,
            Arity = ArgumentArity.Zero,
        };
        command.Options.Add(dryRun);
        return new CleanupSymbols(command, dryRun);
    }
}

internal sealed class CleanupBindingComponents
{
    public required CliHelpContent Help { get; init; }

    public required CleanupOperation Operation { get; init; }

    public required CliContextualInvalidResultFactory<CleanupResult> InvalidResultFactory { get; init; }
}
