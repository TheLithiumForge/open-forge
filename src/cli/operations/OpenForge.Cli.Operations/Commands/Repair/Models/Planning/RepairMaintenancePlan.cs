using System.Collections.Immutable;
using OpenForge.Cli.Core.Commands.Shared.RoutedMaintenance.Models;
using OpenForge.Cli.Core.Framework.GeneratedNavigation.Models;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Files;

namespace OpenForge.Cli.Core.Commands.Repair.Models.Planning;

internal sealed record RepairMaintenancePlan
{
    public required RoutedMaintenanceIntent Intent { get; init; }
    public required GeneratedNavigationProjection Projection { get; init; }
    public required RoutedMaintenanceEffects Effects { get; init; }
    public required ImmutableArray<FileExpectation> PolicyExpectations { get; init; }
}
