---
open-forge:
  description: "references: scenario collection"
  tags: [Memory, Document, CLI, Scenario, Evergreen]
---

# references: scenario collection

These are reviewed user outcomes. Selection does not mean execution passed. Improved targets remain subject to flow validation before new tests. Deferred cases retain only their identity and reason; they are not adopted specifications.

**Who:** A maintainer using Open Forge through the public CLI, without knowing its implementation or internal state model.

**Goal:** See authored links to or from a source and know which link checks were actually possible.

Existing baseline: [Interface](../../../contracts/references-candidate/interface.md), [Behavior](../../../contracts/references-candidate/behavior.md). Exact interface and output differences must be reconciled before implementation.

See [scenario conventions](../_scenarios.md) and the [complete assessment](../../assessment.md).

## C10-01

**Situation:** Links both

**Disposition:** Added. Worth retaining as an independently checked user outcome: Separate incoming and outgoing authored links, with actionable source locations and no generated-navigation noise.

### Starting point

notes.md has one authored outgoing link and one authored incoming link in another source; generated navigation also links to notes.

Fixture: `LINKS`. fixture recipe; not instantiated.

### Steps

```text
open-forge references guidance/notes
```

### Expected result

Separate incoming and outgoing authored links, with actionable source locations and no generated-navigation noise.



### Verification

Enumerate authored link occurrences independently; exclude generated Entries while preserving authored links outside them.

## C10-02

**Situation:** No authored links

**Disposition:** Added. Worth retaining as an independently checked user outcome: Say no authored incoming or outgoing links exist.

### Starting point

Only generated Entries link to the selected source; it has no authored outgoing links.

Fixture: `LINKS`. fixture recipe; not instantiated.

### Steps

```text
open-forge references guidance/notes
```

### Expected result

Say no authored incoming or outgoing links exist.



### Verification

Verify the distinction between generated navigation and ordinary authored Markdown links.

## C10-03

**Situation:** Out only

**Disposition:** Added. Worth retaining as an independently checked user outcome: Return only outgoing links without pretending incoming links were scanned and found absent.

### Starting point

The source has both incoming and outgoing authored links.

Fixture: `LINKS`. fixture recipe; not instantiated.

### Steps

```text
open-forge references guidance/notes --direction=out
```

### Expected result

Return only outgoing links without pretending incoming links were scanned and found absent.



### Verification

Compare the outgoing occurrence set; incoming counts must follow not-requested semantics.

## C10-04

**Situation:** In only with include

**Disposition:** Added. Worth retaining as an independently checked user outcome: Search the explicit incoming universe and show only its matching occurrences.

### Starting point

Incoming authored links exist in guidance and patterns; the selected source is valid.

Fixture: `LINKS`. fixture recipe; not instantiated.

### Steps

```text
open-forge references guidance/notes --direction=in --include=guidance
```

### Expected result

Search the explicit incoming universe and show only its matching occurrences.



### Verification

Compare the included files and preserve coverage relative to that universe, not the whole workspace.

## C10-05

**Situation:** Broken outgoing

**Disposition:** Added. Worth retaining as an independently checked user outcome: Show the exact broken destination and its location without guessing a replacement.

### Starting point

One authored local link names a missing file or fragment.

Fixture: `LINKS`. fixture recipe; not instantiated.

### Steps

```text
open-forge references guidance/notes --direction=out
```

### Expected result

Show the exact broken destination and its location without guessing a replacement.



### Verification

Check target absence or parsed-fragment absence independently; distinguish these causes.

## C10-06

**Situation:** External outgoing

**Disposition:** Added. Worth retaining as an independently checked user outcome: Show the external link as not checked, not proven valid and not automatically broken.

### Starting point

One authored outgoing HTTPS link is present; no network validation is authorized or required.

Fixture: `LINKS`. fixture recipe; not instantiated.

### Steps

```text
open-forge references guidance/notes --direction=out
```

### Expected result

Show the external link as not checked, not proven valid and not automatically broken.



