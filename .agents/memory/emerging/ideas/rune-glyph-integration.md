---
open-forge:
  description: Rune and glyph stay standalone; open-forge integrates via an optional bridge extension, and the CLI keeps only contract operations they cannot cover
  tags: [Memory, Idea, Contextual, Candidate, CLI, Extension, Rune, Glyph, Integration]
---

# Rune And Glyph Integration

Analyzed 2026-07-10 against the maintainer's private planning for the companion tools. Kept deliberately high-level here; detailed product truth stays in the private repository.

Status: the conservative `rune-bridge` extension was implemented 2026-07-15 as one Workspace route plus one Guidance route. It adds no directive, command, configuration, installation, or storage contract. More prescriptive integration remains deferred until Rune exposes a stable public contract and benchmark evidence warrants it.

## What They Are (Boundary-Relevant Summary)

- Rune: a human-first project-knowledge CLI - markdown memories as source of truth, a rebuildable local index, hybrid semantic plus full-text search, relevance features (recency, pins, related), lifecycle commands, agent briefing, and an MCP server. Heavy runtime: local embedding models and a database engine.
- Glyph: an LSP-backed code-intelligence CLI (find, definition, references, callers, rename, code context), with optional crosslinks to rune memories. No overlap with open-forge at all.

## Verdict: Standalone, Integrated Through An Extension

Rune stays a standalone tool. Three reasons:

1. Value conflict at the core: open-forge's promise is plain reviewable files with zero runtime; rune requires model downloads and a database. Folding any rune capability into open-forge core breaks the promise that made the framework trustworthy.
2. Different axes: rune answers "what is relevant to this question" (relevance judgment over any markdown corpus); open-forge answers "what does the contract require loading" (deterministic routing). The boundary rule holds against the real design: relevance judgment belongs to rune, contract operations belong to open-forge.
3. The integration seam already exists from both sides: rune is file-first with configurable source paths, so it can index an open-forge workspace as-is, and the open-forge index generator already accepts `rune:` scoped frontmatter.

Glyph needs no integration beyond, at most, a workspace route telling agents it exists.

## Integration Shape: An Optional Bridge Extension

First-party, install-by-choice, exactly like any other extension - this resolves the "do not push tools on unwanting users" tension: core stays plain; power tooling arrives only through `open-forge extend`.

A `rune-bridge` extension now adds:

- a workspace route describing the authority boundary and when optional relevance assistance may help
- guidance for choosing Rune-assisted broad recall or deterministic Open Forge routing

It deliberately does not add the previously considered directive or config preset. Rune's installed documentation owns its interface; Open Forge does not invent or pin a private command/configuration contract.

Coupling stays one-directional: rune reads markdown; open-forge core never depends on rune.

## Open-Forge CLI Re-Scope Against Rune

Keep (contract operations rune structurally cannot own):

- `index` - generated regions are the contract.
- `find --route ... --follow-required` - deterministic route and Required Routes lookup over the Open Forge contract.
- `find --tag` - deterministic tag matching over routed entries; useful for discovery, including the current global #KeepInMind recheck.
- `chain <route>` - loader, ancestor entrypoint, native skill, target, overwrite, and arbitrary-heading inspection.
- `doctor` - route-contract validation; rune's own integrity checks validate its index, a different artifact.
- `install` / `extend` - unchanged.

Cut or never build (rune territory):

- any free-text or semantic query, ranking, or relatedness
- any memory lifecycle tooling (archive/prune/organize helpers)
- any "briefing"-style summarization
- exploration UX beyond a flat deterministic inventory; `routes --tree` drops to nice-to-have

## Closeout Mechanism Decision

`find --tag KeepInMind --bodies` shipped as deterministic global discovery, and `chain` shipped for inherited context inspection. They do not yet equal an active closeout set. A future active-context receipt may compose the actually selected loader chain, overwrite companions, Required Routes, closeout obligations, cost, and digest; it belongs to Open Forge because those are contract semantics, while Rune remains the relevance engine. Any loader reference must ship only with a real command and plain-file fallback.

Validation path per the fixed-seeds discipline: ship the command, add it as a benchmark variable overlay first, A/B closeout compliance against the v8 baseline, and only then promote the loader wording to core. Same experiment shape that validated the dump tool in v4.

## Idea Pruning Driven By This Analysis

- Needless now: any all-in-one generated cold-start index; a second route inventory beside `find`/`chain`; open-forge-side semantic search flags.
- Reframed: "external or distributed memory as a documented user pattern" becomes concrete through rune's configurable source paths; document it in the bridge extension, not core.
- Unchanged and still valuable: observations rework, extension skill-sharing, defaults pack, planning/tasks extension, CI ladder items.
