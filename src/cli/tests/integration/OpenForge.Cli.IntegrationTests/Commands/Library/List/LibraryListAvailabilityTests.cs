using OpenForge.Cli.Core.Commands.Library.List.Models.Result;
using OpenForge.Cli.Core.Commands.Library.Models.Result.Coordinates.Observation;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.IntegrationTests.Commands.Library.Shared.Reading;

namespace OpenForge.Cli.IntegrationTests.Commands.Library.List;

public sealed class LibraryListAvailabilityTests
{
    [Trait("Boundary", "OS")]
    [Theory(DisplayName = "Library List preserves unavailable source or link observations and incomplete coverage"), Trait("Feature", "library-read"), Trait("Evidence", "Integration")]
    [InlineData("source")]
    [InlineData("destination")]
    public static async Task RequiredObservationUnavailable(string boundary)
    {
        if (!OperatingSystem.IsLinux())
        {
            Assert.Skip("Required permission evidence targets Linux.");
            return;
        }

        using var fixture = new LibraryReadWorkspace();
        fixture.Source();
        fixture.SourceFile();
        fixture.Record(LibraryReadWorkspace.ReviewPath);
        fixture.CurrentLink();
        var directory = fixture.Files.Combine(boundary == "source" ? "shared/team" : ".agents/directives");
        var before = fixture.Snapshot();
        var mode = File.GetUnixFileMode(directory);
        try
        {
            File.SetUnixFileMode(directory, UnixFileMode.None);
            var result = await fixture.ListAsync(TestContext.Current.CancellationToken);
            Assert.Equal(boundary == "source" ? CliSemanticStatus.Attention : CliSemanticStatus.Incomplete, result.Status);
            Assert.Equal(boundary == "source" ? LibraryCoverage.Complete : LibraryCoverage.Incomplete, result.Result.Coverage);
            var library = Assert.Single(result.Result.Libraries);
            if (boundary == "source")
            {
                Assert.Equal(LibrarySourceRootViewState.Unavailable, library.SourceRootState);
                Assert.Contains(result.Result.Findings, finding => finding.Code == LibraryListFindingCode.SourceRootUnavailable);
            }
            else
            {
                Assert.Equal(LibraryLinkViewState.Unavailable, Assert.Single(library.Paths).State);
                Assert.Contains(result.Result.Findings, finding => finding.Code == LibraryListFindingCode.LinkUnavailable);
            }
        }
        finally
        {
            File.SetUnixFileMode(directory, mode);
        }

        Assert.Equal(before, fixture.Snapshot());
        fixture.AssertNoPersistentState();
    }
}
