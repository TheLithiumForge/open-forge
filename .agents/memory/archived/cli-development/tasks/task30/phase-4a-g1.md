---
open-forge:
  description: Active Task 30 G1 state-model subtask with its actionable evidence and the accepted lock-file ownership model
  tags: [Memory, CLI, Task, Subtask, Contextual, Archived, Historical]
---

# Task 30 — Phase 4a / G1 State Model

## Status

Complete. A1–A6 migrated lifecycle/Library consumers to ownership and retired
the obsolete records/subsystem. [A6](06-a6-delete-lifecycle.md) owns that evidence.
[P1](06p-p1-shared-permissions.md) closed steps 3a/3b: all six gated commands use
shared authored destination grants and expose `--allow-path`; the per-owner
reader/writer is deleted. Managed and supported Windows native suites passed,
with separately committed before snapshots and same-commit contracts.
Safe M1 and B1 are complete. B1 migrated generated Entries to headings and
qualified the complete managed/native integration wave. G4 is now complete;
the deferred Framework meaning decisions remain separate. The supported Windows
Native AOT gate requires `vswhere` on `PATH` and was run by the overseer on an
unsandboxed host.

## Evidence source

- [Lifecycle baselines and architecture](../../../../emerging/analysis/cli-experience-audit/lifecycle-baselines-and-architecture.md)
  measured the per-file baseline and lifecycle-record failure modes.
- [Repository dogfood and configuration](../../../../emerging/analysis/cli-experience-audit/repository-dogfood-and-configuration.md)
  measured the configuration and permission dead ends.
- [Structural requirements and markers](../../../../emerging/analysis/cli-experience-audit/structural-requirements-and-markers.md)
  measured the authoring and generated-region friction.

These sources remain in Emerging. This subtask carries their actionable conclusion;
new code evidence is recorded below or in the parent Task.

## Accepted decisions

1. Keep two npm-shaped files in `.agents`: `open-forge.json` for authored
   configuration and `open-forge.lock.json` for ownership state.
2. Do not make a fingerprint chain or a Git executable dependency the change
   detector. Explain previous content through the existing recovery bundle and
   user-visible Git information when a `.git` directory is present.
3. Put `allowInstallPaths` in `open-forge.json` and provide `--allow-path`
   for noninteractive use. **Entries are paths, not patterns**: an entry naming
   a file admits that file, an entry naming a directory admits everything
   beneath it, and there is no glob language. The six gated commands are
   `extension install/remove/update` and `library attach/detach/sync`.
4. Keep a `removedCategories` list so a user-deleted category is not silently
   restored.
5. No migration and no legacy reader. The three old state files are not read,
   converted, or honoured, which is affordable because there are no released
   users.
6. Do not add a Git-cleanliness gate. Install, update, and repair may run on a
   dirty tree and may report an advisory after a write.

## Lock-file ownership: validated and accepted

The proposal was put to the maintainer before any step 4 source change and
accepted with three refinements. The four proof obligations resolved as follows.

| Obligation                                             | Result                             | Evidence                                                                                                                                                                                                                                                                                                                                                                                   |
| ------------------------------------------------------ | ---------------------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------ |
| Deletion intersected with payload ownership            | **Superseded by a stronger shape** | `ExtensionRemovePathInspector.ReadAsync` iterates the record's `LifecycleExtensionPathV1` and derives owners from `path.Owners` alone; the only independent check is `ExtensionDestinationPolicy.IsAllowed` plus `HasOrdinaryAncestors`. No payload cross-check exists today.                                                                                                              |
| Recovery bundle written before any deletion            | **Already satisfied**              | `ExtensionRemoveApplicationOperation.ExecuteUnderLeaseAsync` calls `ExtensionRemoveRecoveryApplication.PrepareAsync` before the effect loop and returns `BeforeEffects` with `RecoveryUnavailable` on any non-`Prepared`/`NotNeeded` state. `LibraryMutationApplicationRunner` refuses at `LibraryExecutionStage.RecoveryPreparation` when reversible effects lack a matching preparation. |
| Stale or missing state is truthful, never over-deletes | **Satisfied by construction**      | No claims means nothing to delete, which is a missed deletion. The over-delete direction is a _stale_ claim, contained by the receipt shape below.                                                                                                                                                                                                                                         |
| No new schema, dependency, or architecture machinery   | **Satisfied**                      | One JSON document through the existing source-generated `System.Text.Json` path and the `Framework/Settings` reader shape landed in `2d99c18c`. Merging the Library record into the same file is already accepted direction.                                                                                                                                                               |

