---
open-forge:
  description: "Historical record: this repository's own Patterns lead with a visual shape and their rules live in Directives, released in beta14"
  tags: [Memory, Task, Pattern, Directive, Maintenance, Linking, Contextual, Complete, Archived, Historical]
---

# Task 81: Own Patterns maintenance

## Outcome

This repository's own Patterns under `.agents/patterns/`, and the bundled Extension Patterns, follow the corrected shipped entrypoint from [Task 80](task80-patterns-show-the-shape.md). Required behavior lives in Directives. Each Pattern keeps only its visual shape, with brief applicability notes and links to the related sources.

The maintainer asked on 2026-10-13 that “any maintenance for our own files regarding how the patterns … switch back to directives plus just the visual pattern” be scheduled as the next task after another beta deployment and release.

Status: Task 81 “Own Patterns maintenance” (phase 3/3): released in beta14 with Task 82.

## Scope

- `.agents/patterns/**` in this repository.
- Bundled Extension Patterns, starting with `src/extensions/planning/content/.agents/patterns/`.
- The Directives that receive required behavior moved out of those Patterns, and the links between them.

## Progress, 2026-10-15

Every own Pattern and the bundled Planning Work Records Pattern now lead with a labelled visual shape, brief applicability notes and links both ways. Required rules moved to the Directives that govern their scope, or to new on-demand scopes for CLI operations, software contracts and versioned configuration, work planning, and the Planning Extension's work records. Procedures moved to Guidance, and advice keeps its original strength. Named Values now links to the CLI's shared operation contract instead of calling its statuses unsettled.

A source identity collision between `tasks/beta-follow-ups.md` and the `tasks/beta-follow-ups/` folder blocked managed Extension updates. The leaf is now `beta-follow-up-backlog.md` with every link updated, and this workspace carries Planning 0.4.0 through a managed update.

An independent Astra high review found the CLI's pre-acceptance variation review weakened to advice and an illustrative schema folder name labelled as required. Both are corrected. The repository Markdown gate, embedded payload, Extension catalogue and Status classes (22 tests) and all 4,962 Unit tests pass, and repository Index is current.
