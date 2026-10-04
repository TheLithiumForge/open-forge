using System.Text;
using OpenForge.Cli.Core.Framework.Libraries.Shared.Observation;
using OpenForge.Cli.Core.Framework.Ownership.Models.Document;
using OpenForge.Cli.Core.Framework.Ownership.Shared.Serialization;

namespace OpenForge.Cli.Core.UnitTests.Framework.Libraries.Shared.GitIgnore;

[Trait("Feature", "library-git-ignore"), Trait("Evidence", "Unit"), Trait("Boundary", "Processing")]
public sealed class LibraryGitIgnoreOwnershipTests
{
    [Theory(DisplayName = "Library Git-ignore intent survives source generated ownership and registration projection")]
    [InlineData(true)]
    [InlineData(false)]
    public void IntentRoundtripAndProjection(bool choice)
    {
        var claim = new LibraryOwnership("team", "vendor/team", "docs", ["a.md"], choice);
        var bytes = WorkspaceOwnershipCodec.Write(WorkspaceOwnershipDocument.Empty with { Libraries = [claim] });
        using var json = System.Text.Json.JsonDocument.Parse(bytes);
        Assert.Equal(choice, json.RootElement.GetProperty("libraries")[0].TryGetProperty("gitIgnore", out var flag));
        if (choice) Assert.True(flag.GetBoolean());
        Assert.DoesNotContain("\"gitIgnore\":false", Encoding.UTF8.GetString(bytes));
        var decoded = Assert.IsType<WorkspaceOwnershipDocument>(WorkspaceOwnershipCodec.Read(bytes).Document);
        Assert.Equal(choice, Assert.Single(decoded.Libraries).GitIgnore);
        Assert.Equal(choice, Assert.Single(LibraryRegistrationReader.ReadRegistrations(decoded).Libraries).GitIgnore);
    }

    [Fact(DisplayName = "Older Library records default to opt-out and retain their serialized shape")]
    public void OldRecordDefaultsFalse()
    {
        var decoded = Assert.IsType<WorkspaceOwnershipDocument>(WorkspaceOwnershipCodec.Read(Encoding.UTF8.GetBytes(
            "{\"schemaVersion\":1,\"libraries\":[{\"id\":\"team\",\"sourceRoot\":\"vendor/team\",\"destinationRoot\":\".\",\"paths\":[\"a.md\"]}]}")).Document);
        Assert.False(Assert.Single(decoded.Libraries).GitIgnore);
        Assert.DoesNotContain("gitIgnore", Encoding.UTF8.GetString(WorkspaceOwnershipCodec.Write(decoded)));
    }
}
