using System.Text.Json;
using System.Text.Json.Nodes;
using OpenForge.Cli.Core.Framework.Tools;
using OpenForge.Cli.Core.Framework.Tools.Models.Registration;
using OpenForge.Cli.Core.Framework.Tools.Shared.Registration;
using OpenForge.Cli.Core.Framework.Tools.Shared.Serialization;
using OpenForge.Cli.TestSupport;
using OpenForge.Cli.TestSupport.Tools;

namespace OpenForge.Cli.IntegrationTests.Framework.Tools.Shared.Registration;

[Trait("Feature", "tool-registration"), Trait("Evidence", "Integration")]
public sealed class ToolHookIntegrationTests
{
    public static IEnumerable<object[]> Scenarios() => ToolVocabulary.Targets.Order(StringComparer.Ordinal)
        .SelectMany(target => new[] { false, true }.Select(shared => new object[] { target, shared }));
    private static CancellationToken Token => TestContext.Current.CancellationToken;

    [Theory(DisplayName = "User hooks survive real-file rewrite, decline and receipt-based removal for every target"), MemberData(nameof(Scenarios))]
    public async Task KeepsExistingUserHooks(string target, bool shared)
    {
        using var temporary = TemporaryWorkspace.Create("hook-platform-selection");
        var definition = JsonSerializer.Deserialize(ToolRegistrationFixtureSource.Read(ToolRegistrationFixtureSource.HookDefinition),
            ToolJsonContext.Default.ToolRegistration) ?? throw new InvalidDataException();
        var hook = Assert.IsType<ToolHookCommandRewrite>(Assert.Single(definition.Rewrites));
        var prior = ToolRegistrationFixtureSource.Read(ToolRegistrationFixtureSource.UserHooks);
        var installed = ToolRegistrationFixtureSource.HookOutput(shared ? ToolRegistrationFixtureSource.SharedHookGroup : ToolRegistrationFixtureSource.CreatedHookGroup,
            target.StartsWith("win-", StringComparison.Ordinal));
        temporary.WriteBytes(hook.File, installed);
        var request = new ToolEntryRewriteRequest
        {
            WorkspaceRoot = temporary.Path,
            Rewrites = [hook],
            DeclinedComponents = [],
            Values = new Dictionary<string, string>(StringComparer.Ordinal)
            {
                [ToolVocabulary.OpenForgeVariable] = temporary.Combine("fixture binary 🛠", ToolEntryDeclarations.PathCommand),
                [ToolVocabulary.TargetVariable] = target,
            },
            BeforeInstallerFiles = new Dictionary<string, byte[]?> { [hook.File] = prior },
        };
        var planner = new ToolEntryPlanner();
        var verifier = new ToolEntryVerifier();
        var hashes = temporary.SnapshotHashes();
        var plan = await planner.Services.Plan(request, Token);
        Assert.Equal(hashes, temporary.SnapshotHashes());
        var rewrite = Assert.Single(plan.Edits);
        Assert.All(await verifier.RecheckAsync(request, rewrite, Token), check => Assert.Equal(ToolEntryState.Original, check.State));
        temporary.ReplaceBytes(hook.File, rewrite.After);
        var verified = await verifier.VerifyAsync(request, Token);
        var receipt = Assert.Single(verified.Rewrites);
        Assert.Equal(!shared, receipt.CreatedHookGroup is not null);
        var resolved = Assert.IsType<ToolHookCommandRewrite>(receipt.Rewrite);
        Assert.Null(resolved.PlatformOverrides);
        Assert.True(resolved.Replacement.Contains(hook.RequiredReplacementSubstring ?? throw new InvalidDataException(), StringComparison.Ordinal));
        AssertUsersSurvive(prior, rewrite.After, resolved.Replacement);
        var afterRewrite = temporary.SnapshotHashes();
        Assert.Equal(ToolEntryState.Matching, Assert.Single(await verifier.ObserveReceiptsAsync(temporary.Path, [receipt], [], Token)).State);
        Assert.Equal(afterRewrite, temporary.SnapshotHashes());

        var component = definition.Components[0];
        var removal = component.DeclinedEntries[0];
        var expected = JsonNode.Parse(removal.Expected.GetRawText())?.AsObject() ?? throw new InvalidDataException();
        expected[ToolJsonEntries.Command] = resolved.Replacement;
        using var expectedJson = JsonDocument.Parse(expected.ToJsonString());
        var removeRequest = request with
        {
            Rewrites = [],
            BeforeInstallerFiles = null,
            CreatedHookGroups = receipt.CreatedHookGroup is { } group ? [group] : [],
            DeclinedComponents = [component with { DeclinedEntries = [removal with { Expected = expectedJson.RootElement.Clone(), PlatformOverrides = null }] }],
        };
        var removed = Assert.Single((await planner.PlanAsync(removeRequest, Token)).Edits);
        temporary.ReplaceBytes(hook.File, removed.After);
        AssertSameDocument(prior, await File.ReadAllBytesAsync(temporary.Combine(hook.File), Token));

        temporary.ReplaceBytes(hook.File, installed);
        var decline = request with { DeclinedComponents = definition.Components };
        var declined = Assert.Single((await planner.PlanAsync(decline, Token)).Edits);
        temporary.ReplaceBytes(hook.File, declined.After);
        AssertSameDocument(prior, declined.After);
        var absence = await verifier.VerifyAsync(decline, Token);
        Assert.Empty(absence.Rewrites);
        Assert.Single(absence.DeclinedComponents);
        temporary.ReplaceBytes(hook.File, rewrite.After);
        Assert.Equal(ToolEntryState.Drifted, Assert.Single(await verifier.ObserveReceiptsAsync(temporary.Path, [], absence.DeclinedComponents, Token)).State);
    }

    private static void AssertSameDocument(byte[] expected, byte[] actual)
    {
        using var before = JsonDocument.Parse(expected);
        using var after = JsonDocument.Parse(actual);
        Assert.True(JsonElement.DeepEquals(before.RootElement, after.RootElement));
    }

    private static void AssertUsersSurvive(byte[] expected, byte[] actual, string toolCommand)
    {
        var root = JsonNode.Parse(actual)?.AsObject() ?? throw new InvalidDataException();
        var groups = root[ToolHookEntries.Hooks]?["UserPromptSubmit"]?.AsArray() ?? throw new InvalidDataException();
        for (var groupIndex = groups.Count - 1; groupIndex >= 0; groupIndex--)
        {
            var hooks = groups[groupIndex]?[ToolHookEntries.Hooks]?.AsArray() ?? throw new InvalidDataException();
            for (var index = hooks.Count - 1; index >= 0; index--)
                if (hooks[index]?[ToolJsonEntries.Command]?.GetValue<string>() == toolCommand) hooks.RemoveAt(index);
            if (hooks.Count == 0) groups.RemoveAt(groupIndex);
        }
        AssertSameDocument(expected, System.Text.Encoding.UTF8.GetBytes(root.ToJsonString()));
    }
}
