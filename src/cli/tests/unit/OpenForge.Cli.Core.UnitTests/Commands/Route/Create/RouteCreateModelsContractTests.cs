using System.Collections.Immutable;
using OpenForge.Cli.Core.Commands.Route.Create;
using OpenForge.Cli.Core.Commands.Route.Create.Models.Request;
using OpenForge.Cli.Core.Commands.Route.Create.Models.Result;
using OpenForge.Cli.Core.Commands.Route.Create.Shared.Result;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Operation;

namespace OpenForge.Cli.Core.UnitTests.Commands.Route.Create;

public sealed class RouteCreateModelsContractTests
{
    [Fact(DisplayName = "Route Create finite mappings cover every named value and reject undefined values"), Trait("Feature", "route-create"), Trait("Evidence", "UnitContract")]
    public void FiniteMappingsCoverEveryNamedValueAndRejectUndefinedValues()
    {
        Assert.Equal(["apply", "dry-run"], Enum.GetValues<RouteCreateMode>().Select(RouteCreateDefinitions.ReadMachineName));
        Assert.Equal(["canonical", "compatibility"], Enum.GetValues<RouteCreateParentForm>().Select(RouteCreateDefinitions.ReadMachineName));
        Assert.Equal(["template"], Enum.GetValues<RouteCreateTemplateClassification>().Select(RouteCreateDefinitions.ReadMachineName));
        Assert.Equal(["not-established", "incomplete", "complete"], Enum.GetValues<RouteCreatePlanCompleteness>().Select(RouteCreateDefinitions.ReadMachineName));
        Assert.Equal(["not-established", "safe", "blocked"], Enum.GetValues<RouteCreatePlanSafety>().Select(RouteCreateDefinitions.ReadMachineName));
        Assert.Equal(["routed-file", "generated-region"], Enum.GetValues<RouteCreateEffectKind>().Select(RouteCreateDefinitions.ReadMachineName));
        Assert.Equal(["create", "replace"], Enum.GetValues<RouteCreateEffectAction>().Select(RouteCreateDefinitions.ReadMachineName));
        Assert.Equal(["planned", "not-started", "verified", "verification-failed", "completion-unknown"], Enum.GetValues<RouteCreateEffectOutcome>().Select(RouteCreateDefinitions.ReadMachineName));
        Assert.Equal(["none", "retained", "unknown"], Enum.GetValues<RouteCreateEffectResidual>().Select(RouteCreateDefinitions.ReadMachineName));
        Assert.Equal(["not-required", "not-created", "removed", "retained", "unknown"], Enum.GetValues<RouteCreateRecoveryState>().Select(RouteCreateDefinitions.ReadMachineName));
        Assert.Equal(["not-requested", "verified", "failed", "unknown"], Enum.GetValues<RouteCreateVerificationState>().Select(RouteCreateDefinitions.ReadMachineName));
        Assert.Equal(ExpectedFindingNames, Enum.GetValues<RouteCreateFindingCode>().Select(RouteCreateDefinitions.ReadMachineName));

        Assert.Throws<ArgumentOutOfRangeException>(() => RouteCreateDefinitions.ReadMachineName((RouteCreateMode)int.MaxValue));
        Assert.Throws<ArgumentOutOfRangeException>(() => RouteCreateDefinitions.ReadMachineName((RouteCreateParentForm)int.MaxValue));
        Assert.Throws<ArgumentOutOfRangeException>(() => RouteCreateDefinitions.ReadMachineName((RouteCreateTemplateClassification)int.MaxValue));
        Assert.Throws<ArgumentOutOfRangeException>(() => RouteCreateDefinitions.ReadMachineName((RouteCreatePlanCompleteness)int.MaxValue));
        Assert.Throws<ArgumentOutOfRangeException>(() => RouteCreateDefinitions.ReadMachineName((RouteCreatePlanSafety)int.MaxValue));
        Assert.Throws<ArgumentOutOfRangeException>(() => RouteCreateDefinitions.ReadMachineName((RouteCreateEffectKind)int.MaxValue));
        Assert.Throws<ArgumentOutOfRangeException>(() => RouteCreateDefinitions.ReadMachineName((RouteCreateEffectAction)int.MaxValue));
        Assert.Throws<ArgumentOutOfRangeException>(() => RouteCreateDefinitions.ReadMachineName((RouteCreateEffectOutcome)int.MaxValue));
        Assert.Throws<ArgumentOutOfRangeException>(() => RouteCreateDefinitions.ReadMachineName((RouteCreateEffectResidual)int.MaxValue));
        Assert.Throws<ArgumentOutOfRangeException>(() => RouteCreateDefinitions.ReadMachineName((RouteCreateRecoveryState)int.MaxValue));
        Assert.Throws<ArgumentOutOfRangeException>(() => RouteCreateDefinitions.ReadMachineName((RouteCreateVerificationState)int.MaxValue));
        Assert.Throws<ArgumentOutOfRangeException>(() => RouteCreateDefinitions.ReadMachineName((RouteCreateFindingCode)int.MaxValue));
    }

