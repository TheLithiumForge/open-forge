using OpenForge.Cli.Core.Commands.Status.Models.Result;
using OpenForge.Cli.Core.Commands.Status.Shared.Aggregation;
using OpenForge.Cli.Core.Framework.Distribution.Operational.Models;
using OpenForge.Cli.Core.Framework.OperationalContributors.Models;
using OpenForge.Cli.Core.Framework.Recovery.Models.Catalogue;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Operation;
using StatusGeneratedNavigationState = OpenForge.Cli.Core.Commands.Status.Models.Result.StatusGeneratedNavigationState;
using StatusInstallationState = OpenForge.Cli.Core.Commands.Status.Models.Result.StatusInstallationState;
using StatusLifecycleState = OpenForge.Cli.Core.Commands.Status.Models.Result.StatusLifecycleState;
using StatusManagedTargetKind = OpenForge.Cli.Core.Commands.Status.Models.Result.StatusManagedTargetKind;
using StatusRecoveryCandidateKind = OpenForge.Cli.Core.Commands.Status.Models.Result.StatusRecoveryCandidateKind;
using StatusRecoveryIntegrity = OpenForge.Cli.Core.Commands.Status.Models.Result.StatusRecoveryIntegrity;
using StatusSourceAvailability = OpenForge.Cli.Core.Commands.Status.Models.Result.StatusSourceAvailability;
using StatusTargetState = OpenForge.Cli.Core.Commands.Status.Models.Result.StatusTargetState;
using StatusValueState = OpenForge.Cli.Core.Commands.Status.Models.Result.StatusValueState;

namespace OpenForge.Cli.Core.UnitTests.Commands.Status;

internal static class StatusResultSeeds
{
    internal static StatusIntegerValue Available(long value)
        => new(StatusValueState.Available, value);

    internal static StatusMeasurement Measurement(
        long files,
        long characters,
        long utf8Bytes,
        long estimatedTokens)
        => new(
            Available(files),
            Available(characters),
            Available(utf8Bytes),
            Available(estimatedTokens));

    internal static StatusResult Representative(
        CliSemanticStatus status = CliSemanticStatus.Attention,
        CliNextAction? next = null)
    {
        var finding = status == CliSemanticStatus.Complete
            ? null
            : Finding(status);
        return new StatusResult
        {
            Status = status,
            Workspace = StatusObservationSeeds.Workspace(),
            Next = next,
            Facts = new StatusFacts
            {
                Installation = new StatusInstallation(
                    StatusInstallationState.Installed,
                    "AGENTS.md",
                    ".agents/loader.md"),
                Context = new StatusContext
                {
                    TokenEstimator = "ceiling-characters-divided-by-four",
                    Startup = new StatusStartupComparison(
                        Measurement(2, 9, 11, 3),
                        Measurement(4, 21, 25, 6),
                        Measurement(2, 12, 14, 3)),
                    TotalAvailable = Measurement(7, 81, 100, 21),
                    StartupPercentage = new StatusDecimalValue(StatusValueState.Available, 25m),
                    Continuity = Measurement(2, 12, 15, 3),
                    ContinuitySources =
                    [
                        new StatusContinuitySource
                        {
                            SourceId = "memory/alpha",
                            Utf8Bytes = 10,
                            Layers =
                            [
                                new StatusContextLayer(".agents/memory/alpha.md", 6),
                                new StatusContextLayer(".agents/memory/alpha.overwrite.md", 4),
                            ],
                        },
                        new StatusContinuitySource
                        {
                            SourceId = "memory/beta",
                            Utf8Bytes = 10,
                            Layers = [new StatusContextLayer(".agents/memory/beta.md", 10)],
                        },
                    ],
                },
                Structure = new StatusStructure
                {
                    RootCategories = new StatusRootCategories
                    {
                        Count = Available(3),
                        Added = ["custom"],
                        Removed = ["patterns"],
                    },
                    GeneratedNavigation =
                    [
                        new StatusGeneratedNavigation(
                            ".agents/directives/_directives.md",
                            status == CliSemanticStatus.Attention
                                ? StatusGeneratedNavigationState.Changed
                                : StatusGeneratedNavigationState.Current),
                    ],
                },
                Lifecycle = new StatusLifecycle(
                    new StatusFrameworkLifecycle
                    {
                        State = StatusLifecycleState.Trusted,
                        SourceAvailability = StatusSourceAvailability.Available,
                        Targets =
                        [
                            new StatusFrameworkTarget
                            {
                                Path = ".agents/loader.md",
                                Kind = StatusManagedTargetKind.File,
                                SourceAssetPath = "loader.md",
                                Region = null,
                                State = StatusTargetState.Current,
                            },
                        ],
                    },
                    new StatusExtensionLifecycle
                    {
                        State = StatusLifecycleState.Trusted,
                        SourceAvailability = StatusSourceAvailability.Available,
                        Installed =
                        [
                            new StatusInstalledExtension
                            {
                                Id = "alpha",
                                Version = "1.0.0",
                                Source = "embedded:alpha",
                                SourceAvailability = StatusSourceAvailability.Available,
                                Dependencies = [],
                                Paths = [".agents/shared.md"],
                            },
                        ],
                        ManagedFiles = new StatusManagedExtensionFiles
                        {
                            Counts = new StatusManagedTargetCounts
                            {
                                Current = Available(1),
                                Changed = Available(0),
                                Missing = Available(0),
                                Unavailable = Available(0),
                                Blocked = Available(0),
                            },
                            Targets =
                            [
                                new StatusExtensionTarget
                                {
                                    Path = ".agents/shared.md",
                                    Owners = ["alpha", "zeta"],
                                State = StatusTargetState.Current,
                                },
                            ],
                        },
                    }),
                Library = StatusLibraryAggregator.Build(StatusAggregationObservationSeed.Create().Libraries),
                Recovery = new StatusRecovery
                {
                    VerifiedFinals = Available(status == CliSemanticStatus.Attention ? 1 : 0),
                    IncompleteDrafts = Available(0),
                    Candidates = status == CliSemanticStatus.Attention
                        ?
                        [
                            new StatusRecoveryCandidate(
                                "/recovery/final.zip",
                                StatusRecoveryCandidateKind.Final,
                                StatusRecoveryIntegrity.Verified),
                        ]
                        : [],
                },
            },
            Findings = finding is null ? [] : [finding],
        };
    }

    internal static StatusFinding Finding(CliSemanticStatus status)
    {
        var code = status switch
        {
            CliSemanticStatus.Attention => StatusFindingCode.GeneratedNavigationChanged,
            CliSemanticStatus.Incomplete => StatusFindingCode.ContextInventoryIncomplete,
            CliSemanticStatus.Invalid => StatusFindingCode.InvalidInput,
            CliSemanticStatus.Blocked => StatusFindingCode.WorkspaceUnsafe,
            CliSemanticStatus.Failed => StatusFindingCode.OperationFailed,
            CliSemanticStatus.Interrupted => StatusFindingCode.Interrupted,
            _ => throw new ArgumentOutOfRangeException(nameof(status), status, "The test status requires a finding."),
        };
        return new StatusFinding
        {
            Code = code,
            Status = status,
            Subject = ".agents/example.md",
            Cause = "Representative owned Status evidence.",
        };
    }
}
