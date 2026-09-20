---
open-forge:
  description: "route create: scenario collection"
  tags: [Memory, Document, CLI, Scenario, Evergreen]
---

# route create: scenario collection

These are reviewed user outcomes. Selection does not mean execution passed. Improved targets remain subject to flow validation before new tests. Deferred cases retain only their identity and reason; they are not adopted specifications.

**Who:** A maintainer using Open Forge through the public CLI, without knowing its implementation or internal state model.

**Goal:** Create one documented source from my metadata or a Template without overwriting existing work.

Existing baseline: [Interface](../../../contracts/route/create/interface.md), [Behavior](../../../contracts/route/create/behavior.md). Exact interface and output differences must be reconciled before implementation.

See [scenario conventions](../_scenarios.md) and the [complete assessment](../../assessment.md).

## C14-01

**Situation:** Created

**Disposition:** Added. Worth retaining as an independently checked user outcome: Create the new file with supplied metadata and update only its bounded navigation.

### Starting point

guidance is a valid parent and new-note.md does not exist.

Fixture: `ROUTES`. fixture recipe; not instantiated.

### Steps

```text
open-forge route create guidance/new-note --description "Team operating notes" --tag=Guidance
```

### Expected result

Create the new file with supplied metadata and update only its bounded navigation.



### Verification

Inspect the destination bytes, metadata and parent entry; no unrelated list churn.

## C14-02

**Situation:** Created from template

**Disposition:** Added. Worth retaining as an independently checked user outcome: Copy the Template's body as independent starting content and use the destination's supplied metadata.

### Starting point

TEMPLATE is an existing routed Template with distinctive frontmatter and body; destination is absent.

Fixture: `ROUTES`. fixture recipe; not instantiated.

### Steps

```text
open-forge route create guidance/new-note --description "Team operating notes" --tag=Guidance --template templates/example
```

### Expected result

Copy the Template's body as independent starting content and use the destination's supplied metadata.



### Verification

Verify Template-specific identity/tags did not transfer; changing the Template afterward must not mutate the new file.

## C14-03

**Situation:** Dry run

**Disposition:** Added. Worth retaining as an independently checked user outcome: Show the proposed file and parent-list change without creating either.

### Starting point

Use the created-from-template fixture without applying it.

Fixture: `ROUTES`. fixture recipe; not instantiated.

### Steps

```text
open-forge route create guidance/new-note --description "Team operating notes" --tag=Guidance --template templates/example --dry-run
```

### Expected result

Show the proposed file and parent-list change without creating either.



### Verification

Verify exact no-write evidence and preserve the Template source.

## C14-04

**Situation:** Already matching

**Disposition:** Added. Worth retaining as an independently checked user outcome: Say the file already matches and there is nothing to do.

### Starting point

The destination already contains exactly the requested supported result and navigation is current.

Fixture: `ROUTES`. fixture recipe; not instantiated.

### Steps

```text
open-forge route create guidance/new-note --description "Team operating notes" --tag=Guidance
```

### Expected result

Say the file already matches and there is nothing to do.



### Verification

Compare bytes and parent entries; no duplicated file or entry.

## C14-05

**Situation:** Missing description

**Disposition:** Improved and added. Omitted optional description or tags should not block a deterministic safe creation.

### Starting point

The target and tag are valid, but description is omitted.

Fixture: `ROUTES`. fixture recipe; not instantiated.

### Steps

```text
open-forge route create guidance/new-note --tag=Guidance
```

### Expected result

**Reviewed target:** Create the explicitly selected absent file under its existing valid parent using only the supplied metadata. Leave omitted description or tags absent, give a concise optional-metadata warning, and update the bounded navigation using real source identity. Do not invent a description, tag or authority.

### Verification

Observe whether the reviewed target is achieved. Compare affected paths, relevant ownership and source bytes independently of the report. Preserve content outside the requested change. Record current disagreement as a failure or unresolved contract difference, not a new expectation.


