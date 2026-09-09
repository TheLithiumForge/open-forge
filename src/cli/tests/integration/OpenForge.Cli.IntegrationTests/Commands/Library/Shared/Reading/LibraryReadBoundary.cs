namespace OpenForge.Cli.IntegrationTests.Commands.Library.Shared.Reading;

internal static class LibraryReadBoundary
{
    internal static void Arrange(LibraryReadWorkspace fixture, string scenario)
    {
        switch (scenario)
        {
            case "malformed-record":
                fixture.Files.WriteText(LibraryReadWorkspace.RecordPath, "{\"schemaVersion\":1,\"libraries\":[],\"unexpected\":true}");
                return;
            case "duplicate-id":
                fixture.Files.WriteText(LibraryReadWorkspace.RecordPath, """
                    {"schemaVersion":1,"libraries":[
                    {"id":"team-knowledge","sourceRoot":"shared/team","destinationRoot":".","paths":[]},
                    {"id":"team-knowledge","sourceRoot":"shared/other","destinationRoot":".","paths":[]}]}
                    """);
                return;
            case "record-link":
                fixture.Files.WriteText("record-target.json", "{\"schemaVersion\":1,\"libraries\":[]}");
                fixture.Files.CreateFileSymbolicLink(LibraryReadWorkspace.RecordPath, "../record-target.json");
                return;
            case "missing-source":
                fixture.Record(LibraryReadWorkspace.ReviewPath);
                return;
            case "source-file":
                fixture.Files.WriteText(LibraryReadWorkspace.SourceRoot, "not a directory");
                fixture.Record(LibraryReadWorkspace.ReviewPath);
                return;
            case "missing-agents":
                fixture.Files.CreateDirectory(LibraryReadWorkspace.SourceRoot);
                fixture.Record(LibraryReadWorkspace.ReviewPath);
                return;
            case "source-link":
                fixture.Files.CreateDirectory("real-source/.agents");
                fixture.Files.CreateDirectorySymbolicLink(LibraryReadWorkspace.SourceRoot, "../real-source");
                fixture.Record(LibraryReadWorkspace.ReviewPath);
                return;
            case "consumer-overlap":
                fixture.Files.WriteText(LibraryReadWorkspace.RecordPath, """
                    {"schemaVersion":1,"libraries":[{"id":"team-knowledge","sourceRoot":".agents","destinationRoot":".","paths":[]}]}
                    """);
                fixture.Files.CreateDirectory(".agents/.agents");
                return;
            case "destination-parent-link":
                fixture.Source();
                fixture.SourceFile(".agents/linked/review.md");
                fixture.Record(".agents/linked/review.md");
                fixture.Files.CreateDirectory("destination-target");
                fixture.Files.CreateDirectorySymbolicLink(".agents/linked", "../destination-target");
                return;
            default:
                throw new ArgumentOutOfRangeException(nameof(scenario), scenario, "Unknown read-boundary fixture.");
        }
    }
}
