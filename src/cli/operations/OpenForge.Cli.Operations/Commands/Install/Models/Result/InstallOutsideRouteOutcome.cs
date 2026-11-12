using OpenForge.Cli.Core.Framework.Workspace.Models;

namespace OpenForge.Cli.Core.Commands.Install.Models.Result;

internal enum InstallOutsideRouteState { Registered, AlreadyRegistered, Failed, NotAttempted }

/// <summary>What happened to the folder chosen at the first-installation question, reported after Install applies.</summary>
internal sealed record InstallOutsideRouteOutcome(string Folder, InstallOutsideRouteState State)
{
    internal string? Catalogue { get; init; }
    internal string? Cause { get; init; }
}

/// <summary>
/// Registers one existing folder as an outside route after Install has written the loader. The root host composes it
/// from the folder Include operation, so Install never reaches into another command's private support.
/// </summary>
internal delegate ValueTask<InstallOutsideRouteOutcome> InstallOutsideRouteRegistration(CliWorkspace workspace, string folder, CancellationToken token);
