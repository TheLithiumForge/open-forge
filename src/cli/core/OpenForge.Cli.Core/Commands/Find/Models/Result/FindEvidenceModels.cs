using OpenForge.Cli.Core.Commands.Find.Models.Query;
using OpenForge.Cli.Core.Commands.Find.Models.Selection;
using OpenForge.Cli.Core.Framework.Documents.Markdown.Models.Structure;
using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;
using OpenForge.Cli.Core.Framework.Sources.Models.Locations;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.Core.Commands.Find.Models.Result;

internal enum FindFindingCode
{
    InvalidInput,
    InvalidSelector,
    WorkspaceUnavailable,
    WorkspaceUnsafe,
    SelectorAmbiguous,
    SelectorUnsafe,
    IdentityCollision,
    CandidateUnsafe,
    LayerUnresolved,
    InspectionUnavailable,
    InvalidEncoding,
    FrontmatterUnavailable,
    SectionAmbiguous,
    ProjectionMissing,
    ProjectionUnavailable,
    OperationFailed,
    Interrupted,
}

internal sealed record FindHeadingEvidence
{
    internal FindHeadingEvidence(int level, MarkdownHeadingForm form, bool canonical)
    {
        if (level is < 1 or > 6)
        {
            throw new ArgumentOutOfRangeException(nameof(level), level, "A Find heading level must be between 1 and 6.");
        }

        if (!Enum.IsDefined(form))
        {
            throw new ArgumentOutOfRangeException(nameof(form), form, "The Markdown heading form is not defined.");
        }

        if (canonical && form != MarkdownHeadingForm.Atx)
        {
            throw new ArgumentException("Only ATX Find heading evidence can be canonical.", nameof(canonical));
        }

        Level = level;
        Form = form;
        Canonical = canonical;
    }

    internal int Level { get; }

    internal MarkdownHeadingForm Form { get; }

    internal bool Canonical { get; }
}

internal sealed record FindEvidence
{
    internal FindEvidence(
        int predicate,
        FindPredicateKind kind,
        string query,
        string authored,
        FindRegion region,
        SourceLayerKind layer,
        string path,
        SourceLocation location,
        int occurrence,
        FindHeadingEvidence? heading)
    {
        if (predicate < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(predicate), predicate, "A Find evidence predicate index must be positive.");
        }

        if (!Enum.IsDefined(kind))
        {
            throw new ArgumentOutOfRangeException(nameof(kind), kind, "The Find evidence predicate kind is not defined.");
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(query);
        ArgumentException.ThrowIfNullOrWhiteSpace(authored);
        ArgumentNullException.ThrowIfNull(region);
        if (region.Kind == FindRegionKind.Document)
        {
            throw new ArgumentException("Find evidence must retain its actual authored region.", nameof(region));
        }
        if (!Enum.IsDefined(layer))
        {
            throw new ArgumentOutOfRangeException(nameof(layer), layer, "The Find evidence layer is not defined.");
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        if (!SourceLogicalPath.IsCanonicalSource(path))
        {
            throw new ArgumentException("Find evidence paths must be canonical .agents source paths.", nameof(path));
        }

        if (!path.EndsWith(".md", StringComparison.Ordinal))
        {
            throw new ArgumentException("Find evidence paths must identify Markdown layers.", nameof(path));
        }

        var isOverwrite = path.EndsWith(".overwrite.md", StringComparison.Ordinal);
        if ((layer == SourceLayerKind.Overwrite) != isOverwrite)
        {
            throw new ArgumentException("The Find evidence path and layer must agree.", nameof(path));
        }
        ArgumentNullException.ThrowIfNull(location);
        if (occurrence < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(occurrence), occurrence, "A Find evidence occurrence must be positive.");
        }

        if ((kind == FindPredicateKind.Heading) != (heading is not null))
        {
            throw new ArgumentException("Only heading evidence can carry heading facts.", nameof(heading));
        }

        Predicate = predicate;
        Kind = kind;
        Query = query;
        Authored = authored;
        Region = region;
        Layer = layer;
        Path = path;
        Location = location;
        Occurrence = occurrence;
        Heading = heading;
    }

    internal int Predicate { get; }

    internal FindPredicateKind Kind { get; }

    internal string Query { get; }

    internal string Authored { get; }

    internal FindRegion Region { get; }

    internal SourceLayerKind Layer { get; }

    internal string Path { get; }

    internal SourceLocation Location { get; }

    internal int Occurrence { get; }

    internal FindHeadingEvidence? Heading { get; }
}
