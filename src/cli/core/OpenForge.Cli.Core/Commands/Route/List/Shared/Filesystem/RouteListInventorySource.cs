using System.Collections.ObjectModel;
using OpenForge.Cli.Core.Commands.Route.List.Shared.Selection;

namespace OpenForge.Cli.Core.Commands.Route.List.Shared.Filesystem;

internal enum RouteListSourceForm
{
    Loader,
    CanonicalEntrypoint,
    IndexEntrypoint,
    UnderscoreIndexEntrypoint,
    ReferencesEntrypoint,
    UnderscoreReferencesEntrypoint,
    Skill,
    Markdown,
    OverwriteCompanion,
}

internal static class RouteListSourceFormFacts
{
    internal static bool IsEntrypoint(RouteListSourceForm form)
    {
        return form is RouteListSourceForm.CanonicalEntrypoint
            or RouteListSourceForm.IndexEntrypoint
            or RouteListSourceForm.UnderscoreIndexEntrypoint
            or RouteListSourceForm.ReferencesEntrypoint
            or RouteListSourceForm.UnderscoreReferencesEntrypoint;
    }

    internal static bool IsCompatibilityEntrypoint(RouteListSourceForm form)
    {
        return form is RouteListSourceForm.IndexEntrypoint
            or RouteListSourceForm.UnderscoreIndexEntrypoint
            or RouteListSourceForm.ReferencesEntrypoint
            or RouteListSourceForm.UnderscoreReferencesEntrypoint;
    }
}

internal enum RouteListMetadataState
{
    NotApplicable,
    Complete,
    Missing,
    Malformed,
    ReadUnavailable,
}

internal sealed class RouteListSourceMetadata
{
    private RouteListSourceMetadata(
        RouteListMetadataState state,
        string? description,
        IEnumerable<string> tags,
        bool isCompatibilityEntrypoint,
        bool isOverwritePresent)
    {
        if (!Enum.IsDefined(state))
        {
            throw new ArgumentOutOfRangeException(nameof(state), state, "The route-list metadata state is not defined.");
        }

        ArgumentNullException.ThrowIfNull(tags);
        var materializedTags = tags.ToArray();
        if (materializedTags.Any(tag => tag is null))
        {
            throw new ArgumentException("Route-list metadata tags cannot contain null.", nameof(tags));
        }

        if (state == RouteListMetadataState.Complete)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(description);
            if (materializedTags.Any(tag => !RouteListMetadataParser.IsValidTag(tag)))
            {
                throw new ArgumentException("Route-list metadata contains an invalid tag.", nameof(tags));
            }
        }
        else if (description is not null || materializedTags.Length > 0)
        {
            throw new ArgumentException("Only complete metadata can carry a description or tags.");
        }

        State = state;
        Description = description;
        Tags = new ReadOnlyCollection<string>(materializedTags);
        IsCompatibilityEntrypoint = isCompatibilityEntrypoint;
        IsOverwritePresent = isOverwritePresent;
    }

    internal RouteListMetadataState State { get; }

    internal string? Description { get; }

    internal IReadOnlyList<string> Tags { get; }

    internal bool IsCompatibilityEntrypoint { get; }

    internal bool IsOverwritePresent { get; }

    internal static RouteListSourceMetadata Complete(
        string description,
        IEnumerable<string> tags,
        bool isCompatibilityEntrypoint,
        bool isOverwritePresent)
    {
        return new RouteListSourceMetadata(
            RouteListMetadataState.Complete,
            description,
            tags,
            isCompatibilityEntrypoint,
            isOverwritePresent);
    }

    internal static RouteListSourceMetadata WithoutValues(
        RouteListMetadataState state,
        bool isCompatibilityEntrypoint,
        bool isOverwritePresent)
    {
        if (state == RouteListMetadataState.Complete)
        {
            throw new ArgumentOutOfRangeException(nameof(state), state, "Complete metadata requires authored values.");
        }

        return new RouteListSourceMetadata(
            state,
            null,
            [],
            isCompatibilityEntrypoint,
            isOverwritePresent);
    }
}

