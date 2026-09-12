using OpenForge.Cli.Core.Commands.References.Models.Inspection;
using OpenForge.Cli.Core.Commands.References.Models.Occurrence;
using OpenForge.Cli.Core.Commands.References.Models.Request;
using OpenForge.Cli.Core.Commands.References.Models.Result;
using OpenForge.Cli.Core.Commands.References.Models.Source;
using OpenForge.Cli.Core.Commands.References.Shared.Documents.Parsing;
using OpenForge.Cli.Core.Commands.References.Shared.Extraction;
using OpenForge.Cli.Core.Commands.References.Shared.Result;
using OpenForge.Cli.Core.Framework.Documents.Markdown.Models;
using OpenForge.Cli.Core.Framework.Filesystem.TypedReads.Models;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;
using OpenForge.Cli.Core.Framework.Sources.Models.Reading;
using OpenForge.Cli.Core.Framework.Sources.Reading;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.Core.Commands.References.Shared.Inspection;

internal delegate ValueTask<SourceDocumentReadResult> ReferencesLayerReader(
    SourceDocumentReader reader,
    SourceLayer layer,
    CancellationToken cancellationToken);

internal sealed class ReferencesLayerInspector
{
    private readonly ReferencesLayerReader _layerReader;
    private readonly ReferencesMarkdownParser _markdownParser;
    private readonly ReferencesLinkExtractor _extractor = new();

    internal ReferencesLayerInspector(
        ReferencesLayerReader layerReader,
        ReferencesMarkdownParser markdownParser)
    {
        ArgumentNullException.ThrowIfNull(layerReader);
        ArgumentNullException.ThrowIfNull(markdownParser);
        _layerReader = layerReader;
        _markdownParser = markdownParser;
    }

    internal async ValueTask<ReferencesLayerInspectionResult> InspectAsync(
        ReferencesLayerInspectionInput input,
        ICollection<ReferencesFinding> findings,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(input);
        var session = input.Session;
        var source = input.Source;
        var layer = input.Layer;
        var direction = input.Direction;
        var provenance = input.Provenance;
        SourceDocumentReadResult read;
        try
        {
            read = await _layerReader(session.DocumentReader, layer, cancellationToken).ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            ReferencesFindingFactory.AddEvent(findings, ReferencesFindingCode.Interrupted, direction);
            return new ReferencesLayerInspectionResult(null, false, false, true, false);
        }
        catch (Exception)
        {
            ReferencesFindingFactory.AddEvent(findings, ReferencesFindingCode.OperationFailed, direction);
            return new ReferencesLayerInspectionResult(null, false, false, false, true);
        }

        var verificationFindingCode = ReadVerificationFindingCode(read.Verification.State);
        if (read.Verification.State == SourceLayerVerificationState.Cancelled
            || read.Read?.State == FileReadState.Cancelled)
        {
            ReferencesFindingFactory.AddEvent(findings, ReferencesFindingCode.Interrupted, direction);
            return new ReferencesLayerInspectionResult(null, false, false, true, false);
        }

        if (verificationFindingCode is { } verificationCode)
        {
            AddLayerFinding(findings, verificationCode, direction, source, layer, "The source layer could not be verified.");
            return new ReferencesLayerInspectionResult(null, false, false, false, false);
        }

        if (read.Read is null)
        {
            AddLayerFinding(
                findings,
                ReferencesFindingCode.InspectionUnavailable,
                direction,
                source,
                layer,
                "The verified source layer did not establish readable bytes.");
            return new ReferencesLayerInspectionResult(null, false, false, false, false);
        }

        if (read.Read.State != FileReadState.Complete || read.Read.Value is null)
        {
            var code = ReadFileFindingCode(read.Read.State);
            if (code == ReferencesFindingCode.Interrupted)
            {
                ReferencesFindingFactory.AddEvent(findings, code, direction);
                return new ReferencesLayerInspectionResult(null, false, false, true, false);
            }

            AddLayerFinding(findings, code, direction, source, layer, "The source layer bytes could not be inspected.");
            return new ReferencesLayerInspectionResult(null, false, false, false, false);
        }

        MarkdownDocumentFacts document;
        try
        {
            document = _markdownParser(read.Read.Value);
        }
        catch (OperationCanceledException)
        {
            ReferencesFindingFactory.AddEvent(findings, ReferencesFindingCode.Interrupted, direction);
            return new ReferencesLayerInspectionResult(null, false, false, true, false);
        }
        catch (Exception)
        {
            AddLayerFinding(
                findings,
                ReferencesFindingCode.InspectionUnavailable,
                direction,
                source,
                layer,
                "The source Markdown facts could not be established.");
            return new ReferencesLayerInspectionResult(null, false, false, false, false);
        }

        ReferencesInspectionFacts inspection;
        try
        {
            inspection = _extractor.Extract(
                source,
                layer,
                layer.CanonicalPath,
                document,
                direction,
                provenance);
        }
        catch (Exception)
        {
            AddLayerFinding(
                findings,
                ReferencesFindingCode.InspectionUnavailable,
                direction,
                source,
                layer,
                "The authored link facts could not be established.");
            return new ReferencesLayerInspectionResult(null, false, false, false, false);
        }

        foreach (var finding in inspection.Findings)
        {
            ReferencesFindingFactory.AddFinding(
                findings,
                new ReferencesFindingInput(finding.Code, finding.Cause)
                {
                    Direction = direction,
                    Source = ToSourceIdentity(source),
                    Layer = layer.Kind,
                    Path = layer.CanonicalPath,
                    Location = finding.Location,
                    DestinationLocation = finding.DestinationLocation,
                    StatusOverride = finding.Blocked ? CliSemanticStatus.Blocked : null,
                });
        }

        if (cancellationToken.IsCancellationRequested)
        {
            ReferencesFindingFactory.AddEvent(findings, ReferencesFindingCode.Interrupted, direction);
            return new ReferencesLayerInspectionResult(inspection, inspection.Established, inspection.Blocked, true, false);
        }

        return new ReferencesLayerInspectionResult(inspection, inspection.Established, inspection.Blocked, false, false);
    }

