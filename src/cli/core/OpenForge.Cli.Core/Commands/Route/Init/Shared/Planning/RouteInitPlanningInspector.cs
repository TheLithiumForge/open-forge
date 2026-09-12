using OpenForge.Cli.Core.Commands.Route.Init.Models.Request;
using OpenForge.Cli.Core.Commands.Route.Init.Models.Result;
using OpenForge.Cli.Core.Framework.Distribution;
using OpenForge.Cli.Core.Framework.Distribution.Models;
using OpenForge.Cli.Core.Framework.Documents.Metadata.Models;
using OpenForge.Cli.Core.Framework.Documents.Metadata;
using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;
using OpenForge.Cli.Core.Framework.Sources.Models.Reading;

namespace OpenForge.Cli.Core.Commands.Route.Init.Shared.Planning;

internal sealed class RouteInitPlanningInspector
{
    private readonly RouteInitTargetPlanner _targetPlanner = new();
    private readonly RouteInitCurrentStateReader _currentStateReader = new();
    private readonly RouteInitFrameworkAlignmentBuilder _alignmentBuilder = new();
    private readonly RouteInitFrameworkLifecycleBuilder _lifecycleBuilder = new();

    internal async ValueTask<RouteInitInspectionResult> InspectAsync(
        RouteInitRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        if (cancellationToken.IsCancellationRequested)
        {
            return Stopped(
                RouteInitFindingCode.Interrupted,
                "Route Init planning was cancelled.",
                incomplete: true);
        }

        var resolution = _targetPlanner.Resolve(request);
        if (!resolution.IsResolved || resolution.Target is not { } requestedTarget)
        {
            return Stopped(
                RouteInitFindingCode.InvalidTarget,
                resolution.Cause ?? "The Route Init target is invalid.",
                incomplete: true,
                requested: request.RouteTarget);
        }

        if (!IsValidMetadataInput(request))
        {
            return Stopped(
                RouteInitFindingCode.InvalidMetadata,
                "The Route Init metadata input is invalid for the selected scaffold and target.",
                incomplete: true,
                requestedTarget);
        }

        FrameworkPayload? payload = null;
        RouteInitFrameworkAlignment? alignment = null;
        var target = requestedTarget;
        if (request.Scaffold == RouteInitScaffold.Framework)
        {
            var payloadRead = EmbeddedFrameworkPayloadReader.Read();
            if (payloadRead.Payload is not { } embedded)
            {
                var code = payloadRead.State == FrameworkPayloadReadState.Invalid
                    ? RouteInitFindingCode.FrameworkPayloadInvalid
                    : RouteInitFindingCode.FrameworkPayloadUnavailable;
                return Stopped(
                    code,
                    payloadRead.Cause ?? "The embedded Framework payload is unavailable.",
                    incomplete: payloadRead.State != FrameworkPayloadReadState.Invalid,
                    requestedTarget);
            }

            payload = embedded;
            IReadOnlyList<SourceLogicalSource> projected;
            try
            {
                projected = new EmbeddedFrameworkSourceProjector().Project(request.Workspace, payload);
            }
            catch (Exception exception) when (exception is ArgumentException
                or InvalidDataException
                or InvalidOperationException)
            {
                return Stopped(
                    RouteInitFindingCode.FrameworkPayloadInvalid,
                    exception.Message,
                    incomplete: false,
                    requestedTarget);
            }

            var aligned = _alignmentBuilder.Build(request.Workspace, requestedTarget, projected);
            if (aligned.State != RouteInitFrameworkAlignmentState.Complete
                || aligned.Alignment is not { } completeAlignment)
            {
                var code = aligned.Cause?.Contains("scope label", StringComparison.OrdinalIgnoreCase) == true
                    ? RouteInitFindingCode.InvalidTarget
                    : RouteInitFindingCode.FrameworkAlignmentBlocked;
                return Stopped(
                    code,
                    aligned.Cause ?? "The Framework target could not be aligned.",
                    incomplete: code == RouteInitFindingCode.InvalidTarget,
                    requestedTarget);
            }

            alignment = completeAlignment;
            target = alignment.Target;
        }

        var catalogue = await _currentStateReader.ReadCatalogueAsync(
                request.Workspace,
                cancellationToken)
            .ConfigureAwait(false);
        if (catalogue.IsCancelled)
        {
            return Stopped(
                RouteInitFindingCode.Interrupted,
                "Route Init inspection was cancelled.",
                incomplete: true,
                target);
        }

        if (ReadCatalogueBoundary(request, target, catalogue) is { } catalogueBoundary)
        {
            return new RouteInitInspectionStopped(catalogueBoundary);
        }

        if (ReadExactCompatibilityBoundary(target, catalogue) is { } compatibilityBoundary)
        {
            return new RouteInitInspectionStopped(compatibilityBoundary with
            {
                Alignment = alignment,
                Payload = payload,
            });
        }

        RouteInitFrameworkTrust? trust = null;
        if (payload is not null)
        {
            trust = await _lifecycleBuilder.ReadTrustAsync(
                    request.Workspace,
                    payload,
                    cancellationToken)
                .ConfigureAwait(false);
            if (!trust.IsCurrent)
            {
                return new RouteInitInspectionStopped(ReadTrustBoundary(
                    target,
                    trust,
                    alignment,
                    payload));
            }
        }

        var current = await _currentStateReader.ReadChainAsync(catalogue, target, cancellationToken)
            .ConfigureAwait(false);
        if (ReadCurrentBoundary(current) is { } currentBoundary)
        {
            return new RouteInitInspectionStopped(currentBoundary with
            {
                Alignment = alignment,
                Payload = payload,
            });
        }

        if (current.Chain[^1].Existing is not null && HasMetadataInput(request.Metadata))
        {
            return Stopped(
                RouteInitFindingCode.InvalidMetadata,
                "Metadata flags cannot be applied to an existing final route entrypoint.",
                incomplete: true,
                target);
        }

        RouteInitFrameworkPlanningBasis? framework = null;
        if (alignment is not null && payload is not null && trust is not null)
        {
            framework = new RouteInitFrameworkPlanningBasis(alignment, payload, trust);
        }

        return new RouteInitInspectionCompleted(
            new RouteInitInspectionFacts(target, catalogue, current, framework));
    }

