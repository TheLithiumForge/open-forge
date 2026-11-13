using System.Text.Json;
using OpenForge.Cli.Core.Framework.Ownership.Shared.Serialization;
using OpenForge.Cli.Core.Framework.Tools.Models.Registration;
using OpenForge.Cli.Core.Framework.Tools.Shared.Serialization;

namespace OpenForge.Cli.Core.UnitTests.Framework.Tools.Shared.Registration;

[Trait("Feature", "tool-registration"), Trait("Evidence", "Unit")]
public sealed class ToolHookContractTests
{
    private const string EventPointer = "/hooks/UserPromptSubmit";
    private const string Recognition = "codegraph prompt-hook";

    [Fact(DisplayName = "Hook declarations round trip event selectors, platform pairs and complete declined entries")]
    public void EventDeclarationsRoundTrip()
    {
        var seed = ToolEntryCase.Definition();
        var original = seed.Rewrites.OfType<ToolHookCommandRewrite>().Single();
        var hook = original with
        {
            Pointer = EventPointer,
            MatchCommand = true,
            Expected = Recognition,
            Replacement = "${openForge} tool run " + Recognition,
            RequiredReplacementSubstring = Recognition,
            PlatformOverrides = new(StringComparer.Ordinal) { [ToolHookPlatforms.Windows] = new("codegraph.cmd prompt-hook", "${openForge} tool run " + Recognition) },
        };
        using var expected = JsonDocument.Parse("{\"type\":\"command\",\"command\":\"codegraph prompt-hook\"}");
        using var windows = JsonDocument.Parse("{\"type\":\"command\",\"command\":\"codegraph.cmd prompt-hook\"}");
        var removal = seed.Components[0].DeclinedEntries[0] with
        {
            Pointer = EventPointer,
            MatchCommand = true,
            RemoveCreatedEmptyGroup = true,
            Expected = expected.RootElement.Clone(),
            PlatformOverrides = new(StringComparer.Ordinal) { [ToolHookPlatforms.Windows] = windows.RootElement.Clone() },
            AbsentCommands = [hook.Replacement],
        };
        var registration = seed with { Rewrites = [hook], Components = [seed.Components[0] with { DeclinedEntries = [removal] }] };
        var bytes = JsonSerializer.SerializeToUtf8Bytes(registration, ToolJsonContext.Default.ToolRegistration);
        var read = Assert.IsType<ToolRegistration>(JsonSerializer.Deserialize(bytes, ToolJsonContext.Default.ToolRegistration));
        var decoded = Assert.IsType<ToolHookCommandRewrite>(Assert.Single(read.Rewrites));
        Assert.Equal(EventPointer, decoded.Pointer);
        Assert.True(decoded.MatchCommand);
        Assert.Equal(hook.PlatformOverrides, decoded.PlatformOverrides);
        Assert.Equal(Recognition, decoded.RequiredReplacementSubstring);
        var decline = Assert.Single(Assert.Single(read.Components).DeclinedEntries);
        Assert.True(decline.MatchCommand);
        Assert.True(decline.RemoveCreatedEmptyGroup);
        Assert.Equal(removal.AbsentCommands, decline.AbsentCommands);
        Assert.True(JsonElement.DeepEquals(windows.RootElement, Assert.IsType<Dictionary<string, JsonElement>>(decline.PlatformOverrides)[ToolHookPlatforms.Windows]));
    }

    [Fact(DisplayName = "Created-group receipts survive the lock graph without storing an array position")]
    public void GroupReceiptRoundTrips()
    {
        using var empty = JsonDocument.Parse("{\"matcher\":\"\",\"hooks\":[]}");
        var hook = ToolEntryCase.Definition().Rewrites.OfType<ToolHookCommandRewrite>().Single();
        var receipt = new ToolRewriteReceipt
        {
            Id = hook.Id,
            Rewrite = hook,
            Declined = false,
            CreatedHookGroup = new(hook.File, EventPointer, empty.RootElement.Clone()),
        };
        var bytes = JsonSerializer.SerializeToUtf8Bytes(receipt, WorkspaceOwnershipJsonContext.Default.ToolRewriteReceipt);
        var decoded = Assert.IsType<ToolRewriteReceipt>(JsonSerializer.Deserialize(bytes, WorkspaceOwnershipJsonContext.Default.ToolRewriteReceipt));
        var group = Assert.IsType<ToolHookGroupReceipt>(decoded.CreatedHookGroup);
        Assert.Equal(hook.File, group.File);
        Assert.Equal(EventPointer, group.EventPointer);
        Assert.True(JsonElement.DeepEquals(empty.RootElement, group.EmptyGroup));
        var legacy = JsonSerializer.SerializeToUtf8Bytes(receipt with { CreatedHookGroup = null }, WorkspaceOwnershipJsonContext.Default.ToolRewriteReceipt);
        Assert.Null(JsonSerializer.Deserialize(legacy, WorkspaceOwnershipJsonContext.Default.ToolRewriteReceipt)?.CreatedHookGroup);
    }
}
