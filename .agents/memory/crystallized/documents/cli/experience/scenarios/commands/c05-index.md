---
open-forge:
  description: "index: scenario collection"
  tags: [Memory, Document, CLI, Scenario, Evergreen]
---

# index: scenario collection

These are reviewed user outcomes. Selection does not mean execution passed. Improved targets remain subject to flow validation before new tests. Deferred cases retain only their identity and reason; they are not adopted specifications.

**Who:** A maintainer using Open Forge through the public CLI, without knowing its implementation or internal state model.

**Goal:** Make newly added or changed material discoverable without rewriting its authored content.

Existing baseline: [Interface](../../../contracts/index-candidate/interface.md), [Behavior](../../../contracts/index-candidate/behavior.md). Exact interface and output differences must be reconciled before implementation.

See [scenario conventions](../_scenarios.md) and the [complete assessment](../../assessment.md).

## C05-01

**Situation:** All current

**Disposition:** Added. Worth retaining as an independently checked user outcome: Say the checked Entries sections are current and nothing needs doing.

### Starting point

Every eligible child and generated Entries list already agrees in ROUTES.

Fixture: `ROUTES`. fixture recipe; not instantiated.

### Steps

```text
open-forge index
```

### Expected result

Say the checked Entries sections are current and nothing needs doing.



### Verification

Verify byte-identical repetition, including line endings, comments and authored paragraphs.




## C05-02

**Situation:** One stale

**Disposition:** Added. Worth retaining as an independently checked user outcome: Update the one affected list and show the actual changed/checked counts.

### Starting point

Add one valid child only under guidance; every other list is already current.

Fixture: `ROUTES`. fixture recipe; not instantiated.

### Steps

```text
open-forge index
```

### Expected result

Update the one affected list and show the actual changed/checked counts.



### Verification

Derive the changed-list set from a tree diff. The supplied capture named one-stale updates two lists and is not this fixture's oracle.




## C05-03

**Situation:** Dry run one stale

**Disposition:** Added. Worth retaining as an independently checked user outcome: Say which Entries section would change, with the correct before/after entry counts and no writes.

### Starting point

Use an untouched copy of the one-stale fixture.

Fixture: `ROUTES`. fixture recipe; not instantiated.

### Steps

```text
open-forge index --dry-run
```

### Expected result

Say which Entries section would change, with the correct before/after entry counts and no writes.



### Verification

Compare file bytes, directory entries and external recovery; require a later apply/repeat branch in the flow.




## C05-04

**Situation:** Explicit source

**Disposition:** Added. Worth retaining as an independently checked user outcome: Rebuild exactly the selected source's contract-defined scope and affected direct parent, not unrelated roots.

### Starting point

One selected guidance scope is stale; a separate patterns scope is also stale.

Fixture: `ROUTES`. fixture recipe; not instantiated.

### Steps

```text
open-forge index guidance
```

### Expected result

Rebuild exactly the selected source's contract-defined scope and affected direct parent, not unrelated roots.



### Verification

Compare the selected closure and actual changed paths; patterns must remain untouched.

## C05-05

**Situation:** Folder operand

**Disposition:** Improved and added. An ordinary folder with one known entrypoint is a clear user selection.

### Starting point

Use the existing ordinary .agents/guidance/ folder with exactly one recognized entrypoint. Keep a separate ambiguous-folder counterpart.

### Steps

```text
open-forge index .agents/guidance/
```

### Expected result

**Reviewed target:** Resolve .agents/guidance/ to its unique known entrypoint and index that same scope as guidance. If no unique entrypoint exists, explain the ambiguity or absence without selecting a parent or inventing a route.

### Verification

Observe whether the reviewed target is achieved. Compare affected paths, relevant ownership and source bytes independently of the report. Preserve content outside the requested change. Record current disagreement as a failure or unresolved contract difference, not a new expectation.


## C05-06

**Situation:** Unknown source

**Disposition:** Added. Worth retaining as an independently checked user outcome: Name the unknown reference and do not turn it into an empty successful index.

### Starting point

No route or file has the requested reference.

Fixture: `ROUTES`. fixture recipe; not instantiated.

### Steps

```text
open-forge index guidance/does-not-exist
```

### Expected result

Name the unknown reference and do not turn it into an empty successful index.



### Verification

