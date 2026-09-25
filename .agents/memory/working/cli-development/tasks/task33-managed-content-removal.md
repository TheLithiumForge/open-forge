---
open-forge:
  description: Open Task 33 to decide whether individually removing managed Extension and Framework content is supported, and to reconcile the removability promise with what the commands actually allow
  tags: [Memory, Working, CLI, Task, Removal, Ownership, Extension, Contextual, Active]
---

# Task 33 — Managed Content Removal

## Task state

[Task 50](../../../archived/cli-development/tasks/task50-unified-remove.md) now implements this requirement through the
unified root command under the maintainer's 2026-09-23 authorization. Its current
specification resolves the command shape, settings exclusions and explicit-file
ownership release. The questions below retain their earlier investigation context.

- State: **Open, not started. The central decision is made.** Raised by the
  maintainer on 2026-09-16 while checking the "almost all files are removable on
  demand" promise, and ruled the same day.
- Owner: Root.
- Evidence: gathered and recorded below from the merged code, not from the
  catalogues.

## Accepted decision

**Per-file removal of managed content is wanted and must be built.** The
maintainer ruled that a user who does not want one file from an Extension should
be able to remove it, without removing the whole package, and that **future
updates must respect that removal** rather than silently restoring the file.

The Task is therefore no longer "should this exist". What remains is the design
and its consequences, below.

## The gap, corrected

The workspace promises that almost all files are removable on demand. That holds
for user-authored content. It does **not** hold for content a manager owns.

The maintainer's framing was that Library already supports this and Extension
does not. **That is not the case, and the gap is wider than it appeared:
neither manager supports per-file removal.**

| Command                       | Granularity                                        |
| ----------------------------- | -------------------------------------------------- |
| `route remove <id-or-path>`   | a single leaf file, **or** a whole category/route   |
| `extension remove <id>`       | a whole Extension package                          |
| `library detach <id>`         | a whole Library registration, all of its links     |

`library detach` takes a Library id, not a path — `Detached <id>: removed <N>
links under <destination>.` — and its only option is `--automatic`. The Library
subcommands are `attach`, `detach`, `inspect`, `list`, `sync`; the Extension
subcommands are `create`, `inspect`, `install`, `list`, `remove`, `update`.
Neither set has a per-file verb.

So the only command that removes an individual file is `route remove`, which is
exactly the command that refuses on managed content. **The design work below
should cover both managers, not Extensions alone.**

## What is true today

Two commands remove content, at different granularity.

| Command                       | Granularity                                        |
| ----------------------------- | -------------------------------------------------- |
| `route remove <id-or-path>`   | a single leaf file, **or** a whole category/route   |
| `extension remove <id>`       | a whole Extension package only                     |

`route remove` genuinely supports both shapes — its catalogue carries
`Removed <path>` for a leaf and `Removed the route <id>  (<N> files)` for a
category. Neither command takes a path filter; both expose only `--automatic`
and `--dry-run`.

The ownership record `.agents/open-forge.lock.json` carries three sections,
`framework`, `extensions` and `libraries`
(`Framework/Ownership/WorkspaceOwnershipDefinitions.cs`).
`RouteOwnershipEvidence.Claims` collects **paths and regions** from `framework`
and `extensions`; `libraries` are excluded, because Library content is links and
is removed through `library detach`.

`route remove` then refuses when any claim falls inside the subject:

```csharp
internal static bool HasSubjectClaims(WorkspaceOwnershipRead ownership, RouteRemoveResolvedSubject subject)
    => RouteOwnershipEvidence.Claims(ownership).Any(claim => IsWithinSubject(subject, claim.Path));
```

producing `route-remove.ownership-claimed`, blocked at exit 5, with the cause
`The selected Route Remove subject contains lifecycle-managed paths.` and the
next actions `open-forge update` / `open-forge extension remove <id>`.

Three consequences follow, and the second is the one that makes this a Task:

1. An Extension-installed file cannot be removed individually.
2. **A category containing even one Extension-owned file cannot be removed
   either.** The claim check runs before the leaf/category branch and matches any
   claim within the subject, so ownership of one file blocks the whole route.
3. A user-authored file inside an Extension-installed category **is** removable,
   because nothing claims it. So "remove one observation" succeeds when the user
   wrote it and fails when the Extension shipped it.

