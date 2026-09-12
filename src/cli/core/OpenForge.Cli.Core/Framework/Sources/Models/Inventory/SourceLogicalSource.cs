using OpenForge.Cli.Core.Framework.Sources.Identity;

namespace OpenForge.Cli.Core.Framework.Sources.Models.Inventory;

internal sealed class SourceLogicalSource
{
    internal SourceLogicalSource(
        SourceLogicalIdentity identity,
        SourceLayer @base,
        SourceLayer? overwrite = null)
    {
        ArgumentNullException.ThrowIfNull(identity);
        ArgumentNullException.ThrowIfNull(@base);
        if (@base.Kind != SourceLayerKind.Base
            || !string.Equals(@base.CanonicalPath, identity.CanonicalBasePath, StringComparison.Ordinal))
        {
            throw new ArgumentException("The source identity and base layer must describe the same canonical base.", nameof(@base));
        }

        if (overwrite is not null)
        {
            if (overwrite.Kind != SourceLayerKind.Overwrite
                || !string.Equals(overwrite.CanonicalPath, SourceOverwritePath.ReadAdjacentPath(@base.CanonicalPath), StringComparison.Ordinal))
            {
                throw new ArgumentException("The overwrite layer must be the exact adjacent companion of the base.", nameof(overwrite));
            }
        }

        Identity = identity;
        Base = @base;
        Overwrite = overwrite;
    }

    internal SourceLogicalIdentity Identity { get; }

    internal SourceLayer Base { get; }

    internal SourceLayer? Overwrite { get; }
}
