using System.Reflection;
using OpenForge.Cli.Core.Commands.Install;
using OpenForge.Cli.Core.Commands.Install.Models.Binding;
using OpenForge.Cli.Core.Commands.Install.Models.Operation;
using OpenForge.Cli.Core.Commands.Install.Models.Planning;
using OpenForge.Cli.Core.Commands.Install.Models.Request;
using OpenForge.Cli.Core.Commands.Install.Models.Result;
using OpenForge.Cli.Core.Framework.Workspace;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.Core.UnitTests.Commands.Install;

public sealed class InstallResultContractTests
{
    [Fact(DisplayName = "Install source and footprint facts preserve identity and reject invalid counts"), Trait("Feature", "install-result"), Trait("Evidence", "Unit")]
    public void SourceAndFootprintFactsAreAtomic()
    {
        var source = new InstallSource("embedded-framework-v1", 7);
        var footprint = new InstallFootprint(4, 2, 3);

        Assert.Equal("embedded-framework-v1", source.InventoryFingerprint);
        Assert.Equal(7, source.AssetCount);
        Assert.Equal(4, footprint.PayloadFiles);
        Assert.Equal(2, footprint.ManagedRegions);
        Assert.Equal(3, footprint.GeneratedRegions);

        Assert.Throws<ArgumentException>(() => new InstallSource(" ", 0));
        Assert.Throws<ArgumentOutOfRangeException>(() => new InstallSource("embedded-framework-v1", -1));
        Assert.Throws<ArgumentOutOfRangeException>(() => new InstallFootprint(-1, 0, 0));
        Assert.Throws<ArgumentOutOfRangeException>(() => new InstallFootprint(0, -1, 0));
        Assert.Throws<ArgumentOutOfRangeException>(() => new InstallFootprint(0, 0, -1));
    }

    [Fact(DisplayName = "Install effects preserve exact kind/action pairs and canonical source provenance"), Trait("Feature", "install-result"), Trait("Evidence", "Unit")]
    public void EffectFactsPreserveKindActionAndProvenance()
    {
        var inputs = new[]
        {
            new InstallEffectInput
            {
                Path = ".agents",
                Kind = InstallEffectKind.Directory,
                Action = InstallEffectAction.Create,
                SourceAssetPath = null,
                Outcome = InstallEffectOutcome.Planned,
                Residual = InstallEffectResidual.None,
            },
            new InstallEffectInput
            {
                Path = ".agents/loader.md",
                Kind = InstallEffectKind.File,
                Action = InstallEffectAction.Create,
                SourceAssetPath = "framework/loader.md",
                Outcome = InstallEffectOutcome.Verified,
                Residual = InstallEffectResidual.None,
            },
            new InstallEffectInput
            {
                Path = ".agents/loader.md",
                Kind = InstallEffectKind.File,
                Action = InstallEffectAction.Replace,
                SourceAssetPath = "framework/loader.md",
                Outcome = InstallEffectOutcome.Verified,
                Residual = InstallEffectResidual.None,
            },
            new InstallEffectInput
            {
                Path = "AGENTS.md",
                Kind = InstallEffectKind.ManagedRegion,
                Action = InstallEffectAction.Append,
                SourceAssetPath = "AGENTS.md",
                Outcome = InstallEffectOutcome.Planned,
                Residual = InstallEffectResidual.None,
            },
            new InstallEffectInput
            {
                Path = "AGENTS.md",
                Kind = InstallEffectKind.ManagedRegion,
                Action = InstallEffectAction.Replace,
                SourceAssetPath = "AGENTS.md",
                Outcome = InstallEffectOutcome.Verified,
                Residual = InstallEffectResidual.None,
            },
            new InstallEffectInput
            {
                Path = ".agents/memory/_memory.md",
                Kind = InstallEffectKind.GeneratedRegion,
                Action = InstallEffectAction.Append,
                SourceAssetPath = null,
                Outcome = InstallEffectOutcome.Planned,
                Residual = InstallEffectResidual.None,
            },
            new InstallEffectInput
            {
                Path = ".agents/memory/_memory.md",
                Kind = InstallEffectKind.GeneratedRegion,
                Action = InstallEffectAction.Replace,
                SourceAssetPath = null,
                Outcome = InstallEffectOutcome.Verified,
                Residual = InstallEffectResidual.None,
            },
        };

        var effects = inputs.Select(input => new InstallEffect(input)).ToArray();

        Assert.Equal(inputs.Select(input => input.Path), effects.Select(effect => effect.Path));
        Assert.Equal(inputs.Select(input => input.Kind), effects.Select(effect => effect.Kind));
        Assert.Equal(inputs.Select(input => input.Action), effects.Select(effect => effect.Action));
        Assert.Equal(inputs.Select(input => input.SourceAssetPath), effects.Select(effect => effect.SourceAssetPath));
        Assert.Equal(inputs.Select(input => input.Outcome), effects.Select(effect => effect.Outcome));
        Assert.Equal(inputs.Select(input => input.Residual), effects.Select(effect => effect.Residual));
    }

