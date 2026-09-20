using OpenForge.Cli.Core.Commands.Library.Models.Application;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Receipts;

namespace OpenForge.Cli.Core.Commands.Library.Shared.Completion.Models;

internal sealed record LibraryMutationEffectReceipt(
        string Path,
        LibraryResidualKind Kind,
        FilesystemEffectState Effect,
        FilesystemVerificationState Verification);
