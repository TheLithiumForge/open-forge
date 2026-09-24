---
open-forge:
  description: "library attach: scenario collection"
  tags: [Memory, Document, CLI, Scenario, Evergreen]
---

# library attach: scenario collection

These are reviewed user outcomes. Selection does not mean execution passed. Improved targets remain subject to flow validation before new tests. Deferred cases retain only their identity and reason; they are not adopted specifications.

**Who:** A maintainer using Open Forge through the public CLI, without knowing its implementation or internal state model.

**Goal:** Link a contained shared source into this workspace, knowing exactly where links and permissions will be created.

Existing baseline: [Interface](../../../contracts/library/attach/interface.md), [Behavior](../../../contracts/library/attach/behavior.md). Exact interface and output differences must be reconciled before implementation.

See [scenario conventions](../_scenarios.md) and the [complete assessment](../../assessment.md).

## C26-01

**Situation:** Attached inside agents

**Disposition:** Added. Worth retaining as an independently checked user outcome: Register team and create relative file symlinks under the chosen destination, not copies or a directory symlink.

### Starting point

shared-guides is a real contained source; the destination route already exists and its leaf targets are unoccupied.

Fixture: `LIB-UNATTACHED`. fixture recipe; not instantiated.

### Steps

```text
open-forge library attach team shared-guides --to .agents/guidance/team --automatic
```

### Expected result

Register team and create relative file symlinks under the chosen destination, not copies or a directory symlink.



### Verification

Compare raw link targets, source bytes, registration and bounded Entries updates; .agents destinations need no extra grant.

## C26-02

**Situation:** Attached outside with flag

**Disposition:** Added. Worth retaining as an independently checked user outcome: Save the explicit scope and create only eligible planned links outside .agents.

### Starting point

The same contained source maps to unoccupied docs/team paths with no previous grant.

Fixture: `LIB-UNATTACHED`. fixture recipe; not instantiated.

### Steps

```text
open-forge library attach team shared-guides --to docs/team --allow-path docs/team --automatic
```

### Expected result

Save the explicit scope and create only eligible planned links outside .agents.



### Verification

Inspect saved scope separately from content effects; reserved destinations remain protected despite a parent grant.

## C26-03

**Situation:** Permission prompt

**Disposition:** Added. Worth retaining as an independently checked user outcome: Explain once versus always and the proposed descendants before obtaining approval.

### Starting point

docs/team is uncovered; use a genuine terminal and deliberately select a displayed approval scope.

Fixture: `LIB-UNATTACHED`. fixture recipe; not instantiated.

### Steps

```text
open-forge library attach team shared-guides --to docs/team
```

### Expected result

Explain once versus always and the proposed descendants before obtaining approval.



### Verification

Record the choice and verify its exact persistence behavior and destination scope.

**Execution constraint:** Use a genuine capability-appropriate terminal and preserve actual prompt/signal evidence.

## C26-04

**Situation:** Permission required non interactive

**Disposition:** Added. Worth retaining as an independently checked user outcome: Name the missing destination permission and stop without creating links.

### Starting point

Redirect execution with an uncovered docs/team destination and no allow-path grant.

Fixture: `LIB-UNATTACHED`. fixture recipe; not instantiated.

### Steps

```text
open-forge library attach team shared-guides --to docs/team --automatic
```

### Expected result

Name the missing destination permission and stop without creating links.



### Verification

Verify --automatic bypasses final confirmation only, not destination consent.

## C26-05

**Situation:** Empty source

**Disposition:** Added. Worth retaining as an independently checked user outcome: Register the empty Library and explain that it has no eligible files yet.

### Starting point

shared-guides is safely enumerable and contains no eligible files.

Fixture: `LIB-UNATTACHED`. fixture recipe; not instantiated.

### Steps

```text
open-forge library attach team shared-guides --to .agents/guidance/team --automatic
```

### Expected result

Register the empty Library and explain that it has no eligible files yet.



### Verification

Verify a real registration, zero file links and no invented copied content; later source additions belong in the sync flow.

## C26-06

**Situation:** Duplicate id

**Disposition:** Improved and added. An exact repeated attach differs from attempting to remap an existing ID.

### Starting point

Use separate copies: one identical existing team registration with current links, and one team registration pointing to a different source or destination.

### Steps

```text
open-forge library attach team shared-guides --to .agents/guidance/team --automatic
```

### Expected result

**Reviewed target:** Treat an identical verified registration and link mapping as already attached with no writes. If the same ID requests a different source or destination, explain the conflict and preserve the existing registration and files.

### Verification

Observe whether the reviewed target is achieved. Compare affected paths, relevant ownership and source bytes independently of the report. Preserve content outside the requested change. Record current disagreement as a failure or unresolved contract difference, not a new expectation.


## C26-07

**Situation:** Source missing

**Disposition:** Added. Worth retaining as an independently checked user outcome: Name the missing source folder and create no registration or destination.

### Starting point

shared-guides does not exist within the selected workspace.

Fixture: `LIB-UNATTACHED`. fixture recipe; not instantiated.

### Steps

```text
open-forge library attach team shared-guides --to .agents/guidance/team --automatic
```

### Expected result

Name the missing source folder and create no registration or destination.



### Verification

Verify no source-folder scaffold or external fallback.

## C26-08

**Situation:** Destination collision

**Disposition:** Added. Worth retaining as an independently checked user outcome: Explain the occupied leaf and preserve it; do not adopt an unregistered link.

### Starting point

One mapped destination already contains an unowned ordinary file or an unregistered exact-looking link.

Fixture: `LIB-UNATTACHED`. fixture recipe; not instantiated.

### Steps

