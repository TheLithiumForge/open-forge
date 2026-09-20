---
open-forge:
  description: "library sync: scenario collection"
  tags: [Memory, Document, CLI, Scenario, Evergreen]
---

# library sync: scenario collection

These are reviewed user outcomes. Selection does not mean execution passed. Improved targets remain subject to flow validation before new tests. Deferred cases retain only their identity and reason; they are not adopted specifications.

**Who:** A maintainer using Open Forge through the public CLI, without knowing its implementation or internal state model.

**Goal:** Make an existing Library match its complete current source without overwriting changed destination files.

Existing baseline: [Interface](../../../contracts/library/sync/interface.md), [Behavior](../../../contracts/library/sync/behavior.md). Exact interface and output differences must be reconciled before implementation.

See [scenario conventions](../_scenarios.md) and the [complete assessment](../../assessment.md).

## C27-01

**Situation:** Up to date

**Disposition:** Added. Worth retaining as an independently checked user outcome: Say team is up to date and nothing needs doing.

### Starting point

Complete source inventory, registration and link targets already agree.

Fixture: `LIB`. fixture recipe; not instantiated.

### Steps

```text
open-forge library sync team --automatic
```

### Expected result

Say team is up to date and nothing needs doing.



### Verification

Compare links, source bytes, registration and directory inventory for zero churn.

## C27-02

**Situation:** Links added

**Disposition:** Added. Worth retaining as an independently checked user outcome: Add its corresponding relative file link and describe the new link.

### Starting point

Add one eligible source file; all existing registered links remain current.

Fixture: `LIB`. fixture recipe; not instantiated.

### Steps

```text
open-forge library sync team --automatic
```

### Expected result

Add its corresponding relative file link and describe the new link.



### Verification

Verify exact source-relative mapping and preservation of existing links and source bytes.

## C27-03

**Situation:** Links removed

**Disposition:** Added. Worth retaining as an independently checked user outcome: Remove the retired destination link only and report that source-driven change.

### Starting point

Remove one eligible source file while leaving its registered destination link intact.

Fixture: `LIB`. fixture recipe; not instantiated.

### Steps

```text
open-forge library sync team --automatic
```

### Expected result

Remove the retired destination link only and report that source-driven change.



### Verification

Verify no source deletion and no removal of unregistered neighboring files.

## C27-04

**Situation:** Both

**Disposition:** Added. Worth retaining as an independently checked user outcome: Add and remove the respective links in one coherent sync, with accurate unchanged count.

### Starting point

Add one source file and retire a different source file in the same complete inventory.

Fixture: `LIB`. fixture recipe; not instantiated.

### Steps

```text
open-forge library sync team --automatic
```

### Expected result

Add and remove the respective links in one coherent sync, with accurate unchanged count.



### Verification

Compute added, retired and unchanged membership sets independently before comparing receipts.

## C27-05

**Situation:** Dry run

**Disposition:** Added. Worth retaining as an independently checked user outcome: Show the two proposed changes and no writes.

### Starting point

Use the both fixture on a fresh copy.

Fixture: `LIB`. fixture recipe; not instantiated.

### Steps

```text
open-forge library sync team --dry-run
```

### Expected result

Show the two proposed changes and no writes.



### Verification

Compare raw link targets, directory entries, registration, settings and external recovery.

## C27-06

**Situation:** Unknown id

**Disposition:** Added. Worth retaining as an independently checked user outcome: Explain that team is unknown rather than creating it from a matching folder.

### Starting point

A readable valid ownership lock contains no team Library.

Fixture: `LIB`. fixture recipe; ownership state must be independently verified.

### Steps

```text
open-forge library sync team --automatic
```

### Expected result

Explain that team is unknown rather than creating it from a matching folder.



### Verification

Verify no implicit attach or registration reconstruction.

## C27-07

**Situation:** Changed occupant

**Disposition:** Improved and added. A changed destination must be preserved while independent safe sync effects can remain useful.

### Starting point

Replace one registered destination link with an ordinary user file containing distinctive bytes. Add a different eligible source file whose unoccupied destination can be safely linked. Keep the complete source inventory, claims and all unaffected mappings readable.

### Steps

```text
open-forge library sync team --automatic
```

### Expected result

**Reviewed target:** Preserve the replaced ordinary file and name its exact path. Continue independently verified additions, restorations or retirements that do not depend on replacing that file. Retain the unresolved mapping and report partial completion without claiming the whole Library is current.

### Verification

Observe whether the reviewed target is achieved. Compare affected paths, relevant ownership and source bytes independently of the report. Preserve content outside the requested change. Record current disagreement as a failure or unresolved contract difference, not a new expectation.


## C27-08

**Situation:** Registered link gone

**Disposition:** Added. Worth retaining as an independently checked user outcome: Restore the missing link and explain that restoration, with the declared warning outcome.

### Starting point

Remove one registered link while keeping its source file and complete inventory.

Fixture: `LIB`. fixture recipe; not instantiated.

### Steps

```text
open-forge library sync team --automatic
```

### Expected result

Restore the missing link and explain that restoration, with the declared warning outcome.



### Verification

Verify the restored raw relative target and distinguish this from an added source member.

## C27-09

**Situation:** Source unreadable

**Disposition:** Added. Worth retaining as an independently checked user outcome: Say sync could not establish the complete source inventory and leave all links unchanged.

### Starting point

A required source directory cannot be fully read.

Fixture: `LIB`. fixture recipe; not instantiated.

### Steps

```text
open-forge library sync team --automatic
```

