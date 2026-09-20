using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Libraries.Models.Identity;
using OpenForge.Cli.Core.Framework.Libraries.Models.Inventory;
using OpenForge.Cli.Core.Framework.Libraries.Shared.Source;
using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.TestSupport;

namespace OpenForge.Cli.IntegrationTests.Framework.Libraries.Shared.Source;

[Trait("Feature", "library-foundation"), Trait("Evidence", "Integration")]
public sealed class LibrarySourceRootReaderIntegrationTests
{
    [Trait("Boundary", "OS")]
    [Fact(DisplayName = "Library source root is available without any specially named child")]
    public void ReadsContainedOrdinarySource()
    {
        using var temporary = TemporaryWorkspace.Create("library-source");
        temporary.CreateDirectory(".agents");
        var source = temporary.CreateDirectory("shared/team");
        var request = Request(temporary, "shared/team");

        var result = LibrarySourceRootReader.Read(new PhysicalPathResolver(), request, TestContext.Current.CancellationToken);

        Assert.Equal(LibrarySourceRootState.Available, result.State);
        Assert.Equal(source, result.LexicalSourceRoot);
        Assert.Equal(source, result.PhysicalSourceRoot);
        Assert.True(result.LexicallyContained);
        Assert.True(result.PhysicallyContained);
        Assert.Null(result.Cause);
    }

    [Trait("Boundary", "OS")]
    [Theory(DisplayName = "Library source never treats missing or non-directory source roots as empty available sources")]
    [InlineData("source-missing"), InlineData("source-file")]
    public static void RejectsMissingAndNonordinaryBoundaries(string scenario)
    {
        using var temporary = TemporaryWorkspace.Create("library-source-missing");
        temporary.CreateDirectory(".agents");
        switch (scenario)
        {
            case "source-file": temporary.CreateFile("shared/team", "source is a file"); break;
        }

        var result = LibrarySourceRootReader.Read(new PhysicalPathResolver(), Request(temporary, "shared/team"), TestContext.Current.CancellationToken);

        Assert.Contains(result.State, new[] { LibrarySourceRootState.Missing, LibrarySourceRootState.Invalid });
        Assert.NotNull(result.Cause);
    }

    [Trait("Boundary", "OS")]
    [Theory(DisplayName = "Library source blocks linked source ancestry and outside aliases")]
    [InlineData("source-link"), InlineData("ancestor-link"), InlineData("outside-link")]
    public static void BlocksUnsafeBoundaries(string scenario)
    {
        using var temporary = TemporaryWorkspace.Create("library-source-unsafe");
        using var external = TemporaryWorkspace.Create("library-source-external");
        temporary.CreateDirectory(".agents");
        temporary.CreateDirectory("actual/team/.agents");
        var root = "shared/team";
        switch (scenario)
        {
            case "source-link": temporary.CreateDirectorySymbolicLink("shared/team", "../actual/team"); break;
            case "ancestor-link": temporary.CreateDirectorySymbolicLink("shared", "actual"); break;
            case "outside-link":
                external.CreateDirectory(".agents");
                temporary.CreateDirectorySymbolicLink("shared/team", external.Path);
                break;
        }

        var result = LibrarySourceRootReader.Read(new PhysicalPathResolver(), Request(temporary, root), TestContext.Current.CancellationToken);

        Assert.Equal(LibrarySourceRootState.Blocked, result.State);
        Assert.False(string.IsNullOrWhiteSpace(result.Cause));
    }

    [Trait("Boundary", "OS")]
    [Fact(DisplayName = "Inaccessible Library source boundary never produces available empty facts")]
    public void ReportsInaccessibleSource()
    {
        if (OperatingSystem.IsWindows())
        {
            Assert.Skip("This evidence requires Unix directory permissions.");
            return;
        }
        using var temporary = TemporaryWorkspace.Create("library-source-access");
        temporary.CreateDirectory(".agents");
        var source = temporary.CreateDirectory("shared/team");
        temporary.CreateDirectory("shared/team/.agents");
        var mode = File.GetUnixFileMode(source);
        File.SetUnixFileMode(source, UnixFileMode.None);
        try
        {
            var result = LibrarySourceRootReader.Read(new PhysicalPathResolver(), Request(temporary, "shared/team"), TestContext.Current.CancellationToken);

            Assert.Contains(result.State, new[] { LibrarySourceRootState.Inaccessible, LibrarySourceRootState.Unavailable });
            Assert.NotNull(result.Cause);
        }
        finally
        {
            File.SetUnixFileMode(source, mode);
        }
    }

    private static LibrarySourceRootRequest Request(TemporaryWorkspace temporary, string sourceRoot)
        => new()
        {
            Workspace = new CliWorkspace(temporary.Path, temporary.Path, CliWorkspaceSelectionMethod.ExplicitWorkspace),
            SourceRoot = WorkspaceRelativeDirectory.Create(sourceRoot),
        };
}
