using System.Text;
using OpenForge.Cli.Core.Presentation.Shared.Prompts.Models;
using OpenForge.Cli.Core.Presentation.Shared.Text;
using OpenForge.Cli.Core.Presentation.Shared.Wording;
using OpenForge.Cli.Core.Shell.Interaction.Models;

namespace OpenForge.Cli.Core.Presentation.Shared.Prompts;

internal sealed partial class CliPrompts
{
    private const int PathPageControlRows = 1;
    private const int MinimumPathRows = 1;

    internal ValueTask<CliPromptReply<CliPermissionChoice>> PermissionAsync(CliPermissionQuestion question, CliPromptPolicy policy, CancellationToken cancellationToken)
        => RunAsync<CliPermissionQuestion, CliPermissionChoice>(question, policy, cancellationToken,
            static (prompts, question, token) => prompts.PermissionCoreAsync(question, token));

    private async ValueTask<CliPromptReply<CliPermissionChoice>> PermissionCoreAsync(CliPermissionQuestion question, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(question.Owner);
        ArgumentNullException.ThrowIfNull(question.Paths);
        var choices = new[]
        {
            new CliChoice<CliPermissionChoice>(CliPermissionChoice.Always, CliPromptWording.Always(), CliPromptWording.AlwaysReason()),
            new CliChoice<CliPermissionChoice>(CliPermissionChoice.Once, CliPromptWording.Once(), CliPromptWording.OnceReason()),
            new CliChoice<CliPermissionChoice>(CliPermissionChoice.Cancel, CliPromptWording.Cancel(), CliPromptWording.CancelReason()),
        };
        var cursor = 0;
        var pathOffset = 0;
        while (_terminal.ReadSelectionViewport() is { } viewport)
        {
            var width = viewport.Width - CliSelectionFrameRenderer.WidthReserve;
            var ownerHeading = CliPromptWording.Permission(question.Owner);
            var compactOwner = CliSelectionText.Wrap(ownerHeading, width).Count > 1;
            var frameHeading = compactOwner ? CliPromptWording.PermissionHeading() : ownerHeading;
            var controls = new[] { CliPromptWording.SelectControls(choices.Length) };
            var controlRows = controls.SelectMany(text => CliSelectionText.WrapItems(text, width)).Count();
            var stickyContext = new List<string>();
            if (compactOwner)
            {
                stickyContext.Add(CliSelectionText.ClipMiddle(question.Owner, width));
                if (question.Paths.FirstOrDefault() is { } path) stickyContext.Add(CliSelectionText.ClipMiddle(PermissionPath(path), width));
            }
            var reviewText = question.Paths.Select(PermissionPath);
            if (compactOwner) reviewText = reviewText.Prepend(question.Owner);
            var pathRows = reviewText.SelectMany(text => CliSelectionText.Wrap(text, width)).ToArray();
            var pageSize = Math.Max(MinimumPathRows, CliSelectionFrameRenderer.ContextCapacity(viewport, frameHeading, controlRows)
                - PathPageControlRows - stickyContext.Count);
            var pageCount = Math.Max(1, (pathRows.Length + pageSize - 1) / pageSize);
            var page = Math.Min(pathOffset / pageSize, pageCount - 1);
            pathOffset = page * pageSize;
            var context = stickyContext.Concat(pathRows.Skip(pathOffset).Take(pageSize)).Append(CliPromptWording.PathPage(page, pageCount)).ToArray();
            var frame = new CliSelectionFrame
            {
                Question = frameHeading,
                Rows = choices.Select(choice => new CliSelectionRow(choice.Label)).ToArray(),
                Focus = cursor,
                Position = CliPromptWording.Position(cursor, choices.Length),
                Context = context,
                Details = choices[cursor].Description is { } description ? [description] : [],
                Controls = controls,
            };
            if (!await DrawSelectionAsync(frame, viewport, cancellationToken).ConfigureAwait(false)) break;
            var key = await _terminal.ReadKeyAsync(cancellationToken).ConfigureAwait(false);
            if (key is null || key.Value.Key == CliKey.Escape) return CliPromptReply<CliPermissionChoice>.Cancelled();
            if (key.Value.Key == CliKey.Up) cursor = (cursor + choices.Length - 1) % choices.Length;
            else if (key.Value.Key == CliKey.Down) cursor = (cursor + 1) % choices.Length;
            else if (key.Value.Key == CliKey.PageUp) pathOffset = Math.Max(0, pathOffset - pageSize);
            else if (key.Value.Key == CliKey.PageDown) pathOffset = Math.Min((pageCount - 1) * pageSize, pathOffset + pageSize);
            else if (key.Value.Key == CliKey.Enter) return PermissionReply(choices[cursor].Value);
            else if (key.Value.Key == CliKey.Character && key.Value.Character is >= '1' and <= '3')
                return PermissionReply(choices[key.Value.Character - '1'].Value);
        }
        var heading = new StringBuilder().Append(CliText.Escape(CliPromptWording.Permission(question.Owner))).Append("\n\n");
        foreach (var path in question.Paths) heading.Append("  ").Append(CliText.Escape(PermissionPath(path))).Append('\n');
        heading.Append('\n');
        foreach (var choice in choices)
            heading.AppendLine($"  {CliText.Escape(choice.Label)}  {CliText.Escape(choice.Description ?? string.Empty)}");
        await WriteFrameAsync(heading.ToString(), cancellationToken).ConfigureAwait(false);
        while (true)
        {
            await WriteFrameAsync("\n" + CliPromptWording.PermissionLine() + "\n", cancellationToken).ConfigureAwait(false);
            var answer = (await _terminal.ReadLineAsync(cancellationToken).ConfigureAwait(false))?.Trim().ToLowerInvariant();
            if (answer is null or "" or "cancel") return CliPromptReply<CliPermissionChoice>.Cancelled();
            if (answer == "always") return PermissionReply(CliPermissionChoice.Always);
            if (answer == "once") return PermissionReply(CliPermissionChoice.Once);
            await WriteFrameAsync(CliPromptWording.PermissionLineRule() + "\n", cancellationToken).ConfigureAwait(false);
        }
    }

    private static string PermissionPath(CliPermissionPath path)
        => path.IsDirectory ? CliPromptWording.Directory(path.Path) : path.Path;

    private static CliPromptReply<CliPermissionChoice> PermissionReply(CliPermissionChoice choice)
        => choice == CliPermissionChoice.Cancel
            ? CliPromptReply<CliPermissionChoice>.Cancelled()
            : CliPromptReply<CliPermissionChoice>.Answered(choice);
}
