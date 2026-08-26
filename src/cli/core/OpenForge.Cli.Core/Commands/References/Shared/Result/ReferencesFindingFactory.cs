using OpenForge.Cli.Core.Commands.References.Models.Request;
using OpenForge.Cli.Core.Commands.References.Models.Result;
using OpenForge.Cli.Core.Commands.References.Models.Source;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;
using OpenForge.Cli.Core.Framework.Sources.Models.Locations;
using OpenForge.Cli.Core.Framework.Sources.Models.Selection;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.Core.Commands.References.Shared.Result;

internal static class ReferencesFindingFactory
{
    internal static void AddEvent(
        ICollection<ReferencesFinding> findings,
        ReferencesFindingCode code,
        ReferencesDirection? direction)
        => AddFinding(
            findings,
            code,
            direction,
            null,
            null,
            null,
            null,
            null,
            code == ReferencesFindingCode.Interrupted
                ? "The References operation was interrupted before all requested evidence was established."
                : "The References operation failed before normal completion.");

    internal static void AddFinding(
        ICollection<ReferencesFinding> findings,
        ReferencesFindingCode code,
        ReferencesDirection? direction,
        ReferencesSourceIdentity? source,
        SourceLayerKind? layer,
        string? path,
        SourceLocation? location,
        SourceLocation? destinationLocation,
        string cause,
        IEnumerable<ReferencesSourceIdentity>? candidates = null,
        SourceUniverseSelectorRole? selectorRole = null,
        int? selectorOccurrence = null,
        string? subject = null,
        CliSemanticStatus? statusOverride = null)
        => findings.Add(new ReferencesFinding(
            code,
            direction,
            subject,
            cause,
            selectorRole,
            selectorOccurrence,
            source,
            layer,
            path,
            location,
            destinationLocation,
            candidates ?? [],
            statusOverride));
}
