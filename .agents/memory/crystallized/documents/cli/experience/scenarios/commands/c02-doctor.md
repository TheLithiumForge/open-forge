---
open-forge:
  description: "doctor: scenario collection"
  tags: [Memory, Document, CLI, Scenario, Evergreen]
---

# doctor: scenario collection

These are reviewed user outcomes. Selection does not mean execution passed. Improved targets remain subject to flow validation before new tests. Deferred cases retain only their identity and reason; they are not adopted specifications.

**Who:** A maintainer using Open Forge through the public CLI, without knowing its implementation or internal state model.

**Goal:** Understand real problems and their consequences, with an honest account of what was checked.

Existing baseline: [Interface](../../../contracts/doctor/interface.md), [Behavior](../../../contracts/doctor/behavior.md). Exact interface and output differences must be reconciled before implementation.

See [scenario conventions](../_scenarios.md) and the [complete assessment](../../assessment.md).

## C02-01

**Situation:** Healthy

**Disposition:** Added. Worth retaining as an independently checked user outcome: Say no problems were found.

### Starting point

W1 is independently checked and has no diagnostic issue.

Fixture: `W1`. fixture recipe; not instantiated.

### Steps

```text
open-forge doctor
```

### Expected result

Say no problems were found. Do not append a no-files-changed mutation footer.



### Verification

Confirm all applicable diagnostic domains were inspected rather than skipped.

## C02-02

**Situation:** Info only

**Disposition:** Added. Worth retaining as an independently checked user outcome: Distinguish an informational observation from an error.

### Starting point

Remove only usable ownership claims; keep all ordinary content readable and valid.

Fixture: `W1`. fixture recipe; not instantiated.

### Steps

```text
open-forge doctor
```

### Expected result

Distinguish an informational observation from an error. Never reconstruct ownership or rewrite the lock.



### Verification

Check informational severity, successful operation and complete observation coverage; retain unknown ownership distinctly.

## C02-03

**Situation:** Warnings only

**Disposition:** Improved and added. A default warning count without affected sources or a useful action leaves the person unable to act.

### Starting point

Seed the output fixture: two broken authored links, one with possible candidates, with no malformed source or unreadable domain.

Fixture: `W1`. fixture recipe; not instantiated.

### Steps

```text
open-forge doctor
```

### Expected result

**Reviewed target:** Show a concise actionable warning summary in the default view, including the affected source and the useful next action. Richer views may expand occurrences and evidence without changing counts or diagnosis.

### Verification

Observe whether the reviewed target is achieved. Compare affected paths, relevant ownership and source bytes independently of the report. Preserve content outside the requested change. Record current disagreement as a failure or unresolved contract difference, not a new expectation.


## C02-04

**Situation:** Error and warnings

**Disposition:** Added. Worth retaining as an independently checked user outcome: Show the error's subject and action, with correct error and warning counts.

### Starting point

Seed the output fixture: one genuinely malformed frontmatter block plus independent broken authored links; all files remain readable.

Fixture: `W1`. fixture recipe; not instantiated.

### Steps

```text
open-forge doctor
```

### Expected result

Show the error's subject and action, with correct error and warning counts. Diagnostic errors do not themselves mean the doctor process failed.



### Verification

Enumerate known defects independently and check the domain status remains completed-with-warnings when diagnosis finished.

## C02-05

**Situation:** Incomplete

**Disposition:** Added. Worth retaining as an independently checked user outcome: Identify checks that did not finish and retain findings from checks that did.

### Starting point

A source needed to inspect an installed Extension is genuinely unreadable; unrelated diagnostic domains remain testable.

Fixture: `W1`. fixture recipe; not instantiated.

### Steps

```text
open-forge doctor
```

### Expected result

Identify checks that did not finish and retain findings from checks that did. Never say no problems found without qualification.



### Verification

Compare per-domain attempted, finished and unavailable coverage; do not add skipped routes to routes-checked totals.

**Execution constraint:** Prove any denied read genuinely fails under the invoking account; missing or malformed metadata is not an I/O failure.

## C02-06

**Situation:** Blocked workspace