### Expected result

Say sync could not establish the complete source inventory and leave all links unchanged.



### Verification

Verify no retirement inference or link deletion from a partial scan.

**Execution constraint:** Prove any denied read genuinely fails under the invoking account; missing or malformed metadata is not an I/O failure.

## C27-10

**Situation:** Permission required

**Disposition:** Added. Worth retaining as an independently checked user outcome: Explain that destination permission is required before synchronization.

### Starting point

Revoke the saved grant for an owned destination outside .agents.

Fixture: `LIB`. fixture recipe; not instantiated.

### Steps

```text
open-forge library sync team --automatic
```

### Expected result

Explain that destination permission is required before synchronization.



### Verification

Verify ownership does not bypass revocation and no settings are saved implicitly.

## C27-11

**Situation:** No ownership record

**Disposition:** Added. Worth retaining as an independently checked user outcome: Report a completed ownership observation, select no claim, and keep unavailable counts unknown.

### Starting point

Remove the generated ownership lock while leaving source and links present.

Fixture: `LIB`. fixture recipe; ownership state must be independently verified.

### Steps

```text
open-forge library sync team --automatic
```

### Expected result

Report a completed ownership observation, select no claim, and keep unavailable counts unknown. Distinguish a missing, unreadable or unusable record from a valid empty record; do not invent an unknown-ID error. Preserve every file.



### Verification

Independently establish the record state and unchanged workspace. Verify no source inventory or selected Library was fabricated; a separate valid-record unknown-ID case must remain invalid-input.


Revision note: Reconciled to the explicit forgiving-ownership behavior in the supplied contract, not the obsolete capture status.

## C27-12

**Situation:** Lock held

**Disposition:** Added. Worth retaining as an independently checked user outcome: Explain contention and make no link or registration changes.

### Starting point

LOCK holds this workspace during a genuine sync plan.

Fixture: `LIB`. fixture recipe; not instantiated.

### Steps

```text
open-forge library sync team --automatic
```

### Expected result

Explain contention and make no link or registration changes.



### Verification

Verify actual lock ownership and all link entry kinds/targets.

**Execution constraint:** Demonstrate live same-workspace lock ownership and release; a lock file alone is insufficient.

## C27-13

**Situation:** Record invalid

**Disposition:** Added. Worth retaining as an independently checked user outcome: Report a completed ownership observation, select no claim, and keep unavailable counts unknown.

### Starting point

The ownership lock is malformed or uninterpretable.

Fixture: `LIB`. fixture recipe; ownership state must be independently verified.

### Steps

```text
open-forge library sync team --automatic
```

### Expected result

Report a completed ownership observation, select no claim, and keep unavailable counts unknown. Distinguish a missing, unreadable or unusable record from a valid empty record; do not invent an unknown-ID error. Preserve every file.



### Verification

Independently establish the record state and unchanged workspace. Verify no source inventory or selected Library was fabricated; a separate valid-record unknown-ID case must remain invalid-input.


Revision note: Reconciled to the explicit forgiving-ownership behavior in the supplied contract, not the obsolete capture status.

## C27-14

**Situation:** Write failed partial

**Disposition:** Added. Worth retaining as an independently checked user outcome: Describe the completed portion, unfinished changes and actual recovery/publication state.

### Starting point

Fail deterministically after one real addition or removal in a multi-change sync.

Fixture: `LIB`. fixture recipe; not instantiated.

### Steps

```text
open-forge library sync team --automatic
```

### Expected result

Describe the completed portion, unfinished changes and actual recovery/publication state.



### Verification

Compare each link and registration against receipts; do not claim global synchronization or automatic rollback.

**Execution constraint:** Establish a deterministic public process or real filesystem boundary and independently verify before/after effects; no timer-only or mocked-result proof.

## C27-15

**Situation:** Cancelled

**Disposition:** Added. Worth retaining as an independently checked user outcome: Report cancellation without changing links or source content.

### Starting point

Reject final confirmation in a real terminal before any link effect.

Fixture: `LIB`. fixture recipe; not instantiated.

### Steps

```text
open-forge library sync team
```

### Expected result

Report cancellation without changing links or source content.



### Verification

Record the prompt boundary and verify exact preservation; later cancellation has a separate truthfulness requirement.

**Execution constraint:** Use a genuine capability-appropriate terminal and preserve actual prompt/signal evidence.

## C27-S06

**Situation:** Outcome 06

**Disposition:** Deferred; not selected yet. Retained recovery matters, but this generic permutation has no demonstrated recovery-requiring plan or reliable post-success cleanup fault.

**Required before reconsideration:** Prove this exact plan requires recovery and synchronize successful effects with denied cleanup of its positively identified bundle.

## C27-16

**Situation:** A retired registration has no deletable link

**Disposition:** Improved and added. Positive absence requires no deletion and should not block releasing an obsolete claim.

### Starting point

Start from a verified registration. Remove a source member and also remove its destination link. Complete source inventory proves retirement, but the registered link to delete is no longer present.

### Steps

```text
open-forge library sync team --automatic
```

### Expected result

**Reviewed target:** With complete source inventory and safely observed destination absence, release the retired mapping without a physical deletion. Continue other independently safe sync effects and distinguish claim release from deleted links. Unknown or unsafe destination state remains unresolved.

### Verification

Observe whether the reviewed target is achieved. Compare affected paths, relevant ownership and source bytes independently of the report. Preserve content outside the requested change. Record current disagreement as a failure or unresolved contract difference, not a new expectation.
