using System.Collections.Immutable;
using OpenForge.Cli.Core.Framework.Ownership.Models.Document;
using OpenForge.Cli.Core.Commands.Extension.Update.Models.Request;
using OpenForge.Cli.Core.Commands.Extension.Update.Models.Result;
using OpenForge.Cli.Core.Framework.Extensions.Models;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Directories;
using OpenForge.Cli.Core.Framework.Settings.Models.Document;

namespace OpenForge.Cli.Core.Commands.Extension.Update.Models.Planning.Reconciliation;

internal sealed class ExtensionUpdateReconciliationInput
{
    internal required ExtensionUpdateRequest Request { get; init; }

    internal required IReadOnlyList<ExtensionPackageFact> Packages { get; init; }

    internal required WorkspaceOwnershipDocument Ownership { get; init; }

    internal required ExtensionUpdateTopology Topology { get; init; }

    internal required string SourceIdentity { get; init; }

    internal required WorkspaceSettingsDocument Settings { get; init; }
}

internal sealed record ExtensionUpdateReconciliation(
    IReadOnlyList<ExtensionUpdateComparison> Comparisons,
    IReadOnlyList<ExtensionUpdatePlannedEffect> Effects,
    IReadOnlyList<PlannedDirectoryCreation> DirectoryCreations,
    ImmutableArray<ExtensionOwnership> IntendedOwnership,
    IReadOnlyList<ExtensionUpdateFinding> Findings,
    IReadOnlyDictionary<string, byte[]> AdmittedOverrides,
    IReadOnlySet<string> AdmittedExclusions,
    ExtensionUpdateFinding? Finding);
