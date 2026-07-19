---
open-forge:
  description: Historical workspace-routing exploration covering map-versus-knowledge boundaries, split criteria, and example shapes
  tags: [Memory, Archived, Idea, Contextual, Historical, Workspace, Routing]
---

# Workspace Usage

Status: archived 2026-07-18.  
Original route: `.agents/memory/emerging/ideas/workspace-usage.md`.  
Archived because: the stable map-versus-knowledge and split rules are now expressed by the Workspace route and source-of-truth map; examples remain historical context.  
Current owner or replacement: `.agents/workspace/_workspace.md`, `.agents/workspace/sources-of-truth.md`, routing decisions, and current documentation.

Captured 2026-07-10 for later exploration; answers "what goes in workspace files, how much, and when to split". Maintainer leaning 2026-07-10: keep workspace routes, roughly one map file per module or area - what and where, without full explanation.

## The Test

A workspace file answers "where is X and when do I go there". The moment it explains what is true about X, that sentence belongs in crystallized documents. The workspace axiom "destination files retain detailed truth" is the guard rail.

## Degree Of Detail

One line per destination: path or URL, what it contains, when it matters. Never summaries of the destination's content.

## Split Criterion

Split a file when its entries stop being selected together - by question, not by count. "Where is the code", "where does it run", and "where is the outside world" are three different moments in a task, so they earn three files.

## Concrete Shapes

- Single repo (this one): `sources-of-truth.md` - one file, one question: which file is authoritative for what. A `commands.md` (build, test, bench invocations and when each matters) splits off only if tooling docs grow heavy.
- Monorepo: `repositories.md` (each package or app dir - what it owns, when you touch it), `services.md` (deployed services - which package builds them, where configs live), `external-systems.md` (tracker project, design system, API console - URL plus when to send the user there).
- Vault or second brain: `vault-map.md` (top-level areas and what belongs in each), `inbox-and-capture.md` (where unsorted material lands and when to triage).

## Relation To Scoped Memory

`mobile-app`'s decisions live in scoped crystallized memory; the fact that mobile-app lives in `apps/mobile` and owns the push service is map, and map is workspace. In multi-project setups scoped memory holds each project's knowledge while workspace holds the map between projects.
