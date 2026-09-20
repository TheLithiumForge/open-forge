using OpenForge.Cli.Core.Commands.Library.Models.Planning;
using OpenForge.Cli.Core.Commands.Library.Models.Result.Coordinates.Effects;
using OpenForge.Cli.Core.Framework.Libraries.Models.Identity;
using OpenForge.Cli.Core.Framework.Libraries.Models.Observation;
using OpenForge.Cli.Core.Framework.Libraries.Operational.Models;

namespace OpenForge.Cli.Core.Commands.Library.Models.Result.Effects;

internal sealed record LibraryMutationPlanView
{

    public required LibraryPlanState State { get; init; }

    public required LibraryDirectoryEffectView[] Directories { get; init; }

    public required LibraryLinkEffectView[] Links { get; init; }

    public required LibraryGeneratedRegionEffectView[] GeneratedRegions { get; init; }

    public required LibraryRecordEffect RecordEffect { get; init; }
    public required LibraryExpectedState? RecordExpected { get; init; }

}
