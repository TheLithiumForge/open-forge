using OpenForge.Cli.Core.Commands.References;
using OpenForge.Cli.Core.Commands.References.Models.Result;
using OpenForge.Cli.Core.Commands.References.Shared.Inspection;
using OpenForge.Cli.Core.Commands.References.Shared.Result;
using OpenForge.Cli.Core.Framework.Filesystem.TypedReads.Models;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;
using OpenForge.Cli.Core.Framework.Sources.Models.Reading;

namespace OpenForge.Cli.Core.UnitTests.Commands.References;

public sealed class ReferencesEnumMappingTests
{
    [Trait("Boundary", "Processing")]
    [Fact(DisplayName = "References layer verification mapping is exhaustive"), Trait("Feature", "references-enum-mapping"), Trait("Evidence", "Unit")]
    public void LayerVerificationMappingIsExhaustive()
    {
        var expected = new (SourceLayerVerificationState State, ReferencesFindingCode? Code)[]
        {
            (SourceLayerVerificationState.Verified, null),
            (SourceLayerVerificationState.Missing, ReferencesFindingCode.LayerUnresolved),
            (SourceLayerVerificationState.Unsafe, ReferencesFindingCode.CandidateUnsafe),
            (SourceLayerVerificationState.Unavailable, ReferencesFindingCode.InspectionUnavailable),
            (SourceLayerVerificationState.Changed, ReferencesFindingCode.LayerUnresolved),
            (SourceLayerVerificationState.Cancelled, ReferencesFindingCode.Interrupted),
        };
        foreach (var (state, code) in expected)
        {
            Assert.Equal(code, ReferencesLayerInspector.ReadVerificationFindingCode(state));
        }

        var undefined = (SourceLayerVerificationState)int.MaxValue;
        var exception = Assert.Throws<ArgumentOutOfRangeException>(
            () => ReferencesLayerInspector.ReadVerificationFindingCode(undefined));
        Assert.Equal("state", exception.ParamName);
        Assert.Equal(undefined, exception.ActualValue);
    }

    [Trait("Boundary", "Processing")]
    [Fact(DisplayName = "References file read mapping is exhaustive"), Trait("Feature", "references-enum-mapping"), Trait("Evidence", "Unit")]
    public void FileReadMappingIsExhaustive()
    {
        var expected = new (FileReadState State, ReferencesFindingCode Code)[]
        {
            (FileReadState.Complete, ReferencesFindingCode.InspectionUnavailable),
            (FileReadState.Missing, ReferencesFindingCode.InspectionUnavailable),
            (FileReadState.InvalidEncoding, ReferencesFindingCode.InvalidEncoding),
            (FileReadState.InvalidSyntax, ReferencesFindingCode.InspectionUnavailable),
            (FileReadState.AccessDenied, ReferencesFindingCode.InspectionUnavailable),
            (FileReadState.InputOutputFailure, ReferencesFindingCode.InspectionUnavailable),
            (FileReadState.Cancelled, ReferencesFindingCode.Interrupted),
        };
        foreach (var (state, code) in expected)
        {
            Assert.Equal(code, ReferencesLayerInspector.ReadFileFindingCode(state));
        }

        var undefined = (FileReadState)int.MaxValue;
        var exception = Assert.Throws<ArgumentOutOfRangeException>(
            () => ReferencesLayerInspector.ReadFileFindingCode(undefined));
        Assert.Equal("state", exception.ParamName);
        Assert.Equal(undefined, exception.ActualValue);
    }

    [Trait("Boundary", "Processing")]
    [Fact(DisplayName = "References layer rank mapping is exhaustive and names null"), Trait("Feature", "references-enum-mapping"), Trait("Evidence", "Unit")]
    public void LayerRankMappingIsExhaustiveAndNamesNull()
    {
        Assert.Equal(0, ReferencesResultBuilder.ReadLayerRank(SourceLayerKind.Base));
        Assert.Equal(1, ReferencesResultBuilder.ReadLayerRank(SourceLayerKind.Overwrite));
        Assert.Equal(2, ReferencesResultBuilder.ReadLayerRank(null));

        var undefined = (SourceLayerKind)int.MaxValue;
        var exception = Assert.Throws<ArgumentOutOfRangeException>(
            () => ReferencesResultBuilder.ReadLayerRank(undefined));
        Assert.Equal("layer", exception.ParamName);
        Assert.Equal(undefined, exception.ActualValue);
    }

