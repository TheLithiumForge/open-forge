using OpenForge.Cli.Core.Framework.Ownership;
using OpenForge.Cli.Core.Framework.Ownership.Models.Document;
using OpenForge.Cli.Core.Framework.Ownership.Shared.Serialization;

namespace OpenForge.Cli.IntegrationTests.Framework.Ownership;

internal static class OwnershipFixture
{
    internal static void Libraries(string workspace, LibraryOwnership library)
    {
        var path = Path.Combine(workspace, WorkspaceOwnershipDefinitions.RelativePath);
        var document = File.Exists(path)
            ? WorkspaceOwnershipCodec.Read(File.ReadAllBytes(path)).Document!
            : WorkspaceOwnershipDocument.Empty;
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        File.WriteAllBytes(path, WorkspaceOwnershipCodec.Write(document with { Libraries = [library] }));
    }
}
