namespace OpenForge.Cli.Core.Framework.Extensions.Models;

internal enum ExtensionSourceFailureDetailKind
{
    InvalidManifest,
    InvalidEncoding,
    AccessDenied,
    FileInUse,
    InputOutput,
}

internal sealed record ExtensionSourceFailureDetail
{
    internal ExtensionSourceFailureDetail(string path, ExtensionSourceFailureDetailKind kind)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        if (!Enum.IsDefined(kind))
        {
            throw new ArgumentOutOfRangeException(
                nameof(kind),
                kind,
                "The Extension source failure detail kind is not defined.");
        }

        Path = path;
        Kind = kind;
    }

    internal string Path { get; }

    internal ExtensionSourceFailureDetailKind Kind { get; }
}
