---
open-forge:
  description: "route inspect: scenario collection"
  tags: [Memory, Document, CLI, Scenario, Evergreen]
---

# route inspect: scenario collection

These are reviewed user outcomes. Selection does not mean execution passed. Improved targets remain subject to flow validation before new tests. Deferred cases retain only their identity and reason; they are not adopted specifications.

**Who:** A maintainer using Open Forge through the public CLI, without knowing its implementation or internal state model.

**Goal:** Understand where one source belongs, when it is read and what context it adds.

Existing baseline: [Interface](../../../contracts/route/inspect/interface.md), [Behavior](../../../contracts/route/inspect/behavior.md). Exact interface and output differences must be reconciled before implementation.

See [scenario conventions](../_scenarios.md) and the [complete assessment](../../assessment.md).

## C12-01

**Situation:** Entrypoint

**Disposition:** Added. Worth retaining as an independently checked user outcome: Explain where the scope belongs, when it is read and the declared context measurements.

### Starting point

guidance is a valid routed entrypoint with a known parent and descendants.

Fixture: `ROUTES`. fixture recipe; not instantiated.

### Steps

```text
open-forge route inspect guidance
```

### Expected result

Explain where the scope belongs, when it is read and the declared context measurements.



### Verification

Check source-role and scope distinctions; do not count all descendants as automatically loaded.

## C12-02

**Situation:** Routed file

**Disposition:** Added. Worth retaining as an independently checked user outcome: Describe that file's identity and inherited context, not an imaginary child scope.

### Starting point

notes.md is a valid ordinary routed leaf under guidance.

Fixture: `ROUTES`. fixture recipe; not instantiated.

### Steps

```text
open-forge route inspect guidance/notes
```

### Expected result

Describe that file's identity and inherited context, not an imaginary child scope.



### Verification

Compare its actual ancestor chain and byte measurements.

## C12-03

**Situation:** Load now child

**Disposition:** Added. Worth retaining as an independently checked user outcome: Explain the parent's loading condition and the child's resulting automatic inclusion.

### Starting point

A child is tagged LoadNow under an exposed parent whose loading is known.

Fixture: `ROUTES`. fixture recipe; not instantiated.

### Steps

```text
open-forge route inspect guidance/notes
```

### Expected result

Explain the parent's loading condition and the child's resulting automatic inclusion.



### Verification

Verify the tag is conditional on its parent exposure, not globally loaded merely because the file exists.

## C12-04

**Situation:** Keep in mind

**Disposition:** Added. Worth retaining as an independently checked user outcome: Explain continuity and refresh behavior without turning the tag into stronger authority.

### Starting point

The selected source has KeepInMind and a defined exposed route.

Fixture: `ROUTES`. fixture recipe; not instantiated.

### Steps

```text
open-forge route inspect guidance/notes
```

### Expected result

Explain continuity and refresh behavior without turning the tag into stronger authority.



### Verification

Compare the source's actual tag and scope; do not claim permanent model memory.

## C12-05

**Situation:** Overwrite pair

**Disposition:** Added. Worth retaining as an independently checked user outcome: Describe one logical source with both layers and their real reading behavior.

### Starting point

notes.md has one valid adjacent notes.overwrite.md with distinct known bytes.

Fixture: `ROUTES`. fixture recipe; not instantiated.

### Steps

```text
open-forge route inspect guidance/notes
```

### Expected result

Describe one logical source with both layers and their real reading behavior.



### Verification

Check base/overwrite association, contribution measurements and layer order; avoid double-counting identity.

## C12-06

**Situation:** Compatibility entrypoint

**Disposition:** Added. Worth retaining as an independently checked user outcome: Resolve it once and explain the same scope without demanding a rename.

### Starting point

Use a recognized compatible entrypoint name in an otherwise unambiguous scope.

Fixture: `ROUTES`. fixture recipe; not instantiated.

### Steps

```text
open-forge route inspect "$COMPAT_ENTRY"
```

### Expected result

Resolve it once and explain the same scope without demanding a rename.



### Verification

Verify physical identity deduplication rather than counting canonical and compatibility matches twice.

## C12-07

**Situation:** Not routed file

**Disposition:** Added. Worth retaining as an independently checked user outcome: Describe that it is not currently routed, without inventing a route or treating physical existence as exposure.

### Starting point

An eligible Markdown file exists physically but is not exposed by a route entry.

Fixture: `ROUTES`. fixture recipe; not instantiated.

### Steps

```text
open-forge route inspect .agents/guidance/unlisted.md
```

