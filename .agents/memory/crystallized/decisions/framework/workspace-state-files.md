---
open-forge:
  description: A workspace keeps one authored settings file and one generated lock file, and the lock records ownership rather than policing integrity
  tags: [Memory, Decision, CurrentTruth, CLI, Lifecycle, Configuration, Permissions, State]
---

# Workspace State Files

## Context

A workspace accumulated four files under one naming convention:
a generated lifecycle record (17 KB of baselines),
`.agents/open-forge.permissions.json` (a short authored allow-list),
a generated Library record, and a legacy
`open-forge.extensions.json` receipt at the repository root that nothing reads.

The count was not the problem. **Generated state and authored settings looked
like the same kind of file**, so a 17 KB machine-owned record and a hand-written
allow-list were indistinguishable, and a user could not tell which they were
permitted to edit.

The lifecycle record also _gated_. Four commands refused to run when it
disagreed with the disk, and every disagreement measured during the CLI
experience audit was false:

- A second baseline per file fingerprinted the **generated** Entries region as
  exact bytes, so any route change anywhere made every Framework target look
  divergent. An Extension could not be installed into a workspace that had ever
  had a route added.
- An absolute `workspacePath` made a byte-identical clone unusable.
- CRLF, a trailing space, or a missing final newline each blocked the workspace,
  despite a normalizer existing on a different path.

The permission model had **no grant mechanism outside a TTY prompt that never
fires**, because the prompt requires a non-redirected stdin. For any agent,
script, or CI run, a destination outside `.agents` was unreachable except by
hand-writing an undocumented schema recovered from the C# source.

## Decision

A workspace keeps **two** files, distinguished by nature and named in the
convention a reader already knows from npm:

- **`.agents/open-forge.json`** — authored. Hand-editable, reviewed in a diff,
  and never rewritten by a lifecycle mutation. Holds `allowInstallPaths` and
  `removedCategories`.
- **`.agents/open-forge.lock.json`** — generated. Machine-owned, documented as
  "do not edit", covering Framework, Extension, and Library records in one file.

Both live under `.agents/`. The directory is already the workspace marker, since
workspace selection walks up looking for it, so placing either at the repository
root would buy no discoverability that the marker does not already provide.

**The lock file is never a gate.** No command refuses because of it. Missing,
stale, or unreadable, it is rebuilt best-effort from the workspace and the
embedded payload, and the command proceeds saying so.

It records **ownership, not integrity**, and it records it as a **receipt rather
than a claim**. Every entry is written from a verified mutation receipt at the
moment the CLI writes the thing it describes: the file Framework or Extension
install created, the leaf link Library attach created, and the generated region
Index produced. The lock is therefore an accurate bookkeeper of what this tool
actually put in the workspace, not a projection of what a payload says it would
put there. None of that is derivable from the workspace afterwards, so it must be
stored. Nothing else must.

Ownership has two kinds, and the lock keeps them distinct because removal treats
them differently. A **path** entry means the CLI created the whole file or link
and may delete it. A **region** entry means the CLI generated a named block
inside a file somebody else authored; the region may be rewritten and the file
may never be deleted.

These are deleted outright: per-file `baselineFingerprint`, the
`region: "entries"` / `exact-bytes` twin entries, `workspacePath`, and
`fingerprintPolicy`.

Region _ownership_ is not part of that deletion. The twin that dies is the
`Targets[]` entry carrying a `Region` alongside a `BaselineFingerprint`, whose
recorded value was computed from payload bytes rather than from the generated
result, so it never matched what install wrote. The fingerprint-free
`GeneratedRegions[]` entry — path and region name, nothing else — is exactly the
region ownership the lock must keep, and it survives unchanged.

Directories are **not** recorded. `library detach` already plans no directory
effects and leaves the directories `attach` created in place. Recording them
would buy a cleanup behaviour the CLI does not have and has not been asked for.

**Change detection moves out of the record and into the user's own tools.**
`update` overwrites managed files and then says where the previous content went.
When a `.git` directory is present it points at `git diff`; otherwise it points
at the recovery bundle that mutation commands already write. No git executable is
invoked — presence is a directory check.

**Permission grants become `allowInstallPaths` in the authored file.** The
entries are **paths, not patterns**: an entry naming a file admits that file, an
entry naming a directory admits everything beneath it, and there is no glob
language, so a reader can tell what an entry admits by looking at it. In a
terminal the CLI asks — allow always, allow once, cancel. Outside a terminal
`--allow-path` grants the same thing and the block message names it, so the
absence of a TTY is never the reason a workflow is impossible.

**`removedCategories`** records the root categories a user deleted, so install
and update do not reinstate them.

There is **no migration and no legacy reader**. The old files are not read, not
converted, and not honoured. This is available because the CLI has no released
users.

## Rationale

Splitting by nature rather than by feature is what fixes the original confusion:
one file a person owns, one file the tool owns, and a naming convention that says
which is which without documentation.

Merging them instead would force every `install`, `update`, and `attach` to
rewrite the file the user hand-edits, which means the serializer must preserve
comments, key order, and foreign keys or it destroys authored content. Two files
remove that requirement entirely.