    internal static ReferencesFindingCode? ReadVerificationFindingCode(
        SourceLayerVerificationState state)
        => state switch
        {
            SourceLayerVerificationState.Verified => null,
            SourceLayerVerificationState.Missing => ReferencesFindingCode.LayerUnresolved,
            SourceLayerVerificationState.Unsafe => ReferencesFindingCode.CandidateUnsafe,
            SourceLayerVerificationState.Unavailable => ReferencesFindingCode.InspectionUnavailable,
            SourceLayerVerificationState.Changed => ReferencesFindingCode.LayerUnresolved,
            SourceLayerVerificationState.Cancelled => ReferencesFindingCode.Interrupted,
            _ => throw new ArgumentOutOfRangeException(
                nameof(state),
                state,
                "The source layer verification state is not defined."),
        };

    internal static ReferencesFindingCode ReadFileFindingCode(FileReadState state)
        => state switch
        {
            FileReadState.Complete => ReferencesFindingCode.InspectionUnavailable,
            FileReadState.Missing => ReferencesFindingCode.InspectionUnavailable,
            FileReadState.InvalidEncoding => ReferencesFindingCode.InvalidEncoding,
            FileReadState.InvalidSyntax => ReferencesFindingCode.InspectionUnavailable,
            FileReadState.AccessDenied => ReferencesFindingCode.InspectionUnavailable,
            FileReadState.InputOutputFailure => ReferencesFindingCode.InspectionUnavailable,
            FileReadState.Cancelled => ReferencesFindingCode.Interrupted,
            _ => throw new ArgumentOutOfRangeException(
                nameof(state),
                state,
                "The file read state is not defined."),
        };

    private static void AddLayerFinding(
        ICollection<ReferencesFinding> findings,
        ReferencesFindingCode code,
        ReferencesDirection direction,
        SourceLogicalSource source,
        SourceLayer layer,
        string cause)
        => ReferencesFindingFactory.AddFinding(
            findings,
            new ReferencesFindingInput(code, cause)
            {
                Direction = direction,
                Source = ToSourceIdentity(source),
                Layer = layer.Kind,
                Path = layer.CanonicalPath,
            });

    private static ReferencesSourceIdentity ToSourceIdentity(SourceLogicalSource source)
        => new(source.Identity.AutomaticId, source.Identity.CanonicalBasePath);
}
