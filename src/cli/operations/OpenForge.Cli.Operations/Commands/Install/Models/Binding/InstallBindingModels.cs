using System.CommandLine;
using OpenForge.Cli.Core.Commands.Install.Models.Request;

namespace OpenForge.Cli.Core.Commands.Install.Models.Binding;

internal sealed record InstallSymbols(
    Command InstallCommand,
    Option<bool> Force,
    Option<bool> Automatic,
    Option<bool> DryRun)
{
    internal static InstallSymbols Create()
    {
        var command = new Command(
            InstallDefinitions.InstallCommand.Name,
            InstallDefinitions.InstallCommand.Description);
        var force = new Option<bool>(InstallDefinitions.Force.Name)
        {
            Description = InstallDefinitions.Force.Description,
            Arity = ArgumentArity.Zero,
        };
        var automatic = new Option<bool>(InstallDefinitions.Automatic.Name)
        {
            Description = InstallDefinitions.Automatic.Description,
            Arity = ArgumentArity.Zero,
        };
        var dryRun = new Option<bool>(InstallDefinitions.DryRun.Name)
        {
            Description = InstallDefinitions.DryRun.Description,
            Arity = ArgumentArity.Zero,
        };
        command.Options.Add(force);
        command.Options.Add(automatic);
        command.Options.Add(dryRun);
        return new InstallSymbols(
            InstallCommand: command,
            Force: force,
            Automatic: automatic,
            DryRun: dryRun);
    }
}

internal sealed record InstallBindingInput(
    bool Force,
    bool Automatic,
    InstallMode Mode)
{
    internal bool IsDryRun => Mode == InstallMode.DryRun;
}
