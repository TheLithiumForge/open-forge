---
open-forge:
  description: Active Task 86 to keep the workspace lock and recovery bundles inside the workspace, so the CLI works in sandboxed worktrees and no leftover file or bundle can leave the workspace stuck
  tags: [Memory, Working, Task, CLI, Locking, Recovery, Sandbox, Worktree, Contextual]
---

# Task 86: Workspace runtime lock

Status: Active since 2026-10-23, requested by the maintainer. It supersedes PR #7, whose branch it builds on.

## Outcome

Every writing command works in any worktree or sandbox that can write the workspace, with no setting. A leftover lock file, a killed process or an unrelated recovery bundle never blocks a later command. When a command cannot take the lock, its message names the real cause and the exact path.

## Accepted design

The [lock scope council](../../../emerging/analysis/lock-scope-council/record.md) decided the design. In short:

- One lock per workspace at `<workspace>/.open-forge-state/workspace.lock`, held for the same span as today.
- The open handle is ownership. Unix adds a checked `flock`. The file's contents never matter.
- Recovery bundles move to `.open-forge-state/recovery/v1/`, which ignores itself in Git and is reserved from every scan.
- Existence-only recovery gates are replaced by checks of each operation's own state.
- A best-effort bridge also holds the legacy per-user lock until 1.0, read-only when a sandbox denies write access.

## Plan

The [implementation plan](../../../emerging/analysis/lock-scope-council/plan.md) holds the frozen foundation API, the waves and packets, the recovery-gate audit, the tests and the documentation changes.

## Delivery

Astra high implementers own one packet each in separate worktrees. The overseer integrates each wave, runs the full Unit, Integration and EndToEnd suites, and closes out with an Astra review, the docs site, help, `index` and a manual check before the PR.
