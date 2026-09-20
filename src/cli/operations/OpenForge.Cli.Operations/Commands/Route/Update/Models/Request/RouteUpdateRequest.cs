using System.Collections.Immutable;
using OpenForge.Cli.Core.Commands.Route.Update.Models.Result;
using OpenForge.Cli.Core.Framework.Workspace.Models;

namespace OpenForge.Cli.Core.Commands.Route.Update.Models.Request;

internal enum RouteUpdateMode
{
    Apply,
    DryRun,
}

internal sealed record RouteUpdateDescriptionRequest
{
    public required bool Requested { get; init; }

    public string? Value { get; init; }
}

internal sealed record RouteUpdateResponsibilityRequest
{
    public required RouteUpdateResponsibilityOperation Operation { get; init; }

    public string? Value { get; init; }
}

internal sealed record RouteUpdateTagsRequest
{
    public required bool Requested { get; init; }

    public required ImmutableArray<string> Values { get; init; }
}

internal sealed record RouteUpdatePatchRequest
{
    public required RouteUpdateDescriptionRequest Description { get; init; }

    public required RouteUpdateResponsibilityRequest Responsibility { get; init; }

    public required RouteUpdateTagsRequest Tags { get; init; }

    internal bool IsRequested => Description.Requested
        || Responsibility.Operation != RouteUpdateResponsibilityOperation.NotRequested
        || Tags.Requested;
}

internal sealed record RouteUpdateRequest
{
    internal RouteUpdateRequest(
        CliWorkspace workspace,
        string sourceReference,
        RouteUpdatePatchRequest patch,
        string? templateReference,
        RouteUpdateMode mode,
        bool allowInteractiveSourceSelection = false,
        string? frozenSourceReference = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sourceReference);
        ArgumentNullException.ThrowIfNull(patch);
        if (frozenSourceReference is not null)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(frozenSourceReference);
        }
        if (!Enum.IsDefined(mode))
        {
            throw new ArgumentOutOfRangeException(
                nameof(mode),
                mode,
                "The Route Update mode is not defined.");
        }

        Workspace = workspace;
        SourceReference = sourceReference;
        Patch = patch;
        TemplateReference = templateReference;
        Mode = mode;
        AllowInteractiveSourceSelection = allowInteractiveSourceSelection;
        FrozenSourceReference = frozenSourceReference;
    }

    internal CliWorkspace Workspace { get; }

    internal string SourceReference { get; }

    internal RouteUpdatePatchRequest Patch { get; }

    internal string? TemplateReference { get; }

    internal RouteUpdateMode Mode { get; }

    internal bool AllowInteractiveSourceSelection { get; }

    internal string? FrozenSourceReference { get; }

    internal string ResolutionReference => FrozenSourceReference ?? SourceReference;

    internal bool IsDryRun => Mode == RouteUpdateMode.DryRun;

    internal RouteUpdateRequest FreezeSource(string exactSourceReference)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(exactSourceReference);
        return new(
            workspace: Workspace,
            sourceReference: SourceReference,
            patch: Patch,
            templateReference: TemplateReference,
            mode: Mode,
            allowInteractiveSourceSelection: AllowInteractiveSourceSelection,
            frozenSourceReference: exactSourceReference);
    }
}