    [Fact(DisplayName = "Install effects reject undefined enums, invalid actions, and non-canonical target or source paths"), Trait("Feature", "install-result"), Trait("Evidence", "Unit")]
    public void EffectFactsRejectUnsafeValues()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new InstallEffect(new InstallEffectInput
        {
            Path = "AGENTS.md",
            Kind = (InstallEffectKind)int.MaxValue,
            Action = InstallEffectAction.Create,
            SourceAssetPath = null,
            Outcome = InstallEffectOutcome.Planned,
            Residual = InstallEffectResidual.None,
        }));
        Assert.ThrowsAny<ArgumentException>(() => new InstallEffect(new InstallEffectInput
        {
            Path = "AGENTS.md",
            Kind = InstallEffectKind.Directory,
            Action = InstallEffectAction.Append,
            SourceAssetPath = null,
            Outcome = InstallEffectOutcome.Planned,
            Residual = InstallEffectResidual.None,
        }));
        Assert.ThrowsAny<ArgumentException>(() => new InstallEffect(new InstallEffectInput
        {
            Path = "AGENTS.md",
            Kind = InstallEffectKind.File,
            Action = InstallEffectAction.Append,
            SourceAssetPath = "framework/loader.md",
            Outcome = InstallEffectOutcome.Planned,
            Residual = InstallEffectResidual.None,
        }));
        Assert.ThrowsAny<ArgumentException>(() => new InstallEffect(new InstallEffectInput
        {
            Path = "AGENTS.md",
            Kind = InstallEffectKind.ManagedRegion,
            Action = InstallEffectAction.Create,
            SourceAssetPath = "AGENTS.md",
            Outcome = InstallEffectOutcome.Planned,
            Residual = InstallEffectResidual.None,
        }));
        Assert.ThrowsAny<ArgumentException>(() => new InstallEffect(new InstallEffectInput
        {
            Path = "AGENTS.md",
            Kind = InstallEffectKind.GeneratedRegion,
            Action = InstallEffectAction.Create,
            SourceAssetPath = null,
            Outcome = InstallEffectOutcome.Planned,
            Residual = InstallEffectResidual.None,
        }));
        Assert.ThrowsAny<ArgumentException>(() => new InstallEffect(new InstallEffectInput
        {
            Path = "AGENTS.md",
            Kind = InstallEffectKind.File,
            Action = (InstallEffectAction)int.MaxValue,
            SourceAssetPath = null,
            Outcome = InstallEffectOutcome.Planned,
            Residual = InstallEffectResidual.None,
        }));

        Assert.Throws<ArgumentOutOfRangeException>(() => new InstallEffect(new InstallEffectInput
        {
            Path = "AGENTS.md",
            Kind = InstallEffectKind.File,
            Action = InstallEffectAction.Create,
            SourceAssetPath = null,
            Outcome = (InstallEffectOutcome)int.MaxValue,
            Residual = InstallEffectResidual.None,
        }));
        Assert.Throws<ArgumentOutOfRangeException>(() => new InstallEffect(new InstallEffectInput
        {
            Path = "AGENTS.md",
            Kind = InstallEffectKind.File,
            Action = InstallEffectAction.Create,
            SourceAssetPath = null,
            Outcome = InstallEffectOutcome.Planned,
            Residual = (InstallEffectResidual)int.MaxValue,
        }));

        foreach (var path in new[] { "", "./AGENTS.md", "x/../AGENTS.md", Path.GetFullPath("AGENTS.md") })
        {
            Assert.Throws<ArgumentException>(() => new InstallEffect(new InstallEffectInput
            {
                Path = path,
                Kind = InstallEffectKind.File,
                Action = InstallEffectAction.Create,
                SourceAssetPath = null,
                Outcome = InstallEffectOutcome.Planned,
                Residual = InstallEffectResidual.None,
            }));
        }

        foreach (var sourcePath in new[] { "", "./framework/loader.md", "x/../framework/loader.md", Path.GetFullPath("framework/loader.md") })
        {
            Assert.Throws<ArgumentException>(() => new InstallEffect(new InstallEffectInput
            {
                Path = ".agents/loader.md",
                Kind = InstallEffectKind.File,
                Action = InstallEffectAction.Create,
                SourceAssetPath = sourcePath,
                Outcome = InstallEffectOutcome.Planned,
                Residual = InstallEffectResidual.None,
            }));
        }
    }

    [Fact(DisplayName = "Install lifecycle, recovery, and verification facts preserve nullable state rules"), Trait("Feature", "install-result"), Trait("Evidence", "Unit")]
    public void LifecycleRecoveryAndVerificationFactsAreCoherent()
    {
        var lifecycle = new InstallLifecycle(
            InstallLifecycleAction.Preserve,
            InstallLifecycleOutcome.AlreadyCurrent);
        Assert.Equal(InstallLifecycleAction.Preserve, lifecycle.Action);
        Assert.Equal(InstallLifecycleOutcome.AlreadyCurrent, lifecycle.Outcome);

        Assert.Throws<ArgumentOutOfRangeException>(() => new InstallLifecycle(
            (InstallLifecycleAction)int.MaxValue,
            InstallLifecycleOutcome.NotRequested));
        Assert.Throws<ArgumentOutOfRangeException>(() => new InstallLifecycle(
            InstallLifecycleAction.None,
            (InstallLifecycleOutcome)int.MaxValue));

        var removed = new InstallRecovery(InstallResultRecoveryState.Removed, null);
        var retainedPath = Path.GetFullPath(Path.Combine(
            Path.GetTempPath(),
            "open-forge-install-contract",
            "bundle.zip"));
        var retained = new InstallRecovery(InstallResultRecoveryState.Retained, retainedPath);
        Assert.Equal(InstallResultRecoveryState.Removed, removed.State);
        Assert.Null(removed.ResidualPath);
        Assert.Equal(InstallResultRecoveryState.Retained, retained.State);
        Assert.Equal(retainedPath, retained.ResidualPath);

        Assert.Throws<ArgumentException>(() => new InstallRecovery(
            InstallResultRecoveryState.NotCreated,
            retainedPath));
        Assert.Throws<ArgumentException>(() => new InstallRecovery(
            InstallResultRecoveryState.Retained,
            null));
        Assert.Throws<ArgumentException>(() => new InstallRecovery(
            InstallResultRecoveryState.Retained,
            "relative/bundle.zip"));
        Assert.Throws<ArgumentException>(() => new InstallRecovery(
            InstallResultRecoveryState.Retained,
            Path.Combine(Path.GetTempPath(), "..", "bundle.zip")));
        Assert.Throws<ArgumentOutOfRangeException>(() => new InstallRecovery(
            (InstallResultRecoveryState)int.MaxValue,
            null));

        var verification = new InstallVerification(InstallResultVerificationState.NotRequested);
        Assert.Equal(InstallResultVerificationState.NotRequested, verification.State);
        Assert.Throws<ArgumentOutOfRangeException>(() => new InstallVerification(
            (InstallResultVerificationState)int.MaxValue));
    }

    [Fact(DisplayName = "Install result facts represent invalid, dry-run, apply, exact, divergence, force, interrupted, failed, and attention summaries with every required nested fact"), Trait("Feature", "install-result"), Trait("Evidence", "Unit")]
    public void RepresentativeResultFactsAreComplete()
    {
        var source = new InstallSource("embedded-framework-v1", 7);
        var footprint = new InstallFootprint(4, 2, 3);
        var retainedPath = Path.GetFullPath(Path.Combine(
            Path.GetTempPath(),
            "open-forge-install-contract",
            "bundle.zip"));
        var cases = new[]
        {
            new
            {
                Name = "invalid",
                ExpectedClassification = (InstallManagementClassification?)null,
                Facts = new InstallResultFactsInput
                {
                    Source = null,
                    Classification = null,
                    Footprint = null,
                    Effects = Array.Empty<InstallEffect>(),
                    Lifecycle = new InstallLifecycle(InstallLifecycleAction.None, InstallLifecycleOutcome.NotRequested),
                    Recovery = new InstallRecovery(InstallResultRecoveryState.NotRequired, null),
                    Verification = new InstallVerification(InstallResultVerificationState.NotRequested),
                },
            },
            new
            {
                Name = "safe-absence-dry-run",
                ExpectedClassification = (InstallManagementClassification?)InstallManagementClassification.SafeAbsence,
                Facts = new InstallResultFactsInput
                {
                    Source = source,
                    Classification = InstallManagementClassification.SafeAbsence,
                    Footprint = footprint,
                    Effects = [Effect(".agents/loader.md", InstallEffectKind.File, InstallEffectAction.Create, "framework/loader.md", InstallEffectOutcome.Planned, InstallEffectResidual.None)],
                    Lifecycle = new InstallLifecycle(InstallLifecycleAction.Publish, InstallLifecycleOutcome.Planned),
                    Recovery = new InstallRecovery(InstallResultRecoveryState.NotRequired, null),
                    Verification = new InstallVerification(InstallResultVerificationState.NotRequested),
                },
            },
            new
            {
                Name = "safe-absence-apply",
                ExpectedClassification = (InstallManagementClassification?)InstallManagementClassification.SafeAbsence,
                Facts = new InstallResultFactsInput
                {
                    Source = source,
                    Classification = InstallManagementClassification.SafeAbsence,
                    Footprint = footprint,
                    Effects = [Effect(".agents/loader.md", InstallEffectKind.File, InstallEffectAction.Create, "framework/loader.md", InstallEffectOutcome.Verified, InstallEffectResidual.None)],
                    Lifecycle = new InstallLifecycle(InstallLifecycleAction.Publish, InstallLifecycleOutcome.Verified),
                    Recovery = new InstallRecovery(InstallResultRecoveryState.NotRequired, null),
                    Verification = new InstallVerification(InstallResultVerificationState.Verified),
                },
            },
            new
            {
                Name = "trusted-exact-no-op",
                ExpectedClassification = (InstallManagementClassification?)InstallManagementClassification.TrustedExact,
                Facts = new InstallResultFactsInput
                {
                    Source = source,
                    Classification = InstallManagementClassification.TrustedExact,
                    Footprint = footprint,
                    Effects = Array.Empty<InstallEffect>(),
                    Lifecycle = new InstallLifecycle(InstallLifecycleAction.Preserve, InstallLifecycleOutcome.AlreadyCurrent),
                    Recovery = new InstallRecovery(InstallResultRecoveryState.NotRequired, null),
                    Verification = new InstallVerification(InstallResultVerificationState.Verified),
                },
            },
            new
            {
                Name = "managed-divergence",
                ExpectedClassification = (InstallManagementClassification?)InstallManagementClassification.ManagedDivergence,
                Facts = new InstallResultFactsInput
                {
                    Source = source,
                    Classification = InstallManagementClassification.ManagedDivergence,
                    Footprint = footprint,
                    Effects = Array.Empty<InstallEffect>(),
                    Lifecycle = new InstallLifecycle(InstallLifecycleAction.Preserve, InstallLifecycleOutcome.NotRequested),
                    Recovery = new InstallRecovery(InstallResultRecoveryState.NotRequired, null),
                    Verification = new InstallVerification(InstallResultVerificationState.NotRequested),
                },
            },
            new
            {
                Name = "eligible-initial-force",
                ExpectedClassification = (InstallManagementClassification?)InstallManagementClassification.EligibleInitialOccupant,
                Facts = new InstallResultFactsInput
                {
                    Source = source,
                    Classification = InstallManagementClassification.EligibleInitialOccupant,
                    Footprint = footprint,
                    Effects = [Effect(".agents/loader.md", InstallEffectKind.File, InstallEffectAction.Replace, "framework/loader.md", InstallEffectOutcome.Verified, InstallEffectResidual.None)],
                    Lifecycle = new InstallLifecycle(InstallLifecycleAction.Publish, InstallLifecycleOutcome.Verified),
                    Recovery = new InstallRecovery(InstallResultRecoveryState.Removed, null),
                    Verification = new InstallVerification(InstallResultVerificationState.Verified),
                },
            },
            new
            {
                Name = "interrupted",
                ExpectedClassification = (InstallManagementClassification?)InstallManagementClassification.SafeAbsence,
                Facts = new InstallResultFactsInput
                {
                    Source = source,
                    Classification = InstallManagementClassification.SafeAbsence,
                    Footprint = footprint,
                    Effects = [Effect(".agents/loader.md", InstallEffectKind.File, InstallEffectAction.Create, "framework/loader.md", InstallEffectOutcome.NotStarted, InstallEffectResidual.Retained)],
                    Lifecycle = new InstallLifecycle(InstallLifecycleAction.Publish, InstallLifecycleOutcome.NotStarted),
                    Recovery = new InstallRecovery(InstallResultRecoveryState.Retained, retainedPath),
                    Verification = new InstallVerification(InstallResultVerificationState.Unknown),
                },
            },
            new
            {
                Name = "failed",
                ExpectedClassification = (InstallManagementClassification?)InstallManagementClassification.SafeAbsence,
                Facts = new InstallResultFactsInput
                {
                    Source = source,
                    Classification = InstallManagementClassification.SafeAbsence,
                    Footprint = footprint,
                    Effects = [Effect(".agents/loader.md", InstallEffectKind.File, InstallEffectAction.Replace, "framework/loader.md", InstallEffectOutcome.VerificationFailed, InstallEffectResidual.Unknown)],
                    Lifecycle = new InstallLifecycle(InstallLifecycleAction.Publish, InstallLifecycleOutcome.VerificationFailed),
                    Recovery = new InstallRecovery(InstallResultRecoveryState.Unknown, null),
                    Verification = new InstallVerification(InstallResultVerificationState.Failed),
                },
            },
            new
            {
                Name = "attention",
                ExpectedClassification = (InstallManagementClassification?)InstallManagementClassification.SafeAbsence,
                Facts = new InstallResultFactsInput
                {
                    Source = source,
                    Classification = InstallManagementClassification.SafeAbsence,
                    Footprint = footprint,
                    Effects = [Effect(".agents/loader.md", InstallEffectKind.File, InstallEffectAction.Replace, "framework/loader.md", InstallEffectOutcome.Verified, InstallEffectResidual.None)],
                    Lifecycle = new InstallLifecycle(InstallLifecycleAction.Publish, InstallLifecycleOutcome.Verified),
                    Recovery = new InstallRecovery(InstallResultRecoveryState.Retained, retainedPath),
                    Verification = new InstallVerification(InstallResultVerificationState.Verified),
                },
            },
        };

        foreach (var testCase in cases)
        {
            var facts = new InstallResultFacts(testCase.Facts);
            Assert.Equal(testCase.ExpectedClassification, facts.Classification);
            Assert.NotNull(facts.Effects);
            Assert.NotNull(facts.Lifecycle);
            Assert.NotNull(facts.Recovery);
            Assert.NotNull(facts.Verification);
        }
    }

    [Fact(DisplayName = "Install result facts materialize an immutable complete graph without changing effect order"), Trait("Feature", "install-result"), Trait("Evidence", "Unit")]
    public void ResultFactsMaterializeAtomicGraph()
    {
        var source = new InstallSource("embedded-framework-v1", 7);
        var footprint = new InstallFootprint(4, 2, 3);
        var effects = new List<InstallEffect>
        {
            Effect(".agents/loader.md", InstallEffectKind.File, InstallEffectAction.Create, "framework/loader.md"),
            Effect("AGENTS.md", InstallEffectKind.ManagedRegion, InstallEffectAction.Append, "AGENTS.md"),
            Effect(".agents/memory/_memory.md", InstallEffectKind.GeneratedRegion, InstallEffectAction.Replace, null),
        };
        var lifecycle = new InstallLifecycle(
            InstallLifecycleAction.Publish,
            InstallLifecycleOutcome.Verified);
        var recovery = new InstallRecovery(InstallResultRecoveryState.NotRequired, null);
        var verification = new InstallVerification(InstallResultVerificationState.Verified);

        var facts = new InstallResultFacts(new InstallResultFactsInput
        {
            Source = source,
            Classification = InstallManagementClassification.SafeAbsence,
            Footprint = footprint,
            Effects = effects,
            Lifecycle = lifecycle,
            Recovery = recovery,
            Verification = verification,
        });

        Assert.Same(source, facts.Source);
        Assert.Equal(InstallManagementClassification.SafeAbsence, facts.Classification);
        Assert.Same(footprint, facts.Footprint);
        Assert.Equal(effects.Select(effect => effect.Path), facts.Effects.Select(effect => effect.Path));
        Assert.NotSame(effects, facts.Effects);
        Assert.Same(lifecycle, facts.Lifecycle);
        Assert.Same(recovery, facts.Recovery);
        Assert.Same(verification, facts.Verification);

        effects.Add(Effect(".agents/new.md", InstallEffectKind.File, InstallEffectAction.Create, "framework/new.md"));
        Assert.Equal(3, facts.Effects.Count);

        Assert.Throws<ArgumentException>(() => new InstallResultFacts(new InstallResultFactsInput
        {
            Source = source,
            Classification = InstallManagementClassification.SafeAbsence,
            Footprint = footprint,
            Effects = [effects[0], null!],
            Lifecycle = lifecycle,
            Recovery = recovery,
            Verification = verification,
        }));
        Assert.Throws<ArgumentOutOfRangeException>(() => new InstallResultFacts(new InstallResultFactsInput
        {
            Source = source,
            Classification = (InstallManagementClassification)int.MaxValue,
            Footprint = footprint,
            Effects = [],
            Lifecycle = lifecycle,
            Recovery = recovery,
            Verification = verification,
        }));
    }

    [Fact(DisplayName = "Install result formation exposes complete facts for invalid, dry-run, apply, exact, divergence, force, interrupted, failed, and attention outcomes"), Trait("Feature", "install-result"), Trait("Evidence", "Unit")]
    public void ResultFormationCarriesCompleteFactsForRepresentativeOutcomes()
    {
        var resultProperty = typeof(InstallResult).GetProperty(
            "Facts",
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        Assert.NotNull(
            resultProperty);
        Assert.Equal(typeof(InstallResultFacts), resultProperty!.PropertyType);

        var results = new[]
        {
            InvalidResult(InstallFindingCode.InvalidInput),
            CreatedResult(InstallMode.DryRun, InstallManagementState.SafelyAbsent),
            CreatedResult(InstallMode.Apply, InstallManagementState.SafelyAbsent),
            CreatedResult(InstallMode.Apply, InstallManagementState.TrustedExact),
            CreatedResult(InstallMode.Apply, InstallManagementState.ManagedDivergence, InstallFindingCode.ManagedDivergence),
            CreatedResult(InstallMode.Apply, InstallManagementState.EligibleInitialOccupant),
            CreatedResult(InstallMode.Apply, InstallManagementState.Interrupted, InstallFindingCode.Interrupted),
            CreatedResult(InstallMode.Apply, InstallManagementState.Blocked, InstallFindingCode.OperationFailed),
            CreatedResult(InstallMode.Apply, InstallManagementState.SafelyAbsent, InstallFindingCode.RecoveryArtifactRetained),
        };

        foreach (var result in results)
        {
            var facts = Assert.IsType<InstallResultFacts>(resultProperty.GetValue(result));
            Assert.NotNull(facts.Effects);
            Assert.NotNull(facts.Lifecycle);
            Assert.NotNull(facts.Recovery);
            Assert.NotNull(facts.Verification);
        }
    }

    private static InstallEffect Effect(
        string path,
        InstallEffectKind kind,
        InstallEffectAction action,
        string? sourceAssetPath,
        InstallEffectOutcome outcome = InstallEffectOutcome.Verified,
        InstallEffectResidual residual = InstallEffectResidual.None)
        => new(new InstallEffectInput
        {
            Path = path,
            Kind = kind,
            Action = action,
            SourceAssetPath = sourceAssetPath,
            Outcome = outcome,
            Residual = residual,
        });

    private static InstallResult InvalidResult(InstallFindingCode code)
        => InstallResult.Invalid(
            new InstallBindingInput(false, false, InstallMode.Apply),
            Workspace(),
            [new InstallFinding(code, "The representative Install result condition was observed.")]);

    private static InstallResult CreatedResult(
        InstallMode mode,
        InstallManagementState managementState,
        InstallFindingCode? findingCode = null)
    {
        var automatic = mode == InstallMode.DryRun || findingCode is not null;
        var request = new InstallRequest(
            Workspace(),
            mode,
            force: managementState == InstallManagementState.EligibleInitialOccupant,
            automatic,
            allowsInteractiveConfirmation: false);
        var findings = findingCode is { } code
            ? [new InstallFinding(code, "The representative Install result condition was observed.")]
            : Array.Empty<InstallFinding>();
        return InstallResult.Create(
            request,
            findings,
            new InstallOperationSummary
            {
                ManagementState = managementState,
                PlannedDirectoryCount = 1,
                PlannedFileCount = 3,
                AppliedDirectoryCount = mode == InstallMode.Apply ? 1 : 0,
                AppliedTargetFileCount = mode == InstallMode.Apply ? 3 : 0,
                LifecyclePublished = mode == InstallMode.Apply && findingCode is null,
                RecoveryState = findingCode == InstallFindingCode.RecoveryArtifactRetained
                    ? InstallRecoveryState.Retained
                    : InstallRecoveryState.NotRequired,
                RecoveryResidualPath = findingCode == InstallFindingCode.RecoveryArtifactRetained
                    ? Path.GetFullPath(Path.Combine(Path.GetTempPath(), "install-contract-bundle.zip"))
                    : null,
            });
    }

    private static CliWorkspace Workspace()
    {
        var root = Path.GetFullPath(Path.Combine(Path.GetTempPath(), "install-result-contract"));
        return new CliWorkspace(root, root, CliWorkspaceSelectionMethod.ExplicitWorkspace);
    }
}