Inspect the selected source inventory independently and verify no scaffold was created.

## C05-07

**Situation:** Blocked malformed leaf

**Disposition:** Improved and added. A malformed leaf should not stop independent lists whose inputs and generated boundaries are complete.

### Starting point

One guidance child has an unclosed YAML frontmatter delimiter. A separate patterns branch contains a newly added valid child and a stale generated list. All inputs and generated boundaries for that independent patterns list are known and readable.

### Steps

```text
open-forge index
```

### Expected result

**Reviewed target:** Name the malformed file and location, preserve its authored bytes and any list that cannot be safely formed, and update independently complete selected lists. Report completed and skipped work separately. Never rewrite malformed YAML or claim the whole request completed.

### Verification

Observe whether the reviewed target is achieved. Compare affected paths, relevant ownership and source bytes independently of the report. Preserve content outside the requested change. Record current disagreement as a failure or unresolved contract difference, not a new expectation.


## C05-08

**Situation:** Incomplete unreadable child

**Disposition:** Improved and added. An unreadable child limits its dependent list, not all unrelated indexing.

### Starting point

Make one required child genuinely unreadable under the running account. Do not use absent metadata to simulate it.

Fixture: `ROUTES`. fixture recipe; not instantiated.

### Steps

```text
open-forge index
```

### Expected result

**Reviewed target:** Report the unreadable child and preserve lists whose membership or values cannot be established. Continue independently complete selected lists, retain readable authored files unchanged, and report incomplete coverage without treating unknown children as absent.

### Verification

Observe whether the reviewed target is achieved. Compare affected paths, relevant ownership and source bytes independently of the report. Preserve content outside the requested change. Record current disagreement as a failure or unresolved contract difference, not a new expectation.


**Execution constraint:** Prove genuine access denial and include at least one independent stale branch. Prove any denied read genuinely fails under the invoking account; missing or malformed metadata is not an I/O failure.

## C05-09

**Situation:** Lock held

**Disposition:** Added. Worth retaining as an independently checked user outcome: Explain that another operation is using this workspace without guessing its identity or suggesting deletion of a lock file.

### Starting point

Use LOCK while another live mutation owns this workspace's lock.

Fixture: `ROUTES`. fixture recipe; not instantiated.

### Steps

```text
open-forge index
```

### Expected result

Explain that another operation is using this workspace without guessing its identity or suggesting deletion of a lock file.



### Verification

Prove actual lock ownership, not merely a persistent zero-byte lock file; compare no-write evidence.

**Execution constraint:** Demonstrate live same-workspace lock ownership and release; a lock file alone is insufficient.

## C05-10

**Situation:** Write failed partial

**Disposition:** Added. Worth retaining as an independently checked user outcome: Distinguish changed, unchanged, failed and unstarted work; report recovery truthfully.

### Starting point

Use FAULT-AFTER-EFFECT with two stale lists and a failure after one real list write.

Fixture: `ROUTES`. controlled fixture required.

### Steps

```text
open-forge index
```

### Expected result

Distinguish changed, unchanged, failed and unstarted work; report recovery truthfully.



### Verification

Compare per-list hashes and receipts; never print the successful updated count as though the whole request completed.

**Execution constraint:** Establish a deterministic public process or real filesystem boundary and independently verify before/after effects; no timer-only or mocked-result proof.

## C05-11

**Situation:** Cancelled

**Disposition:** Added. Worth retaining as an independently checked user outcome: Report cancellation and no content changes for this pre-effect case.

### Starting point

Interrupt a sufficiently large index before the first write, with the boundary independently observed.

Fixture: `ROUTES`. fixture recipe; not instantiated.

### Steps

```text
open-forge index
```

### Expected result

Report cancellation and no content changes for this pre-effect case.



### Verification

Verify actual signal delivery and unchanged bytes; a process timeout without the signal is not equivalent evidence.

**Execution constraint:** Establish a deterministic public process or real filesystem boundary and independently verify before/after effects; no timer-only or mocked-result proof.

## C05-S04

**Situation:** Outcome 04

**Disposition:** Deferred; not selected yet. Retained recovery matters, but this generic permutation has no demonstrated recovery-requiring plan or reliable post-success cleanup fault.

**Required before reconsideration:** Prove this exact plan requires recovery and synchronize successful effects with denied cleanup of its positively identified bundle.
