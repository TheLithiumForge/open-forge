---
open-forge:
  description: Queue Task 81 to maintain this repository's own Patterns after the next beta release, moving required behavior back into Directives and keeping each Pattern as a visual shape
  tags: [Memory, Working, Task, Pattern, Directive, Maintenance, Linking, Contextual, Queued]
---

# Task 81: Own Patterns maintenance

## Outcome

This repository's own Patterns under `.agents/patterns/`, and the bundled Extension Patterns, follow the corrected shipped entrypoint from [Task 80](../../../archived/cli-development/tasks/task80-patterns-show-the-shape.md). Required behavior lives in Directives. Each Pattern keeps only its visual shape, with brief applicability notes and links to the related sources.

The maintainer asked on 2026-10-13 that “any maintenance for our own files regarding how the patterns … switch back to directives plus just the visual pattern” be scheduled as the next task after another beta deployment and release.

Status: Task 81 “Own Patterns maintenance” (phase 1/3): queued, starts after the next beta release.

## Scope

- `.agents/patterns/**` in this repository.
- Bundled Extension Patterns, starting with `src/extensions/planning/content/.agents/patterns/`.
- The Directives that receive required behavior moved out of those Patterns, and the links between them.