Framework-owned content behaves the same way and points at `open-forge update`.

## Why the current behaviour may be right

The design is internally coherent, and this Task should not assume it is a
defect:

- Managed content is installed, updated and removed as a unit, so the ownership
  record and the filesystem stay in agreement.
- Hand-deletion is already detected rather than silently tolerated:
  `extension-list.installed-files-missing` reports it, and
  `extension remove`'s `missing-file-released` situation releases a claim whose
  file is already gone.
- A per-file removal that did not also amend the claim would desynchronise the
  lock file from disk, which is the condition `doctor` exists to find.

## Sequencing — read this before implementing

[Task 35](task35-removal-and-suppression-model.md) explores whether removal
should stay per-manager at all, or become a single `open-forge remove` that
recognizes what owns a path, backed by a shared suppression record.

**Do not implement this Task narrowly before Task 35 settles that shape.** If a
per-command `--path` verb ships first and Task 35 then concludes a unified verb
is right, the work is wasted and the CLI keeps a verb it did not want.

This Task owns the **requirement** — managed content must be removable per file
and must stay removed. Task 35 owns the **shape** and the suppression record that
both Tasks depend on.

## What to design

The decision is made; these are the open design questions.

### The hard one: what happens on the next update

A removal that the next `extension update` silently undoes is not a removal. The
system currently treats a claimed-but-absent file as a **problem**:
`extension-list.installed-files-missing` reports it, and `extension remove`'s
`missing-file-released` situation releases the claim. This feature needs the
same on-disk state to mean **a choice** instead.

So there must be a durable record distinguishing "the user removed this
deliberately" from "this file went missing". Decide where it lives — most likely
alongside the claim in `.agents/open-forge.lock.json` — and then decide what
`update` does when its source still ships that file:

- honour the removal silently,
- honour it and report it as a kept-back path, which is closest to the existing
  `kept-retired` vocabulary that `extension update` already uses, or
- report it as a divergence and ask.

Whatever is chosen, `doctor` must stop calling a deliberately removed file a
problem, or the workspace will never be clean again.

### Command shape

`extension remove <id>` takes a package id today. Decide whether per-file
removal is a path operand on it, a new verb, or an option — and make the Library
answer symmetric, since `library detach <id>` has the same shape and the same
gap. Prefer one consistent shape across both managers over two local ones.

### Region claims

Extensions claim **regions inside files**, not only whole files
(`extension.Regions` in the ownership document). Decide explicitly whether
sub-file removal is in scope. If it is out of scope, say so in the Task and make
the command refuse a region-claimed path with a clear reason rather than
half-working.

### Dependencies and sharing

`extension remove` already has `shared-file-kept` for a file claimed by more
than one package. Per-file removal needs the same care: removing a file two
packages claim must not break the second one.

### The blunt next actions

The blocked `route remove` path currently offers `open-forge update` /
`open-forge extension remove <id>`, which are poor answers to "I want this one
file gone". Once the capability exists, that next action should point at it.

## Actionable boundary

- Decide before implementing. This is a product question about what the
  workspace guarantees, not a refactor.
- Do not weaken the claim check to make removal work. Any per-file removal must
  keep the ownership record and disk consistent, or it is not an improvement.
- Confirm the Library side is genuinely covered by `library detach` rather than
  assumed; `Claims` excludes libraries, so the reasoning above does not apply
  to them unexamined.

## Acceptance

- Per-file removal of managed content exists for **both** Extensions and
  Libraries, in one consistent command shape.
- A removed file stays removed across `extension update` and `open-forge
  update`, with the chosen behaviour specified and evidenced by a test that
  removes a file, updates, and asserts it did not return.
- The ownership record and the filesystem agree after every path, and `doctor`
  does not report a deliberately removed file as a problem.
- A file claimed by more than one package cannot be removed in a way that breaks
  the other claimant.
- Region-claimed paths are either supported or refused with a clear reason;
  never half-supported.
- Consequence 2 above is addressed explicitly: blocking a whole category because
  one file inside it is managed should be an intended outcome, not an accident
  of check ordering.
- The removability promise, wherever it is written, matches what the commands
  now do.

## Axioms

- inherited - No local axioms; loaded ancestor axioms remain active.
