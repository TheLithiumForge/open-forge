---
open-forge:
  description: "library inspect: scenario collection"
  tags: [Memory, Document, CLI, Scenario, Evergreen]
---

# library inspect: scenario collection

These are reviewed user outcomes. Selection does not mean execution passed. Improved targets remain subject to flow validation before new tests. Deferred cases retain only their identity and reason; they are not adopted specifications.

**Who:** A maintainer using Open Forge through the public CLI, without knowing its implementation or internal state model.

**Goal:** Compare one registered Library with its current source before deciding to synchronize it.

Existing baseline: [Interface](../../../contracts/library/inspect/interface.md), [Behavior](../../../contracts/library/inspect/behavior.md). Exact interface and output differences must be reconciled before implementation.

See [scenario conventions](../_scenarios.md) and the [complete assessment](../../assessment.md).

## C25-01

**Situation:** Current

**Disposition:** Added. Worth retaining as an independently checked user outcome: Say team is current and identify the source, destination and actual linked-file count.

### Starting point

Registration, complete source inventory and destination links agree.

Fixture: `LIB`. fixture recipe; not instantiated.

### Steps

```text
open-forge library inspect team
```

### Expected result

Say team is current and identify the source, destination and actual linked-file count.



### Verification

Compare source-relative membership and exact relative link targets independently.

## C25-02

**Situation:** Added source files

**Disposition:** Added. Worth retaining as an independently checked user outcome: Show the new source member that needs a link, without creating it.

### Starting point

Add one eligible ordinary file under the registered source, without syncing.

Fixture: `LIB`. fixture recipe; not instantiated.

### Steps

```text
open-forge library inspect team
```

### Expected result

Show the new source member that needs a link, without creating it.



### Verification

Verify destination absence and preserve the source file.

## C25-03

**Situation:** Retired source files

**Disposition:** Added. Worth retaining as an independently checked user outcome: Show the retired member and the corresponding sync difference.

### Starting point

Remove one source member, leaving its registered destination link in place.

Fixture: `LIB`. fixture recipe; not instantiated.

### Steps

```text
open-forge library inspect team
```

### Expected result

Show the retired member and the corresponding sync difference.



### Verification

Distinguish a dangling existing link from a missing destination; no removal during inspection.

## C25-04

**Situation:** Missing links

**Disposition:** Added. Worth retaining as an independently checked user outcome: Explain that the registered link needs restoration.

### Starting point

Remove one registered destination link while keeping a complete readable source.

Fixture: `LIB`. fixture recipe; not instantiated.

### Steps

```text
open-forge library inspect team
```

### Expected result

Explain that the registered link needs restoration.



### Verification

Verify source membership and actual absent destination independently.

## C25-05

**Situation:** Changed links

**Disposition:** Added. Worth retaining as an independently checked user outcome: Identify the changed destination without implying sync may overwrite it automatically.

### Starting point

Replace a registered link with an ordinary user file.

Fixture: `LIB`. fixture recipe; not instantiated.

### Steps

```text
open-forge library inspect team
```

### Expected result

Identify the changed destination without implying sync may overwrite it automatically.



### Verification

Compare entry kind and user bytes; link drift and safe automatic repair are separate questions.

## C25-06

**Situation:** Empty source

**Disposition:** Added. Worth retaining as an independently checked user outcome: Say the Library is current and has no eligible files, not that its source could not be read.

### Starting point

The registered source has a complete empty eligible inventory and no links are registered.

Fixture: `LIB`. fixture recipe; not instantiated.

### Steps

```text
open-forge library inspect team
```

### Expected result

Say the Library is current and has no eligible files, not that its source could not be read.



### Verification

Prove complete empty enumeration rather than missing/denied source.

## C25-07

**Situation:** Source unreadable

**Disposition:** Added. Worth retaining as an independently checked user outcome: Say inspection could not finish and do not infer additions or retirements from a partial inventory.

### Starting point

A required source directory or eligible file is unreadable.

Fixture: `LIB`. fixture recipe; not instantiated.

### Steps

```text
open-forge library inspect team
```

### Expected result

Say inspection could not finish and do not infer additions or retirements from a partial inventory.



