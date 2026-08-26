using OpenForge.Cli.Core.Commands.References.Models.Inspection;
using OpenForge.Cli.Core.Commands.References.Models.Occurrence;
using OpenForge.Cli.Core.Commands.References.Models.Operation;
using OpenForge.Cli.Core.Commands.References.Models.Request;
using OpenForge.Cli.Core.Commands.References.Models.Result;
using OpenForge.Cli.Core.Commands.References.Models.Source;
using OpenForge.Cli.Core.Commands.References.Shared.Extraction;
using OpenForge.Cli.Core.Commands.References.Shared.Result;
using OpenForge.Cli.Core.Framework.Documents.Markdown.Models;
using OpenForge.Cli.Core.Framework.Filesystem.TypedReads;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;
using OpenForge.Cli.Core.Framework.Sources.Models.Reading;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.Core.Commands.References.Shared.Inspection;

internal sealed record ReferencesLayerInspectionResult(
    ReferencesInspectionFacts? Inspection,
    bool Established,
    bool Blocked,
    bool Interrupted,
    bool Failed);

internal sealed class ReferencesLayerInspector
{
    private readonly ReferencesOperationComponents _components;
    private readonly ReferencesLinkExtractor _extractor = new();

    internal ReferencesLayerInspector(ReferencesOperationComponents components)
    {
        ArgumentNullException.ThrowIfNull(components);
        _components = components;
    }

    internal async ValueTask<ReferencesLayerInspectionResult> InspectAsync(
        ReferencesSourceReadContext context,
        SourceLogicalSource source,
        SourceLayer layer,
        ReferencesDirection direction,
        ReferencesProvenance provenance,
        ICollection<ReferencesFinding> findings,
        CancellationToken cancellationToken)
    {
        SourceDocumentReadResult read;
        try
        {
            read = await _components.LayerReader(context.DocumentReader, layer, cancellationToken).ConfigureAwait(false);
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

        if (read.Verification.State == SourceLayerVerificationState.Cancelled
            || read.Read?.State == FileReadState.Cancelled)
        {
            ReferencesFindingFactory.AddEvent(findings, ReferencesFindingCode.Interrupted, direction);
            return new ReferencesLayerInspectionResult(null, false, false, true, false);
        }

        if (read.Verification.State != SourceLayerVerificationState.Verified)
        {
            var code = read.Verification.State switch
            {
                SourceLayerVerificationState.Unsafe => ReferencesFindingCode.CandidateUnsafe,
                SourceLayerVerificationState.Missing or SourceLayerVerificationState.Changed => ReferencesFindingCode.LayerUnresolved,
                _ => ReferencesFindingCode.InspectionUnavailable,
            };
            AddLayerFinding(findings, code, direction, source, layer, "The source layer could not be verified.");
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
            var code = read.Read.State switch
            {
                FileReadState.InvalidEncoding => ReferencesFindingCode.InvalidEncoding,
                FileReadState.Cancelled => ReferencesFindingCode.Interrupted,
                _ => ReferencesFindingCode.InspectionUnavailable,
            };
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
            document = _components.MarkdownParser(read.Read.Value);
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
                finding.Code,
                direction,
                ToSourceIdentity(source),
                layer.Kind,
                layer.CanonicalPath,
                finding.Location,
                finding.DestinationLocation,
                finding.Cause,
                statusOverride: finding.Blocked ? CliSemanticStatus.Blocked : null);
        }

        if (cancellationToken.IsCancellationRequested)
        {
            ReferencesFindingFactory.AddEvent(findings, ReferencesFindingCode.Interrupted, direction);
            return new ReferencesLayerInspectionResult(inspection, inspection.Established, inspection.Blocked, true, false);
        }

        return new ReferencesLayerInspectionResult(inspection, inspection.Established, inspection.Blocked, false, false);
    }

    private static void AddLayerFinding(
        ICollection<ReferencesFinding> findings,
        ReferencesFindingCode code,
        ReferencesDirection direction,
        SourceLogicalSource source,
        SourceLayer layer,
        string cause)
        => ReferencesFindingFactory.AddFinding(
            findings,
            code,
            direction,
            ToSourceIdentity(source),
            layer.Kind,
            layer.CanonicalPath,
            null,
            null,
            cause);

    private static ReferencesSourceIdentity ToSourceIdentity(SourceLogicalSource source)
        => new(source.Identity.AutomaticId, source.Identity.CanonicalBasePath);
}
