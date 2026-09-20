---
open-forge:
  description: "library list: scenario collection"
  tags: [Memory, Document, CLI, Scenario, Evergreen]
---

# library list: scenario collection

These are reviewed user outcomes. Selection does not mean execution passed. Improved targets remain subject to flow validation before new tests. Deferred cases retain only their identity and reason; they are not adopted specifications.

**Who:** A maintainer using Open Forge through the public CLI, without knowing its implementation or internal state model.

**Goal:** See registered shared sources and whether their destination links still look usable.

Existing baseline: [Interface](../../../contracts/library/list/interface.md), [Behavior](../../../contracts/library/list/behavior.md). Exact interface and output differences must be reconciled before implementation.

See [scenario conventions](../_scenarios.md) and the [complete assessment](../../assessment.md).

## C24-01

**Situation:** None registered

**Disposition:** Added. Worth retaining as an independently checked user outcome: Say no Libraries are registered without confusing it with unavailable ownership.

### Starting point

The ownership lock is readable and its Library registration set is known empty.

Fixture: `LIB`. fixture recipe; ownership state must be independently verified.

### Steps

```text
open-forge library list
```

### Expected result

Say no Libraries are registered without confusing it with unavailable ownership.



### Verification

Verify the known empty registration set; a next instruction may request missing inputs but must not pretend placeholders are runnable.

## C24-02

**Situation:** One current

**Disposition:** Added. Worth retaining as an independently checked user outcome: Show the Library identity, source and destination in a concise row.

### Starting point

LIB has one verified registration and all expected relative file links are current.

Fixture: `LIB`. fixture recipe; not instantiated.

### Steps

```text
open-forge library list
```

### Expected result

Show the Library identity, source and destination in a concise row.



### Verification

Compare registered paths and actual link targets without treating symlink contents as copied files.

## C24-03

**Situation:** Link missing

**Disposition:** Added. Worth retaining as an independently checked user outcome: Name the missing link and specific Library action.

### Starting point

Remove one registered destination link without touching its source.

Fixture: `LIB`. fixture recipe; not instantiated.

### Steps

```text
open-forge library list
```

### Expected result

Name the missing link and specific Library action.



### Verification

Compare directory entries and registration; do not call the source missing.

## C24-04

**Situation:** Link changed

**Disposition:** Added. Worth retaining as an independently checked user outcome: Explain the changed destination and preserve the user's file.

### Starting point

Replace one registered symlink with an ordinary file containing user edits.

Fixture: `LIB`. fixture recipe; not instantiated.

### Steps

```text
open-forge library list
```

### Expected result

Explain the changed destination and preserve the user's file.



### Verification

Inspect entry kind and bytes without following an unsafe replacement; no automatic overwrite.

## C24-05

**Situation:** No ownership record

**Disposition:** Added. Worth retaining as an independently checked user outcome: Report a completed ownership observation, select no claim, and keep unavailable counts unknown.

### Starting point

The ownership lock is absent while files and links may still exist.

Fixture: `LIB`. fixture recipe; ownership state must be independently verified.

### Steps

```text
open-forge library list
```

### Expected result

Report a completed ownership observation, select no claim, and keep unavailable counts unknown. Distinguish a missing, unreadable or unusable record from a valid empty record; do not invent an unknown-ID error. Preserve every file.



### Verification

Independently establish the record state and unchanged workspace. Verify no source inventory or selected Library was fabricated; a separate valid-record unknown-ID case must remain invalid-input.


Revision note: Reconciled to the explicit forgiving-ownership behavior in the supplied contract, not the obsolete capture status.

## C24-06

**Situation:** Source folder missing

**Disposition:** Added. Worth retaining as an independently checked user outcome: Keep the registered identity and describe missing source availability.

### Starting point

Registration and destination links exist, but the recorded source root has been removed.

Fixture: `LIB`. fixture recipe; not instantiated.

### Steps

```text
open-forge library list
```

### Expected result

Keep the registered identity and describe missing source availability.



### Verification

Check dangling links separately from absent destination links; do not claim a complete source inventory was scanned by list.

## C24-07

**Situation:** Record invalid

**Disposition:** Added. Worth retaining as an independently checked user outcome: Report a completed ownership observation, select no claim, and keep unavailable counts unknown.

### Starting point

The ownership lock is readable but malformed or uninterpretable, with otherwise safe workspace files.

Fixture: `LIB`. fixture recipe; ownership state must be independently verified.

### Steps

```text
open-forge library list
```

### Expected result

Report a completed ownership observation, select no claim, and keep unavailable counts unknown. Distinguish a missing, unreadable or unusable record from a valid empty record; do not invent an unknown-ID error. Preserve every file.



### Verification

Independently establish the record state and unchanged workspace. Verify no source inventory or selected Library was fabricated; a separate valid-record unknown-ID case must remain invalid-input.


Revision note: Reconciled to the explicit forgiving-ownership behavior in the supplied contract, not the obsolete capture status.

## C24-08

**Situation:** Record unreadable

**Disposition:** Added. Worth retaining as an independently checked user outcome: Report a completed ownership observation, select no claim, and keep unavailable counts unknown.

### Starting point

The ownership lock exists but cannot be read under the invoking account.

Fixture: `LIB`. fixture recipe; ownership state must be independently verified.

### Steps

```text
open-forge library list
```

### Expected result

Report a completed ownership observation, select no claim, and keep unavailable counts unknown. Distinguish a missing, unreadable or unusable record from a valid empty record; do not invent an unknown-ID error. Preserve every file.



### Verification

Independently establish the record state and unchanged workspace. Verify no source inventory or selected Library was fabricated; a separate valid-record unknown-ID case must remain invalid-input.


Revision note: Reconciled to the explicit forgiving-ownership behavior in the supplied contract, not the obsolete capture status.



**Execution constraint:** Prove any denied read genuinely fails under the invoking account; missing or malformed metadata is not an I/O failure.

## C24-09

**Situation:** Link blocked

**Disposition:** Added. Worth retaining as an independently checked user outcome: Explain the exact unsafe link check and do not follow the boundary.

### Starting point

A registered destination now crosses an unsafe identity or linked-ancestry boundary.

Fixture: `LIB`. fixture recipe; not instantiated.

### Steps

```text
open-forge library list
```

### Expected result

Explain the exact unsafe link check and do not follow the boundary.



### Verification

Verify no outside read or mutation and no current-link claim from unobserved facts.

## C24-10

**Situation:** Invalid input

**Disposition:** Added. Worth retaining as an independently checked user outcome: Explain the invalid form instead of silently switching to inspect.

### Starting point

Pass an unsupported positional ID to this operand-free command.

Fixture: `LIB`. fixture recipe; not instantiated.

### Steps

```text
open-forge library list team
```

### Expected result

Explain the invalid form instead of silently switching to inspect.



### Verification

Check parser-level exit and no workspace mutation.

## C24-S08

**Situation:** Outcome 08

**Disposition:** Deferred; not selected yet. No concrete unexpected failure mechanism or reproducible fixture is supplied; a generic outcome row adds no executable user scenario yet.

**Required before reconsideration:** Supply a plausible public/OS failure mechanism and independently verified fixture; do not inject arbitrary private result states.

## C24-S09

**Situation:** Outcome 09

**Disposition:** Deferred; not selected yet. Repeated cancellation outcome without a concrete signal boundary; retain X09 as the shared invariant until a distinct fixture earns coverage.

**Required before reconsideration:** Use a real signal-capable process and deterministic observed boundary; a timeout or invalid argument is not cancellation. Use a genuine capability-appropriate terminal and preserve actual prompt/signal evidence.
