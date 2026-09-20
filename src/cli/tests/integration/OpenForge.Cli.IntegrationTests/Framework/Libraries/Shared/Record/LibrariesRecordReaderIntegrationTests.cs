using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Libraries.Models.Identity;
using OpenForge.Cli.Core.Framework.Libraries.Models.Observation;
using OpenForge.Cli.Core.Framework.Libraries.Operational.Models;
using OpenForge.Cli.Core.Framework.Libraries.Shared.Observation;
using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.TestSupport;

namespace OpenForge.Cli.IntegrationTests.Framework.Libraries.Shared.Record;

[Trait("Feature", "library-foundation"), Trait("Evidence", "Integration")]
public sealed class LibrariesRecordReaderIntegrationTests
{
    [Trait("Boundary", "OS")]
    [Theory(DisplayName = "Library reader distinguishes strict empty record from proven missing without creating state")]
    [InlineData(false), InlineData(true)]
    public static async Task ReadsOnlyExactConsumerRecord(bool present)
    {
        using var temporary = TemporaryWorkspace.Create("library-record");
        temporary.CreateDirectory(".agents");
        temporary.CreateFile("open-forge.lock.json", "unrelated root record");
        const string record = "{\"schemaVersion\":1,\"libraries\":[]}";
        if (present)
        {
            temporary.CreateFile(".agents/open-forge.lock.json", record);
        }
        var before = temporary.SnapshotHashes();
        var workspace = new CliWorkspace(temporary.Path, temporary.Path, CliWorkspaceSelectionMethod.ExplicitWorkspace);

        var result = await LibraryRegistrationReader.ReadAsync(new PhysicalPathResolver(), workspace, TestContext.Current.CancellationToken);

        Assert.Equal(present ? LibraryRegistrationReadState.Complete : LibraryRegistrationReadState.Missing, result.State);
        if (present)
        {
            Assert.Empty(Assert.IsType<LibraryRegistrationSet>(result.Record).Libraries);
            Assert.NotNull(result.Snapshot);
        }
        else
        {
            Assert.Null(result.Record);
        }
        Assert.Equal(before, temporary.SnapshotHashes());
    }

    [Trait("Boundary", "OS")]
    [Theory(DisplayName = "Library lock reader reports nonordinary boundaries without following record bytes")]
    [InlineData("relative"), InlineData("absolute"), InlineData("dangling"), InlineData("directory"), InlineData("parent")]
    public static async Task BlocksNonordinaryRecord(string scenario)
    {
        using var temporary = TemporaryWorkspace.Create("library-record-unsafe");
        var target = temporary.CreateFile("actual/open-forge.lock.json", "{\"schemaVersion\":1,\"libraries\":[]}");
        switch (scenario)
        {
            case "relative": temporary.CreateFileSymbolicLink(".agents/open-forge.lock.json", "../actual/open-forge.lock.json"); break;
            case "absolute": temporary.CreateFileSymbolicLink(".agents/open-forge.lock.json", target); break;
            case "dangling": temporary.CreateFileSymbolicLink(".agents/open-forge.lock.json", "absent.json"); break;
            case "directory": temporary.CreateDirectory(".agents/open-forge.lock.json"); break;
            case "parent": temporary.CreateDirectorySymbolicLink(".agents", "actual"); break;
        }
        var workspace = new CliWorkspace(temporary.Path, temporary.Path, CliWorkspaceSelectionMethod.ExplicitWorkspace);

        var result = await LibraryRegistrationReader.ReadAsync(new PhysicalPathResolver(), workspace, TestContext.Current.CancellationToken);

        Assert.Equal(LibraryRegistrationReadState.Unavailable, result.State);
        Assert.NotNull(result.OwnershipObservation);
        Assert.Null(result.Record);
        Assert.Equal("{\"schemaVersion\":1,\"libraries\":[]}", await File.ReadAllTextAsync(target, TestContext.Current.CancellationToken));
    }
    [Trait("Boundary", "OS")]
    [Theory(DisplayName = "Library lock reader declines ambiguous ownership and canonicalizes unordered claims")]
    [InlineData("a", "a", (int)LibraryRegistrationReadState.Malformed)]
    [InlineData("b", "a", (int)LibraryRegistrationReadState.Complete)]
    public static async Task PreservesDecodedFailureClassification(string firstId, string secondId, int state)
    {
        using var temporary = TemporaryWorkspace.Create("library-record-classification");
        var json = $$"""
            {"schemaVersion":1,"libraries":[
             {"id":"{{firstId}}","sourceRoot":"one","destinationRoot":"docs","paths":["a.md"]},
             {"id":"{{secondId}}","sourceRoot":"two","destinationRoot":"other","paths":["a.md"]}]}
            """;
        temporary.CreateFile(".agents/open-forge.lock.json", json);
        var before = temporary.SnapshotHashes();
        var workspace = new CliWorkspace(temporary.Path, temporary.Path, CliWorkspaceSelectionMethod.ExplicitWorkspace);

        var result = await LibraryRegistrationReader.ReadAsync(new PhysicalPathResolver(), workspace, TestContext.Current.CancellationToken);

        Assert.Equal((LibraryRegistrationReadState)state, result.State);
        if (firstId == secondId)
        {
            Assert.Null(result.Record);
            Assert.NotNull(result.OwnershipObservation);
        }
        else
        {
            Assert.Equal(new[] { "a", "b" }, result.Record!.Libraries.Select(library => library.Id.Value));
            Assert.Null(result.OwnershipObservation);
        }
        Assert.NotNull(result.Snapshot);
        Assert.Equal(System.Text.Encoding.UTF8.GetBytes(json), result.Snapshot.Bytes.ToArray());
        Assert.Equal(before, temporary.SnapshotHashes());
    }

    [Trait("Boundary", "OS")]
    [Fact(DisplayName = "Unreadable existing Library record is unavailable and grants no partial authority")]
    public async Task DistinguishesUnavailableFromMissing()
    {
        if (OperatingSystem.IsWindows())
        {
            Assert.Skip("This evidence requires Unix file permissions.");
            return;
        }
        using var temporary = TemporaryWorkspace.Create("library-record-access");
        var path = temporary.CreateFile(".agents/open-forge.lock.json", "{\"schemaVersion\":1,\"libraries\":[]}");
        var originalMode = File.GetUnixFileMode(path);
        File.SetUnixFileMode(path, UnixFileMode.None);
        try
        {
            var workspace = new CliWorkspace(temporary.Path, temporary.Path, CliWorkspaceSelectionMethod.ExplicitWorkspace);

            var result = await LibraryRegistrationReader.ReadAsync(new PhysicalPathResolver(), workspace, TestContext.Current.CancellationToken);

            Assert.Equal(LibraryRegistrationReadState.Unavailable, result.State);
            Assert.Null(result.Record);
            Assert.NotNull(result.Cause);
            Assert.True(File.Exists(path));
        }
        finally
        {
            File.SetUnixFileMode(path, originalMode);
        }
    }

}
