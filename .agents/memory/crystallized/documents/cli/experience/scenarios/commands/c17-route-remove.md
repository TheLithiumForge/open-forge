---
open-forge:
  description: "route remove: scenario collection"
  tags: [Memory, Document, CLI, Scenario, Evergreen]
---

# route remove: scenario collection

These are reviewed user outcomes. Selection does not mean execution passed. Improved targets remain subject to flow validation before new tests. Deferred cases retain only their identity and reason; they are not adopted specifications.

**Who:** A maintainer using Open Forge through the public CLI, without knowing its implementation or internal state model.

**Goal:** Remove a selected route without breaking authored meaning or deleting unrelated content.

Existing baseline: [Interface](../../../contracts/route/remove/interface.md), [Behavior](../../../contracts/route/remove/behavior.md). Exact interface and output differences must be reconciled before implementation.

See [scenario conventions](../_scenarios.md) and the [complete assessment](../../assessment.md).

## C17-01

**Situation:** Leaf removed

**Disposition:** Added. Worth retaining as an independently checked user outcome: Remove the leaf and its navigation entry, naming the actual removed source.

### Starting point

notes.md is an unowned routed leaf with no authored incoming link and no protected boundary.

Fixture: `LINKS`. fixture recipe; not instantiated.

### Steps

```text
open-forge route remove guidance/notes --automatic
```

### Expected result

Remove the leaf and its navigation entry, naming the actual removed source.



### Verification

Verify target absence, bounded parent edit and preservation of all neighboring files.

## C17-02

**Situation:** Leaf with detached links

**Disposition:** Added. Worth retaining as an independently checked user outcome: Remove the source and detach safe link markup while preserving Team notes as authored text.

### Starting point

A safe authored link refers to notes.md with visible label Team notes.

Fixture: `LINKS`. fixture recipe; not instantiated.

### Steps

```text
open-forge route remove guidance/notes --automatic
```

### Expected result

Remove the source and detach safe link markup while preserving Team notes as authored text.



### Verification

Compare the exact Markdown span and visible label; do not delete the entire sentence.

## C17-03

**Situation:** Category removed

**Disposition:** Added. Worth retaining as an independently checked user outcome: Remove the selected category, list the actual effects and handle incoming references under the safe-detach rules.

### Starting point

An unowned category contains several eligible descendants and a valid overwrite pair.

Fixture: `LINKS`. fixture recipe; not instantiated.

### Steps

```text
open-forge route remove guidance/team --automatic
```

### Expected result

Remove the selected category, list the actual effects and handle incoming references under the safe-detach rules.



### Verification

Enumerate every removed path and preserved neighbor; a category summary cannot replace the effect inventory when paths are required.

## C17-04

**Situation:** Dry run

**Disposition:** Added. Worth retaining as an independently checked user outcome: Show the removal and link changes in future tense, ending with No files were changed.

### Starting point

Use the category fixture without applying it.

Fixture: `LINKS`. fixture recipe; not instantiated.

### Steps

```text
open-forge route remove guidance/team --dry-run
```

### Expected result

Show the removal and link changes in future tense, ending with No files were changed.



### Verification

Verify source tree, authored links and parent navigation are byte-identical.

## C17-05

**Situation:** Source not found

**Disposition:** Improved and added. A verified already-absent removal target should be a clear no-op.

### Starting point

No route matches the supplied reference.

Fixture: `LINKS`. fixture recipe; not instantiated.

### Steps

```text
open-forge route remove guidance/does-not-exist --automatic
```

### Expected result

**Reviewed target:** Say the exact requested source is already absent and nothing was removed. Preserve unrelated and similarly named files. Keep an unresolved or ambiguous identity distinct from verified absence.

### Verification

Observe whether the reviewed target is achieved. Compare affected paths, relevant ownership and source bytes independently of the report. Preserve content outside the requested change. Record current disagreement as a failure or unresolved contract difference, not a new expectation.


## C17-06

**Situation:** Managed source

**Disposition:** Added. Remove the explicitly selected managed route, record its persistent exclusion, and release its content claims.

### Starting point

The selected route has a valid Framework or Extension owner.

Fixture: `LINKS`. fixture recipe; not instantiated.

### Steps

```text
open-forge route remove "$MANAGED_SOURCE" --automatic
```

### Expected result

Remove the explicitly selected managed route, record its persistent exclusion, and release its content claims.



### Verification

Verify the selected route is absent, matching claims are released, unrelated claims and package registrations remain, and Update respects the recorded exclusion.

## C17-07

**Situation:** Unsafe link detach

