using System.Text.Json;
using OpenForge.Cli.Core.Commands.Doctor;
using OpenForge.Cli.Core.Commands.Doctor.Models.Request;
using OpenForge.Cli.Core.Commands.Doctor.Models.Result;
using OpenForge.Cli.Core.Presentation.Doctor;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Pipeline;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Presentation;

namespace OpenForge.Cli.Core.UnitTests.Commands.Doctor;

public sealed class DoctorProjectionParityTests
{
    [Trait("Boundary", "Output")]
    [Fact(DisplayName = "Doctor native text and JSON projections retain the same typed result")]
    [Trait("Feature", "doctor-command"), Trait("Evidence", "Unit")]
    public async Task ProjectionsRetainTheSameTypedResult()
    {
        var support = new DoctorOperationTestSupport();
        var result = await new DoctorOperation(support.Catalogue)
            .ExecuteAsync(
                new DoctorRequest(DoctorOperationTestSupport.Workspace()),
                CancellationToken.None);

        var text = CliRenderingStage.Render(
            new CliPresentationRequest<DoctorResult>(result, new(CliFormat.Text, CliDetail.Full, null)),
            DoctorPresentation.Rendering).PrimaryContent;
        using var json = JsonDocument.Parse(CliRenderingStage.Render(
            new CliPresentationRequest<DoctorResult>(result, new(CliFormat.Json, CliDetail.Full, null)),
            DoctorPresentation.Rendering).PrimaryContent);

        var root = json.RootElement;
        Assert.Equal(3, root.GetProperty("schemaVersion").GetInt32());
        Assert.Equal(result.Command, root.GetProperty("command").GetString());
        Assert.Equal(result.Status switch
        {
            CliSemanticStatus.Complete => "completed",
            CliSemanticStatus.Attention => "completed-with-warnings",
            CliSemanticStatus.Incomplete => "incomplete",
            CliSemanticStatus.Invalid => "invalid-input",
            CliSemanticStatus.Blocked => "blocked",
            CliSemanticStatus.Failed => "failed",
            CliSemanticStatus.Interrupted => "cancelled",
            _ => throw new ArgumentOutOfRangeException(),
        }, root.GetProperty("status").GetString());

        var findings = root.GetProperty("findings").EnumerateArray().ToArray();
        foreach (var finding in result.Diagnosis.Domains.SelectMany(domain => domain.Findings))
        {
            var code = DoctorDefinitions.ReadFindingKind(finding.Kind);
            Assert.Contains(findings, value => value.GetProperty("code").GetString() == code);
            Assert.Contains(finding.Subject.Path ?? finding.Subject.Identifier ?? string.Empty, text, StringComparison.Ordinal);
        }

        var categories = root.GetProperty("data").GetProperty("categories").EnumerateArray().ToArray();
        Assert.Equal(Enum.GetValues<DoctorDomainKind>().Length, categories.Length);
        Assert.All(categories, category =>
        {
            Assert.True(category.TryGetProperty("name", out _));
            Assert.True(category.TryGetProperty("coverage", out _));
            Assert.True(category.TryGetProperty("counts", out _));
            Assert.True(category.TryGetProperty("lanes", out _));
        });
    }
}
