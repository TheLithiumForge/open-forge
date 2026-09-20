---
open-forge:
  description: "route update: scenario collection"
  tags: [Memory, Document, CLI, Scenario, Evergreen]
---

# route update: scenario collection

These are reviewed user outcomes. Selection does not mean execution passed. Improved targets remain subject to flow validation before new tests. Deferred cases retain only their identity and reason; they are not adopted specifications.

**Who:** A maintainer using Open Forge through the public CLI, without knowing its implementation or internal state model.

**Goal:** Change the metadata I selected, while keeping authored content and unrelated fields.

Existing baseline: [Interface](../../../contracts/route/update/interface.md), [Behavior](../../../contracts/route/update/behavior.md). Exact interface and output differences must be reconciled before implementation.

See [scenario conventions](../_scenarios.md) and the [complete assessment](../../assessment.md).

## C15-01

**Situation:** Description changed

**Disposition:** Added. Worth retaining as an independently checked user outcome: Show the old and new description and the bounded navigation update.

### Starting point

notes.md has an older nonempty description and distinctive body.

Fixture: `ROUTES`. fixture recipe; not instantiated.

### Steps

```text
open-forge route update guidance/notes --description "Current team operating notes"
```

### Expected result

Show the old and new description and the bounded navigation update.



### Verification

Compare body bytes and unrelated frontmatter; only authorized fields and generated navigation may change.

## C15-02

**Situation:** Tags replaced

**Disposition:** Added. Worth retaining as an independently checked user outcome: Replace the complete tag list in the supplied order, rather than appending silently.

### Starting point

The source has an ordered old tag list different from Guidance,Team.

Fixture: `ROUTES`. fixture recipe; not instantiated.

### Steps

```text
open-forge route update guidance/notes --tag=Guidance --tag=Team
```

### Expected result

Replace the complete tag list in the supplied order, rather than appending silently.



### Verification

Compare the exact final list and confirm old-only tags are absent.

## C15-03

**Situation:** Responsibility removed

**Disposition:** Improved and added. Removing responsibility should not impose unrelated mandatory metadata requirements.

### Starting point

The source has a responsibility key, authored metadata and distinctive body text. Removal of responsibility must not require absent optional metadata.

### Steps

```text
open-forge route update guidance/notes --responsibility ""
```

### Expected result

**Reviewed target:** Remove only the responsibility key requested by the empty value. Preserve the existing description, tags, other metadata and authored body exactly; absent optional fields remain absent.

### Verification

Observe whether the reviewed target is achieved. Compare affected paths, relevant ownership and source bytes independently of the report. Preserve content outside the requested change. Record current disagreement as a failure or unresolved contract difference, not a new expectation.


## C15-04

**Situation:** Template applied

**Disposition:** Added. Worth retaining as an independently checked user outcome: Copy the Template body while retaining destination metadata and independent ownership.

### Starting point

The destination is eligible frontmatter-only content; templates/example has a distinctive body.

Fixture: `ROUTES`. fixture recipe; not instantiated.

### Steps

```text
open-forge route update guidance/notes --template templates/example
```

### Expected result

Copy the Template body while retaining destination metadata and independent ownership.



### Verification

Compare the exact eligible body result and verify the Template source remains unchanged.

## C15-05

**Situation:** Template body protected

**Disposition:** Added. Worth retaining as an independently checked user outcome: Apply the allowed metadata change but explain that existing body content was kept and the Template body was not copied.

### Starting point

The destination already has meaningful body text; supply a Template and a valid metadata change.

Fixture: `ROUTES`. fixture recipe; not instantiated.

### Steps

```text
open-forge route update guidance/notes --description "Current team operating notes" --template templates/example
```

### Expected result

Apply the allowed metadata change but explain that existing body content was kept and the Template body was not copied.



### Verification

Verify both halves: metadata changed, body byte-identical. Neither total success nor total no-op is truthful.

## C15-06

**Situation:** No change

**Disposition:** Added. Worth retaining as an independently checked user outcome: Say the source already has the requested values and nothing changed.

### Starting point

Requested metadata already equals the existing supported values.

Fixture: `ROUTES`. fixture recipe; not instantiated.

### Steps

```text
open-forge route update guidance/notes --description "Current team operating notes"
```

### Expected result

Say the source already has the requested values and nothing changed.



### Verification

Compare the exact file and parent entry; avoid formatting churn disguised as a no-op.

## C15-07

**Situation:** Dry run

**Disposition:** Added. Worth retaining as an independently checked user outcome: Show old-to-new values in future tense and No files were changed.

### Starting point

Use a real metadata difference and a current parent list.

Fixture: `ROUTES`. fixture recipe; not instantiated.

### Steps

```text
open-forge route update guidance/notes --description "Current team operating notes" --dry-run
```

### Expected result

Show old-to-new values in future tense and No files were changed.



### Verification

Compare source bytes, parent bytes and recovery inventory.

## C15-08

**Situation:** No patch

**Disposition:** Improved and added. An exact source with no requested patch has a clear harmless no-op meaning.

### Starting point

The source exists but no metadata or Template operation is supplied.

Fixture: `ROUTES`. fixture recipe; not instantiated.

### Steps

```text
open-forge route update guidance/notes
```

