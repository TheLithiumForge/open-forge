using OpenForge.Cli.Core.Framework.Sources.Models.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;
using OpenForge.Cli.Core.Framework.Sources.Models.Reading;
using OpenForge.Cli.Core.Framework.Sources.Models.Routing;

namespace OpenForge.Cli.Core.UnitTests.Framework.Sources.Models;

public sealed class SourceModelContractTests
{
    [Trait("Boundary", "Processing")]
    [Fact(DisplayName = "Neutral source model enums retain their exact cross-topic vocabulary")]
    [Trait("Feature", "source-catalogue"), Trait("Evidence", "Unit")]
    public void CrossTopicEnumsRemainFiniteAndOrdered()
    {
        Assert.Equal(
            ["Loader", "CanonicalEntrypoint", "IndexEntrypoint", "UnderscoreIndexEntrypoint", "ReferencesEntrypoint", "UnderscoreReferencesEntrypoint", "Skill", "Markdown", "OverwriteCompanion"],
            Enum.GetNames<SourceDocumentForm>());
        Assert.Equal(["Base", "Overwrite"], Enum.GetNames<SourceLayerKind>());
        Assert.Equal(["Verified", "Missing", "Unsafe", "Unavailable", "Changed", "Cancelled"], Enum.GetNames<SourceLayerVerificationState>());
        Assert.Equal(["Routed", "Unrouted", "Ambiguous", "Unavailable"], Enum.GetNames<SourceRouteState>());
    }

    [Trait("Boundary", "Processing")]
    [Fact(DisplayName = "Neutral source model issue vocabularies do not carry command status or rendering policy")]
    [Trait("Feature", "source-catalogue"), Trait("Evidence", "Unit")]
    public void IssueVocabulariesRemainTypedFacts()
    {
        Assert.Equal(
            ["Root", "Directory", "Candidate", "Identity", "Pairing"],
            Enum.GetNames<SourceCatalogueIssueStage>());
        Assert.Equal(
            [
                "RootMissing",
                "RootUnsafe",
                "RootUnavailable",
                "DirectoryUnavailable",
                "CandidateUnsafe",
                "CandidateUnavailable",
                "UnsupportedSource",
                "IdentityUnavailable",
                "EntrypointAmbiguous",
                "EntrypointCompatibilityCollision",
                "IdentityCollision",
                "PhysicalAlias",
                "OrphanOverwrite",
            ],
            Enum.GetNames<SourceCatalogueIssueCode>());
        Assert.Equal(
            [
                "LoaderUnavailable",
                "LoaderUnreadable",
                "LoaderDestinationMissing",
                "LoaderDuplicateRoot",
                "LoaderMalformed",
                "LoaderUnsafe",
                "RouteAmbiguous",
                "RouteSupportUnavailable",
            ],
            Enum.GetNames<SourceRouteIssueCode>());
    }
}
