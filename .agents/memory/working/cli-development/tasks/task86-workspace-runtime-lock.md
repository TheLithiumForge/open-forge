---
open-forge:
  description: Active Task 86 to keep the workspace lock and recovery bundles inside the workspace, so the CLI works in sandboxed worktrees and no leftover file or bundle can leave the workspace stuck
  tags: [Memory, Working, Task, CLI, Locking, Recovery, Sandbox, Worktree, Contextual]
---

# Task 86: Workspace runtime lock

Status: Active since 2026-10-23, requested by the maintainer. Implementation is integrated and in review. It supersedes PR #7, whose branch it builds on.

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

## Shipped

| Wave | Packets | What changed |
| --- | --- | --- |
| 1 | A | The lock moved to `.open-forge-state/workspace.lock`, which ignores itself in Git. Unix checks ownership with `flock`. Every failure carries a typed cause and path. A bridge also holds the legacy per-user lock until 1.0 |
| 2 | B, C | New recovery bundles live in `.open-forge-state/recovery/v1`. Every candidate carries its store, so deletion never re-resolves a location. The runtime folder is reserved from every scan, include target, package destination and removal |
| 3 | D, C2 | Lock failures name the real cause and path. Registered roots and links that resolve into the runtime folder are never read |
| 4 | E, F, G | Leftover bundles no longer block Route and Extension commands. Status and Doctor report both stores, and Cleanup plans and deletes per store |
| 5 | E2, H, J, K1 to K3 | Writing commands warn only about drafts, damaged bundles and unreadable stores (chair amendment 4). The lock journeys run on every platform. The lock messages are short |
| 6 | I | Contracts, designs, the CLI guide, the docs site and the CLI Skill describe the runtime folder |

## Evidence

An independent Astra high review found seven issues, fixed in three packets: a FIFO hang in the read-only legacy fallback (chair amendment 5), case aliases of the runtime folder on macOS, linked recovery folders read or written before rejection, Extension history missing the legacy store, and two overpromising docs passages. After those fixes, Windows runs passed 5,190 Unit tests, 3,645 Integration tests with 58 Unix-only skips, and 410 EndToEnd tests with 3 Unix-only skips. The first pull request Build then failed on every platform. On Linux and macOS the lease rejected the read-only legacy handle, whose stream has no path name, so a sandboxed writer could never hold the legacy lock read-only. A cross-process test held its own implicit lock, macOS reports a socket as EOPNOTSUPP, and the delivery skip allowlist did not know the new Linux-only, macOS-only and public Unix-only reasons. After those fixes, Linux runs in WSL passed 5,190 Unit, 3,646 Integration (56 skips) and 378 EndToEnd tests (35 skips), and both Linux and Windows reports pass the delivery skip qualification.

## Remaining

- Qualify the bridge against a pinned 0.9.0-beta.17 executable on each platform.
- Remove the legacy bridge in the 1.0 release.
