---
open-forge:
  description: "context: scenario collection"
  tags: [Memory, Document, CLI, Scenario, Evergreen]
---

# context: scenario collection

These are reviewed user outcomes. Selection does not mean execution passed. Improved targets remain subject to flow validation before new tests. Deferred cases retain only their identity and reason; they are not adopted specifications.

**Who:** A maintainer using Open Forge through the public CLI, without knowing its implementation or internal state model.

**Goal:** Read exactly the relevant instructions and content without noise, omission or invented context.

Existing baseline: [Interface](../../../contracts/context/interface.md), [Behavior](../../../contracts/context/behavior.md). Exact interface and output differences must be reconciled before implementation.

See [scenario conventions](../_scenarios.md) and the [complete assessment](../../assessment.md).

## C08-01

**Situation:** Startup

**Disposition:** Added. Worth retaining as an independently checked user outcome: Emit startup content in the specified order, with source delimiters and no redundant success banner.

### Starting point

ROUTES has a valid host entry, loader and baseline routes with known source bytes.

Fixture: `ROUTES`. fixture recipe; not instantiated.

### Steps

```text
open-forge context
```

### Expected result

Emit startup content in the specified order, with source delimiters and no redundant success banner.



### Verification

Compare the selected source set and each content block against fixture bytes; exclude diagnostics from payload comparison.

## C08-02

**Situation:** One source

**Disposition:** Added. Worth retaining as an independently checked user outcome: Include the selected source and the contract-required context around it, without pulling in unrelated branches.

### Starting point

guidance/notes is an existing ordinary routed source with known ancestors and body.

Fixture: `ROUTES`. fixture recipe; not instantiated.

### Steps

```text
open-forge context guidance/notes
```

### Expected result

Include the selected source and the contract-required context around it, without pulling in unrelated branches.



### Verification

Build the expected closure from source metadata and links, not the command's own reported closure.

## C08-03

**Situation:** Additions only

**Disposition:** Added. Worth retaining as an independently checked user outcome: Explain that there is no additional context instead of printing a duplicate startup dump.

### Starting point

Select only a source already included in the verified startup set, so the additional set is empty.

Fixture: `ROUTES`. fixture recipe; not instantiated.

### Steps

```text
open-forge context guidance --additions-only
```

### Expected result

Explain that there is no additional context instead of printing a duplicate startup dump.



### Verification

Compare selected closure minus startup closure; a different fixture with new material belongs in X06.

## C08-04

**Situation:** Paths

**Disposition:** Added. Worth retaining as an independently checked user outcome: Print the requested paths in contract order, one per line, without Markdown code fences or authored body text.

### Starting point

Use a source with ancestors and an overwrite companion, all with known identities.

Fixture: `ROUTES`. fixture recipe; not instantiated.

### Steps

```text
open-forge context guidance/notes --content=paths
```

### Expected result

Print the requested paths in contract order, one per line, without Markdown code fences or authored body text.



### Verification

Compare ordered physical paths and deduplication; inspect stdout for unwanted headline/table decoration.

## C08-05

**Situation:** Headings

**Disposition:** Added. Worth retaining as an independently checked user outcome: Return parsed headings, not arbitrary lines starting with a hash inside opaque content.

### Starting point

notes.md contains genuine nested headings plus heading-like text inside a fenced code block.

Fixture: `ROUTES`. fixture recipe; not instantiated.

### Steps

```text
open-forge context guidance/notes --content=headings
```

### Expected result

Return parsed headings, not arbitrary lines starting with a hash inside opaque content.



### Verification

Compare structural headings with the authored fixture and exclude the fenced imitation.

## C08-06

**Situation:** Section

**Disposition:** Added. Worth retaining as an independently checked user outcome: Return the requested section's defined extent without leaking the next peer section.

### Starting point

notes.md contains one unambiguous Details section, nested subsections and an unrelated following peer section.

Fixture: `ROUTES`. fixture recipe; not instantiated.

### Steps

```text
open-forge context guidance/notes --content=section:Details
```

### Expected result

Return the requested section's defined extent without leaking the next peer section.



### Verification

Check exact selected bytes and boundary behavior, including final-newline handling.

## C08-07

**Situation:** Frontmatter missing host

**Disposition:** Added. Worth retaining as an independently checked user outcome: Treat the host's absent frontmatter as normal.

### Starting point

AGENTS.md is a valid plain host document with no YAML; routed files remain valid.

Fixture: `ROUTES`. fixture recipe; not instantiated.

### Steps

```text
open-forge context --content=frontmatter,body
```

### Expected result

Treat the host's absent frontmatter as normal. Do not fabricate YAML or a warning that belongs to a different file role.



### Verification

Compare AGENTS.md before/after and ensure its body appears intact.

## C08-08

**Situation:** Section missing

**Disposition:** Added. Worth retaining as an independently checked user outcome: Say the requested section is absent and retain any independently available requested content.

