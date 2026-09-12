using System.Collections.Immutable;
using OpenForge.Cli.Core.Framework.Documents.Metadata;
using OpenForge.Cli.Core.Framework.Workspace.Models;

namespace OpenForge.Cli.Core.Commands.Route.Create.Models.Request;

internal enum RouteCreateMode
{
    Apply,
    DryRun,
}

internal sealed record RouteCreateMetadataInput
{
    internal RouteCreateMetadataInput(
        string description,
        IEnumerable<string> tags,
        string? responsibility)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(description);
        if (responsibility is not null && responsibility.Length > 0 && string.IsNullOrWhiteSpace(responsibility))
        {
            throw new ArgumentException(
                "A whitespace-only Route Create responsibility is not valid.",
                nameof(responsibility));
        }

        var tagValues = tags
            .Select(tag => tag ?? throw new ArgumentException(
                "Route Create tags cannot contain null members.",
                nameof(tags)))
            .ToImmutableArray();
        if (tagValues.IsEmpty
            || tagValues.Any(tag => !FrameworkDocumentMetadataTagGrammar.IsValid(tag))
            || tagValues.Distinct(StringComparer.Ordinal).Count() != tagValues.Length)
        {
            throw new ArgumentException(
                "Route Create tags must be non-empty, canonical, and unique.",
                nameof(tags));
        }

        Description = description;
        Tags = tagValues;
        Responsibility = responsibility is { Length: 0 } ? null : responsibility;
    }

    internal string Description { get; }

    internal ImmutableArray<string> Tags { get; }

    internal string? Responsibility { get; }
}

internal sealed record RouteCreateRequest
{
    internal RouteCreateRequest(
        CliWorkspace workspace,
        string fileTarget,
        RouteCreateMetadataInput metadata,
        string? templateReference,
        RouteCreateMode mode)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(fileTarget);
        if (!Enum.IsDefined(mode))
        {
            throw new ArgumentOutOfRangeException(
                nameof(mode),
                mode,
                "The Route Create mode is not defined.");
        }

        if (templateReference is not null && string.IsNullOrWhiteSpace(templateReference))
        {
            throw new ArgumentException(
                "A Route Create Template reference cannot be blank.",
                nameof(templateReference));
        }

        Workspace = workspace;
        FileTarget = fileTarget;
        Metadata = metadata;
        TemplateReference = templateReference;
        Mode = mode;
    }

    internal CliWorkspace Workspace { get; }

    internal string FileTarget { get; }

    internal RouteCreateMetadataInput Metadata { get; }

    internal string? TemplateReference { get; }

    internal RouteCreateMode Mode { get; }

    internal bool IsDryRun => Mode == RouteCreateMode.DryRun;
}
