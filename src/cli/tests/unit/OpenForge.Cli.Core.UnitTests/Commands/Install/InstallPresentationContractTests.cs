using System.Reflection;
using OpenForge.Cli.Core.Commands.Install;
using OpenForge.Cli.Core.Commands.Install.Models.Binding;
using OpenForge.Cli.Core.Commands.Install.Models.Operation;
using OpenForge.Cli.Core.Commands.Install.Models.Presentation;
using OpenForge.Cli.Core.Commands.Install.Models.Request;
using OpenForge.Cli.Core.Commands.Install.Models.Result;
using OpenForge.Cli.Core.Commands.Install.Shared.Rendering;
using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Presentation;
using OpenForge.Cli.Core.Shell.Presentation.Models;

namespace OpenForge.Cli.Core.UnitTests.Commands.Install;

public sealed class InstallPresentationContractTests
{
    [Fact(DisplayName = "Install JSON DTOs preserve the exact envelope, result, nested member order, and nullable facts"), Trait("Feature", "install-presentation"), Trait("Evidence", "Unit")]
    public void JsonDtosPreserveExactShape()
    {
        AssertProperties<InstallJsonDocument>(
            "SchemaVersion", "Command", "Status", "Workspace", "Result", "Next");
        AssertProperties<InstallJsonWorkspace>("Path", "SelectedBy");
        AssertProperties<InstallJsonResult>(
            "Mode", "Force", "Automatic", "Source", "Classification", "Footprint", "Effects", "Lifecycle", "Recovery", "Verification", "Findings");
        AssertProperties<InstallJsonSource>("InventoryFingerprint", "AssetCount");
        AssertProperties<InstallJsonFootprint>("PayloadFiles", "ManagedRegions", "GeneratedRegions");
        AssertProperties<InstallJsonEffect>("Path", "Kind", "Action", "SourceAssetPath", "Outcome", "Residual");
        AssertProperties<InstallJsonLifecycle>("Action", "Outcome");
        AssertProperties<InstallJsonRecovery>("State", "ResidualPath");
        AssertProperties<InstallJsonFinding>("Code", "Target", "Cause");
        AssertProperties<InstallJsonNext>("Command", "Reason");

        AssertNullable<InstallJsonDocument>(nameof(InstallJsonDocument.Workspace));
        AssertNotNullable<InstallJsonDocument>(nameof(InstallJsonDocument.Result));
        AssertNullable<InstallJsonDocument>(nameof(InstallJsonDocument.Next));
        AssertNullable<InstallJsonResult>(nameof(InstallJsonResult.Source));
        AssertNullable<InstallJsonResult>(nameof(InstallJsonResult.Classification));
        AssertNullable<InstallJsonResult>(nameof(InstallJsonResult.Footprint));
        AssertNotNullable<InstallJsonResult>(nameof(InstallJsonResult.Effects));
        AssertNotNullable<InstallJsonResult>(nameof(InstallJsonResult.Lifecycle));
        AssertNotNullable<InstallJsonResult>(nameof(InstallJsonResult.Recovery));
        AssertNotNullable<InstallJsonResult>(nameof(InstallJsonResult.Verification));
        AssertNotNullable<InstallJsonResult>(nameof(InstallJsonResult.Findings));
        AssertNullable<InstallJsonEffect>(nameof(InstallJsonEffect.SourceAssetPath));
        AssertNullable<InstallJsonRecovery>(nameof(InstallJsonRecovery.ResidualPath));
        AssertNullable<InstallJsonFinding>(nameof(InstallJsonFinding.Target));

        var effects = Property<InstallJsonResult>(nameof(InstallJsonResult.Effects));
        var findings = Property<InstallJsonResult>(nameof(InstallJsonResult.Findings));
        Assert.Equal(typeof(InstallJsonEffect[]), effects.PropertyType);
        Assert.Equal(typeof(InstallJsonFinding[]), findings.PropertyType);
    }

    [Fact(DisplayName = "Install projection must map the complete typed result to the frozen DTO packet"), Trait("Feature", "install-presentation"), Trait("Evidence", "Unit")]
    public void JsonProjectionMapsTheFrozenPacket()
    {
        var document = InstallJsonProjection.Create(InvalidResult());

        Assert.Equal(1, document.SchemaVersion);
        Assert.Equal("install", document.Command);
        Assert.Equal("invalid", document.Status);
        Assert.NotNull(document.Result);
        Assert.NotNull(document.Result.Effects);
        Assert.NotNull(document.Result.Findings);
    }

