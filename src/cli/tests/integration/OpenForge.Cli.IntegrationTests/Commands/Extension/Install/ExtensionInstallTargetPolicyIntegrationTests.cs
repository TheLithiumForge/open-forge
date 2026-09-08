using System.Text;
using OpenForge.Cli.Core.Commands.Extension.Install.Models.Planning;
using OpenForge.Cli.Core.Commands.Extension.Install.Models.Request;
using OpenForge.Cli.Core.Commands.Extension.Install.Models.Result;
using OpenForge.Cli.Core.Commands.Extension.Install.Shared.Planning;
using OpenForge.Cli.Core.Framework.Extensions.Models;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Lifecycle;
using OpenForge.Cli.Core.Framework.Lifecycle.Models;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem;
using OpenForge.Cli.Core.Framework.Mutation.Validation;
using OpenForge.Cli.Core.Framework.Workspace;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Interaction;

namespace OpenForge.Cli.IntegrationTests.Commands.Extension.Install;

public sealed class ExtensionInstallTargetPolicyIntegrationTests
{
    [Theory(DisplayName = "Extension Install force cannot replace authored or marker-ambiguous sources through command composition"),
     Trait("Feature", "extension-install"), Trait("Evidence", "Integration")]
    [InlineData("valid-authored")]
    [InlineData("malformed-marker")]
    public static async Task ComposedForceCannotReplaceProtectedSource(string scenario)
    {
        using var workspace = ExtensionInstallIntegrationWorkspace.Create(
            "extension-install-authored-source-policy");
        await workspace.SeedFrameworkAsync();
        using var source = ExtensionInstallCatalogue.Create(
            "extension-install-authored-source-policy-catalogue");
        var target = $".agents/guidance/{scenario}.md";
        source.AddPackage(
            "toolkit",
            [],
            (target, OpenForge.Cli.TestSupport.OpenForgeDocumentSeed.Metadata(
                "Intended package source",
                ["Extension"],
                "# Intended package source\n")));
        workspace.CreateOccupant(target, scenario == "valid-authored"
            ? OpenForge.Cli.TestSupport.OpenForgeDocumentSeed.Metadata(
                "User-authored source",
                ["User"],
                "# Existing user-authored source\n")
            : "# Existing ambiguous source\n\n## Entries\n\n<!-- open-forge:generated-index:end -->\n<!-- open-forge:generated-index:start -->\n");
        var before = workspace.Snapshot();

        var result = await workspace.RunAsync(
        [
            "extension", "install", "toolkit",
            "--source", source.Path,
            "--force", "--automatic", "--json",
        ]);

        Assert.Equal(5, result.ExitCode);
        Assert.Equal(CliSemanticStatus.Blocked, result.Status);
        using var document = System.Text.Json.JsonDocument.Parse(result.StandardOutput);
        Assert.Contains(
            document.RootElement.GetProperty("result").GetProperty("findings").EnumerateArray(),
            finding => finding.GetProperty("code").GetString()
                == "extension-install.ownership-conflict"
                && finding.GetProperty("target").GetString() == target);
        Assert.Equal(before, workspace.Snapshot());
    }

    [Fact(DisplayName = "Extension Install force cannot replace an arbitrary unknown agents file"), Trait("Feature", "extension-install"), Trait("Evidence", "Integration")]
    public async Task ComposedForceRequiresPositiveInitialOccupantEligibility()
    {
        using var workspace = ExtensionInstallIntegrationWorkspace.Create(
            "extension-install-unknown-occupant-policy");
        await workspace.SeedFrameworkAsync();
        using var source = ExtensionInstallCatalogue.Create(
            "extension-install-unknown-occupant-policy-source");
        const string target = ".agents/arbitrary.txt";
        source.AddPackage("toolkit", [], (target, "intended package bytes\n"));
        workspace.CreateOccupant(target, "unknown workspace bytes\n");
        var before = workspace.Snapshot();

        var result = await workspace.RunAsync(
        [
            "extension", "install", "toolkit",
            "--source", source.Path,
            "--force", "--automatic", "--json",
        ]);

        Assert.Equal(5, result.ExitCode);
        Assert.Equal(CliSemanticStatus.Blocked, result.Status);
        using var document = System.Text.Json.JsonDocument.Parse(result.StandardOutput);
        Assert.Contains(
            document.RootElement.GetProperty("result").GetProperty("findings").EnumerateArray(),
            finding => finding.GetProperty("code").GetString()
                == "extension-install.ownership-conflict"
                && finding.GetProperty("target").GetString() == target);
        Assert.Equal(before, workspace.Snapshot());
    }

