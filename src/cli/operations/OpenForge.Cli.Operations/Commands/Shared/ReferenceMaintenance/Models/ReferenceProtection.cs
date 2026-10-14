using OpenForge.Cli.Core.Commands.Shared.RoutedMaintenance.Metadata.Models;
using OpenForge.Cli.Core.Framework.Ownership.Models.Document;

namespace OpenForge.Cli.Core.Commands.Shared.ReferenceMaintenance.Models;

internal sealed record ReferenceProtection(RoutedMetadataEligibility Protection, OwnedPath? Owner);
