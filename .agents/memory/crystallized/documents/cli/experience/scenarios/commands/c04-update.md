---
open-forge:
  description: "update: scenario collection"
  tags: [Memory, Document, CLI, Scenario, Evergreen]
---

# update: scenario collection

These are reviewed user outcomes. Selection does not mean execution passed. Improved targets remain subject to flow validation before new tests. Deferred cases retain only their identity and reason; they are not adopted specifications.

**Who:** A maintainer using Open Forge through the public CLI, without knowing its implementation or internal state model.

**Goal:** Bring managed Framework content to the intended version while knowing what will be replaced or kept.

Existing baseline: [Interface](../../../contracts/update/interface.md), [Behavior](../../../contracts/update/behavior.md). Exact interface and output differences must be reconciled before implementation.

See [scenario conventions](../_scenarios.md) and the [complete assessment](../../assessment.md).

## C04-01

**Situation:** Up to date

**Disposition:** Added. Worth retaining as an independently checked user outcome: Say the Framework is up to date and there is nothing to do.

### Starting point

Owned files match the running bundle and no retired owned file exists.

Fixture: `W1`. fixture recipe; not instantiated.

### Steps

```text
open-forge update --automatic
```

### Expected result

Say the Framework is up to date and there is nothing to do.



### Verification

Compare all bytes and entries before/after; do not rewrite an unchanged lock.

## C04-02

**Situation:** Changed file replaced

**Disposition:** Added. Worth retaining as an independently checked user outcome: Replace the selected changed content and name the replacement.

### Starting point

Edit one owned Framework file's meaning; make the running bundle's intended bytes independently available.

Fixture: `W1`. fixture recipe; not instantiated.

### Steps

```text
open-forge update --automatic
```

### Expected result

Replace the selected changed content and name the replacement. Do not promise all local edits are preserved by update.



### Verification

Verify intended result and the actual prior-content recovery boundary; preview first in the linked flow.

## C04-03

**Situation:** Missing file restored

**Disposition:** Added. Worth retaining as an independently checked user outcome: Restore and identify that missing file, without calling it a replacement of existing bytes.

### Starting point

Remove one owned file, preserving its valid claim and intended source.

Fixture: `W1`. fixture recipe; not instantiated.

### Steps

```text
open-forge update --automatic
```

### Expected result

Restore and identify that missing file, without calling it a replacement of existing bytes.



### Verification

Compare restored bytes to the pinned intended file; ensure unrelated content is unchanged.

## C04-04

**Situation:** Retired kept

**Disposition:** Added. Worth retaining as an independently checked user outcome: Keep the retired file by default and explain that it remains from an earlier version.

### Starting point

Use VERSION-PAIR with one formerly managed path absent from the new bundle.

Fixture: `W1`. fixture recipe; not instantiated.

### Steps

```text
open-forge update --automatic
```

### Expected result

Keep the retired file by default and explain that it remains from an earlier version.



### Verification

Verify the kept path and its bytes, zero invented deletions and accurate kept count.

**Execution constraint:** Bind package IDs and bytes to a verified fixture catalogue/version pair; toolkit/base are fixture IDs, not assumed bundled packages.

## C04-05

**Situation:** Retired pruned

**Disposition:** Added. Worth retaining as an independently checked user outcome: Remove only eligible retired owned content and name it explicitly.

### Starting point

Use the same verified version pair; review which owned retired file is eligible for removal.

Fixture: `W1`. fixture recipe; not instantiated.

### Steps

```text
open-forge update --prune --automatic
```

### Expected result

Remove only eligible retired owned content and name it explicitly.



### Verification

Verify unowned neighbors survive and pruning does not adopt them into the removal set.

## C04-06

**Situation:** Dry run changes

**Disposition:** Added. Worth retaining as an independently checked user outcome: Show the concrete proposed effects using future tense and No files were changed.

### Starting point

Use a mix of one replacement and one restoration with no blocked target.

Fixture: `W1`. fixture recipe; not instantiated.

### Steps

```text
open-forge update --dry-run
```

### Expected result

Show the concrete proposed effects using future tense and No files were changed.



### Verification

Compare the entire workspace and recovery inventory; match the later apply plan only while inputs remain unchanged.

## C04-07

**Situation:** No ownership record

**Disposition:** Added. Worth retaining as an independently checked user outcome: Explain that update cannot establish its managed files and changes nothing.

