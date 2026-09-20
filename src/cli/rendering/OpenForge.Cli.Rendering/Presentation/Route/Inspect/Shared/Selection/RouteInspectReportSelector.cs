using OpenForge.Cli.Core.Commands.Route.Inspect.Models.Profile;
using OpenForge.Cli.Core.Commands.Route.Inspect.Models.Resolution;
using OpenForge.Cli.Core.Commands.Route.Inspect.Models.Result;
using OpenForge.Cli.Core.Presentation.Route.Inspect.Models;
using OpenForge.Cli.Core.Presentation.Route.Inspect.Shared.Wording;
using OpenForge.Cli.Core.Presentation.Shared.Models;
using OpenForge.Cli.Core.Presentation.Shared.Rendering;
using OpenForge.Cli.Core.Presentation.Shared.Selection.Models;
using OpenForge.Cli.Core.Presentation.Shared.Wording;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Operation;

namespace OpenForge.Cli.Core.Presentation.Route.Inspect.Shared.Selection;

internal static class RouteInspectReportSelector
{
    internal static CliReport<RouteInspectData> Select(RouteInspectResult result, CliSelection selection)
    {
        ArgumentNullException.ThrowIfNull(result);
        ArgumentNullException.ThrowIfNull(selection);
        var standard = selection.Detail >= CliDetail.Standard;
        var full = selection.Detail >= CliDetail.Full;
        return new CliReport<RouteInspectData>
        {
            Command = result.Command,
            Status = result.Status,
            Headline = Headline(result),
            HeadlineFindingCode = HeadlineFindingCode(result),
            Workspace = result.WorkspacePath is { } path
                ? new CliWorkspaceEcho(path, result.WorkspaceExplicit)
                : null,
            Findings = result.Observations.Select(observation => Finding(result, observation))
                .Concat(result.Conditions.Select(condition => Finding(result, condition)))
                .ToArray(),
            Effects = [],
            Counts = Counts(result.Profile),
            Data = ProjectData(result, standard, full),
            Recovery = null,
            Next = result.Next,
            Diagnostics = selection.Detail == CliDetail.Debug
                ? Diagnostics(result)
                : [],
        };
    }

    private static RouteInspectData ProjectData(RouteInspectResult result, bool standard, bool full)
    {
        var identity = result.Identity;
        var profile = result.Profile;
        var topology = profile?.Topology;
        var topologyValue = topology is { State: RouteInspectFactState.Value }
            ? topology.ReadValue()
            : null;
        var automatic = profile?.Reading.Automatic;
        var later = profile?.Reading.Later;
        var axioms = profile?.Axioms;

        return new RouteInspectData
        {
            Id = identity?.Id,
            Path = identity?.CanonicalWorkspaceRelativePath,
            Kind = identity is null ? null : RouteInspectWording.SourceKind(identity.Kind),
            EntrypointForm = identity is null ? null : RouteInspectWording.SourceForm(identity.Form),
            OverwritePath = identity?.PhysicalLayers.Count > 1
                ? identity.PhysicalLayers[1].WorkspaceRelativePath
                : null,
            Belongs = new RouteInspectDataBelongs
            {
                RouteChain = topologyValue?.RouteChain.ToArray(),
                Parent = topologyValue?.ParentId,
                DirectChildren = topologyValue is null ? null : ProjectCounts(topologyValue.Counts, direct: true),
                Descendants = topologyValue is null ? null : ProjectCounts(topologyValue.Counts, direct: false),
            },
            Read = new RouteInspectDataRead
            {
                AtStart = ProjectBoolean(profile?.Reading.TaskStart),
                AutomaticallyWhen = ProjectAutomatic(automatic),
                MayReadAgain = later is { State: RouteInspectFactState.Value }
                    ? later.ReadValue().MayBeReadAgain
                    : null,
            },
            Size = new RouteInspectDataSize
            {
                Own = ProjectMeasurement(profile?.Measurements.OwnSource),
                Adds = ProjectMeasurement(profile?.Measurements.SelectionAddition),
                LoadNow = ProjectMeasurement(profile?.Measurements.LoadNowDescendants),
            },
            Axioms = standard ? ProjectAxioms(axioms) : null,
            Tags = standard && identity is not null ? identity.Tags.ToArray() : null,
            Selected = full && profile is not null
                ? new RouteInspectDataSelected
                {
                    Closure = ProjectMeasurement(profile.Measurements.SelectedClosure),
                    StartupOverlap = ProjectMeasurement(profile.Measurements.TaskStartOverlap),
                }
                : null,
            Selection = full
                ? new RouteInspectDataSelection
                {
                    Kind = RouteInspectWording.ReferenceKind(result.Selection.ReferenceKind),
                    Method = RouteInspectWording.SelectionMethod(result.Selection.SelectionMethod),
                    Requested = result.Selection.RequestedReference,
                }
                : null,
            Layers = full && identity is not null
                ? identity.PhysicalLayers.Select(layer => new RouteInspectDataLayer
                {
                    Path = layer.WorkspaceRelativePath,
                    Kind = RouteInspectWording.LayerRole(layer.Role),
                }).ToArray()
                : null,
            StatusReason = full ? RouteInspectWording.StatusReason(result.Status) : null,
            Identity = identity,
            Profile = profile,
        };
    }