### Expected result

Describe that it is not currently routed, without inventing a route or treating physical existence as exposure.



### Verification

Compare parent Entries and source identity; inspect must not automatically index it.

## C12-08

**Situation:** Id not unique exact path

**Disposition:** Added. Worth retaining as an independently checked user outcome: Inspect the exact file and explain its non-unique automatic ID without blocking a resolved request.

### Starting point

COLLISION has a shared ID, but the request supplies the exact intended physical file path.

Fixture: `ROUTES`. fixture recipe; not instantiated.

### Steps

```text
open-forge route inspect "$EXACT_SOURCE"
```

### Expected result

Inspect the exact file and explain its non-unique automatic ID without blocking a resolved request.



### Verification

Verify the chosen physical identity and no merged measurements from the other candidate.

## C12-09

**Situation:** Ambiguous id prompt

**Disposition:** Added. Worth retaining as an independently checked user outcome: Present clear alternatives, honor the choice and qualify the shared ID.

### Starting point

COLLISION is queried in a genuine prompt-capable terminal; select the second displayed exact source.

Fixture: `ROUTES`. fixture recipe; not instantiated.

### Steps

```text
open-forge route inspect "$AMBIGUOUS_ID"
```

### Expected result

Present clear alternatives, honor the choice and qualify the shared ID.



### Verification

Record the selected item and match every subsequent identity and measurement to it.

**Execution constraint:** Use a genuine capability-appropriate terminal and preserve actual prompt/signal evidence.

## C12-10

**Situation:** Unknown source

**Disposition:** Added. Worth retaining as an independently checked user outcome: Say the source cannot be found, not that it has zero content.

### Starting point

Neither a route nor a file matches the operand.

Fixture: `ROUTES`. fixture recipe; not instantiated.

### Steps

```text
open-forge route inspect guidance/does-not-exist
```

### Expected result

Say the source cannot be found, not that it has zero content.



### Verification

Verify no nearest-match selection or file creation.

## C12-11

**Situation:** Loader subject

**Disposition:** Added. Worth retaining as an independently checked user outcome: Explain that this command does not inspect the loader as an ordinary route.

### Starting point

Supply the loader itself as the inspect operand.

Fixture: `ROUTES`. fixture recipe; not instantiated.

### Steps

```text
open-forge route inspect .agents/loader.md
```

### Expected result

Explain that this command does not inspect the loader as an ordinary route.



### Verification

Verify the dedicated invalid-input boundary without corrupting startup context.

## C12-12

**Situation:** Unreadable source

**Disposition:** Added. Worth retaining as an independently checked user outcome: Keep known identity facts and label unavailable measurements and the unreadable layer.

### Starting point

One required source layer is unreadable while its identity can be established.

Fixture: `ROUTES`. fixture recipe; not instantiated.

### Steps

```text
open-forge route inspect guidance/notes
```

### Expected result

Keep known identity facts and label unavailable measurements and the unreadable layer.



### Verification

Compare unknown measurements with null/unavailable fields; no fabricated zero token cost.

**Execution constraint:** Prove any denied read genuinely fails under the invoking account; missing or malformed metadata is not an I/O failure.

## C12-13

**Situation:** Orphan overwrite

**Disposition:** Added. Worth retaining as an independently checked user outcome: Explain the missing base relationship and do not treat the overwrite as an independent source.

### Starting point

notes.overwrite.md exists without its corresponding base file.

Fixture: `ROUTES`. fixture recipe; not instantiated.

### Steps

```text
open-forge route inspect .agents/guidance/notes.overwrite.md
```

### Expected result

Explain the missing base relationship and do not treat the overwrite as an independent source.



### Verification

Verify the absent base, blocked resolution and no automatic base creation.

## C12-S06

**Situation:** Outcome 06

**Disposition:** Deferred; not selected yet. No concrete unexpected failure mechanism or reproducible fixture is supplied; a generic outcome row adds no executable user scenario yet.

**Required before reconsideration:** Supply a plausible public/OS failure mechanism and independently verified fixture; do not inject arbitrary private result states.

## C12-S07

**Situation:** Outcome 07

**Disposition:** Deferred; not selected yet. Repeated cancellation outcome without a concrete signal boundary; retain X09 as the shared invariant until a distinct fixture earns coverage.

**Required before reconsideration:** Use a real signal-capable process and deterministic observed boundary; a timeout or invalid argument is not cancellation. Use a genuine capability-appropriate terminal and preserve actual prompt/signal evidence.
