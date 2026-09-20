---
open-forge:
  description: "route move: scenario collection"
  tags: [Memory, Document, CLI, Scenario, Evergreen]
---

# route move: scenario collection

These are reviewed user outcomes. Selection does not mean execution passed. Improved targets remain subject to flow validation before new tests. Deferred cases retain only their identity and reason; they are not adopted specifications.

**Who:** A maintainer using Open Forge through the public CLI, without knowing its implementation or internal state model.

**Goal:** Move material and keep its authored links working without losing text or overwriting another source.

Existing baseline: [Interface](../../../contracts/route/move/interface.md), [Behavior](../../../contracts/route/move/behavior.md). Exact interface and output differences must be reconciled before implementation.

See [scenario conventions](../_scenarios.md) and the [complete assessment](../../assessment.md).

## C16-01

**Situation:** Leaf move

**Disposition:** Added. Worth retaining as an independently checked user outcome: Move the leaf and update its old/new parent navigation.

### Starting point

An unowned valid notes leaf has no authored incoming or relocation-sensitive outgoing links; destination is absent.

Fixture: `LINKS`. fixture recipe; not instantiated.

### Steps

```text
open-forge route move guidance/notes guidance/moved-note
```

### Expected result

Move the leaf and update its old/new parent navigation.



### Verification

Verify old path absence, new path content and no duplicate route identity.

## C16-02

**Situation:** Leaf move with rewritten links

**Disposition:** Added. Worth retaining as an independently checked user outcome: Move the source and rewrite exactly the affected link destinations while preserving their visible text.

### Starting point

notes.md has authored incoming links and outgoing relative links whose target resolution would change on relocation.

Fixture: `LINKS`. fixture recipe; not instantiated.

### Steps

```text
open-forge route move guidance/notes patterns/moved-note
```

### Expected result

Move the source and rewrite exactly the affected link destinations while preserving their visible text.



### Verification

Resolve every before/after link independently; compare labels, fragments, unrelated text and both parent lists.

## C16-03

**Situation:** Category move

**Disposition:** Added. Worth retaining as an independently checked user outcome: Move the complete category and adjust every affected route and authored reference.

### Starting point

An unowned category has nested routes, ordinary support files and an overwrite pair; destination is absent.

Fixture: `LINKS`. fixture recipe; not instantiated.

### Steps

```text
open-forge route move guidance/team patterns/team
```

### Expected result

Move the complete category and adjust every affected route and authored reference.



### Verification

Enumerate the full moved path mapping and outside-reference edits; do not treat only the entrypoint as the category.

## C16-04

**Situation:** Dry run

**Disposition:** Added. Worth retaining as an independently checked user outcome: Show the move and reference/navigation changes without changing any path or byte.

### Starting point

Use the linked leaf move on an untouched fixture copy.

Fixture: `LINKS`. fixture recipe; not instantiated.

### Steps

```text
open-forge route move guidance/notes patterns/moved-note --dry-run
```

### Expected result

Show the move and reference/navigation changes without changing any path or byte.



### Verification

Compare the whole workspace, including incoming references outside the moved subtree.

## C16-05

**Situation:** Destination exists

**Disposition:** Added. Worth retaining as an independently checked user outcome: Name the occupied destination and stop without overwriting or deleting either source.

### Starting point

patterns/moved-note.md already contains unrelated authored content.

Fixture: `LINKS`. fixture recipe; not instantiated.

### Steps

```text
open-forge route move guidance/notes patterns/moved-note
```

### Expected result

Name the occupied destination and stop without overwriting or deleting either source.



### Verification

Compare both files and all references; there is no silent merge or force behavior.

## C16-06

**Situation:** Destination inside source

**Disposition:** Added. Worth retaining as an independently checked user outcome: Explain that a category cannot be moved inside itself.

### Starting point

guidance/team is a real category; target is a descendant of itself.

Fixture: `LINKS`. fixture recipe; not instantiated.

### Steps

```text
open-forge route move guidance/team guidance/team/nested
```

### Expected result

Explain that a category cannot be moved inside itself.



### Verification

Verify no recursive copy, newly created descendant or partial move.

## C16-07

**Situation:** Self move

**Disposition:** Improved and added. Moving a source to its current identity is a clear converged state.

### Starting point

The source and destination resolve to the same identity.

Fixture: `LINKS`. fixture recipe; not instantiated.

### Steps

```text
open-forge route move guidance/notes guidance/notes
```

### Expected result

**Reviewed target:** Explain that the source is already at the destination and perform no writes, normalization, reference edits or ownership changes.

### Verification

Observe whether the reviewed target is achieved. Compare affected paths, relevant ownership and source bytes independently of the report. Preserve content outside the requested change. Record current disagreement as a failure or unresolved contract difference, not a new expectation.


## C16-08

**Situation:** Managed source

**Disposition:** Added. Worth retaining as an independently checked user outcome: Explain the ownership restriction and use the relevant management action rather than bypassing it.

### Starting point

