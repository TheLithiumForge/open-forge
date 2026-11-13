using System.Text;
using OpenForge.Cli.Core.Framework.Distribution.Models;
using OpenForge.Cli.Core.Framework.Documents.Markdown.Shared.ManagedHosts;
using OpenForge.Cli.Core.Framework.Tools.Models.Registration;
using OpenForge.Cli.Core.Framework.Tools.Shared.Registration;
using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.Core.Framework.Workspace.Models.Layout;
using OpenForge.Cli.TestSupport.Tools;

namespace OpenForge.Cli.Core.UnitTests.Framework.Tools.Shared.Registration;

[Trait("Feature", "tool-registration"), Trait("Evidence", "Unit")]
public sealed class ToolRoleGuidanceTests
{
    private const string Authored = "# Fixture authored\n\nKeep café and 🛠.\n";
    private const string CustomRoot = "context/team";

    [Fact(DisplayName = "A conditional marker shown in a code example stays authored content")]
    public void KeepsMarkerExamples()
    {
        var request = Request();
        var example = "```md\n" + ToolRoleGuidanceRenderer.Line(request.Active[0]) + "\n```\n";
        var before = Bytes(Block().Replace("Fixture body.", example + "\nFixture body.", StringComparison.Ordinal));
        var planner = new ToolRoleGuidancePlanner(Payload());
        var files = Files(before);
        foreach (var edit in planner.Plan(request, files).Edits) files[edit.File] = edit.After;
        Assert.All(planner.Observe(request, files), check => Assert.Equal(ToolRoleLineState.Matching, check.State));
        foreach (var edit in planner.Plan(request with { Active = [] }, files).Edits) Assert.Equal(before, edit.After);
    }

    [Theory(DisplayName = "Role lines add to both managed blocks, switch providers and clear while keeping all prior bytes")]
    [InlineData("\n", false), InlineData("\r\n", true)]
    public void AddsSwitchesAndClears(string newline, bool bom)
    {
        var request = Request();
        var before = Bytes((bom ? "\uFEFF" : string.Empty) + Authored.ReplaceLineEndings(newline) + Block().ReplaceLineEndings(newline)
            + ToolEntryCase.Text(ToolRegistrationFixtureSource.Read(ToolRegistrationFixtureSource.Agent)).ReplaceLineEndings(newline));
        var files = Files(before);
        var planner = new ToolRoleGuidancePlanner(Payload());
        var plan = planner.Plan(request, files);
        Assert.Equal(2, plan.Edits.Length);
        foreach (var edit in plan.Edits) { Assert.Equal(before, edit.Before); files[edit.File] = edit.After; }
        Assert.All(planner.Observe(request, files), check => Assert.Equal(ToolRoleLineState.Matching, check.State));
        Assert.Empty(planner.Plan(request, files).Edits);
        var next = request.Active[0] with { ToolId = ToolFixtureSource.Id + "-second", Guidance = request.Active[0].Guidance with { Line = "Use the second fixture Skill." } };
        request = request with { Active = [next] };
        var switched = planner.Plan(request, files);
        Assert.Equal(2, switched.Edits.Length);
        foreach (var edit in switched.Edits) files[edit.File] = edit.After;
        Assert.All(planner.Observe(request, files), check => Assert.Equal(ToolRoleLineState.Matching, check.State));
        var empty = request with { Active = [] };
        Assert.All(planner.Observe(empty, files), check => Assert.Equal(ToolRoleLineState.Drifted, check.State));
        foreach (var edit in planner.Plan(empty, files).Edits) { Assert.Equal(before, edit.After); files[edit.File] = edit.After; }
        Assert.Empty(planner.Plan(empty, files).Edits);
        Assert.Empty(planner.Observe(empty, files));
    }

    [Theory(DisplayName = "Absent blocks are planned with prior absence or authored bytes and use the chosen layout")]
    [InlineData(false), InlineData(true)]
    public void PlansAbsentBlock(bool authored)
    {
        var request = Request(CustomRoot);
        var before = authored ? Bytes(Authored) : null;
        var files = Files(before);
        var planner = new ToolRoleGuidancePlanner(Payload());
        Assert.All(planner.Observe(request, files), check => Assert.Equal(ToolRoleLineState.Missing, check.State));
        var plan = planner.Plan(request, files);
        Assert.Equal(2, plan.Edits.Length);
        foreach (var edit in plan.Edits)
        {
            Assert.Equal(before, edit.Before);
            if (before is not null) Assert.Equal(before, edit.After[..before.Length]);
            files[edit.File] = edit.After;
        }
        Assert.All(planner.Observe(request, files), check => Assert.Equal(ToolRoleLineState.Matching, check.State));
        var clear = planner.Plan(request with { Active = [] }, files);
        foreach (var edit in clear.Edits)
        {
            var baseline = WorkspaceBlockWithoutRoles(request, edit.File, authored);
            Assert.Equal(baseline, edit.After);
        }
    }

