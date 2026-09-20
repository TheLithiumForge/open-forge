---
open-forge:
  description: "route init: scenario collection"
  tags: [Memory, Document, CLI, Scenario, Evergreen]
---

# route init: scenario collection

These are reviewed user outcomes. Selection does not mean execution passed. Improved targets remain subject to flow validation before new tests. Deferred cases retain only their identity and reason; they are not adopted specifications.

**Who:** A maintainer using Open Forge through the public CLI, without knowing its implementation or internal state model.

**Goal:** Create a usable routed scope without learning every entrypoint convention first.

Existing baseline: [Interface](../../../contracts/route/init/interface.md), [Behavior](../../../contracts/route/init/behavior.md). Exact interface and output differences must be reconciled before implementation.

See [scenario conventions](../_scenarios.md) and the [complete assessment](../../assessment.md).

## C13-01

**Situation:** New chain

**Disposition:** Added. Worth retaining as an independently checked user outcome: Create the missing entrypoints and navigation in the requested chain, clearly marking any authoring still needed.

### Starting point

guidance exists; team and new-topic do not.

Fixture: `ROUTES`. fixture recipe; not instantiated.

### Steps

```text
open-forge route init guidance/team/new-topic
```

### Expected result

Create the missing entrypoints and navigation in the requested chain, clearly marking any authoring still needed.



### Verification

Inspect every new entrypoint, bounded parent-list edit and placeholder; do not claim invented descriptions are finished content.

## C13-02

**Situation:** Already initialized

**Disposition:** Added. Worth retaining as an independently checked user outcome: Say the route is already initialized and nothing needs changing.

### Starting point

The complete requested chain already exists and its navigation is current.

Fixture: `ROUTES`. fixture recipe; not instantiated.

### Steps

```text
open-forge route init guidance/team/new-topic
```

### Expected result

Say the route is already initialized and nothing needs changing.



### Verification

Compare exact bytes and verify no duplicate entries or needless metadata normalization.

## C13-03

**Situation:** Dry run

**Disposition:** Added. Worth retaining as an independently checked user outcome: Show the entrypoints and parent navigation that would be created or updated, with no writes.

### Starting point

Use the new-chain starting state on a fresh copy.

Fixture: `ROUTES`. fixture recipe; not instantiated.

### Steps

```text
open-forge route init guidance/team/new-topic --dry-run
```

### Expected result

Show the entrypoints and parent navigation that would be created or updated, with no writes.



### Verification

Compare the entire route tree and recovery inventory; future-tense scaffolding must not appear on disk.

## C13-04

**Situation:** Framework scaffold

**Disposition:** Added. Worth retaining as an independently checked user outcome: Create only the requested scoped Framework scaffold and explain what still needs authoring.

### Starting point

W1 has the required current Framework; select a non-root Framework scope supported by --framework.

Fixture: `ROUTES`. fixture recipe; not instantiated.

### Steps

```text
open-forge route init guidance/team --framework
```

### Expected result

Create only the requested scoped Framework scaffold and explain what still needs authoring.



### Verification

Compare the bounded scaffold with its intended source; no installation of unrelated categories.

## C13-05

**Situation:** Explicit metadata

**Disposition:** Added. Worth retaining as an independently checked user outcome: Apply supplied metadata to the final target, not as misleading copied descriptions of every ancestor.

### Starting point

The final target is absent and intermediate scopes also need scaffolding.

Fixture: `ROUTES`. fixture recipe; not instantiated.

### Steps

```text
open-forge route init guidance/team/new-topic --description "Team topic guidance" --tag=Guidance
```

### Expected result

Apply supplied metadata to the final target, not as misleading copied descriptions of every ancestor.



### Verification

Inspect intermediate and final metadata separately and verify ordered parent entries.

## C13-06

**Situation:** Invalid target

**Disposition:** Added. Worth retaining as an independently checked user outcome: Reject the target without normalizing it into a different authorized location.

### Starting point

Use a target containing a forbidden parent-traversal segment.

Fixture: `ROUTES`. fixture recipe; not instantiated.

### Steps

```text
open-forge route init guidance/../outside
```

### Expected result

Reject the target without normalizing it into a different authorized location.



### Verification

