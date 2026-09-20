using System.Globalization;
using System.Text.Json;
using OpenForge.Cli.Core.Commands.Doctor.Models.Result;
using OpenForge.Cli.Core.Presentation.Shared.Models;
using OpenForge.Cli.Core.Presentation.Shared.Rendering;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Operation;

namespace OpenForge.Cli.Core.Presentation.Doctor.Shared.Wording;

internal static class DoctorWording
{
    internal static string MachineCode(DoctorFindingKind kind)
    {
        var value = JsonNamingPolicy.KebabCaseLower.ConvertName(kind.ToString());
        var separator = value.IndexOf('-');
        return separator < 0
            ? value
            : string.Create(
                CultureInfo.InvariantCulture,
                $"{value[..separator]}.{value[(separator + 1)..]}");
    }

    internal static string Category(DoctorFindingKind kind)
        => kind.ToString() switch
        {
            var value when value.StartsWith("Workspace", StringComparison.Ordinal)
                || value.StartsWith("Library", StringComparison.Ordinal) => global::OpenForge.Cli.OutputText.Shared.SharedText.TitleWorkspace(),
            var value when value.StartsWith("Recovery", StringComparison.Ordinal) => global::OpenForge.Cli.OutputText.Shared.SharedText.TitleRecoveryData(),
            var value when value.StartsWith("Route", StringComparison.Ordinal) => global::OpenForge.Cli.OutputText.Doctor.DoctorText.TitleRoutesAndEntries(),
            var value when value.StartsWith("Reference", StringComparison.Ordinal) => global::OpenForge.Cli.OutputText.Shared.SharedText.HelpHeadingLinks(),
            var value when value.StartsWith("Framework", StringComparison.Ordinal) => global::OpenForge.Cli.OutputText.Shared.SharedText.TitleFrameworkFiles(),
            var value when value.StartsWith("Extension", StringComparison.Ordinal) => global::OpenForge.Cli.OutputText.Shared.SharedText.TitleExtensions(),
            _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, "The Doctor finding category is not defined."),
        };

    internal static string Category(DoctorDomainKind domain)
        => domain switch
        {
            DoctorDomainKind.WorkspaceEntry => global::OpenForge.Cli.OutputText.Shared.SharedText.TitleWorkspace(),
            DoctorDomainKind.RecoveryResiduals => global::OpenForge.Cli.OutputText.Shared.SharedText.TitleRecoveryData(),
            DoctorDomainKind.RoutesMetadataOverwritesGeneratedNavigation => global::OpenForge.Cli.OutputText.Doctor.DoctorText.TitleRoutesAndEntries(),
            DoctorDomainKind.LocalReferences => global::OpenForge.Cli.OutputText.Shared.SharedText.HelpHeadingLinks(),
            DoctorDomainKind.FrameworkLifecycle => global::OpenForge.Cli.OutputText.Shared.SharedText.TitleFrameworkFiles(),
            DoctorDomainKind.ExtensionLifecycle => global::OpenForge.Cli.OutputText.Shared.SharedText.TitleExtensions(),
            _ => throw new ArgumentOutOfRangeException(nameof(domain), domain, "The Doctor domain category is not defined."),
        };

    internal static string Coverage(DoctorCoverageState coverage)
        => CliReportVocabulary.Name(coverage);

    internal static string Boundary(DoctorBoundaryKind boundary)
        => CliReportVocabulary.Name(boundary);

    internal static string Provenance(DoctorProvenanceSource source)
        => source switch
        {
            DoctorProvenanceSource.WorkspaceEntry => global::OpenForge.Cli.OutputText.Doctor.DoctorText.LabelWorkspaceFiles(),
            DoctorProvenanceSource.RecoveryResiduals => global::OpenForge.Cli.OutputText.Doctor.DoctorText.LabelRecoveryRecords(),
            DoctorProvenanceSource.RouteInventory => global::OpenForge.Cli.OutputText.Doctor.DoctorText.LabelRouteScan(),
            DoctorProvenanceSource.RouteMetadata => global::OpenForge.Cli.OutputText.Doctor.DoctorText.LabelRouteMetadata(),
            DoctorProvenanceSource.GeneratedNavigation => global::OpenForge.Cli.OutputText.Doctor.DoctorText.LabelGeneratedNavigation(),
            DoctorProvenanceSource.LocalReferences => global::OpenForge.Cli.OutputText.Doctor.DoctorText.LabelLocalLinks(),
            DoctorProvenanceSource.FrameworkLifecycle => global::OpenForge.Cli.OutputText.Doctor.DoctorText.TitleFrameworkInstallationRecord(),
            DoctorProvenanceSource.FrameworkPayload => global::OpenForge.Cli.OutputText.Doctor.DoctorText.LabelDistributedFrameworkContent(),
            DoctorProvenanceSource.ExtensionLifecycle => global::OpenForge.Cli.OutputText.Doctor.DoctorText.TitleExtensionInstallationRecord(),
            DoctorProvenanceSource.ExtensionSource => global::OpenForge.Cli.OutputText.Doctor.DoctorText.TitleExtensionSource(),
            DoctorProvenanceSource.LifecycleOwnership => global::OpenForge.Cli.OutputText.Doctor.DoctorText.LabelFileOwnershipRecords(),
            _ => throw new ArgumentOutOfRangeException(nameof(source), source, "The Doctor provenance source is not defined."),
        };

    internal static string SubjectKind(DoctorSubjectKind kind)
        => kind switch
        {
            DoctorSubjectKind.Workspace => "workspace",
            DoctorSubjectKind.Path => "file",
            DoctorSubjectKind.Route => "source",
            DoctorSubjectKind.GeneratedRegion => "file",
            DoctorSubjectKind.SourceOccurrence => "source",
            DoctorSubjectKind.Target => "file",
            DoctorSubjectKind.RecoveryItem => "file",
            DoctorSubjectKind.ManagedFile => "file",
            DoctorSubjectKind.Extension => "identifier",
            DoctorSubjectKind.Dependency => "identifier",
            DoctorSubjectKind.Library or DoctorSubjectKind.LibraryMapping => "identifier",
            DoctorSubjectKind.LibrarySourceRoot => "directory",
            DoctorSubjectKind.LibraryProjection or DoctorSubjectKind.LibraryResidual => "file",
            _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, "The Doctor subject kind is not defined."),
        };

    internal static string SubjectFallback(DoctorFindingKind kind)
        => kind switch
        {
            DoctorFindingKind.FrameworkLifecycleEvidenceUnavailable => ".agents/open-forge.lock.json",
            DoctorFindingKind.FrameworkDistributedPayloadDefect => "bundled Framework",
            DoctorFindingKind.FrameworkInstallAbsent => "workspace",
            DoctorFindingKind.FrameworkPartialLifecycle => "Framework files",
            DoctorFindingKind.ExtensionPartialLifecycle => "installed Extensions",
            _ => "workspace",
        };

    internal static string Title(DoctorFindingKind kind)
        => kind switch
        {
            DoctorFindingKind.WorkspaceUnavailable => global::OpenForge.Cli.OutputText.Doctor.DoctorText.TitleWorkspaceCannotBeRead(),
            DoctorFindingKind.WorkspaceNotDirectory => global::OpenForge.Cli.OutputText.Shared.SharedText.TitleWorkspaceIsNotADirectory(),
            DoctorFindingKind.WorkspaceAgentsMissing => global::OpenForge.Cli.OutputText.Doctor.DoctorText.TitleTheAgentsFolderIsMissing(),
            DoctorFindingKind.WorkspaceAgentsInaccessible => global::OpenForge.Cli.OutputText.Doctor.DoctorText.TitleTheAgentsFolderCannotBeRead(),
            DoctorFindingKind.WorkspaceLoaderMissing => global::OpenForge.Cli.OutputText.Doctor.DoctorText.TitleLoaderIsMissing(),
            DoctorFindingKind.WorkspaceLoaderUnreadable => global::OpenForge.Cli.OutputText.Doctor.DoctorText.TitleLoaderCannotBeRead(),
            DoctorFindingKind.WorkspaceLoaderMalformed => global::OpenForge.Cli.OutputText.Doctor.DoctorText.TitleLoaderHasInvalidContent(),
            DoctorFindingKind.WorkspaceEntryMissing => global::OpenForge.Cli.OutputText.Doctor.DoctorText.TitleEntrypointIsMissing(),
            DoctorFindingKind.WorkspaceEntryAmbiguous => global::OpenForge.Cli.OutputText.Doctor.DoctorText.TitleSeveralEntrypointsMatch(),
            DoctorFindingKind.WorkspaceEntryCompatibilityCollision => global::OpenForge.Cli.OutputText.Doctor.DoctorText.TitleEntrypointNamesConflict(),
            DoctorFindingKind.WorkspaceSourceIdCollision => global::OpenForge.Cli.OutputText.Doctor.DoctorText.TitleSourceIdsConflict(),
            DoctorFindingKind.WorkspacePathInvalid => global::OpenForge.Cli.OutputText.Doctor.DoctorText.TitlePathIsInvalid(),
            DoctorFindingKind.WorkspacePathContainment => global::OpenForge.Cli.OutputText.Doctor.DoctorText.TitlePathIsOutsideTheWorkspace(),
            DoctorFindingKind.WorkspacePhysicalAlias => global::OpenForge.Cli.OutputText.Doctor.DoctorText.TitlePathIdentityIsAmbiguous(),
            DoctorFindingKind.WorkspaceFrontmatterMalformed => global::OpenForge.Cli.OutputText.Shared.SharedText.TitleFrontmatterIsInvalid(),
            DoctorFindingKind.WorkspaceFrontmatterDuplicate => global::OpenForge.Cli.OutputText.Doctor.DoctorText.TitleFrontmatterContainsDuplicateFields(),
            DoctorFindingKind.WorkspaceParseIncomplete => global::OpenForge.Cli.OutputText.Doctor.DoctorText.TitleSourceCouldNotBeReadCompletely(),
            DoctorFindingKind.WorkspaceUnsupportedSource => global::OpenForge.Cli.OutputText.Doctor.DoctorText.TitleSourceTypeIsUnsupported(),
            DoctorFindingKind.WorkspaceRootMissing => global::OpenForge.Cli.OutputText.Doctor.DoctorText.TitleRootRouteIsMissing(),
            DoctorFindingKind.WorkspaceRootUnreachable => global::OpenForge.Cli.OutputText.Doctor.DoctorText.TitleRootRouteCannotBeReached(),
            DoctorFindingKind.WorkspaceDetached => global::OpenForge.Cli.OutputText.Doctor.DoctorText.TitleSourceIsOutsideTheLoadedRoutes(),
            DoctorFindingKind.LibraryOwnershipObservation => global::OpenForge.Cli.OutputText.Doctor.DoctorText.TitleNoOwnershipRecord(),
            DoctorFindingKind.LibrarySourceRootInvalid => global::OpenForge.Cli.OutputText.Shared.SharedText.TitleLibrarySourceFolderIsInvalid(),
            DoctorFindingKind.LibrarySourceRootAliased => global::OpenForge.Cli.OutputText.Doctor.DoctorText.TitleLibrarySourceRootHasAnAmbiguousPath(),
            DoctorFindingKind.LibraryInventoryIncomplete => global::OpenForge.Cli.OutputText.Doctor.DoctorText.TitleLibrarySourceScanIsIncomplete(),
            DoctorFindingKind.LibraryProjectionMissing => global::OpenForge.Cli.OutputText.Doctor.DoctorText.TitleRegisteredLibraryLinkIsMissing(),
            DoctorFindingKind.LibraryProjectionDangling => global::OpenForge.Cli.OutputText.Doctor.DoctorText.TitleLibraryLinkTargetIsMissing(),
            DoctorFindingKind.LibraryProjectionRetargeted => global::OpenForge.Cli.OutputText.Doctor.DoctorText.TitleLibraryLinkTargetChanged(),
            DoctorFindingKind.LibraryPathCollision => global::OpenForge.Cli.OutputText.Doctor.DoctorText.TitleLibraryDestinationIsOccupied(),
            DoctorFindingKind.LibraryLinkCapabilityUnsupported => global::OpenForge.Cli.OutputText.Doctor.DoctorText.TitleRequiredLibraryLinksAreUnsupported(),
            DoctorFindingKind.LibraryExtensionCollision => global::OpenForge.Cli.OutputText.Doctor.DoctorText.TitleLibraryAndExtensionPathsConflict(),
            DoctorFindingKind.LibraryRecoverySafeExact => global::OpenForge.Cli.OutputText.Doctor.DoctorText.TitleVerifiedLibraryRecoveryIsAvailable(),
            DoctorFindingKind.RecoveryBundleRecognized => global::OpenForge.Cli.OutputText.Shared.SharedText.TitleRecoveryBundleIsKept(),
            DoctorFindingKind.RecoveryDraftRecognized => global::OpenForge.Cli.OutputText.Doctor.DoctorText.TitleIncompleteRecoveryDraftFound(),
            DoctorFindingKind.RecoveryBundleCollision => global::OpenForge.Cli.OutputText.Shared.SharedText.TitleRecoveryBundleIsDamaged(),
            DoctorFindingKind.RecoveryProvenanceUnavailable => global::OpenForge.Cli.OutputText.Doctor.DoctorText.TitleRecoveryOriginCouldNotBeVerified(),
            DoctorFindingKind.RouteEntrypointMissing => global::OpenForge.Cli.OutputText.Doctor.DoctorText.TitleRouteEntrypointIsMissing(),
            DoctorFindingKind.RouteEntrypointDuplicate => global::OpenForge.Cli.OutputText.Doctor.DoctorText.TitleRouteHasSeveralEntrypoints(),
            DoctorFindingKind.RouteEscape => global::OpenForge.Cli.OutputText.Doctor.DoctorText.TitleRouteLeavesItsAllowedBoundary(),
            DoctorFindingKind.RouteUnreachable => global::OpenForge.Cli.OutputText.Doctor.DoctorText.TitleRouteCannotBeReached(),
            DoctorFindingKind.RouteDetached => global::OpenForge.Cli.OutputText.Doctor.DoctorText.TitleSourceIsOutsideTheLoadedRoutes(),
            DoctorFindingKind.RouteMetadataRequiredMissing => global::OpenForge.Cli.OutputText.Doctor.DoctorText.TitleRequiredRouteMetadataIsMissing(),
            DoctorFindingKind.RouteTitleInvalid => global::OpenForge.Cli.OutputText.Doctor.DoctorText.TitleRouteTitleIsInvalid(),
            DoctorFindingKind.RouteAxiomsInvalid => global::OpenForge.Cli.OutputText.Doctor.DoctorText.TitleRouteAxiomsAreInvalid(),
            DoctorFindingKind.RouteGeneratedRegionStale => global::OpenForge.Cli.OutputText.Shared.SharedText.TitleEntriesSectionIsStale(),
            DoctorFindingKind.RouteGeneratedRegionMissing => global::OpenForge.Cli.OutputText.Shared.SharedText.TitleEntriesSectionIsMissing(),
            DoctorFindingKind.RouteGeneratedRegionMalformed => global::OpenForge.Cli.OutputText.Doctor.DoctorText.TitleEntriesSectionIsMalformed(),
            DoctorFindingKind.RouteGeneratedRegionMisplaced => global::OpenForge.Cli.OutputText.Doctor.DoctorText.TitleEntriesSectionIsNotLast(),
            DoctorFindingKind.RouteGeneratedRegionDuplicate => global::OpenForge.Cli.OutputText.Doctor.DoctorText.TitleMoreThanOneEntriesSection(),
            DoctorFindingKind.RouteGeneratedEntryMissing => global::OpenForge.Cli.OutputText.Doctor.DoctorText.TitleEntryIsMissing(),
            DoctorFindingKind.RouteGeneratedEntryExtra => global::OpenForge.Cli.OutputText.Doctor.DoctorText.TitleEntryHasNoFile(),
            DoctorFindingKind.RouteGeneratedEntryOrder => global::OpenForge.Cli.OutputText.Doctor.DoctorText.TitleEntriesAreOutOfOrder(),
            DoctorFindingKind.RouteGeneratedEntryPath => global::OpenForge.Cli.OutputText.Doctor.DoctorText.TitleEntryPathIsWrong(),
            DoctorFindingKind.RouteGeneratedEntryDescription => global::OpenForge.Cli.OutputText.Doctor.DoctorText.TitleEntryDescriptionIsStale(),
            DoctorFindingKind.RouteGeneratedEntryTags => global::OpenForge.Cli.OutputText.Doctor.DoctorText.TitleEntryTagsAreStale(),
            DoctorFindingKind.RouteOverwriteOrphan => global::OpenForge.Cli.OutputText.Shared.SharedText.TitleOverwriteHasNoBaseFile(),
            DoctorFindingKind.RouteOverwriteIndependentIndex => global::OpenForge.Cli.OutputText.Doctor.DoctorText.TitleOverwriteIsListedIndependently(),
            DoctorFindingKind.RouteCompatibilityConflict => global::OpenForge.Cli.OutputText.Doctor.DoctorText.TitleRouteNamesConflict(),
            DoctorFindingKind.ReferenceTargetMissing => global::OpenForge.Cli.OutputText.Shared.SharedText.TitleBrokenLink(),
            DoctorFindingKind.ReferenceFragmentMissing => global::OpenForge.Cli.OutputText.Doctor.DoctorText.TitleLinkedHeadingWasNotFound(),
            DoctorFindingKind.ReferenceFragmentUnverified => global::OpenForge.Cli.OutputText.Doctor.DoctorText.TitleLinkedHeadingCouldNotBeChecked(),
            DoctorFindingKind.ReferenceDestinationMalformed => global::OpenForge.Cli.OutputText.Doctor.DoctorText.TitleLinkDestinationIsInvalid(),
            DoctorFindingKind.ReferenceDestinationAbsolute => global::OpenForge.Cli.OutputText.Doctor.DoctorText.TitleAbsoluteLocalLinkIsUnsupported(),
            DoctorFindingKind.ReferenceDestinationQuery => global::OpenForge.Cli.OutputText.Doctor.DoctorText.TitleLocalLinkQueryIsUnsupported(),
            DoctorFindingKind.ReferenceDestinationEncoding => global::OpenForge.Cli.OutputText.Doctor.DoctorText.TitleLinkEncodingIsUnsupported(),
            DoctorFindingKind.ReferenceTargetOutsideWorkspace => global::OpenForge.Cli.OutputText.Doctor.DoctorText.TitleLinkLeavesTheWorkspace(),
            DoctorFindingKind.ReferenceTargetPhysicalEscape => global::OpenForge.Cli.OutputText.Doctor.DoctorText.TitleLinkResolvesOutsideTheWorkspace(),
            DoctorFindingKind.ReferenceTargetAlias => global::OpenForge.Cli.OutputText.Doctor.DoctorText.TitleLinkTargetIdentityIsAmbiguous(),
            DoctorFindingKind.ReferenceTargetUnreadable => global::OpenForge.Cli.OutputText.Doctor.DoctorText.TitleLinkTargetCannotBeRead(),
            DoctorFindingKind.ReferenceTargetUnsupported => global::OpenForge.Cli.OutputText.Doctor.DoctorText.TitleLinkTargetTypeIsUnsupported(),
            DoctorFindingKind.ReferenceSameTargetPath => global::OpenForge.Cli.OutputText.Doctor.DoctorText.TitleEquivalentLinkPathIsAvailable(),
            DoctorFindingKind.ReferenceSameTargetCase => global::OpenForge.Cli.OutputText.Doctor.DoctorText.TitleEquivalentLinkLetterCaseIsAvailable(),
            DoctorFindingKind.ReferenceSameTargetEncoding => global::OpenForge.Cli.OutputText.Doctor.DoctorText.TitleEquivalentLinkEncodingIsAvailable(),
            DoctorFindingKind.ReferenceSameTargetFragment => global::OpenForge.Cli.OutputText.Doctor.DoctorText.TitleEquivalentHeadingLinkIsAvailable(),
            DoctorFindingKind.FrameworkInstallAbsent => global::OpenForge.Cli.OutputText.Doctor.DoctorText.TitleFrameworkIsNotInstalled(),
            DoctorFindingKind.FrameworkManagedMissing => global::OpenForge.Cli.OutputText.Doctor.DoctorText.TitleFrameworkFileIsMissing(),
            DoctorFindingKind.FrameworkManagedChanged => global::OpenForge.Cli.OutputText.Doctor.DoctorText.TitleFrameworkFileChanged(),
            DoctorFindingKind.FrameworkLifecycleEvidenceUnavailable => global::OpenForge.Cli.OutputText.Doctor.DoctorText.TitleOwnershipRecordCannotBeRead(),
            DoctorFindingKind.FrameworkOwnershipObservation => global::OpenForge.Cli.OutputText.Doctor.DoctorText.TitleNoOwnershipRecord(),
            DoctorFindingKind.FrameworkBridgeBoundary => global::OpenForge.Cli.OutputText.Doctor.DoctorText.TitleOpenForgeSectionInAgentsMdNeedsReview(),
            DoctorFindingKind.FrameworkRootRegionBoundary => global::OpenForge.Cli.OutputText.Doctor.DoctorText.TitleOpenForgeSectionBoundaryIsUnclear(),
            DoctorFindingKind.FrameworkOwnershipConflict => global::OpenForge.Cli.OutputText.Doctor.DoctorText.TitleFrameworkFileOwnershipConflicts(),
            DoctorFindingKind.FrameworkPartialLifecycle => global::OpenForge.Cli.OutputText.Doctor.DoctorText.TitleFrameworkUpdateDidNotFinish(),
            DoctorFindingKind.FrameworkPartialRecovery => global::OpenForge.Cli.OutputText.Doctor.DoctorText.TitleFrameworkRecoveryIsIncomplete(),
            DoctorFindingKind.FrameworkDistributedPayloadDefect => global::OpenForge.Cli.OutputText.Doctor.DoctorText.TitleBundledFrameworkIsInvalid(),
            DoctorFindingKind.ExtensionOwnershipObservation => global::OpenForge.Cli.OutputText.Doctor.DoctorText.TitleNoOwnershipRecord(),
            DoctorFindingKind.ExtensionManifestMissing => global::OpenForge.Cli.OutputText.Doctor.DoctorText.TitleExtensionManifestIsMissing(),
            DoctorFindingKind.ExtensionManifestMalformed => global::OpenForge.Cli.OutputText.Doctor.DoctorText.TitleExtensionManifestIsInvalid(),
            DoctorFindingKind.ExtensionDuplicateId => global::OpenForge.Cli.OutputText.Doctor.DoctorText.TitleExtensionIdIsDuplicated(),
            DoctorFindingKind.ExtensionUnknownId => global::OpenForge.Cli.OutputText.Doctor.DoctorText.TitleExtensionIdIsUnknown(),
            DoctorFindingKind.ExtensionVersionInvalid => global::OpenForge.Cli.OutputText.Doctor.DoctorText.TitleExtensionVersionIsInvalid(),
            DoctorFindingKind.ExtensionManagedMissing => global::OpenForge.Cli.OutputText.Doctor.DoctorText.TitleExtensionFileIsMissing(),
            DoctorFindingKind.ExtensionManagedChanged => global::OpenForge.Cli.OutputText.Doctor.DoctorText.TitleExtensionFileChanged(),
            DoctorFindingKind.ExtensionDependencyMissing => global::OpenForge.Cli.OutputText.Doctor.DoctorText.TitleRequiredExtensionDependencyIsMissing(),
            DoctorFindingKind.ExtensionDependencyCycle => global::OpenForge.Cli.OutputText.Doctor.DoctorText.TitleExtensionDependenciesFormACycle(),
            DoctorFindingKind.ExtensionDependencyIncompatible => global::OpenForge.Cli.OutputText.Doctor.DoctorText.TitleExtensionDependencyVersionIsIncompatible(),
            DoctorFindingKind.ExtensionSourceUnavailable => global::OpenForge.Cli.OutputText.Doctor.DoctorText.TitleExtensionSourceCannotBeRead(),
            DoctorFindingKind.ExtensionCatalogueUnavailable => global::OpenForge.Cli.OutputText.Doctor.DoctorText.TitlePackageFolderCannotBeRead(),
            DoctorFindingKind.ExtensionPartialLifecycle => global::OpenForge.Cli.OutputText.Doctor.DoctorText.TitleExtensionUpdateDidNotFinish(),
            DoctorFindingKind.ExtensionOwnershipCollision => global::OpenForge.Cli.OutputText.Doctor.DoctorText.TitleExtensionFileOwnershipConflicts(),
            DoctorFindingKind.ExtensionBridgeRegistration => global::OpenForge.Cli.OutputText.Doctor.DoctorText.TitleExtensionBridgeRegistrationNeedsReview(),
            _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, "The Doctor finding kind is not defined."),
        };

    internal static string Message(DoctorFinding finding)
    {
        ArgumentNullException.ThrowIfNull(finding);
        var path = Path(finding);
        return finding.Kind switch
        {
            DoctorFindingKind.WorkspaceUnavailable => global::OpenForge.Cli.OutputText.Doctor.DoctorPhrases.FormatDoesNotExistOrCannotBeRead($"{path}"),
            DoctorFindingKind.WorkspaceNotDirectory => global::OpenForge.Cli.OutputText.Doctor.DoctorPhrases.FormatIsAFileNotADirectory($"{path}"),
            DoctorFindingKind.WorkspaceAgentsMissing => global::OpenForge.Cli.OutputText.Doctor.DoctorPhrases.FormatHasNoAgentsFolderOpenForgeIsNotInstalledHere($"{path}"),
            DoctorFindingKind.WorkspaceAgentsInaccessible => global::OpenForge.Cli.OutputText.Doctor.DoctorPhrases.FormatAgentsExistsButCannotBeRead($"{Cause(finding, "the reason is unavailable")}"),
            DoctorFindingKind.WorkspaceLoaderMissing => global::OpenForge.Cli.OutputText.Doctor.DoctorText.MessageAgentsLoaderMdIsMissing(),
            DoctorFindingKind.WorkspaceLoaderUnreadable => global::OpenForge.Cli.OutputText.Doctor.DoctorPhrases.FormatAgentsLoaderMdCannotBeRead($"{Cause(finding, "the reason is unavailable")}"),
            DoctorFindingKind.WorkspaceLoaderMalformed => global::OpenForge.Cli.OutputText.Doctor.DoctorPhrases.FormatAgentsLoaderMdCouldNotBeUnderstood($"{Cause(finding, "the content is invalid")}"),
            DoctorFindingKind.WorkspaceEntryMissing => global::OpenForge.Cli.OutputText.Doctor.DoctorPhrases.FormatIsRoutedButHasNoEntrypointFile($"{path}"),
            DoctorFindingKind.WorkspaceEntryAmbiguous => global::OpenForge.Cli.OutputText.Doctor.DoctorPhrases.FormatHasMoreThanOneEntrypointFileKeepOne($"{path}", $"{Cause(finding, "several names")}"),
            DoctorFindingKind.WorkspaceEntryCompatibilityCollision => global::OpenForge.Cli.OutputText.Doctor.DoctorPhrases.FormatHasIncompatibleEntrypointFormsKeepOne($"{path}"),
            DoctorFindingKind.WorkspaceSourceIdCollision => global::OpenForge.Cli.OutputText.Doctor.DoctorPhrases.FormatDerivesAnIdThatIsAlsoDerivedByAnotherFile($"{path}", $"{Cause(finding, "use exact paths, or rename one")}"),
            DoctorFindingKind.WorkspacePathInvalid => global::OpenForge.Cli.OutputText.Doctor.DoctorPhrases.FormatIsNotAValidPathForASource($"{path}"),
            DoctorFindingKind.WorkspacePathContainment => global::OpenForge.Cli.OutputText.Doctor.DoctorPhrases.FormatPointsOutsideTheWorkspace($"{path}"),
            DoctorFindingKind.WorkspacePhysicalAlias => global::OpenForge.Cli.OutputText.Doctor.DoctorPhrases.FormatHasAnAmbiguousPhysicalIdentity($"{path}", $"{Cause(finding, "the physical identity could not be distinguished")}"),
            DoctorFindingKind.WorkspaceFrontmatterMalformed => EnsureSentence(finding.Message),
            DoctorFindingKind.WorkspaceFrontmatterDuplicate => EnsureSentence(finding.Message),
            DoctorFindingKind.WorkspaceParseIncomplete => global::OpenForge.Cli.OutputText.Doctor.DoctorPhrases.FormatCouldNotBeParsedCompletelyItsChecksAreIncomplete($"{path}", $"{Cause(finding, "the source could not be read")}"),
            DoctorFindingKind.WorkspaceUnsupportedSource => global::OpenForge.Cli.OutputText.Doctor.DoctorPhrases.FormatIsNotAKindOfFileOpenForgeChecks($"{path}"),
            DoctorFindingKind.WorkspaceRootMissing => global::OpenForge.Cli.OutputText.Doctor.DoctorPhrases.FormatTheLoaderListsButDoesNotExist($"{finding.Subject.Identifier ?? path}", $"{path}"),
            DoctorFindingKind.WorkspaceRootUnreachable => global::OpenForge.Cli.OutputText.Doctor.DoctorPhrases.FormatIsListedButCannotBeReachedFromTheLoader($"{finding.Subject.Identifier ?? path}", $"{Cause(finding, "the route is unreachable")}"),
            DoctorFindingKind.WorkspaceDetached => global::OpenForge.Cli.OutputText.Doctor.DoctorPhrases.FormatIsNotReachableFromAnyRouteSoAgentsNeverLoadIt($"{path}"),
            DoctorFindingKind.LibraryOwnershipObservation => OwnershipObservation(global::OpenForge.Cli.OutputText.Shared.SharedText.TitleLibraries()),
            DoctorFindingKind.LibrarySourceRootInvalid => LibrarySourceMessage(finding, global::OpenForge.Cli.OutputText.Doctor.DoctorText.LabelIsNotAFolderInsideTheWorkspace()),
            DoctorFindingKind.LibrarySourceRootAliased => LibrarySourceMessage(finding, global::OpenForge.Cli.OutputText.Doctor.DoctorText.LabelResolvesToAnAmbiguousLocation()),
            DoctorFindingKind.LibraryInventoryIncomplete => global::OpenForge.Cli.OutputText.Doctor.DoctorPhrases.FormatTheSourceFolderOfCouldNotBeScannedCompletely($"{LibraryId(finding)}", $"{Cause(finding, "the source could not be scanned")}"),
            DoctorFindingKind.LibraryProjectionMissing => global::OpenForge.Cli.OutputText.Doctor.DoctorPhrases.FormatALinkOfIsMissing($"{path}", $"{LibraryId(finding)}"),
            DoctorFindingKind.LibraryProjectionDangling => LibraryProjectionDangling(finding),
            DoctorFindingKind.LibraryProjectionRetargeted => LibraryProjectionRetargeted(finding),
            DoctorFindingKind.LibraryPathCollision => global::OpenForge.Cli.OutputText.Doctor.DoctorPhrases.FormatIsUsedByAndByAnotherManagedDomain($"{path}", $"{LibraryId(finding)}"),
            DoctorFindingKind.LibraryLinkCapabilityUnsupported => global::OpenForge.Cli.OutputText.Doctor.DoctorPhrases.FormatThisSystemCannotCreateTheFileLinksTheLibraryNeeds($"{LibraryId(finding)}"),
            DoctorFindingKind.LibraryExtensionCollision => global::OpenForge.Cli.OutputText.Doctor.DoctorPhrases.FormatIsClaimedByTheLibraryAndByAnExtension($"{path}", $"{LibraryId(finding)}"),
            DoctorFindingKind.LibraryRecoverySafeExact => global::OpenForge.Cli.OutputText.Doctor.DoctorPhrases.FormatAVerifiedRecoveryStepForCanRestore($"{LibraryId(finding)}", $"{path}"),
            DoctorFindingKind.RecoveryBundleRecognized => global::OpenForge.Cli.OutputText.Shared.SharedPhrases.FormatARecoveryBundleFromAnEarlierCommandIsKeptAt($"{path}"),
            DoctorFindingKind.RecoveryDraftRecognized => global::OpenForge.Cli.OutputText.Shared.SharedPhrases.FormatAnUnfinishedRecoveryDraftIsAtACommandDidNotFinish($"{path}"),
            DoctorFindingKind.RecoveryBundleCollision => global::OpenForge.Cli.OutputText.Doctor.DoctorPhrases.FormatTheRecoveryBundleAtIsDamagedItWasLeftInPlace($"{path}", $"{Cause(finding, "the records conflict")}"),
            DoctorFindingKind.RecoveryProvenanceUnavailable => global::OpenForge.Cli.OutputText.Doctor.DoctorPhrases.FormatTheRecoveryBundleAtCannotBeVerifiedSoCleanupWillNotDeleteIt($"{path}"),
            DoctorFindingKind.RouteEntrypointMissing => global::OpenForge.Cli.OutputText.Doctor.DoctorPhrases.FormatIsRoutedButHasNoEntrypointFile($"{path}"),
            DoctorFindingKind.RouteEntrypointDuplicate => global::OpenForge.Cli.OutputText.Doctor.DoctorPhrases.FormatHasMoreThanOneEntrypoint($"{path}", $"{Cause(finding, "several names")}"),
            DoctorFindingKind.RouteEscape => EnsureSentence(finding.Message),
            DoctorFindingKind.RouteUnreachable => global::OpenForge.Cli.OutputText.Doctor.DoctorPhrases.FormatIsRoutedButNoParentListsIt($"{path}"),
            DoctorFindingKind.RouteDetached => global::OpenForge.Cli.OutputText.Doctor.DoctorPhrases.FormatLooksLikeARouteButNoLoaderEntryOrParentReachesIt($"{path}"),
            DoctorFindingKind.RouteMetadataRequiredMissing => EnsureSentence(finding.Message),
            DoctorFindingKind.RouteTitleInvalid => global::OpenForge.Cli.OutputText.Doctor.DoctorPhrases.FormatHasNoLevel1Heading($"{path}"),
            DoctorFindingKind.RouteAxiomsInvalid => global::OpenForge.Cli.OutputText.Doctor.DoctorPhrases.FormatHasNoAxiomsSectionOrItsAxiomsSectionIsMalformed($"{path}"),
            DoctorFindingKind.RouteGeneratedRegionStale => global::OpenForge.Cli.OutputText.Doctor.DoctorPhrases.FormatTheEntriesSectionOfDoesNotMatchItsRoutedFiles($"{path}"),
            DoctorFindingKind.RouteGeneratedRegionMissing => global::OpenForge.Cli.OutputText.Shared.SharedPhrases.FormatHasNoEntriesSection($"{path}"),
            DoctorFindingKind.RouteGeneratedRegionMalformed => global::OpenForge.Cli.OutputText.Doctor.DoctorPhrases.FormatTheEntriesSectionOfCouldNotBeReadAsAList($"{path}"),
            DoctorFindingKind.RouteGeneratedRegionMisplaced => global::OpenForge.Cli.OutputText.Doctor.DoctorPhrases.FormatTheEntriesSectionOfIsFollowedByAnotherSection($"{path}"),
            DoctorFindingKind.RouteGeneratedRegionDuplicate => global::OpenForge.Cli.OutputText.Doctor.DoctorPhrases.FormatHasMoreThanOneEntriesSection($"{path}"),
            DoctorFindingKind.RouteGeneratedEntryMissing => global::OpenForge.Cli.OutputText.Doctor.DoctorPhrases.FormatDoesNotList($"{path}", $"{FindingIdentifier(finding)}"),
            DoctorFindingKind.RouteGeneratedEntryExtra => global::OpenForge.Cli.OutputText.Doctor.DoctorPhrases.FormatListsWhichDoesNotExist($"{path}", $"{FindingIdentifier(finding)}"),
            DoctorFindingKind.RouteGeneratedEntryOrder => global::OpenForge.Cli.OutputText.Doctor.DoctorPhrases.FormatTheEntriesInAreNotInTheExpectedOrder($"{path}"),
            DoctorFindingKind.RouteGeneratedEntryPath => global::OpenForge.Cli.OutputText.Doctor.DoctorPhrases.FormatTheEntryForInPointsTo($"{FindingIdentifier(finding)}", $"{path}", $"{ComparisonActual(finding)}"),
            DoctorFindingKind.RouteGeneratedEntryDescription => global::OpenForge.Cli.OutputText.Doctor.DoctorPhrases.FormatTheEntryForInHasAnOldDescription($"{FindingIdentifier(finding)}", $"{path}"),
            DoctorFindingKind.RouteGeneratedEntryTags => global::OpenForge.Cli.OutputText.Doctor.DoctorPhrases.FormatTheEntryForInHasOldTags($"{FindingIdentifier(finding)}", $"{path}"),
            DoctorFindingKind.RouteOverwriteOrphan => OverwriteOrphan(finding),
            DoctorFindingKind.RouteOverwriteIndependentIndex => EnsureSentence(finding.Message),
            DoctorFindingKind.RouteCompatibilityConflict => EnsureSentence(finding.Message),
            DoctorFindingKind.ReferenceTargetMissing => ReferenceTargetMissing(finding),
            DoctorFindingKind.ReferenceFragmentMissing => ReferenceFragmentMissing(finding),
            DoctorFindingKind.ReferenceFragmentUnverified => ReferenceFragmentUnverified(finding),
            DoctorFindingKind.ReferenceDestinationMalformed => global::OpenForge.Cli.OutputText.Doctor.DoctorPhrases.FormatIsNotALinkOpenForgeCanCheck($"{FindingIdentifier(finding)}"),
            DoctorFindingKind.ReferenceDestinationAbsolute => global::OpenForge.Cli.OutputText.Doctor.DoctorPhrases.FormatIsAnAbsolutePathUseARelativePath($"{FindingIdentifier(finding)}"),
            DoctorFindingKind.ReferenceDestinationQuery => global::OpenForge.Cli.OutputText.Doctor.DoctorPhrases.FormatHasAQueryStringWhichLocalLinksDoNotSupport($"{FindingIdentifier(finding)}"),
            DoctorFindingKind.ReferenceDestinationEncoding => global::OpenForge.Cli.OutputText.Doctor.DoctorPhrases.FormatUsesAnEncodingThatCannotBeResolvedSafely($"{FindingIdentifier(finding)}"),
            DoctorFindingKind.ReferenceTargetOutsideWorkspace => global::OpenForge.Cli.OutputText.Doctor.DoctorPhrases.FormatPointsOutsideTheWorkspace($"{FindingIdentifier(finding)}"),
            DoctorFindingKind.ReferenceTargetPhysicalEscape => global::OpenForge.Cli.OutputText.Doctor.DoctorPhrases.FormatResolvesOutsideTheWorkspaceThroughALink($"{FindingIdentifier(finding)}"),
            DoctorFindingKind.ReferenceTargetAlias => global::OpenForge.Cli.OutputText.Doctor.DoctorPhrases.FormatResolvesToMoreThanOneFile($"{FindingIdentifier(finding)}"),
            DoctorFindingKind.ReferenceTargetUnreadable => global::OpenForge.Cli.OutputText.Doctor.DoctorPhrases.FormatExistsButCannotBeRead($"{FindingIdentifier(finding)}", $"{Cause(finding, "the reason is unavailable")}"),
            DoctorFindingKind.ReferenceTargetUnsupported => global::OpenForge.Cli.OutputText.Doctor.DoctorPhrases.FormatIsAKindOfFileOpenForgeDoesNotCheck($"{FindingIdentifier(finding)}"),
            DoctorFindingKind.ReferenceSameTargetPath => CanonicalizationMessage(finding, global::OpenForge.Cli.OutputText.Doctor.DoctorText.LabelWorksButIsNotTheCanonicalSpelling()),
            DoctorFindingKind.ReferenceSameTargetCase => CanonicalizationMessage(finding, global::OpenForge.Cli.OutputText.Doctor.DoctorText.LabelDiffersFromTheFileSNameOnlyByLetterCase()),
            DoctorFindingKind.ReferenceSameTargetEncoding => CanonicalizationMessage(finding, global::OpenForge.Cli.OutputText.Doctor.DoctorText.LabelUsesADifferentEncodingThanTheCanonical()),
            DoctorFindingKind.ReferenceSameTargetFragment => CanonicalFragmentMessage(finding),
            DoctorFindingKind.FrameworkOwnershipObservation => OwnershipObservation(global::OpenForge.Cli.OutputText.Shared.SharedText.TitleFrameworkFiles()),
            DoctorFindingKind.ExtensionOwnershipObservation => OwnershipObservation(global::OpenForge.Cli.OutputText.Shared.SharedText.LabelInstalledExtensions()),
            DoctorFindingKind.FrameworkInstallAbsent => global::OpenForge.Cli.OutputText.Doctor.DoctorText.MessageOpenForgeIsNotInstalledInThisWorkspace(),
            DoctorFindingKind.FrameworkManagedMissing => global::OpenForge.Cli.OutputText.Doctor.DoctorPhrases.FormatIsMissingItWasInstalledByTheFramework($"{path}"),
            DoctorFindingKind.FrameworkManagedChanged => global::OpenForge.Cli.OutputText.Doctor.DoctorPhrases.FormatChangedSinceItWasInstalled($"{path}"),
            DoctorFindingKind.FrameworkLifecycleEvidenceUnavailable => global::OpenForge.Cli.OutputText.Doctor.DoctorPhrases.FormatAgentsOpenForgeLockJsonCouldNotBeRead($"{Cause(finding, "the reason is unavailable")}"),
            DoctorFindingKind.FrameworkBridgeBoundary => global::OpenForge.Cli.OutputText.Doctor.DoctorPhrases.FormatTheOpenForgeSectionInIsMissingOrChanged($"{BridgeFile(finding)}"),
            DoctorFindingKind.FrameworkRootRegionBoundary => global::OpenForge.Cli.OutputText.Doctor.DoctorPhrases.FormatTheOpenForgeSectionInHasNoClearStartOrEnd($"{path}"),
            DoctorFindingKind.FrameworkOwnershipConflict => global::OpenForge.Cli.OutputText.Doctor.DoctorPhrases.FormatIsClaimedByTheFrameworkAndByAnotherManagedDomain($"{path}"),
            DoctorFindingKind.FrameworkPartialLifecycle => global::OpenForge.Cli.OutputText.Doctor.DoctorText.MessageSomeFrameworkFilesAreCurrentAndOthersAreNotSoAnUpdateDidNotFinish(),
            DoctorFindingKind.FrameworkPartialRecovery => global::OpenForge.Cli.OutputText.Doctor.DoctorPhrases.FormatTheRecoveryBundleAtWasPartlyAppliedSomeFilesMatchTheOldContentAndSomeTheNew($"{path}"),
            DoctorFindingKind.FrameworkDistributedPayloadDefect => global::OpenForge.Cli.OutputText.Shared.SharedText.MessageTheFrameworkBundledInThisCliIsInvalid(),
            DoctorFindingKind.ExtensionManifestMissing => global::OpenForge.Cli.OutputText.Doctor.DoctorPhrases.FormatThePackageAtHasNoExtensionJson($"{path}"),
            DoctorFindingKind.ExtensionManifestMalformed => global::OpenForge.Cli.OutputText.Doctor.DoctorPhrases.FormatExtensionJsonCouldNotBeReadExpectedKeysIdNameDescriptionVersionDependencies($"{path}", $"{Cause(finding, "the manifest is invalid")}"),
            DoctorFindingKind.ExtensionDuplicateId => global::OpenForge.Cli.OutputText.Doctor.DoctorPhrases.FormatTwoPackagesInHaveTheId($"{path}", $"{FindingIdentifier(finding)}"),
            DoctorFindingKind.ExtensionUnknownId => global::OpenForge.Cli.OutputText.Doctor.DoctorPhrases.FormatTheOwnershipRecordNamesWhichIsNotInTheBundledExtensionsOrTheRecordedSource($"{FindingIdentifier(finding)}"),
            DoctorFindingKind.ExtensionVersionInvalid => EnsureSentence(finding.Message),
            DoctorFindingKind.ExtensionManagedMissing => global::OpenForge.Cli.OutputText.Doctor.DoctorPhrases.FormatIsMissingItWasInstalledBy($"{path}", $"{ExtensionId(finding)}"),
            DoctorFindingKind.ExtensionManagedChanged => global::OpenForge.Cli.OutputText.Doctor.DoctorPhrases.FormatChangedSinceItWasInstalledBy($"{path}", $"{ExtensionId(finding)}"),
            DoctorFindingKind.ExtensionDependencyMissing => EnsureSentence(finding.Message),
            DoctorFindingKind.ExtensionDependencyCycle => EnsureSentence(finding.Message),
            DoctorFindingKind.ExtensionDependencyIncompatible => EnsureSentence(finding.Message),
            DoctorFindingKind.ExtensionSourceUnavailable => ExtensionSourceUnavailable(finding),
            DoctorFindingKind.ExtensionCatalogueUnavailable => global::OpenForge.Cli.OutputText.Doctor.DoctorPhrases.FormatThePackageFolderCannotBeRead($"{path}"),
            DoctorFindingKind.ExtensionPartialLifecycle => global::OpenForge.Cli.OutputText.Doctor.DoctorPhrases.FormatSomeFilesOfAreCurrentAndOthersAreNot($"{ExtensionId(finding)}"),
            DoctorFindingKind.ExtensionOwnershipCollision => EnsureSentence(finding.Message),
            DoctorFindingKind.ExtensionBridgeRegistration => ExtensionBridgeRegistration(finding),
            _ => EnsureSentence(finding.Message),
        };
    }

    internal static IReadOnlyList<CliNextAction> Actions(DoctorFinding finding)
    {
        var actions = finding.Actions
            .Where(action => action.Command is not null)
            .Select(action => new CliNextAction(action.Command!, action.Reason))
            .ToList();
        var fallback = FallbackAction(finding);
        if (fallback is not null && !actions.Any(action => string.Equals(action.Command, fallback.Command, StringComparison.Ordinal)))
        {
            actions.Add(fallback);
        }

        return actions;
    }

    internal static string CandidateReason(DoctorCandidateBasisKind basis)
        => basis switch
        {
            DoctorCandidateBasisKind.Filename => global::OpenForge.Cli.OutputText.Shared.SharedText.LabelFilenameMatch(),
            DoctorCandidateBasisKind.Title => global::OpenForge.Cli.OutputText.Shared.SharedText.LabelTitleMatch(),
            DoctorCandidateBasisKind.LiteralContent => global::OpenForge.Cli.OutputText.Shared.SharedText.LabelContentMatch(),
            DoctorCandidateBasisKind.RouteNeighborhood => global::OpenForge.Cli.OutputText.Shared.SharedText.LabelNearbyRoute(),
            _ => throw new ArgumentOutOfRangeException(nameof(basis), basis, "The candidate basis is not defined."),
        };

    internal static string CoverageSummary(DoctorCoverageCounts counts)
    {
        var parts = new List<string>();
        if (counts.LinksValid is { } links && counts.RoutesChecked is { } routesChecked && counts.ExternalLinksNotChecked is not (> 0))
        {
            return global::OpenForge.Cli.OutputText.Doctor.DoctorPhrases.FormatAndChecked($"{links}", $"{CliTextPlural(links, "link")}", $"{routesChecked}", $"{CliTextPlural(routesChecked, "route")}");
        }

        if (counts.LinksValid is { } linksValid)
        {
            parts.Add(global::OpenForge.Cli.OutputText.Doctor.DoctorPhrases.FormatValid($"{linksValid}", $"{CliTextPlural(linksValid, "link")}"));
        }

        if (counts.ExternalLinksNotChecked is { } external && external > 0)
        {
            parts.Add(global::OpenForge.Cli.OutputText.Shared.SharedPhrases.FormatNotChecked($"{external}", $"{CliTextPlural(external, "external link")}"));
        }

        if (counts.RoutesChecked is { } routes)
        {
            parts.Add(global::OpenForge.Cli.OutputText.Shared.SharedPhrases.FormatChecked($"{routes}", $"{CliTextPlural(routes, "route")}"));
        }

        return parts.Count == 0 ? string.Empty : string.Join(", ", parts) + ".";
    }

    internal static string ChecksSummary(DoctorCoverageCounts counts)
        => counts.Checks is { } checks && counts.ChecksComplete is { } complete
            ? complete == checks
                ? global::OpenForge.Cli.OutputText.Doctor.DoctorPhrases.FormatChecksComplete($"{complete}")
                : global::OpenForge.Cli.OutputText.Doctor.DoctorPhrases.FormatOfChecksComplete($"{complete}", $"{checks}")
            : string.Empty;

    internal static string IncompleteCheckSummary(DoctorCoverageCounts counts)
    {
        if (counts.Checks is not { } checks || counts.ChecksComplete is not { } complete || complete >= checks)
        {
            return string.Empty;
        }

        var remaining = checks - complete;
        return global::OpenForge.Cli.OutputText.Doctor.DoctorPhrases.FormatCouldNotFinish($"{remaining}", $"{CliTextPlural(remaining, "check")}");
    }

    internal static string CountFindings(long errors, long warnings, long infos)
    {
        if (errors > 0)
        {
            var parts = new List<string> { Parts(errors, global::OpenForge.Cli.OutputText.Shared.SharedText.LabelError()) };
            if (warnings > 0)
            {
                parts.Add(Parts(warnings, global::OpenForge.Cli.OutputText.Shared.SharedText.LabelWarning()));
            }

            if (infos > 0)
            {
                parts.Add(global::OpenForge.Cli.OutputText.Doctor.DoctorPhrases.FormatFindings($"{Parts(infos, "info")}"));
            }

            return JoinWithAnd(parts) + ".";
        }

        if (warnings > 0)
        {
            if (infos > 0)
            {
                return global::OpenForge.Cli.OutputText.Doctor.DoctorPhrases.FormatNoErrorsAnd($"{Parts(warnings, "warning")}", $"{Recorded(infos)}");
            }

            return global::OpenForge.Cli.OutputText.Doctor.DoctorPhrases.FormatNoErrorsRecorded($"{Parts(warnings, "warning")}", $"{(warnings == 1 ? "was" : "were")}");
        }

        return infos > 0
            ? global::OpenForge.Cli.OutputText.Doctor.DoctorPhrases.FormatNoProblemsFound($"{Recorded(infos)}")
            : global::OpenForge.Cli.OutputText.Doctor.DoctorText.MessageNoProblemsFound();
    }

    internal static string Hint(long warnings, long infos, CliDetail detail)
        => detail switch
        {
            CliDetail.Minimal when warnings > 0 => global::OpenForge.Cli.OutputText.Doctor.DoctorText.TitleToListTheWarningsOpenForgeDoctorDetailStandard(),
            CliDetail.Minimal when infos > 0 => global::OpenForge.Cli.OutputText.Doctor.DoctorText.TitleToListTheInfoFindingsOpenForgeDoctorDetailFull(),
            CliDetail.Standard when infos > 0 => global::OpenForge.Cli.OutputText.Doctor.DoctorText.TitleToListTheInfoFindingsOpenForgeDoctorDetailFull(),
            _ => string.Empty,
        };

    internal static string HelpSyntax() => ("  " + global::OpenForge.Cli.OutputText.Doctor.DoctorText.HelpSyntax());

    internal static string HelpInspection()
        => ("  " + global::OpenForge.Cli.OutputText.Doctor.DoctorText.HelpInspection());

    internal static string HelpGlobalOptions()
        => ("  " + global::OpenForge.Cli.OutputText.Shared.SharedText.HelpGlobalOptionsWithDetailSelection());

    internal static string HelpExamples()
        => ("  " + global::OpenForge.Cli.OutputText.Doctor.DoctorText.HelpExamples());

    internal static string IncompleteLine(DoctorDomainKind domain, DoctorDomainReport report)
    {
        var category = Category(domain);
        var finding = report.Findings.FirstOrDefault();
        var why = finding is null
            ? report.Limitations.FirstOrDefault()?.Message ?? global::OpenForge.Cli.OutputText.Doctor.DoctorText.MessageTheCheckCouldNotFinish()
            : domain == DoctorDomainKind.ExtensionLifecycle
                && finding.Kind == DoctorFindingKind.ExtensionSourceUnavailable
                ? global::OpenForge.Cli.OutputText.Doctor.DoctorPhrases.FormatThePackageSourceCannotBeRead($"{finding.Subject.Path ?? "<source>"}")
                : EnsureSentence(Message(finding));
        var label = domain switch
        {
            DoctorDomainKind.ExtensionLifecycle => global::OpenForge.Cli.OutputText.Doctor.DoctorText.TitleExtensionsWereNotChecked(),
            DoctorDomainKind.FrameworkLifecycle => global::OpenForge.Cli.OutputText.Doctor.DoctorText.TitleFrameworkFilesWereNotChecked(),
            DoctorDomainKind.LocalReferences => global::OpenForge.Cli.OutputText.Doctor.DoctorText.TitleLinksWereNotChecked(),
            DoctorDomainKind.RoutesMetadataOverwritesGeneratedNavigation => global::OpenForge.Cli.OutputText.Doctor.DoctorText.TitleRoutesWereNotChecked(),
            DoctorDomainKind.RecoveryResiduals => global::OpenForge.Cli.OutputText.Doctor.DoctorText.TitleRecoveryDataWasNotChecked(),
            _ => global::OpenForge.Cli.OutputText.Doctor.DoctorPhrases.FormatChecksDidNotFinish($"{category}"),
        };
        return $"{label}: {why}";
    }

    internal static string LaneSummary(IReadOnlyList<DoctorFinding> findings)
    {
        var parts = new List<string>();
        AddLane(parts, findings, DoctorResolutionLane.SafeExact, global::OpenForge.Cli.OutputText.Shared.SharedText.LabelCanBeFixedAutomatically(), global::OpenForge.Cli.OutputText.Shared.SharedText.LabelCanBeFixedAutomatically());
        AddLane(parts, findings, DoctorResolutionLane.GuidedChoice, global::OpenForge.Cli.OutputText.Shared.SharedText.LabelNeedsAChoice(), global::OpenForge.Cli.OutputText.Doctor.DoctorText.LabelNeedAChoice());
        AddLane(parts, findings, DoctorResolutionLane.TargetedOperation, global::OpenForge.Cli.OutputText.Doctor.DoctorText.LabelUsesTheIndicatedCommand(), global::OpenForge.Cli.OutputText.Doctor.DoctorText.LabelUseTheIndicatedCommand());
        AddLane(parts, findings, DoctorResolutionLane.ManualDecision, global::OpenForge.Cli.OutputText.Doctor.DoctorText.LabelMustBeFixedByHand(), global::OpenForge.Cli.OutputText.Doctor.DoctorText.LabelMustBeFixedByHand());
        AddLane(parts, findings, DoctorResolutionLane.BlockedRepair, global::OpenForge.Cli.OutputText.Doctor.DoctorText.LabelIsBlocked(), global::OpenForge.Cli.OutputText.Doctor.DoctorText.LabelAreBlocked());
        return string.Join(", ", parts) + (parts.Count == 0 ? string.Empty : ".");
    }

    private static void AddLane(
        ICollection<string> parts,
        IReadOnlyList<DoctorFinding> findings,
        DoctorResolutionLane lane,
        string singularPhrase,
        string pluralPhrase)
    {
        var count = findings.LongCount(finding => finding.Resolution == lane);
        if (count > 0)
        {
            parts.Add($"{count.ToString(CultureInfo.InvariantCulture)} {(count == 1 ? singularPhrase : pluralPhrase)}");
        }
    }

    private static string ReferenceTargetMissing(DoctorFinding finding)
    {
        var destination = finding.Subject.Identifier ?? global::OpenForge.Cli.OutputText.Shared.SharedText.LabelTheDestination();
        return finding.Candidates is { Items.Count: > 0 }
            ? global::OpenForge.Cli.OutputText.Doctor.DoctorPhrases.FormatTheLinkedFileWasNotFound($"{destination}")
            : global::OpenForge.Cli.OutputText.Doctor.DoctorPhrases.FormatTheLinkedFileWasNotFoundNoPossibleTargetWasFound($"{destination}");
    }

    private static string ReferenceFragmentMissing(DoctorFinding finding)
    {
        var destination = finding.Subject.Identifier ?? "the linked file#fragment";
        var separator = destination.IndexOf('#');
        var file = separator > 0 ? destination[..separator] : destination;
        var fragment = separator >= 0 ? destination[separator..] : "#fragment";
        return finding.Candidates is { Items.Count: > 0 }
            ? global::OpenForge.Cli.OutputText.Doctor.DoctorPhrases.FormatHasNoHeading($"{file}", $"{fragment}")
            : global::OpenForge.Cli.OutputText.Doctor.DoctorPhrases.FormatHasNoHeadingNoPossibleTargetWasFound($"{file}", $"{fragment}");
    }

    private static string ReferenceFragmentUnverified(DoctorFinding finding)
    {
        var destination = finding.Subject.Identifier ?? "the linked file#fragment";
        var separator = destination.IndexOf('#');
        var file = separator > 0 ? destination[..separator] : destination;
        var fragment = separator >= 0 ? destination[separator..] : "#fragment";
        return global::OpenForge.Cli.OutputText.Doctor.DoctorPhrases.FormatCouldNotBeParsedSoWasNotChecked($"{file}", $"{fragment}");
    }

    private static string CanonicalizationMessage(DoctorFinding finding, string phrase)
    {
        var canonical = finding.Evidence.OfType<DoctorComparisonEvidence>()
            .Select(evidence => evidence.Expected)
            .FirstOrDefault() ?? global::OpenForge.Cli.OutputText.Doctor.DoctorText.LabelTheCanonicalSpelling();
        return $"{FindingIdentifier(finding)} {phrase}: {canonical}.";
    }

    private static string CanonicalFragmentMessage(DoctorFinding finding)
    {
        var comparison = finding.Evidence.OfType<DoctorComparisonEvidence>().FirstOrDefault();
        var authored = Fragment(comparison?.Actual ?? finding.Subject.Identifier, "#fragment");
        var canonical = Fragment(comparison?.Expected, "#canonical");
        return global::OpenForge.Cli.OutputText.Doctor.DoctorPhrases.FormatMatchesTheHeadingApartFromSpelling($"{authored}", $"{canonical}");
    }

    private static string LibraryProjectionDangling(DoctorFinding finding)
    {
        var target = finding.Subject.Library?.ProjectionActualTarget
            ?? finding.Subject.Library?.ProjectionExpectedTarget
            ?? global::OpenForge.Cli.OutputText.Doctor.DoctorText.LabelTheTarget();
        return global::OpenForge.Cli.OutputText.Doctor.DoctorPhrases.FormatLinksToWhichDoesNotExist($"{Path(finding)}", $"{target}");
    }

    private static string LibraryProjectionRetargeted(DoctorFinding finding)
    {
        var expected = finding.Subject.Library?.ProjectionExpectedTarget ?? global::OpenForge.Cli.OutputText.Doctor.DoctorText.LabelTheExpectedTarget();
        var actual = finding.Subject.Library?.ProjectionActualTarget ?? global::OpenForge.Cli.OutputText.Doctor.DoctorText.LabelTheActualTarget();
        return global::OpenForge.Cli.OutputText.Doctor.DoctorPhrases.FormatNoLongerLinksToItLinksTo($"{Path(finding)}", $"{expected}", $"{actual}");
    }

    private static string LibrarySourceMessage(DoctorFinding finding, string phrase)
        => global::OpenForge.Cli.OutputText.Doctor.DoctorPhrases.FormatTheSourceFolderOf($"{LibraryId(finding)}", $"{Path(finding)}", $"{phrase}");

    private static string ComparisonActual(DoctorFinding finding)
        => finding.Evidence.OfType<DoctorComparisonEvidence>()
            .Select(evidence => evidence.Actual)
            .FirstOrDefault() ?? global::OpenForge.Cli.OutputText.Doctor.DoctorText.LabelTheAuthoredValue();

    private static string OverwriteOrphan(DoctorFinding finding)
    {
        var path = Path(finding);
        const string suffix = ".overwrite.md";
        var basePath = path.EndsWith(suffix, StringComparison.Ordinal)
            ? path[..^suffix.Length]
            : path;
        return global::OpenForge.Cli.OutputText.Doctor.DoctorPhrases.FormatHasNoMdBesideIt($"{path}", $"{basePath}");
    }

    private static string ExtensionSourceUnavailable(DoctorFinding finding)
        => global::OpenForge.Cli.OutputText.Doctor.DoctorPhrases.FormatTheSourceOfCannotBeReadSoItsFilesWereNotCompared($"{ExtensionId(finding)}", $"{Path(finding)}");

    private static string ExtensionBridgeRegistration(DoctorFinding finding)
        => global::OpenForge.Cli.OutputText.Doctor.DoctorPhrases.FormatDoesNotListWhichInstalled($"{finding.Provenance.Path ?? "the parent"}", $"{Path(finding)}", $"{ExtensionId(finding)}");

    private static string Fragment(string? value, string fallback)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return fallback;
        }

        var separator = value.IndexOf('#');
        return separator >= 0 ? value[separator..] : fallback;
    }

    private static string Path(DoctorFinding finding)
        => finding.Subject.Path ?? finding.Subject.Identifier ?? global::OpenForge.Cli.OutputText.Shared.SharedText.LabelTheWorkspace();

    private static string FindingIdentifier(DoctorFinding finding)
        => finding.Subject.Identifier ?? global::OpenForge.Cli.OutputText.Shared.SharedText.LabelTheDestination();

    private static string LibraryId(DoctorFinding finding)
        => finding.Subject.Library?.LibraryIdValue ?? finding.Subject.Identifier ?? global::OpenForge.Cli.OutputText.Doctor.DoctorText.PlaceholderId();

    private static string ExtensionId(DoctorFinding finding)
        => finding.Subject.Identifier ?? global::OpenForge.Cli.OutputText.Doctor.DoctorText.PlaceholderId();

    private static string BridgeFile(DoctorFinding finding)
        => (finding.Subject.Path?.Contains("CLAUDE.md", StringComparison.OrdinalIgnoreCase) == true
            || finding.Message.Contains("CLAUDE.md", StringComparison.OrdinalIgnoreCase))
            ? "CLAUDE.md"
            : "AGENTS.md";

    private static string Cause(DoctorFinding finding, string fallback)
        => PlainCause(finding.Message, fallback);

    private static string PlainCause(string? value, string fallback)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return fallback;
        }

        return value.Trim().TrimEnd('.');
    }

    private static string OwnershipObservation(string what)
        => global::OpenForge.Cli.OutputText.Doctor.DoctorWording.OwnershipObservation(what);

    private static CliNextAction? FallbackAction(DoctorFinding finding)
    {
        var id = finding.Subject.Identifier ?? "<id>";
        var path = finding.Subject.Path ?? finding.Subject.Identifier ?? "the reported file";
        return finding.Kind switch
        {
            DoctorFindingKind.WorkspaceAgentsMissing => Action("open-forge install --dry-run", global::OpenForge.Cli.OutputText.Doctor.DoctorText.MessagePreviewTheFrameworkInstallation()),
            DoctorFindingKind.WorkspaceLoaderMissing => Action("open-forge install --dry-run", global::OpenForge.Cli.OutputText.Doctor.DoctorText.MessagePreviewTheFrameworkInstallation()),
            DoctorFindingKind.WorkspaceLoaderMalformed => Action("open-forge update", global::OpenForge.Cli.OutputText.Doctor.DoctorText.MessageRestoreTheShippedLoader()),
            DoctorFindingKind.WorkspaceEntryMissing or DoctorFindingKind.WorkspaceRootMissing
                or DoctorFindingKind.RouteEntrypointMissing => Action($"open-forge route init {id}", global::OpenForge.Cli.OutputText.Doctor.DoctorText.MessageCreateTheMissingRouteEntrypoint()),
            DoctorFindingKind.WorkspaceDetached => Action("open-forge index", global::OpenForge.Cli.OutputText.Doctor.DoctorText.MessageIndexTheRoutedParentBeforeLoadingThisSource()),
            DoctorFindingKind.RouteUnreachable => Action("open-forge index", global::OpenForge.Cli.OutputText.Doctor.DoctorText.MessageRefreshTheRouteIndex()),
            DoctorFindingKind.RouteGeneratedRegionStale or DoctorFindingKind.RouteGeneratedRegionMissing
                or DoctorFindingKind.RouteGeneratedEntryMissing or DoctorFindingKind.RouteGeneratedEntryExtra
                or DoctorFindingKind.RouteGeneratedEntryOrder or DoctorFindingKind.RouteGeneratedEntryPath
                or DoctorFindingKind.RouteGeneratedEntryDescription or DoctorFindingKind.RouteGeneratedEntryTags
                or DoctorFindingKind.RouteOverwriteIndependentIndex => Action("open-forge index", global::OpenForge.Cli.OutputText.Doctor.DoctorText.MessageUpdateDeterministicGeneratedNavigation()),
            DoctorFindingKind.ReferenceFragmentUnverified => Action("open-forge doctor", global::OpenForge.Cli.OutputText.Doctor.DoctorText.MessageRunDoctorAgainAfterFixingTheSourceFile()),
            DoctorFindingKind.ReferenceTargetMissing or DoctorFindingKind.ReferenceFragmentMissing
                when finding.Candidates is { Items.Count: > 0 } => Action("open-forge repair", global::OpenForge.Cli.OutputText.Doctor.DoctorText.MessageChooseOneOfTheBoundedCandidates()),
            DoctorFindingKind.ReferenceTargetMissing or DoctorFindingKind.ReferenceFragmentMissing => Sentence(global::OpenForge.Cli.OutputText.Doctor.DoctorText.MessageFixTheLinkByHand()),
            DoctorFindingKind.ReferenceSameTargetPath or DoctorFindingKind.ReferenceSameTargetCase
                or DoctorFindingKind.ReferenceSameTargetEncoding or DoctorFindingKind.ReferenceSameTargetFragment
                => Action("open-forge repair --automatic", global::OpenForge.Cli.OutputText.Doctor.DoctorText.MessageApplyTheExactSameTargetCorrection()),
            DoctorFindingKind.FrameworkManagedMissing or DoctorFindingKind.FrameworkManagedChanged
                or DoctorFindingKind.FrameworkBridgeBoundary or DoctorFindingKind.FrameworkPartialLifecycle
                => Action("open-forge update", global::OpenForge.Cli.OutputText.Doctor.DoctorText.MessageReconcileTheFrameworkFiles()),
            DoctorFindingKind.FrameworkPartialRecovery => Action("open-forge doctor --detail full", global::OpenForge.Cli.OutputText.Doctor.DoctorText.MessageInspectTheIncompleteFrameworkRecovery()),
            DoctorFindingKind.FrameworkDistributedPayloadDefect => Sentence(global::OpenForge.Cli.OutputText.Doctor.DoctorText.MessageReinstallTheCli()),
            DoctorFindingKind.FrameworkInstallAbsent => Action("open-forge install --dry-run", global::OpenForge.Cli.OutputText.Doctor.DoctorText.MessagePreviewTheFrameworkInstallation()),
            DoctorFindingKind.ExtensionManagedMissing or DoctorFindingKind.ExtensionManagedChanged
                or DoctorFindingKind.ExtensionPartialLifecycle => Action($"open-forge extension update {id}", global::OpenForge.Cli.OutputText.Doctor.DoctorText.MessageReconcileTheExtensionFiles()),
            DoctorFindingKind.ExtensionUnknownId => Action("open-forge extension list", global::OpenForge.Cli.OutputText.Doctor.DoctorText.MessageListTheAvailableExtensions()),
            DoctorFindingKind.ExtensionDependencyMissing => Action($"open-forge extension install {id}", global::OpenForge.Cli.OutputText.Doctor.DoctorText.MessageInstallTheMissingExtensionDependency()),
            DoctorFindingKind.ExtensionBridgeRegistration => Action("open-forge index", global::OpenForge.Cli.OutputText.Doctor.DoctorText.MessageRefreshTheExtensionEntry()),
            DoctorFindingKind.LibrarySourceRootInvalid or DoctorFindingKind.LibraryInventoryIncomplete
                or DoctorFindingKind.LibraryProjectionDangling or DoctorFindingKind.LibraryProjectionRetargeted
                => Action($"open-forge library inspect {id}", global::OpenForge.Cli.OutputText.Doctor.DoctorText.MessageInspectTheLibrarySourceAndProjection()),
            DoctorFindingKind.LibraryProjectionMissing => Action($"open-forge library sync {id}", global::OpenForge.Cli.OutputText.Doctor.DoctorText.MessageReviewTheMissingLibraryLink()),
            DoctorFindingKind.LibraryRecoverySafeExact => Action("open-forge repair --automatic", global::OpenForge.Cli.OutputText.Doctor.DoctorText.MessageApplyTheVerifiedRecoveryStep()),
            DoctorFindingKind.RecoveryBundleRecognized => Action("open-forge cleanup", global::OpenForge.Cli.OutputText.Doctor.DoctorText.MessageReviewAndRemoveTheKeptRecoveryBundle()),
            DoctorFindingKind.RecoveryDraftRecognized or DoctorFindingKind.RecoveryBundleCollision
                => Action("open-forge cleanup --dry-run", global::OpenForge.Cli.OutputText.Doctor.DoctorText.MessagePreviewRecoveryCleanup()),
            _ when finding.Resolution == DoctorResolutionLane.ManualDecision => Sentence(global::OpenForge.Cli.OutputText.Shared.SharedText.MessageFixItByHand()),
            _ => null,
        };
    }

    private static CliNextAction Action(string command, string reason)
        => new(command, reason);

    private static CliNextAction Sentence(string sentence)
        => new(sentence, sentence) { Kind = CliNextActionKind.Sentence };

    private static string Parts(long value, string singular)
        => $"{value} {CliTextPlural(value, singular)}";

    private static string Recorded(long count)
        => global::OpenForge.Cli.OutputText.Doctor.DoctorPhrases.FormatRecorded($"{Parts(count, "info")}", $"{CliTextPlural(count, "finding")}", $"{(count == 1 ? "was" : "were")}");

    private static string JoinWithAnd(IReadOnlyList<string> parts)
        => parts.Count switch
        {
            0 => string.Empty,
            1 => parts[0],
            2 => global::OpenForge.Cli.OutputText.Doctor.DoctorPhrases.WordListPair($"{parts[0]}", $"{parts[1]}"),
            _ => string.Join(", ", parts.Take(parts.Count - 1)) + global::OpenForge.Cli.OutputText.Doctor.DoctorPhrases.WordListFinalItem($"{parts[^1]}"),
        };

    private static string EnsureSentence(string value)
        => string.IsNullOrWhiteSpace(value)
            ? global::OpenForge.Cli.OutputText.Doctor.DoctorText.MessageTheCheckCouldNotFinish()
            : value.EndsWith(".", StringComparison.Ordinal) ? value : value + ".";

    private static string CliTextPlural(long value, string singular)
        => value == 1 ? singular : singular switch
        {
            "info" => global::OpenForge.Cli.OutputText.Shared.SharedText.LabelInfo(),
            "external link" => global::OpenForge.Cli.OutputText.Doctor.DoctorText.LabelExternalLinks(),
            "image link" => global::OpenForge.Cli.OutputText.Doctor.DoctorText.LabelImageLinks(),
            _ => singular + "s",
        };
}