    [Fact(DisplayName = "Route Create finding statuses cover the accepted finite vocabulary"), Trait("Feature", "route-create"), Trait("Evidence", "UnitContract")]
    public void FindingStatusesCoverAcceptedFiniteVocabulary()
    {
        var actual = Enum.GetValues<RouteCreateFindingCode>()
            .Select(RouteCreateDefinitions.ReadStatus)
            .ToArray();

        Assert.Equal(
        [
            CliSemanticStatus.Invalid,
            CliSemanticStatus.Invalid,
            CliSemanticStatus.Invalid,
            CliSemanticStatus.Invalid,
            CliSemanticStatus.Blocked,
            CliSemanticStatus.Blocked,
            CliSemanticStatus.Blocked,
            CliSemanticStatus.Blocked,
            CliSemanticStatus.Blocked,
            CliSemanticStatus.Blocked,
            CliSemanticStatus.Blocked,
            CliSemanticStatus.Blocked,
            CliSemanticStatus.Blocked,
            CliSemanticStatus.Blocked,
            CliSemanticStatus.Blocked,
            CliSemanticStatus.Blocked,
            CliSemanticStatus.Blocked,
            CliSemanticStatus.Incomplete,
            CliSemanticStatus.Incomplete,
            CliSemanticStatus.Incomplete,
            CliSemanticStatus.Incomplete,
            CliSemanticStatus.Incomplete,
            CliSemanticStatus.Attention,
            CliSemanticStatus.Failed,
            CliSemanticStatus.Failed,
            CliSemanticStatus.Failed,
            CliSemanticStatus.Failed,
            CliSemanticStatus.Failed,
            CliSemanticStatus.Interrupted,
        ],
        actual);
        Assert.Throws<ArgumentOutOfRangeException>(() => RouteCreateDefinitions.ReadStatus((RouteCreateFindingCode)int.MaxValue));
    }

    [Fact(DisplayName = "Route Create result builder orders compatible findings deterministically"), Trait("Feature", "route-create"), Trait("Evidence", "UnitContract")]
    public void ResultBuilderOrdersCompatibleFindingsDeterministically()
    {
        var findings = new[]
        {
            RouteCreateTestData.Finding(RouteCreateFindingCode.RouteAmbiguous, target: "z"),
            RouteCreateTestData.Finding(RouteCreateFindingCode.ParentMissing, target: "b"),
            RouteCreateTestData.Finding(RouteCreateFindingCode.ParentMissing, target: "a"),
        };

        var result = new RouteCreateResultBuilder().Build(
            RouteCreateTestData.BoundaryFormation() with
            {
                Findings = [.. findings],
            });

        Assert.Equal(CliSemanticStatus.Blocked, result.Status);
        Assert.Equal(
        [
            (RouteCreateFindingCode.ParentMissing, "a"),
            (RouteCreateFindingCode.ParentMissing, "b"),
            (RouteCreateFindingCode.RouteAmbiguous, "z"),
        ],
        result.Findings.Select(finding => (finding.Code, finding.Target)));
        Assert.Equal("open-forge route init", result.Next?.Command);
    }

