using System.Text.Json;
using OpenForge.Cli.Core.Framework.Ownership;

namespace OpenForge.Cli.IntegrationTests.Commands.Status;

[Trait("Feature", "ownership-operational-readers"), Trait("Evidence", "Integration")]
public sealed class OwnershipCommandIntegrationTests
{
    [Trait("Boundary", "Host")]
    [Theory(DisplayName = "Status and Doctor distinguish missing and malformed lock ownership without legacy fallback or writes")]
    [InlineData(false), InlineData(true)]
    public async Task MissingOrMalformedOwnershipPreservesTheOwnershipBoundary(bool malformed)
    {
        using var workspace = await StatusIntegrationWorkspace.CreateInstalledAsync("ownership-command");
        var path = workspace.Combine(WorkspaceOwnershipDefinitions.RelativePath);
        if (malformed)
        {
            File.WriteAllText(path, "malformed lock bytes");
        }
        else
        {
            File.Delete(path);
        }
        var before = workspace.SnapshotHashes();
        var recovery = StatusRecoveryCatalogue.SnapshotEntries(workspace.RecoveryDirectory());

        var statusRun = await StatusIntegrationApplication.RunAsync(workspace, "status", "--workspace", workspace.Path, "--format", "json", "--detail", "full");
        Assert.Equal(malformed ? 5 : 0, statusRun.ExitCode);
        using var status = StatusIntegrationApplication.ParseJson(statusRun);
        var facts = status.RootElement.GetProperty("data");
        Assert.Empty(facts.GetProperty("frameworkFiles").EnumerateArray());
        Assert.Empty(facts.GetProperty("extensions").EnumerateArray());
        Assert.Empty(facts.GetProperty("libraries").EnumerateArray());
        var statusFindings = status.RootElement.GetProperty("findings").EnumerateArray().ToArray();
        if (malformed)
        {
            Assert.Contains(statusFindings, finding =>
                finding.GetProperty("code").GetString() == "status.library-record-malformed");
        }
        else
        {
            Assert.All(statusFindings, finding =>
                Assert.EndsWith("ownership-observation", finding.GetProperty("code").GetString(), StringComparison.Ordinal));
        }

        var doctorRun = await StatusIntegrationApplication.RunAsync(workspace, "doctor", "--workspace", workspace.Path, "--format", "json", "--detail", "full");
        using var doctor = StatusIntegrationApplication.ParseJson(doctorRun);
        var findings = doctor.RootElement.GetProperty("findings").EnumerateArray().ToArray();
        foreach (var code in new[] { "framework.ownership-observation", "extension.ownership-observation", "library.ownership-observation" })
        {
            var finding = Assert.Single(findings, finding => finding.GetProperty("code").GetString() == code);
            Assert.Equal("info", finding.GetProperty("severity").GetString());
            Assert.Equal("informational", finding.GetProperty("resolution").GetString());
        }
        Assert.Equal(before, workspace.SnapshotHashes());
        Assert.Equal(recovery, StatusRecoveryCatalogue.SnapshotEntries(workspace.RecoveryDirectory()));
    }
}
