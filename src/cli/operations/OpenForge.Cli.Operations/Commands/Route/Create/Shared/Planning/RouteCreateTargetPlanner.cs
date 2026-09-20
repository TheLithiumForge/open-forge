using OpenForge.Cli.Core.Commands.Route.Create.Models.Request;
using OpenForge.Cli.Core.Commands.Route.Create.Models.Planning;
using OpenForge.Cli.Core.Commands.Route.Create.Models.Result;
using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;

namespace OpenForge.Cli.Core.Commands.Route.Create.Shared.Planning;

internal sealed class RouteCreateTargetPlanner
{
    internal RouteCreateTargetResolution Resolve(RouteCreateRequest request)
    {
        var parsed = SourceReferenceParser.Parse(request.FileTarget);
        if (parsed.State != SourceReferenceParseState.Valid)
        {
            return Invalid(request.FileTarget, parsed.Cause ?? "The Route Create target is invalid.");
        }

        var path = parsed.Kind switch
        {
            SourceReferenceKind.SourceId when parsed.AttemptedId is { } id =>
                $"{SourceLogicalPath.AgentsRoot}/{id}.md",
            SourceReferenceKind.SourcePath when parsed.AttemptedPath is { } exactPath => exactPath,
            _ => throw new ArgumentOutOfRangeException(
                nameof(parsed),
                parsed.Kind,
                "The Route Create target reference kind is not defined."),
        };
        if (!SourceFormClassifier.TryClassify(path, out var form)
            || form != SourceDocumentForm.Markdown
            || SourceIdentity.DeriveId(path) is not { } targetId)
        {
            return Invalid(
                request.FileTarget,
                "Route Create requires one ordinary Markdown file target.");
        }

        var identity = new SourceLogicalIdentity(targetId, path);
        return new RouteCreateTargetResolution
        {
            State = RouteCreateTargetResolutionState.Resolved,
            Target = new RouteCreateTarget
            {
                Requested = request.FileTarget,
                Id = targetId,
                Path = path,
            },
            Identity = identity,
            Cause = null,
        };
    }

    private static RouteCreateTargetResolution Invalid(
        string requested,
        string cause)
        => new()
        {
            State = RouteCreateTargetResolutionState.Invalid,
            Target = new RouteCreateTarget
            {
                Requested = requested,
                Id = null,
                Path = null,
            },
            Identity = null,
            Cause = cause,
        };
}
