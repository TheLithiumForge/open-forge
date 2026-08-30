using System.CommandLine;
using OpenForge.Cli.Core.Commands.Extension.Create.Models.Result;
using OpenForge.Cli.Core.Shell.Pipeline;
using OpenForge.Cli.Core.Shell.Presentation;

namespace OpenForge.Cli.Core.Commands.Extension.Create.Models.Binding;

internal sealed record ExtensionCreateSymbols
{
    public required Command CreateCommand { get; init; }

    public required Argument<string?> StableId { get; init; }

    public required Option<string?> Path { get; init; }

    public required Option<string?> Name { get; init; }

    public required Option<string?> Description { get; init; }

    public required Option<string?> PackageVersion { get; init; }

    public required Option<string[]> Dependency { get; init; }

    public required Option<bool> Automatic { get; init; }

    public required Option<bool> DryRun { get; init; }

    internal static ExtensionCreateSymbols Create(Command extensionGroup)
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

    private static Option<string?> CreateSingleton(
        OpenForge.Cli.Core.Shell.Definitions.CliOptionDefinition<string?> definition)
        => new(definition.Name)
        {
            Description = definition.Description,
            HelpName = definition.ValueName,
            Arity = ArgumentArity.ZeroOrOne,
        };

    private static Option<bool> CreateBoolean(
        OpenForge.Cli.Core.Shell.Definitions.CliOptionDefinition<bool> definition)
        => new(definition.Name)
        {
            Description = definition.Description,
            Arity = ArgumentArity.Zero,
        };
}

internal sealed class ExtensionCreateBindingComponents
{
    public required CliHelpContent Help { get; init; }

    public required ExtensionCreateOperation Operation { get; init; }

    public required CliRendererSet<ExtensionCreateResult> Renderers { get; init; }

    public CliDiagnosticRenderer<ExtensionCreateResult>? DiagnosticRenderer { get; init; }
}