    [Trait("Boundary", "Processing")]
    [Fact(DisplayName = "References root issue mapping is exhaustive"), Trait("Feature", "references-enum-mapping"), Trait("Evidence", "Unit")]
    public void RootIssueMappingIsExhaustive()
    {
        var expected = new (SourceCatalogueIssueCode Code, ReferencesFindingCode? FindingCode)[]
        {
            (SourceCatalogueIssueCode.RootMissing, ReferencesFindingCode.WorkspaceUnavailable),
            (SourceCatalogueIssueCode.RootUnsafe, ReferencesFindingCode.WorkspaceUnsafe),
            (SourceCatalogueIssueCode.RootUnavailable, ReferencesFindingCode.WorkspaceUnavailable),
            (SourceCatalogueIssueCode.DirectoryUnavailable, null),
            (SourceCatalogueIssueCode.CandidateUnsafe, null),
            (SourceCatalogueIssueCode.CandidateUnavailable, null),
            (SourceCatalogueIssueCode.IdentityUnavailable, null),
            (SourceCatalogueIssueCode.IdentityCollision, null),
            (SourceCatalogueIssueCode.PhysicalAlias, null),
            (SourceCatalogueIssueCode.OrphanOverwrite, null),
        };
        foreach (var (code, findingCode) in expected)
        {
            Assert.Equal(findingCode, ReferencesSourceFindingMapper.ReadRootIssueFindingCode(code));
        }

        var undefined = (SourceCatalogueIssueCode)int.MaxValue;
        var exception = Assert.Throws<ArgumentOutOfRangeException>(
            () => ReferencesSourceFindingMapper.ReadRootIssueFindingCode(undefined));
        Assert.Equal("code", exception.ParamName);
        Assert.Equal(undefined, exception.ActualValue);
    }

    [Trait("Boundary", "Processing")]
    [Fact(DisplayName = "References source catalogue issue mapping is exhaustive"), Trait("Feature", "references-enum-mapping"), Trait("Evidence", "Unit")]
    public void SourceCatalogueIssueMappingIsExhaustive()
    {
        var expected = new (SourceCatalogueIssueCode Code, ReferencesFindingCode? FindingCode)[]
        {
            (SourceCatalogueIssueCode.RootMissing, null),
            (SourceCatalogueIssueCode.RootUnsafe, null),
            (SourceCatalogueIssueCode.RootUnavailable, null),
            (SourceCatalogueIssueCode.DirectoryUnavailable, ReferencesFindingCode.InspectionUnavailable),
            (SourceCatalogueIssueCode.CandidateUnsafe, ReferencesFindingCode.CandidateUnsafe),
            (SourceCatalogueIssueCode.CandidateUnavailable, ReferencesFindingCode.InspectionUnavailable),
            (SourceCatalogueIssueCode.IdentityUnavailable, ReferencesFindingCode.IdentityUnavailable),
            (SourceCatalogueIssueCode.IdentityCollision, ReferencesFindingCode.IdentityCollision),
            (SourceCatalogueIssueCode.PhysicalAlias, ReferencesFindingCode.PhysicalAlias),
            (SourceCatalogueIssueCode.OrphanOverwrite, ReferencesFindingCode.LayerUnresolved),
        };
        foreach (var (code, findingCode) in expected)
        {
            Assert.Equal(findingCode, ReferencesSourceFindingMapper.ReadSourceCatalogueFindingCode(code));
        }

        var undefined = (SourceCatalogueIssueCode)int.MaxValue;
        var exception = Assert.Throws<ArgumentOutOfRangeException>(
            () => ReferencesSourceFindingMapper.ReadSourceCatalogueFindingCode(undefined));
        Assert.Equal("code", exception.ParamName);
        Assert.Equal(undefined, exception.ActualValue);
    }
}
