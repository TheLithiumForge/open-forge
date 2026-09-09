using OpenForge.Cli.Core.Commands.Library.Models.Permissions;
using OpenForge.Cli.Core.Framework.Workspace;
using OpenForge.Cli.Core.Framework.Filesystem.Shared.Paths;
using OpenForge.Cli.Core.Commands.Library.Shared.Completion.Models;
using OpenForge.Cli.Core.Commands.Library.Models.Application;
using OpenForge.Cli.Core.Commands.Library.Models.Planning;
using OpenForge.Cli.Core.Commands.Library.Models.Request;
using OpenForge.Cli.Core.Commands.Library.Models.Result.Coordinates.Effects;
using OpenForge.Cli.Core.Commands.Library.Models.Result.Coordinates.Observation;
using OpenForge.Cli.Core.Commands.Library.Models.Result.Effects;
using OpenForge.Cli.Core.Commands.Library.Models.Result.Observation;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths.Models;
using OpenForge.Cli.Core.Framework.Libraries;
using OpenForge.Cli.Core.Framework.Libraries.Models.Identity;
using OpenForge.Cli.Core.Framework.Libraries.Models.Inventory;
using OpenForge.Cli.Core.Framework.Libraries.Models.Observation;
using OpenForge.Cli.Core.Framework.Libraries.Models.Record;
using OpenForge.Cli.Core.Framework.Lifecycle.Models.Ownership;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem;
using OpenForge.Cli.Core.Framework.Recovery.Models;
using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.Core.Commands.Library.Shared.Completion;

internal static class LibraryMutationCompletionProjection
{
    internal static LibraryMutationIdentity Identity(
        string? libraryId,
        string? sourceRoot,
        string? destinationRoot,
        LibraryMode mode,
        bool sourceIndependent)
        => new()
        {
            LibraryId = libraryId,
            SourceRoot = sourceRoot,
            DestinationRoot = destinationRoot,
            Mode = mode,
            SourceIndependent = sourceIndependent,
        };

    internal static LibraryMutationRecord Record(
        LibrariesRecordRead? read,
        string? libraryId,
        LibrariesRecord? intended)
    {
        var selected = read?.Record?.Libraries.FirstOrDefault(library => string.Equals(
            library.Id.Value,
            libraryId,
            StringComparison.Ordinal));
        return new LibraryMutationRecord
        {
            Path = LibraryPathIdentity.RecordRelativePath,
            State = read?.State switch
            {
                null => LibraryMutationRecordState.NotStarted,
                LibrariesRecordReadState.Missing => LibraryMutationRecordState.Missing,
                LibrariesRecordReadState.Complete => LibraryMutationRecordState.Complete,
                LibrariesRecordReadState.Malformed => LibraryMutationRecordState.Invalid,
                LibrariesRecordReadState.Unavailable => LibraryMutationRecordState.Unavailable,
                LibrariesRecordReadState.Blocked => LibraryMutationRecordState.Blocked,
                _ => throw new ArgumentOutOfRangeException(nameof(read), read.State, "The Library record state is not defined."),
            },
            RegisteredPaths = selected?.Paths.Select(path => path.Value).ToArray() ?? [],
            Intended = intended is null ? null : Document(intended),
        };
    }

    private static LibraryMapping ProjectMapping(
        LibraryInventoryRead? read,
        LibraryDestinationRoot? destinationRoot,
        SourceRelativeEligiblePath sourcePath)
        => LibraryPathIdentity.Map(
            read?.Source.Request.SourceRoot ?? throw new InvalidOperationException("An eligible source requires its Library root."),
            destinationRoot ?? throw new InvalidOperationException("An eligible source requires its selected destination root."),
            sourcePath);

