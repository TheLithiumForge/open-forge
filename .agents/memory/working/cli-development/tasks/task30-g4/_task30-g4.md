---
open-forge:
  description: Task 30 G4 execution packet for the CLI output revamp, with the accepted decisions, the shared rules, the command matrix, the lane order, and one subtask per foundation and per command
  tags: [Memory, Working, CLI, Task, Plan, G4, Presentation, Contextual, Active]
---

# Task 30 G4 — Output Revamp Packet

This folder is the execution packet for [Task 30 phase 4b, G4](../task30/phase-4b-g4.md).
It replaces the "not yet specified" execution slices in the Task 30 phase map.
Every subtask here follows the [Work Plan](../../../../../patterns/work-plan.md)
shape and the [conventions](00-conventions.md) below.

## Outcome

Every one of the 28 commands prints, for every status it can reach, at every
detail level, in text and in JSON, exactly the messages specified in its
subtask: one plain-language sentence first, the affected items with their
paths, the counts of everything healthy, and normally at most one next action.
Extension Install retains its catalogue-required advisory after `Next:` as an
open maintainer question. A healthy `doctor` prints two lines. The default
level is the smallest one. One
selection stage decides what a reader sees; renderers only write it. One data
model carries every result from the operation layer to rendering. One escaper,
one line-ending rule, one severity vocabulary. Interactive commands ask the
questions specified in the interaction subtask with keyboard selection, and
never ask outside a terminal.

Non-goals: the project split into separate assemblies (scheduled as a physical
layout only, see [03](03-rendering-system.md)), the phase 5 diagnosis and
interoperability changes, and the Extension selector's dependency solver.

Completion evidence: reviewed before snapshots committed alone; the rendering
system and interaction system merged; all 28 command subtasks merged with
their snapshot diffs reviewed against the catalogues; the verification task
green across all six executable modes; and the documentation propagation task
complete. The supported Native AOT gate was run by the overseer on an
unsandboxed host with `vswhere` on `PATH`.

## Accepted Decisions