### Accepted refinements

7. **The lock is a write-time receipt, not a payload projection.** Every entry is
   recorded from a verified mutation receipt at the moment the CLI writes the
   thing it describes — the file Framework or Extension install created, the leaf
   link Library attach created, the region Index generated. This replaces the
   proposed "intersect against what the payload claims", which could not work for
   an extension installed from an explicit `--source` whose path is gone by
   removal time, and which reintroduced the cross-version falsehood the decision
   already rejected. Residual containment at delete time is existence plus the
   owner's declared destination boundary.
   The maintainer clarified during A4 that the shared allow list applies to all
   owners. Together with implicit `.agents/` admission, reserved-path checks,
   and physical containment, it defines that boundary; no per-extension field
   is required.
8. **`extension remove` stops distinguishing changed from unchanged content.**
   `baselineFingerprint` was the only input to `ExtensionRemovePathClassification`
   via `ExtensionRemovePathInspector.Classify`, so deleting the field collapses
   `ChangedFinalOwner` and `UnchangedFinalOwner` into one final-owner class that
   is always deleted. `KeepAsUnmanaged` and the changed-content policy leave the
   command contract. The recovery bundle and `git diff` carry the protection.
9. **`baselineFingerprint` and `fingerprintKind` leave public output entirely**,
   rather than being reduced to a managed/unmanaged flag.
10. **The comparison becomes two-way: current against intended.** Accepted
    2026-09-12, after measuring that `extension inspect`, `extension update`, and
    `update` compare three sides per path. Only `baseline` comes from the record
    and dies with it; `current` is computed from the file on disk and `intended`
    from the running payload, so both survive. The commands keep the question a
    person acts on — whether the workspace matches what this release would
    install — and lose only the one that needed stored integrity. `Relation` is
    redefined over two sides.
11. **The comparison normalizes with the Markdown-semantic policy.** Both sides
    are hashed in one run, so the policy stops being stored state and becomes a
    local choice of the running binary. It stays semantic rather than exact
    bytes because CRLF, a trailing space, and a missing final newline are the
    exact false-positive class the audit measured, and editors, Prettier, and
    `core.autocrlf` keep producing them. `MarkdownFingerprintReader` already
    declares itself operation-time with no persistence responsibility, so it
    needs no change to survive the record's deletion.
12. **`ExtensionInspectPathRelation` collapses rather than being renamed.**
    `Unchanged` and `Changed` keep their names with two-way meaning: matches, or
    differs from, what this release would install. `CurrentDiverged` is deleted,
    because it existed only to name baseline and intended disagreeing. `Retired`
    survives but is derived from lock ownership against the payload instead of
    from a baseline. The other nine members are untouched.
13. **The computed hashes stay in output; their mechanics do not.** `current` and
    `intended` keep their `sha256`, so a person who disagrees with a verdict can
    check it and a bug report can carry the two values that explain it. The
    `kind`, `policy`, and `origin` sub-fields go, because they describe how the
    CLI compares rather than what it found, which is the detail decision 9
    removes.

### Corrections recorded while validating

- The accepted decision record said `allowInstallPaths` accepts **globs**. That
  contradicted accepted decision 3 and the shipped
  `Framework/Settings/Shared/Planning/WorkspaceAllowList.cs`, both of which say
  paths, not patterns. The decision record was corrected.
- **Region ownership survives the fingerprint deletion.** The twin that dies is
  the `Targets[]` entry carrying a `Region` alongside a `BaselineFingerprint`.
  The fingerprint-free `GeneratedRegions[]` entry is already the region-ownership
  shape the lock needs and is kept. `FrameworkLifecycleCurrentnessReader` carries
  a `continue` for `LifecycleSchema.GeneratedEntriesRegion` whose comment says
  removing the twin "belongs with the lifecycle record redesign" — step 4 deletes
  that workaround along with the twin.
- **Directories are not recorded.** `LibraryDetachPlanner` already plans
  `Directories = []`, so detach never removes a directory attach created.
  Recording directories would add a cleanup behaviour the CLI does not have.
  Stated as a reversible assumption, not a maintainer decision.

### Measured step 4 scope

| Surface                                           | Files                   |
| ------------------------------------------------- | ----------------------- |
| `Framework.Lifecycle` consumers in Core           | 169                     |
| `LibraryRecord` / `LibraryPathIdentity` consumers | 67                      |
| `BaselineFingerprint` consumers                   | 42                      |
| — of those, JSON or rendering contract            | 8                       |
| Accepted records naming the old files             | 44 crystallized         |
| Historical records naming them                    | 26 archived, 6 emerging |

