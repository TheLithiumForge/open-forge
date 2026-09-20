---
open-forge:
  description: "repair: scenario collection"
  tags: [Memory, Document, CLI, Scenario, Evergreen]
---

# repair: scenario collection

These are reviewed user outcomes. Selection does not mean execution passed. Improved targets remain subject to flow validation before new tests. Deferred cases retain only their identity and reason; they are not adopted specifications.

**Who:** A maintainer using Open Forge through the public CLI, without knowing its implementation or internal state model.

**Goal:** Fix only clear, selected problems without guessing the intended content or hiding the problems left over.

Existing baseline: [Interface](../../../contracts/repair/interface.md), [Behavior](../../../contracts/repair/behavior.md). Exact interface and output differences must be reconciled before implementation.

See [scenario conventions](../_scenarios.md) and the [complete assessment](../../assessment.md).

## C06-01

**Situation:** Nothing to repair

**Disposition:** Added. Worth retaining as an independently checked user outcome: Say nothing needs repair; do not claim a nonzero number of verified fixes.

### Starting point

LINKS is valid and all relevant diagnostic inputs are readable.

Fixture: `LINKS`. fixture recipe; not instantiated.

### Steps

```text
open-forge repair --automatic
```

### Expected result

Say nothing needs repair; do not claim a nonzero number of verified fixes.



### Verification

Verify zero effects, exact bytes and that a real diagnosis ran.

## C06-02

**Situation:** Automatic two links

**Disposition:** Added. Worth retaining as an independently checked user outcome: Apply exactly those two corrections and show source locations and old-to-new destinations.

### Starting point

Seed two source-proven safe-exact link corrections with independent expected destinations.

Fixture: `LINKS`. fixture recipe; not instantiated.

### Steps

```text
open-forge repair --automatic
```

### Expected result

Apply exactly those two corrections and show source locations and old-to-new destinations.



### Verification

Compare each changed Markdown span and preserve labels, formatting and unrelated bytes.

## C06-03

**Situation:** Automatic nothing safe two guided

**Disposition:** Added. Worth retaining as an independently checked user outcome: Apply nothing and explain the two choices still needed.

### Starting point

Two broken links each have more than one plausible destination; neither has a safe-exact correction.

Fixture: `LINKS`. fixture recipe; not instantiated.

### Steps

```text
open-forge repair --automatic
```

### Expected result

Apply nothing and explain the two choices still needed. Do not pick a likely target to make the run green.



### Verification

Verify both link spans unchanged and the remaining-problem count derived from actual unresolved occurrences.

## C06-04

**Situation:** Dry run automatic

**Disposition:** Added. Worth retaining as an independently checked user outcome: Show both proposed relinks without writing them.

### Starting point

Reuse the safe two-link fixture on a fresh copy.

Fixture: `LINKS`. fixture recipe; not instantiated.

### Steps

```text
open-forge repair --automatic --dry-run
```

### Expected result

Show both proposed relinks without writing them.



### Verification

Compare full source bytes; preview counts and actual later effects must agree for unchanged inputs.

## C06-05

**Situation:** Relink one

**Disposition:** Added. Worth retaining as an independently checked user outcome: Repair that exact occurrence and no similarly spelled occurrence elsewhere.

### Starting point

Bind LOCATION to one measured source occurrence, EXPECTED to its exact current destination, and TARGET to the intended contained file.

Fixture: `LINKS`. fixture recipe; not instantiated.

### Steps

```text
open-forge repair --relink "$LOCATION" "$EXPECTED" "$TARGET"
```

### Expected result

Repair that exact occurrence and no similarly spelled occurrence elsewhere.



### Verification

Measure line and Unicode-scalar column independently; verify the old-destination guard and preservation of visible link text.

## C06-06

**Situation:** Relink invalid

**Disposition:** Added. Worth retaining as an independently checked user outcome: Reject the mismatch without changing the link.

### Starting point

The selected occurrence exists, but EXPECTED deliberately differs from its current destination.

Fixture: `LINKS`. fixture recipe; not instantiated.

### Steps

```text
open-forge repair --relink "$LOCATION" "$WRONG_EXPECTED" "$TARGET"
```

### Expected result

Reject the mismatch without changing the link.



### Verification

Compare the exact source slice before/after; do not accept a correction at a nearby occurrence.

## C06-07

**Situation:** Contradictory relinks

**Disposition:** Added. Worth retaining as an independently checked user outcome: Explain the contradictory requests before any repair effect.

### Starting point

Supply the same occurrence twice with incompatible destination choices.

Fixture: `LINKS`. fixture recipe; not instantiated.

### Steps

```text
open-forge repair --relink "$LOCATION" "$EXPECTED" "$TARGET" --relink "$LOCATION" "$EXPECTED" "$OTHER_TARGET"
```

### Expected result

Explain the contradictory requests before any repair effect.



### Verification

Verify no first-wins or last-wins behavior and no partial application.

## C06-08

**Situation:** Non interactive no selection

**Disposition:** Added. Worth retaining as an independently checked user outcome: Explain the missing selection and do not start an invisible wizard.

### Starting point

Redirect the session, with a repairable problem but neither automatic nor an explicit relink request.

Fixture: `LINKS`. fixture recipe; not instantiated.

### Steps

```text
open-forge repair
```

### Expected result

