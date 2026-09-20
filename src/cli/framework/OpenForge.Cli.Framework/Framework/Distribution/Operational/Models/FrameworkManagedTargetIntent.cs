using OpenForge.Cli.Core.Framework.Distribution.Models.Content;
using OpenForge.Cli.Core.Framework.Distribution.Operational.Models;

namespace OpenForge.Cli.Core.Framework.Distribution.Operational.Models;

internal sealed record FrameworkManagedTargetIntent(
    string Path,
    FrameworkManagedTargetKind Kind,
    string? Region,
    string? SourceAssetPath,
    string? IntendedFingerprint,
    FrameworkTargetSourceValidation Source);