    internal static LibraryMutationSource Source(LibraryInventoryRead? read, LibraryDestinationRoot? destinationRoot)
    {
        var inventory = read?.Inventory;
        return new LibraryMutationSource
        {
            RootState = read?.Source.State switch
            {
                null => LibrarySourceRootViewState.NotStarted,
                LibrarySourceRootState.Available => LibrarySourceRootViewState.Available,
                LibrarySourceRootState.Missing => LibrarySourceRootViewState.Missing,
                LibrarySourceRootState.Invalid => LibrarySourceRootViewState.Invalid,
                LibrarySourceRootState.Inaccessible or LibrarySourceRootState.Unavailable => LibrarySourceRootViewState.Unavailable,
                LibrarySourceRootState.Blocked => LibrarySourceRootViewState.Blocked,
                _ => throw new ArgumentOutOfRangeException(nameof(read), read.Source.State, "The Library source-root state is not defined."),
            },
            InventoryState = inventory?.State switch
            {
                null => LibraryMutationInventoryState.NotStarted,
                LibraryInventoryState.Complete => LibraryMutationInventoryState.Complete,
                LibraryInventoryState.Incomplete or LibraryInventoryState.Unavailable => LibraryMutationInventoryState.Incomplete,
                LibraryInventoryState.Blocked => LibraryMutationInventoryState.Blocked,
                _ => throw new ArgumentOutOfRangeException(nameof(read), inventory.State, "The Library inventory state is not defined."),
            },
            EligiblePaths = inventory?.Entries.Select(entry =>
            {
                var mapping = ProjectMapping(read, destinationRoot, entry.SourcePath);
                return new LibraryEligiblePath
                {
                    SourcePath = entry.SourcePath.Value,
                    DestinationPath = mapping.DestinationPath.Value,
                    SourceId = SourceIdentity.DeriveId(mapping.DestinationPath.Value),
                };
            }).ToArray() ?? [],
            ExcludedPaths = read?.ExcludedPaths.Select(path => new LibraryExcludedPath
            {
                Path = path.Path,
                Reason = path.Kind switch
                {
                    LibraryInventoryExclusionKind.Loader => LibraryExclusionKind.Loader,
                    LibraryInventoryExclusionKind.Entrypoint => LibraryExclusionKind.Entrypoint,
                    LibraryInventoryExclusionKind.Overwrite => LibraryExclusionKind.Overwrite,
                    LibraryInventoryExclusionKind.ManagerControl => LibraryExclusionKind.ManagerControl,
                    LibraryInventoryExclusionKind.Link => LibraryExclusionKind.Link,
                    LibraryInventoryExclusionKind.ReparsePoint => LibraryExclusionKind.ReparsePoint,
                    LibraryInventoryExclusionKind.Special => LibraryExclusionKind.Special,
                    _ => throw new ArgumentOutOfRangeException(nameof(read), path.Kind, "The Library exclusion kind is not defined."),
                },
            }).ToArray() ?? [],
            UnavailablePaths = read?.UnavailablePaths.Select(path => new LibraryUnavailablePath
            {
                Path = path.Path,
                Cause = path.Cause,
            }).ToArray() ?? [],
            LexicalRoot = read?.Source.LexicalSourceRoot,
            PhysicalRoot = read?.Source.PhysicalSourceRoot,
            LexicallyContained = read?.Source.LexicallyContained,
            PhysicallyContained = read?.Source.PhysicallyContained,
        };
    }

