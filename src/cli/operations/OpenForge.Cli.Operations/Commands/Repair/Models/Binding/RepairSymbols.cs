using System.CommandLine;
using OpenForge.Cli.Core.Commands.Repair.Models.Request;

namespace OpenForge.Cli.Core.Commands.Repair.Models.Binding;

internal sealed record RepairSymbols(
    Command RepairCommand,
    Option<bool> Automatic,
    Option<string[]> Relink,
    Option<bool> DryRun)
{
    internal static RepairSymbols Create()
    {
        var command = new Command(
            RepairDefinitions.RepairCommand.Name,
            RepairDefinitions.RepairCommand.Description);
        var automatic = new Option<bool>(RepairDefinitions.Automatic.Name)
        {
            Description = RepairDefinitions.Automatic.Description,
            Arity = ArgumentArity.Zero,
        };
        var relinkDefinition = RepairDefinitions.Relink;
        var relink = new Option<string[]>(relinkDefinition.Name)
        {
            Description = relinkDefinition.Description,
            HelpName = relinkDefinition.ValueName,
            Arity = new ArgumentArity(
                relinkDefinition.MinimumArguments,
                relinkDefinition.MaximumArguments),
            AllowMultipleArgumentsPerToken = relinkDefinition.AllowMultipleArgumentsPerToken,
        };
        var dryRun = new Option<bool>(RepairDefinitions.DryRun.Name)
        {
            Description = RepairDefinitions.DryRun.Description,
            Arity = ArgumentArity.Zero,
        };

        command.Options.Add(automatic);
        command.Options.Add(relink);
        command.Options.Add(dryRun);
        return new(command, automatic, relink, dryRun);
    }
}

internal sealed record RepairBindingInput(
    RepairMode Mode,
    bool Automatic,
    IReadOnlyList<string> RelinkValues);
