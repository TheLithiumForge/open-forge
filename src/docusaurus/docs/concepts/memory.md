---
title: Memory
description: The four Memory states, what belongs in each, and how records move between them.
---

# Memory

Memory asks one question: **what is worth remembering for current or future work?** It's plain Markdown that grows with the work. People and agents add useful records and scopes as they go, and unselected branches never enter active context.

## Four states

| State            | Question it answers                                                         | Normal status             | Loads at startup?                  |
| ---------------- | --------------------------------------------------------------------------- | ------------------------- | ---------------------------------- |
| **Working**      | What temporary context is needed to continue or resume this work?           | `#Contextual`             | Yes (`#LoadNow`)                   |
| **Emerging**     | What is useful but still unsettled?                                         | `#Contextual`             | Yes, and refreshed (`#KeepInMind`) |
| **Crystallized** | What accepted knowledge should remain current within this scope?            | `#CurrentTruth`           | Yes (`#LoadNow`)                   |
| **Archived**     | What useful history should remain available without governing current work? | `#Contextual`, historical | No                                 |

The states describe how to treat material. They're not quality scores or required stages, and a record doesn't have to pass through every state.

### Working

Temporary state for active work: where a task stands, what's next, what must not be lost. It's expected to expire. When the need ends, keep the useful results and archive or prune the rest.

### Emerging

Useful material that isn't accepted yet: a finding, an investigation, an idea worth revisiting. At each refresh point the agent checks whether anything useful should be saved before it's lost. "Nothing useful to save" is a valid answer.

### Crystallized

Accepted knowledge that should stay current: current documents, accepted decisions. Update, split, or merge existing records instead of creating a competing version. Tags, repetition, and agent confidence don't make something accepted.

### Archived

History that no longer governs current work. Before archiving, move anything still current into the source that defines it, and keep a note of where the material came from and what replaced it.

## What gets saved

Memory holds what's worth keeping. Agents save outcomes deliberately rather than logging every conversation. Capture is warranted when:

- you explicitly ask to preserve something
- work needs to survive a pause, a context boundary, or a handoff
- an occurrence is reusable, surprising, or costly to rediscover
- accepted reasoning or current state would otherwise exist only in chat

## One source per question

One rule keeps Memory from turning into a pile of notes: each source defines only its own part of the truth.

- A **Decision** records what was chosen and why.
- A current **Document** explains its subject as it is now.
- Required behavior belongs in **Directives** or Axioms. Writing an instruction into Memory doesn't make it a Directive.

When something is accepted, update the source that defines it and keep the useful reasoning in Memory.

## Categories inside the states

The base Framework ships only the four state entrypoints. Extensions add named categories inside them:

| Category     | Path                             | Supplied by                                                             |
| ------------ | -------------------------------- | ----------------------------------------------------------------------- |
| Checkpoints  | `memory/working/checkpoints/`    | [Planning](../extensions/planning.md)                                   |
| Handoffs     | `memory/working/handoffs/`       | [Observations and Handoffs](../extensions/observations-and-handoffs.md) |
| Ideas        | `memory/emerging/ideas/`         | [Planning](../extensions/planning.md)                                   |
| Analysis     | `memory/emerging/analysis/`      | [Planning](../extensions/planning.md)                                   |
| Observations | `memory/emerging/observations/`  | [Observations and Handoffs](../extensions/observations-and-handoffs.md) |
| Decisions    | `memory/crystallized/decisions/` | [Planning](../extensions/planning.md)                                   |
| Documents    | `memory/crystallized/documents/` | [Project Documents](../extensions/project-documents.md)                 |

Without them, records can live directly under a state. These categories are useful defaults, not a required taxonomy.

## Scoping Memory

Memory uses the same [scopes](scopes.md) as everything else. `memory/mobile-app/crystallized/` gives a subject its own Memory. `memory/crystallized/documents/mobile-app/` narrows only its Documents.

Adding a new top-level state beneath `memory/` changes the shared model, so it needs your explicit agreement. Adding a scope is ordinary customization.

Next: [Customizing](customizing.md).
