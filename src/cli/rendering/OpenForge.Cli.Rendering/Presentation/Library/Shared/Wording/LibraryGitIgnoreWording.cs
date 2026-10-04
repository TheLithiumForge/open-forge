using OpenForge.Cli.Core.Presentation.Shared.Models;
using OpenForge.Cli.OutputText.Library.Shared;

namespace OpenForge.Cli.Core.Presentation.Library.Shared.Wording;

internal static class LibraryGitIgnoreWording
{
    internal static string Partial(string operation, IReadOnlyList<CliEffect> effects, bool cancelled)
    {
        var verified = effects.Count(effect => effect.Outcome == CliEffectOutcome.Done);
        return cancelled
            ? LibraryGitIgnoreText.CancelledAfter(operation, verified, effects.Count)
            : LibraryGitIgnoreText.StoppedAfter(operation, verified, effects.Count);
    }

    internal static string Effect(CliEffectOutcome outcome) => outcome switch
    {
        CliEffectOutcome.Planned => LibraryGitIgnoreText.WouldUpdate(),
        CliEffectOutcome.Done => LibraryGitIgnoreText.Updated(),
        CliEffectOutcome.Unknown => LibraryGitIgnoreText.Unknown(),
        CliEffectOutcome.Failed => LibraryGitIgnoreText.Failed(),
        CliEffectOutcome.NotStarted => LibraryGitIgnoreText.NotStarted(),
        _ => throw new ArgumentOutOfRangeException(nameof(outcome), outcome, "The ignore effect outcome is not defined."),
    };
}