Inspect all possible affected paths for zero effects and no scope escape.

## C13-07

**Situation:** Invalid metadata

**Disposition:** Added. Worth retaining as an independently checked user outcome: Explain the invalid metadata without creating a partial route chain.

### Starting point

The target is valid but its explicitly supplied description is empty.

Fixture: `ROUTES`. fixture recipe; not instantiated.

### Steps

```text
open-forge route init guidance/team --description ""
```

### Expected result

Explain the invalid metadata without creating a partial route chain.



### Verification

Verify every missing entrypoint remains absent; a supplied invalid value is not equivalent to omission.

## C13-08

**Situation:** Framework not installed

**Disposition:** Added. Worth retaining as an independently checked user outcome: Explain that Framework installation is required; do not silently install it.

### Starting point

W0 has no installed Framework, but the request explicitly selects Framework scaffolding.

Fixture: `ROUTES`. fixture recipe; not instantiated.

### Steps

```text
open-forge route init guidance/team --framework
```

### Expected result

Explain that Framework installation is required; do not silently install it.



### Verification

Verify no .agents controls or partial scope were created and the proposed next action names installation.

## C13-09

**Situation:** Lock held

**Disposition:** Added. Worth retaining as an independently checked user outcome: Explain that the workspace is in use and preserve the chain.

### Starting point

LOCK is held while the requested new chain would otherwise be valid.

Fixture: `ROUTES`. fixture recipe; not instantiated.

### Steps

```text
open-forge route init guidance/team/new-topic
```

### Expected result

Explain that the workspace is in use and preserve the chain.



### Verification

Prove live contention and no newly created directories or entrypoints.

**Execution constraint:** Demonstrate live same-workspace lock ownership and release; a lock file alone is insufficient.

## C13-10

**Situation:** Write failed partial

**Disposition:** Added. Worth retaining as an independently checked user outcome: Report completed and unfinished parts accurately, with recovery where it exists.

### Starting point

Cause a reproducible failure after one created entrypoint in a multi-effect chain.

Fixture: `ROUTES`. fixture recipe; not instantiated.

### Steps

```text
open-forge route init guidance/team/new-topic
```

### Expected result

Report completed and unfinished parts accurately, with recovery where it exists.



### Verification

Compare every entrypoint and parent list; never label the full chain initialized when it is not.

**Execution constraint:** Establish a deterministic public process or real filesystem boundary and independently verify before/after effects; no timer-only or mocked-result proof.

## C13-11

**Situation:** Cancelled

**Disposition:** Added. Worth retaining as an independently checked user outcome: Report cancellation without claiming the chain was created.

### Starting point

Interrupt before the first actual entrypoint effect.

Fixture: `ROUTES`. controlled fixture required.

### Steps

```text
open-forge route init guidance/team/new-topic
```

### Expected result

Report cancellation without claiming the chain was created.



### Verification

Verify signal timing and exact filesystem preservation; test mid-effect interruption separately.

**Execution constraint:** Establish a deterministic public process or real filesystem boundary and independently verify before/after effects; no timer-only or mocked-result proof.

## C13-S04

**Situation:** Outcome 04

**Disposition:** Deferred; not selected yet. Retained recovery matters, but this generic permutation has no demonstrated recovery-requiring plan or reliable post-success cleanup fault.

**Required before reconsideration:** Prove this exact plan requires recovery and synchronize successful effects with denied cleanup of its positively identified bundle.

## C13-S05

**Situation:** Outcome 05

**Disposition:** Added. Worth retaining as an independently checked user outcome: Name the unavailable required fact and its consequence.

### Starting point

Use READ-DENIED on an actually required ordinary source, Template, route input or recovery fact for this operation. An ownership-only observation is not a required-input failure where this command’s explicit ownership contract says otherwise.

Fixture: `ROUTES`. controlled fixture required.

### Steps

```text
open-forge route init guidance/team/new-topic
```

### Expected result

Name the unavailable required fact and its consequence. Do not infer a complete empty inventory or apply a partial mutation plan.



### Verification

Prove the read failure and why this input is required. Verify known safe facts remain distinct from unknown values and that no target effect started.

**Execution constraint:** Prove any denied read genuinely fails under the invoking account; missing or malformed metadata is not an I/O failure.