```text
open-forge library attach team shared-guides --to .agents/guidance/team --automatic
```

### Expected result

Explain the occupied leaf and preserve it; do not adopt an unregistered link.



### Verification

Inspect entry kind, raw link target and bytes; the complete attach plan must not partly apply.

## C26-09

**Situation:** Dry run

**Disposition:** Added. Worth retaining as an independently checked user outcome: Show proposed links, directories and permission scope without creating or saving them.

### Starting point

Use a valid outside destination with an explicit allow-path request.

Fixture: `LIB-UNATTACHED`. fixture recipe; not instantiated.

### Steps

```text
open-forge library attach team shared-guides --to docs/team --allow-path docs/team --dry-run
```

### Expected result

Show proposed links, directories and permission scope without creating or saving them.



### Verification

Compare workspace and settings bytes and external recovery; no prompt is allowed.

## C26-10

**Situation:** Links unsupported

**Disposition:** Improved and added. A real link-creation failure must report any effects already made rather than promise unproved global rollback.

### Starting point

Use an environment where the required file-link operation is genuinely unsupported or denied, established independently.

Fixture: `LIB-UNATTACHED`. fixture recipe; not instantiated.

### Steps

```text
open-forge library attach team shared-guides --to .agents/guidance/team --automatic
```

### Expected result

**Reviewed target:** Explain the unsupported or denied link operation and never silently copy content or claim nonexistent links were registered. If the limitation is known before effects, change nothing; if discovered during application, report the actual created directories or links and unfinished work.

### Verification

Observe whether the reviewed target is achieved. Compare affected paths, relevant ownership and source bytes independently of the report. Preserve content outside the requested change. Record current disagreement as a failure or unresolved contract difference, not a new expectation.


**Execution constraint:** Use a supported host configuration that genuinely denies or cannot create file symlinks; record the actual failure stage.

## C26-11

**Situation:** Lock held

**Disposition:** Added. Worth retaining as an independently checked user outcome: Explain contention and preserve source, destination and ownership.

### Starting point

LOCK holds this workspace and the attach inputs are otherwise valid.

Fixture: `LIB-UNATTACHED`. fixture recipe; not instantiated.

### Steps

```text
open-forge library attach team shared-guides --to .agents/guidance/team --automatic
```

### Expected result

Explain contention and preserve source, destination and ownership.



### Verification

Prove live contention and no newly created parent directories.

**Execution constraint:** Demonstrate live same-workspace lock ownership and release; a lock file alone is insufficient.

## C26-12

**Situation:** Record invalid

**Disposition:** Added. Block Attach on invalid required ownership before link or record effects.

### Starting point

The generated ownership lock is malformed, while the explicit source is complete and the unoccupied destination is safely admitted. A separate absent-lock fixture proves that missing ownership is known empty.

Fixture: `LIB-UNATTACHED`. fixture recipe; ownership state must be independently verified.

### Steps

```text
open-forge library attach team shared-guides --to .agents/guidance/team --automatic
```

### Expected result

Block Attach before any effect. Preserve the malformed lock and explain the required correction. An absent lock does not block an otherwise safe Attach.



### Verification

Verify exact lock and source bytes remain unchanged, no destination links or registration are created, and no settings are changed. Independently verify that the absent-lock case creates only the requested links and new claim.


Revision note: Task 50 distinguishes an absent lock from invalid required ownership; the Attach contract defines this boundary.

## C26-13

**Situation:** Interrupted partial

**Disposition:** Added. Worth retaining as an independently checked user outcome: State completed and remaining links and whether registration/permission publication actually occurred.

### Starting point

Interrupt after one verified link creation in a multi-link attach, with a reliable external boundary.

Fixture: `LIB-UNATTACHED`. requires a controlled post-effect cancellation fixture; not instantiated.

### Steps

```text
open-forge library attach team shared-guides --to .agents/guidance/team --automatic
```

### Expected result

State completed and remaining links and whether registration/permission publication actually occurred.



### Verification

Prove the actual cancellation event and the verified first link. Expect cancelled/130 with the partial variant, not failed/1 and not a no-change sentence. Record remaining links, any saved permission, ownership publication and actual recovery disposition. A write failure without cancellation is a different case.

**Execution constraint:** Establish a deterministic public process or real filesystem boundary and independently verify before/after effects; no timer-only or mocked-result proof.

## C26-14

**Situation:** Invalid input

**Disposition:** Added. Worth retaining as an independently checked user outcome: Explain the ID problem without creating links or a registration.

### Starting point

Use an invalid ID with otherwise valid source and destination.

Fixture: `LIB-UNATTACHED`. fixture recipe; not instantiated.

### Steps

```text
open-forge library attach Bad_ID shared-guides --to .agents/guidance/team --automatic
```

### Expected result

Explain the ID problem without creating links or a registration.



### Verification

Verify no normalization, inferred identity or settings write.

## C26-S04

**Situation:** Outcome 04

**Disposition:** Deferred; not selected yet. Retained recovery matters, but this generic permutation has no demonstrated recovery-requiring plan or reliable post-success cleanup fault.

**Required before reconsideration:** Prove this exact plan requires recovery and synchronize successful effects with denied cleanup of its positively identified bundle.

## C26-S09

**Situation:** Outcome 09

**Disposition:** Deferred; not selected yet. Repeated cancellation outcome without a concrete signal boundary; retain X09 as the shared invariant until a distinct fixture earns coverage.

**Required before reconsideration:** Use a real signal-capable process and deterministic observed boundary; a timeout or invalid argument is not cancellation. Use a genuine capability-appropriate terminal and preserve actual prompt/signal evidence.
