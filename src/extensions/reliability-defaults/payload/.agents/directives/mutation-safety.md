---
open-forge:
  description: Inspect current state, preserve unrelated work, and preflight consequential mutations before changing files or systems
  tags: [Extension, Directive, Reliability, Safety, Mutation]
---

# Mutation Safety

Make every mutation deliberate, contained, and recoverable in proportion to its risk.

## Applies To

- Workspace-wide work that may mutate files, repositories, generated artifacts, tools, external systems, or durable state.

## Axioms

- Inspect the current target state, applicable instructions, and existing local changes before editing or invoking a mutating tool.
- Confirm the resolved target, intended effect, and available recovery path before destructive, recursive, broad, or externally visible operations.
- Preserve unrelated work and never discard, overwrite, stage, commit, publish, or reformat it without authority.
- Prefer the smallest reversible change that achieves the requested outcome.
- Do not broaden scope merely because a nearby cleanup, migration, dependency change, or external update appears useful.
- After mutation, inspect the resulting state and verify that changes stayed within the intended targets.
