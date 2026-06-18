# Handoffs

## Essence

Handoffs defines what `{forgePath}/handoffs/_handoffs.md` and `{forgePath}/handoffs/` should do in the installed framework.

Handoffs help another human or agent continue safely without replaying the whole session.

## Use When

- Work is unfinished and someone else may continue it.
- An agent needs to pass context to another agent.
- A subagent needs to report back to an orchestrator.
- The user needs a compact "next safe action" note.

## Do Not Use When

- The note is meant to become durable memory.
- The user asked to save the chat as a session.
- The information belongs in active docs, directives, guides, or patterns.

## Default Rules

- Prefer local handoffs beside the work they describe.
- Use `{forgePath}/handoffs/` only when there is no useful local owner.
- After a handoff is consumed, delete it, archive it, or summarize it into a session.
- Archived handoffs go under `{forgePath}/handoffs/archive/` unless a local archive is more appropriate.

## Useful Notes

Handoffs are allowed to be rougher than sessions. Their job is continuity, not long-term memory.
