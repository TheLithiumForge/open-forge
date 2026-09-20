using System.CommandLine;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Definitions.Models;
using OpenForge.Cli.Core.Shell.Parsing.Models.CommandTree;

namespace OpenForge.Cli.Core.Shell.Parsing;

internal static class CliRootDefinitionFactory
{
    internal static CliRootDefinition Create(IReadOnlyList<CliRootBranch> branches, IReadOnlyList<CliRootLeaf> rootLeaves)
    {
        ArgumentNullException.ThrowIfNull(branches);
        ArgumentNullException.ThrowIfNull(rootLeaves);
        var workspace = new Option<string?>(CliSyntaxDefinitions.Workspace.Name)
        {
            Description = CliSyntaxDefinitions.Workspace.Description,
            HelpName = CliSyntaxDefinitions.Workspace.ValueName,
            Arity = ArgumentArity.ZeroOrOne,
            Recursive = true,
        };
        var format = Finite(CliSyntaxDefinitions.Format, ["text", "json"]);
        var detail = Finite(CliSyntaxDefinitions.Detail, ["minimal", "standard", "full", "debug"]);
        var filter = new Option<CliSeverityFilter[]>(CliSyntaxDefinitions.DetailFilter.Name)
        {
            Description = CliSyntaxDefinitions.DetailFilter.Description,
            HelpName = CliSyntaxDefinitions.DetailFilter.ValueName,
            Arity = ArgumentArity.OneOrMore,
            AllowMultipleArgumentsPerToken = false,
            Recursive = true,
        };
        filter.AcceptOnlyFromAmong("error", "warning", "info", "all");
        var help = Boolean(CliSyntaxDefinitions.Help);
        var version = Boolean(CliSyntaxDefinitions.Version);
        var root = new RootCommand(CliSyntaxDefinitions.Root.Description);
        root.Options.Clear();
        root.Add(workspace);
        root.Add(format);
        root.Add(detail);
        root.Add(filter);
        root.Add(help);
        root.Add(version);
        root.SetAction(static _ => 0);
        foreach (var branch in branches) root.Add(branch.Command);
        foreach (var leaf in rootLeaves) root.Add(leaf.Command);
        return new CliRootDefinition(root, new CliGlobalOptionSymbols(workspace, format, detail, filter, help, version));
    }

    private static Option<T> Finite<T>(CliOptionDefinition<T> definition, string[] values) where T : struct, Enum
    {
        var option = new Option<T>(definition.Name)
        {
            Description = definition.Description,
            HelpName = definition.ValueName,
            Arity = ArgumentArity.ZeroOrOne,
            Recursive = true,
            DefaultValueFactory = _ => definition.DefaultValue,
        };
        option.AcceptOnlyFromAmong(values);
        return option;
    }

    private static Option<bool> Boolean(CliOptionDefinition<bool> definition)
        => new(definition.Name) { Description = definition.Description, Arity = ArgumentArity.Zero, Recursive = true };
}
