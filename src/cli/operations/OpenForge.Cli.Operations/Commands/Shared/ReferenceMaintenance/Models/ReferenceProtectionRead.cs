using OpenForge.Cli.Core.Commands.Shared.RoutedMaintenance.Metadata.Models;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Files;

namespace OpenForge.Cli.Core.Commands.Shared.ReferenceMaintenance.Models;

internal sealed record ReferenceProtectionRead(RoutedMetadataPolicyContext? Policy, IReadOnlyList<FileExpectation> Expectations);
