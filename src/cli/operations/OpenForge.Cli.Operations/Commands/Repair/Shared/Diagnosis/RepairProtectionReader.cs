using OpenForge.Cli.Core.Commands.Shared.ReferenceMaintenance;
using OpenForge.Cli.Core.Commands.Shared.ReferenceMaintenance.Models;
using OpenForge.Cli.Core.Commands.Repair.Models.Result;
using OpenForge.Cli.Core.Commands.Shared.RoutedMaintenance.Metadata.Models;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Files;
using OpenForge.Cli.Core.Framework.Ownership.Models.Document;
using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;
using OpenForge.Cli.Core.Framework.Sources.Models.Locations;
using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Operation;

namespace OpenForge.Cli.Core.Commands.Repair.Shared.Diagnosis;

internal sealed class RepairProtectionReader(PhysicalPathResolver paths)
{
    internal ValueTask<ReferenceProtectionRead> ReadAsync(CliWorkspace workspace, CancellationToken token)
        => new ReferenceProtectionReader(paths).ReadAsync(workspace, token);

    internal static bool Allows(string path, RoutedMetadataPolicyContext policy)
        => ReferenceProtectionReader.Allows(path, policy);

    internal static RepairFinding Finding(string source, SourceLocation? occurrence, string protectedPath, RoutedMetadataPolicyContext policy)
    {
        var observed = ReferenceProtectionReader.Read(protectedPath, policy);
        var owner = observed.Owner;
        var protection = observed.Protection;
        CliNextAction? action = null;
        var reason = $"{protectedPath} is protected by {protection}.";
        if (owner is not null)
        {
            var command = owner.Manager == OwnedPathManager.Framework ? "open-forge update" : $"open-forge extension update {owner.Owner}";
            action = new(command, "The recorded manager owns this content.");
        }
        var kind = protection switch
        {
            RoutedMetadataEligibility.SettingsExcluded or RoutedMetadataEligibility.PrivateContent => RepairProtectionKind.PrivateOrExcluded,
            RoutedMetadataEligibility.UnsupportedForm => RepairProtectionKind.UnsupportedForm,
            _ => RepairProtectionKind.Protected,
        };
        return new(RepairFindingCode.ManualFindingRemaining, reason, source, occurrence)
        { Protection = true, ProtectionKind = kind, ProtectedPath = protectedPath, OwnerAction = action };
    }
}
