using OpenForge.Cli.Core.Framework.OperationalContributors.Models;

namespace OpenForge.Cli.Core.Framework.Workspace.Operational.Models;

internal sealed record WorkspaceEntryStatusView(
    OperationalViewState State,
    OperationalInstallationState Installation,
    string? EntryPath,
    string? LoaderPath);

internal sealed record WorkspaceEntryDoctorView(
    OperationalViewState State,
    OperationalInstallationState Installation,
    string? EntryPath,
    string? LoaderPath);
