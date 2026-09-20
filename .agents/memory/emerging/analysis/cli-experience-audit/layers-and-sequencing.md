---
open-forge:
  description: The layer model the architecture document is missing, the directive clauses added for sharing and naming, and a considered argument for what should be done first
  tags: [Memory, Analysis, Contextual, Candidate, CLI, Architecture, Layers, Sequencing, Refactoring]
---

# Layers And Sequencing

## Directive clauses added

Three gaps identified in
[csharp-directives-and-structure.md](csharp-directives-and-structure.md) are now
closed in `directives/csharp/`.

**`design.md` — sharing, four clauses.** The existing rule guarded only against
premature abstraction; these guard the other direction and add the
document-justified exception:

- _Build a capability shared from the start when an accepted document already
  establishes it as generic._ A backing contract, architecture or pattern that
  defines one meaning is sufficient justification; a second consumer need not
  exist yet. Cite the accepted source in the shared type. Explicitly not a
  licence for speculative frameworks or extension points.
- _Default to the shared owner._ A shared type with surviving duplicates is worse
  than either alternative, because a change must then be found in several places
  and the copies drift silently.
- _Promotion is not finished until the duplicates are removed._ Migrating every
  consumer belongs to the same coherent phase.
- _A structural question has one implementation._ Two correct answers to the same
  question are a defect **even while they agree**, because nothing keeps them
  agreeing.

**`design.md` — one parsing layer.** Documents enter through one parsing layer
and leave as the typed document model; no other layer re-derives structure or
introduces a second reader.

**`style.md` — naming.** Name a type for what it produces, owns, or decides,
never for what it is not. _"When several types share a prefix and differ only by
an opaque qualifier, the qualifier is the problem."_

## The layer model

