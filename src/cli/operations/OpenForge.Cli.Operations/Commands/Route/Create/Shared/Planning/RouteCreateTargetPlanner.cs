using OpenForge.Cli.Core.Commands.Route.Create.Models.Request;
using OpenForge.Cli.Core.Commands.Route.Create.Models.Planning;
using OpenForge.Cli.Core.Commands.Route.Create.Models.Result;
using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;
using OpenForge.Cli.Core.Framework.Workspace.Models.Layout;
using OpenForge.Cli.Core.Framework.Filesystem.Shared.Paths;

namespace OpenForge.Cli.Core.Commands.Route.Create.Shared.Planning;

internal sealed class RouteCreateTargetPlanner
{
    internal RouteCreateTargetResolution Resolve(RouteCreateRequest request, WorkspaceLayout layout)
    {
        var parsed = SourceReferenceParser.Parse(request.FileTarget);
        if (parsed.State != SourceReferenceParseState.Valid)
        {
            return Invalid(request.FileTarget, parsed.Cause ?? "The Route Create target is invalid.");
        }

        if (parsed.Kind == SourceReferenceKind.SourceId
            && !PortableWorkspacePath.TryNormalize(parsed.AttemptedId + SourceFormClassifier.MarkdownExtension, out _))
        {
            return Invalid(request.FileTarget, "Route Create requires one ordinary Markdown file target.");
        }

        var path = parsed.Kind switch
        {
            SourceReferenceKind.SourceId when parsed.AttemptedId is { } id =>
                layout.SourcePath(id + SourceFormClassifier.MarkdownExtension),
            SourceReferenceKind.SourcePath when parsed.AttemptedPath is { } exactPath => exactPath,
            _ => throw new ArgumentOutOfRangeException(
                nameof(parsed),
                parsed.Kind,
                "The Route Create target reference kind is not defined."),
        };
        var outsideDestination = parsed.Kind == SourceReferenceKind.SourcePath
            && layout.TryGetMaintenanceBoundary(path, out var boundary)
            && boundary.Kind == WorkspaceMaintenanceBoundaryKind.Outside;
        if ((!layout.IsImplicitContentDestination(path) && !outsideDestination)
            || !SourceFormClassifier.TryClassify(layout, path, out var form)
            || form != SourceDocumentForm.Markdown
            || SourceIdentity.DeriveId(layout, path) is not { } targetId)
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
