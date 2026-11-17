using System.Text;
using System.Text.Json.Nodes;
using OpenForge.Cli.Core.Framework.Settings;
using OpenForge.Cli.Core.Framework.Tools;
using OpenForge.Cli.Core.Framework.Tools.Catalogue;
using OpenForge.Cli.Core.Framework.Tools.Models;
using OpenForge.Cli.Core.Framework.Tools.Shared.Detection;
using OpenForge.Cli.Core.Framework.Tools.Shared.Integration;

namespace OpenForge.Cli.Core.UnitTests.Framework.Tools.Shared.Integration;

[Trait("Feature", "tool-catalogue"), Trait("Evidence", "Unit"), Trait("Boundary", "Input")]
public sealed class ToolIntegrationEditorTests
{
    private const string Archive = WorkspaceSettingsDefinitions.MemoryArchivePath;
    private const string OtherMember = "other";
    private static readonly ToolEntry CodeGraph = CodeGraphEntry.Create();
    private static readonly ToolEntry CodebaseMemory = CodebaseMemoryEntry.Create(ToolPlatform.Unix);

    public static TheoryData<string> Ids => [CodeGraphEntry.Id, CodebaseMemoryEntry.Id];

    [Theory(DisplayName = "Adding to a missing file makes the change present and removing it leaves nothing"), MemberData(nameof(Ids))]
    public void RoundTripsFromMissingFile(string id)
    {
        var integration = Entry(id).Integrations.Single();
        Assert.Equal(ToolIntegrationState.Absent, ToolIntegrationEditor.Read(integration, null, Archive).State);

        var added = ToolIntegrationEditor.Add(integration, null, Archive);

        Assert.Equal(ToolIntegrationState.Present, ToolIntegrationEditor.Read(integration, added, Archive).State);
        Assert.Null(ToolIntegrationEditor.Remove(integration, added, Archive));
    }

    [Fact(DisplayName = "The JSON union keeps other members and removes only its own value")]
    public void UnionKeepsOtherMembers()
    {
        var integration = (ToolJsonUnion)CodeGraph.Integrations.Single();
        var other = ToolIntegrationEditor.Pattern(integration, OtherMember);
        var source = new JsonObject { [OtherMember] = true, [integration.Member] = new JsonArray(JsonValue.Create(other)) };

        var added = ToolIntegrationEditor.Add(integration, Encoding.UTF8.GetBytes(source.ToJsonString()), Archive);
        var removed = ToolIntegrationEditor.Remove(integration, added, Archive)!;

        var result = JsonNode.Parse(removed)!.AsObject();
        Assert.True(result[OtherMember]!.GetValue<bool>());
        Assert.Equal([other], result[integration.Member]!.AsArray().Select(value => value!.GetValue<string>()));
    }

    [Fact(DisplayName = "The marked block keeps the rest of the file byte for byte")]
    public void BlockKeepsOtherLines()
    {
        var integration = CodebaseMemory.Integrations.Single();
        var source = Encoding.UTF8.GetBytes(OtherMember + "\n");

        var added = ToolIntegrationEditor.Add(integration, source, Archive);

        Assert.Equal(source, ToolIntegrationEditor.Remove(integration, added, Archive));
    }

    [Fact(DisplayName = "Malformed JSON, a non-array member and a duplicated block are conflicts")]
    public void Conflicts()
    {
        var union = (ToolJsonUnion)CodeGraph.Integrations.Single();
        var block = CodebaseMemory.Integrations.Single();
        var notArray = new JsonObject { [union.Member] = true }.ToJsonString();
        var once = ToolIntegrationEditor.Add(block, null, Archive);

        Assert.Equal(ToolIntegrationState.Conflict, ToolIntegrationEditor.Read(union, [(byte)'['], Archive).State);
        Assert.Equal(ToolIntegrationState.Conflict, ToolIntegrationEditor.Read(union, Encoding.UTF8.GetBytes(notArray), Archive).State);
        Assert.Equal(ToolIntegrationState.Conflict, ToolIntegrationEditor.Read(block, [.. once, .. once], Archive).State);
    }

    [Fact(DisplayName = "Detection reads the first version number a tool prints")]
    public void ReadsVersion()
    {
        Assert.Equal(CodeGraph.TestedVersion, ToolVersionText.Read($"{CodeGraph.Name} v{CodeGraph.TestedVersion}{Environment.NewLine}"));
        Assert.Null(ToolVersionText.Read(CodeGraph.Name));
    }

    private static ToolEntry Entry(string id) => id == CodeGraphEntry.Id ? CodeGraph : CodebaseMemory;
}