    [Fact(DisplayName = "Route Create result builder applies ordinary and event status precedence"), Trait("Feature", "route-create"), Trait("Evidence", "UnitContract")]
    public void ResultBuilderAppliesOrdinaryAndEventStatusPrecedence()
    {
        var ordinary = new RouteCreateResultBuilder().Build(
            RouteCreateTestData.BoundaryFormation() with
            {
                Findings =
                [
                    RouteCreateTestData.Finding(RouteCreateFindingCode.ProjectionIncomplete),
                    RouteCreateTestData.Finding(RouteCreateFindingCode.RouteAmbiguous),
                ],
            });
        var failed = new RouteCreateResultBuilder().Build(
            FailedFormation() with
            {
                Findings = [RouteCreateTestData.Finding(RouteCreateFindingCode.WriteFailed)],
            });
        var interrupted = new RouteCreateResultBuilder().Build(
            RouteCreateTestData.BoundaryFormation() with
            {
                Findings = [RouteCreateTestData.Finding(RouteCreateFindingCode.Interrupted)],
            });
        var retained = RouteCreateTestData.Finding(RouteCreateFindingCode.RecoveryArtifactRetained);
        var attention = new RouteCreateResultBuilder().Build(
            RouteCreateTestData.VerifiedFormation() with
            {
                Recovery = new RouteCreateRecovery
                {
                    State = RouteCreateRecoveryState.Retained,
                    ResidualPath = "/tmp/open-forge-recovery.zip",
                },
                Findings = [retained],
            });

        Assert.Equal(CliSemanticStatus.Blocked, ordinary.Status);
        Assert.Equal(CliSemanticStatus.Failed, failed.Status);
        Assert.Equal(CliSemanticStatus.Interrupted, interrupted.Status);
        Assert.Equal(CliSemanticStatus.Attention, attention.Status);
        Assert.Equal(CliSemanticStatus.Complete, RouteCreateTestData.Result().Status);
    }

    [Fact(DisplayName = "Route Create next guidance follows its accepted precedence"), Trait("Feature", "route-create"), Trait("Evidence", "UnitContract")]
    public void NextGuidanceFollowsAcceptedPrecedence()
    {
        Assert.Equal(
            "open-forge route init",
            Build(RouteCreateFindingCode.TargetContentDiffers, RouteCreateFindingCode.ParentMissing).Next?.Command);
        Assert.Equal("open-forge route update", Build(RouteCreateFindingCode.TargetContentDiffers).Next?.Command);
        Assert.Equal("open-forge route create --help", Build(RouteCreateFindingCode.InvalidMetadata).Next?.Command);
        Assert.Equal("open-forge route create", Build(RouteCreateFindingCode.WorkspaceLockUnavailable).Next?.Command);
        Assert.Equal("open-forge doctor", Build(RouteCreateFindingCode.RouteAmbiguous).Next?.Command);
        Assert.Equal("open-forge doctor", Build(RouteCreateFindingCode.ProjectionIncomplete).Next?.Command);
        var failed = new RouteCreateResultBuilder().Build(
            FailedFormation() with
            {
                Findings = [RouteCreateTestData.Finding(RouteCreateFindingCode.WriteFailed)],
            });
        Assert.Equal("open-forge route create --verbose", failed.Next?.Command);
        Assert.Equal("open-forge route create", Build(RouteCreateFindingCode.Interrupted).Next?.Command);

        var retained = RouteCreateTestData.Finding(RouteCreateFindingCode.RecoveryArtifactRetained);
        var attention = new RouteCreateResultBuilder().Build(
            RouteCreateTestData.VerifiedFormation() with
            {
                Recovery = new RouteCreateRecovery
                {
                    State = RouteCreateRecoveryState.Retained,
                    ResidualPath = "/tmp/open-forge-recovery.zip",
                },
                Findings = [retained],
            });
        Assert.Equal("open-forge cleanup", attention.Next?.Command);
    }

    [Fact(DisplayName = "Route Create metadata owns immutable ordered tag values"), Trait("Feature", "route-create"), Trait("Evidence", "UnitContract")]
    public void MetadataOwnsImmutableOrderedTagValues()
    {
        var source = new List<string> { "Docs", "Overview" };
        var metadata = new RouteCreateMetadataInput(
            description: "Project overview",
            tags: source,
            responsibility: "");

        source[0] = "Changed";

        Assert.Equal(["Docs", "Overview"], metadata.Tags);
        Assert.Null(metadata.Responsibility);
        Assert.Throws<ArgumentException>(() => new RouteCreateMetadataInput("Description", [], null));
        Assert.Throws<ArgumentException>(() => new RouteCreateMetadataInput("Description", ["Docs", "Docs"], null));
        Assert.Throws<ArgumentException>(() => new RouteCreateMetadataInput("Description", ["#Docs"], null));
    }

