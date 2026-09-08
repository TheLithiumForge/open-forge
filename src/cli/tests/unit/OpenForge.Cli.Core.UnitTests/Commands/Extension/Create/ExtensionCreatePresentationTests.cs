using System.Globalization;
using OpenForge.Cli.Core.Commands.Extension.Create;
using OpenForge.Cli.Core.Commands.Extension.Create.Models.Manifest;
using OpenForge.Cli.Core.Commands.Extension.Create.Models.Planning;
using OpenForge.Cli.Core.Commands.Extension.Create.Models.Presentation;
using OpenForge.Cli.Core.Commands.Extension.Create.Models.Result;
using OpenForge.Cli.Core.Commands.Extension.Create.Shared.Rendering;
using OpenForge.Cli.Core.Commands.Extension.Create.Models.Request;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Pipeline;

namespace OpenForge.Cli.Core.UnitTests.Commands.Extension.Create;

public sealed class ExtensionCreatePresentationTests
{
    [Theory(DisplayName = "Extension Create maps every named operation mode to its stable machine name"), Trait("Feature", "extension-create"), Trait("Evidence", "Unit")]
    [InlineData(ExtensionCreateMode.Apply, "apply")]
    [InlineData(ExtensionCreateMode.DryRun, "dry-run")]
    public void OperationModesHaveExactMachineNames(
        object modeValue,
        string expected)
    {
        var mode = Assert.IsType<ExtensionCreateMode>(modeValue);
        Assert.Equal(expected, ExtensionCreateDefinitions.ReadMachineName(mode));
    }

    [Theory(DisplayName = "Extension Create maps every named finding code to its stable machine name"), Trait("Feature", "extension-create"), Trait("Evidence", "Unit")]
    [InlineData(ExtensionCreateFindingCode.InvalidInput, "extension-create.invalid-input")]
    [InlineData(ExtensionCreateFindingCode.CatalogueUnavailable, "extension-create.catalogue-unavailable")]
    [InlineData(ExtensionCreateFindingCode.CatalogueUnsafe, "extension-create.catalogue-unsafe")]
    [InlineData(ExtensionCreateFindingCode.DestinationCollision, "extension-create.destination-collision")]
    [InlineData(ExtensionCreateFindingCode.DestinationChanged, "extension-create.destination-changed")]
    [InlineData(ExtensionCreateFindingCode.ApplicationFailed, "extension-create.application-failed")]
    [InlineData(ExtensionCreateFindingCode.VerificationFailed, "extension-create.verification-failed")]
    [InlineData(ExtensionCreateFindingCode.Interrupted, "extension-create.interrupted")]
    public void FindingCodesHaveExactMachineNames(
        object codeValue,
        string expected)
    {
        var code = Assert.IsType<ExtensionCreateFindingCode>(codeValue);
        Assert.Equal(expected, ExtensionCreateDefinitions.ReadFindingCode(code));
    }

    [Theory(DisplayName = "Extension Create maps every named effect kind to its stable machine name"), Trait("Feature", "extension-create"), Trait("Evidence", "Unit")]
    [InlineData(ExtensionCreateEffectKind.ManifestFile, "manifest")]
    [InlineData(ExtensionCreateEffectKind.PayloadAgentsDirectory, "payload-agents-directory")]
    public void EffectKindsHaveExactMachineNames(
        object kindValue,
        string expected)
    {
        var kind = Assert.IsType<ExtensionCreateEffectKind>(kindValue);
        Assert.Equal(expected, ExtensionCreateDefinitions.ReadEffectKind(kind));
    }

    [Theory(DisplayName = "Extension Create maps every named verification state to its stable machine name"), Trait("Feature", "extension-create"), Trait("Evidence", "Unit")]
    [InlineData(ExtensionCreateVerificationState.NotStarted, "not-started")]
    [InlineData(ExtensionCreateVerificationState.Planned, "planned")]
    [InlineData(ExtensionCreateVerificationState.Verified, "verified")]
    [InlineData(ExtensionCreateVerificationState.Failed, "failed")]
    [InlineData(ExtensionCreateVerificationState.Unavailable, "unavailable")]
    public void VerificationStatesHaveExactMachineNames(
        object stateValue,
        string expected)
    {
        var state = Assert.IsType<ExtensionCreateVerificationState>(stateValue);
        Assert.Equal(expected, ExtensionCreateDefinitions.ReadVerificationState(state));
    }