    private static RouteInspectDataCounts? ProjectCounts(
        RouteInspectFact<RouteInspectTopologyCounts> fact,
        bool direct)
    {
        if (fact.State != RouteInspectFactState.Value)
        {
            return new RouteInspectDataCounts();
        }

        var value = fact.ReadValue();
        return direct
            ? new RouteInspectDataCounts
            {
                Files = value.DirectRoutedFileCount,
                Entrypoints = value.DirectEntrypointCount,
            }
            : new RouteInspectDataCounts
            {
                Files = value.DescendantRoutedFileCount,
                Entrypoints = value.DescendantEntrypointCount,
            };
    }

    private static bool? ProjectBoolean(RouteInspectFact<bool>? fact)
        => fact?.State == RouteInspectFactState.Value ? fact.ReadValue() : null;

    private static IReadOnlyList<string>? ProjectAutomatic(
        RouteInspectFact<RouteInspectAutomaticReadings>? fact)
        => fact?.State == RouteInspectFactState.Value
            ? fact.ReadValue().Reasons.Select(RouteInspectWording.AutomaticExplanation).ToArray()
            : null;

    private static RouteInspectDataMeasurement? ProjectMeasurement(
        RouteInspectFact<RouteInspectMeasurement>? fact)
    {
        if (fact?.State != RouteInspectFactState.Value)
        {
            return null;
        }

        var measurement = fact.ReadValue();
        return new RouteInspectDataMeasurement
        {
            Files = measurement.PhysicalFileCount,
            Bytes = measurement.Utf8ByteCount,
            Tokens = measurement.EstimatedTokens,
        };
    }

    private static RouteInspectDataAxioms? ProjectAxioms(
        RouteInspectFact<RouteInspectAxiomsProfile>? fact)
    {
        if (fact is null)
        {
            return null;
        }

        if (fact.State != RouteInspectFactState.Value)
        {
            return new RouteInspectDataAxioms();
        }

        var value = fact.ReadValue();
        return new RouteInspectDataAxioms
        {
            InheritedFrom = value.Inherited.State == RouteInspectFactState.Value
                ? value.Inherited.ReadValue().SourceIds.ToArray()
                : null,
            Local = value.Local.State == RouteInspectFactState.Value
                ? RouteInspectWording.LocalAxioms(value.Local.ReadValue())
                : null,
        };
    }