**Disposition:** Deferred; not selected yet. Protecting link meaning matters, but the case explicitly lacks the offending Markdown form; a capture does not establish the fixture.

**Required before reconsideration:** Supply a small real Markdown occurrence whose supported detachment cannot preserve authored meaning.

## C17-08

**Situation:** Ambiguous source prompt

**Disposition:** Added. Worth retaining as an independently checked user outcome: Remove only the explicitly selected source after the required confirmation.

### Starting point

COLLISION is queried in a real terminal; select one exact source and approve removal.

Fixture: `LINKS`. fixture recipe; not instantiated.

### Steps

```text
open-forge route remove "$AMBIGUOUS_ID"
```

### Expected result

Remove only the explicitly selected source after the required confirmation.



### Verification

Record selection and confirmation separately; verify the other candidate survives.

**Execution constraint:** Use a genuine capability-appropriate terminal and preserve actual prompt/signal evidence.

## C17-09

**Situation:** Reference scan incomplete

**Disposition:** Added. Worth retaining as an independently checked user outcome: Explain why safe detachment cannot be checked and make no removal.

### Starting point

A required reference-scan source is genuinely unreadable.

Fixture: `LINKS`. fixture recipe; not instantiated.

### Steps

```text
open-forge route remove guidance/notes --automatic
```

### Expected result

Explain why safe detachment cannot be checked and make no removal.



### Verification

Compare all source and reference paths; unknown links must not be represented as a complete zero-link scan.

**Execution constraint:** Prove any denied read genuinely fails under the invoking account; missing or malformed metadata is not an I/O failure.

## C17-10

**Situation:** Lock held

**Disposition:** Added. Worth retaining as an independently checked user outcome: Explain contention and preserve every source and link.

### Starting point

LOCK is held for the selected workspace.

Fixture: `LINKS`. fixture recipe; not instantiated.

### Steps

```text
open-forge route remove guidance/notes --automatic
```

### Expected result

Explain contention and preserve every source and link.



### Verification

Distinguish a held lock from a harmless persistent lock file.

**Execution constraint:** Demonstrate live same-workspace lock ownership and release; a lock file alone is insufficient.

## C17-11

**Situation:** Write failed partial

**Disposition:** Added. Worth retaining as an independently checked user outcome: Say removal stopped after the completed effects and identify remaining work and real recovery.

### Starting point

Fail after one actual removal or detachment effect in a multi-effect operation.

Fixture: `LINKS`. fixture recipe; not instantiated.

### Steps

```text
open-forge route remove guidance/notes --automatic
```

### Expected result

Say removal stopped after the completed effects and identify remaining work and real recovery.



### Verification

Compare every removed path and rewritten span; do not claim nothing changed.

**Execution constraint:** Establish a deterministic public process or real filesystem boundary and independently verify before/after effects; no timer-only or mocked-result proof.

## C17-12

**Situation:** Cancelled

**Disposition:** Added. Worth retaining as an independently checked user outcome: Say removal was cancelled and preserve the source and links.

### Starting point

In a real terminal, reject final removal confirmation.

Fixture: `LINKS`. fixture recipe; not instantiated.

### Steps

```text
open-forge route remove guidance/notes
```

### Expected result

Say removal was cancelled and preserve the source and links.



### Verification

Verify both the chosen source and unrelated candidates remain unchanged.

**Execution constraint:** Use a genuine capability-appropriate terminal and preserve actual prompt/signal evidence.

## C17-S04

**Situation:** Outcome 04

**Disposition:** Deferred; not selected yet. Retained recovery matters, but this generic permutation has no demonstrated recovery-requiring plan or reliable post-success cleanup fault.

**Required before reconsideration:** Prove this exact plan requires recovery and synchronize successful effects with denied cleanup of its positively identified bundle.

## C17-13

**Situation:** Unusable ownership prevents safe route removal

**Disposition:** Improved and added. The removal case accidentally executes route move.

### Starting point

The subject is a readable ordinary route. Its required ownership record is unreadable or uninterpretable, while paths and reference files are otherwise safe and readable. A proven-absent record is a separate known-empty case.

### Steps

1. open-forge route remove guidance/notes --automatic

### Expected result

**Reviewed target:** When required ownership is unavailable, report that boundary, remove nothing, and preserve source, reference, navigation, settings and ownership bytes. Do not infer claims from matching files. A proven-absent lock permits ordinary route removal with persistent exclusions; known Framework and Extension claims are released after verified content removal.

### Verification

Observe whether the reviewed target is achieved. Compare affected paths, relevant ownership and source bytes independently of the report. Preserve content outside the requested change. Record current disagreement as a failure or unresolved contract difference, not a new expectation.
