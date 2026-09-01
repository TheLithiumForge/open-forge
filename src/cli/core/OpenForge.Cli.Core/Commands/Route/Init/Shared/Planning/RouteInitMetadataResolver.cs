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

        string? responsibility = null;
        if (input.ResponsibilitySpecified && input.Responsibility?.Length > 0)
        {
            responsibility = input.Responsibility;
        }

        var responsibilitySource = RouteInitResponsibilitySource.DefaultOmitted;
        if (input.ResponsibilitySpecified)
        {
            responsibilitySource = responsibility is null
                ? RouteInitResponsibilitySource.ExplicitOmitted
                : RouteInitResponsibilitySource.Explicit;
        }

        var tagsSource = RouteInitTagsSource.Explicit;
        if (!hasTags)
        {
            tagsSource = RouteInitTagsSource.Draft;
        }
        else if (!hasDescription)
        {
            tagsSource = RouteInitTagsSource.Mixed;
        }

        return new RouteInitMetadata(
            input.Description ?? draft,
            hasDescription
                ? RouteInitDescriptionSource.Explicit
                : RouteInitDescriptionSource.Draft,
            responsibility,
            responsibilitySource,
            tags,
            tagsSource);
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
