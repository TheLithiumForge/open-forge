using OpenForge.Cli.Core.Commands.Repair.Models.Planning;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Files;

namespace OpenForge.Cli.Core.Commands.Repair.Models.Result;

internal sealed record RepairEffectVerification(
    RepairEffect Effect,
    FileChangeReceipt? Receipt,
    RepairVerificationState Targets,
    RepairVerificationState ResultingBytes);