### Expected result

**Reviewed target:** Explain that no change was requested and change nothing. Do not claim an update occurred or require metadata merely to turn the request into a mutation.

### Verification

Observe whether the reviewed target is achieved. Compare affected paths, relevant ownership and source bytes independently of the report. Preserve content outside the requested change. Record current disagreement as a failure or unresolved contract difference, not a new expectation.


## C15-09

**Situation:** Unknown source

**Disposition:** Added. Worth retaining as an independently checked user outcome: Name the missing source without creating it.

### Starting point

The source operand does not resolve.

Fixture: `ROUTES`. fixture recipe; not instantiated.

### Steps

```text
open-forge route update guidance/does-not-exist --description "Notes"
```

### Expected result

Name the missing source without creating it.



### Verification

Verify no create fallback or parent-list addition.

## C15-10

**Situation:** An ambiguous ID requires an exact path

**Disposition:** Added. Worth retaining as an independently checked user outcome: Stop without a write.

### Starting point

COLLISION contains two distinct safe files with the same automatic ID. Both have
valid metadata, and neither has been selected by an exact path. This command is
noninteractive, including in a real terminal.

### Steps

```text
open-forge route update "$AMBIGUOUS_ID" --description "Selected source only"
```

### Expected result

Stop without a write. Name the ambiguity and exact candidate paths so the person
can rerun against the intended file. Do not open a chooser, choose the first
candidate, or claim the patch was applied.



### Verification

Compare both candidate files and their parents byte-for-byte. Confirm no prompt
was attempted on a terminal or redirected input. A separately executed exact-path
rerun uses the ordinary fields-changed scenario, not the blocked result above.

**Execution constraint:** Use a genuine capability-appropriate terminal and preserve actual prompt/signal evidence.

## C15-11

**Situation:** Lock held

**Disposition:** Added. Worth retaining as an independently checked user outcome: Explain the active operation without modifying metadata or navigation.

### Starting point

LOCK holds this workspace during a real metadata change.

Fixture: `ROUTES`. fixture recipe; not instantiated.

### Steps

```text
open-forge route update guidance/notes --description "Current team operating notes"
```

### Expected result

Explain the active operation without modifying metadata or navigation.



### Verification

Confirm actual lock ownership and byte-identical preservation.

**Execution constraint:** Demonstrate live same-workspace lock ownership and release; a lock file alone is insufficient.

## C15-12

**Situation:** Write failed partial

**Disposition:** Added. Worth retaining as an independently checked user outcome: State the completed portion and the real recovery status.

### Starting point

Fail deterministically after one of the source/parent effects.

Fixture: `ROUTES`. fixture recipe; not instantiated.

### Steps

```text
open-forge route update guidance/notes --description "Current team operating notes"
```

### Expected result

State the completed portion and the real recovery status.



### Verification

Verify the file and parent independently; a changed description does not prove navigation finished.

**Execution constraint:** Establish a deterministic public process or real filesystem boundary and independently verify before/after effects; no timer-only or mocked-result proof.

## C15-13

**Situation:** Cancel before any persistent effect

**Disposition:** Added. Worth retaining as an independently checked user outcome: Report cancellation with no changed files.

### Starting point

Use one exact valid target and a controlled execution fixture that can deliver
Ctrl+C after admission but before any write. No chooser or confirmation exists.
This cancellation timing is a required fixture capability, not a claimed run.

### Steps

```text
open-forge route update .agents/guidance/team.md --description "Team conventions"
```

Deliver Ctrl+C at the defined pre-effect boundary.

### Expected result

Report cancellation with no changed files. Do not treat the signal as an invalid
input or a declined nonexistent prompt. Cancellation after an effect belongs to
the separate shared interrupted-effect checks.



### Verification

Record signal delivery and the process status. Compare the full selected fixture
state and any settings/recovery locations. No signal or unproven delivery means
this case is not run, not passed.

**Execution constraint:** Establish a deterministic public process or real filesystem boundary and independently verify before/after effects; no timer-only or mocked-result proof.

## C15-S05

**Situation:** Outcome 05

**Disposition:** Deferred; not selected yet. Retained recovery matters, but this generic permutation has no demonstrated recovery-requiring plan or reliable post-success cleanup fault.

**Required before reconsideration:** Prove this exact plan requires recovery and synchronize successful effects with denied cleanup of its positively identified bundle.

## C15-S06

**Situation:** Outcome 06

**Disposition:** Added. Worth retaining as an independently checked user outcome: Name the unavailable required fact and its consequence.

### Starting point

Use READ-DENIED on an actually required ordinary source, Template, route input or recovery fact for this operation. An ownership-only observation is not a required-input failure where this command’s explicit ownership contract says otherwise.

Fixture: `ROUTES`. controlled fixture required.

### Steps

```text
open-forge route update guidance/notes --template templates/example
```

### Expected result

Name the unavailable required fact and its consequence. Do not infer a complete empty inventory or apply a partial mutation plan.



### Verification

Prove the read failure and why this input is required. Verify known safe facts remain distinct from unknown values and that no target effect started.

**Execution constraint:** Prove any denied read genuinely fails under the invoking account; missing or malformed metadata is not an I/O failure.
