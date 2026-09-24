---
open-forge:
  description: Open Task 35 to explore a unified removal and suppression model across Routes, Extensions and Libraries, pinning current behaviour with characterization tests before any design is chosen
  tags: [Memory, Working, CLI, Task, Exploration, Removal, Ownership, Lifecycle, Contextual, Active]
---

# Task 35 — Removal and Suppression Model

## Task state

The maintainer explicitly authorized specification and implementation of the
unified command on 2026-09-23. [Task 50](task50-unified-remove.md) carries that
decision and delivery. The open questions below are retained investigation
context, not a competing current implementation plan.

- State: **Open, not started. Exploration, not implementation.** Raised by the
  maintainer on 2026-09-16 as a set of use cases they expect to hit later.
- Owner: Root.
- Nature: this Task answers questions. It does not authorize a command change.
  It terminates in a recorded design decision that a later Task implements.

## Why this exists

The maintainer raised a cluster of related instincts about removing things and
having them stay removed. They were explicit that these are **remembered use
cases rather than verified claims**, and asked that each be pinned by a test so
the exploration reasons from evidence instead of recollection.

Two of the three recollections were checked immediately and are recorded below —
one was right, one was right in a different command than remembered. That is
exactly why the tests come first.

## Verified starting facts

Checked against the merged tree on 2026-09-16. **Re-verify before relying on
these**; they are a starting point, not a specification.

| Command                     | Granularity                                          |
| --------------------------- | ---------------------------------------------------- |
| `route remove <id-or-path>` | one leaf file, **or** a whole category/route          |
| `extension remove <id>`     | a whole Extension package                            |
| `library detach <id>`       | a whole Library registration and all of its links    |

- `route remove` is the only command that removes an individual file, and it
  **refuses on managed content** — `route-remove.ownership-claimed`, blocked at
  exit 5, because `RouteOwnershipEvidence.Claims` covers `framework` and
  `extensions` paths and regions.
- `library detach` is **per source**, as the maintainer recalled: its headline is
  `Detached <id>: removed <N> links under <destination>.` and it takes no path.
- **`library sync` re-adds a removed link.** This is the behaviour the
  maintainer remembered. Its `registered-link-gone` situation recreates the link
  and, as of the ruling made the same day, reports
  `library-sync.registered-link-restored` —
  `<destination path> was registered but missing, so it was restored.` at
  `completed-with-warnings`, exit 2.
- **No suppression, exclusion or blacklist concept exists anywhere** in the
  ownership or lifecycle model. There is no way to record that a path is absent
  on purpose.

## The tension this creates with a decision already made

The `registered-link-restored` ruling above was made on 2026-09-16 and is
correct **given that nothing can record intent**: a registered link that has
vanished is drift, and repairing drift is what `sync` is for.

The moment a suppression model exists, that situation splits in two:

- the link is missing and **not** suppressed — restore it and warn, as today;
- the link is missing because the user **removed it deliberately** — leave it
  alone.

So this Task may revise that ruling. That is expected, and is not a reversal of
a mistake: it is the same situation becoming distinguishable for the first time.
Whoever picks this up should read
[36 — library sync](task30-g4/36-library-sync.md) divergences 1, 6 and 7 first.

## Step 1 — pin the current behaviour with tests

Before any design work, write characterization tests that assert what actually
happens today, one per use case. The maintainer asked for this explicitly: these
are the situations they expect to encounter, and the record must be evidence.

At minimum:

1. Remove a Library-linked file by hand, run `library sync`, assert the link
   returns.
2. Remove a file installed by an Extension by hand, run `extension update`, and
   record whether it returns.
3. Try `route remove` on an Extension-owned leaf; assert it is blocked and record
   the exact message and next actions.
4. Try `route remove` on a user-authored file inside an Extension-installed
   category; assert it succeeds.
5. Try `route remove` on a category containing one Extension-owned file; assert
   the whole category is refused.
