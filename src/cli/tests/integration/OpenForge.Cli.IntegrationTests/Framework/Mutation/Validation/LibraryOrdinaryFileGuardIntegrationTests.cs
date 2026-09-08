using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem;
using OpenForge.Cli.Core.Framework.Mutation.Validation;
using OpenForge.Cli.Core.Framework.Mutation.Validation.Models;
using OpenForge.Cli.Core.Framework.Workspace;
using OpenForge.Cli.TestSupport;

namespace OpenForge.Cli.IntegrationTests.Framework.Mutation.Validation;

[Trait("Feature", "library-foundation"), Trait("Evidence", "Integration")]
public sealed class LibraryOrdinaryFileGuardIntegrationTests
{
    [Theory(DisplayName = "Ordinary file expectations reject relative absolute and dangling final links before target resolution")]
    [InlineData("relative"), InlineData("absolute"), InlineData("dangling")]
    public static async Task RejectsFinalLinks(string form)
    {
        using var temporary = TemporaryWorkspace.Create("library-ordinary-guard");
        var source = temporary.CreateFile("shared/a.md", "source");
        var raw = form switch
        {
            "relative" => "../shared/a.md",
            "absolute" => source,
            "dangling" => "../shared/missing.md",
            _ => throw new ArgumentOutOfRangeException(nameof(form)),
        };
        var destination = temporary.CreateFileSymbolicLink(".agents/a.md", raw);
        var expected = form == "dangling" ? FileExpectation.Missing(destination)
            : FileExpectation.File(destination, source, FileExpectation.Hash("source"u8));
        var workspace = new CliWorkspace(temporary.Path, temporary.Path, CliWorkspaceSelectionMethod.ExplicitWorkspace);

        var result = await new FileExpectationValidator(new PhysicalPathResolver()).ValidateAsync(workspace, expected, TestContext.Current.CancellationToken);

        Assert.Equal(FileExpectationValidationState.Blocked, result.State);
        Assert.Equal(raw, new FileInfo(destination).LinkTarget);
        Assert.Equal("source", File.ReadAllText(source));
        Assert.False(File.Exists(temporary.Combine("shared/missing.md")));
    }
}
