using OpenForge.Cli.Core.Commands.Repair.Models.Selection;

namespace OpenForge.Cli.Core.Commands.Repair.Models.Planning;

internal sealed record RepairSelectionEntry(
        RepairProposalInput Input,
        IReadOnlyList<RepairConflict> Conflicts);

internal sealed record RepairOccurrenceIdentity(
        string SourceCanonicalPath,
        int Line,
        int Column,
        long ByteOffset,
        long ByteLength);
