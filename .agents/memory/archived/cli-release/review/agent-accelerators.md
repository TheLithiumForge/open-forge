---
open-forge:
  description: Historical preliminary disposition of additional outline, batching, provenance, capability, and automation accelerators
  responsibility: Preserve the historical accelerator review without making it current CLI authority
  tags: [Memory, Archived, Contextual, Historical, CLI, Release, PreliminaryDesign, Agent, Automation, Accelerator]
---

# Additional Agent Accelerators Preliminary Design

## Status

This file was archived from the [active CLI Review Queue](queue.md) on 2026-08-15 after Queue item 19 settled. The current [CLI-D079 accelerator decision](../../../working/cli-release/decision-agenda.md) accepts the `headings` Context projection, operation-scoped provenance in expanded and complete structured results, no standalone outline or provenance commands, and deferred read-only batching. The current [Context contract set](../../../crystallized/documents/cli/contracts/context/_context.md) uses `metadata` as the CLI-generated metadata name. This file preserves historical analysis and does not define current CLI authority.

- **Origin:** `.agents/memory/working/cli-release/review/agent-accelerators.md`
- **Archived because:** Queue item 19 left active review after the maintainer accepted the bounded accelerator projections and deferred read-only batching for later measurement.
- **Current sources:** [CLI-D079 accelerator decision](../../../working/cli-release/decision-agenda.md) and [Context contract set](../../../crystallized/documents/cli/contracts/context/_context.md)

The remainder of this file preserves the former review record. Its evidence
thresholds and rejected alternatives are historical context, not current
fallback designs.

## Already Covered

- Stable structured output and semantic results are accepted cross-cutting
  requirements.
- Discovery organization belongs in manually organized root help and
  documentation. No executable Discovery manifest or capability command is
  accepted. Gate 3 may derive help and completion from one command-definition
  source without creating another domain operation.
- Consistent source IDs and canonical paths belong in every relevant result and
  diagnostic.
- Context already retains source inclusion reasons, and route inspection already
  explains loading, chain, Axioms, and overwrite provenance.
- Every accepted mutation uses the same planner for dry-run and application.
- Shell completion is a Gate 3 derivation if the command library supports it
  safely; it is not a separate Framework capability.

## Accepted Boundary 1: Structural Document Outline

### Job

For sources selected by the ordinary Context request, expose the parsed heading
outline without returning complete bodies or summarizing meaning. A caller may
compose the outline with `metadata` or `frontmatter` when those facts are also
needed.

### Why It Might Help

An agent can choose an exact section without first receiving a large document.
Before this direction, `find --heading` discovered sources with known headings
and `context` retrieved a known section, but neither listed every heading for an
unknown document structure. The accepted `headings` projection closes that gap.

### Boundary

- The ordinary Context request, including startup and explicit sources.
- Parsed structural headings, levels, lines, source forms, layers, and
  provenance.
- Optional composition with CLI-generated metadata or authored frontmatter.
- No summary, relevance, inferred role, route authority, or body content unless
  another accepted content part selects it.
- Base and overwrite layers remain distinct.

Use the `headings` context content part rather than a standalone `outline`
command. Conformance evidence must still prove that startup closure behavior and
several selected sources do not make a simple one-document outline awkward.
`find --heading` already discovers sources
when the heading name is known, and `context --content=section:<name>` already
retrieves a known section; the missing job is listing unknown headings first.

### Evidence Gate

Conformance evidence must show that heading projection avoids emitting full
bodies, matches the accepted Markdown parser exactly, and remains understandable across
startup, explicit, multi-source, base, and overwrite results.

## Deferred Boundary 2: Explicit Read-Only Batching

### Job

After the CLI is complete enough to measure, consider whether several explicit read requests together reduce process and tool-call
overhead. Same-operation multi-target reads are the smallest case. A
heterogeneous batch, such as `find` plus `context`, remains plausible only when
the operations stay independently understandable.

### Boundary

- No wildcard, predicate, saved list, or generic batch language.
- Each target retains its own result and diagnostics.
- The enclosing result states whether every target completed; one failure cannot
  silently disappear inside successful rows.
- Only read operations are eligible. Mutation batching is excluded.
- One read must not gain hidden authority from another result. If a later read
  consumes an earlier result, that dependency must be explicit rather than
  looking like independent batching.

`find --content=...` already combines source discovery with context-style
content projection for matched sources. Measure that accepted composition before
adding a generic heterogeneous batch surface.

### Evidence Gate

After the CLI is complete, measure at least five explicit-target workflows.
Reconsider batching only when process or tool-call overhead is a substantial
part of elapsed work and the batch result is easier to consume than independent
calls without weakening failure clarity.

## Accepted Boundary 3: Operation-Scoped Provenance

### Job

Explain why a source was selected, loaded, excluded, attached as an overwrite,
or placed in one route chain without reconstructing the answer from several
commands.

For example, context provenance is the reason beside a returned source:

```text
.agents/memory/working/_working.md
  included because: ancestor of the explicitly selected CLI release route
  order: 3
  layer: base
```

Route-inspection provenance similarly identifies which ancestor supplied an
inherited `Axiom`, why a `#LoadNow` descendant was read, or which base source owns
an overwrite companion. Provenance is evidence for an existing result, not
version history, Git authorship, or a claim about semantic relevance.

### Boundary

Keep structured provenance fields in complete structured results and expanded
human views on the operation that already reports the fact. Compact views omit
optional explanation while retaining identity, order or hierarchy, status,
completeness, safety, and required next actions. Do not add a generic `explain`
command that attempts
to decide authority, merge separate selected scopes, or infer relevance.

### Evidence Gate

Verify that expanded views report the same provenance and completeness as their
typed results. Repeated workflow evidence may refine operation-scoped provenance
fields, but it does not create a separate provenance command or view.

## Rejected Additional Surfaces

- Separate outline, metadata, tags, sections, table-of-contents, navigation,
  authority, layer, provenance, or source-normalization commands. Accepted
  projections remain on their domain operations.
- Generic diffs without a domain snapshot or version contract.
- Generic batch apply, saved plans, query languages, fuzzy search, semantic
  summaries, or persistent indexes.
- A second capability command when manually organized help already exposes the
  live executable surface.

## Remaining Conformance Questions

1. Does heading-only context remain clear for startup closure and multi-source
   results in compact and expanded views?
2. Which provenance fields are required to understand an expanded result without
   adding diagnostic noise?
3. After release, does `find --content=...` already cover the most useful
   find-then-read batch?

## Related Evidence

- [Council Synthesis](agent-accelerator-council.md)
- [Discovery Family](discovery-family.md)
- [Ordered Context Paths](ordered-context-paths.md)
- [Route Inspect contract set](../../../crystallized/documents/cli/contracts/route/inspect/_inspect.md)
- [Context contract set](../../../crystallized/documents/cli/contracts/context/_context.md)