6. `library detach` then `library attach` again — record whether anything about
   a previously removed link is remembered.

These tests are valuable whatever the design turns out to be, because they
document the seam. Keep them even if the behaviour later changes; update them
with the decision.

## Step 2 — the questions to answer

### Shape: one command or three

The maintainer's instinct: rather than three manager-specific removal verbs,
there might be a single `open-forge remove <path-or-id>` that **recognizes what
owns the thing** and does the right thing — refusing, releasing a claim, or
suppressing — instead of making the user know which manager owns a path before
they can remove it.

Weigh that against the existing design, where each manager owns its own
lifecycle and the ownership record is the source of truth. A unified verb is a
better user experience and a worse ownership boundary; decide which matters more
here, and whether a unified verb can delegate rather than reimplement.

### Scope: what does a suppression actually record

The maintainer offered two candidate shapes and did not pick one:

- **blacklist the thing entirely** — this path stays gone, whoever would add it;
- **scope it by source** — this path stays gone *as far as this Extension or
  Library is concerned*, leaving another owner free to provide it.

Decide, and decide what the unit is: an exact path, a path with its region
claims, a subtree, or a pattern. Extensions claim **regions inside files**, so
whole-path suppression may not be expressive enough.

### Consultation: who must honour it

Every lifecycle path that adds files must consult the record, or the feature
leaks: `library sync`, `extension install`, `extension update`, `open-forge
install`, `open-forge update`, and `repair`. Enumerate them from the code rather
than from this list.

### Reporting

- What does `doctor` say about a suppressed path? It must stop calling a
  deliberate absence a problem.
- What do `library list`, `extension list` and `extension inspect` show for one?
  A suppressed path is neither healthy nor broken, and there is currently no
  vocabulary for that third state.

### Lifecycle

Does a suppression survive `detach` then `attach`, or `extension remove` then
`install` again? Both answers are defensible; pick one and say why.

### Was a Library removal verb ever planned?

The maintainer recalls that Library was at some point meant to have a removal of
its own — possibly called `remove`, possibly `unlink` — distinguishing "remove
this entirely" from "remove it from *this* source so it is not linked back".
That second meaning is the suppression question above, arriving from a different
direction.

**Searched on 2026-09-16 and not found.** The only `unlink` in memory is npm
global `link`/`unlink` in the distribution records, which is package
installation and unrelated. No archived or working record proposes a Library
removal verb. The current Library surface is `attach`, `detach`, `inspect`,
`list`, `sync`.

So this is a **recollection without a record**, which is worth resolving rather
than dropping: either the intent predates the retained records, or it was a
different thing remembered under that name. Recheck before designing, and if
nothing turns up, treat it as a fresh design question rather than a revival —
but note that the distinction the maintainer drew ("remove entirely" versus
"remove from this source so it does not come back") is exactly the axis this
Task already has to settle, so the instinct stands even if the record does not.

## Relationship to Task 33 — sequencing matters

[Task 33](task33-managed-content-removal.md) carries an accepted decision that
per-file removal of managed content must exist and must survive updates.

**Do not implement Task 33 narrowly before this Task settles the shape.** If
Task 33 ships `extension remove --path` and this Task then concludes that a
single `open-forge remove` with a shared suppression record is right, the work is
wasted and the CLI carries a verb it did not want. The suppression record is the
part both Tasks need, and it belongs to this one.

Task 33 owns the *requirement*. This Task owns the *shape*.

## Acceptance

- The six characterization tests exist and pass against current behaviour, with
  the observed results recorded here.
- A recorded decision on the command shape, the suppression unit, its storage,
  and which lifecycle paths consult it — with the reasoning, including the
  option not chosen.
- A recorded decision on whether the `registered-link-restored` ruling changes.
- A follow-up implementation Task, or an explicit record that the exploration
  concluded no change is wanted.

## Axioms

- inherited - No local axioms; loaded ancestor axioms remain active.
