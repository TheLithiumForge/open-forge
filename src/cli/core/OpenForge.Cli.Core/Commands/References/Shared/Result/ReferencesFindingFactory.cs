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
            new ReferencesFindingInput(
                code,
                code == ReferencesFindingCode.Interrupted
                ? "The References operation was interrupted before all requested evidence was established."
                : "The References operation failed before normal completion.")
            {
                Direction = direction,
            });

    internal static void AddFinding(
        ICollection<ReferencesFinding> findings,
        ReferencesFindingInput input)
    {
        ArgumentNullException.ThrowIfNull(input);
        findings.Add(new ReferencesFinding(
            input.Code,
            input.Direction,
            input.Subject,
            input.Cause,
            input.SelectorRole,
            input.SelectorOccurrence,
            input.Source,
            input.Layer,
            input.Path,
            input.Location,
            input.DestinationLocation,
            input.Candidates ?? [],
            input.StatusOverride));
    }
}
