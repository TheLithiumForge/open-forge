---
open-forge:
  description: "route list: scenario collection"
  tags: [Memory, Document, CLI, Scenario, Evergreen]
---

# route list: scenario collection

These are reviewed user outcomes. Selection does not mean execution passed. Improved targets remain subject to flow validation before new tests. Deferred cases retain only their identity and reason; they are not adopted specifications.

**Who:** A maintainer using Open Forge through the public CLI, without knowing its implementation or internal state model.

**Goal:** Explore available material with names I can use in the next command.

Existing baseline: [Interface](../../../contracts/route/list/interface.md), [Behavior](../../../contracts/route/list/behavior.md). Exact interface and output differences must be reconciled before implementation.

See [scenario conventions](../_scenarios.md) and the [complete assessment](../../assessment.md).

## C11-01

**Situation:** Roots depth 1

**Disposition:** Added. Worth retaining as an independently checked user outcome: Show the default depth-one tree with usable source IDs and only truthful hidden-child hints.

### Starting point

ROUTES contains known roots and direct descendants.

Fixture: `ROUTES`. fixture recipe; not instantiated.

### Steps

```text
open-forge route list
```

### Expected result

Show the default depth-one tree with usable source IDs and only truthful hidden-child hints.



### Verification

Compare displayed depth and IDs against authored topology; run a shown ID through route inspect on a separate identical fixture.

## C11-02

**Situation:** Subtree

**Disposition:** Added. Worth retaining as an independently checked user outcome: List the selected subtree without blending sibling roots into it.

### Starting point

guidance has two children and patterns contains unrelated material.

Fixture: `ROUTES`. fixture recipe; not instantiated.

### Steps

```text
open-forge route list guidance
```

### Expected result

List the selected subtree without blending sibling roots into it.



### Verification

Verify parent/child structure and the exact selected boundary.

## C11-03

**Situation:** Depth all

**Disposition:** Added. Worth retaining as an independently checked user outcome: Show the full selected tree without an arbitrary display cap.

### Starting point

Use a finite nested route tree with several levels.

Fixture: `ROUTES`. fixture recipe; not instantiated.

### Steps

```text
open-forge route list guidance --depth=all
```

### Expected result

Show the full selected tree without an arbitrary display cap.



### Verification

Count all eligible descendants independently and inspect for silent ellipses.

## C11-04

**Situation:** Depth 0

**Disposition:** Added. Worth retaining as an independently checked user outcome: Treat zero as a valid depth and show only the contract-defined subject level.

### Starting point

Select guidance with known children.

Fixture: `ROUTES`. fixture recipe; not instantiated.

### Steps

```text
open-forge route list guidance --depth=0
```

### Expected result

Treat zero as a valid depth and show only the contract-defined subject level.



### Verification

Verify it is not rejected as invalid or coerced to depth one.

## C11-05

**Situation:** Empty subtree

**Disposition:** Added. Worth retaining as an independently checked user outcome: Explain that the selected scope has no child routes, not that its reference is invalid.

### Starting point

guidance/empty is a valid empty routed scope.

Fixture: `ROUTES`. fixture recipe; not instantiated.

### Steps

```text
open-forge route list guidance/empty
```

### Expected result

Explain that the selected scope has no child routes, not that its reference is invalid.



### Verification

Confirm the entrypoint exists and the independently inspected child set is empty.

## C11-06

**Situation:** Unknown source

**Disposition:** Added. Worth retaining as an independently checked user outcome: Name the unknown reference and avoid an empty-tree success.

### Starting point

The requested source is absent.

Fixture: `ROUTES`. fixture recipe; not instantiated.

### Steps

```text
open-forge route list guidance/does-not-exist
```

### Expected result

Name the unknown reference and avoid an empty-tree success.



### Verification

Verify no parent or nearest-match fallback.

## C11-07

**Situation:** Invalid depth

**Disposition:** Added. Worth retaining as an independently checked user outcome: Explain the invalid depth and accepted forms.

### Starting point

A valid source is paired with a negative depth.

Fixture: `ROUTES`. fixture recipe; not instantiated.

### Steps

