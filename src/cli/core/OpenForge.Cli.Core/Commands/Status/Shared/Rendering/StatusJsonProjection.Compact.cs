using OpenForge.Cli.Core.Commands.Status.Models.Presentation;
using OpenForge.Cli.Core.Commands.Status.Models.Result;

namespace OpenForge.Cli.Core.Commands.Status.Shared.Rendering;

internal static partial class StatusJsonProjection
{
    internal static StatusCompactJsonResult CreateCompact(StatusResult result)
        => new()
        {
            Installation = new StatusJsonInstallation
            {
                State = StatusWireVocabulary.InstallationState(result.Facts.Installation.State),
                EntryPath = result.Facts.Installation.EntryPath,
                LoaderPath = result.Facts.Installation.LoaderPath,
            },
            Context = StatusJsonContextProjection.CreateCompact(result.Facts.Context),
            Structure = StatusJsonStructureProjection.Create(result.Facts.Structure),
            Lifecycle = StatusJsonLifecycleProjection.Create(result.Facts.Lifecycle),
            Library = StatusLibraryPresentation.Project(result.Facts.Library),
            Recovery = StatusJsonRecoveryProjection.Create(result.Facts.Recovery),
            Findings = [.. result.Findings.Select(Project)],
        };
}
