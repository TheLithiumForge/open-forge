---
open-forge:
  description: "library detach: scenario collection"
  tags: [Memory, Document, CLI, Scenario, Evergreen]
---

# library detach: scenario collection

These are reviewed user outcomes. Selection does not mean execution passed. Improved targets remain subject to flow validation before new tests. Deferred cases retain only their identity and reason; they are not adopted specifications.

**Who:** A maintainer using Open Forge through the public CLI, without knowing its implementation or internal state model.

**Goal:** Stop using a registered Library while keeping its source files and unrelated destination content.

Existing baseline: [Interface](../../../contracts/library/detach/interface.md), [Behavior](../../../contracts/library/detach/behavior.md). Exact interface and output differences must be reconciled before implementation.

See [scenario conventions](../_scenarios.md) and the [complete assessment](../../assessment.md).

## C28-01

**Situation:** Detached

**Disposition:** Added. Worth retaining as an independently checked user outcome: Remove the registered links and team registration, explicitly keeping source content.

### Starting point

team has current registered links and required destination permission.

Fixture: `LIB`. fixture recipe; not instantiated.

### Steps

```text
open-forge library detach team --automatic
```

### Expected result

Remove the registered links and team registration, explicitly keeping source content.



### Verification

Verify raw link removal, untouched source bytes, unrelated sibling preservation and bounded navigation changes.

## C28-02

**Situation:** Detached no links

**Disposition:** Added. Worth retaining as an independently checked user outcome: Remove the registration and say it had no links.

### Starting point

team is a valid registration with an empty registered link set.

Fixture: `LIB`. fixture recipe; not instantiated.

### Steps

```text
open-forge library detach team --automatic
```

### Expected result

Remove the registration and say it had no links.



### Verification

Verify zero physical link deletions and preservation of the empty or populated source tree.

## C28-03

**Situation:** Dry run

**Disposition:** Added. Worth retaining as an independently checked user outcome: Show the proposed link removals without touching links, source or registration.

### Starting point

Use a valid current registration and links.

Fixture: `LIB`. fixture recipe; not instantiated.

### Steps

```text
open-forge library detach team --dry-run
```

### Expected result

Show the proposed link removals without touching links, source or registration.



### Verification

Compare exact entry kinds, raw targets, settings and recovery inventory.

## C28-04

**Situation:** Unknown id

**Disposition:** Improved and added. A known absent registration has a harmless repeat-detach no-op meaning.

### Starting point

Ownership is valid and readable but team is absent.

Fixture: `LIB`. fixture recipe; not instantiated.

### Steps

```text
open-forge library detach team --automatic
```

### Expected result

**Reviewed target:** Say that no team Library is registered and nothing needs detaching. Preserve matching unregistered files and links, and distinguish verified absence from unavailable ownership or malformed ID.

### Verification

Observe whether the reviewed target is achieved. Compare affected paths, relevant ownership and source bytes independently of the report. Preserve content outside the requested change. Record current disagreement as a failure or unresolved contract difference, not a new expectation.


## C28-05

**Situation:** Registered link gone

**Disposition:** Improved and added. An already missing link should not force recreation before detach.

### Starting point

One registered destination link is already absent; the record remains readable.

Fixture: `LIB`. fixture recipe; not instantiated.

### Steps

```text
open-forge library detach team --automatic
```

### Expected result

**Reviewed target:** Report the already absent registered destination, remove other verified unchanged registered links after normal permission/path checks, and release the selected registration. Preserve source content and count absent links separately from physical removals.

### Verification

Observe whether the reviewed target is achieved. Compare affected paths, relevant ownership and source bytes independently of the report. Preserve content outside the requested change. Record current disagreement as a failure or unresolved contract difference, not a new expectation.


## C28-06

**Situation:** Changed occupant

**Disposition:** Improved and added. Detach can retain the replacement user file while releasing known registration and removing other exact links.

### Starting point

One registered destination has been replaced by an ordinary user file.

Fixture: `LIB`. fixture recipe; not instantiated.

### Steps

```text
open-forge library detach team --automatic
```

### Expected result

**Reviewed target:** Preserve the ordinary replacement file byte-for-byte and report it as retained user content. Remove independently verified unchanged registered links and release the selected registration when the registered destinations are safely identified. Do not delete by remembered path, follow unsafe replacements or require backing up the user file as a repair ritual.

### Verification

Observe whether the reviewed target is achieved. Compare affected paths, relevant ownership and source bytes independently of the report. Preserve content outside the requested change. Record current disagreement as a failure or unresolved contract difference, not a new expectation.


## C28-07

**Situation:** Destination protected