The architecture document is better than expected here. **Dependency direction**
is defined and correct — six inward tiers with the right prohibitions (_"Shell
types do not depend on concrete commands"_). **Execution Pipeline** is defined as
a typed stage chain:

```text
CliInvocationResolution → CliOperationRequest<T> → CliOperationResult<T>
  → CliPresentation<T> → CliRenderedOutput → CliOutputReceipt → CliProcessCompletion
```

That is a genuine pipeline definition, and the shipped code follows it.

Three gaps remain, and they map exactly onto the defects this audit found.

### Gap 1 — no selection stage

The chain goes `CliOperationResult<T>` → `CliPresentation<T>` → rendered output.
There is no stage between "the complete model" and "render it", so nothing owns
the decision of **what to show**. That absence is not an oversight in the code;
it is absent from the architecture, which is why 324 renderer files each decide
independently and none decides to omit.

Proposed insertion:

```text
CliOperationResult<T>          complete typed facts
  → CliSelection<T>            facts worth showing at this detail level
  → CliPresentation<T>         format-bound projection
  → CliRenderedOutput
```

`CliSelection` owns severity ordering, suppression of healthy results,
zero-count collapsing, truncation, and the per-command size budget. Renderers
become formatters over an already-reduced model. This is G4, stated as
architecture rather than as a fix.

### Gap 2 — parsing, routing and documents are one section

`### Sources, Routing, And Documents` collapses three layers with different
jobs:

| Layer         | Owns                                     | Consumes                    |
| ------------- | ---------------------------------------- | --------------------------- |
| **Documents** | Markdown and YAML → typed document model | the accepted libraries only |
| **Sources**   | identity, metadata, structural facts     | the document model          |
| **Routing**   | the route graph, exposure, loading       | source facts                |

Naming them separately is what makes _"a command never re-derives document
structure"_ enforceable — the rule needs a layer to point at. Their collapse
into one section is why a command grew its own `## Axioms` parser without
obviously violating anything written down.

### Gap 3 — no rule that a layer owns its question

The dependency rules say which layers may _call_ which. Nothing says a question
has one owner. That is now in the directives; the architecture should carry the
same statement, because it is a structural property rather than a coding style.

### On propagating this forward

The layer model worth carrying to the next project is not the specific eight
names — it is the two properties that make it checkable:

- **Each layer names the question it answers**, so a second answer is visibly
  wrong rather than merely redundant.
- **Each stage boundary is a typed value**, so any stage can be entered directly
  in a test. The existing pipeline already has this and it is the single best
  structural decision in the codebase — it is what makes in-process behaviour
  testing possible at all (see
  [test-layer-consolidation.md](test-layer-consolidation.md)).

## Sequencing: should the structural work go first?

Considered seriously, because the answer is not uniformly yes.

### Where I agree — dedup first, strongly

**Promotion-without-migration should be fixed before any behaviour work, and the
argument is stronger than "it is tidy".**

Every group after this one edits messages, statuses, projections and help. Those
are exactly the surfaces that are currently duplicated 10–12 ways. Concretely:

- G3 fixes ~12 individual messages. On a duplicated tree each fix either lands
  once and the copies drift, or must be found in up to twelve places.
- G4 rewrites renderers. Twelve local `WorkspaceSelection` mappings inside
  `*/Shared/Rendering/` become twelve rewrites instead of zero.
- G5 restructures help. **Ten of twenty-two `*HelpSections` bypass
  `CliResultHelp`**, so the restructuring cannot land centrally until they are
  routed through it. This one is a hard prerequisite, not a preference.

Deduplication is pure subtraction, behaviour-preserving, and **multiplies the
value of every group after it**. It also gets harder as more changes pile on top.

I checked the obvious objection — whether these duplicates sit in code G4 will
delete. All twelve live under `*/Shared/Rendering/`, which is inside G4's blast
radius, but the _mapping itself_ survives: enum → wire string is still needed by
the JSON projection regardless of how far the human view is trimmed. So the work
is not thrown away.

The one-file folders (**168 of 624**) belong in the same pass. Same risk profile,
same character, and the tree becomes navigable for everything that follows.

### Where I would defer — naming, but only slightly

Naming second is defensible and I would still change the order, for one reason:
**names should express layers, and the layers are being defined right now.**

If `*HumanRenderer` is renamed to `*TextRenderer` today and G4 then introduces a
selection stage, the honest name is different again — the thing that survives is
a _formatter_, and the _view_ is the new selection layer. That is two renames.

An LSP makes each rename nearly free, so the cost of waiting is zero and the cost
of renaming twice is small but real. The decisive point is not effort, it is that
**a rename encodes a decision**, and the decision is one document away.

So: architecture layers → then rename once, to the layer names. That is a short
document, not a project, and it is what was already asked for.

### Where the ship-blockers sit

Worth separating explicitly, because "structure first" and "release in days" are
different tracks and both can be true.

The release-gating defects are small and mostly unrelated to structure:
`SKILL.md` strict keys (one `switch`), the entries baseline, `workspacePath`,
upward discovery, the extension silent no-op, the frontmatter BOM. None requires
the refactor, and none is made easier by it.

**Those go first regardless** — they are what a user hits, they are individually
tiny, and they do not touch the duplicated surfaces.

### Proposed order

1. **Ship-blockers.** Small, user-visible, release-gating. Independent of
   everything below.
2. **Architecture layers.** One document. Unblocks both the naming pass and G4's
   contract, and is the artefact worth carrying to the next project.
3. **Deduplication and folder collapse.** Targets 1–4 from
   [csharp-directives-and-structure.md](csharp-directives-and-structure.md).
   Pure subtraction; force multiplier for everything after.
4. **Naming pass**, using the layer names from step 2. LSP-assisted, one sweep.
5. **G1 → G4**, in the existing group order, on a deduplicated tree.
6. **Oversized file splits.** Agreed, and last — the seams are clearer once the
   selection layer exists and the renderers have shrunk.

Steps 1–4 are days of work, not weeks, and steps 3–4 make step 5 materially
cheaper. The only real cost of this order is that step 2 must actually be short.

### The one risk in this order

Deduplicating before G1 touches code G1 may delete — the lifecycle baseline
machinery, the permissions subsystem. **Do not deduplicate inside those two
subsystems.** Everything in the ranked targets is in rendering, help, and folder
structure, none of which G1 removes, so this is a boundary to respect rather
than a conflict.
