using System.CommandLine;
using OpenForge.Cli.Core.Commands.Extension.Create.Models.Binding;
using OpenForge.Cli.Core.Commands.Extension.Create.Models.Request;
using OpenForge.Cli.Core.Commands.Extension.Create.Models.Result;
using OpenForge.Cli.Core.Shell.Composition;
using OpenForge.Cli.Core.Shell.Composition.Models;

namespace OpenForge.Cli.Core.Commands.Extension.Create;

internal static class ExtensionCreateBinding
{
    internal static ExtensionCreateSymbols CreateSymbols(Command extensionGroup)
    {
        ArgumentNullException.ThrowIfNull(extensionGroup);
        var command = new Command(
            ExtensionCreateDefinitions.CreateCommand.Name,
            ExtensionCreateDefinitions.CreateCommand.Description);
        var stableId = new Argument<string?>("stable-id")
        {
            Description = "One exact lowercase Extension stable ID.",
            Arity = ArgumentArity.ZeroOrOne,
        };
        var path = CreateSingleton(ExtensionCreateDefinitions.Path);
        var name = CreateSingleton(ExtensionCreateDefinitions.Name);
        var description = CreateSingleton(ExtensionCreateDefinitions.Description);
        var packageVersion = CreateSingleton(ExtensionCreateDefinitions.PackageVersion);
        var dependency = new Option<string[]>(ExtensionCreateDefinitions.Dependency.Name)
        {
            Description = ExtensionCreateDefinitions.Dependency.Description,
            HelpName = ExtensionCreateDefinitions.Dependency.ValueName,
            Arity = ArgumentArity.ZeroOrMore,
            AllowMultipleArgumentsPerToken = false,
        };
        var automatic = CreateBoolean(ExtensionCreateDefinitions.Automatic);
        var dryRun = CreateBoolean(ExtensionCreateDefinitions.DryRun);
        command.Arguments.Add(stableId);
        command.Options.Add(path);
        command.Options.Add(name);
        command.Options.Add(description);
        command.Options.Add(packageVersion);
        command.Options.Add(dependency);
        command.Options.Add(automatic);
        command.Options.Add(dryRun);
        extensionGroup.Add(command);
        return new ExtensionCreateSymbols
        {
            CreateCommand = command,
            StableId = stableId,
            Path = path,
            Name = name,
            Description = description,
            PackageVersion = packageVersion,
            Dependency = dependency,
            Automatic = automatic,
            DryRun = dryRun,
        };
    }

    internal static CliRequestBinding<ExtensionCreateRequest, ExtensionCreateResult> CreateRequestBinding(
        ExtensionCreateSymbols symbols,
        ExtensionCreateBindingComponents components)
    {
        ArgumentNullException.ThrowIfNull(symbols);
        ArgumentNullException.ThrowIfNull(components);
        var binder = new ExtensionCreateRequestBinder(symbols);
        var invalidFactory = new ExtensionCreateInvalidResultFactory(symbols);
        return new CliRequestBinding<ExtensionCreateRequest, ExtensionCreateResult>
        {
            Command = symbols.CreateCommand,
            Help = components.Help,
            WorkspaceRequirement = CliWorkspaceRequirement.Absent,
            Binder = binder.Bind,
            InvalidResultFactory = invalidFactory.Create,
            Operation = components.Operation.ExecuteAsync,
        };
    }

    private static Option<string?> CreateSingleton(
        OpenForge.Cli.Core.Shell.Definitions.Models.CliOptionDefinition<string?> definition)
        => new(definition.Name)
        {
            Description = definition.Description,
            HelpName = definition.ValueName,
            Arity = ArgumentArity.ZeroOrOne,
        };

    private static Option<bool> CreateBoolean(
        OpenForge.Cli.Core.Shell.Definitions.Models.CliOptionDefinition<bool> definition)
        => new(definition.Name)
        {
            Description = definition.Description,
            Arity = ArgumentArity.Zero,
        };
}
