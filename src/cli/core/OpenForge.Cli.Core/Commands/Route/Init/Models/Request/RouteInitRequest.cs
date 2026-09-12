using System.Collections.ObjectModel;
using OpenForge.Cli.Core.Framework.Workspace.Models;

namespace OpenForge.Cli.Core.Commands.Route.Init.Models.Request;

internal enum RouteInitMode
{
    Apply,
    DryRun,
}

internal enum RouteInitScaffold
{
    Generic,
    Framework,
}

internal sealed record RouteInitMetadataInput
{
    internal RouteInitMetadataInput(
        string? description,
        bool responsibilitySpecified,
        string? responsibility,
        IEnumerable<string> tags)
    {
        ArgumentNullException.ThrowIfNull(tags);
        var tagValues = tags
            .Select(tag => tag ?? throw new ArgumentException(
                "Route Init tags cannot contain null members.",
                nameof(tags)))
            .ToArray();
        Description = description;
        ResponsibilitySpecified = responsibilitySpecified;
        Responsibility = responsibility;
        Tags = new ReadOnlyCollection<string>(tagValues);
    }

    internal string? Description { get; }

    internal bool ResponsibilitySpecified { get; }

    internal string? Responsibility { get; }

    internal IReadOnlyList<string> Tags { get; }

    internal static RouteInitMetadataInput None { get; } = new(
        description: null,
        responsibilitySpecified: false,
        responsibility: null,
        tags: []);
}

internal sealed record RouteInitRequest
{
    internal RouteInitRequest(
        CliWorkspace workspace,
        string routeTarget,
        RouteInitScaffold scaffold,
        RouteInitMode mode,
        RouteInitMetadataInput metadata)
    {
        ArgumentNullException.ThrowIfNull(workspace);
        ArgumentException.ThrowIfNullOrWhiteSpace(routeTarget);
        ArgumentNullException.ThrowIfNull(metadata);
        if (!Enum.IsDefined(scaffold))
        {
            throw new ArgumentOutOfRangeException(
                nameof(scaffold),
                scaffold,
                "The Route Init scaffold is not defined.");
        }

        if (!Enum.IsDefined(mode))
        {
            throw new ArgumentOutOfRangeException(
                nameof(mode),
                mode,
                "The Route Init mode is not defined.");
        }

        Workspace = workspace;
        RouteTarget = routeTarget;
        Scaffold = scaffold;
        Mode = mode;
        Metadata = metadata;
    }

    internal CliWorkspace Workspace { get; }

    internal string RouteTarget { get; }

    internal RouteInitScaffold Scaffold { get; }

    internal RouteInitMode Mode { get; }

    internal RouteInitMetadataInput Metadata { get; }

    internal bool IsDryRun => Mode == RouteInitMode.DryRun;
}