    internal static LibraryMutationProjection Projection(
        LibrariesRecordRead? record,
        LibraryInventoryRead? source,
        IReadOnlyList<LibraryMappingObservation>? mappings,
        LifecycleOwnershipReadResult? ownership,
        string? libraryId,
        LibraryPlanState state,
        bool sourceIndependent)
    {
        var registered = record?.Record?.Libraries
            .FirstOrDefault(library => string.Equals(library.Id.Value, libraryId, StringComparison.Ordinal));
        var registeredPaths = registered?.Paths.Select(path => path.Value).ToHashSet(StringComparer.Ordinal)
            ?? new HashSet<string>(StringComparer.Ordinal);
        var eligiblePaths = source?.Inventory?.Entries.Select(entry => entry.SourcePath.Value).ToHashSet(StringComparer.Ordinal)
            ?? new HashSet<string>(StringComparer.Ordinal);
        var values = mappings?.Select(observation => new LibraryMutationMapping
        {
            SourcePath = observation.Mapping.SourcePath.Value,
            DestinationPath = observation.Mapping.DestinationPath.Value,
            SourceId = SourceIdentity.DeriveId(observation.Mapping.DestinationPath.Value),
            ExpectedRelativeLink = observation.Mapping.ExpectedRelativeLink.Value,
            ObservedRelativeLink = observation.Leaf.RelativeFileLink?.RawRelativeTarget,
            State = observation.State switch
            {
                LibraryMappingObservationState.Current => LibraryLinkViewState.Current,
                LibraryMappingObservationState.Missing => LibraryLinkViewState.Missing,
                LibraryMappingObservationState.Changed => LibraryLinkViewState.Changed,
                LibraryMappingObservationState.Blocked => LibraryLinkViewState.Blocked,
                LibraryMappingObservationState.Unavailable => LibraryLinkViewState.Unavailable,
                _ => throw new ArgumentOutOfRangeException(nameof(mappings), observation.State, "The mapping state is not defined."),
            },
            Relation = Relation(
                observation.State,
                registeredPaths.Contains(observation.Mapping.SourcePath.Value),
                eligiblePaths.Contains(observation.Mapping.SourcePath.Value),
                sourceIndependent),
        }).OrderBy(mapping => mapping.DestinationPath, StringComparer.Ordinal).ToArray() ?? [];
        return new LibraryMutationProjection
        {
            State = state,
            Mappings = values,
            Collisions = [],
            Ownership = ownership?.Claims.Select(claim => new LibraryOwnershipObservation
            {
                Path = claim.Path,
                Kind = claim.Manager switch
                {
                    LifecycleOwnershipManager.Framework => LibraryOwnershipKind.Framework,
                    LifecycleOwnershipManager.Extension => LibraryOwnershipKind.Extension,
                    _ => throw new ArgumentOutOfRangeException(nameof(ownership), claim.Manager, "The ownership manager is not defined."),
                },
                OwnerId = claim.Owner,
                Cause = null,
            }).ToArray() ?? [],
        };
    }

    internal static LibraryMutationPlanView NotPlanned() => new()
    {
        State = LibraryPlanState.NotStarted,
        Directories = [],
        Links = [],
        GeneratedRegions = [],
        RecordEffect = LibraryRecordEffect.None,
        RecordExpected = null,
    };

    internal static LibraryMutationPlanView Plan(LibraryMutationPlanProjectionInput input)
        => new()
        {
            State = input.State,
            Directories = input.Effects?.Directories.Select(directory => new LibraryDirectoryEffectView
            {
                Path = Relative(input.Workspace, directory.LogicalPath),
                Expected = Expected(directory.Expectation),
            }).ToArray() ?? [],
            Links = input.Effects?.Links.Select(link => new LibraryLinkEffectView
            {
                Path = link.DestinationPath.Value,
                Kind = link.Kind switch
                {
                    RelativeFileLinkEffectKind.Create => LibraryLinkEffectKind.Create,
                    RelativeFileLinkEffectKind.Delete => LibraryLinkEffectKind.Delete,
                    _ => throw new ArgumentOutOfRangeException(nameof(input), link.Kind, "The Library link effect kind is not defined."),
                },
                RawRelativeTarget = link.RawRelativeTarget,
                Expected = Expected(link.Expected),
            }).ToArray() ?? [],
            GeneratedRegions = input.Effects?.GeneratedRegions.Select(change => new LibraryGeneratedRegionEffectView
            {
                Path = Relative(input.Workspace, change.LogicalPath),
                ExpectedSha256 = change.Expectation.ContentHash ?? string.Empty,
                IntendedSha256 = FileExpectation.Hash(change.IntendedBytes.AsSpan()),
                Expected = Expected(change.Expectation),
            }).ToArray() ?? [],
            RecordEffect = input.Effects?.RecordChange?.Kind switch
            {
                null => LibraryRecordEffect.None,
                PlannedFileChangeKind.Create => LibraryRecordEffect.Create,
                PlannedFileChangeKind.Replace or PlannedFileChangeKind.ReplaceGeneratedRegion => LibraryRecordEffect.Replace,
                PlannedFileChangeKind.Delete => LibraryRecordEffect.Delete,
                _ => throw new ArgumentOutOfRangeException(nameof(input), input.Effects?.RecordChange?.Kind, "The record effect is not defined."),
            },
            RecordExpected = input.Effects?.RecordChange is { } record ? Expected(record.Expectation) : null,
        };

