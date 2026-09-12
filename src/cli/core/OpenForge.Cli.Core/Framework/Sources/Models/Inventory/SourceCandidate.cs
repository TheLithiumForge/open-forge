using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths.Models;
using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.Identity;

namespace OpenForge.Cli.Core.Framework.Sources.Models.Inventory;

internal sealed class SourceCandidate
{
    internal SourceCandidate(
        string canonicalPath,
        SourceDocumentForm? form,
        string? automaticId,
        PhysicalPathState physicalState,
        string? physicalPath,
        string? physicalParentPath)
    {
        if (!SourceLogicalPath.IsCanonicalSource(canonicalPath))
        {
            throw new ArgumentException("The source candidate path is not a canonical .agents source path.", nameof(canonicalPath));
        }

        if (form is not null && !Enum.IsDefined(form.Value))
        {
            throw new ArgumentOutOfRangeException(nameof(form), form, "The source candidate form is not defined.");
        }

        if (automaticId is not null)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(automaticId);
        }

        if (form is null && automaticId is not null)
        {
            throw new ArgumentException("An unrecognized source candidate cannot carry an automatic ID.", nameof(automaticId));
        }

        if (!Enum.IsDefined(physicalState))
        {
            throw new ArgumentOutOfRangeException(nameof(physicalState), physicalState, "The physical path state is not defined.");
        }

        var normalizedPhysicalPath = NormalizeOptionalPhysicalPath(physicalPath, nameof(physicalPath));
        var normalizedPhysicalParentPath = NormalizeOptionalPhysicalPath(physicalParentPath, nameof(physicalParentPath));
        if (normalizedPhysicalParentPath is null)
        {
            throw new ArgumentException("A source candidate requires its proven contained parent path.", nameof(physicalParentPath));
        }

        if (physicalState == PhysicalPathState.Contained
            && normalizedPhysicalPath is null)
        {
            throw new ArgumentException("A contained source candidate requires its physical path and parent path.", nameof(physicalPath));
        }

        if (physicalState != PhysicalPathState.Contained
            && normalizedPhysicalPath is not null)
        {
            throw new ArgumentException("Only a contained source candidate can carry its resolved physical path.", nameof(physicalPath));
        }

        CanonicalPath = canonicalPath;
        Form = form;
        AutomaticId = automaticId;
        PhysicalState = physicalState;
        PhysicalPath = normalizedPhysicalPath;
        PhysicalParentPath = normalizedPhysicalParentPath;
    }

    internal string CanonicalPath { get; }

    internal SourceDocumentForm? Form { get; }

    internal string? AutomaticId { get; }

    internal PhysicalPathState PhysicalState { get; }

    internal string? PhysicalPath { get; }

    internal string PhysicalParentPath { get; }

    private static string? NormalizeOptionalPhysicalPath(string? path, string parameterName)
    {
        if (path is null)
        {
            return null;
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(path, parameterName);
        var normalized = Path.GetFullPath(path);
        if (!Path.IsPathRooted(path)
            || !string.Equals(normalized, path, StringComparison.Ordinal))
        {
            throw new ArgumentException("A physical path must be absolute and normalized.", parameterName);
        }

        return normalized;
    }
}
