using OpenForge.Cli.Core.Commands.Find.Models.Query;
using OpenForge.Cli.Core.Commands.Find.Models.Result;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;

namespace OpenForge.Cli.Core.Commands.Find.Models.Matching;

internal sealed record FindMatchingFindingInput(
    FindFindingCode Code,
    SourceLogicalSource Source,
    SourceLayer Layer,
    FindRegion? Region,
    string Cause);