    [Fact(DisplayName = "The complete active role set is rendered in ordinal role order exactly once")]
    public void OrdersRoleLines()
    {
        var request = Request();
        var seed = request.Active[0];
        request = request with { Active = [seed with { Guidance = new("zeta", "Fixture Z.") }, seed with { Guidance = new("alpha", "Fixture A.") }] };
        var planner = new ToolRoleGuidancePlanner(Payload());
        var files = Files(Bytes(Block()));
        var plan = planner.Plan(request, files);
        foreach (var edit in plan.Edits) files[edit.File] = edit.After;
        Assert.All(planner.Observe(request, files), check => Assert.Equal(ToolRoleLineState.Matching, check.State));
        var selections = ToolRoleGuidanceRenderer.Select(request);
        Assert.Equal(new[] { "alpha", "zeta" }, selections.Select(selection => selection.Guidance.Role));
        foreach (var edit in plan.Edits)
        {
            var source = ToolEntryCase.Text(edit.After);
            var tagged = source.Split('\n').Where(line => line.Contains(ToolRoleGuidanceRenderer.MarkerPrefix, StringComparison.Ordinal)).ToArray();
            Assert.Equal(selections.Select(ToolRoleGuidanceRenderer.Line), tagged);
            files[edit.File] = Bytes(source.Replace(string.Join("\n", tagged), string.Join("\n", tagged.Reverse()), StringComparison.Ordinal));
        }
        Assert.All(planner.Observe(request, files), check => Assert.Equal(ToolRoleLineState.Drifted, check.State));
    }

    [Theory(DisplayName = "Malformed managed blocks and conditional markers refuse all role edits")]
    [InlineData("missing-footer"), InlineData("duplicate-block"), InlineData("malformed-role"), InlineData("invalid-utf8")]
    public void RefusesUncertainBlock(string situation)
    {
        var bytes = situation switch
        {
            "missing-footer" => Bytes($"# {MarkdownManagedHostReader.HeadingText}\n\nFixture body.\n"),
            "duplicate-block" => Bytes(Block() + Block()),
            "malformed-role" => Bytes(Block().Replace("Fixture body.", ToolRoleGuidanceRenderer.MarkerPrefix + "invalid", StringComparison.Ordinal)),
            "invalid-utf8" => [0xff, 0xfe],
            _ => throw new ArgumentOutOfRangeException(nameof(situation)),
        };
        var planner = new ToolRoleGuidancePlanner(Payload());
        var request = Request();
        var plan = planner.Plan(request, Files(bytes));
        Assert.Empty(plan.Edits);
        Assert.All(plan.Checks, check => Assert.Equal(ToolRoleLineState.Drifted, check.State));
        Assert.All(planner.Observe(request with { Active = [] }, Files(bytes)), check => Assert.Equal(ToolRoleLineState.Drifted, check.State));
    }

    [Fact(DisplayName = "Clearing an absent role creates no file or block")]
    public void EmptyMappingNeedsNoBlock()
    {
        var planner = new ToolRoleGuidancePlanner(Payload());
        var empty = Request() with { Active = [] };
        Assert.Empty(planner.Plan(empty, Files(null)).Edits);
        Assert.Empty(planner.Observe(empty, Files(null)));
        var files = Files(Bytes(Authored));
        Assert.Empty(planner.Plan(empty, files).Edits);
    }

    [Theory(DisplayName = "Duplicate roles and multiline guidance are invalid requests")]
    [InlineData(false), InlineData(true)]
    public void RefusesInvalidMapping(bool multiline)
    {
        var request = Request();
        request = multiline ? request with { Active = [request.Active[0] with { Guidance = request.Active[0].Guidance with { Line = "Fixture\nline" } }] }
            : request with { Active = [request.Active[0], request.Active[0]] };
        Assert.Throws<InvalidDataException>(() => new ToolRoleGuidancePlanner(Payload()).Plan(request, Files(Bytes(Block()))));
    }

    private static ToolRoleGuidanceRequest Request(string mainRoot = WorkspaceLayout.DefaultMainRoot)
    {
        var root = Path.GetFullPath("fixture-workspace");
        var workspace = new CliWorkspace(root, root, CliWorkspaceSelectionMethod.ExplicitWorkspace);
        var guidance = Assert.Single(ToolEntryCase.Definition().RoleGuidance);
        return new(root, WorkspaceLayout.Create(workspace, mainRoot), [new(ToolFixtureSource.Id, guidance, WorkspaceLayout.NativeSkillsPath + "/fixture")]);
    }

    private static Dictionary<string, byte[]?> Files(byte[]? bytes) => new(StringComparer.Ordinal)
    { [WorkspaceLayoutNames.AgentHostFileName] = bytes, [WorkspaceLayoutNames.ClaudeHostFileName] = bytes };
    private static byte[] Bytes(string text) => Encoding.UTF8.GetBytes(text);
    private static string Block() => $"# {MarkdownManagedHostReader.HeadingText}\n\nFixture body. Read `{FrameworkPayloadAsset.LoaderPath}`.\n\n**{MarkdownManagedHostReader.FooterText}**\n";
    private static FrameworkPayload Payload() => FrameworkPayload.Create([
        FrameworkPayloadAsset.Create(FrameworkPayloadAsset.RootAgentPath, Bytes(Block())),
        FrameworkPayloadAsset.Create(FrameworkPayloadAsset.RootClaudePath, Bytes(Block())),
        FrameworkPayloadAsset.Create(FrameworkPayloadAsset.LoaderPath, Bytes("# Fixture loader\n")),
    ]);

    private static byte[] WorkspaceBlockWithoutRoles(ToolRoleGuidanceRequest request, string file, bool authored)
    {
        var asset = Payload().Find(file) ?? throw new InvalidDataException();
        var rendered = OpenForge.Cli.Core.Framework.Distribution.Shared.Content.WorkspaceLayoutPayloadRenderer.Render(request.Layout, file, asset.Bytes.AsMemory());
        var bytes = rendered.Bytes ?? throw new InvalidDataException();
        var source = authored ? Authored + "\n" : string.Empty;
        return Bytes(source + Encoding.UTF8.GetString(bytes.Span));
    }
}
