using OpenForge.Cli.Core.Framework.Workspace;
using OpenForge.Cli.Core.Shell.Composition.Models;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Invocation;
using OpenForge.Cli.Core.Shell.Invocation.Models;
using OpenForge.Cli.Core.Shell.Parsing;
using OpenForge.Cli.Core.Shell.Parsing.Models.Input;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Output;
using OpenForge.Cli.Core.Presentation.Shared.Help;
using OpenForge.Cli.Core.Presentation.Shared.Prompts;
using OpenForge.Cli.Core.Presentation.Shared.Text;
using OpenForge.Cli.Core.Shell.Parsing.Models.Results;
using System.CommandLine;
using System.CommandLine.Parsing;

namespace OpenForge.Cli.Core.Shell.Composition;

internal sealed class CliCoreApplication
{
    private readonly CliProcessIdentity _process;
    private readonly CliCommandTree _tree;
    private readonly CliWorkspaceSelector _workspaceSelector;

    internal CliCoreApplication(
        CliProcessIdentity process,
        CliCommandTree tree,
        CliWorkspaceSelector workspaceSelector)
    {
        ArgumentNullException.ThrowIfNull(process);
        ArgumentNullException.ThrowIfNull(tree);
        ArgumentNullException.ThrowIfNull(workspaceSelector);
        _process = process;
        _tree = tree;
        _workspaceSelector = workspaceSelector;
    }

    internal async ValueTask<CliProcessCompletion> RunAsync(
        string[] arguments,
        CliProcessEnvironment environment,
        CliOutputWriters writers,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(arguments);
        ArgumentNullException.ThrowIfNull(environment);
        ArgumentNullException.ThrowIfNull(writers);

        writers = writers with { Width = environment.HelpWidth };

        var parse = _tree.Parse(arguments);
        var global = CliTerminalValidator.Validate(parse);
        if (global.InvalidInput is not null)
        {
            return await WriteShellInvalidAsync(global.InvalidInput, parse, writers, cancellationToken).ConfigureAwait(false);
        }

        var input = global.Input
            ?? throw new InvalidOperationException("A valid global input resolution requires typed input.");
        var selection = CliBindingSelector.Select(parse);
        if (global.TerminalMode == CliTerminalMode.Version)
        {
            await writers.StandardOutput
                .WriteLineAsync(_process.InformationalVersion.AsMemory(), cancellationToken)
                .ConfigureAwait(false);
            return CliProcessCompletionPolicy.Complete(
                CliSemanticStatus.Complete,
                CliOutputTarget.StandardOutput);
        }

        if (global.TerminalMode == CliTerminalMode.Help || selection.Binding is null)
        {
            var help = CliHelpRenderer.Render(parse.Result, parse.Tree.ReadHelp(selection.Command), environment.HelpWidth);
            await writers.StandardOutput.WriteLineAsync(help.AsMemory(), cancellationToken).ConfigureAwait(false);
            return CliProcessCompletionPolicy.Complete(
                CliSemanticStatus.Complete,
                CliOutputTarget.StandardOutput);
        }

        var invocation = CliInvocationResolver.Resolve(
            input,
            _process,
            environment,
            selection.Binding.WorkspaceRequirement,
            _workspaceSelector);
        if (invocation.InvalidInput is not null)
        {
            return await selection.Binding
                .PresentInvalidAsync(
                    new CliInvalidBindingInput(
                        invocation.InvalidInput,
                        input,
                        environment,
                        new CliBindingParse(parse.Result, parse.OriginalArguments))
                    {
                        WorkspaceSelectionState = invocation.WorkspaceSelectionState,
                    },
                    writers,
                    cancellationToken)
                .ConfigureAwait(false);
        }

        var resolvedInvocation = invocation.Invocation
            ?? throw new InvalidOperationException("A valid invocation resolution requires an invocation.");

        return await selection.Binding
            .InvokeAsync(
                new CliBindingParse(parse.Result, parse.OriginalArguments),
                resolvedInvocation,
                writers,
                cancellationToken)
            .ConfigureAwait(false);
    }

    private static async ValueTask<CliProcessCompletion> WriteShellInvalidAsync(
        CliInvalidInput invalidInput,
        CliParseOutcome parse,
        CliOutputWriters writers,
        CancellationToken cancellationToken)
    {
        var command = CommandPath(parse.Result);
        var missing = OptionResults(parse.Result.RootCommandResult)
            .Where(option => !option.Implicit && option.Option.Arity.MaximumNumberOfValues > 0
                && !option.Tokens.Any(token => token.Type == TokenType.Argument))
            .DistinctBy(option => option.Option).ToArray();
        IReadOnlyList<string> diagnostics = invalidInput.Diagnostics;
        if (missing.Length > 0)
        {
            diagnostics = missing.SelectMany(option => new[]
            {
                global::OpenForge.Cli.OutputText.Shared.CliInputWording.MissingValue(option.Option.Name),
                global::OpenForge.Cli.OutputText.Shared.CliInputWording.Example(command, option.Option.Name, $"<{option.Option.HelpName ?? "value"}>"),
            }).Concat(parse.Result.Errors.Where(error => !missing.Contains(error.SymbolResult)).Select(error => error.Message)).ToArray();
        }
        foreach (var diagnostic in diagnostics.Distinct(StringComparer.Ordinal))
        {
            var safeDiagnostic = diagnostic;
            foreach (var token in parse.Result.Tokens.OrderByDescending(token => token.Value.Length))
            {
                var escaped = CliText.Escape(token.Value);
                if (escaped != token.Value)
                    safeDiagnostic = safeDiagnostic.Replace(token.Value, escaped, StringComparison.Ordinal);
            }
            foreach (var paragraph in safeDiagnostic.Replace("\r\n", "\n", StringComparison.Ordinal).Split('\n'))
                foreach (var line in CliSelectionText.Wrap(paragraph.Replace("\t", "  ", StringComparison.Ordinal), writers.Width))
                    await writers.StandardError.WriteLineAsync(line.AsMemory(), cancellationToken).ConfigureAwait(false);
        }
        if (invalidInput.Source == CliInvalidInputSource.Parser || missing.Length > 0)
            await writers.StandardError.WriteLineAsync(
                global::OpenForge.Cli.OutputText.Shared.CliInputWording.HelpNext(command).AsMemory(), cancellationToken).ConfigureAwait(false);

        return CliProcessCompletionPolicy.Complete(
            CliSemanticStatus.Invalid,
            CliOutputTarget.StandardError);
    }

    private static string CommandPath(ParseResult parse)
    {
        var names = new List<string>();
        for (SymbolResult? current = parse.CommandResult; current is not null; current = current.Parent)
            if (current is CommandResult command && !ReferenceEquals(command, parse.RootCommandResult)) names.Add(command.Command.Name);
        names.Reverse();
        return string.Join(" ", new[] { CliSyntaxDefinitions.ExecutableName }.Concat(names));
    }

    private static IEnumerable<OptionResult> OptionResults(CommandResult command)
    {
        foreach (var child in command.Children)
        {
            if (child is OptionResult option) yield return option;
            if (child is CommandResult nested)
                foreach (var nestedOption in OptionResults(nested)) yield return nestedOption;
        }
    }
}
