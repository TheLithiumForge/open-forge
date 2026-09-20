---
open-forge:
  description: "extension remove: scenario collection"
  tags: [Memory, Document, CLI, Scenario, Evergreen]
---

# extension remove: scenario collection

These are reviewed user outcomes. Selection does not mean execution passed. Improved targets remain subject to flow validation before new tests. Deferred cases retain only their identity and reason; they are not adopted specifications.

**Who:** A maintainer using Open Forge through the public CLI, without knowing its implementation or internal state model.

**Goal:** Remove selected package ownership and eligible content while preserving shared files and unrelated dependencies.

Existing baseline: [Interface](../../../contracts/extension/remove/interface.md), [Behavior](../../../contracts/extension/remove/behavior.md). Exact interface and output differences must be reconciled before implementation.

See [scenario conventions](../_scenarios.md) and the [complete assessment](../../assessment.md).

## C23-01

**Situation:** Single package

**Disposition:** Added. Worth retaining as an independently checked user outcome: Remove the selected package's eligible effects and explain what was removed.

### Starting point

toolkit is installed, has no installed dependents and owns ordinary eligible content exclusively.

Fixture: `PACKAGES-INSTALLED`. fixture recipe; not instantiated.

### Steps

```text
open-forge extension remove toolkit --automatic
```

### Expected result

Remove the selected package's eligible effects and explain what was removed.



### Verification

Verify target absence and selected claim removal; source package and unrelated content remain.

**Execution constraint:** Bind package IDs and bytes to a verified fixture catalogue/version pair; toolkit/base are fixture IDs, not assumed bundled packages.

## C23-02

**Situation:** Shared file kept

**Disposition:** Added. Worth retaining as an independently checked user outcome: Remove toolkit while explicitly keeping the shared file for its remaining owner.

### Starting point

toolkit and another installed package validly share one equal-content path.

Fixture: `PACKAGES-INSTALLED`. fixture recipe; not instantiated.

### Steps

```text
open-forge extension remove toolkit --automatic
```

### Expected result

Remove toolkit while explicitly keeping the shared file for its remaining owner.



### Verification

Verify shared bytes and surviving claims; do not call the file deleted merely because one claim was removed.

**Execution constraint:** Bind package IDs and bytes to a verified fixture catalogue/version pair; toolkit/base are fixture IDs, not assumed bundled packages.

## C23-03

**Situation:** Missing file released

**Disposition:** Added. Worth retaining as an independently checked user outcome: Release the claim and distinguish already missing from physically deleted.

### Starting point

One exclusively owned toolkit file is already absent but its claim is readable.

Fixture: `PACKAGES-INSTALLED`. fixture recipe; not instantiated.

### Steps

```text
open-forge extension remove toolkit --automatic
```

### Expected result

Release the claim and distinguish already missing from physically deleted.



### Verification

Compare pre/post directory entries and ownership; deletion counts must exclude nonexistent files.

**Execution constraint:** Bind package IDs and bytes to a verified fixture catalogue/version pair; toolkit/base are fixture IDs, not assumed bundled packages.

## C23-04

**Situation:** Orphaned dependency

**Disposition:** Added. Worth retaining as an independently checked user outcome: Remove toolkit, keep base and explain that the dependency is now unused.

### Starting point

Removing toolkit leaves base installed with no remaining dependent.

Fixture: `PACKAGES-INSTALLED`. fixture recipe; not instantiated.

### Steps

```text
open-forge extension remove toolkit --automatic
```

### Expected result

Remove toolkit, keep base and explain that the dependency is now unused.



### Verification

Verify base survives; do not silently cascade removal or call an orphan a blocker.

**Execution constraint:** Bind package IDs and bytes to a verified fixture catalogue/version pair; toolkit/base are fixture IDs, not assumed bundled packages.

## C23-05

**Situation:** Dependent blocks

**Disposition:** Added. Worth retaining as an independently checked user outcome: Name the dependent package and stop removal.

### Starting point

another installed package still requires toolkit.

Fixture: `PACKAGES-INSTALLED`. fixture recipe; not instantiated.

### Steps

```text
open-forge extension remove toolkit --automatic
```

### Expected result

Name the dependent package and stop removal.



### Verification

Verify all files and claims remain and the next action does not bypass dependency safety.

**Execution constraint:** Bind package IDs and bytes to a verified fixture catalogue/version pair; toolkit/base are fixture IDs, not assumed bundled packages.

## C23-06

**Situation:** Select prompt

**Disposition:** Added. Worth retaining as an independently checked user outcome: Show installed choices and honor the selected removal set.

### Starting point

Use a real terminal with several installed packages; choose toolkit and approve its plan.

Fixture: `PACKAGES-INSTALLED`. fixture recipe; not instantiated.

### Steps

```text
open-forge extension remove
```

### Expected result

Show installed choices and honor the selected removal set.



### Verification

Record selection and confirmation; preserve unselected packages except explicitly documented shared-claim changes.

**Execution constraint:** Use a genuine capability-appropriate terminal and preserve actual prompt/signal evidence. Bind package IDs and bytes to a verified fixture catalogue/version pair; toolkit/base are fixture IDs, not assumed bundled packages.

## C23-07

**Situation:** No selection non interactive

**Disposition:** Added. Worth retaining as an independently checked user outcome: Explain that a package selection is needed and do not default to all.

### Starting point

Redirect execution without package IDs.

Fixture: `PACKAGES-INSTALLED`. fixture recipe; not instantiated.

### Steps

```text
open-forge extension remove
```

### Expected result

Explain that a package selection is needed and do not default to all.



### Verification