    [Fact(DisplayName = "Extension Create machine mappings cover every declared enum member"), Trait("Feature", "extension-create"), Trait("Evidence", "Unit")]
    public void MachineMappingsCoverEveryDeclaredMember()
    {
        Assert.Equal(Enum.GetValues<ExtensionCreateMode>(), [ExtensionCreateMode.Apply, ExtensionCreateMode.DryRun]);
        Assert.Equal(
            Enum.GetValues<ExtensionCreateFindingCode>(),
            [
                ExtensionCreateFindingCode.InvalidInput,
                ExtensionCreateFindingCode.CatalogueUnavailable,
                ExtensionCreateFindingCode.CatalogueUnsafe,
                ExtensionCreateFindingCode.DestinationCollision,
                ExtensionCreateFindingCode.DestinationChanged,
                ExtensionCreateFindingCode.ApplicationFailed,
                ExtensionCreateFindingCode.VerificationFailed,
                ExtensionCreateFindingCode.Interrupted,
            ]);
        Assert.Equal(
            Enum.GetValues<ExtensionCreateEffectKind>(),
            [ExtensionCreateEffectKind.ManifestFile, ExtensionCreateEffectKind.PayloadAgentsDirectory]);
        Assert.Equal(
            Enum.GetValues<ExtensionCreateVerificationState>(),
            [
                ExtensionCreateVerificationState.NotStarted,
                ExtensionCreateVerificationState.Planned,
                ExtensionCreateVerificationState.Verified,
                ExtensionCreateVerificationState.Failed,
                ExtensionCreateVerificationState.Unavailable,
            ]);
    }