    [Theory(DisplayName = "Install compact and expanded human output leads with operation identity and retains one bounded next action"),
        Trait("Feature", "install-presentation"), Trait("Evidence", "Unit")]
    [InlineData((int)CliView.Compact)]
    [InlineData((int)CliView.Expanded)]
    public void HumanViewsLeadWithInstallIdentityAndBoundedNext(int viewValue)
    {
        var view = (CliView)viewValue;
        var output = InstallHumanRenderer.Render(
            new CliPresentationRequest<InstallResult>(
                InvalidResult(),
                new CliPresentation(CliOutputFormat.Human, view, CliVerbosity.Normal)));

        Assert.StartsWith("Open Forge install", output, StringComparison.Ordinal);
        Assert.Contains("Workspace:", output, StringComparison.Ordinal);
        Assert.Contains("Flags:", output, StringComparison.Ordinal);
        Assert.Contains("Classification:", output, StringComparison.Ordinal);
        Assert.Contains("Footprint:", output, StringComparison.Ordinal);
        Assert.Contains("Effects:", output, StringComparison.Ordinal);
        Assert.Contains("Findings:", output, StringComparison.Ordinal);
        Assert.Contains("Lifecycle:", output, StringComparison.Ordinal);
        Assert.Contains("Recovery:", output, StringComparison.Ordinal);
        Assert.Contains("Verification:", output, StringComparison.Ordinal);
        Assert.Contains("Status:", output, StringComparison.Ordinal);
        Assert.True(
            output.Split("Next:", StringSplitOptions.None).Length <= 2,
            "Install human output must contain at most one Next action.");
    }

    [Fact(DisplayName = "Install human attention status uses the accepted requires-attention phrase"), Trait("Feature", "install-presentation"), Trait("Evidence", "Unit")]
    public void HumanAttentionUsesAcceptedStatusPhrase()
    {
        var result = InstallResult.Invalid(
            new InstallBindingInput(false, true, InstallMode.Apply),
            Workspace(),
            [new InstallFinding(
                InstallFindingCode.RecoveryArtifactRetained,
                "The representative recovery artifact remains available.")]);

        var output = InstallHumanRenderer.Render(
            new CliPresentationRequest<InstallResult>(
                result,
                new CliPresentation(
                    CliOutputFormat.Human,
                    CliView.Compact,
                    CliVerbosity.Normal)));

        Assert.Contains("Status: requires attention", output, StringComparison.Ordinal);
        Assert.DoesNotContain("Status: attention", output, StringComparison.Ordinal);
    }

    [Fact(DisplayName = "Install help and diagnostics retain exact public policy and bounded vocabulary"), Trait("Feature", "install-presentation"), Trait("Evidence", "Unit")]
    public void HelpAndDiagnosticsUseExactPublicPolicy()
    {
        var help = InstallHelpSections.Create();
        var diagnostics = InstallDiagnosticRenderer.Render(
            new CliPresentationRequest<InstallResult>(
                InvalidResult(),
                new CliPresentation(
                    CliOutputFormat.Human,
                    CliView.Expanded,
                    CliVerbosity.Verbose)));

        var syntax = Assert.Single(help.Sections, section => section.Heading == "Syntax");
        Assert.Contains(
            "open-forge install [--force] [--automatic] [--dry-run] [global flags]",
            syntax.Body,
            StringComparison.Ordinal);
        var writePolicy = Assert.Single(
            help.Sections,
            section => section.Heading == "Write policy");
        Assert.Contains("never updates or adopts", writePolicy.Body, StringComparison.Ordinal);
        var confirmation = Assert.Single(
            help.Sections,
            section => section.Heading == "Confirmation");
        Assert.Contains("non-prompt-capable human write requires --automatic", confirmation.Body, StringComparison.Ordinal);
        var globalOptions = Assert.Single(
            help.Sections,
            section => section.Heading == "Global options");
        Assert.Contains("--view <compact|expanded>", globalOptions.Body, StringComparison.Ordinal);
        Assert.Contains("status=invalid", diagnostics, StringComparison.Ordinal);
        Assert.Contains("mode=apply", diagnostics, StringComparison.Ordinal);
        Assert.Contains("classification=none", diagnostics, StringComparison.Ordinal);
        Assert.Contains("recovery=not-required", diagnostics, StringComparison.Ordinal);
        Assert.Contains("finding=install.invalid-input", diagnostics, StringComparison.Ordinal);
    }

    private static InstallResult InvalidResult()
        => InstallResult.Invalid(
            new InstallBindingInput(false, false, InstallMode.Apply),
            Workspace(),
            [new InstallFinding(
                InstallFindingCode.InvalidInput,
                "The representative Install input was invalid.")]);

    private static CliWorkspace Workspace()
    {
        var root = Path.GetFullPath(Path.Combine(Path.GetTempPath(), "install-presentation-contract"));
        return new CliWorkspace(root, root, CliWorkspaceSelectionMethod.ExplicitWorkspace);
    }

    private static PropertyInfo Property<T>(string name)
        => typeof(T).GetProperty(name, BindingFlags.Instance | BindingFlags.Public)
            ?? throw new InvalidOperationException($"Missing property {typeof(T).Name}.{name}.");

    private static void AssertProperties<T>(params string[] expected)
        => Assert.Equal(
            expected,
            typeof(T).GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .Select(property => property.Name));

    private static void AssertNullable<T>(string name)
        => Assert.Equal(
            NullabilityState.Nullable,
            new NullabilityInfoContext().Create(Property<T>(name)).ReadState);

    private static void AssertNotNullable<T>(string name)
        => Assert.Equal(
            NullabilityState.NotNull,
            new NullabilityInfoContext().Create(Property<T>(name)).ReadState);
}