Explain the missing selection and do not start an invisible wizard.



### Verification

Verify blocked exit, no effects and bounded completion without waiting on unavailable input.

## C06-09

**Situation:** Library recovery step

**Disposition:** Deferred; not selected yet. The rare Library recovery step has no concrete supported fixture or selected state; a real user recovery journey must identify it.

**Required before reconsideration:** Supply a current diagnosis-backed recovery recipe with exact registered links and checked effects.

## C06-10

**Situation:** Lock held

**Disposition:** Added. Worth retaining as an independently checked user outcome: Explain the active operation and preserve all selected sources.

### Starting point

LOCK holds this workspace while a real repair plan exists.

Fixture: `LINKS`. fixture recipe; not instantiated.

### Steps

```text
open-forge repair --automatic
```

### Expected result

Explain the active operation and preserve all selected sources.



### Verification

Verify lock ownership separately from lock-file presence and compare the complete no-write set.

**Execution constraint:** Demonstrate live same-workspace lock ownership and release; a lock file alone is insufficient.

## C06-11

**Situation:** Write failed partial

**Disposition:** Added. Worth retaining as an independently checked user outcome: Say repair stopped after the completed effects and retain precise remaining work.

### Starting point

Fail after the first of two real repair effects using FAULT-AFTER-EFFECT.

Fixture: `LINKS`. controlled fixture required.

### Steps

```text
open-forge repair --automatic
```

### Expected result

Say repair stopped after the completed effects and retain precise remaining work.



### Verification

Verify edited spans, unedited spans and recovery; do not repeat a stale pre-repair finding as though it is still current without rechecking.

**Execution constraint:** Establish a deterministic public process or real filesystem boundary and independently verify before/after effects; no timer-only or mocked-result proof.

## C06-12

**Situation:** Cancelled

**Disposition:** Added. Worth retaining as an independently checked user outcome: Report cancellation and preserve the source.

### Starting point

Open the guided repair in a real terminal, inspect the choices, then cancel before accepting effects.

Fixture: `LINKS`. fixture recipe; not instantiated.

### Steps

```text
open-forge repair
```

### Expected result

Report cancellation and preserve the source.



### Verification

Record the interaction and compare exact bytes; a redirected pipe does not prove this prompt branch.

**Execution constraint:** Use a genuine capability-appropriate terminal and preserve actual prompt/signal evidence.

## C06-13

**Situation:** Invalid input

**Disposition:** Added. Worth retaining as an independently checked user outcome: Explain the missing operand before diagnosis or mutation.

### Starting point

Provide an incomplete --relink tuple.

Fixture: `LINKS`. fixture recipe; not instantiated.

### Steps

```text
open-forge repair --relink "$LOCATION" "$EXPECTED"
```

### Expected result

Explain the missing operand before diagnosis or mutation.



### Verification

Check parser/semantic classification explicitly; no fabricated schema-3 domain report is required for parser rejection.

## C06-S04

**Situation:** Outcome 04

**Disposition:** Added. Worth retaining as an independently checked user outcome: Distinguish the selected safe correction from the unresolved choice.

### Starting point

One diagnosis-backed safe-exact link correction coexists with a separate ambiguous link that requires a choice.

Fixture: `LINKS`. fixture recipe; not instantiated.

### Steps

```text
open-forge repair --automatic
```

### Expected result

Distinguish the selected safe correction from the unresolved choice. Apply only the safe correction and preserve the ambiguous link.



### Verification

Compare each occurrence, remaining problem count and next action. A successful selected repair does not promise all unrelated Doctor problems are gone.

## C06-S05

**Situation:** Outcome 05

**Disposition:** Added. Worth retaining as an independently checked user outcome: Distinguish the selected safe correction from the unresolved choice.

### Starting point

One diagnosis-backed safe-exact link correction coexists with a separate ambiguous link that requires a choice.

Fixture: `LINKS`. fixture recipe; not instantiated.

### Steps

```text
open-forge repair --automatic --dry-run
```

### Expected result

Distinguish the selected safe correction from the unresolved choice. Preview only; change nothing.



### Verification

Compare each occurrence, remaining problem count and next action. A successful selected repair does not promise all unrelated Doctor problems are gone.

## C06-S07

**Situation:** Outcome 07

**Disposition:** Deferred; not selected yet. Retained recovery matters, but this generic permutation has no demonstrated recovery-requiring plan or reliable post-success cleanup fault.

**Required before reconsideration:** Prove this exact plan requires recovery and synchronize successful effects with denied cleanup of its positively identified bundle.

## C06-S08

**Situation:** Outcome 08

**Disposition:** Added. Worth retaining as an independently checked user outcome: Name the unavailable required fact and its consequence.

### Starting point

Use READ-DENIED on an actually required ordinary source, Template, route input or recovery fact for this operation. An ownership-only observation is not a required-input failure where this command’s explicit ownership contract says otherwise.

Fixture: `LINKS`. controlled fixture required.

### Steps

```text
open-forge repair --automatic
```

### Expected result

Name the unavailable required fact and its consequence. Do not infer a complete empty inventory or apply a partial mutation plan.



### Verification

Prove the read failure and why this input is required. Verify known safe facts remain distinct from unknown values and that no target effect started.

**Execution constraint:** Prove any denied read genuinely fails under the invoking account; missing or malformed metadata is not an I/O failure.
