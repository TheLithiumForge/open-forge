using System.CommandLine;
using OpenForge.Cli.Core.Commands.Install.Models.Configuration;
using OpenForge.Cli.Core.Commands.Install.Models.Request;

namespace OpenForge.Cli.Core.Commands.Install.Models.Binding;

internal sealed record InstallSymbols(
    Command InstallCommand,
    Option<bool> Force,
    Option<bool> Automatic,
    Option<bool> DryRun,
    Option<bool> Configure,
    Option<string?> Preset,
    Option<string[]> Route,
    Option<string[]> Frontmatter)
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
        var configure = new Option<bool>("--configure") { Description = "Configure built-in routes while retaining existing files.", Arity = ArgumentArity.Zero };
        var preset = new Option<string?>("--preset") { Description = "Select essentials, full-core, or custom.", Arity = ArgumentArity.ExactlyOne };
        var route = new Option<string[]>("--route") { Description = "Set a Custom route: <id>=<add|remove|git-ignore>.", Arity = ArgumentArity.OneOrMore, AllowMultipleArgumentsPerToken = false };
        var frontmatter = new Option<string[]>(InstallDefinitions.Frontmatter.Name)
        {
            Description = InstallDefinitions.Frontmatter.Description,
            Arity = ArgumentArity.OneOrMore,
            AllowMultipleArgumentsPerToken = false,
        };
        command.Options.Add(configure);
        command.Options.Add(preset);
        command.Options.Add(frontmatter);
        command.Options.Add(route);
        command.Options.Add(force);
        command.Options.Add(automatic);
        command.Options.Add(dryRun);
        return new InstallSymbols(
            InstallCommand: command,
            Force: force,
            Automatic: automatic,
            DryRun: dryRun, Configure: configure, Preset: preset, Route: route, Frontmatter: frontmatter);
    }
}

internal sealed record InstallBindingInput(
    bool Force,
    bool Automatic,
    InstallMode Mode,
    InstallSetupInput? Setup = null,
    InstallConfiguration? Configuration = null,
    string? SetupError = null)
{
    internal bool IsDryRun => Mode == InstallMode.DryRun;
}
