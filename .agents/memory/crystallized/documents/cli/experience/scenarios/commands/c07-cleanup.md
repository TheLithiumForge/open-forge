---
open-forge:
  description: "cleanup: scenario collection"
  tags: [Memory, Document, CLI, Scenario, Evergreen]
---

# cleanup: scenario collection

These are reviewed user outcomes. Selection does not mean execution passed. Improved targets remain subject to flow validation before new tests. Deferred cases retain only their identity and reason; they are not adopted specifications.

**Who:** A maintainer using Open Forge through the public CLI, without knowing its implementation or internal state model.

**Goal:** See and remove eligible recovery leftovers without deleting source content or hiding partial removal.

Existing baseline: [Interface](../../../contracts/cleanup/interface.md), [Behavior](../../../contracts/cleanup/behavior.md). Exact interface and output differences must be reconciled before implementation.

See [scenario conventions](../_scenarios.md) and the [complete assessment](../../assessment.md).

## C07-01

**Situation:** Nothing to remove

**Disposition:** Added. Worth retaining as an independently checked user outcome: Say there is nothing to remove.

### Starting point

The selected workspace's complete recovery catalogue is empty.

Fixture: `RECOVERY`. fixture recipe; not instantiated.

### Steps

```text
open-forge cleanup
```

### Expected result

Say there is nothing to remove.



### Verification

Verify complete enumeration and no unrelated workspace's bundle was touched.

## C07-02

**Situation:** Two bundles one draft

**Disposition:** Added. Worth retaining as an independently checked user outcome: List what was removed and use correct bundle/draft counts.

### Starting point

RECOVERY contains exactly two eligible complete bundles and one eligible incomplete recovery draft.

Fixture: `RECOVERY`. fixture recipe; not instantiated.

### Steps

```text
open-forge cleanup
```

### Expected result

List what was removed and use correct bundle/draft counts.



### Verification

Compare the external catalogue's exact entries and hashes; source files in the workspace must be untouched.

## C07-03

**Situation:** Dry run

**Disposition:** Added. Worth retaining as an independently checked user outcome: List proposed removals in future tense and finish with No files were changed.

### Starting point

Use a fresh copy of the same recovery inventory.

Fixture: `RECOVERY`. fixture recipe; not instantiated.

### Steps

```text
open-forge cleanup --dry-run
```

### Expected result

List proposed removals in future tense and finish with No files were changed.



### Verification

Verify all bundles, drafts and workspace bytes remain, including permissions/settings.

## C07-04

**Situation:** Damaged bundle

**Disposition:** Improved and added. One damaged candidate need not veto deletion of independently verified cleanup items.

### Starting point

One candidate is readable but structurally damaged, alongside a valid candidate.

Fixture: `RECOVERY`. fixture recipe; not instantiated.

### Steps

```text
open-forge cleanup
```

### Expected result

**Reviewed target:** Retain the damaged or unidentified candidate and explain it. Remove independently identified eligible candidates belonging to this workspace after their ordinary checks pass. Report removed and retained items separately without guessing ownership or deleting the unclear item.

### Verification

Observe whether the reviewed target is achieved. Compare affected paths, relevant ownership and source bytes independently of the report. Preserve content outside the requested change. Record current disagreement as a failure or unresolved contract difference, not a new expectation.


## C07-05

**Situation:** Lock held

**Disposition:** Added. Worth retaining as an independently checked user outcome: Explain the active workspace operation rather than instructing removal of its lock file.

### Starting point

Hold the same workspace's mutation lock while eligible cleanup candidates exist.

Fixture: `RECOVERY`. fixture recipe; not instantiated.

### Steps

```text
open-forge cleanup
```

### Expected result

Explain the active workspace operation rather than instructing removal of its lock file.



### Verification

Prove actual contention and preserve every recovery candidate.

**Execution constraint:** Demonstrate live same-workspace lock ownership and release; a lock file alone is insufficient.

## C07-06

**Situation:** Store unreadable

**Disposition:** Added. Worth retaining as an independently checked user outcome: Say the store could not be read completely and nothing was removed, once.

### Starting point

Deny enumeration of the required recovery store under the invoking account.

Fixture: `RECOVERY`. fixture recipe; not instantiated.

### Steps

```text
open-forge cleanup
```

### Expected result

Say the store could not be read completely and nothing was removed, once.



### Verification

Verify denied access and no deletions; distinguish unknown catalogue size from zero candidates.




**Execution constraint:** Prove any denied read genuinely fails under the invoking account; missing or malformed metadata is not an I/O failure.

## C07-07

**Situation:** Deletion failed partial

**Disposition:** Added. Worth retaining as an independently checked user outcome: State how far cleanup got, what remains and what could not be removed.

### Starting point

Permit one eligible deletion, then cause a deterministic failure on the next candidate.

Fixture: `RECOVERY`. fixture recipe; not instantiated.

### Steps

```text
open-forge cleanup
```

### Expected result

State how far cleanup got, what remains and what could not be removed.



### Verification

Compare the external inventory with every receipt; do not promise all candidates remain after one was deleted.

**Execution constraint:** Establish a deterministic public process or real filesystem boundary and independently verify before/after effects; no timer-only or mocked-result proof.

## C07-08

**Situation:** Cancelled partial

**Disposition:** Added. Worth retaining as an independently checked user outcome: Report cancellation with the actual removed and remaining items, not Nothing was changed.

### Starting point

Interrupt only after observing one completed deletion from a multi-candidate cleanup.

Fixture: `RECOVERY`. fixture recipe; not instantiated.

### Steps

```text
open-forge cleanup
```

### Expected result

Report cancellation with the actual removed and remaining items, not Nothing was changed.



### Verification

Require reliable synchronization and a signal trace; timing guesses are not a reproducible scenario.

**Execution constraint:** Establish a deterministic public process or real filesystem boundary and independently verify before/after effects; no timer-only or mocked-result proof.

## C07-09

**Situation:** Invalid input

**Disposition:** Added. Worth retaining as an independently checked user outcome: Reject the input without enumerating or removing recovery items.

### Starting point

Recovery state is otherwise valid; pass an unsupported positional operand.

Fixture: `RECOVERY`. fixture recipe; not instantiated.

### Steps

```text
open-forge cleanup extra
```

### Expected result

Reject the input without enumerating or removing recovery items.



### Verification

Verify invalid-input exit, parser-stream behavior and unchanged recovery inventory.

## C07-S08

**Situation:** Outcome 08

**Disposition:** Deferred; not selected yet. A catalogue race without a reproducible cooperating-process boundary is an artificial state permutation.

**Required before reconsideration:** Show a permitted filesystem change between selection and guarded revalidation without bypassing the workspace lock.
