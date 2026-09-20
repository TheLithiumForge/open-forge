---
open-forge:
  description: "install: scenario collection"
  tags: [Memory, Document, CLI, Scenario, Evergreen]
---

# install: scenario collection

These are reviewed user outcomes. Selection does not mean execution passed. Improved targets remain subject to flow validation before new tests. Deferred cases retain only their identity and reason; they are not adopted specifications.

**Who:** A maintainer using Open Forge through the public CLI, without knowing its implementation or internal state model.

**Goal:** Add Open Forge without losing existing project content or making me interpret internal state.

Existing baseline: [Interface](../../../contracts/install/interface.md), [Behavior](../../../contracts/install/behavior.md). Exact interface and output differences must be reconciled before implementation.

See [scenario conventions](../_scenarios.md) and the [complete assessment](../../assessment.md).

## C03-01

**Situation:** Fresh directory

**Disposition:** Added. Worth retaining as an independently checked user outcome: Install the Framework, identify created host sections and summarize the managed content with truthful counts.

### Starting point

W0 contains an unrelated README and no Open Forge installation.

Fixture: `W0`. fixture recipe; not instantiated.

### Steps

```text
open-forge install --automatic
```

### Expected result

Install the Framework, identify created host sections and summarize the managed content with truthful counts.



### Verification

Compare the full pre/post tree, including hidden files and root directories; use COUNT rules, not the historical 21/20 literals.

## C03-02

**Situation:** Fresh directory dry run

**Disposition:** Added. Worth retaining as an independently checked user outcome: Show what would be installed and finish with No files were changed.

### Starting point

Use an untouched copy of W0.

Fixture: `W0`. fixture recipe; not instantiated.

### Steps

```text
open-forge install --dry-run
```

### Expected result

Show what would be installed and finish with No files were changed.



### Verification

Compare bytes, directory entries, settings and external recovery inventory before and after; preview must not create consent state.

## C03-03

**Situation:** Already installed

**Disposition:** Added. Worth retaining as an independently checked user outcome: Say Open Forge is already installed and current, with nothing to do.

### Starting point

W1 exactly matches the running bundle and its verified ownership receipts.

Fixture: `W0`. fixture recipe; not instantiated.

### Steps

```text
open-forge install --automatic
```

### Expected result

Say Open Forge is already installed and current, with nothing to do.



### Verification

Hash all files and inventory external recovery; a repeated install must not churn navigation or receipts.

## C03-04

**Situation:** Existing agents md

**Disposition:** Added. Worth retaining as an independently checked user outcome: Keep the authored instructions and describe the actual host-file treatment.

### Starting point

W0 has a hand-authored AGENTS.md with distinctive text and no conflicting managed region.

Fixture: `W0`. fixture recipe; not instantiated.

### Steps

```text
open-forge install --automatic
```

### Expected result

Keep the authored instructions and describe the actual host-file treatment. Do not report the whole file as newly created.



### Verification

Compare authored bytes and any inserted bounded region separately; count files below .agents independently of host files and the root directory.




## C03-05

**Situation:** Occupied without force

**Disposition:** Added. Worth retaining as an independently checked user outcome: Name the exact conflict before any installation effects and explain the available deliberate choice.

### Starting point

One install target is occupied by an eligible unowned file with different bytes; other targets are absent.

Fixture: `W0`. fixture recipe; not instantiated.

### Steps

```text
open-forge install --automatic
```

### Expected result

Name the exact conflict before any installation effects and explain the available deliberate choice.



### Verification

Verify every target and unrelated file remains unchanged; automatic confirmation is not force authority.

## C03-06

**Situation:** Occupied with force

**Disposition:** Added. Worth retaining as an independently checked user outcome: Replace only eligible conflicts and report which existing files were replaced.

### Starting point

Reuse the eligible unowned-occupant fixture, after reviewing the replacement preview.

Fixture: `W0`. fixture recipe; not instantiated.

### Steps

```text
open-forge install --force --automatic
```

### Expected result

Replace only eligible conflicts and report which existing files were replaced.



### Verification

Inspect before/after bytes and recovery evidence; force must not bypass reserved paths or another owner's claims.

## C03-07

**Situation:** Changed framework file

**Disposition:** Added. Worth retaining as an independently checked user outcome: Direct the user to update instead of pretending this is a fresh install conflict.

