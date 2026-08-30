using OpenForge.Cli.Core.Commands.Extension.Create;
using OpenForge.Cli.Core.Commands.Extension.Create.Models.Request;
using OpenForge.Cli.Core.Commands.Extension.Create.Models.Result;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Interaction;
using OpenForge.Cli.TestSupport;

namespace OpenForge.Cli.IntegrationTests.Commands.Extension.Create;

public sealed class ExtensionCreateInteractionIntegrationTests
{
    [Theory(DisplayName = "Extension Create direct and interactive flows resolve zero, one, and all missing required facts equivalently"), Trait("Feature", "extension-create"), Trait("Evidence", "Integration")]
    [InlineData("zero")]
    [InlineData("one-id")]
    [InlineData("one-catalogue")]
    [InlineData("all")]
    public async Task DirectAndInteractiveFlowsAreEquivalent(string scenario)
    {
        using var directCatalogue = TemporaryWorkspace.Create($"extension-create-direct-{scenario}");
        using var interactiveCatalogue = TemporaryWorkspace.Create($"extension-create-interactive-{scenario}");
        var directDestination = directCatalogue.Combine("development-toolkit");
        var interactiveDestination = interactiveCatalogue.Combine("development-toolkit");
        try
        {
            var direct = await ExecuteAsync(
                Request(
                    stableId: "development-toolkit",
                    cataloguePath: directCatalogue.Path,
                    mode: ExtensionCreateMode.DryRun) with
                {
                    Name = "Development Toolkit",
                    Description = "Open Forge Extension package development-toolkit.",
                    PackageVersion = "0.1.0",
                },
                cancellationToken: TestContext.Current.CancellationToken);

            var interactiveInput = scenario switch
            {
                "zero" => $"development-toolkit\n{interactiveCatalogue.Path}\n",
                "one-id" => $"development-toolkit\n",
                "one-catalogue" => $"{interactiveCatalogue.Path}\n",
                "all" => string.Empty,
                _ => throw new ArgumentOutOfRangeException(nameof(scenario), scenario, "The interaction scenario is not defined."),
            };
            var interactiveRequest = scenario switch
            {
                "zero" => Request(stableId: null, cataloguePath: null, mode: ExtensionCreateMode.DryRun, allowInteraction: true),
                "one-id" => Request(stableId: null, cataloguePath: interactiveCatalogue.Path, mode: ExtensionCreateMode.DryRun, allowInteraction: true),
                "one-catalogue" => Request(stableId: "development-toolkit", cataloguePath: null, mode: ExtensionCreateMode.DryRun, allowInteraction: true),
                "all" => Request(stableId: "development-toolkit", cataloguePath: interactiveCatalogue.Path, mode: ExtensionCreateMode.DryRun, allowInteraction: true),
                _ => throw new ArgumentOutOfRangeException(nameof(scenario), scenario, "The interaction scenario is not defined."),
            };
            using var prompts = new StringWriter();
            var interactive = await ExecuteAsync(
                interactiveRequest,
                interactiveInput,
                prompts,
                canPrompt: true,
                cancellationToken: TestContext.Current.CancellationToken);

            Assert.Equal(CliSemanticStatus.Complete, direct.Status);
            Assert.Equal(direct.Status, interactive.Status);
            Assert.Equal(direct.Manifest!.Id, interactive.Manifest!.Id);
            Assert.Equal(direct.Manifest.Name, interactive.Manifest.Name);
            Assert.Equal(direct.Manifest.Description, interactive.Manifest.Description);
            Assert.Equal(direct.Manifest.Version, interactive.Manifest.Version);
            Assert.Equal(direct.Manifest.Dependencies, interactive.Manifest.Dependencies);
            Assert.Equal(direct.IntendedEffects.Select(effect => effect.Kind), interactive.IntendedEffects.Select(effect => effect.Kind));
            Assert.Equal(ExpectedPromptCount(scenario), CountPromptLines(prompts.ToString()));
            Assert.Empty(directCatalogue.SnapshotHashes());
            Assert.Empty(interactiveCatalogue.SnapshotHashes());
        }
        finally
        {
            DeleteDestination(directDestination);
            DeleteDestination(interactiveDestination);
        }
    }

