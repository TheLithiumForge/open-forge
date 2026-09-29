using OpenForge.Cli.Core.Framework.Documents.Metadata.Shared.Applicability.Models;

namespace OpenForge.Cli.Core.Framework.Sources.Shared.Applicability.Models;

internal sealed record SourceApplyToCondition(
    string CanonicalSourcePath,
    ApplyToMetadataFacts Metadata);
