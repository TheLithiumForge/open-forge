---
title: Observations and Handoffs
description: Memory categories and Templates for saving useful observations and sealing a snapshot when work changes hands.
---

# Observations and Handoffs

Save an observation worth revisiting, or give the next person, agent, or session enough information to resume work.

- **Package ID:** `observations-and-handoffs`
- **Depends on:** Nothing
- **Needed by:** [Task Coordination](orchestration.md)
- **Loads at startup:** Only the two category entry lines, through Working and Emerging Memory.

## Why it exists

Some things are worth remembering after seeing them once: a surprising failure, or a workaround that took an hour to find. Observations keep them without claiming more than the evidence shows.

Handoffs exist because resuming from memory, or from a live checkpoint that has changed since, loses state. Both used to ship with Task Coordination. They moved into their own package because solo work needs them too.

## What it installs

```text
.agents/
  memory/
    emerging/observations/_observations.md     <- Memory category: things noticed
    working/handoffs/_handoffs.md              <- Memory category: sealed transfer snapshots
  templates/observations-and-handoffs/
    _observations-and-handoffs.md              <- Template category entrypoint
    observation.md                             <- starter for one observation
    handoff.md                                 <- starter for one handoff
```

## What each file is for

### `memory/emerging/observations/_observations.md`

**Kind:** Memory category (Emerging). **Used when:** something concrete happened that may matter beyond the current work.

An Observation records an occurrence or pattern in evidence: a surprising result, a recurring failure, a detail that would be costly to rediscover. Its rules:

- Before handoff or closeout, record an Observation when something may matter later. One occurrence is enough if it's reusable, surprising, or costly.
- Keep the evidence clear enough to verify and reuse.
- Add matching later occurrences to the same Observation. Recurrence strengthens the case for promoting it, but doesn't validate it by itself.

### `memory/working/handoffs/_handoffs.md`

**Kind:** Memory category (Working). **Used when:** work is actually transferred, or a resumption across a context boundary is explicitly planned.

A Handoff is a **sealed snapshot**. Once written, it stays unchanged. Later state goes in the live Checkpoint or a new Handoff. Its rules:

- Create one only for a real transfer or planned resumption, not for a routine pause or a possible future interruption.
- Keep it short and link to details. Record the boundary status, next action, blockers, and verification state in the Handoff itself.
- Archive it when it no longer supports an active transfer.

### The Templates

| Template         | Main sections                                                                                                                                   |
| ---------------- | ----------------------------------------------------------------------------------------------------------------------------------------------- |
| `observation.md` | What Happened (expected, observed, scope), Evidence, What It Might Mean, Follow-Up (next check, possible action), Later Occurrences             |
| `handoff.md`     | For, Snapshot, Scope, Next Action, State At Transfer, Direction And Boundaries, Verification And Blockers, Required Context, Transfer Ends When |

The Observation starter keeps interpretation separate from fact: "What It Might Mean" is a candidate, and "Possible action" is explicitly not an accepted instruction.

## How to use it

> Preserve this useful observation without treating its explanation as settled.

> Prepare a handoff so another session can resume from the actual state.

## Good to know

- Either record works alone. Neither needs task coordination or a development workflow. An Observation can come from solo work, a conversation, or a tool.
- A Handoff isn't a status page. Keep changing task state in the existing working record.
- These categories used to ship with Task Coordination. Their Memory paths are unchanged, and their Templates now live under `templates/observations-and-handoffs/`.
