using System.Text.Json;
using OpenForge.Cli.Core.Commands.Route.Update.Models.Presentation;
using OpenForge.Cli.Core.Commands.Route.Update.Shared.Rendering;

namespace OpenForge.Cli.Core.UnitTests.Commands.Route.Update;

public sealed class RouteUpdateJsonPresenceContractTests
{
    [Fact(DisplayName = "Route Update schema-v1 orders present workspace next and effect change facts"), Trait("Feature", "route-update"), Trait("Evidence", "UnitContract")]
    public void SchemaV1OrdersPresentWorkspaceNextAndEffectChangeFacts()
    {
        var document = new RouteUpdateJsonDocument
        {
            SchemaVersion = 1,
            Command = "route update",
            Status = "failed",
            Workspace = new RouteUpdateJsonWorkspace
            {
                Path = "/workspace",
                SelectedBy = "explicit-workspace",
            },
            Result = new RouteUpdateJsonResult
            {
                Mode = "apply",
                Target = new RouteUpdateJsonTarget
                {
                    Requested = "memory/topic",
                    SelectedBy = "source-id",
                    Id = "memory/topic",
                    Path = ".agents/memory/topic.md",
                    Form = "ordinary-markdown",
                    OverwritePaths = [],
                },
                Patch = new RouteUpdateJsonPatch
                {
                    Description = new RouteUpdateJsonDescriptionPatch
                    {
                        Requested = true,
                        Before = "Before",
                        Expected = "After",
                        State = "changed",
                    },
                    Responsibility = new RouteUpdateJsonResponsibilityPatch
                    {
                        Requested = false,
                        Operation = "not-requested",
                        Before = "Owns before",
                        Expected = null,
                        State = "not-requested",
                    },
                    Tags = new RouteUpdateJsonTagsPatch
                    {
                        Requested = false,
                        Before = ["Memory"],
                        Expected = null,
                        State = "not-requested",
                    },
                },
                Template = null,
                Plan = new RouteUpdateJsonPlan
                {
                    Completeness = "complete",
                    Safety = "safe",
                    Body = "preserved",
                },
                Effects =
                [
                    new RouteUpdateJsonEffect
                    {
                        Path = ".agents/memory/topic.md",
                        Kind = "routed-file",
                        Action = "replace",
                        Change = new RouteUpdateJsonChange
                        {
                            Before = "before-hash",
                            Expected = "expected-hash",
                        },
                        Preview = [],
                        Outcome = "verification-failed",
                        Residual = "retained",
                    },
                ],
                UnchangedPaths = [".agents/memory/_memory.md"],
                Recovery = new RouteUpdateJsonRecovery
                {
                    State = "retained",
                    ResidualPath = "/tmp/recovery.zip",
                },
                Verification = "failed",
                Findings =
                [
                    new RouteUpdateJsonFinding
                    {
                        Code = "route-update.verification-failed",
                        Status = "failed",
                        Target = ".agents/memory/topic.md",
                        Cause = "Verification failed.",
                    },
                ],
            },
            Next = new RouteUpdateJsonNext
            {
                Command = "open-forge route update --verbose",
                Reason = "Retry with bounded diagnostics.",
            },
        };

        var json = JsonSerializer.Serialize(
            document,
            RouteUpdateJsonContext.Default.RouteUpdateJsonDocument);
        using var parsed = JsonDocument.Parse(json);
        var root = parsed.RootElement;
        var workspace = root.GetProperty("workspace");
        var next = root.GetProperty("next");
        var effect = Assert.Single(
            root.GetProperty("result").GetProperty("effects").EnumerateArray());
        var change = effect.GetProperty("change");

        AssertPropertyOrder(workspace, "path", "selectedBy");
        Assert.Equal("/workspace", workspace.GetProperty("path").GetString());
        Assert.Equal(
            "explicit-workspace",
            workspace.GetProperty("selectedBy").GetString());
        AssertPropertyOrder(next, "command", "reason");
        Assert.Equal(
            "open-forge route update --verbose",
            next.GetProperty("command").GetString());
        Assert.Equal(
            "Retry with bounded diagnostics.",
            next.GetProperty("reason").GetString());
        AssertPropertyOrder(
            effect,
            "path",
            "kind",
            "action",
            "change",
            "preview",
            "outcome",
            "residual");
        AssertPropertyOrder(change, "before", "expected");
        Assert.Equal("before-hash", change.GetProperty("before").GetString());
        Assert.Equal("expected-hash", change.GetProperty("expected").GetString());
        Assert.NotEqual(JsonValueKind.Null, workspace.ValueKind);
        Assert.NotEqual(JsonValueKind.Null, next.ValueKind);
        Assert.NotEqual(JsonValueKind.Null, change.ValueKind);
    }

    private static void AssertPropertyOrder(
        JsonElement element,
        params string[] expected)
        => Assert.Equal(
            expected,
            element.EnumerateObject().Select(property => property.Name));
}
