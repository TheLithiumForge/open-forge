using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;
using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.Identity;

namespace OpenForge.Cli.Core.Commands.Route.Shared.Models.Source;

internal enum RouteSourceKind
{
    Loader,
    Entrypoint,
    Markdown,
    Native,
}

internal sealed class RouteSource
{
    internal RouteSource(
        RouteSourceDocument @base,
        RouteSourceMetadata metadata,
        RouteSourceKind kind,
        RouteSourceDocument? overwrite = null,
        bool isRouteAmbiguous = false)
        : this(DefaultIdentity(@base), @base, metadata, kind, overwrite, isRouteAmbiguous) { }

    internal RouteSource(SourceLogicalIdentity identity, RouteSourceDocument @base, RouteSourceMetadata metadata,
        RouteSourceKind kind, RouteSourceDocument? overwrite = null, bool isRouteAmbiguous = false)
    {
        ArgumentNullException.ThrowIfNull(@base);
        ArgumentNullException.ThrowIfNull(metadata);
        if (!Enum.IsDefined(kind))
        {
            throw new ArgumentOutOfRangeException(nameof(kind), kind, "The source kind is not defined.");
        }

        if (@base.Form == SourceDocumentForm.OverwriteCompanion)
        {
            throw new ArgumentException("An overwrite companion cannot be a logical source base.", nameof(@base));
        }

        ValidateKindAndForm(kind, @base.Form);
        ValidateMetadata(@base.Form, metadata);
        ValidateOverwrite(@base, metadata, overwrite);

        if (identity.CanonicalBasePath != @base.CanonicalLogicalPath)
            throw new ArgumentException("The source identity must match its base document.", nameof(identity));
        Identity = identity;
        Base = @base;
        Metadata = metadata;
        Overwrite = overwrite;
        IsRouteAmbiguous = isRouteAmbiguous;
        Kind = kind;
    }

    internal SourceLogicalIdentity Identity { get; }

    internal string Id => Identity.AutomaticId;

    internal RouteSourceDocument Base { get; }

    internal RouteSourceMetadata Metadata { get; }

    internal RouteSourceDocument? Overwrite { get; }

    internal bool IsRouteAmbiguous { get; }

    internal RouteSourceKind Kind { get; }

    internal string CanonicalPath => Base.CanonicalLogicalPath;

    internal string PhysicalPath => Base.PhysicalPath;

    internal string? OverwritePath => Overwrite?.CanonicalLogicalPath;

    private static SourceLogicalIdentity DefaultIdentity(RouteSourceDocument @base)
    {
        ArgumentNullException.ThrowIfNull(@base);
        return new(SourceIdentity.DeriveId(@base.CanonicalLogicalPath)
            ?? throw new ArgumentException("The source base path does not have a derivable automatic ID.", nameof(@base)), @base.CanonicalLogicalPath);
    }

    private static void ValidateKindAndForm(RouteSourceKind kind, SourceDocumentForm form)
    {
        var valid = SourceFormClassifier.IsEntrypoint(form)
                ? kind == RouteSourceKind.Entrypoint
                : form switch
                {
                    SourceDocumentForm.Loader => kind == RouteSourceKind.Loader,
                    SourceDocumentForm.Skill => kind == RouteSourceKind.Native,
                    SourceDocumentForm.Markdown => kind == RouteSourceKind.Markdown,
                    _ => false,
                };
        if (!valid)
        {
            throw new ArgumentException("The source kind does not match the source document form.", nameof(kind));
        }
    }

    private static void ValidateMetadata(SourceDocumentForm form, RouteSourceMetadata metadata)
    {
        var compatibility = SourceFormClassifier.IsCompatibilityEntrypoint(form);
        if (metadata.IsCompatibilityEntrypoint != compatibility)
        {
            throw new ArgumentException("The metadata compatibility fact does not match the source form.", nameof(metadata));
        }

        if (metadata.State == RouteSourceMetadataState.Complete
            && (form == SourceDocumentForm.Loader
                || form == SourceDocumentForm.Skill && metadata.Tags.Count != 0
                || form != SourceDocumentForm.Skill && metadata.Tags.Count == 0))
        {
            throw new ArgumentException("The complete metadata values do not match the source form.", nameof(metadata));
        }

    }

    private static void ValidateOverwrite(
        RouteSourceDocument @base,
        RouteSourceMetadata metadata,
        RouteSourceDocument? overwrite)
    {
        if (metadata.IsOverwritePresent != (overwrite is not null))
        {
            throw new ArgumentException("The source overwrite document and metadata must agree.", nameof(overwrite));
        }

        if (overwrite is null)
        {
            return;
        }

        if (overwrite.Form != SourceDocumentForm.OverwriteCompanion
            || !string.Equals(overwrite.CanonicalLogicalPath, SourceOverwritePath.ReadAdjacentPath(@base.CanonicalLogicalPath), StringComparison.Ordinal))
        {
            throw new ArgumentException("The overwrite document must be the adjacent companion of the base.", nameof(overwrite));
        }
    }


}
