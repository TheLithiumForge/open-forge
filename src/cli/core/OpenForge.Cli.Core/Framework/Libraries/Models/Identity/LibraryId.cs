using OpenForge.Cli.Core.Framework.Filesystem.LogicalPaths.Models;
using OpenForge.Cli.Core.Framework.Libraries.Shared.Paths;
using System.Collections.Immutable;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;

namespace OpenForge.Cli.Core.Framework.Libraries.Models.Identity;

internal sealed record LibraryId
{
    internal const int MaximumLength = 128;
    private LibraryId(string value)
    {
        Value = value;
    }

    internal string Value { get; }

    internal static LibraryId Create(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);
        if (value.Length > MaximumLength
            || value.Any(character => character is not (>= 'a' and <= 'z'
                or >= '0' and <= '9'
                or '-'))
            || value.StartsWith('-')
            || value.EndsWith('-')
            || value.Contains("--", StringComparison.Ordinal))
        {
            throw new ArgumentException(
                "A Library ID must match [a-z0-9]+(-[a-z0-9]+)* and be at most 128 characters.",
                nameof(value));
        }

        return new LibraryId(value);
    }
}

internal sealed record WorkspaceRelativeDirectory
{
    private WorkspaceRelativeDirectory(string value)
    {
        Value = value;
    }

    internal string Value { get; }

    internal static WorkspaceRelativeDirectory Create(string value)
        => new(PortableRelativePath.Validate(value, nameof(value), requireAgentsPrefix: false));
}

internal sealed record SourceRelativeEligiblePath
{
    private SourceRelativeEligiblePath(string value)
    {
        Value = value;
    }

    internal string Value { get; }

    internal static SourceRelativeEligiblePath Create(string value)
        => new(PortableRelativePath.Validate(value, nameof(value), requireAgentsPrefix: true));
}

internal sealed record WorkspaceRelativeEligiblePath
{
    private WorkspaceRelativeEligiblePath(string value)
    {
        Value = value;
    }

    internal string Value { get; }

    internal CanonicalRelativePath CanonicalPath
        => CanonicalRelativePath.Create(Value);

    internal static WorkspaceRelativeEligiblePath Create(string value)
        => new(PortableRelativePath.Validate(value, nameof(value), requireAgentsPrefix: true));
}

internal sealed record RawRelativeLinkTarget
{
    private RawRelativeLinkTarget(string value)
    {
        Value = value;
    }

    internal string Value { get; }

    internal static RawRelativeLinkTarget Create(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);
        if (Path.IsPathFullyQualified(value)
            || value.Contains((char)92)
            || value.StartsWith('/'))
        {
            throw new ArgumentException(
                "A relative link target must be a non-empty slash-separated relative path.",
                nameof(value));
        }

        return new RawRelativeLinkTarget(value);
    }
}
