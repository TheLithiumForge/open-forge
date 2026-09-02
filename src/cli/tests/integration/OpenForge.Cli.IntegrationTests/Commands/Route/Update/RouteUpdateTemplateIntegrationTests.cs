using System.Text;
using OpenForge.Cli.Core.Commands.Route.Create.Models.Planning;
using OpenForge.Cli.Core.Commands.Route.Create.Models.Request;
using OpenForge.Cli.Core.Commands.Route.Create.Models.Result;
using OpenForge.Cli.Core.Commands.Route.Create.Shared.Planning;
using OpenForge.Cli.Core.Commands.Route.Shared.Templates;
using OpenForge.Cli.Core.Commands.Route.Shared.Templates.Models;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;

namespace OpenForge.Cli.IntegrationTests.Commands.Route.Update;

public sealed class RouteUpdateTemplateIntegrationTests
{
    [Theory(DisplayName = "Route Update Template resolution accepts exact ID and path and copies only body bytes")]
    [InlineData(RouteUpdateIntegrationWorkspace.TemplateId)]
    [InlineData(RouteUpdateIntegrationWorkspace.TemplatePath)]
    [Trait("Feature", "route-update"), Trait("Evidence", "IntegrationBehavior")]
    public async Task ExactClassifiedBodyIsResolvedWithoutFrontmatter(string reference)
    {
        using var workspace = RouteUpdateIntegrationWorkspace.Create(
            "route-update-template-exact");
        workspace.SeedTemplate();

        var resolution = await workspace.ResolveTemplateAsync(reference);

        Assert.Equal(RouteTemplateResolutionState.Resolved, resolution.State);
        Assert.Equal(RouteUpdateIntegrationWorkspace.TemplateId, resolution.Template?.Id);
        Assert.Equal(RouteUpdateIntegrationWorkspace.TemplatePath, resolution.Template?.Path);
        Assert.Equal(
            RouteUpdateIntegrationWorkspace.TemplateBody,
            Encoding.UTF8.GetString(resolution.BodyBytes.AsSpan()));
    }

    [Fact(DisplayName = "Route Update Template resolution preserves the Route Create shared selection and body facts"), Trait("Feature", "route-update"), Trait("Evidence", "IntegrationBehavior")]
    public async Task SharedTemplateFactsMatchRouteCreate()
    {
        using var workspace = RouteUpdateIntegrationWorkspace.Create(
            "route-update-template-create-equivalence");
        workspace.SeedTemplate();
        var catalogue = await workspace.ReadCatalogueAsync();
        var update = await workspace.ResolveTemplateAsync(
            RouteUpdateIntegrationWorkspace.TemplateId);
        var create = await new RouteCreateTemplateResolver().ResolveAsync(
            new RouteCreateRequest(
                workspace.Workspace,
                "memory/project-alpha/new-route",
                new RouteCreateMetadataInput(
                    "New route",
                    ["Memory"],
                    responsibility: null),
                RouteUpdateIntegrationWorkspace.TemplateId,
                RouteCreateMode.DryRun),
            catalogue,
            TestContext.Current.CancellationToken);

        Assert.NotNull(update.Source);
        Assert.NotNull(create.Source);
        var updateTemplate = Assert.IsType<RouteTemplateSelection>(update.Template);
        var createTemplate = Assert.IsType<RouteCreateTemplate>(create.Template);
        Assert.Equal(RouteTemplateResolutionState.Resolved, update.State);
        Assert.Equal(RouteCreateTemplateResolutionState.Resolved, create.State);
        Assert.Equal(createTemplate.Requested, updateTemplate.Requested);
        Assert.Equal(createTemplate.Id, updateTemplate.Id);
        Assert.Equal(createTemplate.Path, updateTemplate.Path);
        Assert.Equal(createTemplate.BodyByteLength, updateTemplate.BodyByteLength);
        Assert.Equal(update.BodyBytes, create.BodyBytes);
        Assert.Equal(
            update.Source!.Identity.AutomaticId,
            create.Source!.Identity.AutomaticId);
        Assert.Equal(
            update.Source.Identity.CanonicalBasePath,
            create.Source.Identity.CanonicalBasePath);
    }

    [Theory(DisplayName = "Route Create and Update reject unknown Template IDs and missing exact paths")]
    [InlineData("templates/missing")]
    [InlineData(".agents/templates/missing.md")]
    [Trait("Feature", "route-update"), Trait("Evidence", "IntegrationBehavior")]
    public async Task MissingTemplateReferencesAreInvalidForCreateAndUpdate(string reference)
    {
        using var workspace = RouteUpdateIntegrationWorkspace.Create(
            "route-update-template-missing");
        var catalogue = await workspace.ReadCatalogueAsync();

        var update = await ResolveUpdateAsync(workspace, catalogue, reference);
        var create = await new RouteCreateTemplateResolver().ResolveAsync(
            CreateRequest(workspace, reference),
            catalogue,
            TestContext.Current.CancellationToken);

        Assert.Equal(RouteTemplateResolutionState.Invalid, update.State);
        Assert.Equal(RouteTemplateResolutionIssue.InvalidReference, update.Issue);
        Assert.Equal(RouteCreateTemplateResolutionState.Invalid, create.State);
        Assert.Equal(RouteCreateFindingCode.InvalidTemplate, create.Finding?.Code);
        Assert.Null(update.Template);
        Assert.Null(create.Template);
        Assert.Empty(update.BodyBytes);
        Assert.Empty(create.BodyBytes);
    }