**Disposition:** Added. Worth retaining as an independently checked user outcome: Explain why this workspace cannot be checked, without producing findings for another one.

### Starting point

Select an unsafe or nonexistent workspace explicitly.

Fixture: `W1`. fixture recipe; not instantiated.

### Steps

```text
open-forge doctor --workspace "$MISSING_WS"
```

### Expected result

Explain why this workspace cannot be checked, without producing findings for another one.



### Verification

Verify no parent fallback, installation or repair occurred.

## C02-07

**Situation:** Invalid input

**Disposition:** Added. Worth retaining as an independently checked user outcome: Explain the flag error and the valid help path.

### Starting point

W1 is valid; pass an unsupported flag.

Fixture: `W1`. fixture recipe; not instantiated.

### Steps

```text
open-forge doctor --not-an-option
```

### Expected result

Explain the flag error and the valid help path. A parser failure need not use a domain report.



### Verification

Check no diagnosis or workspace effect occurred and no fake JSON envelope was printed.

## C02-08

**Situation:** Changed extension file

**Disposition:** Added. Worth retaining as an independently checked user outcome: Attribute the difference to the correct Extension and point to its inspection or update operation.

### Starting point

Change exactly one owned toolkit file; retain a readable pinned source for comparison.

Fixture: `W1`. fixture recipe; not instantiated.

### Steps

```text
open-forge doctor
```

### Expected result

Attribute the difference to the correct Extension and point to its inspection or update operation.



### Verification

Verify the modified bytes remain; do not blame the Framework or invent missing source history.

**Execution constraint:** Bind package IDs and bytes to a verified fixture catalogue/version pair; toolkit/base are fixture IDs, not assumed bundled packages.

## C02-09

**Situation:** Stale entries

**Disposition:** Added. Worth retaining as an independently checked user outcome: Diagnose stale navigation, not a fabricated incomplete installation or update.

### Starting point

Add one valid route and leave only the parent's generated list stale.

Fixture: `W1`. fixture recipe; not instantiated.

### Steps

```text
open-forge doctor
```

### Expected result

Diagnose stale navigation, not a fabricated incomplete installation or update.



### Verification

Inspect the actual list difference and verify doctor does not index automatically.

## C02-10

**Situation:** Library drift

**Disposition:** Added. Worth retaining as an independently checked user outcome: Explain the missing Library link and its targeted action without silently recreating it.

### Starting point

In LIB, remove one registered destination link while all source inventories remain readable.

Fixture: `W1`. fixture recipe; not instantiated.

### Steps

```text
open-forge doctor
```

### Expected result

Explain the missing Library link and its targeted action without silently recreating it.



### Verification

Verify the complete source inventory prerequisite before calling drift safe; a missing inventory must become a different scenario.

## C02-11

**Situation:** Recovery bundle

**Disposition:** Added. Worth retaining as an independently checked user outcome: Describe the item and its consequence, with cleanup preview rather than automatic deletion.

### Starting point

RECOVERY contains one verifiable retained item for this workspace.

Fixture: `W1`. fixture recipe; not instantiated.

### Steps

```text
open-forge doctor
```

### Expected result

Describe the item and its consequence, with cleanup preview rather than automatic deletion.



### Verification

Confirm the item persists byte-for-byte and the diagnostic names the real recovery subject.

## C02-S08

**Situation:** Outcome 08

**Disposition:** Deferred; not selected yet. No concrete unexpected failure mechanism or reproducible fixture is supplied; a generic outcome row adds no executable user scenario yet.

**Required before reconsideration:** Supply a plausible public/OS failure mechanism and independently verified fixture; do not inject arbitrary private result states.

## C02-S09

**Situation:** Outcome 09

**Disposition:** Deferred; not selected yet. Repeated cancellation outcome without a concrete signal boundary; retain X09 as the shared invariant until a distinct fixture earns coverage.

**Required before reconsideration:** Use a real signal-capable process and deterministic observed boundary; a timeout or invalid argument is not cancellation. Use a genuine capability-appropriate terminal and preserve actual prompt/signal evidence.