The crystallized count is 44, not the 41 carried into this step. Archived and
emerging records keep the old names as provenance and are not rewritten.

### Current-state baseline for step 4

`doctor` run against this repository, which is a Framework source checkout with
no `.agents/open-forge.lifecycle.json`, reports:

- `ERROR framework.install-incomplete` — "The lifecycle document is missing",
  with resolution "repair is blocked".
- `INFO extension.lifecycle-document-missing` — installed coverage cannot be
  proven empty.

This is the live form of the accepted consequence that `doctor` must stop
reporting a missing lifecycle document on a Framework source checkout. Use it as
the before-state when step 4 changes `doctor` output.

### Step 4 progress

| Surface                                           | State                                                                                                                                                                                |
| ------------------------------------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------ |
| Ownership model, codec, reader and schema         | Complete; all current state consumers use it.                                                                                                                                        |
| Public baseline fields and three-way comparisons  | Removed; current/intended hashes and semantic comparison remain.                                                                                                                     |
| Framework, Extension and Library readers/writers  | Complete through A1–A6; one best-effort ownership publication.                                                                                                                       |
| Framework/Lifecycle and standalone Library record | Deleted after all readers were moved; old user files are unrelated content.                                                                                                          |
| Verification                                      | A6 Unit 3,417 passed; Integration 1,823 passed plus 17 expected skips; EndToEnd 125 passed; zero failures. Native counterparts match. Format retains exactly five known diagnostics. |

The [A6 receipt](06-a6-delete-lifecycle.md) owns exact executables, logs, artifact
hashes, the 50-view reviewed diff, before commits and every implementation
divergence. Counts are evidence inventory, never pass conditions.

Snapshot evidence, corrected: the Imprint package was referenced by the unit
project from `911470d2`, but no test used it and no snapshot existed, so there
was no reviewed baseline to diff output changes against. The first two live under
`src/cli/tests/unit/OpenForge.Cli.Core.UnitTests/__snapshots__/`. Imprint's
`ignoreLineEndings` defaults to true, so CRLF-written snapshots still compare
correctly against LF checkouts.

### The comparison mode was renamed, not re-scoped

`ExtensionInspectComparisonMode.ThreeWay` is now `InstalledAndAvailable`, and its
wire value is `installed-and-available`. The old name counted sides, which told a
reader nothing and did not match its own siblings: `installed-only` and
`available-only` already name sources. `ReadMode` takes exactly
`(trustedLifecycle, hasInstalled, hasAvailable)`, so naming the sources is what
the value has always meant.

The new name also survives the record's deletion, because the lock still answers
"installed". It does not need renaming a second time.

An earlier attempt renamed it to `TwoWay` and was reverted the same day: the CLI
still performs the three-sided comparison internally, and only the publishing
changed, so `TwoWay` would have claimed a comparison that is not happening.

### Recorded step 4 execution order (completed)

Commands read and write the lock, one at a time, each with its contract in the
same increment:

1. `install` and `route init` write Framework ownership from verified receipts.
2. `extension install`, `update`, and `remove` write and read Extension
   ownership. Remove is where `ExtensionRemovePathClassification` collapses and
   `KeepAsUnmanaged` leaves the contract.
3. `library attach`, `detach`, and `sync` write Library ownership, replacing
   `.agents/open-forge.libraries.json`.
4. `status` and `doctor` read the lock instead of the lifecycle record. This is
   where `state` is redefined to current against intended, and where `doctor`
   stops reporting a missing lifecycle document on a Framework source checkout.
5. Delete `Framework/Lifecycle`, the Libraries record, and the relation members
   that name baseline agreement. `comparison.mode` and the relation vocabulary
   collapse here, together, once nothing computes a baseline.

Deferred out of step 4, each recorded so it is not lost:

- **Task record filenames take a task-number prefix**, so
  `tasks/cli-experience-remediation.md` becomes
  `tasks/task30-cli-experience-remediation.md` and
  `tasks/implementation-duplication.md` becomes
  `tasks/task31-implementation-duplication.md`. Accepted 2026-09-12. Not applied
  yet: both files, and most of the task tree, carry uncommitted edits from a
  second agent that is serializing the Emerging analysis into new task records
  and moving the analysis routes during a lifecycle review. A rename
  while it holds those paths open leaves the old and new names both present with
  diverging content. Apply it once that work lands, and update every inbound link
  in the same change. New task files created before then should already use the
  prefix.