    private static CliHeadline Headline(RouteInspectResult result)
    {
        if (result.Identity is not null
            && (result.Status is CliSemanticStatus.Complete
                or CliSemanticStatus.Attention
                or CliSemanticStatus.Incomplete))
        {
            var identity = result.Identity
                ?? throw new InvalidOperationException("A resolved route-inspect status requires an identity.");
            var kind = result.Status switch
            {
                CliSemanticStatus.Complete => CliHeadlineKind.Done,
                CliSemanticStatus.Attention => CliHeadlineKind.Warnings,
                CliSemanticStatus.Incomplete => CliHeadlineKind.Incomplete,
                _ => throw new ArgumentOutOfRangeException(nameof(result)),
            };
            return new($"{identity.Id}  {identity.CanonicalWorkspaceRelativePath}", kind);
        }

        var condition = result.Conditions.FirstOrDefault(condition => condition.Status == result.Status)
            ?? result.Conditions.FirstOrDefault()
            ?? throw new InvalidOperationException("A non-resolved route-inspect result requires a condition.");
        var reference = result.Selection.RequestedReference ?? condition.Subject;
        return result.Status switch
        {
            CliSemanticStatus.Invalid => new(
                RouteInspectWording.CannotInspect(reference, ConditionMessage(result, condition)),
                CliHeadlineKind.CannotStart),
            CliSemanticStatus.Blocked => new(
                RouteInspectWording.CannotInspect(reference, ConditionMessage(result, condition)),
                CliHeadlineKind.Blocked),
            CliSemanticStatus.Failed => new(RouteInspectWording.Failed(condition.Message), CliHeadlineKind.Failed),
            CliSemanticStatus.Interrupted => new(RouteInspectWording.Cancelled(), CliHeadlineKind.Cancelled),
            _ => throw new ArgumentOutOfRangeException(nameof(result), result.Status, "The route-inspect status is not defined."),
        };
    }

    private static string? HeadlineFindingCode(RouteInspectResult result)
    {
        if (result.Status is not (
                CliSemanticStatus.Invalid
                or CliSemanticStatus.Blocked
                or CliSemanticStatus.Failed
                or CliSemanticStatus.Interrupted))
        {
            return null;
        }

        return result.Conditions.FirstOrDefault(condition => condition.Status == result.Status)?.MachineCode
            ?? result.Conditions.FirstOrDefault()?.MachineCode;
    }

    private static CliFinding Finding(RouteInspectResult result, RouteInspectObservation observation)
    {
        var message = ObservationMessage(result, observation);
        var subject = observation.Subject;
        return new CliFinding
        {
            Severity = observation.Code == RouteInspectObservationCode.AutomaticIdNotUnique
                ? CliSeverity.Warning
                : CliSeverity.Info,
            Code = observation.MachineCode,
            Title = ObservationTitle(observation.Code),
            Message = message,
            Subject = new CliSubject(CliSubjectKind.Source, subject, subject),
            Candidates = observation.Code == RouteInspectObservationCode.AutomaticIdNotUnique
                ? Candidates(observation.Paths, result.Identity?.CanonicalWorkspaceRelativePath)
                : [],
        };
    }

    private static CliFinding Finding(RouteInspectResult result, RouteInspectCondition condition)
    {
        var subject = condition.Subject;
        var subjectKind = condition.Code switch
        {
            RouteInspectConditionCode.InvalidWorkspace
                or RouteInspectConditionCode.WorkspaceUnavailable
                or RouteInspectConditionCode.UnsafeWorkspace => CliSubjectKind.Workspace,
            RouteInspectConditionCode.InvalidSourceReference
                or RouteInspectConditionCode.MissingSource
                or RouteInspectConditionCode.MultipleSources
                or RouteInspectConditionCode.UnknownSource => CliSubjectKind.Identifier,
            _ => CliSubjectKind.Source,
        };
        return new CliFinding
        {
            Severity = CliReportVocabulary.Severity(condition.Status),
            Code = condition.MachineCode,
            Title = ConditionTitle(condition.Code),
            Message = ConditionMessage(result, condition),
            Subject = new CliSubject(subjectKind, subjectKind == CliSubjectKind.Source ? subject : null,
                subjectKind == CliSubjectKind.Source ? subject : subject),
            Candidates = Candidates(condition.Paths, subject),
        };
    }

    private static string ObservationMessage(RouteInspectResult result, RouteInspectObservation observation)
        => observation.Code switch
        {
            RouteInspectObservationCode.AutomaticIdNotUnique => RouteInspectWording.AutomaticIdNotUnique(
                observation.Subject,
                observation.Paths.FirstOrDefault(path => path != result.Identity?.CanonicalWorkspaceRelativePath)
                    ?? global::OpenForge.Cli.OutputText.Route.Inspect.RouteInspectText.LabelAnotherSource()),
            RouteInspectObservationCode.CompatibilityEntrypoint => RouteInspectWording.CompatibilityEntrypoint(
                observation.Subject,
                RouteInspectWording.EntryPointName(observation.Subject),
                RouteInspectWording.EntryPointFolder(observation.Subject)),
            RouteInspectObservationCode.DetachedSource => RouteInspectWording.Detached(observation.Subject),
            RouteInspectObservationCode.NotRouted => RouteInspectWording.NotRouted(observation.Subject),
            RouteInspectObservationCode.ValidOverwrite => result.Identity?.PhysicalLayers is { Count: > 1 } layers
                ? RouteInspectWording.ValidOverwrite(layers[1].WorkspaceRelativePath, layers[0].WorkspaceRelativePath)
                : observation.Message,
            _ => throw new ArgumentOutOfRangeException(nameof(observation), observation.Code, "The observation code is not defined."),
        };

