using System.Text;
using OpenForge.Cli.Core.Commands.Route.Shared.Templates.Models;
using OpenForge.Cli.Core.Commands.Route.Update.Models.Planning;
using OpenForge.Cli.Core.Commands.Route.Update.Models.Result;
using OpenForge.Cli.Core.Commands.Route.Update.Shared.Planning;

namespace OpenForge.Cli.Core.UnitTests.Commands.Route.Update;

public sealed class RouteUpdateBodyPlannerTests
{
    [Fact(DisplayName = "Route Update body planner copies exact Template bytes into Unicode-whitespace body"), Trait("Feature", "route-update"), Trait("Evidence", "UnitBehavior")]
    public void CopiesExactTemplateBytesIntoUnicodeWhitespaceBody()
    {
        const string source = "---\nopen-forge:\n  description: Before\n  tags: [Memory]\n---\n \t\u2003\r\n";
        const string templateBody = "# Template body\r\n\r\nExact.\n";
        const string expected = "---\nopen-forge:\n  description: Before\n  tags: [Memory]\n---\n# Template body\r\n\r\nExact.\n";
        var observation = RouteUpdateTestData.Observation(
            text: source,
            templateReference: RouteUpdateTestData.TemplateId);
        var metadata = RouteUpdateTestData.MetadataPlan(observation, source);

        var build = Build(
            metadata,
            RouteUpdateTestData.ResolvedTemplate(
                observation.Request.Workspace,
                templateBody));
        var body = Assert.IsType<RouteUpdateBodyPlan>(build.Body);

        Assert.Null(build.Boundary);
        Assert.Equal(RouteUpdateBodyState.TemplateCopied, body.State);
        Assert.Equal(RouteUpdateTemplateDecision.Copied, body.Template?.Decision);
        Assert.Equal(RouteUpdateTestData.TemplateId, body.Template?.Requested);
        Assert.Equal(RouteUpdateTestData.TemplateId, body.Template?.Id);
        Assert.Equal(RouteUpdateTestData.TemplatePath, body.Template?.Path);
        Assert.Equal(RouteUpdateTemplateClassification.Template, body.Template?.Classification);
        Assert.Equal(Encoding.UTF8.GetByteCount(templateBody), body.Template?.BodyByteLength);
        Assert.Equal(expected, Encoding.UTF8.GetString(body.IntendedTargetBytes.AsSpan()));
        var preview = Assert.Single(body.Preview);
        Assert.Equal(RouteUpdatePreviewKind.TemplateBody, preview.Kind);
        Assert.Equal(templateBody, preview.Expected);
    }

    [Fact(DisplayName = "Route Update body planner protects every authored body byte"), Trait("Feature", "route-update"), Trait("Evidence", "UnitBehavior")]
    public void ProtectsEveryAuthoredBodyByte()
    {
        var observation = RouteUpdateTestData.Observation(
            templateReference: RouteUpdateTestData.TemplateId);
        var metadata = RouteUpdateTestData.MetadataPlan(
            observation,
            RouteUpdateTestData.TargetText);

        var build = Build(
            metadata,
            RouteUpdateTestData.ResolvedTemplate(observation.Request.Workspace));
        var body = Assert.IsType<RouteUpdateBodyPlan>(build.Body);

        Assert.Null(build.Boundary);
        Assert.Equal(RouteUpdateBodyState.AuthoredBodyProtected, body.State);
        Assert.Equal(
            RouteUpdateTemplateDecision.AuthoredBodyProtected,
            body.Template?.Decision);
        Assert.Equal(
            RouteUpdateTestData.TargetText,
            Encoding.UTF8.GetString(body.IntendedTargetBytes.AsSpan()));
        Assert.Empty(body.Preview);
    }

    [Fact(DisplayName = "Route Update body planner preserves exact body when Template is omitted"), Trait("Feature", "route-update"), Trait("Evidence", "UnitBehavior")]
    public void PreservesExactBodyWhenTemplateIsOmitted()
    {
        const string source = "---\r\nopen-forge:\r\n  description: Before\r\n  tags: [Memory]\r\n---\r\n\r\nAuthored\tbytes\r\n";
        var observation = RouteUpdateTestData.Observation(text: source);
        var metadata = RouteUpdateTestData.MetadataPlan(observation, source);

        var build = Build(metadata, template: null);
        var body = Assert.IsType<RouteUpdateBodyPlan>(build.Body);

        Assert.Null(build.Boundary);
        Assert.Null(body.Template);
        Assert.Equal(RouteUpdateBodyState.Preserved, body.State);
        Assert.Equal(source, Encoding.UTF8.GetString(body.IntendedTargetBytes.AsSpan()));
        Assert.Empty(body.Preview);
    }

