using OpenForge.Cli.Core.Commands.Doctor.Models.Result;
using OpenForge.Cli.Core.Presentation.Doctor;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Pipeline;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Presentation;
using OpenForge.Cli.TestSupport.Snapshots;
using TheLithium.Imprint;

namespace OpenForge.Cli.Core.UnitTests.Commands.Doctor.Shared.Rendering;

[Trait("Feature", "command-output-snapshots"), Trait("Evidence", "Unit")]
public sealed class DoctorBeforeOutputSnapshotTests
{
    [Trait("Boundary", "Output")]
    [Theory(DisplayName = "Doctor output preserves the complete named diagnosis fixture")]
    [InlineData("healthy", (int)CliSemanticStatus.Complete)]
    [InlineData("info-only", (int)CliSemanticStatus.Complete)]
    [InlineData("info-only-explicit-workspace", (int)CliSemanticStatus.Complete)]
    [InlineData("warnings-only", (int)CliSemanticStatus.Attention)]
    [InlineData("error-and-warnings", (int)CliSemanticStatus.Attention)]
    [InlineData("incomplete", (int)CliSemanticStatus.Incomplete)]
    [InlineData("blocked-workspace", (int)CliSemanticStatus.Blocked)]
    [InlineData("invalid-input", (int)CliSemanticStatus.Invalid)]
    [InlineData("changed-extension-file", (int)CliSemanticStatus.Attention)]
    [InlineData("stale-entries", (int)CliSemanticStatus.Attention)]
    [InlineData("library-drift", (int)CliSemanticStatus.Attention)]
    [InlineData("recovery-bundle", (int)CliSemanticStatus.Complete)]
    [InlineData("recovery-stores", (int)CliSemanticStatus.Complete)]
    [InlineData("recovery-legacy-unavailable", (int)CliSemanticStatus.Incomplete)]
    public void Diagnosis(string situation, int expectedStatus)
    {
        var result = DoctorBeforeOutputFixtures.Create(situation);
        Assert.Equal((CliSemanticStatus)expectedStatus, result.Status);
        var mismatches = new List<string>();
        void Match(string content, string name)
        {
            try { CommandOutputSnapshot.MatchSnapshot(content, name, testName: nameof(Diagnosis)); }
            catch (SnapshotMismatchException exception) { mismatches.Add(exception.Message); }
        }
        foreach (var view in Enum.GetValues<CliDetail>())
        {
            var viewName = view.ToString().ToLowerInvariant();
            var rendering = DoctorPresentation.Rendering;
            var textRendered = CliRenderingStage.Render(new CliPresentationRequest<DoctorResult>(result, new(CliFormat.Text, view, null)), rendering);
            var jsonRendered = CliRenderingStage.Render(new CliPresentationRequest<DoctorResult>(result, new(CliFormat.Json, view, null)), rendering);
            var text = textRendered.PrimaryContent.Replace("\r\n", "\n", StringComparison.Ordinal);
            var json = jsonRendered.PrimaryContent.Replace("\r\n", "\n", StringComparison.Ordinal);
            if (result.WorkspacePath is { } workspace)
            {
                text = CommandOutputNormalization.ReplaceDelimitedPath(text, workspace, "<workspace>");
                json = CommandOutputNormalization.ReplaceDelimitedPath(json, workspace, "<workspace>");
            }
            Match(text, $"{situation}.{viewName}");
            Match(json, $"{situation}{CommandOutputSnapshot.JsonContentNameSegment}{viewName}");
            if (view == CliDetail.Debug)
            {
                if (textRendered.DiagnosticContent is { } textDiagnostics)
                {
                    Match(textDiagnostics.Replace("\r\n", "\n", StringComparison.Ordinal), $"{situation}.{viewName}.diagnostics");
                }

                if (jsonRendered.DiagnosticContent is { } jsonDiagnostics)
                {
                    Match(jsonDiagnostics.Replace("\r\n", "\n", StringComparison.Ordinal), $"{situation}{CommandOutputSnapshot.JsonContentNameSegment}{viewName}.diagnostics");
                }
            }
        }
        Assert.Empty(mismatches);
    }
}
