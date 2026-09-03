using OpenForge.Cli.Core.Commands.References.Models.Occurrence;
using OpenForge.Cli.Core.Commands.References.Models.Resolution;
using OpenForge.Cli.Core.Commands.References.Models.Result;
using OpenForge.Cli.Core.Commands.References.Models.Source;
using OpenForge.Cli.Core.Commands.References.Shared.Documents.Parsing;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Filesystem.TypedReads;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;
using OpenForge.Cli.Core.Framework.Sources.Models.References;
using OpenForge.Cli.Core.Framework.Sources.References;
using OpenForge.Cli.Core.Framework.Workspace;

namespace OpenForge.Cli.Core.Commands.References.Shared.Resolution;

internal delegate PhysicalPathResolution ReferencesPhysicalPathResolver(
    CliWorkspace workspace,
    string lexicalPath);

internal delegate ValueTask<FileReadResult<string>> ReferencesStrictUtf8Reader(
    string physicalPath,
    string logicalPath,
    CancellationToken cancellationToken);

internal sealed class ReferencesDestinationResolver
{
    private readonly SourceLinkDestinationResolver _resolver;

    internal ReferencesDestinationResolver(
        ReferencesPhysicalPathResolver physicalPathResolver,
        ReferencesStrictUtf8Reader strictUtf8Reader,
        ReferencesMarkdownParser markdownParser)
    {
        ArgumentNullException.ThrowIfNull(physicalPathResolver);
        ArgumentNullException.ThrowIfNull(strictUtf8Reader);
        ArgumentNullException.ThrowIfNull(markdownParser);
        _resolver = new SourceLinkDestinationResolver(
            (workspace, lexicalPath) => physicalPathResolver(workspace, lexicalPath),
            (physicalPath, logicalPath, cancellationToken) =>
                strictUtf8Reader(physicalPath, logicalPath, cancellationToken),
            source => markdownParser(source));
    }

    internal async ValueTask<ReferencesDestinationFacts> ResolveAsync(
        ReferencesDestinationInput input,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(input);
        var facts = await _resolver.ResolveAsync(
            new SourceLinkDestinationInput
            {
                Workspace = input.Workspace,
                Catalogue = input.Catalogue,
                SourceCanonicalPath = input.Layer.CanonicalPath,
                RawDestination = input.RawDestination,
            },
            cancellationToken).ConfigureAwait(false);
        return new ReferencesDestinationFacts(
            facts.Fragment,
            new ReferencesTarget(
                TargetKind(facts.Target.Kind),
                facts.Target.Id,
                facts.Target.Path,
                facts.Target.Layer,
                Resolution(facts.Target.Resolution),
                facts.Target.Network is null ? null : ReferencesNetworkState.NetworkNotAttempted),
            facts.Finding is null
                ? null
                : new ReferencesDestinationFinding(
                    FindingCode(facts.Finding.Code),
                    facts.Finding.Cause,
                    facts.Finding.Candidates.Select(Identity).ToArray()));
    }

    private static ReferencesSourceIdentity Identity(SourceLinkIdentity candidate)
    {
        if (candidate.Id is not { } id)
        {
            throw new InvalidOperationException(
                "A shared managed-source finding candidate requires an automatic ID.");
        }

        return new ReferencesSourceIdentity(id, candidate.Path);
    }

    private static ReferencesTargetKind TargetKind(SourceLinkTargetKind value)
        => value switch
        {
            SourceLinkTargetKind.Local => ReferencesTargetKind.Local,
            SourceLinkTargetKind.External => ReferencesTargetKind.External,
            SourceLinkTargetKind.Unsupported => ReferencesTargetKind.Unsupported,
            _ => throw new ArgumentOutOfRangeException(nameof(value), value, "The shared link target kind is not defined."),
        };

    private static ReferencesTargetResolution Resolution(SourceLinkTargetResolution value)
        => value switch
        {
            SourceLinkTargetResolution.Complete => ReferencesTargetResolution.Complete,
            SourceLinkTargetResolution.Missing => ReferencesTargetResolution.Missing,
            SourceLinkTargetResolution.FragmentMissing => ReferencesTargetResolution.FragmentMissing,
            SourceLinkTargetResolution.Malformed => ReferencesTargetResolution.Malformed,
            SourceLinkTargetResolution.Absolute => ReferencesTargetResolution.Absolute,
            SourceLinkTargetResolution.Query => ReferencesTargetResolution.Query,
            SourceLinkTargetResolution.EncodingUnsupported => ReferencesTargetResolution.EncodingUnsupported,
            SourceLinkTargetResolution.OutsideWorkspace => ReferencesTargetResolution.OutsideWorkspace,
            SourceLinkTargetResolution.PhysicalEscape => ReferencesTargetResolution.PhysicalEscape,
            SourceLinkTargetResolution.Ambiguous => ReferencesTargetResolution.Ambiguous,
            SourceLinkTargetResolution.Unreadable => ReferencesTargetResolution.Unreadable,
            SourceLinkTargetResolution.Unsupported => ReferencesTargetResolution.Unsupported,
            SourceLinkTargetResolution.ExternalUnchecked => ReferencesTargetResolution.ExternalUnchecked,
            _ => throw new ArgumentOutOfRangeException(nameof(value), value, "The shared link target resolution is not defined."),
        };

    private static ReferencesFindingCode FindingCode(SourceLinkDestinationFindingCode value)
        => value switch
        {
            SourceLinkDestinationFindingCode.DestinationMalformed => ReferencesFindingCode.DestinationMalformed,
            SourceLinkDestinationFindingCode.DestinationUnsupported => ReferencesFindingCode.DestinationUnsupported,
            SourceLinkDestinationFindingCode.TargetUnsafe => ReferencesFindingCode.TargetUnsafe,
            SourceLinkDestinationFindingCode.InvalidEncoding => ReferencesFindingCode.InvalidEncoding,
            SourceLinkDestinationFindingCode.TargetMissing => ReferencesFindingCode.TargetMissing,
            SourceLinkDestinationFindingCode.TargetUnreadable => ReferencesFindingCode.TargetUnreadable,
            SourceLinkDestinationFindingCode.TargetAmbiguous => ReferencesFindingCode.TargetAmbiguous,
            SourceLinkDestinationFindingCode.IdentityCollision => ReferencesFindingCode.IdentityCollision,
            SourceLinkDestinationFindingCode.FragmentMissing => ReferencesFindingCode.FragmentMissing,
            _ => throw new ArgumentOutOfRangeException(nameof(value), value, "The shared link finding code is not defined."),
        };
}