    internal static LibraryMutationApplication Application(
        CliWorkspace workspace,
        LibraryExecutionEvidence execution,
        int plannedEffectCount)
    {
        var receipts = execution.Directories.Select(receipt => new LibraryMutationEffectReceipt(
                Relative(workspace, receipt.Creation.LogicalPath), LibraryResidualKind.Directory,
                receipt.EffectState, receipt.VerificationState))
            .Concat(execution.Links.Select(receipt => new LibraryMutationEffectReceipt(
                receipt.Effect.DestinationPath.Value, LibraryResidualKind.Link,
                receipt.EffectState, receipt.VerificationState)))
            .Concat(execution.GeneratedRegions.Select(receipt => new LibraryMutationEffectReceipt(
                Relative(workspace, receipt.Change.LogicalPath), LibraryResidualKind.GeneratedRegion,
                receipt.EffectState, receipt.VerificationState)))
            .Concat(execution.Record is null
                ? []
                : [new LibraryMutationEffectReceipt(
                    Relative(workspace, execution.Record.Change.LogicalPath), LibraryResidualKind.Record,
                    execution.Record.EffectState, execution.Record.VerificationState)])
            .ToArray();
        var permissionReceipt = execution.Permission?.Receipt;
        var permissionVerified = permissionReceipt is null
            || permissionReceipt.EffectState == FilesystemEffectState.Applied && permissionReceipt.VerificationState == FilesystemVerificationState.Verified;
        var allVerified = plannedEffectCount == receipts.Length && permissionVerified
            && receipts.All(receipt => receipt.Effect == FilesystemEffectState.Applied
                && receipt.Verification == FilesystemVerificationState.Verified);
        var failed = execution.UnexpectedFailure is not null || execution.Permission?.Failure is not null || !permissionVerified
            || receipts.Any(receipt => receipt.Effect == FilesystemEffectState.Unknown
                || receipt.Verification == FilesystemVerificationState.Failed);
        var residuals = (failed || execution.Cancellation is not null
                ? receipts.Where(receipt => receipt.Effect != FilesystemEffectState.NotStarted)
                    .Select(receipt => new LibraryResidualView
                    {
                        Path = receipt.Path,
                        Kind = receipt.Kind,
                        State = receipt.Effect == FilesystemEffectState.Unknown
                            || receipt.Verification == FilesystemVerificationState.Failed
                            ? LibraryResidualState.Unknown
                            : LibraryResidualState.Retained,
                    })
                : [])
            .ToList();
        var recovery = Recovery(execution, residuals);
        var totalEffects = plannedEffectCount + (permissionReceipt is null ? 0 : 1);
        return new LibraryMutationApplication
        {
            State = ReadApplicationState(execution, failed, allVerified, totalEffects),
            Verification = ReadVerificationState(totalEffects, allVerified, failed),
            Recovery = recovery,
            Residuals = [.. residuals.OrderBy(residual => residual.Path, StringComparer.Ordinal).ThenBy(residual => residual.Kind)],
            RecordPublication = Publication(execution.Record, execution.RecordPublicationOrder),
        };
    }

    private static LibraryApplicationState ReadApplicationState(LibraryExecutionEvidence execution, bool failed, bool allVerified, int effectCount)
    {
        if (execution.Cancellation is not null && execution.UnexpectedFailure is null)
        {
            return LibraryApplicationState.Interrupted;
        }
        if (failed || !allVerified && effectCount > 0)
        {
            return LibraryApplicationState.Failed;
        }
        return effectCount == 0 ? LibraryApplicationState.NoOp : LibraryApplicationState.Applied;
    }

    private static LibraryVerificationState ReadVerificationState(int effectCount, bool allVerified, bool failed)
    {
        if (effectCount == 0)
        {
            return LibraryVerificationState.NotStarted;
        }
        if (allVerified)
        {
            return LibraryVerificationState.Verified;
        }
        return failed ? LibraryVerificationState.Failed : LibraryVerificationState.Unavailable;
    }

