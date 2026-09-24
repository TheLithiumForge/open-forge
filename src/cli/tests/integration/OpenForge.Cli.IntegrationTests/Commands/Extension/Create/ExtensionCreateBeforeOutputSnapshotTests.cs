using OpenForge.Cli.Core.Commands.Extension.Create;
using OpenForge.Cli.Core.Commands.Extension.Create.Models.Request;
using OpenForge.Cli.Core.Commands.Extension.Create.Models.Result;
using OpenForge.Cli.Core.Presentation.Extension.Create;
using OpenForge.Cli.Core.Presentation.Shared.Prompts;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.IntegrationTests.Commands.Extension.Shared.Interaction;
using OpenForge.Cli.IntegrationTests.Commands.Shared.Snapshots;
using OpenForge.Cli.TestSupport;
using OpenForge.Cli.TestSupport.Filesystem;
using OpenForge.Cli.TestSupport.Interaction;

namespace OpenForge.Cli.IntegrationTests.Commands.Extension.Create;

[Trait("Feature", "command-output-snapshots"), Trait("Evidence", "Integration")]
public sealed class ExtensionCreateBeforeOutputSnapshotTests
{
    private static readonly CommandOutputRenderers<ExtensionCreateResult> Renderers = CommandOutputRenderers<ExtensionCreateResult>.From(ExtensionCreatePresentation.Rendering);

    [Trait("Boundary", "Output")]
    [Fact(DisplayName = "Extension create output preserves an inaccessible catalogue without effects")]
    public async Task CatalogueUnreadable()
    {
        if (!OperatingSystem.IsWindows())
        {
            Assert.Skip("This catalogue enumeration boundary uses an owned Windows directory ACL.");
            return;
        }
        using var catalogue = TemporaryWorkspace.Create("extension-create-output-unreadable");
        var scripted = ScriptedCliTerminal.Lines([], canPrompt: false);
        var prompts = new CliPrompts(scripted.Terminal);
        var operation = ExtensionCreateOperationFactory.Create(
            ExtensionInteractionTestFactory.ForCreate(prompts));
        var request = new ExtensionCreateRequest
        {
            StableId = "toolkit",
            CataloguePath = catalogue.Path,
            Mode = ExtensionCreateMode.Apply,
            Automatic = true,
            AllowInteraction = false,
            Name = null,
            Description = null,
            PackageVersion = null,
            Dependencies = [],
        };
        var destination = catalogue.Combine("toolkit");
        try
        {
            Assert.Equal(CliSemanticStatus.Complete, (await operation.ExecuteAsync(request, TestContext.Current.CancellationToken)).Status);
            var before = catalogue.SnapshotHashes();
            ExtensionCreateResult result;
            using (var denied = WindowsDirectoryEnumerationDenial.Create(catalogue.Path, destination))
            {
                result = await operation.ExecuteAsync(request, TestContext.Current.CancellationToken);
            }
            Assert.Equal(CliSemanticStatus.Incomplete, result.Status);
            Assert.Contains(result.Findings, finding => finding.Code == ExtensionCreateFindingCode.CatalogueUnavailable);
            Assert.Equal(before, catalogue.SnapshotHashes());
            Renderers.MatchDetails(result, "catalogue-unreadable", extensionSourcePath: catalogue.Path);
        }
        finally
        {
            DeleteScaffold(destination);
        }
    }

    [Trait("Boundary", "Output")]
    [Fact(DisplayName = "Extension create output preserves its written manifest when payload directory creation is denied")]
    public async Task PartialWriteFailure()
    {
        if (!OperatingSystem.IsWindows())
        {
            Assert.Skip("This directory creation boundary uses an owned Windows catalogue ACL.");
            return;
        }
        using var catalogue = TemporaryWorkspace.Create("extension-create-output-partial");
        catalogue.CreateDirectory("packages");
        var cataloguePath = catalogue.Combine("packages");
        var scripted = ScriptedCliTerminal.Lines([], canPrompt: false);
        var prompts = new CliPrompts(scripted.Terminal);
        var operation = ExtensionCreateOperationFactory.Create(
            ExtensionInteractionTestFactory.ForCreate(prompts));
        var request = new ExtensionCreateRequest
        {
            StableId = "toolkit",
            CataloguePath = cataloguePath,
            Mode = ExtensionCreateMode.Apply,
            Automatic = true,
            AllowInteraction = false,
            Name = null,
            Description = null,
            PackageVersion = null,
            Dependencies = [],
        };
        var destination = Path.Combine(cataloguePath, "toolkit");
        try
        {
            var planning = await operation.PlanAsync(request, TestContext.Current.CancellationToken);
            var plan = Assert.IsType<Core.Commands.Extension.Create.Models.Planning.ExtensionCreatePlan>(planning.Plan);
            ExtensionCreateResult result;
            using (var denied = WindowsChildDirectoryCreationDenial.Create(catalogue.Path, cataloguePath))
            {
                result = await operation.ExecuteAsync(request, TestContext.Current.CancellationToken);
            }
            Assert.Equal(CliSemanticStatus.Failed, result.Status);
            Assert.Equal(plan.ManifestBytes.ToArray(), File.ReadAllBytes(Path.Combine(destination, "extension.json")));
            Assert.False(Directory.Exists(Path.Combine(destination, "content")));
            Assert.Single(result.AppliedEffects);
            var failure = Assert.Single(result.Findings, finding => finding.Code == ExtensionCreateFindingCode.ApplicationFailed);
            Assert.Equal($"Access to the path '{Path.Combine(destination, "content", ".agents")}' is denied.", failure.Cause);
            Renderers.MatchDetails(result, "write-failed-partial", extensionSourcePath: cataloguePath,
                diagnosticComparer: new ExtensionCreateDiagnosticSnapshotComparer(cataloguePath));
        }
        finally
        {
            DeleteScaffold(destination);
        }
    }

