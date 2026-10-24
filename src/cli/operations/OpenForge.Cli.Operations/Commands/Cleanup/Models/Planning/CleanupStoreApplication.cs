using OpenForge.Cli.Core.Commands.Cleanup.Shared.Application;
using OpenForge.Cli.Core.Framework.Recovery.Models.Storage;

namespace OpenForge.Cli.Core.Commands.Cleanup.Models.Planning;

internal sealed record CleanupStoreApplication(RecoveryStoreObservation Observation, CleanupResultBuilder Result);
