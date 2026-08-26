using OpenForge.Cli.Core.Commands.Context.Models.Operation;
using OpenForge.Cli.Core.Commands.Context.Models.Result;
using OpenForge.Cli.Core.Framework.Documents.Markdown.Models;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;
using OpenForge.Cli.Core.Framework.Sources.Models.Locations;
using OpenForge.Cli.Core.Framework.Sources.Models.References;

namespace OpenForge.Cli.Core.Commands.Context.Shared.Links;

internal static class ContextLinkResultFactory
{
    internal static ContextFinding Finding(
        ContextAuthoredLinkEvidence evidence,
        SourceLinkDestinationFacts facts)
    {
        var finding = facts.Finding
            ?? throw new InvalidOperationException("A shared link finding is required.");
        return new ContextFinding(
            code: FindingCode(finding.Code),
            subject: evidence.Authored.RawDestination,
            cause: finding.Cause,
            reference: null,
            source: Identity(evidence.Source),
            layer: Layer(evidence.Layer.Kind),
            path: facts.Target.Path ?? evidence.Layer.CanonicalPath,
            part: null,
            location: evidence.Location,
            destinationLocation: evidence.DestinationLocation,
            candidates: finding.Candidates.Select(candidate => new ContextSourceIdentity(
                id: candidate.Id,
                path: candidate.Path)));
    }

    internal static ContextFinding UnavailableTargetFinding(
        ContextAuthoredLinkEvidence evidence,
        string? path)
        => new(
            code: ContextFindingCode.ClosureUnavailable,
            subject: evidence.Authored.RawDestination,
            cause: "The contained local Markdown target could not be added to the Context graph.",
            reference: null,
            source: Identity(evidence.Source),
            layer: Layer(evidence.Layer.Kind),
            path: path ?? evidence.Layer.CanonicalPath,
            part: null,
            location: evidence.Location,
            destinationLocation: evidence.DestinationLocation,
            candidates: []);

    internal static ContextFinding CaseMismatchFinding(
        ContextAuthoredLinkEvidence evidence,
        string? path)
        => new(
            code: ContextFindingCode.TargetCaseMismatch,
            subject: evidence.Authored.RawDestination,
            cause: "The authored local destination casing differs from the exact target path casing.",
            reference: null,
            source: Identity(evidence.Source),
            layer: Layer(evidence.Layer.Kind),
            path: path ?? evidence.Layer.CanonicalPath,
            part: null,
            location: evidence.Location,
            destinationLocation: evidence.DestinationLocation,
            candidates: []);

    internal static ContextLinkTarget Target(
        SourceLinkTarget target,
        ContextGraphSource? source,
        bool caseMismatch)
        => new()
        {
            Kind = TargetKind(target.Kind),
            Id = target.Id,
            Path = target.Path,
            Layer = TargetLayer(target, source),
            Resolution = caseMismatch && target.Resolution == SourceLinkTargetResolution.Complete
                ? ContextLinkResolution.CaseMismatch
                : Resolution(target.Resolution),
            Network = target.Network is null ? null : ContextLinkNetwork.NetworkNotAttempted,
        };

    internal static ContextSourceLayerKind Layer(SourceLayerKind value)
        => value switch
        {
            SourceLayerKind.Base => ContextSourceLayerKind.Base,
            SourceLayerKind.Overwrite => ContextSourceLayerKind.Overwrite,
            _ => throw new ArgumentOutOfRangeException(nameof(value), value, "The source layer is not defined."),
        };

    internal static ContextSourceIdentity Identity(ContextGraphSource source)
        => new(id: source.Id, path: source.CanonicalPath);

    private static ContextFindingCode FindingCode(SourceLinkDestinationFindingCode value)
        => value switch
        {
            SourceLinkDestinationFindingCode.DestinationMalformed => ContextFindingCode.ClosureUnavailable,
            SourceLinkDestinationFindingCode.DestinationUnsupported => ContextFindingCode.ClosureUnavailable,
            SourceLinkDestinationFindingCode.TargetUnsafe => ContextFindingCode.TargetUnsafe,
            SourceLinkDestinationFindingCode.InvalidEncoding => ContextFindingCode.LinkEncodingInvalid,
            SourceLinkDestinationFindingCode.TargetMissing => ContextFindingCode.TargetMissing,
            SourceLinkDestinationFindingCode.TargetUnreadable => ContextFindingCode.TargetUnreadable,
            SourceLinkDestinationFindingCode.TargetAmbiguous => ContextFindingCode.TargetAmbiguous,
            SourceLinkDestinationFindingCode.IdentityCollision => ContextFindingCode.IdentityCollision,
            SourceLinkDestinationFindingCode.FragmentMissing => ContextFindingCode.FragmentMissing,
            _ => throw new ArgumentOutOfRangeException(nameof(value), value, "The shared link finding code is not defined."),
        };

    private static ContextSourceLayerKind? TargetLayer(
        SourceLinkTarget target,
        ContextGraphSource? source)
    {
        if (target.Layer is { } layer)
        {
            return Layer(layer);
        }

        return source is not null && target.Kind == SourceLinkTargetKind.Local
            ? ContextSourceLayerKind.Base
            : null;
    }

    private static ContextLinkTargetKind TargetKind(SourceLinkTargetKind value)
        => value switch
        {
            SourceLinkTargetKind.Local => ContextLinkTargetKind.Local,
            SourceLinkTargetKind.External => ContextLinkTargetKind.External,
            SourceLinkTargetKind.Unsupported => ContextLinkTargetKind.Unsupported,
            _ => throw new ArgumentOutOfRangeException(nameof(value), value, "The shared link target kind is not defined."),
        };

    private static ContextLinkResolution Resolution(SourceLinkTargetResolution value)
        => value switch
        {
            SourceLinkTargetResolution.Complete => ContextLinkResolution.Complete,
            SourceLinkTargetResolution.Missing => ContextLinkResolution.Missing,
            SourceLinkTargetResolution.FragmentMissing => ContextLinkResolution.FragmentMissing,
            SourceLinkTargetResolution.Malformed => ContextLinkResolution.Malformed,
            SourceLinkTargetResolution.Absolute => ContextLinkResolution.Absolute,
            SourceLinkTargetResolution.Query => ContextLinkResolution.Query,
            SourceLinkTargetResolution.EncodingUnsupported => ContextLinkResolution.EncodingUnsupported,
            SourceLinkTargetResolution.OutsideWorkspace => ContextLinkResolution.OutsideWorkspace,
            SourceLinkTargetResolution.PhysicalEscape => ContextLinkResolution.PhysicalEscape,
            SourceLinkTargetResolution.Ambiguous => ContextLinkResolution.Ambiguous,
            SourceLinkTargetResolution.Unreadable => ContextLinkResolution.Unreadable,
            SourceLinkTargetResolution.Unsupported => ContextLinkResolution.Unsupported,
            SourceLinkTargetResolution.ExternalUnchecked => ContextLinkResolution.ExternalUnchecked,
            _ => throw new ArgumentOutOfRangeException(nameof(value), value, "The shared link target resolution is not defined."),
        };
}
