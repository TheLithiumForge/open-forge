---
title: Memory
description: The four Memory states, what belongs in each, and how records move between them.
---

# Memory

Memory asks one question: **what is worth remembering for current or future work?** It's self-growing Markdown state: people and agents add useful records and routed scopes as the work produces them. Unselected branches don't enter active context.

## Four states

| State            | Question it answers                                                         | Normal status             | At startup                                        |
| ---------------- | --------------------------------------------------------------------------- | ------------------------- | ------------------------------------------------- |
| **Working**      | What temporary context is needed to continue or resume this work?           | `#Contextual`             | Entrypoint at startup                             |
| **Emerging**     | What is useful but still unsettled?                                         | `#Contextual`             | Entrypoint at startup, and at every refresh point |
| **Crystallized** | What accepted knowledge should remain current within this scope?            | `#CurrentTruth`           | Entrypoint at startup                             |
| **Archived**     | What useful history should remain available without governing current work? | `#Contextual`, historical | Entrypoint on demand                              |

"Entrypoint at startup" means the agent reads the state's entrypoint and its list of entries, not the records. Emerging is re-read at each refresh point because it's tagged `#KeepInMind`. [Loading and tags](loading-and-tags.md#what-loads-at-startup-in-a-fresh-install) defines these terms.

The states describe how to treat material. They're not quality scores or required stages, and a record doesn't have to pass through every state.

### Working

Temporary state for work in progress: where a task stands, what's next, and what must not be lost. Examples include a Checkpoint from the [Planning](../extensions/planning.md) Extension or a Handoff from the [Observations and Handoffs](../extensions/observations-and-handoffs.md) Extension. It's expected to expire. When the need ends, keep the useful results, then move, archive, consolidate, or prune the record.

### Emerging

Useful material that isn't accepted yet, such as a finding, an investigation, or an idea worth revisiting. Examples include an Idea from Planning or an Observation from Observations and Handoffs. At each refresh point the rules ask the agent to check whether anything useful should be saved before it's lost. "Nothing useful to save" is a valid answer.

### Crystallized

Accepted knowledge that should stay current. Examples include a current Document from the [Project Documents](../extensions/project-documents.md) Extension or a Decision from Planning. Update, split, or merge existing records instead of creating a competing version. Tags, repetition, and agent confidence don't make something accepted.

### Archived

History that no longer governs current work. Before archiving, move anything still current into the source that defines it. Keep a note of where the material came from, why it was archived, and what replaced it.

## What gets saved

Memory holds what's worth keeping. The rules ask agents to save outcomes deliberately rather than log every conversation. They call for a record when:

- you explicitly ask to preserve something
- future work needs an accepted result, its reasoning, a required behavior, or a reusable shape
- an unsettled finding is worth revisiting
- work needs to survive a pause, a context boundary, or a handoff
- accepted knowledge would otherwise exist only in chat

## One source per question

Each source defines only its own part of the truth. This keeps Memory from filling up with competing copies of the same answer.

- Required behavior belongs in **Directives** or Axioms, which are Core. Writing an instruction into Memory doesn't make it a Directive.
- A **Decision**, from Planning, records what was chosen and why.
- A current **Document**, from Project Documents, explains its subject as it is now.

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

These categories carry no loading tag. At startup only their one-line entry in the parent state's `Entries` is visible. The category entrypoint and its records open on demand.

Without them, records can live directly under a state. These categories are useful defaults, not a required taxonomy.

## Scoping Memory

Memory uses the same [scopes](scopes.md) as everything else. `memory/mobile-app/crystallized/` gives a subject its own Memory. `memory/crystallized/documents/mobile-app/` narrows only the Documents route.

Adding a new top-level state beneath `memory/` changes the shared model, so it needs your explicit direction. Adding a scope is ordinary customization.

Next: [Customizing](customizing.md).