Recorded from the maintainer's review of the
[consolidated proposal](../task30/phase-4b-g4-output-proposal-consolidated.md)
on 2026-09-14. These are the accepted gate decisions for every subtask. The
current wire vocabulary after implementation is summarized in [Closeout Notes](#closeout-notes).

> The table retains the gate's historical replacement wording; cells naming
> retired options or statuses are not current CLI syntax.

| ID  | Decision                                                                                                                                                                                                                                                      |
| --- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| C1  | `--detail minimal\|standard\|full\|debug`, default `minimal`. The top level is `debug`.                                                                                                                                                                       |
| C1a | `--detail-filter error\|warning\|info\|all`, repeatable. It selects which severities are listed at any level. Every command's findings are triaged into those three severities.                                                                               |
| C2  | `--format text\|json`, default `text`, replaces `--json`.                                                                                                                                                                                                     |
| C3  | `--verbose` retires. Its stderr diagnostics belong to `--detail debug`.                                                                                                                                                                                       |
| C4  | Severity words are `Error`, `Warning`, `Info`. No `Note`.                                                                                                                                                                                                     |
| C5  | Coverage-kind Doctor findings are aggregated in the result model (option B). No per-occurrence "link is valid" objects anywhere. Duplication is removed wherever it is found.                                                                                 |
| C6  | No caps on listed findings. `--detail-filter` is the filter. `--limit` is not added.                                                                                                                                                                          |
| C7  | One data model is handed from the operation layer to rendering, and rendering does everything else with it. One envelope, schema version 3. The detail level applies to JSON as well as text.                                                                 |
| C8  | Escaping displays content in the best way a terminal can show it: visible `\n`, `\r`, `\t`, `\uXXXX` for other controls, everything else literal. The CLI targets terminals only.                                                                             |
| C9  | Statuses are renamed for clarity; `attention` becomes a name that says the command finished with warnings. Names of statuses, classes and vocabulary are improved in [02](02-naming.md).                                                                      |
| C10 | No TSV promise. Text rows are readable columns; machines use JSON.                                                                                                                                                                                            |
| C11 | Framework `install` is human-first: enough, not overloaded. Creations are summarized by count and directory; the lock file holds the roster; every changed existing file is named.                                                                            |
| C12 | Workspace echo: my call, recorded as an assumption in [00](00-conventions.md#shared-presentation-rules).                                                                                                                                                      |
| C13 | Finding codes appear in text only at `full` and `debug`, and always in JSON. Progressive disclosure is used to the maximum.                                                                                                                                   |
| C14 | Interactive prompts are designed completely in [04](04-interaction-system.md): what to ask, keyboard selection with space, dependency marks with a legend.                                                                                                    |
| C15 | `references` counts authored links only. Established behavior, not reopened.                                                                                                                                                                                  |
| C16 | `index` rewrites only the list of entries inside `## Entries`, never text before or after it. Fixed in [05](05-index-entries-region.md).                                                                                                                      |
| C17 | Implementers update only their own task file and the source and test tree. Contracts, public documentation and memory records are updated afterwards by [41](41-documentation-propagation.md). This supersedes the same-commit contract rule for this packet. |
| C18 | The three-layer split (shell, data processing, rendering) is scheduled as a physical layout and dependency rule now, without separate projects.                                                                                                               |

## Plan

### Shared rules

The shared presentation rules every command follows are in
[00 — Conventions](00-conventions.md#shared-presentation-rules). The data model,
selection stage, renderers, escaping and line endings are designed in
[03 — Rendering system](03-rendering-system.md). Prompts are designed in
[04 — Interaction system](04-interaction-system.md).

### Command matrix

One row per command subtask. The subtask holds the complete message catalogue.

| Subtask                                         | Question                                            | `minimal` keeps                                                                     | Prompts                           |
| ----------------------------------------------- | --------------------------------------------------- | ----------------------------------------------------------------------------------- | --------------------------------- |
| [10 status](10-status.md)                       | Installed, current, what does startup cost?         | One sentence, startup line, Extensions, items needing attention, `Next`             | none                              |
| [11 doctor](11-doctor.md)                       | What is wrong and what do I do?                     | Counts, errors, checks that could not finish, hint when findings are hidden, `Next` | none                              |
| [12 install](12-install.md)                     | What was created, was anything existing touched?    | Sentence, creation counts, every changed existing file                              | confirm                           |
| [13 update](13-update.md)                       | What changed, what was kept, where is the old text? | Replaced, restored, deleted, kept-retired paths; previous-content pointer; `Next`   | confirm                           |
| [14 index](14-index.md)                         | Which Entries sections were rewritten?              | Sentence with N of M, changed files with entry counts                               | none                              |
| [15 repair](15-repair.md)                       | What was fixed, what needs me?                      | Repaired links old -> new, unresolved count, `Next`                                 | choose, confirm                   |
| [16 cleanup](16-cleanup.md)                     | What recovery data was removed?                     | Every deleted path                                                                  | none                              |
| [17 context](17-context.md)                     | Give me the documents, in order                     | Delimiters and byte-exact content                                                   | none                              |
| [18 find](18-find.md)                           | Which sources match?                                | One row per match; one sentence when empty                                          | none                              |
| [19 references](19-references.md)               | What links in and out?                              | In and out rows with locations                                                      | none                              |
| [20 route list](20-route-list.md)               | What routes exist?                                  | ID and description rows, depth trailer                                              | none                              |
| [21 route inspect](21-route-inspect.md)         | Where is it, when is it read, what does it cost?    | Identity line and the three question blocks                                         | choose                            |
| [22 route init](22-route-init.md)               | Which entrypoints were created?                     | Created entrypoints, parent Entries updated, placeholder advisory                   | none                              |
| [23 route create](23-route-create.md)           | Was the file created and listed?                    | Created path with ID, parent Entries updated                                        | none                              |
| [24 route update](24-route-update.md)           | Which metadata changed?                             | Field changes old -> new, Entries updated                                           | choose                            |
| [25 route move](25-route-move.md)               | What moved and which links were rewritten?          | Old -> new, Entries updated, rewritten links                                        | choose                            |
| [26 route remove](26-route-remove.md)           | What was removed, what happened to incoming links?  | Removed paths, Entries updated, detached links, recovery path                       | choose                            |
| [27 extension list](27-extension-list.md)       | What is installed, what is available?               | Installed rows, available rows with descriptions, `Next`                            | none                              |
| [28 extension inspect](28-extension-inspect.md) | Does the installed package match?                   | Sentence with versions, paths needing attention                                     | none                              |
| [29 extension create](29-extension-create.md)   | Where is the scaffold?                              | Created scaffold paths, where to edit                                               | text input                        |
| [30 extension install](30-extension-install.md) | Which packages and files were installed?            | Packages, created files, Entries updated, grants, recovery                          | multi-select, permission, confirm |
| [31 extension update](31-extension-update.md)   | What changed against the source?                    | Replaced, restored, kept-retired paths, recovery                                    | multi-select, permission, confirm |
| [32 extension remove](32-extension-remove.md)   | Which claims and files were removed?                | Deleted paths, kept shared paths, Entries updated, retained bundle, `Next`          | multi-select, confirm             |
| [33 library list](33-library-list.md)           | Which Libraries and links are registered?           | One row per Library, links needing attention, `Next`                                | none                              |
| [34 library inspect](34-library-inspect.md)     | Does the source match the projection?               | Sentence with counts, paths needing attention                                       | none                              |
| [35 library attach](35-library-attach.md)       | What was registered and linked?                     | Sentence, created links, Entries updated, grants, recovery                          | permission                        |
| [36 library sync](36-library-sync.md)           | Which links were added or removed?                  | Added, removed, unchanged counts with the changed paths                             | permission                        |
| [37 library detach](37-library-detach.md)       | Which links were removed?                           | Removed links, source-kept sentence                                                 | permission                        |

### Order and lanes

| Order | Subtask                                                         | Depends on                 | Lane                          | Changes output |
| ----- | --------------------------------------------------------------- | -------------------------- | ----------------------------- | -------------- |
| 1     | [01 before snapshots](01-before-snapshots.md)                   | none                       | sequential                    | no             |
| 2     | [02 naming](02-naming.md)                                       | none                       | sequential (decision)         | no             |
| 3     | [03 rendering system](03-rendering-system.md)                   | 01, 02                     | sequential                    | yes            |
| 3     | [05 index Entries region](05-index-entries-region.md)           | 01                         | parallel with 03              | yes            |
| 4     | [04 interaction system](04-interaction-system.md)               | 03                         | sequential                    | yes            |
| 5     | 10, 11                                                          | 03                         | lane A                        | yes            |
| 5     | 12, 13, 14, 15, 16                                              | 03; 12, 13, 15 also 04     | lane B                        | yes            |
| 5     | 17, 18, 19                                                      | 03                         | lane C                        | yes            |
| 5     | 20 to 26                                                        | 03; 21, 24, 25, 26 also 04 | lane D                        | yes            |
| 5     | 27 to 32                                                        | 03, 04                     | lane E                        | yes            |
| 5     | 33 to 37                                                        | 03, 04                     | lane F                        | yes            |
| 6     | [40 verification](40-verification.md)                           | every lane                 | sequential                    | no             |
| 7     | [41 documentation propagation](41-documentation-propagation.md) | 40                         | sequential (lower-cost model) | no             |

Lanes A to F run in parallel after 03 and 04 merge. Inside a lane the subtasks
run in order because they share command-family source folders. Two lanes never
touch the same folder under `Commands/`. 05 may run at any point after 01.

### Verification

Each subtask carries its own verification. [40](40-verification.md) adds the
cross-command invariants, regenerates and reviews every snapshot, and runs the
complete managed suite and the supported Native AOT gate once.

## Current State

- G4 is complete: 01, 02, 03, 04, 05, every command subtask 10–37, 40
  verification and 41 documentation propagation are merged and green.
- The [Task 30 open findings](../task30-cli-experience-remediation.md#open-findings)
  retain the pre-G4 Entries finding as history; [05](05-index-entries-region.md)
  now owns the fixed list-only boundary and its verification evidence.

## Closeout Notes

- All six test suites use `--parallel collections`; the former serial default is
  historical evidence only.
- Native AOT qualification requires `vswhere` on `PATH` and cannot run in a
  sandboxed worker. The final gate was assigned to the overseer on an
  unsandboxed host.
- The merged report model uses `--format text|json`, `--detail
  minimal|standard|full|debug`, repeatable `--detail-filter`, and one schema-3
  envelope. The current status names are `completed`,
  `completed-with-warnings`, `incomplete`, `invalid-input`, `blocked`, `failed`,
  and `cancelled`.

## Axioms

- inherited - No local axioms; loaded ancestor axioms remain active.

## Entries

- [Shared rules, verification commands, message style, the shared message families, and the record-only instruction every G4 subtask follows](00-conventions.md) - #Memory #Working #CLI #Task #Plan #G4 #Contextual #Active #KeepInMind
- [Capture and commit the current output of every command at both current views and both formats before any G4 behavior change](01-before-snapshots.md) - #Memory #Working #CLI #Task #Plan #G4 #Contextual #Active
- [Decide the names of statuses, severities, resolution lanes, states, presentation classes and flags before the rendering system uses them](02-naming.md) - #Memory #Working #CLI #Task #Plan #G4 #Naming #Contextual #Active
- [Redesign the rendering system around one report model, one selection stage, generic text and JSON renderers, one escaper, and the physical layout for the later project split](03-rendering-system.md) - #Memory #Working #CLI #Task #Plan #G4 #Presentation #Architecture #Contextual #Active
- [Design and implement the interaction system with keyboard selection, dependency marks, plan review before confirmation, and non-interactive equivalents](04-interaction-system.md) - #Memory #Working #CLI #Task #Plan #G4 #Interaction #Contextual #Active
- [Make index rewrite only the list of entries inside the Entries section and give status and update the same boundary](05-index-entries-region.md) - #Memory #Working #CLI #Task #Plan #G4 #Index #Safety #Contextual #Active
- [Status output catalogue with every status, level, format and finding message](10-status.md) - #Memory #Working #CLI #Task #Plan #G4 #Status #Contextual #Active
- [Doctor output catalogue with the severity ladder, coverage counts, every finding kind and its message](11-doctor.md) - #Memory #Working #CLI #Task #Plan #G4 #Doctor #Contextual #Active
- [Install output catalogue and confirmation flow](12-install.md) - #Memory #Working #CLI #Task #Plan #G4 #Install #Contextual #Active
- [Update output catalogue and confirmation flow](13-update.md) - #Memory #Working #CLI #Task #Plan #G4 #Update #Contextual #Active
- [Index output catalogue](14-index.md) - #Memory #Working #CLI #Task #Plan #G4 #Index #Contextual #Active
- [Repair output catalogue and guided selection flow](15-repair.md) - #Memory #Working #CLI #Task #Plan #G4 #Repair #Contextual #Active
- [Cleanup output catalogue](16-cleanup.md) - #Memory #Working #CLI #Task #Plan #G4 #Cleanup #Contextual #Active
- [Context output catalogue with the payload stream and its framing](17-context.md) - #Memory #Working #CLI #Task #Plan #G4 #Context #Contextual #Active
- [Find output catalogue](18-find.md) - #Memory #Working #CLI #Task #Plan #G4 #Find #Contextual #Active
- [References output catalogue](19-references.md) - #Memory #Working #CLI #Task #Plan #G4 #References #Contextual #Active
- [Route list output catalogue](20-route-list.md) - #Memory #Working #CLI #Task #Plan #G4 #Route #Contextual #Active
- [Route inspect output catalogue and ambiguity prompt](21-route-inspect.md) - #Memory #Working #CLI #Task #Plan #G4 #Route #Contextual #Active
- [Route init output catalogue and the complete-status change](22-route-init.md) - #Memory #Working #CLI #Task #Plan #G4 #Route #Contextual #Active
- [Route create output catalogue](23-route-create.md) - #Memory #Working #CLI #Task #Plan #G4 #Route #Contextual #Active
- [Route update output catalogue](24-route-update.md) - #Memory #Working #CLI #Task #Plan #G4 #Route #Contextual #Active
- [Route move output catalogue](25-route-move.md) - #Memory #Working #CLI #Task #Plan #G4 #Route #Contextual #Active
- [Route remove output catalogue](26-route-remove.md) - #Memory #Working #CLI #Task #Plan #G4 #Route #Contextual #Active
- [Extension list output catalogue](27-extension-list.md) - #Memory #Working #CLI #Task #Plan #G4 #Extension #Contextual #Active
- [Extension inspect output catalogue](28-extension-inspect.md) - #Memory #Working #CLI #Task #Plan #G4 #Extension #Contextual #Active
- [Extension create output catalogue and input prompts](29-extension-create.md) - #Memory #Working #CLI #Task #Plan #G4 #Extension #Contextual #Active
- [Extension install output catalogue, package selection, permission and confirmation flows](30-extension-install.md) - #Memory #Working #CLI #Task #Plan #G4 #Extension #Contextual #Active
- [Extension update output catalogue and flows](31-extension-update.md) - #Memory #Working #CLI #Task #Plan #G4 #Extension #Contextual #Active
- [Extension remove output catalogue and flows](32-extension-remove.md) - #Memory #Working #CLI #Task #Plan #G4 #Extension #Contextual #Active
- [Library list output catalogue](33-library-list.md) - #Memory #Working #CLI #Task #Plan #G4 #Library #Contextual #Active
- [Library inspect output catalogue](34-library-inspect.md) - #Memory #Working #CLI #Task #Plan #G4 #Library #Contextual #Active
- [Library attach output catalogue and permission flow](35-library-attach.md) - #Memory #Working #CLI #Task #Plan #G4 #Library #Contextual #Active
- [Library sync output catalogue and permission flow](36-library-sync.md) - #Memory #Working #CLI #Task #Plan #G4 #Library #Contextual #Active
- [Library detach output catalogue and permission flow](37-library-detach.md) - #Memory #Working #CLI #Task #Plan #G4 #Library #Contextual #Active
- [Cross-command invariants, snapshot regeneration and review, and the complete managed and Native AOT gate](40-verification.md) - #Memory #Working #CLI #Task #Plan #G4 #Testing #Contextual #Active
- [Propagate the accepted changes recorded in every subtask's changes ledger into contracts, public documentation, Directives and memory records](41-documentation-propagation.md) - #Memory #Working #CLI #Task #Plan #G4 #Documentation #Contextual #Active
