using System.Collections.Immutable;

namespace OpenForge.Cli.Core.Commands.Route.Init.Models.Result;

internal sealed record RouteInitResultEntrypoint
{
    internal RouteInitResultEntrypoint(RouteInitEntrypoint source)
    {
        ArgumentNullException.ThrowIfNull(source);
        Id = source.Id;
        Path = source.Path;
        Form = source.Form;
        Current = source.Current;
        Ownership = source.Ownership;
        Metadata = source.Metadata is { } metadata
            ? new RouteInitResultMetadata(metadata)
            : null;
        SourceAssetPath = source.SourceAssetPath;
        Outcome = source.Outcome;
    }

    internal string Id { get; }

    internal string Path { get; }

    internal RouteInitEntrypointForm Form { get; }

    internal RouteInitEntrypointCurrent Current { get; }

    internal RouteInitEntrypointOwnership Ownership { get; }

    internal RouteInitResultMetadata? Metadata { get; }

    internal string? SourceAssetPath { get; }

    internal RouteInitEntrypointOutcome Outcome { get; }
}

internal sealed record RouteInitResultMetadata
{
    internal RouteInitResultMetadata(RouteInitMetadata source)
    {
        ArgumentNullException.ThrowIfNull(source);
        Description = source.Description;
        DescriptionSource = source.DescriptionSource;
        Responsibility = source.Responsibility;
        ResponsibilitySource = source.ResponsibilitySource;
        Tags = source.Tags;
        TagsSource = source.TagsSource;
        ApplyTo = source.ApplyTo
            .Select(pattern => pattern.Text)
            .ToImmutableArray();
    }

    internal string Description { get; }

    internal RouteInitDescriptionSource DescriptionSource { get; }

    internal string? Responsibility { get; }

    internal RouteInitResponsibilitySource ResponsibilitySource { get; }

    internal ImmutableArray<string> Tags { get; }

    internal RouteInitTagsSource TagsSource { get; }

    internal ImmutableArray<string> ApplyTo { get; }
}
