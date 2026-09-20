using OpenForge.Cli.Core.Commands.Doctor.Models.Result;
using OpenForge.Cli.Core.Presentation.Doctor;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Pipeline;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Presentation;
using OpenForge.Cli.TestSupport.Snapshots;

namespace OpenForge.Cli.Core.UnitTests.Commands.Doctor.Shared.Rendering;

[Trait("Feature", "command-output-snapshots"), Trait("Evidence", "Unit")]
public sealed class DoctorBeforeOutputSnapshotTests
{
    [Trait("Boundary", "Output")]
    [Theory(DisplayName = "Doctor output preserves the complete named diagnosis fixture")]
    [InlineData("healthy", (int)CliSemanticStatus.Complete)]
    [InlineData("info-only", (int)CliSemanticStatus.Complete)]
    [InlineData("warnings-only", (int)CliSemanticStatus.Attention)]
    [InlineData("error-and-warnings", (int)CliSemanticStatus.Attention)]
    [InlineData("incomplete", (int)CliSemanticStatus.Incomplete)]
    [InlineData("blocked-workspace", (int)CliSemanticStatus.Blocked)]
    [InlineData("invalid-input", (int)CliSemanticStatus.Invalid)]
    [InlineData("changed-extension-file", (int)CliSemanticStatus.Attention)]
    [InlineData("stale-entries", (int)CliSemanticStatus.Attention)]
    [InlineData("library-drift", (int)CliSemanticStatus.Attention)]
    [InlineData("recovery-bundle", (int)CliSemanticStatus.Complete)]
    public void Diagnosis(string situation, int expectedStatus)
    {
        var result = DoctorBeforeOutputFixtures.Create(situation);
        Assert.Equal((CliSemanticStatus)expectedStatus, result.Status);
        foreach (var view in Enum.GetValues<CliDetail>())
        {
            var viewName = view.ToString().ToLowerInvariant();
            var rendering = DoctorPresentation.Rendering;
            var textRendered = CliRenderingStage.Render(new CliPresentationRequest<DoctorResult>(result, new(CliFormat.Text, view, null)), rendering);
            var jsonRendered = CliRenderingStage.Render(new CliPresentationRequest<DoctorResult>(result, new(CliFormat.Json, view, null)), rendering);
            CommandOutputSnapshot.MatchSnapshot(textRendered.PrimaryContent.Replace("\r\n", "\n", StringComparison.Ordinal), $"{situation}.{viewName}");
            CommandOutputSnapshot.MatchSnapshot(jsonRendered.PrimaryContent.Replace("\r\n", "\n", StringComparison.Ordinal), $"{situation}{CommandOutputSnapshot.JsonContentNameSegment}{viewName}");
            if (view == CliDetail.Debug)
            {
                if (textRendered.DiagnosticContent is { } textDiagnostics)
                {
                    CommandOutputSnapshot.MatchSnapshot(textDiagnostics.Replace("\r\n", "\n", StringComparison.Ordinal), $"{situation}.{viewName}.diagnostics");
                }

                if (jsonRendered.DiagnosticContent is { } jsonDiagnostics)
                {
                    CommandOutputSnapshot.MatchSnapshot(jsonDiagnostics.Replace("\r\n", "\n", StringComparison.Ordinal), $"{situation}{CommandOutputSnapshot.JsonContentNameSegment}{viewName}.diagnostics");
                }
            }
        }
    }
}
