using System.Collections.ObjectModel;
using System.CommandLine;
using OpenForge.Cli.Core.Shell.Composition;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Presentation;

namespace OpenForge.Cli.Core.Shell.Parsing;

internal sealed record CliGlobalOptionSymbols(
    Option<string?> Workspace,
    Option<bool> Json,
    Option<CliView> View,
    Option<bool> Verbose,
    Option<bool> Help,
    Option<bool> Version);

internal sealed record CliRootBranch(
    Command Command,
    CliHelpContent Help,
    IReadOnlyList<CliDelimiterPolicy> DelimiterPolicies);

internal sealed record CliCommandHelp(Command Command, CliHelpContent Content);

internal sealed record CliParseOutcome(
    ParseResult Result,
    CliCommandTree Tree,
    CliGlobalOptionSymbols Options,
    IReadOnlyList<string> OriginalArguments,
    IReadOnlyList<CliDelimiterPolicy> DelimiterPolicies);

internal sealed class CliCommandTree
{
    private readonly ParserConfiguration _parserConfiguration;
    private readonly IReadOnlyList<CliDelimiterPolicy> _delimiterPolicies;
    private readonly IReadOnlyDictionary<Command, ICliCommandBinding> _bindings;
    private readonly IReadOnlySet<Command> _groups;
    private readonly IReadOnlyDictionary<Command, CliHelpContent> _help;

    private CliCommandTree(
        RootCommand root,
        CliGlobalOptionSymbols options,
        IReadOnlyList<CliDelimiterPolicy> delimiterPolicies,
        IReadOnlyDictionary<Command, ICliCommandBinding> bindings,
        IReadOnlySet<Command> groups,
        IReadOnlyDictionary<Command, CliHelpContent> help)
    {
        Root = root;
        Options = options;
        _delimiterPolicies = delimiterPolicies;
        _bindings = bindings;
        _groups = groups;
        _help = help;
        _parserConfiguration = new ParserConfiguration
        {
            EnablePosixBundling = false,
            ResponseFileTokenReplacer = null,
        };
    }

    internal RootCommand Root { get; }

    internal CliGlobalOptionSymbols Options { get; }

    internal static CliCommandTree Create(
        CliHelpContent rootHelp,
        IEnumerable<CliRootBranch> branches,
        IEnumerable<ICliCommandBinding> bindings,
        IEnumerable<CliCommandHelp>? additionalHelp = null)
    {
        ArgumentNullException.ThrowIfNull(rootHelp);
        ArgumentNullException.ThrowIfNull(branches);
        ArgumentNullException.ThrowIfNull(bindings);

        var branchArray = branches.ToArray();
        var definition = CliRootDefinitionFactory.Create(branchArray);
        var helpByCommand = new Dictionary<Command, CliHelpContent>(ReferenceEqualityComparer.Instance)
        {
            [definition.Root] = rootHelp,
        };
        var groups = new HashSet<Command>(ReferenceEqualityComparer.Instance);
        foreach (var branch in branchArray)
        {
            ArgumentNullException.ThrowIfNull(branch);
            groups.Add(branch.Command);
            helpByCommand.Add(branch.Command, branch.Help);
        }

        var bindingByCommand = new Dictionary<Command, ICliCommandBinding>(ReferenceEqualityComparer.Instance);
        foreach (var binding in bindings)
        {
            ArgumentNullException.ThrowIfNull(binding);
            bindingByCommand.Add(binding.Command, binding);
            helpByCommand.TryAdd(binding.Command, binding.Help);
        }

        if (additionalHelp is not null)
        {
            foreach (var registration in additionalHelp)
            {
                ArgumentNullException.ThrowIfNull(registration);
                helpByCommand[registration.Command] = registration.Content;
            }
        }

        return new CliCommandTree(
            definition.Root,
            definition.Options,
            definition.DelimiterPolicies,
            new ReadOnlyDictionary<Command, ICliCommandBinding>(bindingByCommand),
            groups,
            new ReadOnlyDictionary<Command, CliHelpContent>(helpByCommand));
    }

    internal CliParseOutcome Parse(string[] arguments)
    {
        ArgumentNullException.ThrowIfNull(arguments);
        var originalArguments = Array.AsReadOnly(arguments.ToArray());
        return new CliParseOutcome(
            Root.Parse(originalArguments, _parserConfiguration),
            this,
            Options,
            originalArguments,
            _delimiterPolicies);
    }

    internal ICliCommandBinding? FindBinding(Command command)
    {
        ArgumentNullException.ThrowIfNull(command);
        return _bindings.GetValueOrDefault(command);
    }

    internal bool IsGroup(Command command)
    {
        ArgumentNullException.ThrowIfNull(command);
        return _groups.Contains(command);
    }

    internal CliHelpContent ReadHelp(Command command)
    {
        ArgumentNullException.ThrowIfNull(command);
        return _help.GetValueOrDefault(command) ?? CliHelpContent.Empty;
    }

}
