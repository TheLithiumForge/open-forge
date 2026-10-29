using OpenForge.Cli.Core.Commands.Install.Models.Result;
using OpenForge.Cli.Core.Commands.Route.Include;
using OpenForge.Cli.Core.Commands.Route.Include.Models.Request;
using OpenForge.Cli.Core.Framework.Mutation.Locking.Models;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.Composition.Shared.InstallOutsideRoute;

/// <summary>
/// Composes Install's docs-folder registration from the folder Include operation, so both commands register an
/// outside route the same way while Install never imports Route's private support.
/// </summary>
internal static class InstallOutsideRouteComposer
{
    private const string UnknownCause = "Folder Include did not complete.";

    internal static InstallOutsideRouteRegistration Create(WorkspaceLockStoreRoot? lockStoreRoot)
        => async (workspace, folder, token) =>
        {
            var included = await RouteIncludeOperationFactory.Create(lockStoreRoot)
                .ExecuteAsync(new RouteIncludeRequest { Workspace = workspace, File = folder }, token).ConfigureAwait(false);
            var outcome = included.Outcome;
            if (outcome.Status != CliSemanticStatus.Complete)
                return new(folder, InstallOutsideRouteState.Failed) { Cause = outcome.Cause ?? UnknownCause };
            return new(folder, outcome.NoOp ? InstallOutsideRouteState.AlreadyRegistered : InstallOutsideRouteState.Registered)
            {
                Catalogue = outcome.Catalogue,
            };
        };
}
