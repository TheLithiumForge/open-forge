---
open-forge:
  description: Accepted current technology-neutral operation flow, coverage mechanics, and conformance for `find`
  responsibility: Define how a conforming implementation resolves, inspects, aggregates, and forms the typed result consumed by `find` renderers
  tags: [Memory, Crystallized, CLI, Release, Command, Contract, Find, Behavior, Discovery, Determinism, Coverage, CurrentTruth]
---

# Find Behavior Contract

## Status And Authority

This file is the current Crystallized authority for the technology-neutral
`find` Behavior Contract. The command is a current non-shipping contract and
does not claim executable behavior.

This contract defines only the technology-neutral operation flow and conformance
mechanics behind the [Interface Contract](interface.md). The Interface Contract
defines all public grammar, selection, matching, result, status, output, error,
and non-goal detail. A conforming implementation may choose different libraries
or source boundaries, but it must produce the same observable facts. The
accepted implementation choices are recorded in the [Technical Design](technical-design.md).

## Operation Invariants

- `find` performs one complete flat source-discovery operation.
  Bare inventory and predicate-bearing invocations use the same resolved request
  and source universe; predicates do not select hidden child operations.
- For unchanged workspace bytes, include and exclude selector occurrences,
  predicate occurrences, selected regions, and projections, the operation
  deterministically resolves the same effective source universe and produces the
  same public candidates, matches, order, evidence, findings, and semantic result
  described by the [Interface Purpose](interface.md#purpose), [Structured Result
  Fields](interface.md#structured-result-fields), [Section Findings And Public
  Ordering](interface.md#section-findings-and-public-ordering), and [Filtered
  Coverage, Results, And Ordering](interface.md#filtered-coverage-results-and-ordering)
  sections.
- The operation is read-only. It does not diagnose, repair, install, clean, or
  otherwise persistently mutate the workspace, and it does not acquire mutation
  authority while collecting or rendering a result. This satisfies the
  Interface Contract's [Non-Goals](interface.md#non-goals) and [Public
  Verification](interface.md#public-verification) boundaries.

## Required Implementation-Evidence Boundary

The public verification requirements in the Interface Contract remain in force.
Implementation evidence **must cover** the dimensions listed in the Interface
Contract's [Public Verification](interface.md#public-verification) section.
The following stable sections retain the detailed evidence allocation without
copying the public contract.

- Required evidence covers exact current-directory and explicit workspace
  selection without upward or marker-based discovery, as specified by [Workspace
  And Source Universe](interface.md#workspace-and-source-universe).
- Required evidence covers complete deterministic enumeration of eligible
  `.agents` Markdown and every candidate class in [Workspace And Source
  Universe](interface.md#workspace-and-source-universe).
- Required evidence covers bare inventory at minimal detail and explicit
  ID-and-path results in [Bare Inventory](interface.md#bare-inventory) and
  [Human Output](interface.md#human-output).
- Required evidence covers tag input boundaries, case variants, duplicate
  predicates, Unicode variants, whole-token boundaries, and invalid comma lists
  in [Tag Query Values](interface.md#tag-query-values) and [Public Tag Matching](interface.md#public-tag-matching).
- Required evidence covers culture-independent comparison, including Turkish-
  culture coverage, and authored spelling in [Public Tag Matching](interface.md#public-tag-matching).
  The concrete comparison realization remains in the [Technical Design](technical-design.md).
- Required evidence covers frontmatter list-value tags and body tag tokens in
  every included and excluded region in [Public Tag Matching](interface.md#public-tag-matching).
- Required evidence covers parser-recognized ATX, Setext, formatted, linked,
  code-containing, duplicate, and malformed headings at every level in [Public
  Heading Matching](interface.md#public-heading-matching).
- Required evidence covers shared heading and section boundaries between find
  evidence and `context` projection in [Public Heading Matching](interface.md#public-heading-matching)
  and [Content Projection](interface.md#content-projection).
- Required evidence covers repeated tags, repeated headings, mixed predicates,
  `all`, `any`, duplicate inputs, and flat-logic limitations in [Predicate
  Requirement](interface.md#predicate-requirement), [Logical Sources And
  Overwrite Framing](interface.md#logical-sources-and-overwrite-framing), and
  [Public Heading Matching](interface.md#public-heading-matching).
- Required evidence covers default regions, `document`, `frontmatter`, `body`,
  exact sections, unions, redundancy, escaping, and incompatible selections in
  [Search Regions](interface.md#search-regions) and [Section Findings And Public
  Ordering](interface.md#section-findings-and-public-ordering).
- Required evidence covers minimal, standard, full, debug, content-projected,
  and structured output from one typed result in [Human Output](interface.md#human-output),
  [Content Projection](interface.md#content-projection), [Structured Output](interface.md#structured-output),
  and [Structured Result Fields](interface.md#structured-result-fields).
- Required evidence covers source ID collisions, canonical ordering, layer
  ordering, evidence ordering, and stable repeat invocation in [Section Findings
  And Public Ordering](interface.md#section-findings-and-public-ordering).
- Required evidence covers completed matches, completed zero matches,
  completed-with-warnings, incomplete safe matches, invalid-input, blocked
  boundaries, failed execution, and cancellation in [Semantic Results](interface.md#semantic-results).
- Required evidence covers the non-goals that `find` does not use a persistent
  index, follow links, search arbitrary text, rank results, build the complete
  graph, or mutate anything in [Non-Goals](interface.md#non-goals) and [Workspace
  And Source Universe](interface.md#workspace-and-source-universe).
- Direct tests **should** prove predicate parsing, comparison, aggregation,
  region selection, structural heading extraction, body-tag scanning, ordering,
  and semantic results. They should exercise the public facts in [Tag Query
  Values](interface.md#tag-query-values), [Search Regions](interface.md#search-regions),
  [Human Output](interface.md#human-output), and [Semantic Results](interface.md#semantic-results).
- Focused integration tests **should** use real temporary workspaces and
  filesystem state to cross the source-boundary and inspection mechanics
  described below.
- Repeat invocations with unchanged inputs must produce the same semantic result
  and ordered evidence, satisfying the [Interface Purpose](interface.md#purpose)
  and [Section Findings And Public Ordering](interface.md#section-findings-and-public-ordering)
  rules.

## Deterministic Conformance Responsibilities

The responsibilities below describe a technology-neutral conceptual flow for
conformance review. They do not require separate runtime stages, one source
module per responsibility, or a fixed internal execution order. An
implementation may combine or reorder work when it preserves the Interface
Contract and does not claim facts before their preconditions are established.

- Request resolution validates the command path, operands,
  flags, shared global inputs, defaults, repetitions, and conflicts according to
  the Interface Contract. Invalid input produces the public invalid-input result; this
  responsibility does not invent alternate grammar.
- Workspace resolution establishes the exact selected
  directory and selection method. It checks the safe lexical and physical
  boundary needed for inspection before claiming a source universe, using the
  public boundary in [Workspace And Source Universe](interface.md#workspace-and-source-universe).
- Enumeration creates a fresh per-invocation candidate set
  from the selected boundary. The operation records every discovered candidate's
  canonical identity and inspection disposition so navigation, timing, or a
  missing index cannot silently remove an eligible candidate.
- Layer resolution groups each valid adjacent overwrite with its
  base logical source while retaining both physical layers and their order. An
  orphan, ambiguous relationship, or unsafe identity remains an inspection
  finding rather than becoming an invented standalone source. The public
  identity and framing are defined by [Logical Sources And Overwrite Framing](interface.md#logical-sources-and-overwrite-framing).
- Inspection validates the bytes and containment of each discovered candidate
  before admitting it as an inspectable layer. The stage records
  unreadable, non-UTF-8, malformed, ambiguous, orphaned, and unsafe outcomes;
  it never replaces an unavailable fact with a guessed one. Coverage formation
  uses [Workspace And Source Universe](interface.md#workspace-and-source-universe),
  [Logical Sources And Overwrite Framing](interface.md#logical-sources-and-overwrite-framing),
  [Public Tag Matching](interface.md#public-tag-matching), and [Section Findings
  And Public Ordering](interface.md#section-findings-and-public-ordering).
- Parsing produces technology-neutral authored frontmatter and
  body structures for each inspectable layer. It retains the source order,
  physical layer, canonical path, source locations, visible inline content, and
  structural heading boundaries needed by the public facts in [Public Tag
  Matching](interface.md#public-tag-matching) and [Public Heading Matching](interface.md#public-heading-matching).
  The parser library and extension set remain isolated in the Technical Design.
- Region resolution converts the validated `--within` request
  into the ordered authored regions for evaluation. It applies the public
  defaults and union/redundancy rules, identifies unsupported or ambiguous
  section evaluations, and retains missing-section findings without changing
  the public rules in [Search Regions](interface.md#search-regions) and [Section
  Findings And Public Ordering](interface.md#section-findings-and-public-ordering).
- Predicate evaluation visits each selected region of each
  inspectable physical layer, evaluates only the public tag or heading predicates,
  and emits an occurrence when the predicate's public matching rule is met. It
  keeps authored spelling and source evidence; it does not perform a second
  search over serialized or hidden content.
- Evidence aggregation groups occurrences by logical source and
  predicate while retaining physical layer, region, path, location, and authored
  spelling. It applies the declared `all` or `any` requirement to the aggregated
  predicate set, including evidence distributed across overwrite layers, without
  changing the public combination rules in [Predicate Requirement](interface.md#predicate-requirement)
  and [Logical Sources And Overwrite Framing](interface.md#logical-sources-and-overwrite-framing).
- Coverage evaluation combines candidate dispositions,
  boundary findings, layer findings, section findings, and inspection results.
  It may retain independently verified safe matches, but it cannot claim a
  completed result while an unresolved fact could add a match. It selects only
  among the public semantic results in [Semantic Results](interface.md#semantic-results).
- Result ordering and deduplication run after logical-source
  aggregation and before rendering. The stage applies the single public order,
  collision, evidence, layer, occurrence, and equivalent-input rules in
  [Section Findings And Public Ordering](interface.md#section-findings-and-public-ordering), rather than filesystem or
  discovery order.
- Projection resolution selects the requested human view and
  content parts from the already aggregated result. It records missing content
  findings without rerunning matching. A known missing section after complete
  inspection contributes `completed-with-warnings`; ambiguous or incompletely inspected
  projection coverage contributes `incomplete`. The stage preserves the public
  view/content relationship in [Human Output](interface.md#human-output) and
  [Content Projection](interface.md#content-projection).
- Result construction creates one typed result with the exact
  public field meanings in [Structured Result Fields](interface.md#structured-result-fields). It includes the
  resolved request, inspected facts, ordered logical sources, evidence, content,
  findings, coverage, and semantic result needed by both presentations.
- Human and structured renderers consume the one typed result
  without rerunning enumeration, parsing, matching, projection, or verification.
  They preserve the public stream, output, and status rules in [Human Output](interface.md#human-output)
  and [Semantic Results](interface.md#semantic-results).
- A caller interruption or unexpected internal failure stops the
  current flow and forms the corresponding public status. The implementation
  does not convert a partial or failed inspection into a completed result and
  does not fabricate missing evidence.
- The operation does not persist an index, graph, receipt,
  repair artifact, or mutation
  as a side effect, consistent with [Non-Goals](interface.md#non-goals) and
  [Workspace And Source Universe](interface.md#workspace-and-source-universe).
- A `completed` result accounts for every candidate in the
  effective source universe. An unsafe, unreadable, orphaned, or ambiguous
  candidate that prevents complete accounting produces the applicable public
  finding and result instead of being silently discarded. Sources excluded
  before effective-universe formation are not candidates for that result.

## Source-Universe Filter Responsibilities

The [Shared Source-Universe Filters Behavior
Contract](../shared/source-universe-filters/behavior.md) owns the technology-
neutral resolution, physical expansion, set formation, safety, determinism, and
conformance mechanics below. The following Find responsibilities extend the
current request, workspace, enumeration, inspection, matching, coverage,
ordering, projection, result, and rendering responsibilities while preserving
their stage authorities. They describe semantic preconditions rather than
requiring one implementation stage or one source module.

- Request resolution reads `--include` and `--exclude` as
  Find-specific repeatable scalar flags and applies the shared filter Behavior
  and Interface Contracts. It keeps each occurrence as one shared
  source-reference value and does not reinterpret the flags as predicates,
  global flags, content projections, result caps, positional operands, comma
  lists, globs, arbitrary directories, or a new qualifier language. Invalid
  flag shape produces the public `invalid-input` result.
- Selector resolution runs against the exact selected
  workspace and delegates source-ID or `.agents/...` path classification,
  identity, normalization, containment, collision, and interactive
  disambiguation to the shared filter and source-reference rules. It preserves
  the supplied occurrence and its resolution outcome. It never chooses a
  candidate by likely intent or guesses an unresolved ID, path, or physical
  identity.
- For a resolved selector, Find applies the shared physical
  expansion rule: the Loader, a recognized `entrypoint`, or `SKILL.md` expands
  to the eligible Markdown sources physically contained below that source's
  folder; an ordinary source expands to one logical source. A valid base and
  overwrite pair remains one logical source with both layers. Expansion follows
  neither routed descendants nor links and infers neither authority nor
  lifecycle.
- Effective-universe formation applies the shared set
  algebra: it uses the union of all include expansions, or Find's normal
  complete eligible `.agents` Markdown universe when no include occurs, then
  subtracts the union of all exclude expansions. It deduplicates equivalent and
  overlapping selections, gives exclusion priority over inclusion, and produces
  the same effective universe regardless of include/exclude argument order.
- Effective-universe formation is a semantic precondition for
  candidate inspection and predicate evaluation. `--require` and `--within`
  neither resolve nor compose source selectors. A bare inventory may form an
  effective universe without any predicate, and predicates evaluate only over
  that formed universe.
- Enumeration and candidate-record formation admit only the
  effective logical sources. Excluded files and folders are outside the
  invocation's candidate universe and need not be parsed or inspected. The
  operation does not treat omission of excluded areas as an incomplete scan.
- Inspection, layer resolution, parsing, region resolution,
  and predicate evaluation run only for effective candidates. Existing
  base/overwrite, UTF-8, Markdown, section, safety, and authored-occurrence
  rules remain unchanged for those candidates.
- Coverage combines only dispositions within the effective
  universe. It is `complete` when that universe is fully accounted for under
  the existing rules, including when it has zero candidates or produces zero
  matches. Candidate, inspected, and matched counts exclude every source removed
  by the exclude union.
- Result construction carries the supplied include and exclude
  occurrences and their resolved selector records under the shared reporting
  contract, together with the default-or-filtered universe state and effective
  candidate, inspected, and matched counts, into the one typed result. Human
  and JSON renderers consume those facts without rerunning selector resolution,
  enumeration, inspection, or matching.
- Deduplication and ordering occur after effective-universe
  formation and logical-source aggregation. Selector spelling, repetition,
  overlap, and argument order do not alter the existing automatic-ID,
  canonical-path, layer, predicate, region, or occurrence ordering rules.
- Selector errors preserve the shared distinction between
  invalid-input values and blocked ambiguity or unsafe containment. Missing, empty,
  unknown, and unsupported selectors are invalid-input. An unresolved collision or
  unsafe or escaping physical boundary is blocked; JSON and non-interactive
  flows do not prompt, and no failure is replaced with a guess. These outcomes
  are formed before candidate inspection.
- Conformance evidence must cover omitted and filtered bare
  inventory, every selector expansion class, include and exclude set algebra,
  overlap and order independence, shared resolution and safety outcomes,
  excluded-area inspection boundaries, effective coverage and counts, and
  minimal, standard, full, debug, and JSON reporting. It must also prove that `all` remains
  the omitted flat predicate requirement and `any` remains explicit and flat.
- Source-universe filters do not change the read-only,
  stateless, deterministic operation. Find creates no persistent index, receipt,
  session, selector cache, repair artifact, or mutation authority as a result of
  forming or reporting the effective universe.

## Related Current Sources

- [Find Interface Contract](interface.md)
- [Find Technical Design](technical-design.md)
- [CLI Command Contract Set — Behavior Contract](../../command-contract-set.md#behavior-contract)
- [Context Behavior Contract](../context/behavior.md)
- [Global CLI Flags Behavior Contract](../shared/global-flags/behavior.md)
- [Shared Source-Universe Filters Interface Contract](../shared/source-universe-filters/interface.md)
- [Shared Source-Universe Filters Behavior Contract](../shared/source-universe-filters/behavior.md)
- [CLI Source References Behavior Contract](../shared/source-references/behavior.md)
- [CLI Source References Interface Contract](../shared/source-references/interface.md)
- [Framework Path Identity And Containment](../../../framework/routing/paths.md)
- [Framework Overwrite Customization](../../../framework/routing/overwrites.md)
- [Canonical Markdown Syntax](../../../framework/markdown/syntax.md)
- [Markdown Compatibility Boundary](../../../framework/markdown/compatibility.md)
