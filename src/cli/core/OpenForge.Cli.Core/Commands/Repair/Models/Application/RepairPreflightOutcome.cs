using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem;

namespace OpenForge.Cli.Core.Commands.Repair.Models.Application;

internal sealed record RepairPreflightOutcome(
    RepairApplicationOutcome Outcome,
    IReadOnlyList<FileExpectation> Targets);