    [Fact(DisplayName = "Route Create result rejects invalid effect order and attention state"), Trait("Feature", "route-create"), Trait("Evidence", "UnitContract")]
    public void ResultRejectsInvalidEffectOrderAndAttentionState()
    {
        var builder = new RouteCreateResultBuilder();
        Assert.Throws<ArgumentException>(() => builder.Build(
            RouteCreateTestData.PreviewFormation() with
            {
                Effects =
                [
                    RouteCreateTestData.ParentEffect(),
                    RouteCreateTestData.CreateEffect(),
                ],
            }));
        Assert.Throws<ArgumentException>(() => new RouteCreateResult(
            RouteCreateTestData.VerifiedFormation(),
            CliSemanticStatus.Attention,
            new CliNextAction("open-forge cleanup", "Cleanup is required.")));
    }

    [Fact(DisplayName = "Route Create result rejects action-incompatible change facts"), Trait("Feature", "route-create"), Trait("Evidence", "UnitContract")]
    public void ResultRejectsActionIncompatibleChangeFacts()
    {
        var builder = new RouteCreateResultBuilder();

        Assert.Throws<ArgumentException>(() => builder.Build(
            RouteCreateTestData.PreviewFormation() with
            {
                Effects =
                [
                    RouteCreateTestData.CreateEffect() with
                    {
                        Change = new RouteCreateEffectChange
                        {
                            Before = "existing-content-hash",
                            Expected = "target-content-hash",
                        },
                    },
                ],
            }));
        Assert.Throws<ArgumentException>(() => builder.Build(
            RouteCreateTestData.PreviewFormation() with
            {
                Effects =
                [
                    RouteCreateTestData.ParentEffect() with
                    {
                        Change = new RouteCreateEffectChange
                        {
                            Before = null,
                            Expected = "parent-expected-hash",
                        },
                    },
                ],
            }));
    }

    [Fact(DisplayName = "Route Create resolved results require complete destination metadata"), Trait("Feature", "route-create"), Trait("Evidence", "UnitContract")]
    public void ResolvedResultsRequireCompleteDestinationMetadata()
    {
        var builder = new RouteCreateResultBuilder();

        Assert.Throws<ArgumentException>(() => builder.Build(
            RouteCreateTestData.VerifiedFormation() with
            {
                Metadata = new RouteCreateMetadata
                {
                    Description = string.Empty,
                    Responsibility = null,
                    Tags = ["Docs"],
                },
            }));
        Assert.Throws<ArgumentException>(() => builder.Build(
            RouteCreateTestData.VerifiedFormation() with
            {
                Metadata = new RouteCreateMetadata
                {
                    Description = "Project overview",
                    Responsibility = null,
                    Tags = [],
                },
            }));
    }

    private static readonly ImmutableArray<string> ExpectedFindingNames =
    [
        "route-create.invalid-input",
        "route-create.invalid-target",
        "route-create.invalid-metadata",
        "route-create.invalid-template",
        "route-create.workspace-unavailable",
        "route-create.workspace-unsafe",
        "route-create.target-unsafe",
        "route-create.target-content-differs",
        "route-create.parent-missing",
        "route-create.route-ambiguous",
        "route-create.identity-collision",
        "route-create.template-unsafe",
        "route-create.metadata-unsafe",
        "route-create.generated-region-unsafe",
        "route-create.workspace-lock-unavailable",
        "route-create.target-changed",
        "route-create.recovery-conflict",
        "route-create.inspection-incomplete",
        "route-create.template-unavailable",
        "route-create.metadata-incomplete",
        "route-create.projection-incomplete",
        "route-create.recovery-unavailable",
        "route-create.recovery-artifact-retained",
        "route-create.target-changed-during-apply",
        "route-create.write-failed",
        "route-create.verification-failed",
        "route-create.recovery-failed",
        "route-create.operation-failed",
        "route-create.interrupted",
    ];

    private static RouteCreateResult Build(params RouteCreateFindingCode[] codes)
        => new RouteCreateResultBuilder().Build(
            RouteCreateTestData.BoundaryFormation() with
            {
                Findings = codes
                    .Select(code => RouteCreateTestData.Finding(code))
                    .ToImmutableArray(),
            });

    private static RouteCreateResultFormation FailedFormation()
    {
        var preview = RouteCreateTestData.PreviewFormation();
        return preview with
        {
            Effects =
            [
                RouteCreateTestData.CreateEffect() with
                {
                    Outcome = RouteCreateEffectOutcome.CompletionUnknown,
                    Residual = RouteCreateEffectResidual.Unknown,
                },
            ],
            Verification = RouteCreateVerificationState.Unknown,
        };
    }
}
