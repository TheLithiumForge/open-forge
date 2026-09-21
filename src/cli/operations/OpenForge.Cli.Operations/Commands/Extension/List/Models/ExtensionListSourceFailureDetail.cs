namespace OpenForge.Cli.Core.Commands.Extension.List.Models;

internal enum ExtensionListSourceFailureDetailKind
{
    InvalidManifest,
    InvalidEncoding,
    AccessDenied,
    FileInUse,
    InputOutput,
}

internal sealed record ExtensionListSourceFailureDetail
{
    internal ExtensionListSourceFailureDetail(string path, ExtensionListSourceFailureDetailKind kind)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        if (!Enum.IsDefined(kind))
        {
            throw new ArgumentOutOfRangeException(
                nameof(kind),
                kind,
                "The Extension List source failure detail kind is not defined.");
        }

        Path = path;
        Kind = kind;
    }

    internal string Path { get; }

    internal ExtensionListSourceFailureDetailKind Kind { get; }
}