- The `$schema` URL resolves only once the repository is public. It is written
  now because there are no released users to hand a dead link to.
- A command that writes the schema documents into a workspace, so the files
  validate offline and inside a private repository.
- Validating the schema documents against their own examples in CI. No JSON
  Schema validator is an accepted dependency, so today the only check is that
  both documents parse and their `$id` matches the URL the CLI writes.

Decided while implementing:

- The capability is named for **ownership**, not for the lock file, because
  `WorkspaceLock` already means the mutation lease in
  `Framework/Mutation/Locking`. `WorkspaceLockState` and `WorkspaceLockResult`
  exist and mean something else entirely.
- There is **no standalone lock writer**. The codec renders bytes; the file write
  belongs in each command's plan as a `PlannedFileChange`, exactly as
  `plan.LifecycleChange` is applied today, so the lease, revalidation, and
  recovery bundle continue to cover it. A writer of its own would bypass all
  three.
- Record equality does not compare `ImmutableArray` members by contents, so the
  round-trip evidence compares sections element by element rather than comparing
  whole documents.
- The `$schema` URL cannot be published until the repository is public. Ship the
  schema documents now, and **defer** both the absolute URL and a command that
  writes the schema into a workspace for offline use.

## Earlier foundation commits

Steps 0 to 3a are committed. Each was verified against the unit suite and, for
the behaviour-preserving parts, a throwaway characterization capture.

| Commit                 | What                                                                                                                                         |
| ---------------------- | -------------------------------------------------------------------------------------------------------------------------------------------- |
| `88dfd9da`             | [Workspace State Files](../../../../crystallized/decisions/framework/workspace-state-files.md) accepted; Framework layer record points at it |
| `911470d2`             | Imprint snapshot package wired into the unit suite from `vendor/`                                                                            |
| `2d99c18c`             | `Framework/Settings` reader for the authored file, unread by any command                                                                     |
| `3be5a108`             | `allowInstallPaths` grants a destination with no terminal — **the ship-blocker is closed**                                                   |
| `ca83b1b1`             | Writer that keeps every other authored key, including unrecognised ones                                                                      |
| `9650b19d`, `bd958db7` | `--allow-path` on `library attach` and `extension install`                                                                                   |

Settled while implementing, and not yet reflected above:

- The authored file **accepts and ignores unknown keys**, and tolerates comments
  and trailing commas. It is hand-edited and gains keys over releases, so
  refusing an unrecognised one would make a newer file unusable by an older CLI
  for no safety gained.
  - **Superseded on 2026-09-12**: this previously also said the authored file
    carries no `schemaVersion`. The maintainer decided that **both** files carry
    one, alongside a `$schema` URL for editor completion and validation. The
    reason the version was refused — that it would make a newer file unusable —
    is answered by keeping it non-binding: an unrecognised version is read for
    the keys this release understands and reported, never refused. The shape is
    recorded as the
    [Versioned Configuration Schema](../../../../../patterns/software/versioned-configuration-schema.md)
    Pattern.
- `--allow-path` **persisted** in the G1 model — it was the non-interactive form
  of "allow always" — and never wrote under `--dry-run`.

  > Superseded by G4: explicit grants and interactive Allow-always decisions are
  > staged in the permission plan and published only during confirmed
  > application under lease, revalidation, and recovery. Cancellation or refusal
  > before application leaves settings unchanged.
- The writer round-trips through the JSON object model, so unknown keys and
  authored key order survive. **Comments do not survive a write**; keeping them
  would mean hand-rolling a second parser, which is refused.
- An unreadable settings file yields an empty allow list, so it withholds
  permission rather than granting it.
- `rules` and `thresholds` are deferred out of G1. They need a design pass of
  their own.

At the foundation checkpoint, the remaining 3a work was `--allow-path` on the
other four gated commands. The deferred 3b work was:
deleting `Framework/Permissions`, collapsing the `Permissions:` result fields,
and dropping subject scoping — measured at roughly 156 files, 26 of them public
result or presentation contract.
These historical remaining items are now closed by [P1](06p-p1-shared-permissions.md).

## Execution and evidence

- Record each implementation discovery in Task 30, this subtask, or a new
  focused subtask. Do not rewrite the Emerging Analysis to follow implementation
  drift.
- Architecture or public-contract changes stop here for maintainer acceptance.
- The first decisive evidence is a written contract/update to the owning
  durable sources, followed by focused tests for read, write, stale, missing,
  and recovery cases. Do not implement from this proposal alone.