    internal static CliSemanticStatus Status(
        LibraryPlanState planState,
        IEnumerable<CliSemanticStatus> findingStatuses,
        LibraryExecutionEvidence execution,
        LibraryMutationApplication application,
        bool dryRun,
        IReadOnlyList<WorkspaceRelativeDirectory> protectedSources)
    {
        if (execution.UnexpectedFailure is not null
            || execution.RecordPublicationOrder == LibraryRecordPublicationOrder.NotLast
            || !SafeScope(execution.SourceEffectScope, protectedSources))
        {
            return CliSemanticStatus.Failed;
        }

        if (execution.Cancellation is not null || execution.Permission?.Failure == LibraryPermissionFailure.Interrupted)
        {
            return CliSemanticStatus.Interrupted;
        }

        var statuses = findingStatuses.ToArray();
        if (statuses.Contains(CliSemanticStatus.Failed))
        {
            return CliSemanticStatus.Failed;
        }
        if (statuses.Contains(CliSemanticStatus.Interrupted))
        {
            return CliSemanticStatus.Interrupted;
        }
        if (statuses.Contains(CliSemanticStatus.Blocked))
        {
            return CliSemanticStatus.Blocked;
        }

        if (statuses.Contains(CliSemanticStatus.Invalid))
        {
            return CliSemanticStatus.Invalid;
        }

        if (statuses.Contains(CliSemanticStatus.Incomplete) || planState == LibraryPlanState.Incomplete)
        {
            return CliSemanticStatus.Incomplete;
        }

        if (planState == LibraryPlanState.Blocked)
        {
            return CliSemanticStatus.Blocked;
        }

        if (dryRun)
        {
            return CliSemanticStatus.Complete;
        }

        if (application.Recovery.State == LibraryRecoveryState.Unknown
            || application.State == LibraryApplicationState.Failed)
        {
            return CliSemanticStatus.Failed;
        }

        if (application.State == LibraryApplicationState.Interrupted)
        {
            return CliSemanticStatus.Interrupted;
        }

        return application.Recovery.State == LibraryRecoveryState.Retained
            ? CliSemanticStatus.Attention
            : CliSemanticStatus.Complete;
    }

    internal static LibraryMutationApplication NotStarted()
        => new()
        {
            State = LibraryApplicationState.NotStarted,
            Verification = LibraryVerificationState.NotStarted,
            Recovery = new LibraryRecoveryView { State = LibraryRecoveryState.NotRequested, Path = null },
            Residuals = [],
            RecordPublication = new LibraryRecordPublication
            {
                State = LibraryRecordPublicationState.NotStarted,
                PublishedLast = null,
            },
        };

    private static LibrariesRecordDocument Document(LibrariesRecord record)
        => new()
        {
            SchemaVersion = record.SchemaVersion,
            Libraries = [.. record.Libraries.Select(library => new LibraryRecordDocument
            {
                Id = library.Id.Value,
                SourceRoot = library.SourceRoot.Value,
                DestinationRoot = library.DestinationRoot.Value,
                Paths = [.. library.Paths.Select(path => path.Value)],
            })],
        };

    private static LibraryComparisonRelation Relation(
        LibraryMappingObservationState state,
        bool registered,
        bool eligible,
        bool sourceIndependent)
    {
        if (state == LibraryMappingObservationState.Blocked)
        {
            return LibraryComparisonRelation.Blocked;
        }

        if (state == LibraryMappingObservationState.Unavailable)
        {
            return LibraryComparisonRelation.Unavailable;
        }

        if (registered && (eligible || sourceIndependent))
        {
            return state switch
            {
                LibraryMappingObservationState.Current => sourceIndependent
                    ? LibraryComparisonRelation.Retired
                    : LibraryComparisonRelation.Current,
                LibraryMappingObservationState.Missing => LibraryComparisonRelation.Missing,
                LibraryMappingObservationState.Changed => LibraryComparisonRelation.Changed,
                _ => throw new ArgumentOutOfRangeException(nameof(state), state, "The mapping state is not defined."),
            };
        }

        return eligible ? LibraryComparisonRelation.Added : LibraryComparisonRelation.Retired;
    }

    private static LibraryExpectedState Expected(FileExpectation expectation)
        => new()
        {
            Kind = expectation.Kind switch
            {
                FileExpectationKind.Missing => LibraryExpectedStateKind.Missing,
                FileExpectationKind.File => LibraryExpectedStateKind.OrdinaryFile,
                FileExpectationKind.Directory => throw new ArgumentOutOfRangeException(
                    nameof(expectation), expectation.Kind, "A planned file expectation cannot be a directory."),
                _ => throw new ArgumentOutOfRangeException(nameof(expectation), expectation.Kind, "The file expectation is not defined."),
            },
            Length = null,
            Sha256 = expectation.ContentHash,
            RawRelativeTarget = null,
        };