Verify no hang or effects.

## C23-08

**Situation:** Not installed

**Disposition:** Added. Worth retaining as an independently checked user outcome: Say there is no installed toolkit content to remove, without deleting similarly named unowned files.

### Starting point

The readable ownership lock contains no toolkit claims.

Fixture: `PACKAGES-INSTALLED`. fixture recipe; not instantiated.

### Steps

```text
open-forge extension remove toolkit --automatic
```

### Expected result

Say there is no installed toolkit content to remove, without deleting similarly named unowned files.



### Verification

Compare known absent claims with untouched matching user paths; this is distinct from unavailable ownership.

**Execution constraint:** Bind package IDs and bytes to a verified fixture catalogue/version pair; toolkit/base are fixture IDs, not assumed bundled packages.

## C23-09

**Situation:** Dry run

**Disposition:** Added. Worth retaining as an independently checked user outcome: Show the removal and kept content in future tense with no writes.

### Starting point

Use a valid removal containing one deleted path and one shared kept path.

Fixture: `PACKAGES-INSTALLED`. fixture recipe; not instantiated.

### Steps

```text
open-forge extension remove toolkit --dry-run
```

### Expected result

Show the removal and kept content in future tense with no writes.



### Verification

Compare targets, claims, settings and recovery inventory.

**Execution constraint:** Bind package IDs and bytes to a verified fixture catalogue/version pair; toolkit/base are fixture IDs, not assumed bundled packages.

## C23-10

**Situation:** Permission required

**Disposition:** Added. Worth retaining as an independently checked user outcome: Explain that removal still needs destination permission.

### Starting point

A selected owned external path no longer has an applicable saved destination grant.

Fixture: `PACKAGES-INSTALLED`. fixture recipe; not instantiated.

### Steps

```text
open-forge extension remove toolkit --automatic
```

### Expected result

Explain that removal still needs destination permission.



### Verification

Verify ownership does not bypass revocation and source-independent removal does not mean permission-independent removal.

**Execution constraint:** Bind package IDs and bytes to a verified fixture catalogue/version pair; toolkit/base are fixture IDs, not assumed bundled packages.

## C23-11

**Situation:** Lock held

**Disposition:** Added. Worth retaining as an independently checked user outcome: Explain contention and preserve content and ownership.

### Starting point

LOCK holds this workspace during an otherwise eligible removal.

Fixture: `PACKAGES-INSTALLED`. fixture recipe; not instantiated.

### Steps

```text
open-forge extension remove toolkit --automatic
```

### Expected result

Explain contention and preserve content and ownership.



### Verification

Prove live lock ownership and exact no-write evidence.

**Execution constraint:** Demonstrate live same-workspace lock ownership and release; a lock file alone is insufficient. Bind package IDs and bytes to a verified fixture catalogue/version pair; toolkit/base are fixture IDs, not assumed bundled packages.

## C23-12

**Situation:** Write failed partial

**Disposition:** Added. Worth retaining as an independently checked user outcome: Say removal stopped, identify removed and remaining items, and describe actual recovery.

### Starting point

Cause a deterministic failure after one real removal effect.

Fixture: `PACKAGES-INSTALLED`. fixture recipe; not instantiated.

### Steps

```text
open-forge extension remove toolkit --automatic
```

### Expected result

Say removal stopped, identify removed and remaining items, and describe actual recovery.



### Verification

Compare file kinds, remaining claims and recovery; no false total removal or no-change statement.

**Execution constraint:** Establish a deterministic public process or real filesystem boundary and independently verify before/after effects; no timer-only or mocked-result proof. Bind package IDs and bytes to a verified fixture catalogue/version pair; toolkit/base are fixture IDs, not assumed bundled packages.

## C23-13

**Situation:** Cancelled

**Disposition:** Added. Worth retaining as an independently checked user outcome: Report cancellation and leave every package effect unapplied.

### Starting point

Reject final removal confirmation in a real terminal.

Fixture: `PACKAGES-INSTALLED`. fixture recipe; not instantiated.

### Steps

```text
open-forge extension remove toolkit
```

### Expected result

Report cancellation and leave every package effect unapplied.



### Verification

Record the prompt and verify all claims and files are unchanged.

**Execution constraint:** Use a genuine capability-appropriate terminal and preserve actual prompt/signal evidence. Bind package IDs and bytes to a verified fixture catalogue/version pair; toolkit/base are fixture IDs, not assumed bundled packages.

## C23-S05

**Situation:** Outcome 05

**Disposition:** Improved and added. Removal must not require the original package source; the generic required-input recipe leaves that distinction unclear.

### Starting point

Use a valid selected toolkit ownership claim and an owned existing file whose prior bytes are genuinely unreadable for required recovery. Remove the original package source as a separate source-independence counterpart.

### Steps

```text
open-forge extension remove toolkit --automatic
```

### Expected result

**Reviewed target:** If a safely selected owned file or required reference/recovery input cannot be read sufficiently for this removal, name the fact and withhold dependent deletion. Keep unrelated independent facts visible. The missing original package catalogue alone does not prevent ownership-based removal.

### Verification

Observe whether the reviewed target is achieved. Compare affected paths, relevant ownership and source bytes independently of the report. Preserve content outside the requested change. Record current disagreement as a failure or unresolved contract difference, not a new expectation.


**Execution constraint:** Independently prove the denied read and that it is required for the selected effect, not for package source lookup. Prove any denied read genuinely fails under the invoking account; missing or malformed metadata is not an I/O failure. Bind package IDs and bytes to a verified fixture catalogue/version pair; toolkit/base are fixture IDs, not assumed bundled packages.
