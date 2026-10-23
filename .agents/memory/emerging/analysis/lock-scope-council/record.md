---
open-forge:
  description: What the lock scope council decided about where the workspace lock and recovery bundles live, lock granularity, never-stuck behavior and the move from the per-user store
  responsibility: Record council 5's question, method, rankings, chair decisions, where the chair overruled the council, and declined ideas
  tags: [Memory, Analysis, Contextual, Candidate, CLI, Council, Locking, Recovery, Sandbox, Worktree]
---

# Lock Scope Council

This is council 5. [Task 86](../../../working/cli-development/tasks/task86-workspace-runtime-lock.md) implements it, and the [implementation plan](plan.md) turns the decisions into work packets.

## What the maintainer asked

The first request was to "make sure we can run the cli in a different worktree without a problem, either via some flag or idk". Running the CLI in a plain or nested worktree already worked. A sandboxed agent that can write only inside its worktree could not write the per-user data folder, so every writing command falsely reported that another command held the lock. PR #7 fixed only that message and documented `OPENFORGE_DATA_HOME`.

The maintainer then asked for more:

> We need a better solution, maybe do the locks actually scoped, either per smaller unit sizes of folders, or per repo + workspaces, idk. But find a better solution, one that also doesn't let the workspace in a corrupted/read only state just because a lock file was not removed

## Method

Seven read-only Astra xhigh members worked from `develop` commit `c2913bab1`: an OS locking engineer, the CLI architect, an agent workflow realist, a crash safety and recovery engineer, a fine-grained concurrency advocate, a contrarian minimalist and a migration and contract guardian. Each answered the same seven questions in Stage 1. In Stage 2 each member checked the others' claims against the code and ranked all seven proposals in five areas. The chair then decided, and the CLI architect, which won the PLAN ranking, wrote the frozen plan.

## Rankings

Average rank from seven reviewers, lower is better. LOCATION covers where the lock lives and configuration, SCOPE the granularity, NEVER-STUCK the leftover-state and recovery questions, MIGRATION mixed CLI versions, PLAN the implementation plan.

| Proposal | LOCATION | SCOPE | NEVER-STUCK | MIGRATION | PLAN |
| --- | ---: | ---: | ---: | ---: | ---: |
| P1 Migration and contract guardian | 6.00 | 5.29 | 6.57 | 2.29 | 3.00 |
| P2 OS locking engineer | **1.00** | 3.71 | 2.00 | 3.86 | 2.71 |
| P3 Contrarian minimalist | 2.00 | 2.00 | **1.86** | 3.43 | 3.57 |
| P4 Crash safety and recovery engineer | 6.57 | 6.29 | 5.00 | 6.43 | 6.71 |
| P5 Agent workflow realist | 4.00 | 6.43 | 4.00 | 6.29 | 5.57 |
| P6 Fine-grained concurrency advocate | 5.00 | **1.00** | 5.14 | 4.71 | 5.29 |
| P7 CLI architect | 3.43 | 3.29 | 3.43 | **1.00** | **1.14** |

P2 won LOCATION, P6 won SCOPE and P7 won MIGRATION unanimously. The [proposals](proposals.md) and [reviews](reviews.md) are kept as evidence.

## What the council found

- **The lock itself never goes stale.** Ownership is an open handle with `FileShare.None`, which the OS releases when the process dies. A leftover lock file blocks nothing, with one exception: both the manager and the lease reject a lock file that is not empty.
- **The read-only state had two causes.** The first was the per-user store a sandbox cannot write. The second was recovery gates. Route Move, Route Include and Exclude, and Route Create, Init and Update stop when any recovery bundle exists, even an unrelated one.
- **Finer locks do not pay off now.** Index reads settings and ownership and expands its selection to parents and siblings. Route Move rewrites links anywhere and rebuilds its plan under the lease. Install writes the shared controls. Folder or file locks would need read-set tracking, ordering and escalation, and would not fix the sandbox failure. Separate worktrees already give independent locks.
- **Expected-state checks cannot replace the lock.** Two writers can validate the same bytes before either replaces them.
- **.NET does not always enforce the lock on Unix.** Its `FileStream` ignores `flock` failures other than `EWOULDBLOCK`, and `DOTNET_SYSTEM_IO_DISABLEFILELOCKING` turns locking off.
- **`.agents/` is a poor home for runtime state.** The default Codex sandbox protects existing `.agents/` paths, and a lock folder there would collide with a first Install that plans to create `.agents/`.