## C14-06

**Situation:** Missing tag

**Disposition:** Improved and added. Omitted optional description or tags should not block a deterministic safe creation.

### Starting point

The target and description are valid, but tags are omitted.

Fixture: `ROUTES`. fixture recipe; not instantiated.

### Steps

```text
open-forge route create guidance/new-note --description "Team operating notes"
```

### Expected result

**Reviewed target:** Create the explicitly selected absent file under its existing valid parent using only the supplied metadata. Leave omitted description or tags absent, give a concise optional-metadata warning, and update the bounded navigation using real source identity. Do not invent a description, tag or authority.

### Verification

Observe whether the reviewed target is achieved. Compare affected paths, relevant ownership and source bytes independently of the report. Preserve content outside the requested change. Record current disagreement as a failure or unresolved contract difference, not a new expectation.


## C14-07

**Situation:** Missing both

**Disposition:** Improved and added. Omitted optional description or tags should not block a deterministic safe creation.

### Starting point

Supply only the new target.

Fixture: `ROUTES`. fixture recipe; not instantiated.

### Steps

```text
open-forge route create guidance/new-note
```

### Expected result

**Reviewed target:** Create the explicitly selected absent file under its existing valid parent using only the supplied metadata. Leave omitted description or tags absent, give a concise optional-metadata warning, and update the bounded navigation using real source identity. Do not invent a description, tag or authority.

### Verification

Observe whether the reviewed target is achieved. Compare affected paths, relevant ownership and source bytes independently of the report. Preserve content outside the requested change. Record current disagreement as a failure or unresolved contract difference, not a new expectation.


## C14-08

**Situation:** Invalid target

**Disposition:** Added. Worth retaining as an independently checked user outcome: Explain the invalid destination and do not reinterpret it as a valid source.

### Starting point

The target attempts a parent traversal or reserved control-file destination.

Fixture: `ROUTES`. fixture recipe; not instantiated.

### Steps

```text
open-forge route create guidance/../outside --description "Notes" --tag=Guidance
```

### Expected result

Explain the invalid destination and do not reinterpret it as a valid source.



### Verification

Inspect the selected and escaped locations for zero effects.

## C14-09

**Situation:** Create an unambiguous descendant with absent intermediate scopes

**Disposition:** Improved and added. An exact descendant under a recognized root need not require a separate parent-init ritual when all missing intermediate scopes are deterministic.

### Starting point

The recognized guidance root exists. guidance/no-parent and its new-note leaf are absent; the requested ordinary path has no conflicting entrypoint, existing occupant, ambiguous identity or unsafe ancestry.

### Steps

```text
open-forge route create guidance/no-parent/new-note --description "Notes" --tag=Guidance
```

### Expected result

**Reviewed target:** Create the explicitly requested leaf and the smallest deterministic missing intermediate scopes under the recognized guidance root, using only supplied metadata and real path identities. Update bounded navigation and report created scopes. Do not invent descriptions or tags for intermediate scopes. Ambiguous roots, conflicting entrypoints or unsafe targets remain unresolved without writes. For the current executable, explicit route init followed by route create remains a documented workaround, not a required step in this pending target.

### Verification

Observe whether the reviewed target is achieved. Compare affected paths, relevant ownership and source bytes independently of the report. Preserve content outside the requested change. Record current disagreement as a failure or unresolved contract difference, not a new expectation.


**Execution constraint:** Distinguish the current explicit-init workaround from the proposed automatic creation target; an existing recognized root and unambiguous admitted path are required.

## C14-10

**Situation:** Exists with different content

**Disposition:** Added. Worth retaining as an independently checked user outcome: Refuse replacement and identify the existing file.

### Starting point

new-note.md already contains unrelated authored text and different metadata.

Fixture: `ROUTES`. fixture recipe; not instantiated.

### Steps

```text
open-forge route create guidance/new-note --description "Team operating notes" --tag=Guidance
```

### Expected result

