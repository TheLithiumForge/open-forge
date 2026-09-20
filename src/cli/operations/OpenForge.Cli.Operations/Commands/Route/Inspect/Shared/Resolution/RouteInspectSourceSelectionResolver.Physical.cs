using OpenForge.Cli.Core.Commands.Route.Inspect.Models.Resolution;
using OpenForge.Cli.Core.Commands.Route.Shared.Models.Source;
using OpenForge.Cli.Core.Framework.Sources.Models.Reading;

namespace OpenForge.Cli.Core.Commands.Route.Inspect.Shared.Resolution;

internal sealed partial class RouteInspectSourceSelectionResolver
{
    private static RouteInspectResolution? ReadPhysicalResolution(
        RouteInspectSelection selection,
        RouteSourceProjection projection,
        string requestedPath)
    {
        var baseIssue = ReadLayerIssue(projection.BaseRead, isBase: true);
        if (baseIssue is not null)
        {
            return CreatePhysicalResolution(selection, requestedPath, baseIssue);
        }

        if (projection.OverwriteRead is { } overwriteRead)
        {
            var overwriteIssue = ReadLayerIssue(overwriteRead, isBase: false);
            if (overwriteIssue is not null)
            {
                return CreatePhysicalResolution(selection, requestedPath, overwriteIssue);
            }
        }

        return null;
    }

    private static RouteInspectResolutionIssue? ReadLayerIssue(
        SourceDocumentReadResult read,
        bool isBase)
    {
        return read.Verification.State switch
        {
            SourceLayerVerificationState.Verified => null,
            SourceLayerVerificationState.Cancelled => RouteInspectResolutionSupport.CreateIssue(
                RouteInspectResolutionIssueCode.Interrupted,
                read.Layer.CanonicalPath,
                "Source resolution was interrupted."),
            SourceLayerVerificationState.Missing => RouteInspectResolutionSupport.CreateIssue(
                isBase ? RouteInspectResolutionIssueCode.MissingSource : RouteInspectResolutionIssueCode.ReadUnavailable,
                read.Layer.CanonicalPath,
                isBase
                    ? "The source physical boundary could not be proved."
                    : "The overwrite physical boundary could not be proved."),
            _ => RouteInspectResolutionSupport.CreateIssue(
                RouteInspectResolutionIssueCode.UnsafeSource,
                read.Layer.CanonicalPath,
                isBase
                    ? "The source does not match its catalogue physical identity."
                    : "The overwrite physical boundary could not be proved."),
        };
    }

    private static RouteInspectResolution CreatePhysicalResolution(
        RouteInspectSelection selection,
        string requestedPath,
        RouteInspectResolutionIssue issue)
    {
        if (issue.Code == RouteInspectResolutionIssueCode.Interrupted)
        {
            return RouteInspectResolutionSupport.Interrupted(selection, requestedPath);
        }

        return RouteInspectResolution.Create(
            RouteInspectResolutionState.Blocked,
            selection,
            null,
            null,
            [issue]);
    }

    private static RouteInspectResolution Unsafe(
        RouteInspectSelection selection,
        string subject)
    {
        return RouteInspectResolution.Create(
            RouteInspectResolutionState.Blocked,
            selection,
            null,
            null,
            [RouteInspectResolutionSupport.CreateIssue(
                RouteInspectResolutionIssueCode.UnsafeSource,
                subject,
                "The selected source crosses an unproved physical boundary.")]);
    }

    private static RouteInspectResolution Interrupted(RouteInspectResolutionInput input)
    {
        var subject = input.Parsed.AttemptedId ?? input.Parsed.AttemptedPath
            ?? throw new InvalidOperationException("A parsed source reference requires an attempted identity.");
        return RouteInspectResolutionSupport.Interrupted(input.UnresolvedSelection, subject);
    }
}