```text
open-forge route list guidance --depth=-1
```

### Expected result

Explain the invalid depth and accepted forms.



### Verification

Verify depth zero remains valid in its separate case; do not use one rule for both.

## C11-08

**Situation:** Ambiguous source

**Disposition:** Added. Worth retaining as an independently checked user outcome: Explain the ambiguity instead of combining or selecting trees arbitrarily.

### Starting point

COLLISION makes the operand resolve to two sources without an interactive choice.

Fixture: `ROUTES`. fixture recipe; not instantiated.

### Steps

```text
open-forge route list "$AMBIGUOUS_ID"
```

### Expected result

Explain the ambiguity instead of combining or selecting trees arbitrarily.



### Verification

Verify exact alternatives and no invented resolved source.

## C11-09

**Situation:** Metadata missing

**Disposition:** Improved and added. Optional missing description or tags must not be described as an unreadable or invalid route.

### Starting point

One physically eligible readable route has no optional description or tags; its source identity and containing route are unambiguous.

### Steps

```text
open-forge route list guidance
```

### Expected result

**Reviewed target:** List the readable route using its real identity and available authored metadata. Give a concise warning for absent optional metadata, preserve authored bytes, and do not require completion before discovery or reading.

### Verification

Observe whether the reviewed target is achieved. Compare affected paths, relevant ownership and source bytes independently of the report. Preserve content outside the requested change. Record current disagreement as a failure or unresolved contract difference, not a new expectation.


## C11-10

**Situation:** Unreadable entrypoint

**Disposition:** Added. Worth retaining as an independently checked user outcome: Show safe available routes and name the branch whose inspection could not finish.

### Starting point

Deny a selected entrypoint read while another branch remains readable.

Fixture: `ROUTES`. fixture recipe; not instantiated.

### Steps

```text
open-forge route list
```

### Expected result

Show safe available routes and name the branch whose inspection could not finish.



### Verification

Compare displayed partial topology and coverage; no fake empty branch or invented descendant count.

**Execution constraint:** Prove any denied read genuinely fails under the invoking account; missing or malformed metadata is not an I/O failure.

## C11-11

**Situation:** Loader malformed

**Disposition:** Added. Worth retaining as an independently checked user outcome: Explain the loader problem and avoid inventing a usable root graph.

### Starting point

The actual Loader is readable and parsed, but its root declarations are ambiguous. A separate scenario covers a Loader that cannot be parsed at all.

Fixture: `ROUTES`. fixture recipe; not instantiated.

### Steps

```text
open-forge route list
```

### Expected result

Explain the loader problem and avoid inventing a usable root graph.



### Verification

Verify loader bytes remain unchanged and normal child warnings cannot hide this root blocker.

## C11-S07

**Situation:** Outcome 07

**Disposition:** Deferred; not selected yet. No concrete unexpected failure mechanism or reproducible fixture is supplied; a generic outcome row adds no executable user scenario yet.

**Required before reconsideration:** Supply a plausible public/OS failure mechanism and independently verified fixture; do not inject arbitrary private result states.

## C11-S08

**Situation:** Outcome 08

**Disposition:** Deferred; not selected yet. Repeated cancellation outcome without a concrete signal boundary; retain X09 as the shared invariant until a distinct fixture earns coverage.

**Required before reconsideration:** Use a real signal-capable process and deterministic observed boundary; a timeout or invalid argument is not cancellation. Use a genuine capability-appropriate terminal and preserve actual prompt/signal evidence.

## C11-12

**Situation:** Loader cannot establish root declarations

**Disposition:** Added. Worth retaining as an independently checked user outcome: Report incomplete root discovery; do not invent the usual standard roots or call the workspace empty.

### Starting point

The Loader exists and is readable, but its root-declaration syntax cannot be parsed. Preserve its exact malformed bytes. No known fallback roots are allowed.

### Steps

```text
open-forge route list
```

### Expected result

Report incomplete root discovery; do not invent the usual standard roots or call the workspace empty.



### Verification

Verify the Loader is unchanged and that only independently established facts appear. Compare with C11-11, whose parsed declarations are ambiguous.
