---
open-forge:
  description: "find: scenario collection"
  tags: [Memory, Document, CLI, Scenario, Evergreen]
---

# find: scenario collection

These are reviewed user outcomes. Selection does not mean execution passed. Improved targets remain subject to flow validation before new tests. Deferred cases retain only their identity and reason; they are not adopted specifications.

**Who:** A maintainer using Open Forge through the public CLI, without knowing its implementation or internal state model.

**Goal:** Find the material I asked for and distinguish no matches from material that could not be searched.

Existing baseline: [Interface](../../../contracts/find/interface.md), [Behavior](../../../contracts/find/behavior.md). Exact interface and output differences must be reconciled before implementation.

See [scenario conventions](../_scenarios.md) and the [complete assessment](../../assessment.md).

## C09-01

**Situation:** Bare inventory

**Disposition:** Added. Worth retaining as an independently checked user outcome: List the eligible inventory in stable order; do not silently apply a tag or route-only filter.

### Starting point

SEARCH has a known eligible Markdown inventory including routed and eligible unrouted sources.

Fixture: `SEARCH`. fixture recipe; not instantiated.

### Steps

```text
open-forge find
```

### Expected result

List the eligible inventory in stable order; do not silently apply a tag or route-only filter.



### Verification

Compare distinct logical sources against the independently enumerated eligible universe.

## C09-02

**Situation:** One tag

**Disposition:** Added. Worth retaining as an independently checked user outcome: Return the two genuine tag matches with useful selection information.

### Starting point

Exactly two sources carry the Decision tag; a third merely contains that word in ordinary text.

Fixture: `SEARCH`. fixture recipe; not instantiated.

### Steps

```text
open-forge find --tag=Decision
```

### Expected result

Return the two genuine tag matches with useful selection information.



### Verification

Verify matching rules across authored metadata/body tag locations and exclude the ordinary-word decoy.

## C09-03

**Situation:** Two tags all

**Disposition:** Added. Worth retaining as an independently checked user outcome: Use the default all requirement and return only the intersection.

### Starting point

One source has both Decision and Architecture, one has only each, and one has neither.

Fixture: `SEARCH`. fixture recipe; not instantiated.

### Steps

```text
open-forge find --tag=Decision --tag=Architecture
```

### Expected result

Use the default all requirement and return only the intersection.



### Verification

Calculate the intersection independently; a union would look plausible but be wrong.

## C09-04

**Situation:** Heading

**Disposition:** Added. Worth retaining as an independently checked user outcome: Match the actual structural heading, not the fenced imitation.

### Starting point

One source has a parsed Acceptance heading and another has the same text inside a code fence.

Fixture: `SEARCH`. fixture recipe; not instantiated.

### Steps

```text
open-forge find --heading=Acceptance
```

### Expected result

Match the actual structural heading, not the fenced imitation.



### Verification

Check selected source IDs and heading occurrence locations against fixture syntax.

## C09-05

**Situation:** No matches

**Disposition:** Added. Worth retaining as an independently checked user outcome: Say no sources matched this request.

### Starting point

All eligible sources are readable and none has the requested NonexistentTag tag.

Fixture: `SEARCH`. fixture recipe; not instantiated.

### Steps

```text
open-forge find --tag=NonexistentTag
```

### Expected result

Say no sources matched this request. Do not imply the workspace was empty or search incomplete.



### Verification

Verify complete eligible coverage and an empty independently computed match set.

## C09-06

**Situation:** With content headings

**Disposition:** Added. Worth retaining as an independently checked user outcome: Preserve the matching source set and add requested headings without changing the query.

### Starting point

The Decision-tag matches have known parsed headings.

Fixture: `SEARCH`. fixture recipe; not instantiated.

### Steps

```text
open-forge find --tag=Decision --content=headings
```

### Expected result

Preserve the matching source set and add requested headings without changing the query.



### Verification

Compare with one-tag on an identical fixture; content projection must not act as an extra filter.

## C09-07

**Situation:** Include selector

**Disposition:** Added. Worth retaining as an independently checked user outcome: Limit the searched universe to the declared guidance subtree, including eligible unrouted Markdown there.

