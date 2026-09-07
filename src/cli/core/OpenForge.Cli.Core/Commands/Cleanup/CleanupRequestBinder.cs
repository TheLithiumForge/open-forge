using OpenForge.Cli.Core.Commands.Cleanup.Models.Binding;
using OpenForge.Cli.Core.Commands.Cleanup.Models.Request;
using OpenForge.Cli.Core.Commands.Cleanup.Models.Result;
using OpenForge.Cli.Core.Shell.Composition;
using OpenForge.Cli.Core.Shell.Composition.Models;
using OpenForge.Cli.Core.Shell.Invocation;

namespace OpenForge.Cli.Core.Commands.Cleanup;

internal sealed class CleanupRequestBinder(CleanupSymbols symbols)
{
    private readonly CleanupSymbols _symbols = symbols;

    internal CliBindResult<CleanupRequest, CleanupResult> Bind(
        CliBindingParse parse,
        CliInvocation invocation)
    {
        ArgumentNullException.ThrowIfNull(parse);
        ArgumentNullException.ThrowIfNull(invocation);
        var mode = parse.Result.GetValue(_symbols.DryRun)
            ? CleanupMode.DryRun
            : CleanupMode.Apply;

        var workspace = invocation.Workspace
            ?? throw new InvalidOperationException(
                "A bound Cleanup invocation requires a selected workspace.");
        return CliBindResult<CleanupRequest, CleanupResult>.Bound(
            new CleanupRequest(workspace, mode));
    }
}