Removing the gate is what makes the lock affordable. Integrity was the expensive
half — it demanded per-file hashes, normalization policy, and a verdict on every
command — and every ship-blocker in this area came from it. Once the question is
only "what did we put here", a path list answers it, and a wrong answer costs a
missed deletion rather than an unusable workspace.

Delegating change detection to the user's tools rather than to git-the-dependency
keeps the CLI honest about what it knows. A fingerprint chain across versions was
considered and rejected as unmaintainable. Invoking git would answer a different
question — "changed since the last commit" rather than "changed since we wrote
it" — and would silently overwrite an edit the user committed alongside other
work.

## Alternatives And Tradeoffs

- **One `open-forge.json` holding both** — fewest files, but reinstates the
  diagnosed confusion at smaller size and forces round-trip-safe serialization
  over a user-edited document.
- **Keeping one semantic fingerprint per file** — correct and roughly 43% smaller
  than today, but keeps integrity as a concept and with it the normalization
  policy that produced the CRLF and trailing-whitespace blocks.
- **Version plus file list, recomputing expected bytes from the embedded
  payload** — the smallest record, but wrong on the primary path: a newer binary
  comparing files an older one wrote reports every file the release touched as
  locally modified.
- **Invoking git for change detection** — formatting-tolerant and
  path-independent, but answers the wrong question and adds git-on-PATH,
  non-repository, submodule, and nested-repository cases.
- **Per-subject permission grants**, naming both the subject and the destination,
  are tighter than a plain path list but are the shape that made the model
  impossible to hand-write.

The accepted risk: `extension remove` deletes by the lock's file list, so a wrong
lock over-deletes. Writing each entry from a verified receipt removes the largest
source of wrongness, because the list is no longer derived from a payload that a
later binary may describe differently. What remains is contained by deleting only
paths that still exist and still resolve inside the owner's declared destination
boundary, and by the recovery bundle written before any deletion.

The destination boundary uses the shared `allowInstallPaths` list for all
owners, with `.agents/` implicitly admitted and the existing reserved-path and
physical-containment checks applied. There is no separate per-extension
destination declaration. The maintainer confirmed this interpretation during A4.

A stale or missing lock therefore fails toward a **missed** deletion rather than
an over-deletion: no claims means nothing to delete, and the command says so.

## Consequences

- Separate lifecycle, Library and permission state become authored settings and
  one generated ownership lock. Old files are not migrated or automatically deleted.
- The legacy `open-forge.extensions.json` has no runtime role. It is not read,
  migrated, or given a dedicated informational notice. The maintainer retired
  the earlier one-time notice requirement during pre-G4 reconciliation; no
  reporting state or compatibility subsystem is added for it.
- `Framework/Permissions` is deleted as a subsystem; grants become authored
  settings read through the same reader as the rest of the file.
- Commands stop disagreeing about the same file. The `attention` versus `blocked`
  split between `update` and `extension install` cannot recur, because neither
  consults a baseline.
- `doctor` no longer reports a missing lifecycle document on a Framework source
  checkout.
- A clone, a `prettier` run, an editor's trim-on-save, and `core.autocrlf` stop
  being able to block a workspace.
- **`extension remove` stops distinguishing a changed file from an unchanged
  one.** `baselineFingerprint` was that distinction's only input, so deleting it
  collapses `ChangedFinalOwner` and `UnchangedFinalOwner` into one final-owner
  class. A path the CLI owns and that nothing else still owns is deleted. The
  `KeepAsUnmanaged` action and the changed-content policy that selected it are
  removed from the command contract. Protection for an edited file moves to the
  recovery bundle written before the deletion and to `git diff`, which is the
  same delegation `update` already makes.
- **`baselineFingerprint` and `fingerprintKind` leave public output entirely.**
  They are dropped from the JSON documents and human renderers of `status`,
  `update`, `extension update`, `extension inspect`, and `doctor` rather than
  reduced to a weaker signal. A path appearing in the lock already carries the
  only claim that survives — that the CLI put it there.
- Every accepted record naming the old files becomes wrong on the day the file
  it names disappears, and is updated with the step that removes it.
- `rules` and `thresholds` are **not** part of this decision. They are settings
  that belong in the authored file eventually, but choosing their shape is
  diagnosis work and is deferred.

## Authoritative Sources

- [CLI Framework layer](../../documents/cli/layers/framework.md)
- [Ownership And Source Alignment technical design](../../documents/cli/technical-designs/lifecycle-provenance.md)
- [Workspace Libraries technical design](../../documents/cli/technical-designs/workspace-libraries.md)
- [CLI Experience Remediation task, group G1](../../../working/cli-development/tasks/task30-cli-experience-remediation.md)
- [Lifecycle baselines and architecture analysis](../../../emerging/analysis/cli-experience-audit/lifecycle-baselines-and-architecture.md)
- [Repository dogfood and configuration analysis](../../../emerging/analysis/cli-experience-audit/repository-dogfood-and-configuration.md)

## Decision Relationships

- [Source and packaging](source-and-packaging.md)
- [Core primitive roles](core-primitives.md)
- [CLI dependency policy](../cli-dependency-policy.md)
