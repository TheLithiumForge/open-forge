---
open-forge:
  description: Keep this repository's default test feedback pure and fast while preserving explicit closure evidence for real OS, Git, CLI, packaging, and benchmark boundaries
  tags: [Pattern, Framework, Testing, Unit, Closure, CI, CLI, Evidence]
---

# Tiered Test Evidence

## Shape

- Name pure, in-memory regression files `*.unit.test.ts`. They may import a narrow internal test surface but do not create OS temporary directories, start subprocesses, initialize Git, build packages, or mutate the workspace.
- Name tests that cross filesystem, subprocess, Git, link, packaging, installation, rollback, or benchmark lifecycle boundaries `*.closure.test.ts`.
- `bun run test` and `bun run test:fast` run only the unit tier. `bun run test:closure` runs the closure tier, and `bun run test:ci` runs both tiers in order.
- Raw `bun test` retains Bun's ordinary discovery and therefore finds both suffixes. IDE users select unit files for fast feedback and closure files intentionally; do not globally hide closure tests from discovery.
- `tests/run-tests.ts` resolves this repository from its own location, selects tier files by absolute path, and sets an explicit repository cwd.
- `tests/support/index.ts` is the single shared boundary for repository paths, suite-scoped temporary sandboxes, subprocess capture, CLI and Git invocation, filesystem predicates, tree copying, and byte snapshots.
- Prefer one suite OS-temporary root with isolated case directories and one closeout cleanup. Do not allocate and recursively delete a new OS root for every case unless isolation evidence specifically requires it.
- Unit tests own pure parsing, validation, ordering, state transition, ownership, and normalization contracts. Closure tests own public command, real byte, Git, containment, rollback, packaging, and evidence-lifecycle claims.
- A successful existence assertion is insufficient by itself. Assert parsed meaning, important bytes, ownership and hashes, route validity, idempotence, or another user-visible invariant.
- Preserve absence and unchanged-tree assertions when they prove rejected mutation, containment, rollback, tamper refusal, or transaction atomicity.
- Avoid repeating one full install or route traversal for every catalogue item when one all-catalogue composition plus receipt integrity and doctor validation covers the shared mechanism.

## Triggers

- Run the unit tier freely during implementation.
- Run the affected closure file when changing its owned process or OS boundary.
- Run the complete closure tier in CI and before closeout of changes to CLI dispatch, installation, extension ownership, Git checkpoints, filesystem safety, packaged layouts, or benchmark evidence.

## Review Checks

- Terminal tier selection works from outside the repository cwd.
- IDE discovery can run either tier directly.
- Closure evidence still covers rollback, partial-write refusal, path escape, links, hard links, ownership tamper, and real Git lifecycle behavior.
- Payload wording changes do not require updating tests unless the wording is executable syntax or accepted behavior.