    [Theory(DisplayName = "Route Update adapts every Template stop class to accepted planning facts"), Trait("Feature", "route-update"), Trait("Evidence", "UnitBehavior")]
    [InlineData(
        (int)RouteTemplateResolutionState.Invalid,
        (int)RouteTemplateResolutionIssue.InvalidReference,
        (int)RouteUpdateFindingCode.InvalidTemplate,
        (int)RouteUpdatePlanCompleteness.NotEstablished,
        (int)RouteUpdatePlanSafety.NotEstablished)]
    [InlineData(
        (int)RouteTemplateResolutionState.Blocked,
        (int)RouteTemplateResolutionIssue.SourceUnsafe,
        (int)RouteUpdateFindingCode.TemplateUnsafe,
        (int)RouteUpdatePlanCompleteness.NotEstablished,
        (int)RouteUpdatePlanSafety.Blocked)]
    [InlineData(
        (int)RouteTemplateResolutionState.Incomplete,
        (int)RouteTemplateResolutionIssue.SourceUnavailable,
        (int)RouteUpdateFindingCode.TemplateUnavailable,
        (int)RouteUpdatePlanCompleteness.Incomplete,
        (int)RouteUpdatePlanSafety.NotEstablished)]
    [InlineData(
        (int)RouteTemplateResolutionState.Incomplete,
        (int)RouteTemplateResolutionIssue.ResolutionInterrupted,
        (int)RouteUpdateFindingCode.Interrupted,
        (int)RouteUpdatePlanCompleteness.Incomplete,
        (int)RouteUpdatePlanSafety.NotEstablished)]
    [InlineData(
        (int)RouteTemplateResolutionState.Incomplete,
        (int)RouteTemplateResolutionIssue.ReadInterrupted,
        (int)RouteUpdateFindingCode.Interrupted,
        (int)RouteUpdatePlanCompleteness.Incomplete,
        (int)RouteUpdatePlanSafety.NotEstablished)]
    public void AdaptsTemplateStopClass(
        int stateValue,
        int issueValue,
        int findingValue,
        int completenessValue,
        int safetyValue)
    {
        var metadata = RouteUpdateTestData.MetadataPlan(
            RouteUpdateTestData.Observation(
                templateReference: RouteUpdateTestData.TemplateId));
        var resolution = new RouteTemplateResolution
        {
            State = (RouteTemplateResolutionState)stateValue,
            Template = null,
            Source = null,
            BodyBytes = [],
            Issue = (RouteTemplateResolutionIssue)issueValue,
            Cause = "Independent Template stop.",
            Target = RouteUpdateTestData.TemplateId,
        };

        var build = Build(metadata, resolution);
        var boundary = Assert.IsType<RouteUpdatePlanningBoundary>(build.Boundary);

        Assert.Null(build.Body);
        Assert.Equal(
            (RouteUpdatePlanCompleteness)completenessValue,
            boundary.Formation.Plan.Completeness);
        Assert.Equal(
            (RouteUpdatePlanSafety)safetyValue,
            boundary.Formation.Plan.Safety);
        Assert.Equal(
            (RouteUpdateFindingCode)findingValue,
            Assert.Single(boundary.Formation.Findings).Code);
    }

    [Fact(DisplayName = "Route Update Template adapter rejects resolved and undefined stop values"), Trait("Feature", "route-update"), Trait("Evidence", "UnitContract")]
    public void TemplateAdapterRejectsResolvedAndUndefinedStopValues()
    {
        Assert.Throws<InvalidOperationException>(() =>
            RouteUpdateTemplateFindingAdapter.Read(
                RouteTemplateResolutionState.Resolved,
                issue: null));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            RouteUpdateTemplateFindingAdapter.Read(
                (RouteTemplateResolutionState)int.MaxValue,
                issue: null));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            RouteUpdateTemplateFindingAdapter.Read(
                RouteTemplateResolutionState.Incomplete,
                (RouteTemplateResolutionIssue)int.MaxValue));
    }

    private static RouteUpdateBodyPlanBuild Build(
        RouteUpdateMetadataPatch metadata,
        RouteTemplateResolution? template)
        => new RouteUpdateBodyPlanner().Build(
            new RouteUpdateBodyPlanInput
            {
                Metadata = metadata,
                Template = template,
            });
}
