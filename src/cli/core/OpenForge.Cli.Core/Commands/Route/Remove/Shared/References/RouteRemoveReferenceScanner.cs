using System.Collections.Immutable;
using System.Text;
using OpenForge.Cli.Core.Commands.Route.Remove.Models.Planning;
using OpenForge.Cli.Core.Commands.Route.Remove.Models.Result;
using OpenForge.Cli.Core.Commands.Route.Remove.Shared.Planning;
using OpenForge.Cli.Core.Commands.Route.Shared.Models.References;
using OpenForge.Cli.Core.Framework.Documents.Markdown;
using OpenForge.Cli.Core.Framework.Documents.Markdown.Models;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem;
using OpenForge.Cli.Core.Framework.Mutation.Validation;
using OpenForge.Cli.Core.Framework.Mutation.Validation.Models;
using OpenForge.Cli.Core.Framework.Sources.Locations;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;
using OpenForge.Cli.Core.Framework.Sources.Models.References;
using OpenForge.Cli.Core.Framework.Sources.References;
using OpenForge.Cli.Core.Framework.Workspace;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.Core.Commands.Route.Remove.Shared.References;

internal sealed partial class RouteRemoveReferenceScanner(
    MarkdownDocumentParser markdownParser,
    SourceLinkDestinationResolver destinationResolver,
    FileExpectationValidator expectationValidator)
{
    private static readonly UTF8Encoding StrictUtf8 = new(false, true);
    private readonly MarkdownDocumentParser _markdownParser = markdownParser;
    private readonly SourceLinkDestinationResolver _destinationResolver = destinationResolver;
    private readonly FileExpectationValidator _expectationValidator = expectationValidator;

    internal async ValueTask<RouteRemoveReferenceScanResult> ScanAsync(
        RouteRemoveReferenceScanInput input,
        RouteRemoveCategoryInventory inventory,
        CancellationToken cancellationToken)
    {
        var documents = new List<RouteRemoveReferenceDocumentPlan>();
        var detachments = new List<RouteRemoveReferenceDetachment>();
        var occurrenceCount = 0;
        var removed = RemovedPaths(input.Request, inventory);
        foreach (var sourcePath in input.Catalogue.SelectedPaths)
        {
            if (removed.Contains(sourcePath))
            {
                continue;
            }

            if (cancellationToken.IsCancellationRequested)
            {
                return Boundary(input, RouteRemoveFindingCode.Interrupted, CliSemanticStatus.Interrupted,
                    sourcePath, "Route Remove reference inspection was interrupted.");
            }

            var read = await ReadDocumentAsync(input, sourcePath, cancellationToken).ConfigureAwait(false);
            if (read is null)
            {
                return Boundary(input, RouteRemoveFindingCode.ReferenceCoverageIncomplete, CliSemanticStatus.Incomplete,
                    sourcePath, "One Markdown reference source could not retain an exact readable snapshot.");
            }

            var facts = _markdownParser.Parse(read.Value.Text);
            var edits = new List<LineEdit>();
            foreach (var link in facts.Links.Where(link => IsAuthoredLink(facts, link)))
            {
                occurrenceCount++;
                var destination = await ResolveAsync(input, sourcePath, link, cancellationToken).ConfigureAwait(false);
                if (!TargetsRemoved(destination, removed))
                {
                    if (destination.Target.Kind != SourceLinkTargetKind.External
                        && destination.Finding is { } unrelatedFinding
                        && destination.Target.Resolution != SourceLinkTargetResolution.FragmentMissing)
                    {
                        var blocked = unrelatedFinding.Code is SourceLinkDestinationFindingCode.TargetUnsafe
                            or SourceLinkDestinationFindingCode.IdentityCollision;
                        return Boundary(
                            input,
                            blocked ? RouteRemoveFindingCode.ReferenceUnsafe : RouteRemoveFindingCode.ReferenceCoverageIncomplete,
                            blocked ? CliSemanticStatus.Blocked : CliSemanticStatus.Incomplete,
                            sourcePath,
                            unrelatedFinding.Cause);
                    }

                    continue;
                }

                if (link.Form != MarkdownLinkForm.Inline || link.IsImage
                    || link.Label.State != MarkdownLinkLabelState.Supported
                    || link.Label.Text is not { } visibleLabel)
                {
                    return Boundary(input, RouteRemoveFindingCode.ReferenceUnsafe, CliSemanticStatus.Blocked,
                        sourcePath, "An incoming Markdown reference cannot be detached without losing authored meaning.");
                }

                var line = ReadLine(read.Value.Text, link.Span);
                var expectedLine = line.Text.Remove(
                    link.Span.Start - line.Start,
                    link.Span.Length).Insert(
                    link.Span.Start - line.Start,
                    visibleLabel);
                edits.Add(new LineEdit(line.Start, line.Text.Length, line.Text, expectedLine, link, visibleLabel));
            }

            if (edits.Count == 0)
            {
                continue;
            }

            var coalesced = Coalesce(edits);
            var intended = Apply(read.Value.Text, coalesced);
            var map = new Utf8SourceMap(read.Value.Text);
            var documentEdits = coalesced.Select(edit => new RouteRemoveReferenceDocumentEdit
            {
                Location = map.Map(edit.Start, edit.Length),
                Before = edit.Before,
                Expected = edit.Expected,
            }).ToImmutableArray();
            detachments.AddRange(coalesced.SelectMany(edit => edit.Links.Select(link => new RouteRemoveReferenceDetachment
            {
                SourcePath = sourcePath,
                Layer = ReadLayer(input.Request.Subject.Catalogue, sourcePath),
                Location = map.Map(link.Link.Span.Start, link.Link.Span.Length),
                Before = edit.Before,
                Expected = edit.Expected,
                OriginalDestination = link.Link.RawDestination,
                VisibleLabel = link.VisibleLabel,
            })));
            documents.Add(new RouteRemoveReferenceDocumentPlan
            {
                SourcePath = sourcePath,
                Snapshot = read.Value.Snapshot,
                IntendedText = intended,
                Edits = documentEdits,
            });
        }

        return new RouteRemoveReferenceScanResult(
            new RouteRemoveReferenceScan
            {
                Documents = documents.ToImmutableArray(),
                Detachments = detachments
                    .OrderBy(item => item.SourcePath, StringComparer.Ordinal)
                    .ThenBy(item => item.Location.ByteOffset)
                    .ToImmutableArray(),
                OccurrenceCount = occurrenceCount,
            },
            boundary: null);
    }

    internal async ValueTask<RouteRemoveReferenceAbsenceResult> ProveAbsenceAsync(
        RouteRemoveAbsenceScope scope,
        SourceCatalogue sourceCatalogue,
        RouteMarkdownCatalogue catalogue,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(scope);
        ArgumentNullException.ThrowIfNull(sourceCatalogue);
        ArgumentNullException.ThrowIfNull(catalogue);
        var occurrenceCount = 0;
        foreach (var sourcePath in catalogue.SelectedPaths)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                return AbsenceBoundary(
                    RouteRemoveFindingCode.Interrupted,
                    CliSemanticStatus.Interrupted,
                    sourcePath,
                    "Route Remove absence reference inspection was interrupted.");
            }

            var read = await ReadDocumentAsync(
                scope.Request.Workspace,
                sourcePath,
                cancellationToken).ConfigureAwait(false);
            if (read is null)
            {
                return AbsenceBoundary(
                    RouteRemoveFindingCode.ReferenceCoverageIncomplete,
                    CliSemanticStatus.Incomplete,
                    sourcePath,
                    "One Markdown source could not retain an exact readable absence-proof snapshot.");
            }

            var facts = _markdownParser.Parse(read.Value.Text);
            foreach (var link in facts.Links.Where(link => IsAuthoredLink(facts, link)))
            {
                occurrenceCount++;
                var destination = await ResolveAsync(
                    scope.Request.Workspace,
                    sourceCatalogue,
                    sourcePath,
                    link,
                    cancellationToken).ConfigureAwait(false);
                if (destination.Target.Kind == SourceLinkTargetKind.Local
                    && destination.Target.Path is { } targetPath
                    && scope.Contains(targetPath))
                {
                    var supported = link.Form == MarkdownLinkForm.Inline
                        && !link.IsImage
                        && link.Label.State == MarkdownLinkLabelState.Supported
                        && link.Label.Text is not null;
                    return supported
                        ? AbsenceBoundary(
                            RouteRemoveFindingCode.SourceNotFound,
                            CliSemanticStatus.Invalid,
                            sourcePath,
                            "A residual incoming Markdown reference prevents verified Route Remove absence.")
                        : AbsenceBoundary(
                            RouteRemoveFindingCode.ReferenceUnsafe,
                            CliSemanticStatus.Blocked,
                            sourcePath,
                            "A residual incoming Markdown reference has an unsupported detachment form.");
                }

                if (destination.Target.Kind != SourceLinkTargetKind.External
                    && destination.Finding is { } unrelatedFinding
                    && destination.Target.Resolution != SourceLinkTargetResolution.FragmentMissing)
                {
                    var blocked = unrelatedFinding.Code is SourceLinkDestinationFindingCode.TargetUnsafe
                        or SourceLinkDestinationFindingCode.IdentityCollision;
                    return AbsenceBoundary(
                        blocked
                            ? RouteRemoveFindingCode.ReferenceUnsafe
                            : RouteRemoveFindingCode.ReferenceCoverageIncomplete,
                        blocked ? CliSemanticStatus.Blocked : CliSemanticStatus.Incomplete,
                        sourcePath,
                        unrelatedFinding.Cause);
                }
            }
        }

        return new RouteRemoveReferenceAbsenceResult
        {
            References = new RouteRemoveReferences
            {
                Coverage = RouteRemoveCoverage.Complete,
                ScannedSourceCount = catalogue.SelectedPaths.Length,
                InspectedSourceCount = catalogue.SelectedPaths.Length,
                OccurrenceCount = occurrenceCount,
            },
        };
    }

    private async ValueTask<(string Text, FileStateSnapshot Snapshot)?> ReadDocumentAsync(
        RouteRemoveReferenceScanInput input,
        string sourcePath,
        CancellationToken cancellationToken)
        => await ReadDocumentAsync(
            input.Request.Subject.Request.Workspace,
            sourcePath,
            cancellationToken).ConfigureAwait(false);

    private async ValueTask<(string Text, FileStateSnapshot Snapshot)?> ReadDocumentAsync(
        CliWorkspace workspace,
        string sourcePath,
        CancellationToken cancellationToken)
    {
        var logical = Path.Combine(
            workspace.LexicalRoot,
            sourcePath.Replace('/', Path.DirectorySeparatorChar));
        var resolution = _expectationValidator.ResolvePath(workspace, logical);
        if (resolution.State != PhysicalPathState.Contained)
        {
            return null;
        }

        try
        {
            var physical = resolution.GetContainedPhysicalPath();
            var bytes = await File.ReadAllBytesAsync(physical, cancellationToken).ConfigureAwait(false);
            var snapshot = FileStateSnapshot.File(logical, physical, bytes);
            var validation = await _expectationValidator.ValidateAsync(
                workspace,
                snapshot.Expectation,
                cancellationToken).ConfigureAwait(false);
            return validation.State == FileExpectationValidationState.Matched
                ? (StrictUtf8.GetString(bytes), snapshot)
                : null;
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception exception) when (exception is DecoderFallbackException
            or UnauthorizedAccessException or IOException)
        {
            return null;
        }
    }

    private async ValueTask<SourceLinkDestinationFacts> ResolveAsync(
        RouteRemoveReferenceScanInput input,
        string sourcePath,
        MarkdownLinkFact link,
        CancellationToken cancellationToken)
        => await ResolveAsync(
            input.Request.Subject.Request.Workspace,
            input.Request.Subject.Catalogue,
            sourcePath,
            link,
            cancellationToken).ConfigureAwait(false);

    private async ValueTask<SourceLinkDestinationFacts> ResolveAsync(
        CliWorkspace workspace,
        SourceCatalogue catalogue,
        string sourcePath,
        MarkdownLinkFact link,
        CancellationToken cancellationToken)
        => await _destinationResolver.ResolveAsync(
            new SourceLinkDestinationInput
            {
                Workspace = workspace,
                Catalogue = catalogue,
                SourceCanonicalPath = sourcePath,
                RawDestination = link.RawDestination,
            },
            cancellationToken).ConfigureAwait(false);

    private static RouteRemoveReferenceAbsenceResult AbsenceBoundary(
        RouteRemoveFindingCode code,
        CliSemanticStatus status,
        string? target,
        string cause)
        => new()
        {
            References = new RouteRemoveReferences
            {
                Coverage = status switch
                {
                    CliSemanticStatus.Incomplete => RouteRemoveCoverage.Incomplete,
                    CliSemanticStatus.Blocked => RouteRemoveCoverage.Blocked,
                    CliSemanticStatus.Interrupted => RouteRemoveCoverage.Interrupted,
                    CliSemanticStatus.Invalid => RouteRemoveCoverage.Complete,
                    _ => throw new ArgumentOutOfRangeException(
                        nameof(status),
                        status,
                        "The Route Remove absence reference status is not defined."),
                },
                ScannedSourceCount = 0,
                InspectedSourceCount = 0,
                OccurrenceCount = 0,
            },
            Finding = new RouteRemoveFinding(code, status, target, cause),
        };

    private static bool TargetsRemoved(
        SourceLinkDestinationFacts destination,
        IReadOnlySet<string> removed)
        => destination.Target.Kind == SourceLinkTargetKind.Local
            && destination.Target.Path is { } path
            && removed.Contains(path);

    private static HashSet<string> RemovedPaths(
        RouteRemoveReferencePlanningRequest request,
        RouteRemoveCategoryInventory inventory)
        => inventory.Items
            .Where(item => item.Kind != RouteRemoveItemKind.Directory)
            .Select(item => Path.GetRelativePath(
                    request.Subject.Request.Workspace.LexicalRoot,
                    item.SourcePath)
                .Replace(Path.DirectorySeparatorChar, '/'))
            .ToHashSet(StringComparer.Ordinal);

    private static bool IsAuthoredLink(MarkdownDocumentFacts facts, MarkdownLinkFact link)
        => facts.GeneratedRegion.ContentSpan is not { } generated
            || link.Span.Start < generated.Start
            || link.Span.End > generated.End;

    private static (int Start, string Text) ReadLine(string text, MarkdownTextSpan span)
    {
        var start = text.LastIndexOf('\n', Math.Max(0, span.Start - 1));
        start = start < 0 ? 0 : start + 1;
        var end = text.IndexOf('\n', span.End);
        end = end < 0 ? text.Length : end;
        return (start, text[start..end]);
    }

    private static IReadOnlyList<CoalescedLineEdit> Coalesce(IEnumerable<LineEdit> edits)
        => edits.GroupBy(edit => (edit.Start, edit.Length))
            .Select(group =>
            {
                var ordered = group.OrderByDescending(edit => edit.Link.Span.Start).ToArray();
                var expected = group.First().Before;
                foreach (var edit in ordered)
                {
                    var offset = edit.Link.Span.Start - edit.Start;
                    expected = expected.Remove(offset, edit.Link.Span.Length).Insert(offset, edit.VisibleLabel);
                }

                return new CoalescedLineEdit(
                    group.Key.Start,
                    group.Key.Length,
                    group.First().Before,
                    expected,
                    group.Select(edit => (edit.Link, edit.VisibleLabel)).ToArray());
            })
            .OrderBy(edit => edit.Start)
            .ToArray();

    private static string Apply(string source, IReadOnlyList<CoalescedLineEdit> edits)
    {
        var result = source;
        foreach (var edit in edits.Reverse())
        {
            result = result.Remove(edit.Start, edit.Length).Insert(edit.Start, edit.Expected);
        }

        return result;
    }

    private static RouteRemoveLayerKind? ReadLayer(SourceCatalogue catalogue, string path)
    {
        var source = catalogue.FindByPath(path);
        if (source is null)
        {
            return null;
        }

        return string.Equals(source.Overwrite?.CanonicalPath, path, StringComparison.Ordinal)
            ? RouteRemoveLayerKind.Overwrite
            : RouteRemoveLayerKind.Base;
    }

    private static RouteRemoveReferenceScanResult Boundary(
        RouteRemoveReferenceScanInput input,
        RouteRemoveFindingCode code,
        CliSemanticStatus status,
        string? target,
        string cause)
        => new(
            scan: null,
            Stop(input, code, status, target, cause));

    internal static RouteRemoveResultFormation Stop(
        RouteRemoveReferenceScanInput input,
        RouteRemoveFindingCode code,
        CliSemanticStatus status,
        string? target,
        string cause)
        => RouteRemoveBoundary.Stop(
            RouteRemoveBoundary.Start(input.Request.Subject.Request) with
            {
                Source = input.Request.Subject.Source,
                Subject = new RouteRemoveSubject { Kind = input.Request.Subject.Kind },
            },
            code,
            status,
            target,
            cause);

    private sealed record LineEdit(
        int Start,
        int Length,
        string Before,
        string Expected,
        MarkdownLinkFact Link,
        string VisibleLabel);

    private sealed record CoalescedLineEdit(
        int Start,
        int Length,
        string Before,
        string Expected,
        IReadOnlyList<(MarkdownLinkFact Link, string VisibleLabel)> Links);
}
