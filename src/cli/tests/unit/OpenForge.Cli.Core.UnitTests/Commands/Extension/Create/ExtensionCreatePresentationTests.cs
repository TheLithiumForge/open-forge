using OpenForge.Cli.Core.Commands.Extension.Create.Models.Request;
using System.Text.Json;
using OpenForge.Cli.Core.Commands.Extension.Create;
using OpenForge.Cli.Core.Commands.Extension.Create.Models.Manifest;
using OpenForge.Cli.Core.Commands.Extension.Create.Models.Planning;
using OpenForge.Cli.Core.Commands.Extension.Create.Models.Result;
using OpenForge.Cli.Core.Presentation.Extension.Create;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Operation;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Presentation;
using OpenForge.Cli.Core.Shell.Pipeline;
using OpenForge.Cli.Core.Shell.Presentation.Models;

namespace OpenForge.Cli.Core.UnitTests.Commands.Extension.Create;

public sealed class ExtensionCreatePresentationTests
{
    [Theory(DisplayName = "Extension Create maps every named operation mode to its stable machine name"), Trait("Feature", "extension-create"), Trait("Evidence", "Unit"), Trait("Boundary", "Processing")]
    [InlineData(ExtensionCreateMode.Apply, "apply")]
    [InlineData(ExtensionCreateMode.DryRun, "dry-run")]
    public void OperationModesHaveExactMachineNames(
        object modeValue,
        string expected)
    {
        var mode = Assert.IsType<ExtensionCreateMode>(modeValue);
        Assert.Equal(expected, ExtensionCreateDefinitions.ReadMachineName(mode));
    }

    [Theory(DisplayName = "Extension Create maps every named finding code to its stable machine name"), Trait("Feature", "extension-create"), Trait("Evidence", "Unit"), Trait("Boundary", "Processing")]
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

    [Theory(DisplayName = "Extension Create maps every named effect kind to its stable machine name"), Trait("Feature", "extension-create"), Trait("Evidence", "Unit"), Trait("Boundary", "Processing")]
    [InlineData(ExtensionCreateEffectKind.ManifestFile, "manifest")]
    [InlineData(ExtensionCreateEffectKind.PayloadAgentsDirectory, "payload-agents-directory")]
    public void EffectKindsHaveExactMachineNames(
        object kindValue,
        string expected)
    {
        var kind = Assert.IsType<ExtensionCreateEffectKind>(kindValue);
        Assert.Equal(expected, ExtensionCreateDefinitions.ReadEffectKind(kind));
    }

    [Theory(DisplayName = "Extension Create maps every named verification state to its stable machine name"), Trait("Feature", "extension-create"), Trait("Evidence", "Unit"), Trait("Boundary", "Processing")]
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

    [Fact(DisplayName = "Extension Create machine mappings cover every declared enum member"), Trait("Feature", "extension-create"), Trait("Evidence", "Unit"), Trait("Boundary", "Processing")]
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
                ExtensionCreateFindingCode.ConfirmationRequired,
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

    [Fact(DisplayName = "Extension Create machine mappings reject every undefined enum value"), Trait("Feature", "extension-create"), Trait("Evidence", "Unit"), Trait("Boundary", "Processing")]
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

    [Fact(DisplayName = "Extension Create native JSON data follows the detail contract"), Trait("Feature", "extension-create"), Trait("Evidence", "Unit"), Trait("Boundary", "Output")]
    public void NativeJsonDataFollowsDetailContract()
    {
        var result = CreateResult(CliSemanticStatus.Complete);

        var minimal = SelectJson(result, CliDetail.Minimal);
        var standard = SelectJson(result, CliDetail.Standard);
        var full = SelectJson(result, CliDetail.Full);

        Assert.Equal("apply", minimal.GetProperty("mode").GetString());
        Assert.Equal("development-toolkit", minimal.GetProperty("id").GetString());
        Assert.Equal("/catalogue", minimal.GetProperty("folder").GetString());
        Assert.Equal("/catalogue/development-toolkit", minimal.GetProperty("packagePath").GetString());
        Assert.Equal("/catalogue/development-toolkit/extension.json", minimal.GetProperty("manifestPath").GetString());
        Assert.Equal("/catalogue/development-toolkit/content/.agents/", minimal.GetProperty("contentPath").GetString());
        Assert.False(minimal.TryGetProperty("manifest", out _));
        Assert.False(minimal.TryGetProperty("manifestContent", out _));

        var manifest = standard.GetProperty("manifest");
        Assert.Equal("Development Toolkit", manifest.GetProperty("name").GetString());
        Assert.Equal("Open Forge Extension package development-toolkit.", manifest.GetProperty("description").GetString());
        Assert.Equal("0.1.0", manifest.GetProperty("version").GetString());
        Assert.Empty(manifest.GetProperty("dependencies").EnumerateArray());
        Assert.Contains("\"id\":\"development-toolkit\"", full.GetProperty("manifestContent").GetString(), StringComparison.Ordinal);
    }

