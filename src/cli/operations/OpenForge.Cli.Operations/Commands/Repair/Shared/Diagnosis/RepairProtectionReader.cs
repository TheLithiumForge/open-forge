using OpenForge.Cli.Core.Framework.Settings.Models.Observation;
using OpenForge.Cli.Core.Commands.Shared.ReferenceMaintenance;
using OpenForge.Cli.Core.Commands.Shared.ReferenceMaintenance.Models;
using OpenForge.Cli.Core.Commands.Repair.Models.Result;
using OpenForge.Cli.Core.Commands.Shared.RoutedMaintenance.Metadata.Models;
using OpenForge.Cli.Core.Framework.Ownership.Models.Document;
using OpenForge.Cli.Core.Framework.Sources.Models.Locations;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Operation;

namespace OpenForge.Cli.Core.Commands.Repair.Shared.Diagnosis;

internal static class RepairProtectionReader
{
    internal static ReferenceProtectionRead Read(WorkspaceLayoutRead observation)
        => ReferenceProtectionReader.Read(observation);

    internal static bool Allows(string path, RoutedMetadataPolicyContext policy)
        => policy.Layout is { } layout && (layout.IsImplicitContentDestination(path)
                || layout.TryGetMaintenanceBoundary(path, out var boundary)
                && boundary.Kind == OpenForge.Cli.Core.Framework.Workspace.Models.Layout.WorkspaceMaintenanceBoundaryKind.Outside
                && policy.AvailableOutsideBoundary(boundary) is not null)
            && ReferenceProtectionReader.Allows(path, policy);

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
