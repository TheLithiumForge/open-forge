using System.Text;
using OpenForge.Cli.Core.Commands.Route.Move.Models.Planning;
using OpenForge.Cli.Core.Commands.Route.Move.Models.Result;
using OpenForge.Cli.Core.Commands.Route.Move.Shared.Planning;
using OpenForge.Cli.Core.Framework.Documents.Markdown;
using OpenForge.Cli.Core.Framework.Documents.Markdown.Models.Inline;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths.Models;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Files;
using OpenForge.Cli.Core.Framework.Mutation.Validation;
using OpenForge.Cli.Core.Framework.Mutation.Validation.Models;
using OpenForge.Cli.Core.Framework.Sources.Models.References;
using OpenForge.Cli.Core.Framework.Sources.References;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.Core.Commands.Route.Move.Shared.References;

internal sealed partial class RouteMoveReferenceScanner(
    MarkdownDocumentParser markdownParser,
    SourceLinkDestinationResolver destinationResolver,
    FileExpectationValidator expectationValidator)
{
    private static readonly UTF8Encoding StrictUtf8 = new(false, true);
    private readonly MarkdownDocumentParser _markdownParser = markdownParser;
    private readonly SourceLinkDestinationResolver _destinationResolver = destinationResolver;
    private readonly FileExpectationValidator _expectationValidator = expectationValidator;

    internal async ValueTask<RouteMoveReferenceScanResult> ScanAsync(
        RouteMoveReferenceScanInput input,
        CancellationToken cancellationToken)
    {
        var documents = new List<RouteMoveReferenceDocumentPlan>(input.Catalogue.SelectedPaths.Length);
        var rewrites = new List<RouteMoveReferenceRewrite>();
        var occurrences = 0;
        foreach (var sourcePath in input.Catalogue.SelectedPaths)
        {
            var inspected = await InspectSafelyAsync(input, sourcePath, cancellationToken)
                .ConfigureAwait(false);
            if (inspected.Boundary is { } boundary)
            {
                return new RouteMoveReferenceScanResult(scan: null, boundary);
            }

            var inspection = inspected.Inspection
                ?? throw new InvalidOperationException(
                    "A successful Route Move reference inspection requires its document.");
            documents.Add(inspection.Document);
            rewrites.AddRange(inspection.Rewrites);
            occurrences += inspection.OccurrenceCount;
        }

        return new RouteMoveReferenceScanResult(
            new RouteMoveReferenceScan
            {
                Documents = [.. documents],
                Rewrites = [.. rewrites],
                OccurrenceCount = occurrences,
            },
            boundary: null);
    }

    private async ValueTask<RouteMoveReferenceDocumentInspectionResult> InspectSafelyAsync(
        RouteMoveReferenceScanInput input,
        string sourcePath,
        CancellationToken cancellationToken)
    {
        if (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                return await InspectDocumentAsync(input, sourcePath, cancellationToken)
                    .ConfigureAwait(false);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
            }
        }

        return new RouteMoveReferenceDocumentInspectionResult(
            inspection: null,
            Stop(
                input,
                RouteMoveFindingCode.Interrupted,
                CliSemanticStatus.Interrupted,
                sourcePath,
                "Route Move reference inspection was interrupted."));
    }

    private async ValueTask<RouteMoveReferenceDocumentInspectionResult> InspectDocumentAsync(
        RouteMoveReferenceScanInput input,
        string sourcePath,
        CancellationToken cancellationToken)
    {
        var workspace = input.Request.Destination.Inventory.Subject.Request.Workspace;
        var document = await ReadDocumentAsync(workspace, sourcePath, cancellationToken)
            .ConfigureAwait(false);
        if (document is null)
        {
            return new RouteMoveReferenceDocumentInspectionResult(
                inspection: null,
                Stop(
                    input,
                    RouteMoveFindingCode.ReferenceCoverageIncomplete,
                    CliSemanticStatus.Incomplete,
                    sourcePath,
                    "One selected Markdown reference source could not retain an exact readable snapshot."));
        }

        var intendedSourcePath = input.MovedPaths.GetValueOrDefault(sourcePath, sourcePath);
        var scanned = await ReadReplacementsAsync(
            input,
            sourcePath,
            intendedSourcePath,
            document,
            cancellationToken).ConfigureAwait(false);
        if (scanned.Boundary is { } boundary)
        {
            return new RouteMoveReferenceDocumentInspectionResult(inspection: null, boundary);
        }

        var replacements = scanned.Scan
            ?? throw new InvalidOperationException(
                "A successful Route Move replacement scan requires its replacements.");
        return new RouteMoveReferenceDocumentInspectionResult(
            FormInspection(input, sourcePath, intendedSourcePath, document, replacements),
            boundary: null);
    }

    private async ValueTask<RouteMoveReferenceDocument?> ReadDocumentAsync(
        Framework.Workspace.Models.CliWorkspace workspace,
        string sourcePath,
        CancellationToken cancellationToken)
    {
        var logicalPath = Path.Combine(
            workspace.LexicalRoot,
            sourcePath.Replace('/', Path.DirectorySeparatorChar));
        var resolution = _expectationValidator.ResolvePath(workspace, logicalPath);
        if (resolution.State != PhysicalPathState.Contained)
        {
            return null;
        }

        try
        {
            var physicalPath = resolution.GetContainedPhysicalPath();
            var bytes = await File.ReadAllBytesAsync(physicalPath, cancellationToken)
                .ConfigureAwait(false);
            var text = StrictUtf8.GetString(bytes);
            var snapshot = FileStateSnapshot.File(logicalPath, physicalPath, bytes);
            var check = await _expectationValidator.ValidateAsync(
                workspace,
                snapshot.Expectation,
                cancellationToken).ConfigureAwait(false);
            if (check.State != FileExpectationValidationState.Matched)
            {
                return null;
            }

            return new RouteMoveReferenceDocument
            {
                Text = text,
                Facts = _markdownParser.Parse(text),
                Snapshot = snapshot,
            };
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception exception) when (exception is DecoderFallbackException
            or UnauthorizedAccessException
            or IOException)
        {
            return null;
        }
    }

    private async ValueTask<RouteMoveReferenceReplacementScanResult> ReadReplacementsAsync(
        RouteMoveReferenceScanInput input,
        string sourcePath,
        string intendedSourcePath,
        RouteMoveReferenceDocument document,
        CancellationToken cancellationToken)
    {
        var replacements = new Dictionary<(int Start, int Length), RouteMoveReferenceReplacement>();
        var meanings = new List<RouteMoveReferenceMeaning>();
        foreach (var link in document.Facts.Links.Where(link => IsAuthoredLink(document, link)))
        {
            var projected = await InspectLinkAsync(
                input,
                sourcePath,
                intendedSourcePath,
                link,
                cancellationToken).ConfigureAwait(false);
            if (projected.Boundary is { } boundary)
            {
                return new RouteMoveReferenceReplacementScanResult(scan: null, boundary);
            }

            if (projected.Replacement is { } replacement)
            {
                replacements.TryAdd((replacement.Span.Start, replacement.Span.Length), replacement);
            }

            if (projected.Meaning is { } meaning)
            {
                meanings.Add(meaning);
            }
        }

        return Complete(document, replacements, meanings);
    }

    private async ValueTask<RouteMoveReferenceReplacementProjection> InspectLinkAsync(
        RouteMoveReferenceScanInput input,
        string sourcePath,
        string intendedSourcePath,
        MarkdownLinkFact link,
        CancellationToken cancellationToken)
    {
        var facts = await ResolveLinkAsync(input, sourcePath, link, cancellationToken)
            .ConfigureAwait(false);
        var boundary = ReadLinkBoundary(input, sourcePath, facts);
        return boundary is null
            ? ProjectReplacement(input, sourcePath, intendedSourcePath, link, facts) with
            {
                Meaning = ProjectMeaning(input.MovedPaths, facts),
            }
            : new RouteMoveReferenceReplacementProjection { Boundary = boundary };
    }

    private static RouteMoveReferenceMeaning? ProjectMeaning(
        IReadOnlyDictionary<string, string> movedPaths,
        SourceLinkDestinationFacts facts)
    {
        if (facts.Target.Kind != SourceLinkTargetKind.Local
            || facts.Target.Path is not { } targetPath)
        {
            return null;
        }

        var expectedPath = movedPaths.GetValueOrDefault(targetPath, targetPath);
        return new RouteMoveReferenceMeaning
        {
            TargetId = facts.Target.Id is null ? null : Framework.Sources.Identity.SourceIdentity.DeriveId(expectedPath),
            TargetPath = expectedPath,
            Layer = facts.Target.Layer,
            Resolution = facts.Target.Resolution,
            Fragment = facts.Fragment,
        };
    }

    private static RouteMoveReferenceReplacementScanResult Complete(
        RouteMoveReferenceDocument document,
        IDictionary<(int Start, int Length), RouteMoveReferenceReplacement> replacements,
        IEnumerable<RouteMoveReferenceMeaning> meanings)
        => new(
            new RouteMoveReferenceReplacementScan
            {
                Replacements = [.. replacements.Values],
                Meanings = [.. meanings],
                OccurrenceCount = document.Facts.Links.Count(link => IsAuthoredLink(document, link)),
            },
            boundary: null);

    private static bool IsAuthoredLink(
        RouteMoveReferenceDocument document,
        MarkdownLinkFact link)
    {
        if (document.Facts.GeneratedRegion.ContentSpan is not { } generated
            || link.DestinationSpan is not { } destination)
        {
            return true;
        }

        return destination.Start < generated.Start || destination.End > generated.End;
    }

    private async ValueTask<SourceLinkDestinationFacts> ResolveLinkAsync(
        RouteMoveReferenceScanInput input,
        string sourcePath,
        MarkdownLinkFact link,
        CancellationToken cancellationToken)
        => await _destinationResolver.ResolveAsync(
            new SourceLinkDestinationInput
            {
                Workspace = input.Request.Destination.Inventory.Subject.Request.Workspace,
                Catalogue = input.Request.Destination.Inventory.Subject.Catalogue,
                SourceCanonicalPath = sourcePath,
                RawDestination = link.RawDestination,
            },
            cancellationToken).ConfigureAwait(false);

    private static RouteMoveResultFormation? ReadLinkBoundary(
        RouteMoveReferenceScanInput input,
        string sourcePath,
        SourceLinkDestinationFacts facts)
    {
        if (facts.Target.Kind == SourceLinkTargetKind.External
            || facts.Finding is null
            || facts.Target.Resolution == SourceLinkTargetResolution.FragmentMissing)
        {
            return null;
        }

        var unsafeTarget = facts.Finding.Code is
            SourceLinkDestinationFindingCode.TargetUnsafe
            or SourceLinkDestinationFindingCode.IdentityCollision;
        return Stop(
            input,
            unsafeTarget
                ? RouteMoveFindingCode.ReferenceUnsafe
                : RouteMoveFindingCode.ReferenceCoverageIncomplete,
            unsafeTarget ? CliSemanticStatus.Blocked : CliSemanticStatus.Incomplete,
            sourcePath,
            facts.Finding.Cause);
    }

    internal static RouteMoveResultFormation Stop(
        RouteMoveReferenceScanInput input,
        RouteMoveFindingCode code,
        CliSemanticStatus status,
        string? target,
        string cause)
    {
        var subject = input.Request.Destination.Inventory.Subject;
        var formation = RouteMoveBoundary.Start(subject.Request) with
        {
            Source = subject.Source,
            Destination = input.Request.Destination.Destination,
            Subject = input.Request.Destination.Subject,
            Ownership = RouteMoveOwnershipProjector.Project(
                input.Request.Destination.Inventory.Ownership,
                subject),
            References = new RouteMoveReferences
            {
                Coverage = ReadCoverage(status),
                ScannedSourceCount = input.Catalogue.SelectedPaths.Length,
                InspectedSourceCount = 0,
                OccurrenceCount = 0,
            },
        };
        return RouteMoveBoundary.Stop(formation, code, status, target, cause);
    }

    private static RouteMoveCoverage ReadCoverage(CliSemanticStatus status)
        => status switch
        {
            CliSemanticStatus.Blocked => RouteMoveCoverage.Blocked,
            CliSemanticStatus.Incomplete => RouteMoveCoverage.Incomplete,
            CliSemanticStatus.Interrupted => RouteMoveCoverage.Interrupted,
            _ => throw new ArgumentOutOfRangeException(
                nameof(status),
                status,
                "The reference boundary status is not defined."),
        };
}
