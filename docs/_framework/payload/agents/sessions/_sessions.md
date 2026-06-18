# Sessions

## Essence

Sessions defines what `{forgePath}/sessions/_sessions.md` and `{forgePath}/sessions/` should do in the installed framework.

Sessions are durable saved chat summaries.

## Use When

- The user explicitly asks to save a conversation.
- A future save-session skill is invoked.
- Important context should survive chat boundaries.
- A handoff was valuable enough to become durable memory.

## Do Not Use When

- The chat was casual and does not need to persist.
- A temporary handoff is enough.
- The content is an unreviewed observation.

## Default Rules

- Create sessions only by user request or explicit save-session workflow.
- Use `{yyyy-MM-dd}_{HHmm}_{slug}.md` for saved sessions.
- Archive old sessions in `{forgePath}/sessions/archive/`.
- Sessions may include decisions and context, but they do not override active truth.

## Useful Notes

A session is memory, not authority. If a session contains a rule that should govern future work, promote that rule into the right active file.