Select an actual Framework- or Extension-owned route whose ownership is valid.

Fixture: `LINKS`. fixture recipe; not instantiated.

### Steps

```text
open-forge route move "$MANAGED_SOURCE" guidance/moved-managed
```

### Expected result

Explain the ownership restriction and use the relevant management action rather than bypassing it.



### Verification

Verify the managed source and receipts remain intact.

## C16-09

**Situation:** Source not found

**Disposition:** Added. Worth retaining as an independently checked user outcome: Name the missing source and do not create the destination.

### Starting point

The operand names no existing source.

Fixture: `LINKS`. fixture recipe; not instantiated.

### Steps

```text
open-forge route move guidance/does-not-exist guidance/moved-note
```

### Expected result

Name the missing source and do not create the destination.



### Verification

Verify no best-match or parent fallback.

## C16-10

**Situation:** Ambiguous source prompt

**Disposition:** Added. Worth retaining as an independently checked user outcome: Honor the selected physical source and leave the other candidate alone.

### Starting point

COLLISION is queried in a genuine terminal; choose the intended candidate and a clear absent destination.

Fixture: `LINKS`. fixture recipe; not instantiated.

### Steps

```text
open-forge route move "$AMBIGUOUS_ID" guidance/moved-note
```

### Expected result

Honor the selected physical source and leave the other candidate alone.



### Verification

Verify the selected mapping and all rewritten references; record choice identity in the run evidence.

**Execution constraint:** Use a genuine capability-appropriate terminal and preserve actual prompt/signal evidence.

## C16-11

**Situation:** Reference scan incomplete

**Disposition:** Added. Worth retaining as an independently checked user outcome: Explain that safe reference updates cannot be established and leave the move unapplied.

### Starting point

One required incoming-reference candidate is unreadable, even though the source and destination are otherwise valid.

Fixture: `LINKS`. fixture recipe; not instantiated.

### Steps

```text
open-forge route move guidance/notes guidance/moved-note
```

### Expected result

Explain that safe reference updates cannot be established and leave the move unapplied.



### Verification

Verify the complete no-write set includes source, destination and already scanned references.

**Execution constraint:** Prove any denied read genuinely fails under the invoking account; missing or malformed metadata is not an I/O failure.

## C16-12

**Situation:** Lock held

**Disposition:** Added. Worth retaining as an independently checked user outcome: Explain that another operation is using the workspace and make no move.

### Starting point

LOCK holds this workspace while the move would otherwise be valid.

Fixture: `LINKS`. fixture recipe; not instantiated.

### Steps

```text
open-forge route move guidance/notes guidance/moved-note
```

### Expected result

Explain that another operation is using the workspace and make no move.



### Verification

Verify actual contention and unchanged old/new path existence.

**Execution constraint:** Demonstrate live same-workspace lock ownership and release; a lock file alone is insufficient.

## C16-13

**Situation:** Write failed partial

**Disposition:** Added. Worth retaining as an independently checked user outcome: Distinguish completed effects from unfinished work and explain actual recovery.

### Starting point

Fail after one real move/reference effect using a reproducible external fault boundary.

Fixture: `LINKS`. controlled fixture required.

### Steps

```text
open-forge route move guidance/notes guidance/moved-note
```

### Expected result

Distinguish completed effects from unfinished work and explain actual recovery.



### Verification

Compare path identity and every affected reference; do not infer atomic rollback from a failed status.

**Execution constraint:** Establish a deterministic public process or real filesystem boundary and independently verify before/after effects; no timer-only or mocked-result proof.

## C16-14

**Situation:** Cancelled

**Disposition:** Added. Worth retaining as an independently checked user outcome: Report cancellation with no move or link rewrites.

### Starting point

Cancel the ambiguous-source selection before any move starts.

Fixture: `LINKS`. fixture recipe; not instantiated.

### Steps

```text
open-forge route move "$AMBIGUOUS_ID" guidance/moved-note
```

### Expected result

Report cancellation with no move or link rewrites.



### Verification

Record the terminal choice boundary and compare all candidate files.

## C16-S04

**Situation:** Outcome 04

**Disposition:** Deferred; not selected yet. Retained recovery matters, but this generic permutation has no demonstrated recovery-requiring plan or reliable post-success cleanup fault.

**Required before reconsideration:** Prove this exact plan requires recovery and synchronize successful effects with denied cleanup of its positively identified bundle.

## C16-15

**Situation:** Ownership cannot establish an unmanaged subject

**Disposition:** Added. Worth retaining as an independently checked user outcome: Complete the ownership observation without planning or applying a move/removal.

### Starting point

The subject is a readable ordinary route. Its required ownership record is missing or unusable, while paths and reference files are otherwise safe and readable.

### Steps

```text
open-forge route move guidance/notes guidance/moved-note
```

### Expected result

Complete the ownership observation without planning or applying a move/removal. Explain that no route changed.



### Verification

Compare all source, destination, reference, index and ownership bytes. No new registration or positive unmanaged claim may be invented.
