using System.CommandLine;
using OpenForge.Cli.Core.Commands.Update.Models.Request;
using OpenForge.Cli.Core.Shell.Definitions.Models;

namespace OpenForge.Cli.Core.Commands.Update.Models.Binding;

internal sealed record UpdateSymbols(
    Command UpdateCommand,
    Option<bool> Force,
    Option<bool> Prune,
    Option<bool> Automatic,
    Option<bool> DryRun)
{
    internal static UpdateSymbols Create()
    {
        var command = new Command(
            UpdateDefinitions.UpdateCommand.Name,
            UpdateDefinitions.UpdateCommand.Description);
        var force = CreateOption(UpdateDefinitions.Force);
        var prune = CreateOption(UpdateDefinitions.Prune);
        var automatic = CreateOption(UpdateDefinitions.Automatic);
        var dryRun = CreateOption(UpdateDefinitions.DryRun);
        command.Options.Add(force);
        command.Options.Add(prune);
        command.Options.Add(automatic);
        command.Options.Add(dryRun);
        return new UpdateSymbols(command, force, prune, automatic, dryRun);
    }

    private static Option<bool> CreateOption(CliOptionDefinition<bool> definition)
        => new(definition.Name)
        {
            Description = definition.Description,
            Arity = ArgumentArity.Zero,
        };
}

internal sealed record UpdateBindingInput(
    bool Force,
    bool Prune,
    bool Automatic,
    UpdateMode Mode,
    bool AllowsInteractiveConfirmation)
{
    internal bool IsDryRun => Mode == UpdateMode.DryRun;
}