    [Trait("Boundary", "Output")]
    [Theory(DisplayName = "Extension create output preserves scaffolds, exact no-ops, metadata and required input")]
    [InlineData("created", (int)CliSemanticStatus.Complete)]
    [InlineData("created-with-metadata", (int)CliSemanticStatus.Complete)]
    [InlineData("dry-run", (int)CliSemanticStatus.Complete)]
    [InlineData("already-present", (int)CliSemanticStatus.Complete)]
    [InlineData("destination-has-other-content", (int)CliSemanticStatus.Blocked)]
    [InlineData("missing-id-non-interactive", (int)CliSemanticStatus.Invalid)]
    [InlineData("prompted-id-and-path", (int)CliSemanticStatus.Complete)]
    [InlineData("invalid-id", (int)CliSemanticStatus.Invalid)]
    [InlineData("cancelled", (int)CliSemanticStatus.Interrupted)]
    public async Task Scaffold(string situation, int status)
    {
        using var catalogue = TemporaryWorkspace.Create("extension-create-output");
        var destination = catalogue.Combine("toolkit");
        if (situation == "destination-has-other-content") catalogue.WriteText("toolkit/occupied.txt", "preserve occupied content\n");
        var prompted = situation == "prompted-id-and-path";
        var scripted = ScriptedCliTerminal.Lines(
            prompted ? ["toolkit", catalogue.Path, "yes"] : [],
            canPrompt: prompted);
        var prompts = new CliPrompts(scripted.Terminal);
        var operation = ExtensionCreateOperationFactory.Create(
            ExtensionInteractionTestFactory.ForCreate(prompts));
        var stableId = situation switch
        {
            "missing-id-non-interactive" or "prompted-id-and-path" => null,
            "invalid-id" => "INVALID ID",
            _ => "toolkit",
        };
        var request = new ExtensionCreateRequest
        {
            StableId = stableId,
            CataloguePath = prompted ? null : catalogue.Path,
            Name = situation == "created-with-metadata" ? "My Toolkit" : null,
            Description = situation == "created-with-metadata" ? "Shared authoring tools." : null,
            PackageVersion = situation == "created-with-metadata" ? "2.0.0" : null,
            Dependencies = situation == "created-with-metadata" ? ["base"] : [],
            Automatic = !prompted && situation != "dry-run",
            AllowInteraction = prompted,
            Mode = situation == "dry-run" ? ExtensionCreateMode.DryRun : ExtensionCreateMode.Apply,
        };
        try
        {
            if (situation == "already-present")
            {
                var initial = await operation.ExecuteAsync(request, TestContext.Current.CancellationToken);
                Assert.Equal(CliSemanticStatus.Complete, initial.Status);
            }

            var before = catalogue.SnapshotHashes();
            using var cancellation = new CancellationTokenSource();
            if (situation == "cancelled") cancellation.Cancel();
            var result = await operation.ExecuteAsync(request, cancellation.Token);
            Assert.Equal((CliSemanticStatus)status, result.Status);
            if (situation is "created" or "created-with-metadata" or "prompted-id-and-path")
            {
                Assert.True(File.Exists(Path.Combine(destination, "extension.json")));
                Assert.True(Directory.Exists(Path.Combine(destination, "content", ".agents")));
            }
            else Assert.Equal(before, catalogue.SnapshotHashes());
            if (prompted) Assert.NotEmpty(scripted.Output.ToString());
            Renderers.MatchDetails(result, situation, extensionSourcePath: catalogue.Path, testName: situation);
        }
        finally
        {
            if (situation != "destination-has-other-content" && Directory.Exists(destination))
            {
                DeleteScaffold(destination);
            }
        }
    }

    private static void DeleteScaffold(string destination)
    {
        var manifest = Path.Combine(destination, "extension.json");
        if (File.Exists(manifest))
        {
            Assert.Equal(0, (int)(File.GetAttributes(manifest) & (FileAttributes.Directory | FileAttributes.ReparsePoint | FileAttributes.Device)));
            File.Delete(manifest);
        }
        foreach (var path in new[] { Path.Combine(destination, "content", ".agents"), Path.Combine(destination, "content"), destination })
        {
            if (!Directory.Exists(path)) continue;
            Assert.Equal(0, (int)(File.GetAttributes(path) & (FileAttributes.ReparsePoint | FileAttributes.Device)));
            Directory.Delete(path, recursive: false);
        }
    }
}
