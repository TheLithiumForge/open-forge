using OpenForge.Cli.Core.Commands.Route.Inspect.Models.Result;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Definitions.Models;

namespace OpenForge.Cli.Core.Commands.Route.Inspect;

internal static class RouteInspectDefinitions
{
    internal const int SchemaVersion = 1;

    internal const string CommandIdentity = "route inspect";

    internal static readonly CliSyntaxDefinition InspectCommand = new(
        "inspect",
        "Explain one source's route behavior without returning authored content.");

    internal static readonly CliSyntaxDefinition SourceReference = new(
        "source-reference",
        "Select one source by its ID or exact .agents path.");

    internal static readonly CliOptionDefinition<string[]> WorkingPath = new(
        "--for",
        "Explain applicability for one workspace-relative working path; repeat for more paths.",
        CliOptionArity.ExactlyOne,
        [],
        "path");

    internal static readonly CliOptionDefinition<bool> MatchingFiles = new(
        "--matching-files",
        "List current workspace files matching this source's own and inherited applyTo conditions.",
        CliOptionArity.None,
        false);

    internal static string ReadObservationCode(RouteInspectObservationCode code)
    {
        return code switch
        {
            RouteInspectObservationCode.AutomaticIdNotUnique => "route-inspect.automatic-id-not-unique",
            RouteInspectObservationCode.CompatibilityEntrypoint => "route-inspect.compatibility-entrypoint",
            RouteInspectObservationCode.DetachedSource => "route-inspect.detached-source",
            RouteInspectObservationCode.NotRouted => "route-inspect.not-routed",
            RouteInspectObservationCode.ValidOverwrite => "route-inspect.valid-overwrite",
            _ => throw new ArgumentOutOfRangeException(
                nameof(code),
                code,
                "The route-inspect observation code is not defined."),
        };
    }

    internal static string ReadConditionCode(RouteInspectConditionCode code)
    {
        return code switch
        {
            RouteInspectConditionCode.InvalidWorkspace => "route-inspect.invalid-workspace",
            RouteInspectConditionCode.InvalidWorkingPath => "route-inspect.invalid-working-path",
            RouteInspectConditionCode.WorkspaceUnavailable => "route-inspect.workspace-unavailable",
            RouteInspectConditionCode.UnsafeWorkspace => "route-inspect.unsafe-workspace",
            RouteInspectConditionCode.MissingSource => "route-inspect.missing-source",
            RouteInspectConditionCode.MultipleSources => "route-inspect.multiple-sources",
            RouteInspectConditionCode.InvalidSourceReference => "route-inspect.invalid-source-reference",
            RouteInspectConditionCode.LoaderSubject => "route-inspect.loader-subject",
            RouteInspectConditionCode.UnknownSource => "route-inspect.unknown-source",
            RouteInspectConditionCode.MissingSourceFile => "route-inspect.missing-source-file",
            RouteInspectConditionCode.UnsupportedSource => "route-inspect.unsupported-source",
            RouteInspectConditionCode.AmbiguousSource => "route-inspect.ambiguous-source",
            RouteInspectConditionCode.UnsafeSource => "route-inspect.unsafe-source",
            RouteInspectConditionCode.AmbiguousRoute => "route-inspect.ambiguous-route",
            RouteInspectConditionCode.OrphanOverwrite => "route-inspect.orphan-overwrite",
            RouteInspectConditionCode.AmbiguousOverwrite => "route-inspect.ambiguous-overwrite",
            RouteInspectConditionCode.UnreadableSource => "route-inspect.unreadable-source",
            RouteInspectConditionCode.IncompleteRoute => "route-inspect.incomplete-route",
            RouteInspectConditionCode.UnavailableFact => "route-inspect.unavailable-fact",
            RouteInspectConditionCode.OperationFailed => "route-inspect.operation-failed",
            RouteInspectConditionCode.Interrupted => "route-inspect.interrupted",
            _ => throw new ArgumentOutOfRangeException(
                nameof(code),
                code,
                "The route-inspect condition code is not defined."),
        };
    }
}