    private static string ConditionMessage(RouteInspectResult result, RouteInspectCondition condition)
        => condition.Code switch
        {
            RouteInspectConditionCode.InvalidWorkspace
                or RouteInspectConditionCode.WorkspaceUnavailable => SharedWorkspaceUnavailable(condition.Subject),
            RouteInspectConditionCode.UnsafeWorkspace => SharedWorkspaceUnsafe(condition),
            RouteInspectConditionCode.MissingSource => RouteInspectWording.MissingSource(),
            RouteInspectConditionCode.MultipleSources => RouteInspectWording.MultipleSources(),
            RouteInspectConditionCode.InvalidSourceReference => RouteInspectWording.InvalidSourceReference(condition.Subject),
            RouteInspectConditionCode.LoaderSubject => RouteInspectWording.LoaderSubject(),
            RouteInspectConditionCode.UnknownSource => RouteInspectWording.UnknownSource(
                result.Selection.RequestedReference ?? condition.Subject),
            RouteInspectConditionCode.MissingSourceFile => RouteInspectWording.MissingSourceFile(condition.Subject),
            RouteInspectConditionCode.UnsupportedSource => RouteInspectWording.UnsupportedSource(condition.Subject),
            RouteInspectConditionCode.AmbiguousSource => CliFindingWording.SourceAmbiguous(condition.Subject),
            RouteInspectConditionCode.UnsafeSource => CliFindingWording.SourceUnsafe(condition.Subject),
            RouteInspectConditionCode.AmbiguousRoute => CliFindingWording.RouteAmbiguous(condition.Subject),
            RouteInspectConditionCode.OrphanOverwrite => RouteInspectWording.OrphanOverwrite(condition.Subject),
            RouteInspectConditionCode.AmbiguousOverwrite => RouteInspectWording.AmbiguousOverwrite(condition.Subject),
            RouteInspectConditionCode.UnreadableSource => RouteInspectWording.InspectionIncomplete(condition.Subject),
            RouteInspectConditionCode.IncompleteRoute => RouteInspectWording.IncompleteRoute(condition.Subject, condition.Message),
            RouteInspectConditionCode.UnavailableFact => RouteInspectWording.UnavailableFact(
                condition.Message.Contains("own-source", StringComparison.OrdinalIgnoreCase)
                    ? global::OpenForge.Cli.OutputText.Route.Inspect.RouteInspectText.TitleThisFile()
                    : global::OpenForge.Cli.OutputText.Route.Inspect.RouteInspectText.TitleRouteFacts(),
                condition.Message),
            RouteInspectConditionCode.OperationFailed => RouteInspectWording.Failed(condition.Message),
            RouteInspectConditionCode.Interrupted => RouteInspectWording.Cancelled(),
            _ => throw new ArgumentOutOfRangeException(nameof(condition), condition.Code, "The condition code is not defined."),
        };

    private static string SharedWorkspaceUnavailable(string path)
        => global::OpenForge.Cli.OutputText.Route.Inspect.RouteInspectPhrases.FormatCannotUseAsTheWorkspaceItDoesNotExistOrCannotBeRead($"{path}");

    private static string SharedWorkspaceUnsafe(RouteInspectCondition condition)
        => global::OpenForge.Cli.OutputText.Route.Inspect.RouteInspectPhrases.FormatCannotUseAsTheWorkspaceItsLocationCouldNotBeVerified($"{condition.Subject}", $"{RouteInspectWording.TrimSentence(condition.Message)}");

