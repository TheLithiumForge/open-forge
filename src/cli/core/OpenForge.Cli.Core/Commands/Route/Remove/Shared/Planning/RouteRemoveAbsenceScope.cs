using OpenForge.Cli.Core.Commands.Route.Remove.Models.Planning;
using OpenForge.Cli.Core.Commands.Route.Remove.Models.Request;
using OpenForge.Cli.Core.Commands.Route.Remove.Models.Result;
using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.Identity;

namespace OpenForge.Cli.Core.Commands.Route.Remove.Shared.Planning;

internal sealed record RouteRemoveAbsenceScope
{
    public required RouteRemoveRequest Request { get; init; }

    public required string Id { get; init; }

    public required string CategoryRoot { get; init; }

    public required string LeafPath { get; init; }

    public required string LeafOverwritePath { get; init; }

    public required string IntendedPath { get; init; }

    public required RouteRemoveSource Source { get; init; }

    public required RouteRemoveSubject Subject { get; init; }

    internal bool Contains(string canonicalPath)
        => string.Equals(canonicalPath, LeafPath, StringComparison.Ordinal)
            || string.Equals(canonicalPath, LeafOverwritePath, StringComparison.Ordinal)
            || string.Equals(canonicalPath, CategoryRoot, StringComparison.Ordinal)
            || canonicalPath.StartsWith($"{CategoryRoot}/", StringComparison.Ordinal);

    internal static RouteRemoveAbsenceScope? TryCreate(RouteRemoveRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        var parsed = SourceReferenceParser.Parse(request.SourceReference);
        if (parsed.State != SourceReferenceParseState.Valid)
        {
            return null;
        }

        return parsed.Kind switch
        {
            SourceReferenceKind.SourceId => FromId(
                request,
                parsed.AttemptedId
                    ?? throw new InvalidOperationException(
                        "A valid Route Remove source ID requires its attempted identity.")),
            SourceReferenceKind.SourcePath => FromPath(
                request,
                parsed.AttemptedPath
                    ?? throw new InvalidOperationException(
                        "A valid Route Remove source path requires its attempted identity.")),
            _ => throw new ArgumentOutOfRangeException(
                nameof(request),
                parsed.Kind,
                "The Route Remove source-reference kind is not defined."),
        };
    }

    internal static RouteRemoveAbsenceScope FromPlan(RouteRemovePlan plan)
    {
        ArgumentNullException.ThrowIfNull(plan);
        var source = plan.Preview.Source;
        var id = source.Id
            ?? throw new InvalidOperationException(
                "An accepted Route Remove plan requires its exact source identity.");
        var intendedPath = source.Path
            ?? throw new InvalidOperationException(
                "An accepted Route Remove plan requires its exact source path.");
        var subjectKind = plan.Preview.Subject.Kind
            ?? throw new InvalidOperationException(
                "An accepted Route Remove plan requires its exact subject kind.");
        var categoryRoot = $"{SourceLogicalPath.AgentsRoot}/{id}";
        return new RouteRemoveAbsenceScope
        {
            Request = plan.Request,
            Id = id,
            CategoryRoot = categoryRoot,
            LeafPath = $"{SourceLogicalPath.AgentsRoot}/{id}.md",
            LeafOverwritePath = $"{SourceLogicalPath.AgentsRoot}/{id}.overwrite.md",
            IntendedPath = intendedPath,
            Source = source,
            Subject = new RouteRemoveSubject { Kind = subjectKind },
        };
    }

    private static RouteRemoveAbsenceScope FromId(RouteRemoveRequest request, string id)
    {
        var categoryRoot = $"{SourceLogicalPath.AgentsRoot}/{id}";
        var name = id[(id.LastIndexOf('/') + 1)..];
        var intendedPath = $"{categoryRoot}/_{name}.md";
        return new RouteRemoveAbsenceScope
        {
            Request = request,
            Id = id,
            CategoryRoot = categoryRoot,
            LeafPath = $"{SourceLogicalPath.AgentsRoot}/{id}.md",
            LeafOverwritePath = $"{SourceLogicalPath.AgentsRoot}/{id}.overwrite.md",
            IntendedPath = intendedPath,
            Source = new RouteRemoveSource
            {
                Requested = request.SourceReference,
                SelectedBy = RouteRemoveSourceSelection.SourceId,
                Id = id,
                Path = intendedPath,
                Form = RouteRemoveSourceForm.CanonicalEntrypoint,
            },
            Subject = new RouteRemoveSubject { Kind = RouteRemoveSubjectKind.Category },
        };
    }

    private static RouteRemoveAbsenceScope? FromPath(RouteRemoveRequest request, string path)
    {
        if (!SourceFormClassifier.TryClassify(path, out var form)
            || !SourceFormClassifier.IsEntrypoint(form))
        {
            return null;
        }

        var id = SourceIdentity.DeriveId(path);
        if (id is null)
        {
            return null;
        }

        var categoryRoot = SourceLogicalPath.ReadParent(path);
        return new RouteRemoveAbsenceScope
        {
            Request = request,
            Id = id,
            CategoryRoot = categoryRoot,
            LeafPath = $"{SourceLogicalPath.AgentsRoot}/{id}.md",
            LeafOverwritePath = $"{SourceLogicalPath.AgentsRoot}/{id}.overwrite.md",
            IntendedPath = path,
            Source = new RouteRemoveSource
            {
                Requested = request.SourceReference,
                SelectedBy = RouteRemoveSourceSelection.BasePath,
                Id = id,
                Path = path,
                Form = form == SourceDocumentForm.CanonicalEntrypoint
                    ? RouteRemoveSourceForm.CanonicalEntrypoint
                    : RouteRemoveSourceForm.CompatibilityEntrypoint,
            },
            Subject = new RouteRemoveSubject { Kind = RouteRemoveSubjectKind.Category },
        };
    }
}