    [Fact(DisplayName = "Extension Create machine mappings reject every undefined enum value"), Trait("Feature", "extension-create"), Trait("Evidence", "Unit")]
    public void MachineMappingsRejectUndefinedValues()
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () => ExtensionCreateDefinitions.ReadMachineName((ExtensionCreateMode)int.MaxValue));
        Assert.Throws<ArgumentOutOfRangeException>(
            () => ExtensionCreateDefinitions.ReadFindingCode((ExtensionCreateFindingCode)int.MaxValue));
        Assert.Throws<ArgumentOutOfRangeException>(
            () => ExtensionCreateDefinitions.ReadEffectKind((ExtensionCreateEffectKind)int.MaxValue));
        Assert.Throws<ArgumentOutOfRangeException>(
            () => ExtensionCreateDefinitions.ReadVerificationState((ExtensionCreateVerificationState)int.MaxValue));
    }

    [Fact(DisplayName = "Extension Create JSON projection retains the workspace-free ordered result graph and empty arrays"), Trait("Feature", "extension-create"), Trait("Evidence", "Unit")]
    public void JsonProjectionRetainsCompleteOrderedGraph()
    {
        var result = CreateResult(CliSemanticStatus.Complete);

        var document = ExtensionCreateJsonProjection.Create(result);

        Assert.Equal(1, document.SchemaVersion);
        Assert.Equal("extension create", document.Command);
        Assert.Equal("complete", document.Status);
        Assert.Null(document.Workspace);
        Assert.NotNull(document.Result);
        Assert.Null(document.Next);
        Assert.Equal("/catalogue", document.Result.Catalogue);
        Assert.Equal("/catalogue/development-toolkit", document.Result.Destination);
        Assert.Equal("development-toolkit", document.Result.Id);
        Assert.NotNull(document.Result.Manifest);
        Assert.Equal("Development Toolkit", document.Result.Manifest.Name);
        Assert.Equal("Open Forge Extension package development-toolkit.", document.Result.Manifest.Description);
        Assert.Equal("0.1.0", document.Result.Manifest.Version);
        Assert.Empty(document.Result.Manifest.Dependencies);
        Assert.Equal("apply", document.Result.Mode);
        Assert.Equal(["manifest", "payload-agents-directory"], document.Result.IntendedEffects.Select(effect => effect.Kind));
        Assert.Equal(["manifest", "payload-agents-directory"], document.Result.AppliedEffects.Select(effect => effect.Kind));
        Assert.Equal("verified", document.Result.Verification.Catalogue);
        Assert.Equal("verified", document.Result.Verification.Destination);
        Assert.Equal("verified", document.Result.Verification.Manifest);
        Assert.Equal("verified", document.Result.Verification.Payload);
        Assert.Null(document.Result.Verification.Cause);
        Assert.False(document.Result.WorkspaceLifecycleChanged);
    }

    [Fact(DisplayName = "Extension Create human renderer exposes compact and expanded plan, effect, verification, and unchanged-workspace facts"),
     Trait("Feature", "extension-create"), Trait("Evidence", "Unit")]
    public void HumanViewsExposeCompleteFacts()
    {
        var result = CreateResult(CliSemanticStatus.Complete);

        var compact = ExtensionCreateHumanRenderer.Render(
            new CliPresentationRequest<ExtensionCreateResult>(
                result,
                new CliPresentation(CliOutputFormat.Human, CliView.Compact, CliVerbosity.Normal)));
        var expanded = ExtensionCreateHumanRenderer.Render(
            new CliPresentationRequest<ExtensionCreateResult>(
                result,
                new CliPresentation(CliOutputFormat.Human, CliView.Expanded, CliVerbosity.Normal)));

        Assert.Contains("extension create", compact, StringComparison.Ordinal);
        Assert.Contains("development-toolkit", compact, StringComparison.Ordinal);
        Assert.Contains("catalogue=/catalogue", compact, StringComparison.Ordinal);
        Assert.Contains("/catalogue/development-toolkit", compact, StringComparison.Ordinal);
        Assert.Contains("Mode: apply", compact, StringComparison.Ordinal);
        Assert.Contains("verification=catalogue=verified", compact, StringComparison.Ordinal);
        Assert.Contains("workspace=unchanged", compact, StringComparison.Ordinal);
        Assert.Contains("complete", compact, StringComparison.Ordinal);
        Assert.Contains("Open Forge extension create", expanded, StringComparison.Ordinal);
        Assert.Contains("Catalogue: /catalogue", expanded, StringComparison.Ordinal);
        Assert.Contains("Destination: /catalogue/development-toolkit", expanded, StringComparison.Ordinal);
        Assert.Contains("ID: development-toolkit", expanded, StringComparison.Ordinal);
        Assert.Contains("Mode: apply", expanded, StringComparison.Ordinal);
        Assert.Contains("Manifest: Development Toolkit; Open Forge Extension package development-toolkit.; 0.1.0", expanded, StringComparison.Ordinal);
        Assert.Contains("Dependencies: none", expanded, StringComparison.Ordinal);
        Assert.Contains("extension.json", expanded, StringComparison.Ordinal);
        Assert.Contains("content/.agents", expanded, StringComparison.Ordinal);
        Assert.Contains("Verification: catalogue=verified, destination=verified, manifest=verified, payload=verified", expanded, StringComparison.Ordinal);
        Assert.Contains("Workspace lifecycle: unchanged", expanded, StringComparison.Ordinal);
        Assert.Contains("Status: complete", expanded, StringComparison.Ordinal);
    }

    [Fact(DisplayName = "Extension Create human errors expose the exact subject, direct cause, and bounded next action in both views"), Trait("Feature", "extension-create"), Trait("Evidence", "Unit")]
    public void HumanErrorsExposeSubjectCauseAndNextAction()
    {
        var result = CreateResult(
            CliSemanticStatus.Blocked,
            new ExtensionCreateFinding
            {
                Code = ExtensionCreateFindingCode.DestinationCollision,
                Status = CliSemanticStatus.Blocked,
                Subject = "/catalogue/collision",
                Cause = "The destination contains an existing occupant.",
            });

        var compact = ExtensionCreateHumanRenderer.Render(
            new CliPresentationRequest<ExtensionCreateResult>(
                result,
                new CliPresentation(CliOutputFormat.Human, CliView.Compact, CliVerbosity.Normal)));
        var expanded = ExtensionCreateHumanRenderer.Render(
            new CliPresentationRequest<ExtensionCreateResult>(
                result,
                new CliPresentation(CliOutputFormat.Human, CliView.Expanded, CliVerbosity.Normal)));

        foreach (var rendered in new[] { compact, expanded })
        {
            Assert.Contains("subject=/catalogue/collision", rendered, StringComparison.Ordinal);
            Assert.Contains("cause=The destination contains an existing occupant.", rendered, StringComparison.Ordinal);
            Assert.Contains("Next: open-forge extension create --dry-run", rendered, StringComparison.Ordinal);
        }
    }

    [Fact(DisplayName = "Extension Create diagnostics remain bounded and escape the retained finding subject and cause"), Trait("Feature", "extension-create"), Trait("Evidence", "Unit")]
    public void DiagnosticsAreBoundedAndSafe()
    {
        var result = CreateResult(
            CliSemanticStatus.Blocked,
            new ExtensionCreateFinding
            {
                Code = ExtensionCreateFindingCode.DestinationCollision,
                Status = CliSemanticStatus.Blocked,
                Subject = "destination\nwith-control",
                Cause = "existing\tbytes",
            });

        var diagnostic = ExtensionCreateDiagnosticRenderer.Render(
            new CliPresentationRequest<ExtensionCreateResult>(
                result,
                new CliPresentation(CliOutputFormat.Human, CliView.Expanded, CliVerbosity.Verbose)));

        Assert.NotNull(diagnostic);
        Assert.True(diagnostic.Length <= CliRenderingStage.MaximumDiagnosticLength);
        Assert.DoesNotContain('\n', diagnostic);
        Assert.DoesNotContain('\r', diagnostic);
        Assert.Contains("extension-create.destination-collision", diagnostic, StringComparison.Ordinal);
        Assert.Contains("existing", diagnostic, StringComparison.Ordinal);
    }

    [Theory(DisplayName = "Extension Create status policy selects the fixed stream and exit for every semantic status"), Trait("Feature", "extension-create"), Trait("Evidence", "Unit")]
    [InlineData(CliSemanticStatus.Complete, 0, CliOutputTarget.StandardOutput)]
    [InlineData(CliSemanticStatus.Failed, 1, CliOutputTarget.StandardError)]
    [InlineData(CliSemanticStatus.Attention, 2, CliOutputTarget.StandardOutput)]
    [InlineData(CliSemanticStatus.Incomplete, 3, CliOutputTarget.StandardOutput)]
    [InlineData(CliSemanticStatus.Invalid, 4, CliOutputTarget.StandardError)]
    [InlineData(CliSemanticStatus.Blocked, 5, CliOutputTarget.StandardError)]
    [InlineData(CliSemanticStatus.Interrupted, 130, CliOutputTarget.StandardError)]
    public async Task StatusPolicyIsExact(
        object statusValue,
        int expectedExitCode,
        object targetValue)
    {
        var status = Assert.IsType<CliSemanticStatus>(statusValue);
        var expectedTarget = Assert.IsType<CliOutputTarget>(targetValue);
        var result = CreateResult(status);
        var pipeline = new CliCommandPipeline<ExtensionCreateResult, ExtensionCreateResult>(
            (_, _) => ValueTask.FromResult(result),
            new CliRendererSet<ExtensionCreateResult>(
                presentation => $"human:{presentation.Result.Command}",
                presentation => $"json:{presentation.Result.Command}"));
        using var standardOutput = new StringWriter(CultureInfo.InvariantCulture);
        using var standardError = new StringWriter(CultureInfo.InvariantCulture);

        var completion = await pipeline.ExecuteAsync(
            result,
            new CliPresentation(CliOutputFormat.Human, CliView.Compact, CliVerbosity.Normal),
            new CliOutputWriters(standardOutput, standardError),
            TestContext.Current.CancellationToken);

        Assert.Equal(status, completion.Status);
        Assert.Equal(expectedExitCode, completion.ExitCode);
        Assert.Equal(expectedTarget, completion.PrimaryOutputTarget);
        var rendered = $"human:{result.Command}{Environment.NewLine}";
        Assert.Equal(expectedTarget == CliOutputTarget.StandardOutput ? rendered : string.Empty, standardOutput.ToString());
        Assert.Equal(expectedTarget == CliOutputTarget.StandardError ? rendered : string.Empty, standardError.ToString());
    }

    private static ExtensionCreateResult CreateResult(
        CliSemanticStatus status,
        ExtensionCreateFinding? finding = null)
        => new()
        {
            Status = status,
            Catalogue = "/catalogue",
            Destination = "/catalogue/development-toolkit",
            StableId = "development-toolkit",
            Manifest = new ExtensionCreateManifest
            {
                Id = "development-toolkit",
                Name = "Development Toolkit",
                Description = "Open Forge Extension package development-toolkit.",
                Version = "0.1.0",
                Dependencies = [],
            },
            Mode = ExtensionCreateMode.Apply,
            IntendedEffects =
            [
                new() { Kind = ExtensionCreateEffectKind.ManifestFile, Path = "/catalogue/development-toolkit/extension.json" },
                new() { Kind = ExtensionCreateEffectKind.PayloadAgentsDirectory, Path = "/catalogue/development-toolkit/content/.agents/" },
            ],
            AppliedEffects =
            [
                new() { Kind = ExtensionCreateEffectKind.ManifestFile, Path = "/catalogue/development-toolkit/extension.json" },
                new() { Kind = ExtensionCreateEffectKind.PayloadAgentsDirectory, Path = "/catalogue/development-toolkit/content/.agents/" },
            ],
            Verification = new ExtensionCreateVerification
            {
                Catalogue = ExtensionCreateVerificationState.Verified,
                Destination = ExtensionCreateVerificationState.Verified,
                Manifest = ExtensionCreateVerificationState.Verified,
                Payload = ExtensionCreateVerificationState.Verified,
                Cause = null,
            },
            Findings = finding is null ? [] : [finding],
            Next = NextForStatus(status),
        };

    private static CliNextAction? NextForStatus(CliSemanticStatus status)
        => status switch
        {
            CliSemanticStatus.Complete or CliSemanticStatus.Attention => null,
            CliSemanticStatus.Incomplete => ExtensionCreateDefinitions.IncompleteNext,
            CliSemanticStatus.Invalid => ExtensionCreateDefinitions.InvalidNext,
            CliSemanticStatus.Blocked => ExtensionCreateDefinitions.BlockedNext,
            CliSemanticStatus.Failed => ExtensionCreateDefinitions.FailedNext,
            CliSemanticStatus.Interrupted => ExtensionCreateDefinitions.InterruptedNext,
            _ => throw new ArgumentOutOfRangeException(nameof(status), status, "The test status is not defined."),
        };
}
