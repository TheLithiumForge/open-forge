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
    internal static CliRootDefinition Create(IReadOnlyList<CliRootBranch> branches)
    {
        ArgumentNullException.ThrowIfNull(branches);
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
            Arity = ReadArity(definition.Arity),
            Recursive = true,
            CustomParser = ParseWorkspace,
        };
    }

    private static string? ParseWorkspace(ArgumentResult result)
    {
        if (result.Tokens.Count != 1)
        {
            return null;
        }

        var value = result.Tokens[0].Value;
        if (!string.IsNullOrEmpty(value))
        {
            return value;
        }

        result.AddError($"{CliSyntaxDefinitions.Workspace.Name} requires a non-empty value.");
        return null;
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
        return new Option<CliView>(definition.Name)
        {
            Description = definition.Description,
            HelpName = definition.ValueName,
            Arity = ReadArity(definition.Arity),
            Recursive = true,
            DefaultValueFactory = _ => definition.DefaultValue,
            CustomParser = ParseView,
        };
    }

    private static CliView ParseView(ArgumentResult result)
    {
        if (result.Tokens.Count != 1)
        {
            return CliView.Expanded;
        }

        var definition = CliSyntaxDefinitions.View;
        if (definition.TryReadFinite(result.Tokens[0].Value, out var view))
        {
            return view;
        }

        result.AddError(
            $"{CliSyntaxDefinitions.View.Name} must be {CliPresentationDefinitions.Compact} or {CliPresentationDefinitions.Expanded}.");
        return CliView.Expanded;
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