### Starting point

W1 contains one semantically changed Framework-owned file.

Fixture: `W0`. fixture recipe; not instantiated.

### Steps

```text
open-forge install --automatic
```

### Expected result

Direct the user to update instead of pretending this is a fresh install conflict.



### Verification

Verify the edit remains intact and install does not widen force or ownership authority.

## C03-08

**Situation:** Confirmation unavailable

**Disposition:** Added. Worth retaining as an independently checked user outcome: Explain that confirmation is required without waiting forever or printing an unanswered terminal prompt.

### Starting point

Run from a redirected, noninteractive session with an actual installation plan and no automatic flag.

Fixture: `W0`. fixture recipe; not instantiated.

### Steps

```text
open-forge install
```

### Expected result

Explain that confirmation is required without waiting forever or printing an unanswered terminal prompt.



### Verification

Capture stdin mode, stderr, exit and unchanged filesystem; compare against the valid --automatic counterpart.

## C03-09

**Situation:** Recovery store unavailable

**Disposition:** Added. Worth retaining as an independently checked user outcome: Explain the required recovery limitation and make no target changes.

### Starting point

Use a plan that genuinely needs prior-byte recovery and make its required recovery store unreadable.

Fixture: `W0`. fixture recipe; not instantiated.

### Steps

```text
open-forge install --automatic
```

### Expected result

Explain the required recovery limitation and make no target changes. Do not substitute a corrupt unrelated old log.



### Verification

Prove the recovery dependency is actually needed, access really fails, and no target was touched.

**Execution constraint:** Prove any denied read genuinely fails under the invoking account; missing or malformed metadata is not an I/O failure.

## C03-10

**Situation:** Write failed partial

**Disposition:** Added. Worth retaining as an independently checked user outcome: State that install stopped, distinguish completed and unstarted effects, and name available recovery truthfully.

### Starting point

Use FAULT-AFTER-EFFECT with an externally reproducible write failure after a known completed effect.

Fixture: `W0`. controlled fixture required.

### Steps

```text
open-forge install --automatic
```

### Expected result

State that install stopped, distinguish completed and unstarted effects, and name available recovery truthfully.



### Verification

Compare receipts with actual completed effects; never use a successful-install headline or claim nothing changed.

**Execution constraint:** Establish a deterministic public process or real filesystem boundary and independently verify before/after effects; no timer-only or mocked-result proof.

## C03-11

**Situation:** Cancelled

**Disposition:** Added. Worth retaining as an independently checked user outcome: Say installation was cancelled and nothing was changed.

### Starting point

Use a real terminal and reject final confirmation before any effect.

Fixture: `W0`. fixture recipe; not instantiated.

### Steps

```text
open-forge install
```

### Expected result

Say installation was cancelled and nothing was changed.



### Verification

Verify no target, settings or recovery effect; mid-write cancellation is separately required by X09.

**Execution constraint:** Use a genuine capability-appropriate terminal and preserve actual prompt/signal evidence.

## C03-12

**Situation:** Invalid input

**Disposition:** Added. Worth retaining as an independently checked user outcome: Reject the argument without attempting installation.

### Starting point

W0 is valid; supply an unsupported flag.

Fixture: `W0`. fixture recipe; not instantiated.

### Steps

```text
open-forge install --not-an-option
```

### Expected result

Reject the argument without attempting installation.



### Verification

Check parser-level diagnostics, invalid-input exit and preservation of W0.

## C03-S05

**Situation:** Outcome 05

**Disposition:** Deferred; not selected yet. Retained recovery matters, but this generic permutation has no demonstrated recovery-requiring plan or reliable post-success cleanup fault.

**Required before reconsideration:** Prove this exact plan requires recovery and synchronize successful effects with denied cleanup of its positively identified bundle.

## C03-S10

**Situation:** Outcome 10

**Disposition:** Added. Worth retaining as an independently checked user outcome: Explain the exact unsafe boundary and stop before all target effects.

### Starting point

An intended install target crosses a protected or unsafe path boundary; no unrelated metadata problem is present.

Fixture: `W0`. fixture recipe; not instantiated.

### Steps

```text
open-forge install --automatic
```

### Expected result

Explain the exact unsafe boundary and stop before all target effects.



### Verification

Verify no traversal or modification beyond the safe workspace; force and automatic must not bypass it.
