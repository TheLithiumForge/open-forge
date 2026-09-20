using System.Globalization;
using OpenForge.Cli.Core.Commands.Route.Inspect.Models.Profile;
using OpenForge.Cli.Core.Commands.Route.Inspect.Models.Resolution;
using OpenForge.Cli.Core.Commands.Route.Inspect.Models.Result;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.Core.Presentation.Route.Inspect.Shared.Wording;

internal static class RouteInspectWording
{
    internal const string BelongsHeading = "Where this source belongs";
    internal const string ReadingHeading = "When it is read";
    internal const string ContextSizeHeading = "Context size";
    internal const string AxiomsHeading = "Axioms";
    internal const string PhysicalLayersHeading = "Physical layers";

    internal static string SourceKind(RouteInspectSourceKind kind)
        => kind switch
        {
            RouteInspectSourceKind.Entrypoint => "entrypoint",
            RouteInspectSourceKind.Markdown => "markdown",
            RouteInspectSourceKind.Native => "native",
            _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, "The source kind is not defined."),
        };

    internal static string SourceForm(RouteInspectSourceForm form)
        => form switch
        {
            RouteInspectSourceForm.CanonicalEntrypoint => "canonical",
            RouteInspectSourceForm.CompatibilityEntrypoint => "compatibility",
            RouteInspectSourceForm.Markdown => "markdown",
            RouteInspectSourceForm.Native => "native",
            _ => throw new ArgumentOutOfRangeException(nameof(form), form, "The source form is not defined."),
        };

    internal static string ReferenceKind(RouteInspectReferenceKind kind)
        => kind switch
        {
            RouteInspectReferenceKind.Missing => "missing",
            RouteInspectReferenceKind.SourceId => "source-id",
            RouteInspectReferenceKind.SourcePath => "source-path",
            RouteInspectReferenceKind.Invalid => "invalid",
            _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, "The reference kind is not defined."),
        };

    internal static string SelectionMethod(RouteInspectSelectionMethod method)
        => method switch
        {
            RouteInspectSelectionMethod.Unresolved => "unresolved",
            RouteInspectSelectionMethod.AutomaticId => "automatic-id",
            RouteInspectSelectionMethod.ExactPath => "exact-path",
            RouteInspectSelectionMethod.Interactive => "interactive",
            _ => throw new ArgumentOutOfRangeException(nameof(method), method, "The selection method is not defined."),
        };

    internal static string LayerRole(RouteInspectLayerRole role)
        => role switch
        {
            RouteInspectLayerRole.Base => "base",
            RouteInspectLayerRole.Overwrite => "overwrite",
            _ => throw new ArgumentOutOfRangeException(nameof(role), role, "The layer role is not defined."),
        };

    internal static string AutomaticReadingKind(RouteInspectAutomaticReadingKind kind)
        => kind switch
        {
            RouteInspectAutomaticReadingKind.OnDemand => "on-demand",
            RouteInspectAutomaticReadingKind.ParentLoadNow => "parent-load-now",
            RouteInspectAutomaticReadingKind.EntrypointKeepInMind => "entrypoint-keep-in-mind",
            RouteInspectAutomaticReadingKind.RoutedFileKeepInMind => "routed-file-keep-in-mind",
            RouteInspectAutomaticReadingKind.OverwriteAfterBase => "overwrite-after-base",
            _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, "The automatic-reading kind is not defined."),
        };

    internal static string ReadingEvent(RouteInspectAutomaticReadingEvent readingEvent)
        => readingEvent switch
        {
            RouteInspectAutomaticReadingEvent.RouteSelected => "route-selected",
            RouteInspectAutomaticReadingEvent.ExposingParentRead => "exposing-parent-read",
            RouteInspectAutomaticReadingEvent.TaskStartVisible => "task-start-visible",
            RouteInspectAutomaticReadingEvent.ScopeSelected => "scope-selected",
            RouteInspectAutomaticReadingEvent.AncestorRequired => "ancestor-required",
            RouteInspectAutomaticReadingEvent.TaskReview => "task-review",
            RouteInspectAutomaticReadingEvent.LaterReview => "later-review",
            RouteInspectAutomaticReadingEvent.BaseRead => "base-read",
            _ => throw new ArgumentOutOfRangeException(nameof(readingEvent), readingEvent, "The reading event is not defined."),
        };

    internal static string LaterOccasion(RouteInspectLaterReadOccasion occasion)
        => occasion switch
        {
            RouteInspectLaterReadOccasion.ContextRestoration => "context-restoration",
            RouteInspectLaterReadOccasion.Handoff => "handoff",
            RouteInspectLaterReadOccasion.Closeout => "closeout",
            RouteInspectLaterReadOccasion.FollowupTransition => "followup-transition",
            _ => throw new ArgumentOutOfRangeException(nameof(occasion), occasion, "The later-read occasion is not defined."),
        };

    internal static string AutomaticExplanation(RouteInspectAutomaticReading reading)
    {
        if (reading.Kind == RouteInspectAutomaticReadingKind.ParentLoadNow)
        {
            var relatedSourceId = reading.RelatedSourceId
                ?? throw new InvalidOperationException("A parent-load automatic reading requires a related source ID.");
            return relatedSourceId is "loader" or ".agents/loader.md"
                ? global::OpenForge.Cli.OutputText.Route.Inspect.RouteInspectText.LabelTheLoaderIsRead()
                : global::OpenForge.Cli.OutputText.Route.Inspect.RouteInspectPhrases.FormatIsRead($"{relatedSourceId}");
        }

        return reading.Kind switch
        {
            RouteInspectAutomaticReadingKind.EntrypointKeepInMind
                or RouteInspectAutomaticReadingKind.RoutedFileKeepInMind => Events(reading.Events),
            RouteInspectAutomaticReadingKind.OnDemand => global::OpenForge.Cli.OutputText.Route.Inspect.RouteInspectText.LabelThisRouteIsSelected(),
            RouteInspectAutomaticReadingKind.OverwriteAfterBase => global::OpenForge.Cli.OutputText.Route.Inspect.RouteInspectText.LabelTheBaseSourceIsRead(),
            _ => throw new ArgumentOutOfRangeException(nameof(reading), reading.Kind, "The automatic-reading kind is not defined."),
        };
    }

    internal static string AutomaticReadLine(RouteInspectAutomaticReading reading)
        => reading.Kind == RouteInspectAutomaticReadingKind.OnDemand
            ? global::OpenForge.Cli.OutputText.Route.Inspect.RouteInspectText.TitleReadWhenThisRouteIsSelectedYes()
            : global::OpenForge.Cli.OutputText.Route.Inspect.RouteInspectPhrases.FormatReadAutomaticallyWhen($"{AutomaticExplanation(reading)}");

    internal static string LaterDescription(IReadOnlyList<RouteInspectLaterReadOccasion> occasions)
    {
        var descriptions = new List<string>();
        if (occasions.Contains(RouteInspectLaterReadOccasion.ContextRestoration))
        {
            descriptions.Add(global::OpenForge.Cli.OutputText.Route.Inspect.RouteInspectText.LabelAfterContextRestoration());
        }

        if (occasions.Contains(RouteInspectLaterReadOccasion.Handoff)
            || occasions.Contains(RouteInspectLaterReadOccasion.Closeout))
        {
            descriptions.Add(global::OpenForge.Cli.OutputText.Route.Inspect.RouteInspectText.LabelBeforeHandoffOrCloseout());
        }

        if (occasions.Contains(RouteInspectLaterReadOccasion.FollowupTransition))
        {
            descriptions.Add(global::OpenForge.Cli.OutputText.Route.Inspect.RouteInspectText.LabelAfterAChangeThatMayAffectItsFollowUpWork());
        }

        return descriptions.Count switch
        {
            0 => global::OpenForge.Cli.OutputText.Route.Inspect.RouteInspectText.LabelAtTheDefinedLaterReviewOccasions(),
            1 => descriptions[0],
            _ => string.Join(", ", descriptions.Take(descriptions.Count - 1)) + global::OpenForge.Cli.OutputText.Route.Inspect.RouteInspectPhrases.ReadingFinalAlternative($"{descriptions[^1]}"),
        };
    }

    internal static string LocalAxioms(RouteInspectAxiomsLocalState state)
        => state switch
        {
            RouteInspectAxiomsLocalState.Substantive => global::OpenForge.Cli.OutputText.Route.Inspect.RouteInspectText.LabelSubstantiveLocalAxioms(),
            RouteInspectAxiomsLocalState.InheritedSentinel => global::OpenForge.Cli.OutputText.Route.Inspect.RouteInspectText.LabelInheritsAncestorAxiomsWithoutLocalRules(),
            RouteInspectAxiomsLocalState.Empty => global::OpenForge.Cli.OutputText.Route.Inspect.RouteInspectText.LabelEmptyLocalAxioms(),
            RouteInspectAxiomsLocalState.Missing => global::OpenForge.Cli.OutputText.Route.Inspect.RouteInspectText.LabelNoLocalAxiomsSection(),
            RouteInspectAxiomsLocalState.NotApplicable => global::OpenForge.Cli.OutputText.Shared.SharedText.LabelNotApplicable(),
            _ => throw new ArgumentOutOfRangeException(nameof(state), state, "The local Axioms state is not defined."),
        };

    internal static string StatusReason(CliSemanticStatus status)
        => status switch
        {
            CliSemanticStatus.Complete => global::OpenForge.Cli.OutputText.Route.Inspect.RouteInspectText.LabelAllApplicableRouteFactsAreAvailable(),
            CliSemanticStatus.Attention => global::OpenForge.Cli.OutputText.Route.Inspect.RouteInspectText.LabelTheSourceIsSafeAndCompleteButItsAutomaticIdIsNotUnique(),
            CliSemanticStatus.Incomplete => global::OpenForge.Cli.OutputText.Route.Inspect.RouteInspectText.LabelARequiredRouteFactOrMeasurementIsUnavailable(),
            CliSemanticStatus.Invalid => global::OpenForge.Cli.OutputText.Route.Inspect.RouteInspectText.LabelTheSourceReferenceWasNotAccepted(),
            CliSemanticStatus.Blocked => global::OpenForge.Cli.OutputText.Route.Inspect.RouteInspectText.LabelASafeSourceOrRouteBoundaryCouldNotBeEstablished(),
            CliSemanticStatus.Failed => global::OpenForge.Cli.OutputText.Route.Inspect.RouteInspectText.LabelAnUnexpectedFailureStoppedRouteInspection(),
            CliSemanticStatus.Interrupted => global::OpenForge.Cli.OutputText.Route.Inspect.RouteInspectText.LabelRouteInspectionWasInterruptedBeforeCompletion(),
            _ => throw new ArgumentOutOfRangeException(nameof(status), status, "The status is not defined."),
        };

    internal static string CannotInspect(string reference, string problem)
        => global::OpenForge.Cli.OutputText.Shared.SharedPhrases.FormatCannotInspect($"{reference}", $"{TrimSentence(problem)}");

    internal static string UnknownSource(string reference)
        => global::OpenForge.Cli.OutputText.Route.Inspect.RouteInspectWording.UnknownSource(reference);

    internal static string MissingSource()
        => global::OpenForge.Cli.OutputText.Route.Inspect.RouteInspectText.MessageASourceReferenceIsRequired();

    internal static string MultipleSources()
        => global::OpenForge.Cli.OutputText.Route.Inspect.RouteInspectText.MessageRouteInspectTakesOneSource();

    internal static string InvalidSourceReference(string operand)
        => global::OpenForge.Cli.OutputText.Route.Inspect.RouteInspectWording.InvalidSourceReference(operand);

    internal static string LoaderSubject()
        => global::OpenForge.Cli.OutputText.Route.Inspect.RouteInspectText.MessageTheLoaderIsNotARouteInspectOneOfItsRoutesInstead();

    internal static string MissingSourceFile(string path)
        => global::OpenForge.Cli.OutputText.Route.Inspect.RouteInspectWording.MissingSourceFile(path);

    internal static string UnsupportedSource(string path)
        => global::OpenForge.Cli.OutputText.Route.Inspect.RouteInspectWording.UnsupportedSource(path);

    internal static string OrphanOverwrite(string path)
        => global::OpenForge.Cli.OutputText.Route.Inspect.RouteInspectPhrases.FormatHasNoBesideIt($"{FileName(path)}", $"{BaseName(path)}");

    internal static string AmbiguousOverwrite(string path)
        => global::OpenForge.Cli.OutputText.Shared.SharedPhrases.FormatCouldBelongToMoreThanOneBaseFile($"{FileName(path)}");

    internal static string IncompleteRoute(string path, string reason)
        => global::OpenForge.Cli.OutputText.Route.Inspect.RouteInspectPhrases.FormatTheRouteChainAboveCouldNotBeEstablishedCompletely($"{path}", $"{TrimSentence(reason)}");

    internal static string UnavailableFact(string fact, string reason)
        => global::OpenForge.Cli.OutputText.Route.Inspect.RouteInspectPhrases.FormatCouldNotBeMeasured($"{fact}", $"{TrimSentence(reason)}");

    internal static string AutomaticIdNotUnique(string id, string other)
        => global::OpenForge.Cli.OutputText.Route.Inspect.RouteInspectWording.AutomaticIdNotUnique(id, other);

    internal static string CompatibilityEntrypoint(string path, string name, string folder)
        => global::OpenForge.Cli.OutputText.Route.Inspect.RouteInspectWording.CompatibilityEntrypoint(path, name, folder);

    internal static string NotRouted(string path)
        => global::OpenForge.Cli.OutputText.Route.Inspect.RouteInspectWording.NotRouted(path);

    internal static string Detached(string path)
        => global::OpenForge.Cli.OutputText.Route.Inspect.RouteInspectWording.Detached(path);

    internal static string ValidOverwrite(string path, string basePath)
        => global::OpenForge.Cli.OutputText.Route.Inspect.RouteInspectWording.ValidOverwrite(path, basePath);

    internal static string Failed(string reason)
        => global::OpenForge.Cli.OutputText.Route.Inspect.RouteInspectPhrases.FormatRouteInspectStoppedBecauseOfAnUnexpectedError($"{TrimSentence(reason)}");

    internal static string Cancelled()
        => global::OpenForge.Cli.OutputText.Route.Inspect.RouteInspectText.MessageRouteInspectWasCancelled();

    internal static string InspectionIncomplete(string path)
        => global::OpenForge.Cli.OutputText.Route.Inspect.RouteInspectWording.InspectionIncomplete(path);

    internal static string EntryPointName(string path)
        => Path.GetFileName(path);

    internal static string EntryPointFolder(string path)
    {
        var directory = Path.GetDirectoryName(path)?.Replace('\\', '/')
            ?? throw new InvalidOperationException("A compatibility entrypoint requires a containing folder.");
        return directory[(directory.LastIndexOf('/') + 1)..];
    }

    internal static string Bytes(long bytes)
    {
        if (bytes < 1024)
        {
            return $"{bytes} B";
        }

        if (bytes < 1024 * 1024)
        {
            return global::OpenForge.Cli.OutputText.Route.Inspect.RouteInspectPhrases.FormatKiB(string.Create(global::System.Globalization.CultureInfo.InvariantCulture, $"{bytes / 1024d:0.##}"));
        }

        return global::OpenForge.Cli.OutputText.Route.Inspect.RouteInspectPhrases.FormatMiB(string.Create(global::System.Globalization.CultureInfo.InvariantCulture, $"{bytes / (1024d * 1024d):0.##}"));
    }

    internal static string TrimSentence(string value)
        => value.TrimEnd('.');

    private static string Events(IReadOnlyList<RouteInspectAutomaticReadingEvent> events)
    {
        var descriptions = events.Select(Event).ToArray();
        return descriptions.Length switch
        {
            0 => global::OpenForge.Cli.OutputText.Route.Inspect.RouteInspectText.LabelTheDefinedAutomaticReadingEvents(),
            1 => descriptions[0],
            _ => global::OpenForge.Cli.OutputText.Route.Inspect.RouteInspectPhrases.ReadingAlternatives($"{string.Join(", ", descriptions[..^1])}", $"{descriptions[^1]}"),
        };
    }

    private static string Event(RouteInspectAutomaticReadingEvent readingEvent)
        => readingEvent switch
        {
            RouteInspectAutomaticReadingEvent.ExposingParentRead => global::OpenForge.Cli.OutputText.Route.Inspect.RouteInspectText.LabelItsExposingParentIsRead(),
            RouteInspectAutomaticReadingEvent.RouteSelected => global::OpenForge.Cli.OutputText.Route.Inspect.RouteInspectText.LabelItsRouteIsSelected(),
            RouteInspectAutomaticReadingEvent.TaskStartVisible => global::OpenForge.Cli.OutputText.Route.Inspect.RouteInspectText.LabelItIsVisibleFromTaskStartRouting(),
            RouteInspectAutomaticReadingEvent.ScopeSelected => global::OpenForge.Cli.OutputText.Route.Inspect.RouteInspectText.LabelItsScopeIsSelected(),
            RouteInspectAutomaticReadingEvent.AncestorRequired => global::OpenForge.Cli.OutputText.Route.Inspect.RouteInspectText.LabelItIsNeededAsAnAncestor(),
            RouteInspectAutomaticReadingEvent.TaskReview => global::OpenForge.Cli.OutputText.Route.Inspect.RouteInspectText.LabelATaskReviewPointIsReachedWhileItsScopeIsActive(),
            RouteInspectAutomaticReadingEvent.LaterReview => global::OpenForge.Cli.OutputText.Route.Inspect.RouteInspectText.LabelALaterReviewPointIsReachedWhileItsScopeIsActive(),
            _ => throw new ArgumentOutOfRangeException(nameof(readingEvent), readingEvent, "The automatic-reading event is not defined."),
        };

    private static string FileName(string path)
    {
        var normalized = path.Replace('\\', '/');
        return normalized[(normalized.LastIndexOf('/') + 1)..];
    }

    private static string BaseName(string path)
    {
        var fileName = FileName(path);
        const string suffix = ".overwrite.md";
        return fileName.EndsWith(suffix, StringComparison.Ordinal)
            ? fileName[..(fileName.Length - suffix.Length)] + ".md"
            : fileName;
    }
}
