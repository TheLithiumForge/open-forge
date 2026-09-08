using OpenForge.Cli.Core.Framework.Libraries;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem;
using OpenForge.Cli.Core.Framework.Libraries.Models.Record;
using OpenForge.Cli.Core.Framework.Libraries.Operational.Models;
using OpenForge.Cli.Core.Commands.Status.Models.Operation;
using OpenForge.Cli.Core.Framework.Extensions.Operational.Models;
using OpenForge.Cli.Core.Framework.Lifecycle.Models;
using OpenForge.Cli.Core.Framework.Lifecycle.Operational.Models;
using OpenForge.Cli.Core.Framework.OperationalContributors.Models;
using OpenForge.Cli.Core.Framework.Recovery.Models;
using OpenForge.Cli.Core.Framework.Recovery.Operational.Models;
using OpenForge.Cli.Core.Framework.Sources.Operational.Models;
using OpenForge.Cli.Core.Framework.Workspace.Operational.Models;

namespace OpenForge.Cli.Core.UnitTests.Commands.Status;

internal static class StatusAggregationObservationSeed
{
    internal static StatusObservationSet Create()
        => new(
            new WorkspaceEntryStatusView(
                OperationalViewState.Complete,
                OperationalInstallationState.Installed,
                "AGENTS.md",
                ".agents/loader.md"),
            new RecoveryResidualStatusView(
                OperationalViewState.Complete,
                [
                    new RecoveryCandidateObservation(
                        "/recovery/z-final.zip",
                        RecoveryBundleCandidateKind.Final,
                        RecoveryBundleIntegrity.Verified),
                    new RecoveryCandidateObservation(
                        "/recovery/a-draft.tmp",
                        RecoveryBundleCandidateKind.Draft,
                        RecoveryBundleIntegrity.Incomplete),
                ]),
            new RouteStatusView
            {
                State = OperationalViewState.Complete,
                SourceInventory = RouteSourceInventoryState.Present,
                InitialStartup = StatusObservationSeeds.Measurement(2, 9, 11, 3),
                CurrentStartup = StatusObservationSeeds.Measurement(4, 21, 25, 6),
                TotalAvailable = StatusObservationSeeds.Measurement(7, 81, 100, 21),
                Continuity = StatusObservationSeeds.Measurement(2, 12, 15, 3),
                ContinuitySources =
                [
                    new ContextSourceContributionObservation
                    {
                        SourceId = "memory/zeta",
                        Utf8Bytes = 5,
                        Layers = [new ContextLayerContributionObservation(".agents/memory/zeta.md", 5)],
                    },
                    new ContextSourceContributionObservation
                    {
                        SourceId = "memory/alpha",
                        Utf8Bytes = 10,
                        Layers =
                        [
                            new ContextLayerContributionObservation(".agents/memory/alpha.md", 6),
                            new ContextLayerContributionObservation(".agents/memory/alpha.overwrite.md", 4),
                        ],
                    },
                    new ContextSourceContributionObservation
                    {
                        SourceId = "memory/beta",
                        Utf8Bytes = 10,
                        Layers = [new ContextLayerContributionObservation(".agents/memory/beta.md", 10)],
                    },
                ],
                InitialRootCategories = ["directives", "memory", "patterns"],
                CurrentRootCategories = ["memory", "custom", "directives"],
                GeneratedNavigation =
                [
                    new GeneratedNavigationTargetObservation(
                        ".agents/memory/_memory.md",
                        OperationalGeneratedNavigationState.Current),
                    new GeneratedNavigationTargetObservation(
                        ".agents/directives/_directives.md",
                        OperationalGeneratedNavigationState.Changed),
                ],
            },
            new FrameworkLifecycleStatusView
            {
                State = OperationalViewState.Complete,
                Presence = OperationalLifecyclePresenceState.Present,
                Lifecycle = OperationalLifecycleState.Trusted,
                SourceAvailability = OperationalSourceAvailability.Available,
                Targets =
                [
                    new FrameworkManagedTargetObservation
                    {
                        Path = ".agents/zeta.md",
                        Kind = FrameworkManagedTargetKind.File,
                        SourceAssetPath = "zeta.md",
                        Region = null,
                        BaselineFingerprint = "sha256:zeta",
                        FingerprintKind = "bytes-v1",
                        Source = new FrameworkLifecycleTargetSourceValidation(
                            FrameworkLifecycleTargetSourceState.Valid,
                            cause: null),
                        State = OperationalTargetState.Current,
                    },
                    new FrameworkManagedTargetObservation
                    {
                        Path = ".agents/alpha.md",
                        Kind = FrameworkManagedTargetKind.GeneratedRegion,
                        SourceAssetPath = null,
                        Region = "entries",
                        BaselineFingerprint = "sha256:alpha",
                        FingerprintKind = "open-forge-markdown-v1",
                        Source = new FrameworkLifecycleTargetSourceValidation(
                            FrameworkLifecycleTargetSourceState.Valid,
                            cause: null),
                        State = OperationalTargetState.Missing,
                    },
                ],
            },
            new ExtensionLifecycleStatusView
            {
                State = OperationalViewState.Complete,
                Presence = OperationalLifecyclePresenceState.Present,
                Lifecycle = OperationalLifecycleState.Trusted,
                SourceAvailability = OperationalSourceAvailability.Unavailable,
                Installed =
                [
                    new InstalledExtensionObservation
                    {
                        Id = "zeta",
                        Version = null,
                        Source = null,
                        SourceAvailability = OperationalSourceAvailability.Unavailable,
                        Dependencies = ["beta", "alpha"],
                        Paths = [".agents/zeta.md", ".agents/shared.md"],
                    },
                    new InstalledExtensionObservation
                    {
                        Id = "alpha",
                        Version = "1.0.0",
                        Source = "embedded:alpha",
                        SourceAvailability = OperationalSourceAvailability.Available,
                        Dependencies = [],
                        Paths = [".agents/shared.md"],
                    },
                ],
                Targets =
                [
                    new ExtensionManagedTargetObservation
                    {
                        Path = ".agents/shared.md",
                        Owners = ["zeta"],
                        BaselineFingerprint = "sha256:shared",
                        FingerprintKind = "open-forge-markdown-v1",
                        State = OperationalTargetState.Changed,
                    },
                    new ExtensionManagedTargetObservation
                    {
                        Path = ".agents/alpha.md",
                        Owners = ["alpha"],
                        BaselineFingerprint = "sha256:alpha",
                        FingerprintKind = "open-forge-markdown-v1",
                        State = OperationalTargetState.Current,
                    },
                    new ExtensionManagedTargetObservation
                    {
                        Path = ".agents/shared.md",
                        Owners = ["alpha"],
                        BaselineFingerprint = "sha256:shared",
                        FingerprintKind = "open-forge-markdown-v1",
                        State = OperationalTargetState.Changed,
                    },
                ],
            },
            new LibraryStatusView
            {
                State = OperationalViewState.Complete,
                Ownership = null,
                LinkCapability = null,
                Record = new LibrariesRecordRead
                {
                    State = LibrariesRecordReadState.Missing,
                    Record = null,
                    Snapshot = FileStateSnapshot.Missing(LibraryPathIdentity.RecordPath(StatusObservationSeeds.Workspace().LexicalRoot)),
                    Cause = null,
                },
                Sources = [],
                Mappings = [],
            });
}
