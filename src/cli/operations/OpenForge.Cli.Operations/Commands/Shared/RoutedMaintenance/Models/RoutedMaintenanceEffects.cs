using System.Collections.Immutable;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Files;
using OpenForge.Cli.Core.Framework.Recovery.Models.Preparation;

namespace OpenForge.Cli.Core.Commands.Shared.RoutedMaintenance.Models;

internal sealed record RoutedMaintenanceEffects(ImmutableArray<PlannedFileChange> Changes, ImmutableArray<RecoveryBundleTarget> RecoveryTargets);
