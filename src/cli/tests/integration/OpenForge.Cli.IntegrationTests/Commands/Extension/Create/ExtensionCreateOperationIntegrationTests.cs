using System.Globalization;
using OpenForge.Cli.Core.Commands.Extension.Create;
using OpenForge.Cli.Core.Commands.Extension.Create.Models.Request;
using OpenForge.Cli.Core.Commands.Extension.Create.Models.Result;
using OpenForge.Cli.Core.Presentation.Shared.Prompts;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Interaction;
using OpenForge.Cli.Core.Shell.Interaction.Models;
using OpenForge.Cli.IntegrationTests.Commands.Extension.Shared.Interaction;
using OpenForge.Cli.TestSupport;
using OpenForge.Cli.TestSupport.Interaction;

namespace OpenForge.Cli.IntegrationTests.Commands.Extension.Create;

public sealed class ExtensionCreateOperationIntegrationTests
{
    [Trait("Boundary", "OS")]
    [Fact(DisplayName = "Extension Create dry-run forms deterministic defaults in an exact workspace-free scaffold plan"), Trait("Feature", "extension-create"), Trait("Evidence", "Integration")]
    public async Task DryRunFormsDefaultManifestAndScaffoldPlan()
    {
        using var catalogue = TemporaryWorkspace.Create("extension-create-defaults");
        var before = catalogue.SnapshotHashes();
        var result = await ExecuteAsync(
            Request(
                stableId: "development-toolkit",
                cataloguePath: catalogue.Path,
                mode: ExtensionCreateMode.DryRun),
            cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(CliSemanticStatus.Complete, result.Status);
        Assert.Null(result.Workspace);
        Assert.Equal("development-toolkit", result.StableId);
        Assert.Equal("Development Toolkit", result.Manifest!.Name);
        Assert.Equal("Open Forge Extension package development-toolkit.", result.Manifest.Description);
        Assert.Equal("0.1.0", result.Manifest.Version);
        Assert.Empty(result.Manifest.Dependencies);
        Assert.Equal(ExtensionCreateMode.DryRun, result.Mode);
        Assert.Equal(2, result.IntendedEffects.Count);
        Assert.Equal(before, catalogue.SnapshotHashes());
    }

    [Trait("Boundary", "OS")]
    [Fact(DisplayName = "Extension Create preserves nonblank metadata overrides, ordinally orders dependencies, and performs no source lookup"), Trait("Feature", "extension-create"), Trait("Evidence", "Integration")]
    public async Task OverridesPreserveTextAndOrderDependencies()
    {
        using var catalogue = TemporaryWorkspace.Create("extension-create-overrides");
        var result = await ExecuteAsync(
            Request(
                stableId: "new-package",
                cataloguePath: catalogue.Path,
                mode: ExtensionCreateMode.DryRun,
                dependencies: ["zeta", "alpha", "middle"]) with
            {
                Name = "  Display exactly  ",
                Description = "Description with\tspacing",
                PackageVersion = "preview-build",
            },
            cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(CliSemanticStatus.Complete, result.Status);
        Assert.Equal("  Display exactly  ", result.Manifest!.Name);
        Assert.Equal("Description with\tspacing", result.Manifest.Description);
        Assert.Equal("preview-build", result.Manifest.Version);
        Assert.Equal(["alpha", "middle", "zeta"], result.Manifest.Dependencies);
        Assert.Equal(ExtensionCreateMode.DryRun, result.Mode);
        Assert.Empty(catalogue.SnapshotHashes());
    }

    [Trait("Boundary", "OS")]
    [Theory(DisplayName = "Extension Create rejects blank optional metadata without prompting or writing"), Trait("Feature", "extension-create"), Trait("Evidence", "Integration")]
    [InlineData("name")]
    [InlineData("description")]
    [InlineData("package-version")]
    public async Task BlankOptionalMetadataIsInvalid(string field)
    {
        using var catalogue = TemporaryWorkspace.Create("extension-create-blank-metadata");
        using var prompts = new StringWriter(CultureInfo.InvariantCulture);
        var result = await ExecuteAsync(
            Request(
                stableId: "development-toolkit",
                cataloguePath: catalogue.Path,
                mode: ExtensionCreateMode.Apply,
                allowInteraction: true) with
            {
                Name = field == "name" ? "  " : null,
                Description = field == "description" ? "\t" : null,
                PackageVersion = field == "package-version" ? "\r\n" : null,
            },
            prompts: prompts,
            canPrompt: true,
            cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(CliSemanticStatus.Invalid, result.Status);
        Assert.Contains(result.Findings, finding => finding.Code == ExtensionCreateFindingCode.InvalidInput);
        Assert.Empty(prompts.ToString());
        Assert.Empty(catalogue.SnapshotHashes());
    }

    [Trait("Boundary", "OS")]
    [Theory(DisplayName = "Extension Create prompts only for currently missing required facts"), Trait("Feature", "extension-create"), Trait("Evidence", "Integration")]
    [InlineData("none", null, null, "development-toolkit\n{catalogue}\n", 2)]
    [InlineData("stable-id", null, "{catalogue}", "development-toolkit\n", 1)]
    [InlineData("catalogue", "development-toolkit", null, "{catalogue}\n", 1)]
    [InlineData("all", "development-toolkit", "{catalogue}", "", 0)]
    public async Task PromptsOnlyForMissingFacts(
        string missingFacts,
        string? stableId,
        string? cataloguePath,
        string input,
        int expectedPromptCount)
    {
        using var catalogue = TemporaryWorkspace.Create("extension-create-prompt");
        using var prompts = new StringWriter(CultureInfo.InvariantCulture);
        input = input.Replace("{catalogue}", catalogue.Path, StringComparison.Ordinal);
        var result = await ExecuteAsync(
            Request(
                stableId: stableId,
                cataloguePath: cataloguePath?.Replace("{catalogue}", catalogue.Path, StringComparison.Ordinal),
                mode: ExtensionCreateMode.DryRun,
                allowInteraction: true),
            input,
            prompts,
            canPrompt: true,
            cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(CliSemanticStatus.Complete, result.Status);
        Assert.Equal(expectedPromptCount, CountPromptLines(prompts.ToString()));
        Assert.DoesNotContain("name", prompts.ToString(), StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("description", prompts.ToString(), StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("version", prompts.ToString(), StringComparison.OrdinalIgnoreCase);
        _ = missingFacts;
    }

    [Trait("Boundary", "OS")]
    [Theory(DisplayName = "Extension Create derives names from stable IDs and rejects invalid dependency sets without source lookup"), Trait("Feature", "extension-create"), Trait("Evidence", "Integration")]
    [InlineData("alpha-beta-gamma", "Alpha Beta Gamma", "valid-name")]
    [InlineData("a1-2beta", "A1 2beta", "valid-name")]
    [InlineData("development-toolkit", "Development Toolkit", "duplicate")]
    [InlineData("development-toolkit", "Development Toolkit", "self")]
    [InlineData("development-toolkit", "Development Toolkit", "invalid")]
    public async Task NameAndDependencyValidationIsLocal(string stableId, string expectedName, string dependencyScenario)
    {
        using var catalogue = TemporaryWorkspace.Create("extension-create-validation");
        var dependencies = dependencyScenario switch
        {
            "valid-name" => Array.Empty<string>(),
            "duplicate" => ["alpha", "alpha"],
            "self" => [stableId],
            "invalid" => ["not a stable id"],
            _ => throw new ArgumentOutOfRangeException(nameof(dependencyScenario), dependencyScenario, "The dependency scenario is not defined."),
        };

        var result = await ExecuteAsync(
            Request(
                stableId: stableId,
                cataloguePath: catalogue.Path,
                mode: ExtensionCreateMode.DryRun,
                dependencies: dependencies),
            cancellationToken: TestContext.Current.CancellationToken);

        if (dependencyScenario == "valid-name")
        {
            Assert.Equal(CliSemanticStatus.Complete, result.Status);
            Assert.Equal(expectedName, result.Manifest!.Name);
            Assert.Empty(result.Manifest.Dependencies);
            Assert.Empty(catalogue.SnapshotHashes());
            return;
        }

        Assert.Equal(CliSemanticStatus.Invalid, result.Status);
        Assert.Contains(result.Findings, finding => finding.Code == ExtensionCreateFindingCode.InvalidInput);
        Assert.Empty(catalogue.SnapshotHashes());
    }

    [Trait("Boundary", "OS")]
    [Fact(DisplayName = "Extension Create prompts correct blank and invalid required answers without an attempt limit"), Trait("Feature", "extension-create"), Trait("Evidence", "Integration")]
    public async Task PromptsCorrectInvalidAnswersLocally()
    {
        using var catalogue = TemporaryWorkspace.Create("extension-create-correction");
        using var prompts = new StringWriter(CultureInfo.InvariantCulture);
        var result = await ExecuteAsync(
            Request(
                stableId: null,
                cataloguePath: null,
                mode: ExtensionCreateMode.DryRun,
                allowInteraction: true),
            "\nBAD ID\ndevelopment-toolkit\n\n{catalogue}\n".Replace("{catalogue}", catalogue.Path, StringComparison.Ordinal),
            prompts,
            canPrompt: true,
            cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(CliSemanticStatus.Complete, result.Status);
        Assert.Equal("development-toolkit", result.Manifest!.Id);
        Assert.True(CountPromptLines(prompts.ToString()) >= 4);
        Assert.Contains("'BAD ID' is not a valid ID. Use lowercase letters, digits and hyphens.", prompts.ToString(), StringComparison.Ordinal);
        Assert.Contains("ordinary directory", prompts.ToString(), StringComparison.Ordinal);
    }

    [Trait("Boundary", "OS")]
    [Fact(DisplayName = "Extension Create end of input is invalid and leaves the catalogue unchanged"), Trait("Feature", "extension-create"), Trait("Evidence", "Integration")]
    public async Task EndOfInputIsInvalidWithoutWrites()
    {
        using var catalogue = TemporaryWorkspace.Create("extension-create-eof");
        using var prompts = new StringWriter(CultureInfo.InvariantCulture);
        var before = catalogue.SnapshotHashes();
        var result = await ExecuteAsync(
            Request(
                stableId: null,
                cataloguePath: null,
                mode: ExtensionCreateMode.Apply,
                allowInteraction: true),
            string.Empty,
            prompts,
            canPrompt: true,
            cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(CliSemanticStatus.Interrupted, result.Status);
        Assert.Contains(result.Findings, finding => finding.Code == ExtensionCreateFindingCode.Interrupted);
        Assert.Equal(before, catalogue.SnapshotHashes());
    }

    [Trait("Boundary", "OS")]
    [Fact(DisplayName = "Extension Create retains a prompted stable ID when catalogue input ends"), Trait("Feature", "extension-create"), Trait("Evidence", "Integration")]
    public async Task CatalogueEndOfInputRetainsPromptedStableId()
    {
        using var catalogue = TemporaryWorkspace.Create("extension-create-partial-eof");
        using var prompts = new StringWriter(CultureInfo.InvariantCulture);
        var before = catalogue.SnapshotHashes();
        var result = await ExecuteAsync(
            Request(
                stableId: null,
                cataloguePath: null,
                mode: ExtensionCreateMode.Apply,
                allowInteraction: true),
            $"development-toolkit{Environment.NewLine}",
            prompts,
            canPrompt: true,
            cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(CliSemanticStatus.Interrupted, result.Status);
        Assert.Equal("development-toolkit", result.StableId);
        Assert.Contains(result.Findings, finding => finding.Code == ExtensionCreateFindingCode.Interrupted);
        Assert.Equal(4, CountPromptLines(prompts.ToString()));
        Assert.Equal(before, catalogue.SnapshotHashes());
    }

    [Trait("Boundary", "OS")]
    [Fact(DisplayName = "Extension Create retains a prompted stable ID when catalogue input is cancelled"), Trait("Feature", "extension-create"), Trait("Evidence", "Integration")]
    public async Task CatalogueCancellationRetainsPromptedStableId()
    {
        using var catalogue = TemporaryWorkspace.Create("extension-create-partial-cancellation");
        using var cancellation = new CancellationTokenSource();
        var lineReads = 0;
        var terminal = new CliTerminal(
            new CliTerminalCapabilities(true, false, false),
            (_, token) => ValueTask.CompletedTask,
            token =>
            {
                if (++lineReads == 1)
                {
                    cancellation.Cancel();
                }

                return ValueTask.FromResult<string?>("development-toolkit");
            },
            _ => ValueTask.FromResult<CliKeyStroke?>(null));
        var prompts = new CliPrompts(terminal);
        var before = catalogue.SnapshotHashes();

        var result = await ExtensionCreateOperationFactory
            .Create(ExtensionInteractionTestFactory.ForCreate(prompts))
            .ExecuteAsync(
                Request(
                    stableId: null,
                    cataloguePath: null,
                    mode: ExtensionCreateMode.Apply,
                    allowInteraction: true),
                cancellation.Token);

        Assert.Equal(CliSemanticStatus.Interrupted, result.Status);
        Assert.Equal("development-toolkit", result.StableId);
        Assert.Contains(result.Findings, finding => finding.Code == ExtensionCreateFindingCode.Interrupted);
        Assert.Equal(before, catalogue.SnapshotHashes());
    }

    [Trait("Boundary", "OS")]
    [Fact(DisplayName = "Extension Create cancellation is interrupted before effects and leaves the catalogue unchanged"), Trait("Feature", "extension-create"), Trait("Evidence", "Integration")]
    public async Task CancellationIsInterruptedWithoutWrites()
    {
        using var catalogue = TemporaryWorkspace.Create("extension-create-cancel");
        var before = catalogue.SnapshotHashes();
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();
        var result = await ExecuteAsync(
            Request(
                stableId: "development-toolkit",
                cataloguePath: catalogue.Path,
                mode: ExtensionCreateMode.Apply),
            cancellationToken: cancellation.Token);

        Assert.Equal(CliSemanticStatus.Interrupted, result.Status);
        Assert.Contains(result.Findings, finding => finding.Code == ExtensionCreateFindingCode.Interrupted);
        Assert.Equal(ExtensionCreateVerificationState.NotStarted, result.Verification.Destination);
        Assert.Equal(ExtensionCreateVerificationState.NotStarted, result.Verification.Manifest);
        Assert.Equal(ExtensionCreateVerificationState.NotStarted, result.Verification.Payload);
        Assert.Equal(before, catalogue.SnapshotHashes());
    }

    [Trait("Boundary", "OS")]
    [Fact(DisplayName = "Extension Create disabled interaction never prompts and reports missing required input without writes"), Trait("Feature", "extension-create"), Trait("Evidence", "Integration")]
    public async Task DisabledInteractionNeverPrompts()
    {
        using var catalogue = TemporaryWorkspace.Create("extension-create-no-prompt");
        using var prompts = new StringWriter(CultureInfo.InvariantCulture);
        var before = catalogue.SnapshotHashes();
        var result = await ExecuteAsync(
            Request(
                stableId: null,
                cataloguePath: null,
                mode: ExtensionCreateMode.Apply),
            "development-toolkit\n/catalogue\n",
            prompts,
            canPrompt: false,
            cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(CliSemanticStatus.Invalid, result.Status);
        Assert.Empty(prompts.ToString());
        Assert.Equal(before, catalogue.SnapshotHashes());
    }

    [Trait("Boundary", "OS")]
    [Fact(DisplayName = "Extension Create dry-run resolves the same plan while making no catalogue change"), Trait("Feature", "extension-create"), Trait("Evidence", "Integration")]
    public async Task DryRunHasNoEffects()
    {
        using var catalogue = TemporaryWorkspace.Create("extension-create-dry-run");
        var before = catalogue.SnapshotHashes();
        var result = await ExecuteAsync(
            Request(
                stableId: "development-toolkit",
                cataloguePath: catalogue.Path,
                mode: ExtensionCreateMode.DryRun),
            cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(CliSemanticStatus.Complete, result.Status);
        Assert.Equal(ExtensionCreateMode.DryRun, result.Mode);
        Assert.Equal(2, result.IntendedEffects.Count);
        Assert.Empty(result.AppliedEffects);
        Assert.Equal(before, catalogue.SnapshotHashes());
    }

    private static async Task<ExtensionCreateResult> ExecuteAsync(
        ExtensionCreateRequest request,
        string input = "",
        StringWriter? prompts = null,
        bool canPrompt = false,
        CancellationToken cancellationToken = default)
    {
        var lines = string.IsNullOrEmpty(input)
            ? Array.Empty<string?>()
            : input.Split(["\r\n", "\n", "\r"], StringSplitOptions.None);
        var scripted = ScriptedCliTerminal.Lines(lines, canPrompt);
        var terminalPrompts = new CliPrompts(scripted.Terminal);
        var result = await ExtensionCreateOperationFactory
            .Create(ExtensionInteractionTestFactory.ForCreate(terminalPrompts))
            .ExecuteAsync(request, cancellationToken);
        prompts?.Write(scripted.Output.ToString());
        return result;
    }

    private static int CountPromptLines(string prompts)
        => prompts.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries).Length;

    private static ExtensionCreateRequest Request(
        string? stableId,
        string? cataloguePath,
        ExtensionCreateMode mode,
        bool allowInteraction = false,
        IReadOnlyList<string>? dependencies = null)
        => new()
        {
            StableId = stableId,
            CataloguePath = cataloguePath,
            Name = null,
            Description = null,
            PackageVersion = null,
            Dependencies = dependencies ?? [],
            Automatic = mode == ExtensionCreateMode.Apply && !allowInteraction,
            AllowInteraction = allowInteraction,
            Mode = mode,
        };

}