### Verification

Compare known partial facts and unknown membership; no zero-file current claim.

**Execution constraint:** Prove any denied read genuinely fails under the invoking account; missing or malformed metadata is not an I/O failure.

## C25-08

**Situation:** Record invalid

**Disposition:** Added. Worth retaining as an independently checked user outcome: Report a completed ownership observation, select no claim, and keep unavailable counts unknown.

### Starting point

The generated ownership lock is malformed or uninterpretable.

Fixture: `LIB`. fixture recipe; ownership state must be independently verified.

### Steps

```text
open-forge library inspect team
```

### Expected result

Report a completed ownership observation, select no claim, and keep unavailable counts unknown. Distinguish a missing, unreadable or unusable record from a valid empty record; do not invent an unknown-ID error. Preserve every file.



### Verification

Independently establish the record state and unchanged workspace. Verify no source inventory or selected Library was fabricated; a separate valid-record unknown-ID case must remain invalid-input.


Revision note: Reconciled to the explicit forgiving-ownership behavior in the supplied contract, not the obsolete capture status.

## C25-09

**Situation:** Unknown id

**Disposition:** Added. Worth retaining as an independently checked user outcome: Say the requested Library ID is unknown.

### Starting point

Ownership is readable and valid but contains no team registration.

Fixture: `LIB`. fixture recipe; not instantiated.

### Steps

```text
open-forge library inspect team
```

### Expected result

Say the requested Library ID is unknown.



### Verification

Verify that this is genuinely known absence, unlike the unusable-record case.

## C25-10

**Situation:** No ownership record

**Disposition:** Added. Worth retaining as an independently checked user outcome: Report a completed ownership observation, select no claim, and keep unavailable counts unknown.

### Starting point

The lock is absent while similarly named source and destination files may exist.

Fixture: `LIB`. review current ownership observation; fixture not instantiated.

### Steps

```text
open-forge library inspect team
```

### Expected result

Report a completed ownership observation, select no claim, and keep unavailable counts unknown. Distinguish a missing, unreadable or unusable record from a valid empty record; do not invent an unknown-ID error. Preserve every file.



### Verification

Independently establish the record state and unchanged workspace. Verify no source inventory or selected Library was fabricated; a separate valid-record unknown-ID case must remain invalid-input.


Revision note: Reconciled to the explicit forgiving-ownership behavior in the supplied contract, not the obsolete capture status.

## C25-11

**Situation:** Invalid id

**Disposition:** Added. Worth retaining as an independently checked user outcome: Explain the invalid ID without normalizing it into another registration.

### Starting point

Supply an ID outside the stable lowercase grammar.

Fixture: `LIB`. fixture recipe; not instantiated.

### Steps

```text
open-forge library inspect Bad_ID
```

### Expected result

Explain the invalid ID without normalizing it into another registration.



### Verification

Verify no source selection and no mutation.

## C25-12

**Situation:** Blocked mapping

**Disposition:** Added. Worth retaining as an independently checked user outcome: Name the blocked mapping without following an unsafe source or destination.

### Starting point

A readable registration points through an unsafe or conflicting destination mapping.

Fixture: `LIB`. fixture recipe; not instantiated.

### Steps

```text
open-forge library inspect team
```

### Expected result

Name the blocked mapping without following an unsafe source or destination.



### Verification

Verify no arbitrary mapping correction or guessed link target.

## C25-S08

**Situation:** Outcome 08

**Disposition:** Deferred; not selected yet. No concrete unexpected failure mechanism or reproducible fixture is supplied; a generic outcome row adds no executable user scenario yet.

**Required before reconsideration:** Supply a plausible public/OS failure mechanism and independently verified fixture; do not inject arbitrary private result states.

## C25-S09

**Situation:** Outcome 09

**Disposition:** Deferred; not selected yet. Repeated cancellation outcome without a concrete signal boundary; retain X09 as the shared invariant until a distinct fixture earns coverage.

**Required before reconsideration:** Use a real signal-capable process and deterministic observed boundary; a timeout or invalid argument is not cancellation. Use a genuine capability-appropriate terminal and preserve actual prompt/signal evidence.
