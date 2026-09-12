using OpenForge.Cli.Core.Commands.Extension.Update.Models.Request;
using OpenForge.Cli.Core.Commands.Extension.Update.Models.Result;
using OpenForge.Cli.Core.Framework.Extensions.Models;
using OpenForge.Cli.Core.Framework.Lifecycle.Models.Document;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Directories;

namespace OpenForge.Cli.Core.Commands.Extension.Update.Models.Planning.Reconciliation;

internal sealed class ExtensionUpdateReconciliationInput
{
    internal required ExtensionUpdateRequest Request { get; init; }

    internal required IReadOnlyList<ExtensionPackageFact> Packages { get; init; }

    internal required ExtensionLifecycleState Current { get; init; }

    internal required ExtensionUpdateTopology Topology { get; init; }

    internal required string SourceIdentity { get; init; }
}

internal sealed record ExtensionUpdateReconciliation(
    IReadOnlyList<ExtensionUpdateComparison> Comparisons,
    IReadOnlyList<ExtensionUpdatePlannedEffect> Effects,
    IReadOnlyList<PlannedDirectoryCreation> DirectoryCreations,
    ExtensionLifecycleState IntendedLifecycle,
    IReadOnlyList<ExtensionUpdateFinding> Findings,
    IReadOnlyDictionary<string, byte[]> AdmittedOverrides,
    IReadOnlySet<string> AdmittedExclusions,
    ExtensionUpdateFinding? Finding);