## Decisions

1. **One lock per workspace, in a root runtime folder.** The lock is `<workspace>/.open-forge-state/workspace.lock`. Its path is fixed, carries no path hash and cannot be moved by a setting or environment variable. It is held for the same span as today. Planning, prompts, dry runs, refusals and verified no-ops take no lock and create nothing. There are no folder, route, file or repository locks.
2. **The handle is ownership, checked on every platform.** The file is never deleted, truncated or rewritten, and its contents are irrelevant. Windows keeps `FileShare.None`. Linux and macOS add a checked `flock(LOCK_EX | LOCK_NB)` through NativeAOT interop, and the lease is granted only when it succeeds. A lock path that is a directory, a link or a special file is reported, never removed.
3. **Errors name the real cause and path:** busy, access denied, locking unsupported, invalid target, I/O error or cancelled. Each command keeps its finding code and status. No message claims another writer unless contention was established, and none tells anyone to delete a lock or a bundle. There is no wait loop.
4. **Recovery bundles move into the runtime folder,** at `.open-forge-state/recovery/v1/<workspace key>/`, in the existing format. `.open-forge-state/.gitignore` with `*` is written first, so Install's `.gitignore` section is unchanged. The folder is reserved from every scan, catalogue, include target, package destination and removal. One typed recovery location is shared by the writer, catalogue and deletion guard. Status, Doctor and Cleanup observe the local and the legacy store separately, and an unreadable legacy store never blocks local work.
5. **Leftover recovery artifacts never block unrelated work.** Existence-only gates are replaced by checks of the operation's own inputs and targets, and retained bundles become a warning that names them. Install's state-based checks stay.
6. **A best-effort bridge to older CLIs until 1.0.** After taking the workspace lock, every writer also takes the legacy per-user lock: read-write as older CLIs do, or read-only with `FileShare.None` when write access is denied and the file exists. It continues without it only when access is denied and no legacy file exists. Existing `locks/v1` and `recovery/v1` data stay where they are.
7. **`OPENFORGE_DATA_HOME` only locates the legacy store,** for the bridge and for finding old bundles. Status reports retained bundles per store and never probes writability.
8. **Contracts and docs** change together: the mutation-and-recovery design, the architecture storage summary, the shared operation contract, the Status, Doctor and Cleanup contracts, the Install footprint, the affected command contracts, `docs/cli.md`, the docs site and the CLI Skill.

## Where the chair overruled the council

- **The bridge needs no cutover setting.** Every member said never to continue without the legacy lock after access is denied, and to require an explicit workspace-mode setting after all older CLIs are stopped. That leaves a sandboxed agent blocked until an operator acts, which is the read-only state the maintainer asked to remove. On 2026-10-23 the chair checked, inside a Codex `workspace-write` worker on Windows, that opening an existing legacy lock file read-only with `FileShare.None` succeeds where the read-write open and file creation are denied. Outside the sandbox, a read-only and a read-write exclusive handle exclude each other in both directions. So the bridge keeps exclusion whenever an older CLI has ever locked the workspace. The remaining gap is stated: an older CLI (0.9.0-beta.17 or earlier) that has never written this workspace, started outside the sandbox during a sandboxed write. The project is in beta and the bridge ends at 1.0.
- **New bundles are always local.** The council asked that bundles stay in the legacy store while older CLIs are supported, so their recovery checks see them. That is the store a sandbox cannot write. An older CLI may therefore miss evidence of an interrupted operation by a newer one. The evidence still exists, and the newer CLI reports it.
- **No external recovery destination.** The council wanted `OPENFORGE_DATA_HOME` to keep selecting external recovery storage. Nothing needs it yet, so it waits until someone does.

## Declined

- Folder, route, file, hierarchical or repository locks. Scoped Index locking can be revisited once contention is measured.
- A lock in the Git directory, which can be outside the worktree, protected, or shared by every worktree.
- PID files, timed leases, heartbeat stealing, stale-file deletion and a lock daemon.
- A five-second wait on contention. Its author withdrew it in favor of a prompt, classified failure.
- A second-open probe as proof of ownership. Its author withdrew it in favor of the checked `flock`.
- Keeping every verified final bundle, extra disk flushing and recovery copies for newly created files. These are separate recovery work.
- Automatic rollback, a journal or replay.
