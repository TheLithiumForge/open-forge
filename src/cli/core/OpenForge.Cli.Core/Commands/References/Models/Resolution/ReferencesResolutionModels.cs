using OpenForge.Cli.Core.Commands.References.Models.Occurrence;
using OpenForge.Cli.Core.Commands.References.Models.Result;
using OpenForge.Cli.Core.Commands.References.Models.Source;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;
using OpenForge.Cli.Core.Framework.Workspace;

namespace OpenForge.Cli.Core.Commands.References.Models.Resolution;

internal sealed record ReferencesDestinationInput
{
    internal ReferencesDestinationInput(
        CliWorkspace workspace,
        SourceCatalogue catalogue,
        SourceLogicalSource source,
        SourceLayer layer,
        string rawDestination)
    {
        ArgumentNullException.ThrowIfNull(workspace);
        ArgumentNullException.ThrowIfNull(catalogue);
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(layer);
        ArgumentNullException.ThrowIfNull(rawDestination);
        Workspace = workspace;
        Catalogue = catalogue;
        Source = source;
        Layer = layer;
        RawDestination = rawDestination;
    }

    internal CliWorkspace Workspace { get; }

    internal SourceCatalogue Catalogue { get; }

    internal SourceLogicalSource Source { get; }

    internal SourceLayer Layer { get; }

    internal string RawDestination { get; }
}

internal sealed record ReferencesDestinationFinding(
    ReferencesFindingCode Code,
    string Cause,
    IReadOnlyList<ReferencesSourceIdentity> Candidates);

internal sealed record ReferencesDestinationFacts(
    string? Fragment,
    ReferencesTarget Target,
    ReferencesDestinationFinding? Finding);
