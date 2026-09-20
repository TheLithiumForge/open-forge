---
open-forge:
  description: "Historical CLI-v2 source: Git-first mutation recovery, Gitless sibling backups, in-process reversal, hard-stop evidence, and residual-state behavior"
  responsibility: Define recoverability without a persistent Open Forge transaction journal, hidden repository state, or claims stronger than the available Git and backup evidence
  tags: [Memory, Archived, Contextual, Historical, CLI, CLIv2, RawData]
---

# CLI Workspace Recovery Contract

## Scope

Open Forge optimizes managed workspace mutation for ordinary Git use. Git is
the primary durable review and recovery mechanism. The CLI still uses complete
planning, revalidation, atomic complete-file writes, verification, and
in-process reverse recovery; Git does not replace those mechanical guarantees.

The replacement does not initially create a persistent transaction directory,
workspace mutation journal, repository lock, or recovery database. Those costs
are not justified by the expected short operations while Git and a clear
Gitless fallback cover the most important user journeys. This policy may change
only from measured failure evidence, not speculative completeness.

## Guarantees

### Git-Backed Journey

Before a workspace mutation, the CLI inspects the selected workspace and the
exact relevant paths:

```text
select workspace
  -> inspect Git availability and relevant status
  -> build and preflight the complete plan
  -> show effects and decisions
  -> confirm
  -> apply and verify
  -> show the resulting Git diff and commit guidance
```

Git is recommended, not a Framework presence condition. Before installation,
guided request construction may offer to initialize Git when none is present;
that initialization must be an explicit planned prerequisite rather than an
unannounced side effect. After installation, the CLI prints exact review and
commit guidance. It does not create a commit, collect a commit message, or
continue into Extension or Completion mutation. Instead, the result may suggest
the separate `extension.add` and `completion.install` operations. Each follow-up
requires its own request, authority, plan, result, and process completion.

A dirty relevant path blocks mutation by default because Open Forge cannot
separate its planned result from uncommitted user work reliably. The
canonical-only shared mutation flag `--skip-git-check` explicitly bypasses that
gate. It does not grant overwrite, deletion, ownership, formatter `RUN`, or
containment authority. When bypassing the gate weakens recoverability, the plan
uses the Gitless backup rules for affected replacements and deletions.

Repository cleanliness is not itself recovery evidence. Preflight classifies
every planned filesystem target independently:

| Target state                                                                        | Recovery class                                                                         |
| ----------------------------------------------------------------------------------- | -------------------------------------------------------------------------------------- |
| Existing target tracked at the clean starting commit with matching bytes            | Git-backed                                                                             |
| Existing ignored, untracked, absent-from-`HEAD`, or otherwise non-restorable target | Gitless sibling backup required                                                        |
| Absent target that will be created                                                  | No before-state backup required; in-process reversal removes only the created identity |
| Target whose Git or filesystem identity cannot be proven                            | Blocked                                                                                |

An ignored target may therefore require a Gitless backup even when every
Git-visible relevant path is clean. A staged addition, staged deletion,
submodule boundary, or another ambiguous repository state does not become
Git-backed through a clean summary. Named production values represent these
classes; no boolean `gitClean` substitutes for per-target recovery policy.

`--yes` never bypasses the Git check. `--overwrite` authorizes only the eligible
replacement set already exposed by lifecycle planning.

### Gitless Journey

When no usable Git repository exists, the CLI explains the weaker guarantee
before mutation:

```text
Git is not available for this workspace.
Open Forge can create adjacent backups before replacing or deleting files,
but it cannot provide commit-based review or complete hard-stop recovery.
```

The guided operation offers:

```text
CREATE BACKUPS AND CONTINUE
CONTINUE WITHOUT BACKUPS
CANCEL
```

The safe recommendation is `CREATE BACKUPS AND CONTINUE`. Deterministic
automation may accept that documented recommendation through `--yes`; it may
not silently choose the weaker no-backup path.

### Backup Shape

Every replaced or deleted ordinary file that needs Gitless protection receives
one adjacent sibling backup:

```text
name.md      -> name.md.bak
config.json  -> config.json.bak
```

Append `.bak` to the complete filename. Never use `name.bak.md`, because it
still looks like routed or formattable Markdown. A backup is user-owned,
non-routed recovery material and never enters `.agents/open-forge.json`.

Open Forge never overwrites an existing `.bak`. A collision blocks before any
write and asks the user to move or remove that backup. It does not invent a
numbered or timestamped alternative.