### Starting point

notes.md is valid but has no Details section.

Fixture: `ROUTES`. fixture recipe; not instantiated.

### Steps

```text
open-forge context guidance/notes --content=section:Details
```

### Expected result

Say the requested section is absent and retain any independently available requested content.



### Verification

Verify no synthetic empty section or unrelated substitute is returned; counts and status must preserve the missing-section fact.

## C08-09

**Situation:** Follow links

**Disposition:** Added. Worth retaining as an independently checked user outcome: Follow only the permitted depth and report source framing without unbounded recursive expansion.

### Starting point

notes.md has a contained authored link to target.md; target links onward to a third document.

Fixture: `ROUTES`. fixture recipe; not instantiated.

### Steps

```text
open-forge context guidance/notes --follow-links=1
```

### Expected result

Follow only the permitted depth and report source framing without unbounded recursive expansion.



### Verification

Compare the expected one-hop set; verify no duplicate source and no accidental second-hop load.

## C08-10

**Situation:** Broken followed link

**Disposition:** Added. Worth retaining as an independently checked user outcome: Keep readable selected content and explain the failed followed link and that it was not followed.

### Starting point

The explicitly followed link points to a missing or unreadable local source, while the original source remains readable.

Fixture: `ROUTES`. fixture recipe; not instantiated.

### Steps

```text
open-forge context guidance/notes --follow-links=1
```

### Expected result

Keep readable selected content and explain the failed followed link and that it was not followed.



### Verification

Verify the missing material is not silently omitted from coverage or replaced with guessed text; check the output contract’s explicit consequence clause for omitted or unfollowed material.

**Execution constraint:** Prove any denied read genuinely fails under the invoking account; missing or malformed metadata is not an I/O failure.

## C08-11

**Situation:** Unknown source

**Disposition:** Added. Worth retaining as an independently checked user outcome: Name the unresolved reference and reject the request without inventing a scope.

### Starting point

No source matches guidance/does-not-exist.

Fixture: `ROUTES`. fixture recipe; not instantiated.

### Steps

```text
open-forge context guidance/does-not-exist
```

### Expected result

Name the unresolved reference and reject the request without inventing a scope.



### Verification

Verify no unrequested startup-only success masquerades as resolution of the requested source.

## C08-12

**Situation:** Ambiguous source

**Disposition:** Added. Worth retaining as an independently checked user outcome: Explain the ambiguity and give exact resolvable alternatives.

### Starting point

COLLISION contains two different sources with one automatic ID; no terminal choice is possible.

Fixture: `ROUTES`. fixture recipe; not instantiated.

### Steps

```text
open-forge context "$AMBIGUOUS_ID"
```

### Expected result

Explain the ambiguity and give exact resolvable alternatives. Do not choose by traversal order.



### Verification

Repeat with reversed directory creation order and confirm the same blocked meaning and stable alternatives.

## C08-13

**Situation:** Unreadable source

**Disposition:** Added. Worth retaining as an independently checked user outcome: Preserve safe available content and explicitly qualify the missing part.

### Starting point

One selected source is genuinely unreadable; its readable ancestor and another selected source are available.

Fixture: `ROUTES`. fixture recipe; not instantiated.

### Steps

```text
open-forge context guidance/notes patterns/example
```

### Expected result

Preserve safe available content and explicitly qualify the missing part.



### Verification

Compare reported coverage and actual readable source set; do not claim complete closure.

**Execution constraint:** Prove any denied read genuinely fails under the invoking account; missing or malformed metadata is not an I/O failure.

## C08-14

**Situation:** Invalid content

**Disposition:** Added. Worth retaining as an independently checked user outcome: Explain the unsupported content value without reading or returning a misleading partial result.

### Starting point

The source is valid but the content projection is unknown.

Fixture: `ROUTES`. fixture recipe; not instantiated.

### Steps

```text
open-forge context guidance/notes --content=not-a-part
```

### Expected result

Explain the unsupported content value without reading or returning a misleading partial result.



### Verification

Check argument classification and no mutation; keep parser failures distinct from typed semantic failures.

## C08-S07

**Situation:** Outcome 07

**Disposition:** Deferred; not selected yet. No concrete unexpected failure mechanism or reproducible fixture is supplied; a generic outcome row adds no executable user scenario yet.

**Required before reconsideration:** Supply a plausible public/OS failure mechanism and independently verified fixture; do not inject arbitrary private result states.

## C08-S08

**Situation:** Outcome 08

**Disposition:** Deferred; not selected yet. Repeated cancellation outcome without a concrete signal boundary; retain X09 as the shared invariant until a distinct fixture earns coverage.

**Required before reconsideration:** Use a real signal-capable process and deterministic observed boundary; a timeout or invalid argument is not cancellation. Use a genuine capability-appropriate terminal and preserve actual prompt/signal evidence.
