---
open-forge:
  description: Argued recommendations on the open design questions - workspace category, docs placement, loader route patterns, native backlog, description rubric
  tags: [Memory, Analysis, Reasoning, Contextual, Candidate, Design]
---

# Open Question Recommendations

Captured 2026-07-09 so the reasoning survives outside the chat. These are argued recommendations, not accepted decisions; promote the accepted ones to `crystallized/decisions/`.

## Keep The Workspace Category

Workspace routes are the map; crystallized memory is the knowledge. The map answers "where does X live and when do I go there" - source directories, build outputs, validation commands, external systems, module ownership. Orientation has no truth lifecycle: it does not emerge, crystallize, or get superseded by acceptance; it changes when the filesystem changes. Folding it into scoped crystallized memory would give navigation a memory-state semantics it does not have, violate "memory records state and must not own operational routing", and bury first-contact orientation under the memory tree. In multi-project setups the split sharpens: scoped memory holds each project's knowledge; workspace holds the map between projects. Boundary rule: when a workspace file starts explaining what is true about a destination instead of where it is and when to use it, that content moves to crystallized documents.

## Keep docs/ Outside .agents/

`docs/framework/` governs the product being shipped, not this workspace's state; descriptors are maintainer specs, and placing them inside `.agents/` puts governance one hop from baseline agent context - the exact "hidden runtime context" failure the payload boundary forbids. `docs/cli.md`, `docs/dev.md`, and `docs/extensions.md` are published npm artifacts needing stable public paths. Keeping this repo shaped like both audiences (installed user via `.agents/`, maintainer via `docs/`) keeps the dogfood honest. Integration happens through workspace routes pointing at docs, which already exists.

Reconciliation with the "docs dictate how something is, like an architecture doc" argument (2026-07-10): descriptors are normative records, but they are source of the shipped product - change a descriptor and the payload must change in the same commit, which is a build relationship, not a memory lifecycle. An architecture doc describing this repository itself would belong in crystallized documents, and `crystallized/documents/` may hold a route file pointing at `docs/framework/` the same way dogfood-reports routes to its records: memory routes to it, ownership stays with the product source. For installed users the argument fully wins - their "how our system is" docs belong in crystallized documents, which is what the route exists for.

## Description Rot Classes

From the 2026-07-10 audit of about fifty descriptions, only three failed, in three repeatable ways: missing the trigger (content stated, no "load when"), hiding the answer (naming the category but not the members an agent would search for, as in the do-not-revive list), and stale status words ("uncommitted", "pending") which rot fastest. Audit new descriptions against these three classes.

## Keep Route Patterns In The Loader

Six lines, the only place an agent learns scope-route shape before creating one, and the loader sits at 65 authored lines against a 40-90 target. Moving it buys a few lines and costs an indirection at exactly the moment an agent does something structurally risky.

## No Native Backlog Route In working/

The accepted decision already blocks a base tasks/backlog route before planning ownership is designed. A plain `backlog.md` file inside `working/` is the sanctioned local pattern and serves the need at zero framework cost. Task management wants external mapping (GitHub, Jira, Linear), which is extension territory; ship the planning extension when designed.

## Description Rubric

A description is selection surface, so the bar depends on whether selection happens. On-demand routes (no load-policy tag) need decision-grade descriptions: trigger plus outcome, "use when X; you get Y", because the one line is all that prevents a wrong skip. Always-loaded routes (#LoadNow) never gate a decision, so clean identity statements are correct there and trigger-phrasing would be noise. Example-phrase activation hints ("use when the user asks to shape a product vision") belong inside the description of on-demand routes, not in a new metadata field. Audit question: can an agent select or skip this route from the one line alone.

## Required Routes Naming

`Required Routes` over "Required Skill Entries" (too narrow - dependencies may be skills, guidance, patterns, or workflows) and over "Required Route Entries" (`entry` is a defined term owned by generated regions; reusing it destroys the containment-versus-dependency distinction). Full design in `.agents/memory/emerging/ideas/workflow-redesign.md`.
