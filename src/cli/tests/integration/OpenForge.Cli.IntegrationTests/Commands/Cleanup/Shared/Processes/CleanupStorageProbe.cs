using OpenForge.Cli.Core.Commands.Cleanup;
using OpenForge.Cli.Core.Commands.Cleanup.Models.Request;
using OpenForge.Cli.Core.Framework.Workspace.Models;

namespace OpenForge.Cli.IntegrationTests.Commands.Cleanup.Shared.Processes;

internal static class CleanupStorageProbe
{
    internal const string ChildArgument = "--cleanup-storage-probe";

    internal static async Task RunAsync(string[] arguments)
    {
        var workspace = new CliWorkspace(arguments[1], arguments[1], CliWorkspaceSelectionMethod.ExplicitWorkspace);
        var mode = Enum.Parse<CleanupMode>(arguments[2]);
        var result = await new CleanupOperation().ExecuteAsync(new(workspace, mode), CancellationToken.None);
        var store = result.Stores.Single(item => item.Kind == arguments[3]);
        Console.WriteLine(store.Facts.Catalogue.Coverage);
        Console.WriteLine(store.Facts.Catalogue.Candidates.Length);
        Console.WriteLine(result.Lease.State);
    }
}
