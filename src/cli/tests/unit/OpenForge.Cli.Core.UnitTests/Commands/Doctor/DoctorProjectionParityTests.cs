using OpenForge.Cli.Core.Commands.Doctor;
using OpenForge.Cli.Core.Commands.Doctor.Models.Request;
using OpenForge.Cli.Core.Commands.Doctor.Models.Result;
using OpenForge.Cli.Core.Commands.Doctor.Shared.Rendering;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Pipeline;

namespace OpenForge.Cli.Core.UnitTests.Commands.Doctor;

public sealed class DoctorProjectionParityTests
{
    [Fact(DisplayName = "Doctor compact, expanded, and JSON projections retain one typed result")]
    [Trait("Feature", "doctor-command"), Trait("Evidence", "Unit")]
    public async Task ProjectionsRetainTheSameTypedResult()
    {
        var support = new DoctorOperationTestSupport();
        var result = await new DoctorOperation(support.Catalogue)
            .ExecuteAsync(
                new DoctorRequest(DoctorOperationTestSupport.Workspace()),
                CancellationToken.None);
        var compact = Render(result, CliView.Compact);
        var expanded = Render(result, CliView.Expanded);
        var json = DoctorJsonProjection.Create(result);

        Assert.Contains($"Result: {DoctorWireVocabulary.Status(result.Status)}", compact, StringComparison.Ordinal);
        Assert.Contains($"Result: {DoctorWireVocabulary.Status(result.Status)}", expanded, StringComparison.Ordinal);
        Assert.Equal(DoctorWireVocabulary.Status(result.Status), json.Status);
        Assert.Equal(DoctorWireVocabulary.Coverage(result.Diagnosis.Coverage), json.Result.Coverage);

        foreach (var domain in result.Diagnosis.Domains)
        {
            var domainName = DoctorWireVocabulary.Domain(domain.Domain);
            Assert.Contains(domainName, compact, StringComparison.Ordinal);
            Assert.Contains(domainName, expanded, StringComparison.Ordinal);
            var jsonDomain = Assert.Single(json.Result.Domains, candidate =>
                string.Equals(candidate.Domain, domainName, StringComparison.Ordinal));
            Assert.Equal(DoctorWireVocabulary.Coverage(domain.Coverage), jsonDomain.Coverage);
            foreach (var limitation in domain.Limitations)
            {
                Assert.Contains(limitation.Message, compact, StringComparison.Ordinal);
                Assert.Contains(limitation.Message, expanded, StringComparison.Ordinal);
                Assert.Contains(jsonDomain.Limitations, candidate =>
                    string.Equals(candidate.Message, limitation.Message, StringComparison.Ordinal));
            }

            foreach (var finding in domain.Findings)
            {
                var kind = DoctorDefinitions.ReadFindingKind(finding.Kind);
                Assert.Contains(kind, compact, StringComparison.Ordinal);
                Assert.Contains(kind, expanded, StringComparison.Ordinal);
                Assert.Contains(jsonDomain.Findings, candidate =>
                    string.Equals(candidate.Kind, kind, StringComparison.Ordinal));
            }
        }
    }

    private static string Render(
        DoctorResult result,
        CliView view)
        => DoctorHumanRenderer.Render(new CliPresentationRequest<DoctorResult>(
            result,
            new CliPresentation(CliOutputFormat.Human, view, CliVerbosity.Normal)));
}
