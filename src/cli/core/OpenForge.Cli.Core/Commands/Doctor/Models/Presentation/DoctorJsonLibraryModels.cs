using OpenForge.Cli.Core.Framework.Libraries.Models.Record;

namespace OpenForge.Cli.Core.Commands.Doctor.Models.Presentation;

internal sealed record DoctorJsonLibrary
{
    public required string State { get; init; }
    public required string RecordPath { get; init; }
    public required string RecordState { get; init; }
    public required LibrariesRecordDocument? Record { get; init; }
    public required DoctorJsonLibraryRoot[] Roots { get; init; }
    public required DoctorJsonLibraryMapping[] Mappings { get; init; }
    public required DoctorJsonEvidence[] Ownership { get; init; }
    public required string? LinkCapability { get; init; }
    public required string? LinkCapabilityEvidence { get; init; }
}

internal sealed record DoctorJsonLibraryRoot
{
    public required string SourceRoot { get; init; }
    public required string State { get; init; }
    public required string? LexicalPath { get; init; }
    public required string? PhysicalPath { get; init; }
    public required bool? LexicallyContained { get; init; }
    public required bool? PhysicallyContained { get; init; }
    public required bool? PhysicallyDisjoint { get; init; }
    public required string? PhysicalAgentsDirectory { get; init; }
    public required string[]? InventoryPaths { get; init; }
    public required DoctorJsonEvidence[] ExcludedPaths { get; init; }
    public required DoctorJsonEvidence[] UnavailablePaths { get; init; }
    public required string? Cause { get; init; }
}

internal sealed record DoctorJsonLibraryMapping
{
    public required string SourcePath { get; init; }
    public required string DestinationPath { get; init; }
    public required string ExpectedRelativeLink { get; init; }
    public required string State { get; init; }
    public required string LeafState { get; init; }
    public required string? LinkKind { get; init; }
    public required string? RawLinkTarget { get; init; }
    public required string? LinkTargetForm { get; init; }
    public required string? Cause { get; init; }
}

internal sealed record DoctorJsonLibrarySubject
{
    public required string LibraryId { get; init; }
    public required string? SourceRoot { get; init; }
    public required string? SourcePath { get; init; }
    public required string? DestinationPath { get; init; }
    public required string? ExpectedRelativeLink { get; init; }
    public required string? BundlePath { get; init; }
    public required int? EntryOrdinal { get; init; }
}
