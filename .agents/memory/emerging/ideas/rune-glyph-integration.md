---
open-forge:
  description: Rune and glyph stay standalone; open-forge integrates via an optional bridge extension, and the CLI keeps only contract operations they cannot cover
  tags: [Memory, Idea, Contextual, Candidate, CLI, Extension, Rune, Glyph, Integration]
---

# Rune And Glyph Integration

Analyzed 2026-07-10 against the maintainer's private planning for the companion tools. Kept deliberately high-level here; detailed product truth stays in the private repository.

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

A `rune-bridge` extension would add:

- a workspace route describing the rune commands agents may use for semantic recall over workspace memory and when to prefer them over walking routes manually
- optionally a directive that memory searches beyond routed navigation go through rune when it is installed
- optionally a config preset pointing rune's source paths at `.agents/`

Coupling stays one-directional: rune reads markdown; open-forge core never depends on rune.

## Open-Forge CLI Re-Scope Against Rune

Keep (contract operations rune structurally cannot own):

- `index` - generated regions are the contract.
- `context` with tiers (startup, closeout, path, `--follow-required`) - load-policy tags, ancestor chains, and Required Routes are contract semantics invisible to an IR engine.
- `routes --tag` / find-by-tags - deterministic tag matching over generated entries; needed as the closeout primitive.
- `doctor` - route-contract validation; rune's own integrity checks validate its index, a different artifact.
- `install` / `extend` - unchanged.

Cut or never build (rune territory):

- any free-text or semantic query, ranking, or relatedness
- any memory lifecycle tooling (archive/prune/organize helpers)
- any "briefing"-style summarization
- exploration UX beyond a flat deterministic inventory; `routes --tree` drops to nice-to-have

## Closeout Mechanism Decision

Implement find-by-tag as the primitive and `context closeout` as the named tier (print the #KeepInMind route bodies plus their follow-ups). Loader wording gains a tool-assisted, never tool-dependent line - "recheck loaded #KeepInMind entries; `open-forge context closeout` prints them when the CLI is available" - shipped only together with the command, because the framework must not reference tools that do not exist and must keep working from a tarball install with no CLI.

Validation path per the fixed-seeds discipline: ship the command, add it as a benchmark variable overlay first, A/B closeout compliance against the v8 baseline, and only then promote the loader wording to core. Same experiment shape that validated the dump tool in v4.

## Idea Pruning Driven By This Analysis

- Needless now: any all-in-one generated cold-start index (context startup plus rune cover both halves); route inventory beyond flat `routes`; open-forge-side search flags.
- Reframed: "external or distributed memory as a documented user pattern" becomes concrete through rune's configurable source paths; document it in the bridge extension, not core.
- Unchanged and still valuable: observations rework, extension skill-sharing, defaults pack, planning/tasks extension, CI ladder items.
