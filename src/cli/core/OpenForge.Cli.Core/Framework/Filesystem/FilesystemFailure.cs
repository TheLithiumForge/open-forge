namespace OpenForge.Cli.Core.Framework.Filesystem;

internal enum FilesystemFailureKind
{
    AccessDenied,
    InputOutput,
    InvalidPath,
    InvalidEncoding,
    InvalidSyntax,
    Unsupported,
}

internal sealed record FilesystemFailure
{
    private const int MaximumCauseLength = 256;

    internal FilesystemFailure(FilesystemFailureKind kind, string directCause)
    {
        if (!Enum.IsDefined(kind))
        {
            throw new ArgumentOutOfRangeException(nameof(kind), kind, "The filesystem failure kind is not defined.");
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(directCause);
        Kind = kind;
        DirectCause = directCause.Length <= MaximumCauseLength
            ? directCause
            : directCause[..MaximumCauseLength];
    }

    internal FilesystemFailureKind Kind { get; }

    internal string DirectCause { get; }

    internal static FilesystemFailure FromException(FilesystemFailureKind kind, Exception exception)
    {
        ArgumentNullException.ThrowIfNull(exception);
        var summary = kind switch
        {
            FilesystemFailureKind.AccessDenied => "Filesystem access was denied.",
            FilesystemFailureKind.InputOutput => "The filesystem operation failed.",
            FilesystemFailureKind.InvalidPath => "The path is invalid.",
            FilesystemFailureKind.InvalidEncoding => "The file is not valid UTF-8.",
            FilesystemFailureKind.InvalidSyntax => "The document syntax is invalid.",
            FilesystemFailureKind.Unsupported => "The filesystem operation is unsupported.",
            _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, "The filesystem failure kind is not defined."),
        };
        return new FilesystemFailure(
            kind,
            $"{exception.GetType().Name} (0x{exception.HResult:X8}): {summary}");
    }
}