    private static string ObservationTitle(RouteInspectObservationCode code)
        => code switch
        {
            RouteInspectObservationCode.AutomaticIdNotUnique => global::OpenForge.Cli.OutputText.Route.Inspect.RouteInspectText.TitleIdIsNotUnique(),
            RouteInspectObservationCode.CompatibilityEntrypoint => global::OpenForge.Cli.OutputText.Route.Shared.RouteSharedText.TitleCompatibilityEntrypoint(),
            RouteInspectObservationCode.DetachedSource => global::OpenForge.Cli.OutputText.Route.Inspect.RouteInspectText.TitleSourceIsDetached(),
            RouteInspectObservationCode.NotRouted => global::OpenForge.Cli.OutputText.Route.Inspect.RouteInspectText.TitleSourceIsNotRouted(),
            RouteInspectObservationCode.ValidOverwrite => global::OpenForge.Cli.OutputText.Route.Inspect.RouteInspectText.TitleOverwriteIsValid(),
            _ => throw new ArgumentOutOfRangeException(nameof(code), code, "The observation code is not defined."),
        };

    private static string ConditionTitle(RouteInspectConditionCode code)
        => code switch
        {
            RouteInspectConditionCode.InvalidWorkspace
                or RouteInspectConditionCode.WorkspaceUnavailable => global::OpenForge.Cli.OutputText.Shared.SharedText.TitleWorkspaceIsUnavailable(),
            RouteInspectConditionCode.UnsafeWorkspace => global::OpenForge.Cli.OutputText.Shared.SharedText.TitleWorkspaceIsUnsafe(),
            RouteInspectConditionCode.MissingSource
                or RouteInspectConditionCode.UnknownSource => global::OpenForge.Cli.OutputText.Shared.SharedText.TitleSourceIsUnknown(),
            RouteInspectConditionCode.MultipleSources => global::OpenForge.Cli.OutputText.Route.Inspect.RouteInspectText.TitleTooManySources(),
            RouteInspectConditionCode.InvalidSourceReference => global::OpenForge.Cli.OutputText.Route.Shared.RouteSharedText.TitleInvalidSourceReference(),
            RouteInspectConditionCode.LoaderSubject => global::OpenForge.Cli.OutputText.Route.Inspect.RouteInspectText.TitleLoaderIsNotARoute(),
            RouteInspectConditionCode.MissingSourceFile => global::OpenForge.Cli.OutputText.Route.Inspect.RouteInspectText.TitleSourceFileIsMissing(),
            RouteInspectConditionCode.UnsupportedSource => global::OpenForge.Cli.OutputText.Route.Shared.RouteSharedText.TitleSourceIsUnsupported(),
            RouteInspectConditionCode.AmbiguousSource => global::OpenForge.Cli.OutputText.Shared.SharedText.TitleSourceIsAmbiguous(),
            RouteInspectConditionCode.UnsafeSource => global::OpenForge.Cli.OutputText.Shared.SharedText.TitleSourceIsUnsafe(),
            RouteInspectConditionCode.AmbiguousRoute => global::OpenForge.Cli.OutputText.Route.Shared.RouteSharedText.TitleRouteIsAmbiguous(),
            RouteInspectConditionCode.OrphanOverwrite => global::OpenForge.Cli.OutputText.Route.Inspect.RouteInspectText.TitleOverwriteIsOrphaned(),
            RouteInspectConditionCode.AmbiguousOverwrite => global::OpenForge.Cli.OutputText.Shared.SharedText.TitleOverwriteIsAmbiguous(),
            RouteInspectConditionCode.UnreadableSource => global::OpenForge.Cli.OutputText.Shared.SharedText.TitleSourceCouldNotBeRead(),
            RouteInspectConditionCode.IncompleteRoute => global::OpenForge.Cli.OutputText.Route.Inspect.RouteInspectText.TitleRouteIsIncomplete(),
            RouteInspectConditionCode.UnavailableFact => global::OpenForge.Cli.OutputText.Route.Inspect.RouteInspectText.TitleRouteFactIsUnavailable(),
            RouteInspectConditionCode.OperationFailed => global::OpenForge.Cli.OutputText.Route.Inspect.RouteInspectText.TitleRouteInspectFailed(),
            RouteInspectConditionCode.Interrupted => global::OpenForge.Cli.OutputText.Route.Inspect.RouteInspectText.TitleRouteInspectWasCancelled(),
            _ => throw new ArgumentOutOfRangeException(nameof(code), code, "The condition code is not defined."),
        };

