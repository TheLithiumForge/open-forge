using OpenForge.Cli.Core.Commands.Extension.Create.Models.Request;
using System.Globalization;
using OpenForge.Cli.Core.Commands.Extension.Create;
using OpenForge.Cli.Core.Commands.Extension.Create.Models.Manifest;
using OpenForge.Cli.Core.Commands.Extension.Create.Models.Planning;
using OpenForge.Cli.Core.Commands.Extension.Create.Models.Result;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Operation;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Output;
using OpenForge.Cli.Core.Shell.Pipeline;
using OpenForge.Cli.Core.Shell.Presentation.Models;

namespace OpenForge.Cli.Core.UnitTests.Commands.Extension.Create;

public sealed class ExtensionCreatePresentationTests
{
    [Theory(DisplayName = "Extension Create status policy selects the fixed stream and exit for every semantic status"), Trait("Feature", "extension-create"), Trait("Evidence", "Integration"), Trait("Boundary", "Host")]
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
        var pipeline = new CliReportPipeline<ExtensionCreateResult, ExtensionCreateResult, OpenForge.Cli.Core.UnitTests.Commands.Shared.Presentation.Models.CommandBindingTestData>(
            (_, _) => ValueTask.FromResult(result),
            OpenForge.Cli.Core.UnitTests.Commands.Shared.Presentation.CommandBindingTestRendering.Create<ExtensionCreateResult>($"human:{result.Command}"));
        using var standardOutput = new StringWriter(CultureInfo.InvariantCulture);
        using var standardError = new StringWriter(CultureInfo.InvariantCulture);

        var completion = await pipeline.ExecuteAsync(
            result,
            new CliPresentation(CliFormat.Text, CliDetail.Standard, null),
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