    private static LibraryExpectedState Expected(RelativeFileLinkState state)
        => new()
        {
            Kind = state.State switch
            {
                NoFollowLeafState.Missing => LibraryExpectedStateKind.Missing,
                NoFollowLeafState.RelativeFileLink => LibraryExpectedStateKind.RelativeFileLink,
                _ => throw new ArgumentOutOfRangeException(nameof(state), state.State, "The link expectation is not defined."),
            },
            Length = null,
            Sha256 = null,
            RawRelativeTarget = state.Link?.RawRelativeTarget,
        };

    private static LibraryRecoveryView Recovery(
        LibraryExecutionEvidence execution,
        List<LibraryResidualView> residuals)
    {
        if (execution.RecoveryCleanup is { } cleanup)
        {
            var state = cleanup.Disposition switch
            {
                RecoveryBundleDisposition.Removed => LibraryRecoveryState.Removed,
                RecoveryBundleDisposition.Retained => LibraryRecoveryState.Retained,
                RecoveryBundleDisposition.Unknown => LibraryRecoveryState.Unknown,
                _ => throw new ArgumentOutOfRangeException(nameof(execution), cleanup.Disposition, "The recovery disposition is not defined."),
            };
            var path = cleanup.ResidualPath ?? execution.RecoveryPreparation?.BundlePath;
            if (state != LibraryRecoveryState.Removed && path is not null)
            {
                residuals.Add(new LibraryResidualView
                {
                    Path = path,
                    Kind = LibraryResidualKind.Recovery,
                    State = state == LibraryRecoveryState.Retained
                        ? LibraryResidualState.Retained
                        : LibraryResidualState.Unknown,
                });
            }

            return new LibraryRecoveryView { State = state, Path = path };
        }

        return execution.RecoveryPreparation is null
            ? new LibraryRecoveryView { State = LibraryRecoveryState.NotRequested, Path = null }
            : new LibraryRecoveryView
            {
                State = LibraryRecoveryState.Prepared,
                Path = execution.RecoveryPreparation.BundlePath,
            };
    }

    private static LibraryRecordPublication Publication(
        FileChangeReceipt? receipt,
        LibraryRecordPublicationOrder order)
        => new()
        {
            State = ReadPublicationState(receipt),
            PublishedLast = order switch
            {
                LibraryRecordPublicationOrder.Last => true,
                LibraryRecordPublicationOrder.NotLast => false,
                LibraryRecordPublicationOrder.NotObserved => null,
                _ => throw new ArgumentOutOfRangeException(nameof(order), order, "The record publication order is not defined."),
            },
        };

    private static LibraryRecordPublicationState ReadPublicationState(FileChangeReceipt? receipt)
    {
        if (receipt is null)
        {
            return LibraryRecordPublicationState.NotStarted;
        }
        if (receipt.EffectState == FilesystemEffectState.Unknown)
        {
            return LibraryRecordPublicationState.Unknown;
        }
        return receipt.EffectState == FilesystemEffectState.Applied && receipt.VerificationState == FilesystemVerificationState.Verified
            ? LibraryRecordPublicationState.Verified
            : LibraryRecordPublicationState.Failed;
    }

    private static bool SafeScope(
        LibrarySourceEffectScopeFacts? scope,
        IReadOnlyList<WorkspaceRelativeDirectory> protectedSources)
    {
        if (scope is null)
        {
            return true;
        }

        if (!scope.IsComplete)
        {
            return false;
        }

        var sources = protectedSources.Concat(scope.ProtectedSourceRoots)
            .Select(source => PortableWorkspacePath.CreatePortableKey(source.Value)).Distinct(StringComparer.Ordinal).ToArray();
        return scope.AttemptedMutationTargets.All(target =>
        {
            var key = PortableWorkspacePath.CreatePortableKey(target.Value);
            return sources.All(source => key != source && !key.StartsWith($"{source}/", StringComparison.Ordinal));
        });
    }

    private static string Relative(CliWorkspace workspace, string logicalPath)
        => Path.GetRelativePath(workspace.LexicalRoot, logicalPath).Replace(Path.DirectorySeparatorChar, '/');
}
