using OpenForge.Cli.Core.Framework.Filesystem.TypedReads;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;

namespace OpenForge.Cli.Core.Framework.Sources.Models.Reading;

internal sealed class SourceDocumentReadResult
{
    internal SourceDocumentReadResult(
        SourceLayer layer,
        SourceLayerVerification verification,
        FileReadResult<string>? read)
    {
        ArgumentNullException.ThrowIfNull(layer);
        ArgumentNullException.ThrowIfNull(verification);
        if (!ReferenceEquals(layer, verification.Layer))
        {
            throw new ArgumentException("The verification must retain the requested source layer.", nameof(verification));
        }

        if (read is not null
            && !string.Equals(read.LogicalPath, layer.CanonicalPath, StringComparison.Ordinal))
        {
            throw new ArgumentException("A source document read must retain the layer canonical path.", nameof(read));
        }

        if (verification.State == SourceLayerVerificationState.Verified && read is null
            || verification.State == SourceLayerVerificationState.Missing
                && read?.State != FileReadState.Missing
            || verification.State is not (SourceLayerVerificationState.Verified or SourceLayerVerificationState.Missing)
                && read is not null)
        {
            throw new ArgumentException("The source document read does not match the verification state.", nameof(read));
        }

        Layer = layer;
        Verification = verification;
        Read = read;
    }

    internal SourceLayer Layer { get; }

    internal SourceLayerVerification Verification { get; }

    internal FileReadResult<string>? Read { get; }
}