    private static IReadOnlyList<CliCandidate> Candidates(
        IReadOnlyList<string> paths,
        string? selectedPath)
        => paths
            .Where(path => selectedPath is null || path != selectedPath)
            .Select(path => new CliCandidate(
                new CliSubject(CliSubjectKind.Source, path, path),
                [global::OpenForge.Cli.OutputText.Route.Shared.RouteSharedText.LabelMatchesTheRequestedSourceReference()]))
            .ToArray();

    private static IReadOnlyList<CliCount> Counts(RouteInspectProfile? profile)
    {
        var measurements = profile?.Measurements;
        var topology = profile?.Topology;
        var topologyCounts = topology is { State: RouteInspectFactState.Value }
            ? topology.ReadValue().Counts
            : null;
        return
        [
            new CliCount("ownBytes", global::OpenForge.Cli.OutputText.Route.Inspect.RouteInspectText.LabelOwnBytes(), Value(measurements?.OwnSource, measurement => measurement.Utf8ByteCount)),
            new CliCount("ownTokens", global::OpenForge.Cli.OutputText.Route.Inspect.RouteInspectText.LabelOwnTokens(), Value(measurements?.OwnSource, measurement => measurement.EstimatedTokens)),
            new CliCount("addedFiles", global::OpenForge.Cli.OutputText.Route.Inspect.RouteInspectText.LabelAddedFiles(), Value(measurements?.SelectionAddition, measurement => measurement.PhysicalFileCount)),
            new CliCount("addedBytes", global::OpenForge.Cli.OutputText.Route.Inspect.RouteInspectText.LabelAddedBytes(), Value(measurements?.SelectionAddition, measurement => measurement.Utf8ByteCount)),
            new CliCount("addedTokens", global::OpenForge.Cli.OutputText.Route.Inspect.RouteInspectText.LabelAddedTokens(), Value(measurements?.SelectionAddition, measurement => measurement.EstimatedTokens)),
            new CliCount("loadNowFiles", global::OpenForge.Cli.OutputText.Route.Inspect.RouteInspectText.TitleLoadNowFiles(), Value(measurements?.LoadNowDescendants, measurement => measurement.PhysicalFileCount)),
            new CliCount("loadNowBytes", global::OpenForge.Cli.OutputText.Route.Inspect.RouteInspectText.TitleLoadNowBytes(), Value(measurements?.LoadNowDescendants, measurement => measurement.Utf8ByteCount)),
            new CliCount("loadNowTokens", global::OpenForge.Cli.OutputText.Route.Inspect.RouteInspectText.TitleLoadNowTokens(), Value(measurements?.LoadNowDescendants, measurement => measurement.EstimatedTokens)),
            new CliCount("directChildren", global::OpenForge.Cli.OutputText.Route.Inspect.RouteInspectText.LabelDirectChildren(), topologyCounts is { State: RouteInspectFactState.Value }
                ? topologyCounts.ReadValue().DirectRoutedFileCount + topologyCounts.ReadValue().DirectEntrypointCount
                : null),
            new CliCount("descendants", global::OpenForge.Cli.OutputText.Route.Inspect.RouteInspectText.LabelDescendants(), topologyCounts is { State: RouteInspectFactState.Value }
                ? topologyCounts.ReadValue().DescendantRoutedFileCount + topologyCounts.ReadValue().DescendantEntrypointCount
                : null),
        ];
    }

    private static long? Value(
        RouteInspectFact<RouteInspectMeasurement>? fact,
        Func<RouteInspectMeasurement, long> read)
        => fact?.State == RouteInspectFactState.Value ? read(fact.ReadValue()) : null;

    private static IReadOnlyList<string> Diagnostics(RouteInspectResult result)
        =>
        [
            $"status={CliStatusDefinitions.Read(result.Status).MachineName}",
            $"referenceKind={RouteInspectWording.ReferenceKind(result.Selection.ReferenceKind)}",
            $"selectionMethod={RouteInspectWording.SelectionMethod(result.Selection.SelectionMethod)}",
            $"requested={result.Selection.RequestedReference ?? "none"}",
            $"identity={(result.Identity is null ? "none" : "present")}",
            $"profile={(result.Profile is null ? "none" : "present")}",
            $"observations={result.Observations.Count}",
            $"conditions={result.Conditions.Count}",
            $"next={(result.Next is null ? "none" : "present")}",
        ];
}
