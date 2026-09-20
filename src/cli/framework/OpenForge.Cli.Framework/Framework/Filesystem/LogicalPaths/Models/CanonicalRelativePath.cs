namespace OpenForge.Cli.Core.Framework.Filesystem.LogicalPaths.Models;

// Neutral repository-relative path identity shared by mechanical mutation and
// capability-specific models. It carries no capability-specific root policy.
internal sealed record CanonicalRelativePath
{
    private CanonicalRelativePath(string value)
    {
        Value = value;
    }

    internal string Value { get; }

    internal static CanonicalRelativePath Create(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);
        if (Path.IsPathFullyQualified(value)
            || value.Contains((char)92)
            || value.StartsWith('/')
            || value.EndsWith('/'))
        {
            throw new ArgumentException(
                "A canonical relative path must use slash-separated relative spelling.",
                nameof(value));
        }

        if (value.Split('/').Any(segment => segment is "" or "." or ".."))
        {
            throw new ArgumentException(
                "A canonical relative path cannot contain empty, dot, or dot-dot segments.",
                nameof(value));
        }

        return new CanonicalRelativePath(value);
    }
}