    [Theory(DisplayName = "Extension Install force cannot replace Framework or current authored targets"), Trait("Feature", "extension-install"), Trait("Evidence", "Integration")]
    [InlineData(".agents/loader.md", "framework")]
    [InlineData(".agents/user/_user.md", "authored")]
    public static async Task ForceCannotReplaceProtectedWorkspaceTargets(
        string target,
        string protection)
    {
        var root = Path.Combine(
            Path.GetTempPath(),
            $"extension-install-target-policy-{Guid.NewGuid():N}");
        var parent = Path.GetDirectoryName(Path.Combine(root, target));
        Assert.NotNull(parent);
        Directory.CreateDirectory(parent);
        await File.WriteAllTextAsync(
            Path.Combine(root, target),
            "# Existing protected source\n",
            TestContext.Current.CancellationToken);
        try
        {
            var workspace = new CliWorkspace(
                root,
                root,
                CliWorkspaceSelectionMethod.ExplicitWorkspace);
            var request = new ExtensionInstallRequest(
                workspace,
                ExtensionInstallMode.Apply,
                ["toolkit"],
                all: false,
                sourcePath: null,
                force: true,
                automatic: true,
                allowInteraction: false);
            var inspector = new ExtensionInstallTargetInspector(
                new CliInteractiveSession(TextReader.Null, TextWriter.Null, canPrompt: false),
                new FileExpectationValidator(new PhysicalPathResolver()));
            var package = Package(target);
            var payloadBytes = package.Payload[0].Bytes;
            Assert.NotNull(payloadBytes);

            var result = await inspector.InspectAsync(
                new ExtensionInstallTargetInspectionInput
                {
                    Request = request,
                    Packages = [package],
                    CurrentExtensions = EmptyExtensions(),
                    FrameworkLifecycle = Framework(
                        protection == "framework" ? [target] : []),
                    ProtectedAuthoredPaths = protection == "authored"
                        ? new HashSet<string>([target], StringComparer.Ordinal)
                        : new HashSet<string>(StringComparer.Ordinal),
                    InitialForceEligiblePaths = new HashSet<string>(StringComparer.Ordinal),
                    Topology = ExtensionInstallTopology.Create(
                        new Dictionary<string, byte[]>(StringComparer.Ordinal)
                        {
                            [target] = payloadBytes.Value.ToArray(),
                        },
                        new Dictionary<string, byte[]>(StringComparer.Ordinal),
                        []),
                    SourceIdentity = "embedded catalogue",
                },
                TestContext.Current.CancellationToken);

            Assert.Null(result.State);
            Assert.NotNull(result.Finding);
            Assert.Equal(ExtensionInstallFindingCode.OwnershipConflict, result.Finding.Code);
            Assert.Equal(target, result.Finding.Target);
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    private static ExtensionPackageFact Package(string target)
        => Package("toolkit", target, "# Intended package source\n");

    private static ExtensionPackageFact Package(
        string id,
        string target,
        string contents)
    {
        var bytes = Encoding.UTF8.GetBytes(contents);
        var file = ExtensionPackageFileFact.Create(new ExtensionPackageFileSnapshot
        {
            Path = $"content/{target}",
            TargetPath = target,
            State = ExtensionPackageFileReadState.Available,
            ByteLength = bytes.Length,
            Sha256 = FileExpectation.Hash(bytes),
            Bytes = bytes,
        });
        return ExtensionPackageFact.Create(
            new ExtensionPackageManifestFact
            {
                Id = id,
                Name = id,
                Description = $"{id} package.",
                Version = "1.0.0",
                Dependencies = [],
            },
            new ExtensionPackageContentsFact
            {
                ManifestPath = "extension.json",
                Payload = [file],
            });
    }

    private static ExtensionLifecycleState EmptyExtensions()
        => new()
        {
            Coverage = LifecycleSchema.CompleteCoverage,
            Packages = [],
            Paths = [],
        };

    private static FrameworkLifecycleState Framework(string[] targets)
        => new()
        {
            Coverage = LifecycleSchema.CompleteCoverage,
            Source = new FrameworkLifecycleSource
            {
                Id = "open-forge",
                Version = null,
                InventoryFingerprint = new string('0', 64),
            },
            Targets = [.. targets.Select(target => new FrameworkLifecycleTarget
            {
                Path = target,
                SourceAssetPath = target,
                Region = null,
                BaselineFingerprint = new string('0', 64),
                FingerprintKind = LifecycleSchema.ExactBytesFingerprintKind,
            })],
            GeneratedRegions = [],
        };
}