    [Fact(DisplayName = "Extension Create interaction-disabled requests never prompt or infer omitted required facts"), Trait("Feature", "extension-create"), Trait("Evidence", "Integration")]
    public async Task InteractionDisabledOmissionIsInvalidAndSilent()
    {
        using var catalogue = TemporaryWorkspace.Create("extension-create-nonprompt");
        using var prompts = new StringWriter();
        var before = catalogue.SnapshotHashes();
        var result = await ExecuteAsync(
            Request(stableId: null, cataloguePath: null, mode: ExtensionCreateMode.Apply),
            string.Empty,
            prompts,
            canPrompt: false,
            cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(CliSemanticStatus.Invalid, result.Status);
        Assert.Empty(prompts.ToString());
        Assert.Equal(before, catalogue.SnapshotHashes());
    }

    [Fact(DisplayName = "Extension Create cancellation returns interrupted and leaves both catalogue and workspace unchanged"), Trait("Feature", "extension-create"), Trait("Evidence", "Integration")]
    public async Task CancellationRetainsNoWritesAndNoWorkspaceLifecycle()
    {
        using var catalogue = TemporaryWorkspace.Create("extension-create-cancel-integration");
        using var workspace = TemporaryWorkspace.Create("extension-create-cancel-workspace");
        var beforeCatalogue = catalogue.SnapshotHashes();
        var beforeWorkspace = workspace.SnapshotHashes();
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();
        var result = await ExecuteAsync(
            Request(stableId: "development-toolkit", cataloguePath: catalogue.Path, mode: ExtensionCreateMode.Apply),
            cancellationToken: cancellation.Token);

        Assert.Equal(CliSemanticStatus.Interrupted, result.Status);
        Assert.Equal(beforeCatalogue, catalogue.SnapshotHashes());
        Assert.Equal(beforeWorkspace, workspace.SnapshotHashes());
        Assert.False(Directory.Exists(workspace.Combine(".agents")));
    }

    [Fact(DisplayName = "Extension Create keeps the workspace path irrelevant and never creates lock or lifecycle state"), Trait("Feature", "extension-create"), Trait("Evidence", "Integration")]
    public async Task WorkspaceIsACompleteNoOp()
    {
        using var catalogue = TemporaryWorkspace.Create("extension-create-workspace-no-op-catalogue");
        using var workspace = TemporaryWorkspace.Create("extension-create-workspace-no-op");
        workspace.CreateFile("workspace-note.txt", "preserve");
        var beforeWorkspace = workspace.SnapshotHashes();
        var result = await ExecuteAsync(
            Request(stableId: "development-toolkit", cataloguePath: catalogue.Path, mode: ExtensionCreateMode.DryRun),
            cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(CliSemanticStatus.Complete, result.Status);
        Assert.Null(result.Workspace);
        Assert.Equal(beforeWorkspace, workspace.SnapshotHashes());
        Assert.False(Directory.Exists(workspace.Combine(".agents")));
        Assert.False(File.Exists(workspace.Combine(".agents", "open-forge.lifecycle.json")));
    }

    [Fact(DisplayName = "Extension Create revalidates a fresh plan and refuses a destination occupant introduced after preview"), Trait("Feature", "extension-create"), Trait("Evidence", "Integration")]
    public async Task FreshPlanRefusesPreviewCollision()
    {
        using var catalogue = TemporaryWorkspace.Create("extension-create-revalidation");
        var destination = catalogue.Combine("development-toolkit");
        var preview = await ExecuteAsync(
            Request(stableId: "development-toolkit", cataloguePath: catalogue.Path, mode: ExtensionCreateMode.DryRun),
            cancellationToken: TestContext.Current.CancellationToken);
        Assert.Equal(CliSemanticStatus.Complete, preview.Status);

        catalogue.CreateFile("development-toolkit/extension.json", "occupant");
        var beforeApply = catalogue.SnapshotHashes();
        var apply = await ExecuteAsync(
            Request(stableId: "development-toolkit", cataloguePath: catalogue.Path, mode: ExtensionCreateMode.Apply),
            cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(CliSemanticStatus.Blocked, apply.Status);
        Assert.Equal(beforeApply, catalogue.SnapshotHashes());
        Assert.Equal("occupant", File.ReadAllText(Path.Combine(destination, "extension.json")));
    }

    [Theory(DisplayName = "Extension Create wizard explains and corrects a missing or non-directory catalogue answer"), Trait("Feature", "extension-create"), Trait("Evidence", "Integration")]
    [InlineData("missing")]
    [InlineData("file")]
    public async Task WizardCorrectsIneligibleCatalogueAnswer(string scenario)
    {
        using var catalogue = TemporaryWorkspace.Create($"extension-create-catalogue-correction-{scenario}");
        var ineligible = catalogue.Combine("ineligible");
        if (scenario == "file")
        {
            catalogue.CreateFile("ineligible", "preserve");
        }

        var before = catalogue.SnapshotHashes();
        using var prompts = new StringWriter();
        var result = await ExecuteAsync(
            Request(stableId: "development-toolkit", cataloguePath: null, mode: ExtensionCreateMode.DryRun, allowInteraction: true),
            $"{ineligible}{Environment.NewLine}{catalogue.Path}{Environment.NewLine}",
            prompts,
            canPrompt: true,
            cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(CliSemanticStatus.Complete, result.Status);
        Assert.Equal(2, CountPromptLines(prompts.ToString()));
        Assert.Contains("not an existing ordinary directory", prompts.ToString(), StringComparison.Ordinal);
        Assert.Equal(before, catalogue.SnapshotHashes());
    }

    [Fact(DisplayName = "Extension Create wizard returns invalid when input ends after an ineligible catalogue answer"), Trait("Feature", "extension-create"), Trait("Evidence", "Integration")]
    public async Task WizardEndOfInputAfterIneligibleCatalogueIsInvalid()
    {
        using var catalogue = TemporaryWorkspace.Create("extension-create-catalogue-correction-eof");
        var missing = catalogue.Combine("missing");
        var before = catalogue.SnapshotHashes();
        using var prompts = new StringWriter();
        var result = await ExecuteAsync(
            Request(stableId: "development-toolkit", cataloguePath: null, mode: ExtensionCreateMode.Apply, allowInteraction: true),
            $"{missing}{Environment.NewLine}",
            prompts,
            canPrompt: true,
            cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(CliSemanticStatus.Invalid, result.Status);
        Assert.Contains(result.Findings, finding => finding.Code == ExtensionCreateFindingCode.InvalidInput);
        Assert.Equal(2, CountPromptLines(prompts.ToString()));
        Assert.Equal(before, catalogue.SnapshotHashes());
    }

    private static int ExpectedPromptCount(string scenario)
        => scenario switch
        {
            "zero" => 2,
            "one-id" or "one-catalogue" => 1,
            "all" => 0,
            _ => throw new ArgumentOutOfRangeException(nameof(scenario), scenario, "The interaction scenario is not defined."),
        };

    private static async Task<ExtensionCreateResult> ExecuteAsync(
        ExtensionCreateRequest request,
        string input = "",
        StringWriter? prompts = null,
        bool canPrompt = false,
        CancellationToken cancellationToken = default)
    {
        using var reader = new StringReader(input);
        using var ownedPrompts = prompts is null ? new StringWriter() : null;
        var session = new CliInteractiveSession(reader, prompts ?? ownedPrompts!, canPrompt);
        return await ExtensionCreateOperationFactory.Create(session).ExecuteAsync(request, cancellationToken);
    }

    private static ExtensionCreateRequest Request(
        string? stableId,
        string? cataloguePath,
        ExtensionCreateMode mode,
        bool allowInteraction = false)
        => new()
        {
            StableId = stableId,
            CataloguePath = cataloguePath,
            Name = null,
            Description = null,
            PackageVersion = null,
            Dependencies = [],
            AllowInteraction = allowInteraction,
            Mode = mode,
        };

    private static int CountPromptLines(string prompts)
        => prompts.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries).Length;

    private static void DeleteDestination(string destination)
    {
        if (Directory.Exists(destination))
        {
            Directory.Delete(destination, recursive: true);
        }
        else if (File.Exists(destination))
        {
            File.Delete(destination);
        }
    }
}
