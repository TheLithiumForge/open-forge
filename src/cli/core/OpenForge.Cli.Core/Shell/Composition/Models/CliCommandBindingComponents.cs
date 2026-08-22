using OpenForge.Cli.Core.Shell.Composition;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Pipeline;
using OpenForge.Cli.Core.Shell.Presentation;

namespace OpenForge.Cli.Core.Shell.Composition.Models;

internal sealed class CliCommandBindingComponents<TRequest, TResult>
    where TResult : ICliCommandResult
{
    internal required CliHelpContent Help { get; init; }

    internal required CliWorkspaceRequirement WorkspaceRequirement { get; init; }

    internal required CliRequestBinder<TRequest, TResult> Binder { get; init; }

    internal required CliContextualInvalidResultFactory<TResult> InvalidResultFactory { get; init; }

    internal required CliOperation<TRequest, TResult> Operation { get; init; }

    internal required CliRendererSet<TResult> Renderers { get; init; }

    internal CliDiagnosticRenderer<TResult>? DiagnosticRenderer { get; init; }
}
