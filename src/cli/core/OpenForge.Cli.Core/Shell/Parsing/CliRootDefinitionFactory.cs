using System.CommandLine;
using System.CommandLine.Parsing;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.Core.Shell.Parsing;

internal sealed record CliRootDefinition(
    RootCommand Root,
    CliGlobalOptionSymbols Options,
    IReadOnlyList<CliDelimiterPolicy> DelimiterPolicies);

internal static class CliRootDefinitionFactory
{
    internal static CliRootDefinition Create(
        IReadOnlyList<CliRootBranch> branches,
        IReadOnlyList<CliRootLeaf> rootLeaves)
    {
        ArgumentNullException.ThrowIfNull(branches);
        ArgumentNullException.ThrowIfNull(rootLeaves);

        var workspace = CreateWorkspaceOption();
        var json = CreateBooleanOption(CliSyntaxDefinitions.Json);
        var view = CreateViewOption();
        var verbose = CreateBooleanOption(CliSyntaxDefinitions.Verbose);
        var help = CreateBooleanOption(CliSyntaxDefinitions.Help);
        var version = CreateBooleanOption(CliSyntaxDefinitions.Version);
        var root = new RootCommand(CliSyntaxDefinitions.Root.Description);
        root.Options.Clear();
        root.Add(workspace);
        root.Add(json);
        root.Add(view);
        root.Add(verbose);
        root.Add(help);
        root.Add(version);
        root.SetAction(static _ => 0);

        var policies = new List<CliDelimiterPolicy>();
        foreach (var branch in branches)
        {
            ArgumentNullException.ThrowIfNull(branch);
            root.Add(branch.Command);
            policies.AddRange(branch.DelimiterPolicies);
        }

        foreach (var rootLeaf in rootLeaves)
        {
            ArgumentNullException.ThrowIfNull(rootLeaf, "root leaf");
            root.Add(rootLeaf.Command);
            policies.AddRange(rootLeaf.DelimiterPolicies);
        }

        return new CliRootDefinition(
            root,
            new CliGlobalOptionSymbols(workspace, json, view, verbose, help, version),
            policies.AsReadOnly());
    }

    private static Option<string?> CreateWorkspaceOption()
    {
        var definition = CliSyntaxDefinitions.Workspace;
        return new Option<string?>(definition.Name)
        {
            Description = definition.Description,
            HelpName = definition.ValueName,
            Arity = ArgumentArity.ZeroOrOne,
            Recursive = true,
        };
    }

    private static Option<bool> CreateBooleanOption(CliOptionDefinition<bool> definition)
    {
        return new Option<bool>(definition.Name)
        {
            Description = definition.Description,
            Arity = ReadArity(definition.Arity),
            Recursive = true,
        };
    }

    private static Option<CliView> CreateViewOption()
    {
        var definition = CliSyntaxDefinitions.View;
        var option = new Option<CliView>(definition.Name)
        {
            Description = definition.Description,
            HelpName = definition.ValueName,
            Arity = ArgumentArity.ZeroOrOne,
            Recursive = true,
            DefaultValueFactory = _ => definition.DefaultValue,
        };

        option.AcceptOnlyFromAmong(
            CliPresentationDefinitions.Compact,
            CliPresentationDefinitions.Expanded);
        return option;
    }

    private static ArgumentArity ReadArity(CliOptionArity arity)
    {
        return arity switch
        {
            CliOptionArity.None => ArgumentArity.Zero,
            CliOptionArity.ExactlyOne => ArgumentArity.ExactlyOne,
            _ => throw new ArgumentOutOfRangeException(nameof(arity), arity, "The option arity is not defined."),
        };
    }
}