    [Theory(DisplayName = "Route Create and Update keep post-catalogue missing or unreadable Templates unavailable")]
    [InlineData(false)]
    [InlineData(true)]
    [Trait("Feature", "route-update"), Trait("Evidence", "IntegrationBehavior")]
    public async Task PostCatalogueTemplateReadFailureRemainsIncomplete(bool unreadable)
    {
        using var workspace = RouteUpdateIntegrationWorkspace.Create(
            "route-update-template-stale-catalogue");
        workspace.SeedTemplate();
        var catalogue = await workspace.ReadCatalogueAsync();
        if (unreadable)
        {
            workspace.SeedUnreadableTemplate();
        }
        else
        {
            File.Delete(workspace.Absolute(RouteUpdateIntegrationWorkspace.TemplatePath));
        }

        var update = await ResolveUpdateAsync(
            workspace,
            catalogue,
            RouteUpdateIntegrationWorkspace.TemplateId);
        var create = await new RouteCreateTemplateResolver().ResolveAsync(
            CreateRequest(workspace, RouteUpdateIntegrationWorkspace.TemplateId),
            catalogue,
            TestContext.Current.CancellationToken);

        Assert.Equal(RouteTemplateResolutionState.Incomplete, update.State);
        Assert.Equal(RouteTemplateResolutionIssue.SourceUnavailable, update.Issue);
        Assert.Equal(RouteCreateTemplateResolutionState.Incomplete, create.State);
        Assert.Equal(RouteCreateFindingCode.TemplateUnavailable, create.Finding?.Code);
        Assert.Null(update.Template);
        Assert.Null(create.Template);
        Assert.Empty(update.BodyBytes);
        Assert.Empty(create.BodyBytes);
    }

    [Theory(DisplayName = "Route Update Template resolution rejects collisions and non-Template classification")]
    [InlineData(true, false, false, (int)RouteTemplateResolutionState.Blocked, (int)RouteTemplateResolutionIssue.OverwriteUnsafe)]
    [InlineData(false, true, false, (int)RouteTemplateResolutionState.Invalid, (int)RouteTemplateResolutionIssue.MissingClassification)]
    [InlineData(false, false, true, (int)RouteTemplateResolutionState.Blocked, (int)RouteTemplateResolutionIssue.SourceUnsafe)]
    [Trait("Feature", "route-update"), Trait("Evidence", "IntegrationSafety")]
    public async Task UnsafeTemplateInputsStopBeforeBodyUse(
        bool seedOverwrite,
        bool omitClassification,
        bool seedCollision,
        int expectedStateValue,
        int expectedIssueValue)
    {
        var expectedState = (RouteTemplateResolutionState)expectedStateValue;
        var expectedIssue = (RouteTemplateResolutionIssue)expectedIssueValue;
        using var workspace = RouteUpdateIntegrationWorkspace.Create(
            "route-update-template-unsafe");
        workspace.SeedTemplate(tags: omitClassification ? ["Example"] : null);
        if (seedOverwrite)
        {
            workspace.SeedTemplateOverwrite();
        }

        if (seedCollision)
        {
            workspace.SeedTemplateCollision();
        }

        var resolution = await workspace.ResolveTemplateAsync(
            RouteUpdateIntegrationWorkspace.TemplateId);

        Assert.Equal(expectedState, resolution.State);
        Assert.Equal(expectedIssue, resolution.Issue);
        Assert.Null(resolution.Template);
        Assert.Empty(resolution.BodyBytes);
    }

    private static RouteCreateRequest CreateRequest(
        RouteUpdateIntegrationWorkspace workspace,
        string reference)
        => new(
            workspace.Workspace,
            "memory/project-alpha/new-route",
            new RouteCreateMetadataInput(
                "New route",
                ["Memory"],
                responsibility: null),
            reference,
            RouteCreateMode.DryRun);

    private static ValueTask<RouteTemplateResolution> ResolveUpdateAsync(
        RouteUpdateIntegrationWorkspace workspace,
        SourceCatalogue catalogue,
        string reference)
        => new RouteTemplateResolver().ResolveAsync(
            new RouteTemplateResolutionRequest
            {
                Workspace = workspace.Workspace,
                Reference = reference,
                Catalogue = catalogue,
            },
            TestContext.Current.CancellationToken);
}