internal sealed record RouteListOverwriteRelation
{
    internal RouteListOverwriteRelation(string canonicalLogicalPath, string physicalPath)
    {
        if (!RouteListLogicalPath.IsCanonical(canonicalLogicalPath)
            || !canonicalLogicalPath.EndsWith(".overwrite.md", StringComparison.Ordinal))
        {
            throw new ArgumentException("The overwrite logical path is not canonical.", nameof(canonicalLogicalPath));
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(physicalPath);
        var normalizedPhysicalPath = Path.GetFullPath(physicalPath);
        if (!Path.IsPathRooted(physicalPath)
            || !string.Equals(normalizedPhysicalPath, physicalPath, StringComparison.Ordinal))
        {
            throw new ArgumentException("The overwrite physical path must be absolute and normalized.", nameof(physicalPath));
        }

        CanonicalLogicalPath = canonicalLogicalPath;
        PhysicalPath = normalizedPhysicalPath;
    }

    internal string CanonicalLogicalPath { get; }

    internal string PhysicalPath { get; }
}

internal sealed class RouteListInventorySource
{
    internal RouteListInventorySource(
        RouteListSource source,
        RouteListSourceForm form,
        RouteListSourceMetadata metadata,
        RouteListOverwriteRelation? overwrite)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(metadata);
        if (!Enum.IsDefined(form) || form == RouteListSourceForm.OverwriteCompanion)
        {
            throw new ArgumentOutOfRangeException(nameof(form), form, "The form is not a logical source form.");
        }

        ValidateKindAndForm(source, form);
        ValidateMetadata(source, form, metadata);
        if ((overwrite is not null) != metadata.IsOverwritePresent
            || !string.Equals(source.OverwritePath, overwrite?.CanonicalLogicalPath, StringComparison.Ordinal))
        {
            throw new ArgumentException("The source overwrite relation and metadata must agree.", nameof(overwrite));
        }

        Source = source;
        Form = form;
        Metadata = metadata;
        Overwrite = overwrite;
    }

    internal RouteListSource Source { get; }

    internal RouteListSourceForm Form { get; }

    internal RouteListSourceMetadata Metadata { get; }

    internal RouteListOverwriteRelation? Overwrite { get; }

    private static void ValidateKindAndForm(RouteListSource source, RouteListSourceForm form)
    {
        var valid = RouteListSourceFormFacts.IsEntrypoint(form)
            ? source.Kind == RouteListSourceKind.Entrypoint
            : form switch
            {
                RouteListSourceForm.Loader => source.Kind == RouteListSourceKind.Loader,
                RouteListSourceForm.Skill => source.Kind is RouteListSourceKind.RoutedNative or RouteListSourceKind.Unrouted,
                RouteListSourceForm.Markdown => source.Kind is RouteListSourceKind.RoutedLeaf or RouteListSourceKind.Unrouted,
                _ => false,
            };
        if (!valid)
        {
            throw new ArgumentException("The selection source kind does not match the inventory source form.", nameof(source));
        }
    }

    private static void ValidateMetadata(
        RouteListSource source,
        RouteListSourceForm form,
        RouteListSourceMetadata metadata)
    {
        var compatibility = RouteListSourceFormFacts.IsCompatibilityEntrypoint(form);
        if (metadata.IsCompatibilityEntrypoint != compatibility)
        {
            throw new ArgumentException("The metadata compatibility fact does not match the source form.", nameof(metadata));
        }

        if (metadata.State == RouteListMetadataState.Complete)
        {
            if (form == RouteListSourceForm.Loader
                || form == RouteListSourceForm.Skill && metadata.Tags.Count != 0
                || form != RouteListSourceForm.Skill && metadata.Tags.Count == 0)
            {
                throw new ArgumentException("The complete metadata values do not match the source form.", nameof(metadata));
            }
        }

        if (source.Kind is RouteListSourceKind.RoutedLeaf or RouteListSourceKind.RoutedNative
            && metadata.State != RouteListMetadataState.Complete)
        {
            throw new ArgumentException("A routed leaf or native source requires complete metadata.", nameof(metadata));
        }

        if (source.Kind == RouteListSourceKind.Unrouted
            && metadata.State == RouteListMetadataState.Complete)
        {
            throw new ArgumentException("A complete supported source cannot be classified as unrouted.", nameof(metadata));
        }
    }
}
