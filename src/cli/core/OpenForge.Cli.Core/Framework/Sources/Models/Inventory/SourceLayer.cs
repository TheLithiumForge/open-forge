using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.Identity;

namespace OpenForge.Cli.Core.Framework.Sources.Models.Inventory;

internal enum SourceLayerKind
{
    Base,
    Overwrite,
}

internal sealed class SourceLayer
{
    internal SourceLayer(
        string canonicalPath,
        string physicalPath,
        SourceDocumentForm form,
        SourceLayerKind kind)
    {
        if (!SourceLogicalPath.IsCanonicalSource(canonicalPath))
        {
            throw new ArgumentException("The source layer path is not a canonical .agents source path.", nameof(canonicalPath));
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(physicalPath);
        var normalizedPhysicalPath = Path.GetFullPath(physicalPath);
        if (!Path.IsPathRooted(physicalPath)
            || !string.Equals(normalizedPhysicalPath, physicalPath, StringComparison.Ordinal))
        {
            throw new ArgumentException("The source layer physical path must be absolute and normalized.", nameof(physicalPath));
        }

        if (!Enum.IsDefined(form))
        {
            throw new ArgumentOutOfRangeException(nameof(form), form, "The source layer form is not defined.");
        }

        if (!Enum.IsDefined(kind))
        {
            throw new ArgumentOutOfRangeException(nameof(kind), kind, "The source layer kind is not defined.");
        }

        var isOverwritePath = canonicalPath.EndsWith(".overwrite.md", StringComparison.Ordinal);
        if ((form == SourceDocumentForm.OverwriteCompanion) != isOverwritePath
            || (kind == SourceLayerKind.Overwrite) != isOverwritePath)
        {
            throw new ArgumentException("The source layer form and kind do not match its canonical path.", nameof(kind));
        }

        CanonicalPath = canonicalPath;
        PhysicalPath = normalizedPhysicalPath;
        Form = form;
        Kind = kind;
    }

    internal string CanonicalPath { get; }

    internal string PhysicalPath { get; }

    internal SourceDocumentForm Form { get; }

    internal SourceLayerKind Kind { get; }
}