### Starting point

SEARCH has matching Decisions inside guidance and outside it.

Fixture: `SEARCH`. fixture recipe; not instantiated.

### Steps

```text
open-forge find --tag=Decision --include=guidance
```

### Expected result

Limit the searched universe to the declared guidance subtree, including eligible unrouted Markdown there.



### Verification

Independently compute selector expansion and prove external matches were excluded intentionally, not dropped accidentally.

## C09-08

**Situation:** Ambiguous selector

**Disposition:** Added. Worth retaining as an independently checked user outcome: Report the selector ambiguity without searching an arbitrary candidate.

### Starting point

COLLISION makes an include reference ambiguous in a noninteractive session.

Fixture: `SEARCH`. fixture recipe; not instantiated.

### Steps

```text
open-forge find --include="$AMBIGUOUS_ID"
```

### Expected result

Report the selector ambiguity without searching an arbitrary candidate.



### Verification

Compare the selected universe to empty/unresolved state; do not print a successful zero-match result.

## C09-09

**Situation:** Unreadable source

**Disposition:** Added. Worth retaining as an independently checked user outcome: Return known safe matches but clearly say search was incomplete.

### Starting point

One eligible source cannot be read and another has a confirmed matching Decision tag.

Fixture: `SEARCH`. fixture recipe; not instantiated.

### Steps

```text
open-forge find --tag=Decision
```

### Expected result

Return known safe matches but clearly say search was incomplete.



### Verification

Verify inaccessible source coverage remains unknown and the confirmed match is not suppressed.

**Execution constraint:** Prove any denied read genuinely fails under the invoking account; missing or malformed metadata is not an I/O failure.

## C09-10

**Situation:** Section missing

**Disposition:** Added. Worth retaining as an independently checked user outcome: Distinguish missing requested regions from readable non-matches and retain valid matches.

### Starting point

Some eligible sources lack the selected Details search section; at least one has a genuine matching tag inside it.

Fixture: `SEARCH`. fixture recipe; not instantiated.

### Steps

```text
open-forge find --tag=Decision --within=section:Details
```

### Expected result

Distinguish missing requested regions from readable non-matches and retain valid matches.



### Verification

Compare per-source search regions and ensure absence is not treated as an empty complete scan without the declared warning.

## C09-11

**Situation:** Invalid selector

**Disposition:** Added. Worth retaining as an independently checked user outcome: Name the invalid selector rather than silently searching everything or nothing.

### Starting point

No logical source exists for the explicit include selector.

Fixture: `SEARCH`. fixture recipe; not instantiated.

### Steps

```text
open-forge find --include=guidance/does-not-exist
```

### Expected result

Name the invalid selector rather than silently searching everything or nothing.



### Verification

Verify no fallback universe or fabricated match result.

## C09-12

**Situation:** Invalid require

**Disposition:** Added. Worth retaining as an independently checked user outcome: Explain the invalid require value and the accepted alternatives.

### Starting point

The query uses an unsupported combination mode.

Fixture: `SEARCH`. fixture recipe; not instantiated.

### Steps

```text
open-forge find --tag=Decision --require=some
```

### Expected result

Explain the invalid require value and the accepted alternatives.



### Verification

Check invalid-input outcome; do not coerce it to all or any.

## C09-S07

**Situation:** Outcome 07

**Disposition:** Deferred; not selected yet. No concrete unexpected failure mechanism or reproducible fixture is supplied; a generic outcome row adds no executable user scenario yet.

**Required before reconsideration:** Supply a plausible public/OS failure mechanism and independently verified fixture; do not inject arbitrary private result states.

## C09-S08

**Situation:** Outcome 08

**Disposition:** Deferred; not selected yet. Repeated cancellation outcome without a concrete signal boundary; retain X09 as the shared invariant until a distinct fixture earns coverage.

**Required before reconsideration:** Use a real signal-capable process and deterministic observed boundary; a timeout or invalid argument is not cancellation. Use a genuine capability-appropriate terminal and preserve actual prompt/signal evidence.
