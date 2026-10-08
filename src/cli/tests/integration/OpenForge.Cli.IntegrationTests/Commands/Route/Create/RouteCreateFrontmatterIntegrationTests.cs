using System.Text;
using OpenForge.Cli.Core.Commands.Route.Create.Models.Operation;
using OpenForge.Cli.Core.Commands.Route.Create.Models.Planning;
using OpenForge.Cli.Core.Commands.Route.Create.Models.Request;
using OpenForge.Cli.Core.Commands.Route.Create.Models.Result;
using OpenForge.Cli.Core.Commands.Route.Create.Shared.Application;
using OpenForge.Cli.Core.Commands.Route.Create.Shared.Planning;
using OpenForge.Cli.Core.Framework.Documents.Markdown;
using OpenForge.Cli.Core.Framework.Documents.Metadata;
using OpenForge.Cli.Core.Framework.Documents.Metadata.Models;
using OpenForge.Cli.Core.Framework.Settings.Models.Observation;

namespace OpenForge.Cli.IntegrationTests.Commands.Route.Create;

[Trait("Feature", "route-create"), Trait("Evidence", "IntegrationBehavior"), Trait("Boundary", "OS")]
public sealed class RouteCreateFrontmatterIntegrationTests
{
    [Fact(DisplayName = "Route Create plans root metadata from the workspace preference")]
    public async Task CreatesRootMetadataInRootWorkspace()
    {
        using var workspace = RouteCreateIntegrationWorkspace.Create("route-create-root-metadata");
        workspace.SeedBase();
        workspace.WriteText(".agents/open-forge.json", "{\"frontmatter\":\"root\"}");

        var plan = await PlanAsync(workspace.Request());
        var metadata = ReadMetadata(plan.IntendedTargetBytes.AsSpan());

        Assert.Equal(FrontmatterForm.Root, metadata.Syntax.AuthoredForm);
        Assert.Equal("Project overview", metadata.Metadata?.Description);
        Assert.Equal(["Docs", "Overview"], metadata.Metadata?.Tags);
        Assert.Equal(FrontmatterForm.Root, plan.Settings.Document.Frontmatter);
    }

    [Fact(DisplayName = "Route Create preserves scoped metadata when the preference is absent")]
    public async Task CreatesScopedMetadataWithoutSetting()
    {
        using var workspace = RouteCreateIntegrationWorkspace.Create("route-create-default-metadata");
        workspace.SeedBase();
        workspace.SeedCompleteTarget();

        var plan = await PlanAsync(workspace.Request());

        Assert.Equal(WorkspaceSettingsReadState.Absent, plan.Settings.State);
        Assert.Equal(FrontmatterForm.Scoped, ReadMetadata(plan.IntendedTargetBytes.AsSpan()).Syntax.AuthoredForm);
        Assert.True(plan.IsNoOp);
        Assert.Equal(File.ReadAllBytes(workspace.Absolute(RouteCreateIntegrationWorkspace.TargetPath)), plan.IntendedTargetBytes.ToArray());
    }

    [Theory(DisplayName = "Route Create copies the exact Template body from either authored form")]
    [InlineData(false), InlineData(true)]
    public async Task TemplateBodyIsCopiedExactly(bool rootTemplate)
    {
        using var workspace = RouteCreateIntegrationWorkspace.Create("route-create-template-body-form");
        workspace.SeedBase();
        workspace.WriteText(".agents/open-forge.json", "{\"frontmatter\":\"root\"}");
        const string body = "\r\n# Starting {topic}\r\n\r\nCafé and λ.\r\n```yaml\r\nopen-forge:\r\n  tags: [Template]\r\n```\r\n";
        var header = rootTemplate
            ? "---\r\ndescription: Template metadata\r\ntags: [Template]\r\n---\r\n"
            : "---\r\nopen-forge:\r\n  description: Template metadata\r\n  tags: [Template]\r\n---\r\n";
        workspace.WriteText(RouteCreateIntegrationWorkspace.TemplatePath, header + body);

        var plan = await PlanAsync(workspace.Request(templateReference: RouteCreateIntegrationWorkspace.TemplateId));
        var document = new MarkdownDocumentParser().Parse(Encoding.UTF8.GetString(plan.IntendedTargetBytes.AsSpan()));
        var bodySpan = Assert.IsType<OpenForge.Cli.Core.Framework.Documents.Markdown.Models.MarkdownTextSpan>(document.BodySpan);

        Assert.Equal(Encoding.UTF8.GetBytes(body), Encoding.UTF8.GetBytes(document.Source.Substring(bodySpan.Start, bodySpan.Length)));
        Assert.Equal("Project overview", ReadMetadata(plan.IntendedTargetBytes.AsSpan()).Metadata?.Description);
    }

    [Theory(DisplayName = "Route Create invalidates plans after a preference or settings observation change")]
    [InlineData("{\"frontmatter\":\"root\"}"), InlineData("{\"frontmatter\":\"scoped\"}")]
    public async Task SettingsChangeInvalidatesPlan(string changedSettings)
    {
        using var workspace = RouteCreateIntegrationWorkspace.Create("route-create-settings-revalidation");
        workspace.SeedBase();
        var builder = new RouteCreatePlanBuilder();
        var plan = Assert.IsType<RouteCreatePlan>((await builder.BuildAsync(workspace.Request(), TestContext.Current.CancellationToken)).Plan);
        workspace.WriteText(".agents/open-forge.json", changedSettings);
        var before = workspace.SnapshotHashes();

        var result = await new RouteCreatePlanRevalidator(builder).RevalidateAsync(plan, TestContext.Current.CancellationToken);

        Assert.Equal(RouteCreatePlanRevalidationState.Changed, result.State);
        Assert.Equal(before, workspace.SnapshotHashes());
    }

    [Fact(DisplayName = "Route Create rejects invalid workspace settings before planning effects")]
    public async Task InvalidSettingsStopPlanning()
    {
        using var workspace = RouteCreateIntegrationWorkspace.Create("route-create-invalid-settings");
        workspace.SeedBase();
        workspace.WriteText(".agents/open-forge.json", "{\"frontmatter\":\"unknown\"}");
        var before = workspace.SnapshotHashes();

        var build = await new RouteCreatePlanBuilder().BuildAsync(workspace.Request(), TestContext.Current.CancellationToken);

        Assert.Null(build.Plan);
        Assert.Contains(build.Formation.Findings, finding => finding.Code == RouteCreateFindingCode.InvalidInput);
        Assert.Empty(build.Formation.Effects);
        Assert.Equal(before, workspace.SnapshotHashes());
    }

    private static async Task<RouteCreatePlan> PlanAsync(RouteCreateRequest request)
        => Assert.IsType<RouteCreatePlan>((await new RouteCreatePlanBuilder().BuildAsync(request, TestContext.Current.CancellationToken)).Plan);

    private static FrameworkDocumentMetadataFacts ReadMetadata(ReadOnlySpan<byte> bytes)
    {
        var facts = new FrameworkDocumentMetadataParser().Parse(
            new MarkdownDocumentParser().Parse(Encoding.UTF8.GetString(bytes)), FrameworkMetadataReadScope.RoutedSource);
        Assert.Equal(FrameworkDocumentMetadataState.Complete, facts.State);
        return facts;
    }
}
