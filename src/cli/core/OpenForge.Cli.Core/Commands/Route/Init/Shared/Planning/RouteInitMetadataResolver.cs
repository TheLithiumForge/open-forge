using OpenForge.Cli.Core.Commands.Route.Init.Models.Request;
using OpenForge.Cli.Core.Commands.Route.Init.Models.Result;

namespace OpenForge.Cli.Core.Commands.Route.Init.Shared.Planning;

internal sealed class RouteInitMetadataResolver
{
    private const string NeedsAuthoring = "NeedsAuthoring";

    internal RouteInitMetadata Resolve(
        string id,
        bool isFinalTarget,
        RouteInitMetadataInput input)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);
        ArgumentNullException.ThrowIfNull(input);
        var draft = $"Draft route for {id}; replace this description before relying on it for selection";
        if (!isFinalTarget)
        {
            return Draft(draft);
        }

        var hasDescription = input.Description is not null;
        var hasTags = input.Tags.Count > 0;
        var tags = input.Tags.ToList();
        if ((!hasDescription || !hasTags)
            && !tags.Contains(NeedsAuthoring, StringComparer.Ordinal))
        {
            tags.Add(NeedsAuthoring);
        }

        var responsibility = input.ResponsibilitySpecified && input.Responsibility?.Length > 0
            ? input.Responsibility
            : null;
        return new RouteInitMetadata(
            input.Description ?? draft,
            hasDescription
                ? RouteInitDescriptionSource.Explicit
                : RouteInitDescriptionSource.Draft,
            responsibility,
            input.ResponsibilitySpecified
                ? responsibility is null
                    ? RouteInitResponsibilitySource.ExplicitOmitted
                    : RouteInitResponsibilitySource.Explicit
                : RouteInitResponsibilitySource.DefaultOmitted,
            tags,
            !hasTags
                ? RouteInitTagsSource.Draft
                : !hasDescription
                    ? RouteInitTagsSource.Mixed
                    : RouteInitTagsSource.Explicit);
    }

    private static RouteInitMetadata Draft(string description)
        => new(
            description,
            RouteInitDescriptionSource.Draft,
            responsibility: null,
            RouteInitResponsibilitySource.DefaultOmitted,
            [NeedsAuthoring],
            RouteInitTagsSource.Draft);
}
