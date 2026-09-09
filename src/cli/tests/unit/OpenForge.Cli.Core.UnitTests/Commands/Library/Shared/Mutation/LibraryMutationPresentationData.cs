using OpenForge.Cli.Core.Commands.Library.Models.Application;
using OpenForge.Cli.Core.Commands.Library.Models.Planning;
using OpenForge.Cli.Core.Commands.Library.Models.Request;
using OpenForge.Cli.Core.Commands.Library.Models.Result.Coordinates.Effects;
using OpenForge.Cli.Core.Commands.Library.Models.Result.Coordinates.Observation;
using OpenForge.Cli.Core.Commands.Library.Models.Result.Effects;
using OpenForge.Cli.Core.Commands.Library.Models.Result.Observation;

namespace OpenForge.Cli.Core.UnitTests.Commands.Library.Shared.Mutation;

internal static class LibraryMutationPresentationData
{
    internal static LibraryMutationIdentity Identity(bool sourceIndependent)
        => new() { LibraryId = "team-knowledge", SourceRoot = "shared/team-knowledge", DestinationRoot = ".", Mode = LibraryMode.Apply, SourceIndependent = sourceIndependent };
    internal static LibraryMutationRecord Record()
        => new() { Path = ".agents/open-forge.libraries.json", State = LibraryMutationRecordState.NotStarted, RegisteredPaths = [], Intended = null };
    internal static LibraryMutationSource Source()
        => new()
        {
            RootState = LibrarySourceRootViewState.NotStarted,
            InventoryState = LibraryMutationInventoryState.NotStarted,
            EligiblePaths = [],
            ExcludedPaths = [],
            UnavailablePaths = [],
            LexicalRoot = null,
            PhysicalRoot = null,
            LexicallyContained = null,
            PhysicallyContained = null,
        };
    internal static LibraryMutationProjection Projection()
        => new() { State = LibraryPlanState.NotStarted, Mappings = [], Collisions = [], Ownership = [] };
    internal static LibraryMutationPlanView Plan()
        => new() { State = LibraryPlanState.NotStarted, Directories = [], Links = [], GeneratedRegions = [], RecordEffect = LibraryRecordEffect.None, RecordExpected = null };
    internal static LibraryMutationApplication Application()
        => new()
        {
            State = LibraryApplicationState.NotStarted,
            Verification = LibraryVerificationState.NotStarted,
            Recovery = new() { State = LibraryRecoveryState.NotRequested, Path = null },
            Residuals = [],
            RecordPublication = new() { State = LibraryRecordPublicationState.NotStarted, PublishedLast = null },
        };
}