The [filesystem effect contract](filesystem-effects.md#file-replacement) owns
the two exact executor-sidecar names used to prepare replacement bytes and
preserve the observed target. Those sidecars are temporary while an effect is
active and become visible residual evidence after a hard stop. They never
replace the independently preflighted, user-owned `.bak` required for Gitless
recovery.

New file creation has no before-state to back up. A hard stop may therefore
leave complete newly created files whose lifecycle record was not yet written.
`doctor` reports that evidence without claiming ownership; the user recovers
through Git, the visible plan, or manual review.

### Application And Handled Failure

The plan contains every persistent effect before the first write. The executor:

1. Revalidates the complete preflighted plan.
2. Creates every required Gitless backup before changing its protected target.
3. Applies payload and authored complete-file effects in order.
4. Rebuilds affected generated navigation.
5. Verifies each primary effect and the complete resulting operation state.
6. Writes `.agents/open-forge.json` atomically as the final primary persistent
   effect when managed lifecycle state applies.

The primary operation is complete after that record verifies. Optional
formatting then runs as post-processing over the exact affected files. When
automatic formatting succeeds, Open Forge validates the formatted bytes and
atomically refreshes only their advisory checksums. A formatter or checksum
refresh failure returns attention and preserves completed primary work; it does
not enter reverse recovery for the primary mutation.

Application records exact applied state in process. A handled failure or caught
cancellation stops new effects, reverses applied effects in reverse order, and
verifies restoration. A concurrent unexpected target is preserved and reported
as residual state rather than overwritten during recovery.

Successful rollback does not turn a failed selected operation into success.
Incomplete rollback returns failure and names every residual logical target.

### Hard Stops

A hard process or machine stop may leave a mixture of complete old and new
files. Open Forge claims no later-invocation automatic transaction recovery.

Recovery is explicit:

| Available evidence                            | Recovery                                          |
| --------------------------------------------- | ------------------------------------------------- |
| Clean Git starting point                      | Inspect or restore the operation diff through Git |
| Gitless sibling backup                        | Compare and restore the exact adjacent `.bak`     |
| Reconstructable generated navigation          | Rerun `route rebuild`                             |
| Complete created file without lifecycle state | Review, keep, or remove manually                  |
| Unknown or conflicting state                  | Preserve and diagnose; never guess                |

`status` counts known executor sidecars and available sibling backups around
its exact inspected targets. `doctor` reports their exact paths, relationships,
and recovery findings. `repair` may remove or restore only when one result is
mechanically proven and explicitly authorized. It does not reconstruct
historical ownership from current bytes.

### Scope And Limits

Git and sibling backups protect workspace files. Exact external mutations such
as shell profile integration require their own explicit target and recovery
contract before shipping. A workspace Git repository does not protect an
external profile.

This contract does not claim protection from hardware loss, hostile filesystem
actors, unavailable Git history, manually deleted backups, unsupported
metadata, or writes after the last valid observation. It favors visible,
ordinary recovery over a hidden mechanism whose complexity would itself create
failure and maintenance cost.

## Boundaries

Recovery evidence cannot make a stronger claim than the available Git state,
verified sibling backup, applied record, or reconstructable derived state.
This contract provides no persistent mutation journal, repository lock,
recovery database, external-profile protection, or guarantee against hardware
loss, hostile filesystems, unavailable history, deleted backups, or later
writes.

## Verification

### Evidence

Use real temporary repositories, filesystems, child processes, and built CLI
artifacts. Prove:

- Clean, dirty, untracked, ignored, and absent Git states.
- Per-target Git-backed, Gitless-backup, creation-only, and blocked recovery
  classification.
- Git initialization and post-operation review and commit guidance.
- `--skip-git-check` without authority leakage.
- Backup creation for replacement and deletion.
- Existing-backup collision preservation.
- Reverse recovery after every applied-effect boundary.
- Hard stops before and after payload, route, primary state-file,
  post-formatter, and checksum-refresh boundaries.
- Detection of created files without final lifecycle state.
- Cross-runtime execution of the same public ABI.

The archived spike branch preserves process-durable journal experiments as
evidence. Those prototypes are not production architecture; valuable scenarios
are recreated later against the accepted implementation.

### Review Checks

- Git is the primary durable review and recovery path.
- Dirty relevant paths block unless explicitly bypassed.
- `--yes`, `--overwrite`, and `--skip-git-check` retain different authority.
- Gitless destructive effects recommend adjacent `.bak` files.
- Existing backups are never overwritten.
- Handled failures recover in process and verify restoration.
- Hard-stop recovery makes no journal-backed claim.
- The lifecycle record is the final primary effect; optional formatting and
  advisory-checksum refresh occur afterward as non-rollback post-processing.
- Unknown recovery state is preserved for review.

## Related Current Sources

- [Mutation execution](mutation-execution.md)
- [Filesystem effects](filesystem-effects.md)
- [Managed lifecycle](managed-lifecycle.md)
- [Operation prerequisites](operation-prerequisites.md)
