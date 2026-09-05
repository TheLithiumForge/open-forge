using OpenForge.Cli.Core.Commands.Status.Models.Presentation;
using OpenForge.Cli.Core.Commands.Status.Models.Result;

namespace OpenForge.Cli.Core.Commands.Status.Shared.Rendering;

internal static class StatusJsonStructureProjection
{
    internal static StatusJsonStructure Create(StatusStructure structure)
        => new()
        {
            RootCategories = new StatusJsonRootCategories
            {
                Count = StatusJsonContextProjection.Value(structure.RootCategories.Count),
                Added = structure.RootCategories.Added.ToArray(),
                Removed = structure.RootCategories.Removed.ToArray(),
            },
            GeneratedNavigation = structure.GeneratedNavigation.Select(target => new StatusJsonGeneratedNavigation
            {
                Path = target.Path,
                State = StatusWireVocabulary.GeneratedNavigationState(target.State),
            }).ToArray(),
        };
}