    [Fact(DisplayName = "Extension Create native text renderer exposes the scaffold and edit instruction"),
     Trait("Feature", "extension-create"), Trait("Evidence", "Unit"), Trait("Boundary", "Output")]
    public void NativeTextRendererExposesScaffoldAndEditInstruction()
    {
        var result = CreateResult(CliSemanticStatus.Complete);

        var compact = SelectText(result, CliDetail.Minimal);
        var expanded = SelectText(result, CliDetail.Standard);

        Assert.StartsWith("Created the development-toolkit Extension scaffold at /catalogue/development-toolkit", compact, StringComparison.Ordinal);
        Assert.Contains("/catalogue/development-toolkit/extension.json", compact, StringComparison.Ordinal);
        Assert.Contains("/catalogue/development-toolkit/content/.agents/", compact, StringComparison.Ordinal);
        Assert.Contains("Edit extension.json, then add files under content/.agents/.", compact, StringComparison.Ordinal);
        Assert.Contains("name: Development Toolkit", expanded, StringComparison.Ordinal);
        Assert.Contains("description: Open Forge Extension package development-toolkit.", expanded, StringComparison.Ordinal);
        Assert.Contains("version: 0.1.0", expanded, StringComparison.Ordinal);
        Assert.Contains("dependencies: none", expanded, StringComparison.Ordinal);
        Assert.Contains("extension.json", expanded, StringComparison.Ordinal);
        Assert.Contains("content/.agents", expanded, StringComparison.Ordinal);
        Assert.Contains("Edit extension.json, then add files under content/.agents/.", expanded, StringComparison.Ordinal);
    }

    [Fact(DisplayName = "Extension Create native text errors expose the catalogue message and next action"), Trait("Feature", "extension-create"), Trait("Evidence", "Unit"), Trait("Boundary", "Output")]
    public void NativeTextErrorsExposeCatalogueMessageAndNextAction()
    {
        var result = CreateResult(
            CliSemanticStatus.Blocked,
            new ExtensionCreateFinding
            {
                Code = ExtensionCreateFindingCode.DestinationCollision,
                Status = CliSemanticStatus.Blocked,
                Subject = "/catalogue/development-toolkit",
                Cause = "The destination contains an existing occupant.",
            });

        var compact = SelectText(result, CliDetail.Minimal);
        var expanded = SelectText(result, CliDetail.Standard);

        foreach (var rendered in new[] { compact, expanded })
        {
            Assert.Contains("Cannot create development-toolkit at /catalogue/development-toolkit: /catalogue/development-toolkit already exists with different content.", rendered, StringComparison.Ordinal);
            Assert.Contains("Next: open-forge extension create --dry-run", rendered, StringComparison.Ordinal);
        }
    }

    [Fact(DisplayName = "Extension Create diagnostics remain bounded and escape the retained finding subject and cause"), Trait("Feature", "extension-create"), Trait("Evidence", "Unit"), Trait("Boundary", "Output")]
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

        var diagnostic = CliRenderingStage.Render(
            new CliPresentationRequest<ExtensionCreateResult>(
                result,
                new CliPresentation(CliFormat.Text, CliDetail.Debug, null)), ExtensionCreatePresentation.Rendering).DiagnosticContent;

        Assert.NotNull(diagnostic);
        Assert.True(diagnostic.Length <= CliPresentationDefinitions.MaximumDiagnosticLength);
        Assert.All(diagnostic.Split('\n'), line => Assert.InRange(line.Length, 1, 240));
        Assert.DoesNotContain('\r', diagnostic);
        Assert.Contains("extension-create.destination-collision", diagnostic, StringComparison.Ordinal);
        Assert.Contains("destination\\nwith-control", diagnostic, StringComparison.Ordinal);
        Assert.Contains("existing\\tbytes", diagnostic, StringComparison.Ordinal);
    }

    [Theory(DisplayName = "Both Extension Create views keep every intended path without claiming an unchanged scaffold was applied"), Trait("Feature", "extension-create"), Trait("Evidence", "Unit"), Trait("Boundary", "Output")]
    [InlineData((int)CliDetail.Minimal)]
    [InlineData((int)CliDetail.Standard)]
    public void VerifiedNoOpRetainsPaths(int view)
    {
        var result = CreateResult(CliSemanticStatus.Complete) with { AppliedEffects = [] };
        var rendered = SelectText(result, (CliDetail)view);
        Assert.StartsWith("The development-toolkit scaffold at /catalogue/development-toolkit already matches. Nothing to do.", rendered, StringComparison.Ordinal);
        Assert.Contains("/catalogue/development-toolkit/extension.json", rendered, StringComparison.Ordinal);
        Assert.Contains("/catalogue/development-toolkit/content/.agents/", rendered, StringComparison.Ordinal);
        Assert.DoesNotContain("created", rendered, StringComparison.Ordinal);
        Assert.Empty(result.AppliedEffects);
    }

    private static JsonElement SelectJson(ExtensionCreateResult result, CliDetail detail)
    {
        var request = new CliPresentationRequest<ExtensionCreateResult>(
            result,
            new CliPresentation(CliFormat.Json, detail, null));
        using var document = JsonDocument.Parse(
            CliRenderingStage.Render(request, ExtensionCreatePresentation.Rendering).PrimaryContent);
        return document.RootElement.GetProperty("data").Clone();
    }

    private static string SelectText(ExtensionCreateResult result, CliDetail detail)
        => CliRenderingStage.Render(
            new CliPresentationRequest<ExtensionCreateResult>(
                result,
                new CliPresentation(CliFormat.Text, detail, null)),
            ExtensionCreatePresentation.Rendering).PrimaryContent;

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
