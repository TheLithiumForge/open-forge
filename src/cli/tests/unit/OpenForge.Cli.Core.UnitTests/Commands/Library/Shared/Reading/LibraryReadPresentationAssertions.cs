using System.Text.Json;

namespace OpenForge.Cli.Core.UnitTests.Commands.Library.Shared.Reading;

internal static class LibraryReadPresentationAssertions
{
    internal static void Members(JsonElement value, params string[] expected)
        => Assert.Equal(expected, value.EnumerateObject().Select(property => property.Name));

    internal static void Envelope(JsonElement root, string command, string status)
    {
        Members(root, "schemaVersion", "command", "status", "detail", "filter", "workspace", "summary", "findings", "effects", "counts", "limitations", "data", "recovery", "next");
        Assert.Equal(3, root.GetProperty("schemaVersion").GetInt32());
        Assert.Equal(command, root.GetProperty("command").GetString());
        Assert.Equal(status, root.GetProperty("status").GetString());
        Assert.Equal(JsonValueKind.Object, root.GetProperty("data").ValueKind);
        Assert.Equal(JsonValueKind.Null, root.GetProperty("next").ValueKind);
        if (root.GetProperty("workspace").ValueKind != JsonValueKind.Null)
        {
            Members(root.GetProperty("workspace"), "path", "selectedBy");
            Assert.Equal(LibraryReadInputs.Workspace.LexicalRoot, root.GetProperty("workspace").GetProperty("path").GetString());
            Assert.Equal("explicit-workspace", root.GetProperty("workspace").GetProperty("selectedBy").GetString());
        }
    }
}
