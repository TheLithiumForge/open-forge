using OpenForge.Cli.Core.Commands.Cleanup.Models.Binding;
using OpenForge.Cli.Core.Commands.Cleanup.Models.Request;
using OpenForge.Cli.Core.Commands.Cleanup.Models.Result;
using OpenForge.Cli.Core.Shell.Composition;
using OpenForge.Cli.Core.Shell.Composition.Models;
using OpenForge.Cli.Core.Shell.Invocation.Models;

namespace OpenForge.Cli.Core.Commands.Cleanup;

internal static class CleanupBinding
{
    internal static CleanupSymbols CreateSymbols() => CleanupSymbols.Create();

    internal static CliBindResult<CleanupRequest, CleanupResult> Bind(
        CliBindingParse parse,
        CliInvocation invocation,
        CleanupSymbols symbols)
    {
        ArgumentNullException.ThrowIfNull(symbols);
        return new CleanupRequestBinder(symbols).Bind(parse, invocation);
    }

    internal static CliRequestBinding<CleanupRequest, CleanupResult> CreateRequestBinding(
        CleanupSymbols symbols,
        CleanupBindingComponents components)
    {
        ArgumentNullException.ThrowIfNull(symbols);
        ArgumentNullException.ThrowIfNull(components);
        ArgumentNullException.ThrowIfNull(components.Operation);
        ArgumentNullException.ThrowIfNull(components.InvalidResultFactory);
        return new CliRequestBinding<CleanupRequest, CleanupResult>
        {
            Command = symbols.CleanupCommand,
            Help = components.Help,
            WorkspaceRequirement = CliWorkspaceRequirement.Required,
            Binder = (parse, invocation) => Bind(parse, invocation, symbols),
            InvalidResultFactory = components.InvalidResultFactory,
            Operation = components.Operation.ExecuteAsync,
        };
    }
}