    private static RouteInitPlanningBoundary? ReadCatalogueBoundary(
        RouteInitRequest request,
        RouteInitTargetFacts target,
        RouteInitCurrentCatalogueFacts current)
    {
        var issue = current.Catalogue.Issues.FirstOrDefault(candidate =>
            candidate.Code != SourceCatalogueIssueCode.RootMissing
            || request.Scaffold == RouteInitScaffold.Framework);
        if (issue is null)
        {
            return null;
        }

        var (code, incomplete) = ReadCatalogueFinding(
            issue.Code,
            request.Scaffold == RouteInitScaffold.Framework);
        return new RouteInitPlanningBoundary(
            code,
            $"The source catalogue boundary '{issue.AttemptedCanonicalPath}' is not safe and complete.",
            incomplete,
            target);
    }

    internal static (RouteInitFindingCode Code, bool Incomplete) ReadCatalogueFinding(
        SourceCatalogueIssueCode code,
        bool isFramework)
        => code switch
        {
            SourceCatalogueIssueCode.IdentityCollision
                or SourceCatalogueIssueCode.PhysicalAlias
                or SourceCatalogueIssueCode.OrphanOverwrite => (RouteInitFindingCode.IdentityCollision, false),
            SourceCatalogueIssueCode.RootUnsafe
                or SourceCatalogueIssueCode.CandidateUnsafe => (RouteInitFindingCode.TargetUnsafe, false),
            SourceCatalogueIssueCode.RootMissing when isFramework => (RouteInitFindingCode.FrameworkInstallRequired, false),
            SourceCatalogueIssueCode.RootMissing
                or SourceCatalogueIssueCode.RootUnavailable
                or SourceCatalogueIssueCode.DirectoryUnavailable
                or SourceCatalogueIssueCode.CandidateUnavailable
                or SourceCatalogueIssueCode.IdentityUnavailable => (RouteInitFindingCode.InspectionIncomplete, true),
            _ => throw new ArgumentOutOfRangeException(
                nameof(code),
                code,
                "The source catalogue issue code is not defined."),
        };

    private static RouteInitPlanningBoundary? ReadExactCompatibilityBoundary(
        RouteInitTargetFacts target,
        RouteInitCurrentCatalogueFacts current)
    {
        if (target.Kind != RouteInitTargetKind.ExactPath
            || target.CanonicalPath is not { } requestedPath
            || !SourceFormClassifier.TryClassify(requestedPath, out var requestedForm)
            || !SourceFormClassifier.IsCompatibilityEntrypoint(requestedForm))
        {
            return null;
        }

        var exactExists = target.Id is { } targetId
            && current.Catalogue.FindAllById(targetId).Any(source =>
                SourceFormClassifier.IsEntrypoint(source.Base.Form)
                && string.Equals(
                    source.Identity.CanonicalBasePath,
                    requestedPath,
                    StringComparison.Ordinal));
        return exactExists
            ? null
            : new RouteInitPlanningBoundary(
                RouteInitFindingCode.InvalidTarget,
                "An exact compatibility Route Init target must already exist at the requested path.",
                Incomplete: true,
                target);
    }

