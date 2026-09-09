---
open-forge:
  description: Technology-neutral resolution, expansion, set formation, safety, determinism, and conformance for shared source-universe filters
  responsibility: Define how an applicable consumer forms and reports an effective logical source universe without choosing implementation technology
  tags: [Memory, Crystallized, CLI, Release, Command, Contract, Shared, Source, Universe, Filter, Behavior, Determinism, CurrentTruth]
---

# Shared Source-Universe Filters Behavior Contract

## Status And Scope

This file is the accepted current Crystallized authority for the
technology-neutral resolution and conformance behavior behind the [Shared
Source-Universe Filters Interface Contract](interface.md). The commands that
apply the contract do not ship yet; implementation and executable evidence are tracked in
[CLI Development](../../../../../../working/cli-development/_cli-development.md). The [Shared Result
Coordinates](../result-coordinates/interface.md) define the shared result
boundary. The [CLI Architecture](../../../architecture.md) defines filesystem,
runtime, and evidence boundaries; this behavior defines only selector resolution
and set meaning.

The behavior below does not choose a parser library, filesystem library,
runtime, source module, or implementation stage. A conforming implementation
may combine or reorder internal work when it preserves the same observable
resolution, expansion, set, safety, reporting, and deterministic facts.

## Applicability Boundary

The consuming command establishes whether the shared filters apply. Its parser
recognizes `--include` and `--exclude` only for that declared operation. An
unrelated command rejects them as unknown rather than passing them through as
no-ops. The shared behavior therefore starts only after an applicable consumer
has declared the flags in its public command form.

The consumer also supplies its selected workspace boundary, default source
universe, and source-eligibility rules. The shared behavior does not invent a
universal value for any of them.

## Resolution Responsibilities

### Request And Occurrence Resolution

The operation reads each applicable `--include` or `--exclude` occurrence as
one scalar shared source reference. It preserves the supplied occurrence and
its command-line position for later reporting. It rejects missing values,
comma lists, globs, arbitrary-directory operands, positional substitution, and
new qualifier grammar rather than assigning them alternate meanings.

Source-reference classification, normalization, identity, collision handling,
disambiguation, and physical-safety checks use the shared rules. Resolution
does not choose a source by likely intent or guess whether an unresolved value
was meant as another ID, path, layer, or directory.

Missing, empty, unknown, and unsupported selectors form the shared invalid
outcome. Ambiguous IDs form the shared blocked outcome unless an interactive
disambiguation flow resolves the ambiguity to a specific identity. JSON and
other non-interactive flows do not prompt. Unsafe or escaping physical
boundaries form the shared blocked outcome. A consumer reports these outcomes
without turning them into a guessed effective universe.

### Physical Expansion

For each successfully resolved occurrence, expansion is based on the resolved
source kind:

- Loader, a recognized entrypoint, and `SKILL.md` expand from the physical
  folder containing the selected source. Expansion visits every Markdown source
  eligible under the consumer's rules that is physically contained below that
  folder, including eligible unrouted Markdown.
- An ordinary source expands to one logical source.

Expansion follows physical containment, not route traversal. It does not follow
links or infer route, scope, authority, lifecycle, relevance, or applicability.
It does not add a routed child outside the selected folder or remove an
eligible unrouted source inside it.

A valid base and overwrite companion is resolved as one logical source with
both physical layers. A reference through the ID, base path, or overwrite path
does not split the pair or make the overwrite an independent candidate.

The expansion result retains exact logical identity and physical paths needed
for collision, safety, and selector reporting. A selector that resolves
validly may produce an empty expansion; the consumer owns the meaning of an
empty effective universe.

### Effective-Set Formation

After all occurrences resolve and expand, the operation forms the effective
logical source universe in this order:

1. If no include occurs, start with the consumer's declared default universe.
   Otherwise, start with the union of every include expansion.
2. Form the union of every exclude expansion, or the empty set when no exclude
   occurs.
3. Subtract the exclude union from the starting set.

The operation deduplicates equivalent logical identities and overlapping
expansions. Exclusion therefore wins overlap, and changing the argument order
does not change the effective set. Selector spelling, repetition, overlap, and
argument order do not become source multiplicity.

Effective-set formation is complete before the consumer performs its
operation-specific candidate inspection, predicate matching, projection, or
result formation. The consumer receives only effective logical sources as
candidates. Sources removed by the exclude union are outside that operation's
effective universe; the consumer decides how its own coverage and counts treat
the remaining candidates.

### Reporting Handoff

The resolver carries both the supplied occurrence stream and the resolved
selector records into the consumer's result construction. Supplied occurrences
remain in command-line order, including duplicates. Resolved selector sets and
effective logical sources remain deduplicated. The consumer's renderer or
structured result exposes those facts without rerunning selector resolution.

The shared behavior does not choose a result-row schema, selector-detail
location, source ordering, coverage status, or output density. Those are
consumer-owned after the shared effective universe is available.

## Safety And Determinism Invariants

- Every resolved physical identity remains within the selected workspace and
  selected folder boundary required by its expansion kind.
- An unsafe, escaping, ambiguous, or otherwise unresolved identity is never
  silently omitted and never admitted through a weaker path or guessed match.
- The effective set is formed from logical-source identity, so a valid base and
  overwrite pair cannot be duplicated by selecting either physical layer.
- With unchanged workspace state, consumer default, source eligibility, and
  supplied occurrence stream, resolution produces the same selector outcomes,
  expansion identities, effective logical set, and supplied/resolved reporting
  facts. Include/exclude argument order does not affect those set results.
- The behavior has no implementation requirement beyond preserving the
  shared Interface Contract; no library or module choice is part of this
  contract.

## Conformance Responsibilities

A Gate 5 executable proof must cover all of the following:

- an applicable command accepts the exact scalar spellings and an unrelated
  command rejects them as unknown;
- omission uses the consumer's declared default and omitted exclusion subtracts
  nothing, without assuming a universal shared default;
- comma lists, globs, arbitrary directories, positional substitutions, and
  unaccepted qualifiers are rejected;
- Loader, recognized entrypoint, and `SKILL.md` expand by physical folder root,
  including eligible unrouted Markdown, while an ordinary source selects one
  logical source;
- valid base/overwrite pairs remain one logical source through ID, base-path,
  and overwrite-path selection;
- include and exclude unions, duplicate and overlapping selectors,
  exclusion-wins behavior, and argument-order independence are proven;
- shared identity, collision, disambiguation, invalid, blocked, and unsafe
  boundary outcomes are preserved without guessing or non-interactive prompts;
- effective-universe formation precedes consumer inspection and matching, and
  excluded sources are not treated as effective candidates;
- supplied occurrences remain reportable in command-line order while resolved
  selector sets and effective logical sources are deduplicated; and
- unchanged inputs produce stable resolution and set facts without relying on
  filesystem enumeration order or discovery timing.

The consuming command must separately verify its declared default universe,
coverage, semantic results, output schema, and operation-specific composition
with predicates or projections. Those dimensions are not redefined here.

## Related Contracts

- [Shared Source-Universe Filters Interface Contract](interface.md)
- [Shared Source-Universe Filters route](_source-universe-filters.md)
- [CLI Source References Interface Contract](../source-references/interface.md)
- [CLI Source References Behavior Contract](../source-references/behavior.md)
- [CLI Architecture](../../../architecture.md)
