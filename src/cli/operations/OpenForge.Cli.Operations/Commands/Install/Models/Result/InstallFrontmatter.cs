using System.Collections.Immutable;
using OpenForge.Cli.Core.Framework.Filesystem.Shared.Paths;

namespace OpenForge.Cli.Core.Commands.Install.Models.Result;

internal enum InstallFrontmatterKeptReason
{
    Edited,
    SourceUnavailable,
}

internal sealed record InstallFrontmatterKeptFile
{
    internal InstallFrontmatterKeptFile(string path, InstallFrontmatterKeptReason reason)
    {
        if (!PortableWorkspacePath.TryNormalize(path, out var normalizedPath)
            || !string.Equals(path, normalizedPath, StringComparison.Ordinal))
        {
            throw new ArgumentException(
                "Kept frontmatter paths must be canonical workspace-relative paths.",
                nameof(path));
        }

        if (!Enum.IsDefined(reason))
        {
            throw new ArgumentOutOfRangeException(nameof(reason), reason, "The kept frontmatter reason is not defined.");
        }

        Path = path;
        Reason = reason;
    }

    internal string Path { get; }

    internal InstallFrontmatterKeptReason Reason { get; }
}

internal sealed record InstallFrontmatter
{
    internal const string Root = "root";
    internal const string Scoped = "scoped";

    internal InstallFrontmatter(string form, string? previousForm, IEnumerable<InstallFrontmatterKeptFile> kept)
    {
        if (!IsForm(form))
        {
            throw new ArgumentException("The frontmatter form must be root or scoped.", nameof(form));
        }

        if (previousForm is not null && !IsForm(previousForm))
        {
            throw new ArgumentException("The previous frontmatter form must be root or scoped.", nameof(previousForm));
        }

        ArgumentNullException.ThrowIfNull(kept);

        Form = form;
        PreviousForm = previousForm;
        Kept = [.. kept.OrderBy(file => file.Path, StringComparer.Ordinal)];
    }

    internal string Form { get; }

    internal string? PreviousForm { get; }

    internal ImmutableArray<InstallFrontmatterKeptFile> Kept { get; }

    internal bool Changed => PreviousForm is not null && !string.Equals(PreviousForm, Form, StringComparison.Ordinal);

    private static bool IsForm(string value) => value is Root or Scoped;
}
