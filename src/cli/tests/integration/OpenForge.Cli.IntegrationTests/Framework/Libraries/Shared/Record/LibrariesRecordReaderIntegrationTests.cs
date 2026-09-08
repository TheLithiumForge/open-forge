using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Libraries.Models.Record;
using OpenForge.Cli.Core.Framework.Libraries.Shared.Record;
using OpenForge.Cli.Core.Framework.Workspace;
using OpenForge.Cli.TestSupport;

namespace OpenForge.Cli.IntegrationTests.Framework.Libraries.Shared.Record;

[Trait("Feature", "library-foundation"), Trait("Evidence", "Integration")]
public sealed class LibrariesRecordReaderIntegrationTests
{
    [Theory(DisplayName = "Library reader distinguishes strict empty record from proven missing without creating state")]
    [InlineData(false), InlineData(true)]
    public static async Task ReadsOnlyExactConsumerRecord(bool present)
    {
        using var temporary = TemporaryWorkspace.Create("library-record");
        temporary.CreateDirectory(".agents");
        temporary.CreateFile("open-forge.libraries.json", "unrelated root record");
        const string record = "{\"schemaVersion\":1,\"libraries\":[]}";
        if (present)
        {
            temporary.CreateFile(".agents/open-forge.libraries.json", record);
        }
        var before = temporary.SnapshotHashes();
        var workspace = new CliWorkspace(temporary.Path, temporary.Path, CliWorkspaceSelectionMethod.ExplicitWorkspace);

        var result = await LibrariesRecordReader.ReadAsync(new PhysicalPathResolver(), workspace, TestContext.Current.CancellationToken);

        Assert.Equal(present ? LibrariesRecordReadState.Complete : LibrariesRecordReadState.Missing, result.State);
        if (present)
        {
            Assert.Empty(Assert.IsType<LibrariesRecord>(result.Record).Libraries);
            Assert.NotNull(result.Snapshot);
        }
        else
        {
            Assert.Null(result.Record);
        }
        Assert.Equal(before, temporary.SnapshotHashes());
    }

    [Theory(DisplayName = "Library record reader blocks linked leaves and linked consumer ancestry without following record bytes")]
    [InlineData("relative"), InlineData("absolute"), InlineData("dangling"), InlineData("directory"), InlineData("parent")]
    public static async Task BlocksNonordinaryRecord(string scenario)
    {
        using var temporary = TemporaryWorkspace.Create("library-record-unsafe");
        var target = temporary.CreateFile("actual/open-forge.libraries.json", "{\"schemaVersion\":1,\"libraries\":[]}");
        switch (scenario)
        {
            case "relative": temporary.CreateFileSymbolicLink(".agents/open-forge.libraries.json", "../actual/open-forge.libraries.json"); break;
            case "absolute": temporary.CreateFileSymbolicLink(".agents/open-forge.libraries.json", target); break;
            case "dangling": temporary.CreateFileSymbolicLink(".agents/open-forge.libraries.json", "absent.json"); break;
            case "directory": temporary.CreateDirectory(".agents/open-forge.libraries.json"); break;
            case "parent": temporary.CreateDirectorySymbolicLink(".agents", "actual"); break;
        }
        var workspace = new CliWorkspace(temporary.Path, temporary.Path, CliWorkspaceSelectionMethod.ExplicitWorkspace);

        var result = await LibrariesRecordReader.ReadAsync(new PhysicalPathResolver(), workspace, TestContext.Current.CancellationToken);

        Assert.Equal(LibrariesRecordReadState.Blocked, result.State);
        Assert.Null(result.Record);
        Assert.Equal("{\"schemaVersion\":1,\"libraries\":[]}", await File.ReadAllTextAsync(target, TestContext.Current.CancellationToken));
    }
    [Fact(DisplayName = "Unreadable existing Library record is unavailable and grants no partial authority")]
    public async Task DistinguishesUnavailableFromMissing()
    {
        if (OperatingSystem.IsWindows())
        {
            Assert.Skip("This evidence requires Unix file permissions.");
            return;
        }
        using var temporary = TemporaryWorkspace.Create("library-record-access");
        var path = temporary.CreateFile(".agents/open-forge.libraries.json", "{\"schemaVersion\":1,\"libraries\":[]}");
        var originalMode = File.GetUnixFileMode(path);
        File.SetUnixFileMode(path, UnixFileMode.None);
        try
        {
            var workspace = new CliWorkspace(temporary.Path, temporary.Path, CliWorkspaceSelectionMethod.ExplicitWorkspace);

            var result = await LibrariesRecordReader.ReadAsync(new PhysicalPathResolver(), workspace, TestContext.Current.CancellationToken);

            Assert.Equal(LibrariesRecordReadState.Unavailable, result.State);
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