    private static RouteInitPlanningBoundary? ReadCurrentBoundary(
        RouteInitCurrentStateFacts current)
    {
        if (current.Chain.FirstOrDefault(entry => entry.IsAmbiguous) is not null)
        {
            return new RouteInitPlanningBoundary(
                RouteInitFindingCode.RouteAmbiguous,
                "A route folder has more than one recognized entrypoint.",
                Incomplete: false,
                current.Target);
        }

        if (current.Chain.FirstOrDefault(entry => entry.IsMissing && entry.IdentityOccupants.Count > 0) is not null)
        {
            return new RouteInitPlanningBoundary(
                RouteInitFindingCode.IdentityCollision,
                "A route identity is occupied by a non-entrypoint source.",
                Incomplete: false,
                current.Target);
        }

        foreach (var read in current.Reads.SelectMany(ReadLayers))
        {
            if (read.Verification.State == SourceLayerVerificationState.Cancelled
                || read.Read?.State == OpenForge.Cli.Core.Framework.Filesystem.TypedReads.Models.FileReadState.Cancelled)
            {
                return new RouteInitPlanningBoundary(
                    RouteInitFindingCode.Interrupted,
                    "Route Init source inspection was cancelled.",
                    Incomplete: true,
                    current.Target);
            }

            if (read.Verification.State == SourceLayerVerificationState.Unsafe)
            {
                var code = read.Layer.Form == SourceDocumentForm.Loader
                    ? RouteInitFindingCode.LoaderUnsafe
                    : RouteInitFindingCode.TargetUnsafe;
                return new RouteInitPlanningBoundary(
                    code,
                    "A required Route Init source has an unsafe physical boundary.",
                    Incomplete: false,
                    current.Target);
            }

            if (read.Verification.State != SourceLayerVerificationState.Verified
                || read.Read?.State != OpenForge.Cli.Core.Framework.Filesystem.TypedReads.Models.FileReadState.Complete)
            {
                return new RouteInitPlanningBoundary(
                    RouteInitFindingCode.InspectionIncomplete,
                    "A required Route Init source could not be read completely.",
                    Incomplete: true,
                    current.Target);
            }
        }

        return null;
    }

    private static IEnumerable<SourceDocumentReadResult> ReadLayers(RouteInitCurrentSourceRead source)
    {
        yield return source.Base;
        if (source.Overwrite is not null)
        {
            yield return source.Overwrite;
        }
    }

    private static RouteInitPlanningBoundary ReadTrustBoundary(
        RouteInitTargetFacts target,
        RouteInitFrameworkTrust trust,
        RouteInitFrameworkAlignment? alignment,
        FrameworkPayload payload)
    {
        var (code, incomplete) = ReadTrustFinding(trust.State);
        return new RouteInitPlanningBoundary(
            code,
            trust.Cause ?? "The trusted Framework lifecycle boundary is unavailable.",
            incomplete,
            target,
            Alignment: alignment,
            Payload: payload);
    }

    internal static (RouteInitFindingCode Code, bool Incomplete) ReadTrustFinding(
        RouteInitFrameworkTrustState state)
        => state switch
        {
            RouteInitFrameworkTrustState.Current => (RouteInitFindingCode.LifecycleBlocked, false),
            RouteInitFrameworkTrustState.InstallRequired => (RouteInitFindingCode.FrameworkInstallRequired, false),
            RouteInitFrameworkTrustState.UpdateRequired => (RouteInitFindingCode.FrameworkUpdateRequired, false),
            RouteInitFrameworkTrustState.Incomplete => (RouteInitFindingCode.LifecycleUnavailable, true),
            RouteInitFrameworkTrustState.Cancelled => (RouteInitFindingCode.Interrupted, true),
            RouteInitFrameworkTrustState.Blocked => (RouteInitFindingCode.LifecycleBlocked, false),
            _ => throw new ArgumentOutOfRangeException(
                nameof(state),
                state,
                "The Route Init Framework trust state is not defined."),
        };

    private static bool HasMetadataInput(RouteInitMetadataInput input)
        => input.Description is not null
            || input.ResponsibilitySpecified
            || input.Tags.Count > 0;

    private static bool IsValidMetadataInput(RouteInitRequest request)
    {
        var input = request.Metadata;
        if (request.Scaffold == RouteInitScaffold.Framework && HasMetadataInput(input))
        {
            return false;
        }

        if (input.Description is not null && string.IsNullOrWhiteSpace(input.Description))
        {
            return false;
        }

        if (input.ResponsibilitySpecified
            && input.Responsibility is { Length: > 0 }
            && string.IsNullOrWhiteSpace(input.Responsibility))
        {
            return false;
        }

        return input.Tags.All(FrameworkDocumentMetadataTagGrammar.IsValid)
            && input.Tags.Distinct(StringComparer.Ordinal).Count() == input.Tags.Count;
    }

    private static RouteInitInspectionStopped Stopped(
        RouteInitFindingCode code,
        string cause,
        bool incomplete,
        RouteInitTargetFacts? target = null,
        string? requested = null,
        RouteInitFrameworkAlignment? alignment = null,
        FrameworkPayload? payload = null)
        => new(new RouteInitPlanningBoundary(
            code,
            cause,
            incomplete,
            target,
            requested,
            alignment,
            payload));
}
