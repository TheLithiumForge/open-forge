using OpenForge.Cli.Core.Framework.Workspace;
using OpenForge.Cli.Core.Shell.Composition.Models;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Invocation;
using OpenForge.Cli.Core.Shell.Invocation.Models;
using OpenForge.Cli.Core.Shell.Parsing;
using OpenForge.Cli.Core.Shell.Parsing.Models.Input;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Output;
using OpenForge.Cli.Core.Presentation.Shared.Help;

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

        var parse = _tree.Parse(arguments);
        var global = CliTerminalValidator.Validate(parse);
        if (global.InvalidInput is not null)
        {
            return await WriteShellInvalidAsync(global.InvalidInput, writers, cancellationToken).ConfigureAwait(false);
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
        CliOutputWriters writers,
        CancellationToken cancellationToken)
    {
        foreach (var diagnostic in invalidInput.Diagnostics)
        {
            await writers.StandardError.WriteLineAsync(diagnostic.AsMemory(), cancellationToken).ConfigureAwait(false);
        }

        return CliProcessCompletionPolicy.Complete(
            CliSemanticStatus.Invalid,
            CliOutputTarget.StandardError);
    }
}
