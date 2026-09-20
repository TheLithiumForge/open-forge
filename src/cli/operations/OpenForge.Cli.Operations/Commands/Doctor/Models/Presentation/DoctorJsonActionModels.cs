namespace OpenForge.Cli.Core.Commands.Doctor.Models.Presentation;

internal sealed class DoctorJsonAction
{
    public required string Kind { get; init; }

    public required string? Operation { get; init; }

    public required string? Command { get; init; }

    public required string Reason { get; init; }
}

internal sealed class DoctorJsonLocation
{
    public required int Line { get; init; }

    public required int Column { get; init; }

    public required long ByteOffset { get; init; }

    public required long ByteLength { get; init; }
}