Refuse replacement and identify the existing file. There is no force option for this command.



### Verification

Verify all original bytes survive and no misleading Created headline appears.

## C14-11

**Situation:** Template unknown

**Disposition:** Added. Worth retaining as an independently checked user outcome: Explain the missing Template, not a successful empty-body creation.

### Starting point

Destination is valid but the explicit Template reference does not resolve.

Fixture: `ROUTES`. fixture recipe; not instantiated.

### Steps

```text
open-forge route create guidance/new-note --description "Notes" --tag=Guidance --template templates/does-not-exist
```

### Expected result

Explain the missing Template, not a successful empty-body creation.



### Verification

Verify no target or navigation change and distinguish unknown from unreadable Template.

## C14-12

**Situation:** Lock held

**Disposition:** Added. Worth retaining as an independently checked user outcome: Explain the active workspace operation and preserve every path.

### Starting point

LOCK is held for an otherwise valid create.

Fixture: `ROUTES`. fixture recipe; not instantiated.

### Steps

```text
open-forge route create guidance/new-note --description "Team operating notes" --tag=Guidance
```

### Expected result

Explain the active workspace operation and preserve every path.



### Verification

Verify contention and no target file, parent edit or transient duplicate entry.

**Execution constraint:** Demonstrate live same-workspace lock ownership and release; a lock file alone is insufficient.

## C14-13

**Situation:** Write failed partial

**Disposition:** Added. Worth retaining as an independently checked user outcome: Say creation stopped and name the completed file and unfinished navigation.

### Starting point

Cause a failure after creating the leaf but before completing the second planned effect.

Fixture: `ROUTES`. fixture recipe; not instantiated.

### Steps

```text
open-forge route create guidance/new-note --description "Team operating notes" --tag=Guidance
```

### Expected result

Say creation stopped and name the completed file and unfinished navigation.



### Verification

Compare the actual target and parent list; do not claim a fully usable route merely because a leaf exists.

**Execution constraint:** Establish a deterministic public process or real filesystem boundary and independently verify before/after effects; no timer-only or mocked-result proof.

## C14-14

**Situation:** Cancelled

**Disposition:** Added. Worth retaining as an independently checked user outcome: Report cancellation and no completed creation.

### Starting point

Interrupt before the first write of an otherwise valid creation.

Fixture: `ROUTES`. fixture recipe; not instantiated.

### Steps

```text
open-forge route create guidance/new-note --description "Team operating notes" --tag=Guidance
```

### Expected result

Report cancellation and no completed creation.



### Verification

Verify the signal boundary and that the target remains absent.

**Execution constraint:** Establish a deterministic public process or real filesystem boundary and independently verify before/after effects; no timer-only or mocked-result proof.

## C14-S04

**Situation:** Outcome 04

**Disposition:** Deferred; not selected yet. Retained recovery matters, but this generic permutation has no demonstrated recovery-requiring plan or reliable post-success cleanup fault.

**Required before reconsideration:** Prove this exact plan requires recovery and synchronize successful effects with denied cleanup of its positively identified bundle.

## C14-S05

**Situation:** Outcome 05

**Disposition:** Added. Worth retaining as an independently checked user outcome: Name the unavailable required fact and its consequence.

### Starting point

Use READ-DENIED on an actually required ordinary source, Template, route input or recovery fact for this operation. An ownership-only observation is not a required-input failure where this command’s explicit ownership contract says otherwise.

Fixture: `ROUTES`. controlled fixture required.

### Steps

```text
open-forge route create guidance/new-note --description "Notes" --tag=Guidance --template templates/example
```

### Expected result

Name the unavailable required fact and its consequence. Do not infer a complete empty inventory or apply a partial mutation plan.



### Verification

Prove the read failure and why this input is required. Verify known safe facts remain distinct from unknown values and that no target effect started.

**Execution constraint:** Prove any denied read genuinely fails under the invoking account; missing or malformed metadata is not an I/O failure.