**Disposition:** Added. Worth retaining as an independently checked user outcome: Explain the protected destination and make no removal.

### Starting point

A recorded destination now violates a protected-control or unsafe-ancestry boundary.

Fixture: `LIB`. fixture recipe; not instantiated.

### Steps

```text
open-forge library detach team --automatic
```

### Expected result

Explain the protected destination and make no removal.



### Verification

Inspect the path without following unsafe links and verify no force or consent bypass.

## C28-08

**Situation:** Permission required

**Disposition:** Added. Worth retaining as an independently checked user outcome: Explain that detaching still requires destination permission.

### Starting point

An external registered destination's permission was revoked.

Fixture: `LIB`. fixture recipe; not instantiated.

### Steps

```text
open-forge library detach team --automatic
```

### Expected result

Explain that detaching still requires destination permission.



### Verification

Verify source independence does not bypass permission and --automatic does not grant it.

## C28-09

**Situation:** No ownership record

**Disposition:** Updated for persistent removal. Record the selected ID without inferring link ownership.

### Starting point

The ownership lock is absent but source and exact-looking links remain.

Fixture: `LIB`. fixture recipe; ownership state must be independently verified.

### Steps

```text
open-forge library detach team --automatic
```

### Expected result

Record `team` in `removedLibraries` and complete the settings-only removal. The
absent lock supplies no link ownership: preserve every source and destination,
including exact-looking links. Do not create a registration or ownership lock.



### Verification

Independently establish lock absence, the new ID exclusion and unchanged source
and link identities. Repeat and verify a no-op. A separate valid empty lock also
permits this exclusion; neither case authorizes link deletion.


Revision note: Updated for the accepted Task 50 persistent-removal contract.

## C28-10

**Situation:** Lock held

**Disposition:** Added. Worth retaining as an independently checked user outcome: Explain contention and preserve all registered links and source content.

### Starting point

LOCK holds the selected workspace during an otherwise valid detach.

Fixture: `LIB`. fixture recipe; not instantiated.

### Steps

```text
open-forge library detach team --automatic
```

### Expected result

Explain contention and preserve all registered links and source content.



### Verification

Verify live ownership and no partial detach.

**Execution constraint:** Demonstrate live same-workspace lock ownership and release; a lock file alone is insufficient.

## C28-11

**Situation:** Record invalid

**Disposition:** Added. Preserve all state when ownership cannot be interpreted.

### Starting point

The lock is malformed or uninterpretable, with no safely selected team claim.

Fixture: `LIB`. fixture recipe; ownership state must be independently verified.

### Steps

```text
open-forge library detach team --automatic
```

### Expected result

Report the unusable ownership boundary, select no claim and perform no effects.
Preserve content, links, settings and the malformed lock. Do not treat unknown
ownership as the known-empty state of a proven-absent lock.



### Verification

Independently establish the malformed record and unchanged workspace. Verify no
registration, link deletion or removal exclusion was fabricated. A separate
valid-record unregistered-ID case permits only its settings exclusion.


Revision note: Updated for the accepted Task 50 persistent-removal contract.

## C28-12

**Situation:** Write failed partial

**Disposition:** Added. Worth retaining as an independently checked user outcome: State which links were removed and which remain, and whether registration publication finished.

### Starting point

Fail after removing one registered link in a multi-link detach.

Fixture: `LIB`. fixture recipe; not instantiated.

### Steps

```text
open-forge library detach team --automatic
```

### Expected result

State which links were removed and which remain, and whether registration publication finished.



### Verification

Compare actual links, source preservation and recovery; do not claim fully detached if registration or links remain.

**Execution constraint:** Establish a deterministic public process or real filesystem boundary and independently verify before/after effects; no timer-only or mocked-result proof.

## C28-13

**Situation:** Cancelled

**Disposition:** Added. Worth retaining as an independently checked user outcome: Say detach was cancelled and keep both links and source files.

### Starting point

Reject final detach confirmation in a real terminal before effects.

Fixture: `LIB`. fixture recipe; not instantiated.

### Steps

```text
open-forge library detach team
```

### Expected result

Say detach was cancelled and keep both links and source files.



### Verification

Record the real prompt interaction and compare all relevant bytes and link identities.

**Execution constraint:** Use a genuine capability-appropriate terminal and preserve actual prompt/signal evidence.

## C28-S05

**Situation:** Outcome 05

**Disposition:** Deferred; not selected yet. Retained recovery matters, but this generic permutation has no demonstrated recovery-requiring plan or reliable post-success cleanup fault.

**Required before reconsideration:** Prove this exact plan requires recovery and synchronize successful effects with denied cleanup of its positively identified bundle.
