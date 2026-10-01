using System.Text;
using System.Text.Json;
using OpenForge.Cli.IntegrationTests.Commands.Status;
using OpenForge.Cli.IntegrationTests.Hosting;
using OpenForge.Cli.TestSupport;

namespace OpenForge.Cli.IntegrationTests.Commands.Doctor;

public sealed class DoctorGeneratedNavigationOwnershipIntegrationTests
{
    private const string LoaderPath = ".agents/loader.md";
    private const string MapsEntrypointPath = ".agents/maps/_maps.md";
    private const string ManagedChangedCode = "framework.managed-changed";
    private const string ManagedMissingCode = "framework.managed-missing";
    private const string GeneratedRegionStaleCode = "route.generated-region-stale";

    [Theory(DisplayName = "Doctor assigns stale loader navigation to routes without implying authored modification"), Trait("Feature", "doctor-command"), Trait("Evidence", "Integration")]
    [InlineData(false)]
    [InlineData(true)]
    public async Task StaleLoaderNavigationDoesNotImplyAuthoredModification(bool editLoader)
    {
        using var workspace = TemporaryWorkspace.Create("doctor-generated-navigation-ownership");
        try
        {
            var install = await CliHostCapture.RunAsync(
                ["install", "--automatic"],
                workspace.Path);
            Assert.Equal(0, install.ExitCode);
            Assert.Equal(string.Empty, install.Error);

            var loaderPath = workspace.Combine(LoaderPath);
            var loaderBytes = File.ReadAllBytes(loaderPath);

            File.Delete(workspace.Combine(MapsEntrypointPath));
            if (editLoader)
            {
                EditLoaderTitle(loaderPath, loaderBytes);
            }

            var beforeDoctor = workspace.SnapshotHashes();
            var doctor = await CliHostCapture.RunAsync(
                ["doctor", "--format", "json", "--detail", "full"],
                workspace.Path);

            Assert.Equal(string.Empty, doctor.Error);
            using var document = JsonDocument.Parse(doctor.Output);
            var findings = document.RootElement.GetProperty("findings").EnumerateArray().ToArray();

            Assert.Contains(findings, finding =>
                finding.GetProperty("code").GetString() == ManagedMissingCode
                && SubjectPath(finding) == MapsEntrypointPath);
            Assert.Contains(findings, finding =>
                finding.GetProperty("code").GetString() == GeneratedRegionStaleCode
                && SubjectPath(finding) == LoaderPath);

            if (editLoader)
            {
                Assert.Contains(findings, finding =>
                    finding.GetProperty("code").GetString() == ManagedChangedCode
                    && SubjectPath(finding) == LoaderPath);
            }
            else
            {
                Assert.DoesNotContain(findings, finding =>
                    finding.GetProperty("code").GetString() == ManagedChangedCode
                    && SubjectPath(finding) == LoaderPath);
            }

            Assert.Equal(beforeDoctor, workspace.SnapshotHashes());
        }
        finally
        {
            StatusInstalledWorkspaceArtifacts.Delete(workspace);
        }
    }

    private static void EditLoaderTitle(string loaderPath, byte[] loaderBytes)
    {
        const string title = "# Open Forge Loader";
        const string editedTitle = "# Open Forge Loader (authored title edit)";
        var loader = Encoding.UTF8.GetString(loaderBytes);
        var entriesStart = loader.IndexOf("\n## Entries\n", StringComparison.Ordinal);
        var titleStart = loader.IndexOf(title, StringComparison.Ordinal);
        Assert.True(entriesStart > titleStart);
        Assert.Equal(1, Count(loader, title));

        File.WriteAllBytes(
            loaderPath,
            Encoding.UTF8.GetBytes(loader.Replace(title, editedTitle, StringComparison.Ordinal)));
    }

    private static int Count(string value, string fragment)
        => (value.Length - value.Replace(fragment, string.Empty, StringComparison.Ordinal).Length)
            / fragment.Length;

    private static string SubjectPath(JsonElement finding)
        => finding.GetProperty("subject").GetProperty("path").GetString()
            ?? throw new InvalidOperationException("Doctor findings require a subject path.");
}
