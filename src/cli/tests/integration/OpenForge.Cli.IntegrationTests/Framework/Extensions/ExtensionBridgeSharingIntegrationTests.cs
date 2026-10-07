using System.Text.Json;
using OpenForge.Cli.Core.Framework.Extensions;
using OpenForge.Cli.Core.Framework.Extensions.Operational;
using OpenForge.Cli.Core.Framework.Extensions.Operational.Models.Registration;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.OperationalContributors.Models;
using OpenForge.Cli.IntegrationTests.Commands.Extension.Install;

namespace OpenForge.Cli.IntegrationTests.Framework.Extensions;

[Trait("Feature", "workspace-route-sharing"), Trait("Evidence", "Integration"), Trait("Boundary", "OS")]
public sealed class ExtensionBridgeSharingIntegrationTests
{
    private const string Templates = ".agents/templates/_templates.md";
    private const string Core = ".agents/templates/core/_core.md";

    [Fact(DisplayName = "Private bundled templates need no shared bridge registration and retain full target diagnosis")]
    public async Task PrivateBundledTemplatesRetainLocalDiagnosis()
    {
        using var workspace = ExtensionInstallIntegrationWorkspace.Create("private-template-bridges");
        await workspace.SeedFrameworkAsync();
        var configure = await workspace.RunAsync(["install", "--configure", "--preset", "custom", "--route", "templates=git-ignore", "--automatic", "--format", "json"]);
        Assert.True(configure.ExitCode == 0, configure.StandardOutput);
        await InstallTemplates(workspace);
        Assert.DoesNotContain("core/_core.md", workspace.ReadText(Templates), StringComparison.Ordinal);
        Assert.Contains("!/.agents/templates/_templates.md", workspace.ReadText(".gitignore"), StringComparison.Ordinal);

        var before = workspace.Snapshot();
        var doctor = await workspace.RunAsync(["doctor", "--format", "json", "--detail", "full"]);
        Assert.True(doctor.ExitCode == 0, doctor.StandardOutput);
        using var diagnosis = JsonDocument.Parse(doctor.StandardOutput);
        AssertNoBridgeFindings(diagnosis.RootElement);
        var view = await Contributor().ReadDoctorAsync(workspace.Workspace, TestContext.Current.CancellationToken);
        Assert.Equal(ExtensionBridgeRegistrationCoverage.Complete, view.BridgeRegistrations.Coverage);
        Assert.Empty(view.BridgeRegistrations.Observations);
        Assert.Contains(view.Targets, target => target.Target.Path == Core && target.Target.State == OperationalTargetState.Current);
        Assert.Equal(before, workspace.Snapshot());

        var privateLeaf = view.Targets.First(target => target.Target.Path.StartsWith(".agents/templates/core/", StringComparison.Ordinal) && target.Target.Path != Core).Target.Path;
        workspace.ReplaceText(privateLeaf, workspace.ReadText(privateLeaf) + "\nLocal private edit.\n");
        before = workspace.Snapshot();
        var changed = await workspace.RunAsync(["doctor", "--format", "json", "--detail", "full"]);
        using var changedDiagnosis = JsonDocument.Parse(changed.StandardOutput);
        Assert.Contains(changedDiagnosis.RootElement.GetProperty("findings").EnumerateArray(), finding =>
            finding.GetProperty("code").GetString() == "extension.managed-changed"
            && finding.GetProperty("subject").GetProperty("path").GetString() == privateLeaf);
        AssertNoBridgeFindings(changedDiagnosis.RootElement);
        Assert.Equal(before, workspace.Snapshot());
    }

    [Fact(DisplayName = "Public bundled templates still diagnose a missing bridge registration")]
    public async Task PublicMissingRegistrationRemainsDiagnosed()
    {
        using var workspace = ExtensionInstallIntegrationWorkspace.Create("public-template-bridges");
        await workspace.SeedFrameworkAsync();
        await InstallTemplates(workspace);
        var text = workspace.ReadText(Templates);
        var entry = Assert.Single(text.Split('\n'), line => line.Contains("](core/_core.md)", StringComparison.Ordinal));
        workspace.ReplaceText(Templates, text.Replace(entry + "\n", string.Empty, StringComparison.Ordinal));
        var before = workspace.Snapshot();
        var doctor = await workspace.RunAsync(["doctor", "--format", "json", "--detail", "full"]);
        using var diagnosis = JsonDocument.Parse(doctor.StandardOutput);
        Assert.Contains(diagnosis.RootElement.GetProperty("findings").EnumerateArray(), finding =>
            finding.GetProperty("code").GetString() == "extension.bridge-registration"
            && finding.GetProperty("subject").GetProperty("path").GetString() == Core);
        var view = await Contributor().ReadDoctorAsync(workspace.Workspace, TestContext.Current.CancellationToken);
        Assert.Contains(view.BridgeRegistrations.Observations, observation => observation.TargetPath == Core
            && observation.ParentPath == Templates && observation.State == ExtensionBridgeRegistrationState.Missing);
        Assert.Equal(before, workspace.Snapshot());
    }

    private static async Task InstallTemplates(ExtensionInstallIntegrationWorkspace workspace)
    {
        var install = await workspace.RunAsync(["extension", "install", "core-templates", "--automatic", "--format", "json"]);
        Assert.True(install.ExitCode == 0, install.StandardOutput);
        Assert.True(File.Exists(workspace.Combine(Core)));
    }

    private static ExtensionLifecycleOperationalContributor Contributor()
    {
        var paths = new PhysicalPathResolver();
        return new(paths, new ExtensionSourceReader(paths), new ExtensionLifecycleTargetReader(paths));
    }

    private static void AssertNoBridgeFindings(JsonElement result)
        => Assert.DoesNotContain(result.GetProperty("findings").EnumerateArray(), finding => finding.GetProperty("code").GetString() == "extension.bridge-registration");
}