### Starting point

Remove the generated ownership lock, retaining matching and changed files.

Fixture: `W1`. fixture recipe; not instantiated.

### Steps

```text
open-forge update --automatic
```

### Expected result

Explain that update cannot establish its managed files and changes nothing. Do not infer claims from matching content.



### Verification

Verify no files are adopted, deleted or replaced; distinguish unavailable ownership from known empty ownership.

## C04-08

**Situation:** Confirmation unavailable

**Disposition:** Added. Worth retaining as an independently checked user outcome: Explain the missing confirmation instead of starting or hanging.

### Starting point

Use a real change plan, redirected input/output and no automatic flag.

Fixture: `W1`. fixture recipe; not instantiated.

### Steps

```text
open-forge update
```

### Expected result

Explain the missing confirmation instead of starting or hanging.



### Verification

Check invalid-input outcome in the output baseline and no effects; record the prompt-capability boundary.

## C04-09

**Situation:** Write failed partial

**Disposition:** Added. Worth retaining as an independently checked user outcome: Report the completed portion, stopped operation and actual recovery location or failure.

### Starting point

Use FAULT-AFTER-EFFECT during a two-effect update.

Fixture: `W1`. controlled fixture required.

### Steps

```text
open-forge update --automatic
```

### Expected result

Report the completed portion, stopped operation and actual recovery location or failure.



### Verification

Compare each reported effect against disk; do not state all files were updated or all prior content restored without proof.

**Execution constraint:** Establish a deterministic public process or real filesystem boundary and independently verify before/after effects; no timer-only or mocked-result proof.

## C04-10

**Situation:** Cancelled

**Disposition:** Added. Worth retaining as an independently checked user outcome: Report cancellation without implying an update happened.

### Starting point

Reject final update confirmation in a real terminal before writes.

Fixture: `W1`. fixture recipe; not instantiated.

### Steps

```text
open-forge update
```

### Expected result

Report cancellation without implying an update happened.



### Verification

Verify exact pre/post equality; test post-effect cancellation separately.

**Execution constraint:** Use a genuine capability-appropriate terminal and preserve actual prompt/signal evidence.

## C04-11

**Situation:** Invalid input

**Disposition:** Added. Worth retaining as an independently checked user outcome: Reject the argument without selecting an update.

### Starting point

W1 is valid; provide an unsupported operation flag.

Fixture: `W1`. fixture recipe; not instantiated.

### Steps

```text
open-forge update --not-an-option
```

### Expected result

Reject the argument without selecting an update.



### Verification

Verify parser failure does not publish a misleading domain JSON success.

## C04-S06

**Situation:** Outcome 06

**Disposition:** Deferred; not selected yet. Retained recovery matters, but this generic permutation has no demonstrated recovery-requiring plan or reliable post-success cleanup fault.

**Required before reconsideration:** Prove this exact plan requires recovery and synchronize successful effects with denied cleanup of its positively identified bundle.

## C04-S07

**Situation:** Outcome 07

**Disposition:** Added. Worth retaining as an independently checked user outcome: Name the unavailable required fact and its consequence.

### Starting point

Use READ-DENIED on an actually required ordinary source, Template, route input or recovery fact for this operation. An ownership-only observation is not a required-input failure where this command’s explicit ownership contract says otherwise.

Fixture: `W1`. controlled fixture required.

### Steps

```text
open-forge update --automatic
```

### Expected result

Name the unavailable required fact and its consequence. Do not infer a complete empty inventory or apply a partial mutation plan.



### Verification

Prove the read failure and why this input is required. Verify known safe facts remain distinct from unknown values and that no target effect started.

**Execution constraint:** Prove any denied read genuinely fails under the invoking account; missing or malformed metadata is not an I/O failure.

## C04-S09

**Situation:** Outcome 09

**Disposition:** Added. Worth retaining as an independently checked user outcome: Name the target conflict and preserve the complete selected update.

### Starting point

A planned owned target has changed into an unsafe linked-ancestry or other-owner conflict. Use the physical conflict, not just an invalid ownership lock.

Fixture: `W1`. fixture recipe; not instantiated.

### Steps

```text
open-forge update --automatic
```

### Expected result

Name the target conflict and preserve the complete selected update.



### Verification

Verify no replacement, pruning or adoption across that boundary; apply the owning command’s ownership-observation rule separately.