### Verification

Verify no network request. The current Interface explicitly allows completed status for otherwise complete HTTP/HTTPS outgoing facts; unsupported non-HTTP schemes are a separate case in X22.

## C10-07

**Situation:** Unknown source

**Disposition:** Added. Worth retaining as an independently checked user outcome: Reject the unknown source, not a successful empty link inventory.

### Starting point

No source matches the operand.

Fixture: `LINKS`. fixture recipe; not instantiated.

### Steps

```text
open-forge references guidance/does-not-exist
```

### Expected result

Reject the unknown source, not a successful empty link inventory.



### Verification

Verify no substitute source was selected.

## C10-08

**Situation:** Invalid direction

**Disposition:** Added. Worth retaining as an independently checked user outcome: Explain the invalid direction and leave source content alone.

### Starting point

A valid source is paired with an unsupported direction value.

Fixture: `LINKS`. fixture recipe; not instantiated.

### Steps

```text
open-forge references guidance/notes --direction=sideways
```

### Expected result

Explain the invalid direction and leave source content alone.



### Verification

Verify no coercion to both; distinguish parser rejection from a typed invalid request.

## C10-09

**Situation:** Include with out only

**Disposition:** Improved and added. The selected outgoing source remains unambiguous despite an irrelevant incoming filter.

### Starting point

Supply an incoming-universe filter with an out-only request.

Fixture: `LINKS`. fixture recipe; not instantiated.

### Steps

```text
open-forge references guidance/notes --direction=out --include=guidance
```

### Expected result

**Reviewed target:** Return the requested outgoing references and warn that --include only affects incoming scans and had no effect here. Preserve the actual outgoing source and make no incoming-coverage claim.

### Verification

Observe whether the reviewed target is achieved. Compare affected paths, relevant ownership and source bytes independently of the report. Preserve content outside the requested change. Record current disagreement as a failure or unresolved contract difference, not a new expectation.


## C10-10

**Situation:** Ambiguous source

**Disposition:** Added. Worth retaining as an independently checked user outcome: Show exact alternatives and stop rather than choosing one.

### Starting point

COLLISION supplies two candidates for the requested source ID and no choice-capable terminal.

Fixture: `LINKS`. fixture recipe; not instantiated.

### Steps

```text
open-forge references "$AMBIGUOUS_ID"
```

### Expected result

Show exact alternatives and stop rather than choosing one.



### Verification

Verify deterministic alternatives and no mixed link inventory from unrelated candidates.

## C10-11

**Situation:** Unreadable source

**Disposition:** Added. Worth retaining as an independently checked user outcome: Retain the known link and explain that the unreadable candidate's links were not counted.

### Starting point

One eligible incoming candidate cannot be scanned while another has a known incoming link.

Fixture: `LINKS`. fixture recipe; not instantiated.

### Steps

```text
open-forge references guidance/notes
```

### Expected result

Retain the known link and explain that the unreadable candidate's links were not counted.



### Verification

Compare incoming coverage and the later consequence wording; never treat the missing scan as zero links in a complete universe.

**Execution constraint:** Prove any denied read genuinely fails under the invoking account; missing or malformed metadata is not an I/O failure.

## C10-S07

**Situation:** Outcome 07

**Disposition:** Deferred; not selected yet. No concrete unexpected failure mechanism or reproducible fixture is supplied; a generic outcome row adds no executable user scenario yet.

**Required before reconsideration:** Supply a plausible public/OS failure mechanism and independently verified fixture; do not inject arbitrary private result states.

## C10-S08

**Situation:** Outcome 08

**Disposition:** Deferred; not selected yet. Repeated cancellation outcome without a concrete signal boundary; retain X09 as the shared invariant until a distinct fixture earns coverage.

**Required before reconsideration:** Use a real signal-capable process and deterministic observed boundary; a timeout or invalid argument is not cancellation. Use a genuine capability-appropriate terminal and preserve actual prompt/signal evidence.
