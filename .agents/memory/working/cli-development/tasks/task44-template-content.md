---
open-forge:
  description: Open Task 44 to fix what the installed templates and core files say, since they are the first Open Forge prose a beta user reads
  tags: [Memory, Working, CLI, Task, Templates, Wording, Beta, Contextual, Active]
---

# Task 44 — Template and core file content

## Task state

- State: **Open, not started.** Raised by the maintainer on 2026-09-17.
- Owner: Root.

## Why this is a beta priority

[Task 37](task37-wording-review-against-proposals.md) reviews the CLI's own
sentences. This task is about different prose: **the files Open Forge installs
into the user's workspace.**

Those files are read far more often than any command output. They are what an
agent loads at startup, what a user opens when deciding where something belongs,
and the first evidence of whether the Framework is worth adopting. A rough
template makes a rougher first impression than a rough error message.

## Scope

- What each template contains, and whether that is the least a user needs to
  start rather than a demonstration of the format.
- The wording of every installed core file, against the
  [writing standard](../../../../crystallized/documents/maintenance/writing.md).
- Whether a file earns its place at all. A template nobody fills in is worse
  than no template.
- The relation between a file's prose and the `description` a reader uses to
  decide whether to open it.

## Boundary

- This is the shipped payload under `src/open-forge/`, not the CLI's messages.
- It depends on [Task 42](task42-minimal-core.md) deciding which files remain in
  the core, so effort is not spent on files that are about to move.

## Acceptance

- Every shipped file has been read and either rewritten, kept deliberately, or
  removed.
- A new user can tell what to put in each file without reading anything else.
